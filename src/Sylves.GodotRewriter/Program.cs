using System;
using System.Collections.Generic;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

MSBuildLocator.RegisterDefaults();

using var workspace = MSBuildWorkspace.Create();
var project = await workspace.OpenProjectAsync("..\\Sylves\\Sylves.csproj");
project = project.WithParseOptions(((CSharpParseOptions)project.ParseOptions!).WithPreprocessorSymbols("GODOT"));
var compilation = await project.GetCompilationAsync();

foreach (var st in compilation!.SyntaxTrees)
{
    Console.WriteLine($"Processing {st.FilePath}");
    var dest = st.FilePath.Replace("src\\Sylves\\", "src\\Sylves.Godot\\");
    if (dest == st.FilePath) continue;
    if (UnityToGodotRewriter.ExcludeFiles.Any(x => dest.EndsWith("\\" + x))) continue;

    var model = compilation.GetSemanticModel(st);

    var s = st.GetCompilationUnitRoot();

    var rw = new UnityToGodotRewriter(model);
    var s2 = rw.Visit(s);
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
