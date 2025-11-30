using System;
using System.Collections.Generic;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

MSBuildLocator.RegisterDefaults();

using var workspace = MSBuildWorkspace.Create();
var project = await workspace.OpenProjectAsync("..\\Sylves\\Sylves.csproj");
project = project.WithParseOptions(((CSharpParseOptions)project.ParseOptions!).WithPreprocessorSymbols("BIGINT"));
var compilation = await project.GetCompilationAsync();

foreach (var st in compilation!.SyntaxTrees)
{
    var dest = st.FilePath.Replace("src\\Sylves\\", "src\\Sylves.BigInt\\");
    if (dest == st.FilePath) continue;
    //if (!dest.Contains("MeshDataOp"))
    //    continue;

    var skip = BigIntRewriter.SkipFiles.Any(x => dest.EndsWith("\\" + x)) || BigIntRewriter.SkipDirs.Any(x => st.FilePath.Contains(x));
    SyntaxNode? s2;
    if (skip)
    {
        Console.WriteLine($"Skipping {st.FilePath}");
        var model = compilation.GetSemanticModel(st);

        s2 = st.GetCompilationUnitRoot();
    }
    else
    {
        Console.WriteLine($"Processing {st.FilePath}");

        var model = compilation.GetSemanticModel(st);

        var s = st.GetCompilationUnitRoot();

        var rw = new BigIntRewriter(model);
        s2 = rw.Visit(s);
    }
    Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
    {
        var fileInfo = new FileInfo(dest);
        if (fileInfo.Exists)
            fileInfo.IsReadOnly = false;
    }
    using (var file = File.Create(dest))
    {
        await file.WriteAsync(st.Encoding!.GetBytes(s2.ToFullString()));
    }
    {
        var fileInfo = new FileInfo(dest);
        fileInfo.IsReadOnly = true;
    }
}
