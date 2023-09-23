using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

public class UnityToGodotRewriter : CSharpSyntaxRewriter
{
    readonly SemanticModel model;

    public static string[] ExcludeFiles =
    {
        "Quaternion.cs",
        "Vector2.cs",
        "Vector2Int.cs",
        "Vector3.cs",
        "Vector3Int.cs",
        "Vector4.cs",
    };

    private static Dictionary<string, string> strTypeReplacements = new()
    {
        // For now, keep this, but we need to fully qualify it.
        {"Sylves.Mathf", "Sylves.Mathf" },

        {"Sylves.Quaternion", "Godot.Quaternion" },
        {"Sylves.Vector2", "Godot.Vector2" },
        {"Sylves.Vector2Int", "Godot.Vector2I" },
        {"Sylves.Vector3", "Godot.Vector3" },
        {"Sylves.Vector3Int", "Godot.Vector3I" },
        {"Sylves.Vector4", "Godot.Vector4" },
    };

    private static Dictionary<string, string> strMemberReplacements = new()
    {
        {"Sylves.Quaternion.x", "X" },
        {"Sylves.Quaternion.y", "Y" },
        {"Sylves.Quaternion.z", "Z" },
        {"Sylves.Quaternion.w", "W" },
        {"Sylves.Quaternion.identity", "Identity" },
        {"Sylves.Quaternion.AngleAxis", "GodotVectorUtils.AngleAxis" },
        {"Sylves.Quaternion.Euler", "GodotVectorUtils.Euler" },

        {"Sylves.Vector2.x", "X" },
        {"Sylves.Vector2.y", "Y" },
        {"Sylves.Vector2.one", "One" },
        {"Sylves.Vector2.zero", "Zero" },
        {"Sylves.Vector2.left", "Left" },
        {"Sylves.Vector2.right", "Right" },
        {"Sylves.Vector2.up", "Up" },
        {"Sylves.Vector2.down", "Down" },
        {"Sylves.Vector2.magnitude", "Length()" },
        {"Sylves.Vector2.sqrMagnitude", "LengthSquared()" },
        {"Sylves.Vector2.Distance", "GodotVectorUtils.Distance" },
        {"Sylves.Vector2.Min", "GodotVectorUtils.Min" },
        {"Sylves.Vector2.Max", "GodotVectorUtils.Max" },
        {"Sylves.Vector2.normalized", "Normalized()" },
        {"Sylves.Vector2.Scale", "GodotVectorUtils.Scale" },
        {"Sylves.Vector2.Dot", "GodotVectorUtils.Dot" },

        {"Sylves.Vector2Int.x", "X" },
        {"Sylves.Vector2Int.y", "Y" },
        {"Sylves.Vector2Int.one", "One" },
        {"Sylves.Vector2Int.zero", "Zero" },
        {"Sylves.Vector2Int.left", "Left" },
        {"Sylves.Vector2Int.right", "Right" },
        {"Sylves.Vector2Int.up", "Up" },
        {"Sylves.Vector2Int.down", "Down" },
        {"Sylves.Vector2Int.Min", "GodotVectorUtils.Min" },
        {"Sylves.Vector2Int.Max", "GodotVectorUtils.Max" },
        {"Sylves.Vector2Int.FloorToInt", "GodotVectorUtils.FloorToInt" },

        {"Sylves.Vector3.x", "X" },
        {"Sylves.Vector3.y", "Y" },
        {"Sylves.Vector3.z", "Z" },
        {"Sylves.Vector3.one", "One" },
        {"Sylves.Vector3.zero", "Zero" },
        {"Sylves.Vector3.left", "Left" },
        {"Sylves.Vector3.right", "Right" },
        {"Sylves.Vector3.up", "Up" },
        {"Sylves.Vector3.down", "Down" },
        {"Sylves.Vector3.forward", "Back" }, // Note: Inverted!
        {"Sylves.Vector3.back", "Forward" }, // Note: Inverted!
        {"Sylves.Vector3.magnitude", "Length()" },
        {"Sylves.Vector3.sqrMagnitude", "LengthSquared()" },
        {"Sylves.Vector3.Distance", "GodotVectorUtils.Distance" },
        {"Sylves.Vector3.Cross", "GodotVectorUtils.Cross" },
        {"Sylves.Vector3.Min", "GodotVectorUtils.Min" },
        {"Sylves.Vector3.Max", "GodotVectorUtils.Max" },
        {"Sylves.Vector3.normalized", "Normalized()" },
        {"Sylves.Vector3.Scale", "GodotVectorUtils.Scale" },
        {"Sylves.Vector3.Dot", "GodotVectorUtils.Dot" },
        {"Sylves.Vector3.ProjectOnPlane", "GodotVectorUtils.ProjectOntoPlane" },

        {"Sylves.Vector3Int.x", "X" },
        {"Sylves.Vector3Int.y", "Y" },
        {"Sylves.Vector3Int.z", "Z" },
        {"Sylves.Vector3Int.one", "One" },
        {"Sylves.Vector3Int.zero", "Zero" },
        {"Sylves.Vector3Int.left", "Left" },
        {"Sylves.Vector3Int.right", "Right" },
        {"Sylves.Vector3Int.up", "Up" },
        {"Sylves.Vector3Int.down", "Down" },
        {"Sylves.Vector3Int.forward", "Back" }, // Note: Inverted!
        {"Sylves.Vector3Int.back", "Forward" }, // Note: Inverted!
        {"Sylves.Vector3Int.Min", "GodotVectorUtils.Min" },
        {"Sylves.Vector3Int.Max", "GodotVectorUtils.Max" },
        {"Sylves.Vector3Int.FloorToInt", "GodotVectorUtils.FloorToInt" },



        {"Sylves.Vector4.x", "X" },
        {"Sylves.Vector4.y", "Y" },
        {"Sylves.Vector4.z", "Z" },
        {"Sylves.Vector4.w", "W" },
        {"Sylves.Vector4.one", "One" },
        {"Sylves.Vector4.zero", "Zero" },
        {"Sylves.Vector4.left", "Left" },
        {"Sylves.Vector4.right", "Right" },
        {"Sylves.Vector4.up", "Up" },
        {"Sylves.Vector4.down", "Down" },
        {"Sylves.Vector4.forward", "Back" }, // Note: Inverted!
        {"Sylves.Vector4.back", "Forward" }, // Note: Inverted!
        {"Sylves.Vector4.magnitude", "Length()" },
        {"Sylves.Vector4.sqrMagnitude", "LengthSquared()" },
        {"Sylves.Vector4.Distance", "GodotVectorUtils.Distance" },
        {"Sylves.Vector4.Min", "GodotVectorUtils.Min" },
        {"Sylves.Vector4.Max", "GodotVectorUtils.Max" },
        {"Sylves.Vector4.normalized", "Normalized()" },
        {"Sylves.Vector4.Scale", "GodotVectorUtils.Scale" },
        {"Sylves.Vector4.Dot", "GodotVectorUtils.Dot" },
    };

    private Dictionary<(string? Namespace, string Type), (string? Namespace, string Type)> typeReplacements;
    private Dictionary<(string? Namespace, string Type, string Name), string> memberReplacements;

    public UnityToGodotRewriter(SemanticModel model)
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

        if (symbolKind == SymbolKind.NamedType)
        {
            if (typeReplacements.TryGetValue((type.ContainingNamespace?.Name, type.Name), out var t))
            {
                return QualifiedName(IdentifierName(t.Namespace), IdentifierName(t.Type))
                    .WithTriviaFrom(node);
            }
        }
        else if(symbolKind is SymbolKind.Local or SymbolKind.Parameter or SymbolKind.Field or SymbolKind.Property)
        {

        }
        else
        {
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

        if(type != null &&
            memberReplacements.TryGetValue((type.ContainingNamespace?.Name, type.Name, node.Name.Identifier.ValueText), out var replName))
        {
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
}