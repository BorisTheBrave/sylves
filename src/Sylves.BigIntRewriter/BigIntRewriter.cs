using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Printing;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

public class BigIntRewriter : CSharpSyntaxRewriter
{
    readonly SemanticModel model;

    // Many utility files that don't work with grids don't need any changes.
    public static string[] SkipFiles =
    {
        "Heap.cs",
        "HashUtils.cs",
        "BitUtils.cs",
        "SphericalVoronator.cs",
        "Matrix4x4.cs",
    };
    public static string[] SkipDirs =
    {
        "Sylves\\Mesh\\",
        "Sylves\\Voronoi\\",
        "Sylves\\Deform\\",
    };

    private static Dictionary<string, string> strTypeReplacements = new()
    {
        {"int", "System.Numerics.BigInteger" },
    };

    private static Dictionary<string, string> strMemberReplacements = new()
    {
        {"System.Math.Max", "System.Numerics.BigInteger.Max" },
        {"System.Math.Min", "System.Numerics.BigInteger.Min" },
        {"System.Math.Abs", "System.Numerics.BigInteger.Abs" },
        {"System.Math.DivRem", "System.Numerics.BigInteger.DivRem" },
    };

    private Dictionary<(string? Namespace, string Type), (string? Namespace, string Type)> typeReplacements;
    private Dictionary<(string? Namespace, string Type, string Name), string> memberReplacements;

    INamedTypeSymbol int32Type;
    INamedTypeSymbol bigIntType;
    INamedTypeSymbol floatType;

    QualifiedNameSyntax bigIntQualifiedName;
    PredefinedTypeSyntax floatSyntaxName;

    public BigIntRewriter(SemanticModel model)
    {
        this.model = model;
        (string? Namespace, string Type) ParseType(string s)
        {
            var i = s.LastIndexOf(".");
            if (i == -1)
                return (null, s);
            return (s.Substring(0, i), s.Substring(i + 1));
        }
        (string? Namespace, string Type, string Name) ParseMember(string s)
        {
            var i = s.LastIndexOf(".");
            var i2 = s.LastIndexOf(".", i - 1);
            if (i2 == -1)
                return (null, s.Substring(0, i), s.Substring(i+1));
            return (s.Substring(0, i2), s.Substring(i2 + 1, i - i2 - 1), s.Substring(i + 1));
        }
        typeReplacements = strTypeReplacements.ToDictionary(kv => ParseType(kv.Key), kv => ParseType(kv.Value));
        memberReplacements = strMemberReplacements.ToDictionary(kv => ParseMember(kv.Key), kv => kv.Value);
        int32Type = model.Compilation.GetSpecialType(SpecialType.System_Int32);
        bigIntType = model.Compilation.GetTypeByMetadataName("System.Numerics.BigInteger")!;
        floatType = model.Compilation.GetSpecialType(SpecialType.System_Single);

        bigIntQualifiedName = QualifiedName(
            IdentifierName("System.Numerics"),
            IdentifierName("BigInteger")
        );
        floatSyntaxName = PredefinedType(Token(SyntaxKind.FloatKeyword));
    }

    public SyntaxNode? VisitName(NameSyntax node)
    {

        var symbolKind = model.GetSymbolInfo(node).Symbol?.Kind;
        var type = model.GetTypeInfo(node).Type;

        if (type == null && node.Parent is ObjectCreationExpressionSyntax oces)
        {
            type = model.GetTypeInfo(oces).Type;
        }

        if (type == null)
            return node;

        if (symbolKind is SymbolKind.NamedType)
        {
            //if (typeReplacements.TryGetValue((type.ContainingNamespace?.Name, type.Name), out var t))
            //{
            //    return QualifiedName(IdentifierName(t.Namespace!), IdentifierName(t.Type))
            //        .WithTriviaFrom(node);
            //}
        }
        else if (symbolKind is SymbolKind.Local or SymbolKind.Parameter or SymbolKind.Field or SymbolKind.Property) {
        }
        else
        {
            //Console.WriteLine($"Visiting type {type} {symbolKind}");
        }
        return node;
    }

    public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
    {
        return VisitName(node);
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        return VisitName(node);
    }

    public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var type = model.GetTypeInfo(node.Expression).Type;

