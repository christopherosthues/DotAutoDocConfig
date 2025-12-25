using System;
using System.Collections.Generic;
using System.IO;
using DotAutoDocConfig.SourceGenerator.DocumentationParser;
using DotAutoDocConfig.SourceGenerator.DocumentationSyntaxTree;
using DotAutoDocConfig.SourceGenerator.Extensions;
using DotAutoDocConfig.SourceGenerator.Models;
using DotAutoDocConfig.SourceGenerator.Renderers;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.TableGenerators;

internal class SeparateTableGenerator : TableGeneratorBase
{
    public override void GenerateTable(DocumentationOptionsDataModel docOptions, SourceProductionContext context,
        INamedTypeSymbol classSymbol, string projectDirectory, string repoRoot, ISet<string> filePaths)
    {
        LocalFormat fmt = docOptions.Format;
        IDocumentationRenderer documentationRenderer = DocumentationRendererFactory.CreateRenderer(fmt);
        IDocumentationParser documentationParser = new SeparateTableParser();
        string directory = ComposeRootOutputPath(
            context,
            docOptions.OutputDirectory,
            projectDirectory,
            repoRoot);
        IList<IDocumentationNode> trees = documentationParser.Parse(classSymbol, docOptions, directory, filePaths);

        HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (IDocumentationNode tree in trees)
        {
            tree.Accept(documentationRenderer);

            // Write root file
            WriteResolvedFile(context, tree.OutputFilePath, documentationRenderer.GetResult());

            documentationRenderer.Clear();
        }
    }
}