        if (type != null &&
            memberReplacements.TryGetValue((type.ContainingNamespace?.Name, type.Name, node.Name.Identifier.ValueText), out var replName))
        {
            // Skip replacement if this member access is used in a call expression and none of the arguments are int
            // This is for overloads like Math.Max
            if (node.Parent is InvocationExpressionSyntax invocation && invocation.Expression == node)
            {
                bool hasIntArgument = false;
                foreach (var argument in invocation.ArgumentList.Arguments)
                {
                    var argType = model.GetTypeInfo(argument.Expression).ConvertedType;
                    if (argType != null && SymbolEqualityComparer.Default.Equals(argType, int32Type))
                    {
                        hasIntArgument = true;
                        break;
                    }
                }
                if (!hasIntArgument)
                {
                    return base.VisitMemberAccessExpression(node);
                }
            }

            if(replName.Contains("."))
            {
                // Ok, this is not strickly legal, but seems to work.
                // Do a replacement of the entire node. This is for static methods only.
                return IdentifierName(replName)
                    .WithTriviaFrom(node);
            }

            return node
                .WithExpression((ExpressionSyntax)Visit(node.Expression))
                .WithName(IdentifierName(replName).WithTriviaFrom(node.Name));
        }
        return base.VisitMemberAccessExpression(node);
    }

    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        var modifiers = node.Modifiers;
        var constIndex = modifiers.IndexOf(SyntaxKind.ConstKeyword);
        
        // Only process const fields that are declared as int
        if (constIndex >= 0 && node.Declaration.Type is PredefinedTypeSyntax pts && pts.Keyword.IsKind(SyntaxKind.IntKeyword))
        {
            // Remove the const modifier
            var newModifiers = modifiers.RemoveAt(constIndex);
            
            // Add /*const*/ comment before the field declaration
            var constCommentTrivia = SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, "/*const*/");
            var leadingTrivia = node.GetLeadingTrivia().Add(constCommentTrivia).Add(SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "));

            var visitedNode = base.VisitFieldDeclaration(node);
            if (visitedNode is FieldDeclarationSyntax visitedField)
            {
                return visitedField
                    .WithModifiers(newModifiers)
                    .WithLeadingTrivia(leadingTrivia);
            }
        }
        
        return base.VisitFieldDeclaration(node);
    }

    public override SyntaxNode? Visit(SyntaxNode? node)
    {
        // Any type references to int need to be replaced with BigInteger
        if (node is PredefinedTypeSyntax pts && pts.Keyword.IsKind(SyntaxKind.IntKeyword))
        {
            // Skip if this is a cast expression (int)subexpr where subexpr is an enum
            // This is for a common idiom (CellDir)(1 + (int)x)
            if (node.Parent is CastExpressionSyntax castExpr)
            {
                var expressionType = model.GetTypeInfo(castExpr.Expression).Type;
                if (expressionType != null && expressionType.TypeKind == TypeKind.Enum)
                {
                    return node;
                }
            }
            return bigIntQualifiedName.WithTriviaFrom(node);
        }
        // Implicit casts from int to float need to be made explicit
        if (node is ExpressionSyntax expr && IsCastTarget(expr) && !(expr is LiteralExpressionSyntax))
        {
            var typeInfo = model.GetTypeInfo(node);
            if (SymbolEqualityComparer.Default.Equals(typeInfo.Type, int32Type) &&
                SymbolEqualityComparer.Default.Equals(typeInfo.ConvertedType, floatType))
            {
                var conversion = model.Compilation.ClassifyConversion(typeInfo.Type, typeInfo.ConvertedType);
                if (conversion.IsImplicit)
                {
                    return CastExpression(floatSyntaxName, ParenthesizedExpression((ExpressionSyntax)base.Visit(node)));
                }
            }
        }
        return base.Visit(node);
    }

    private static bool IsCastTarget(ExpressionSyntax expr)
    {
        var parent = expr.Parent;
        if (parent is null)
            return false;

        // never cast the member name in v.x or v?.x
        if (parent is MemberAccessExpressionSyntax mae && mae.Name == expr)
            return false;

        // never cast the when-not-null part of v?.x
        if (parent is ConditionalAccessExpressionSyntax cae && cae.WhenNotNull == expr)
            return false;

        // never cast the “callee” in Foo(x)
        if (parent is InvocationExpressionSyntax inv && inv.Expression == expr)
            return false;

        return parent switch
        {
            AssignmentExpressionSyntax a when a.Right == expr => true,
            EqualsValueClauseSyntax e when e.Value == expr => true, // initializers: int x = expr;
            ReturnStatementSyntax r when r.Expression == expr => true,
            ArgumentSyntax arg when arg.Expression == expr => true,
            ArrowExpressionClauseSyntax arrow when arrow.Expression == expr => true,
            BinaryExpressionSyntax => true,
            CastExpressionSyntax _ => false,
            _ => false
        };
    }
}