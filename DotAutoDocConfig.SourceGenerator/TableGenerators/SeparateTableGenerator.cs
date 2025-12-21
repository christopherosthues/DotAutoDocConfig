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
        INamedTypeSymbol classSymbol, string projectDirectory, string repoRoot)
    {
        LocalFormat fmt = (LocalFormat)docOptions.Format;
        IDocumentationRenderer documentationRenderer = DocumentationRendererFactory.CreateRenderer(fmt);
        IDocumentationParser documentationParser = new SeparateTableParser();
        bool includeNamespaces = docOptions.IncludeNamespaces;
        IList<(INamedTypeSymbol Symbol, IDocumentationNode Tree)> trees = documentationParser.Parse(classSymbol, includeNamespaces);

        string directory = ComposeRootOutputPath(
            context,
            docOptions.OutputDirectory,
            projectDirectory,
            repoRoot);
        string ext = fmt.ToFileExtension();
        HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
        foreach ((INamedTypeSymbol symbol, IDocumentationNode tree) in trees)
        {
            tree.Accept(documentationRenderer);

            string baseName = includeNamespaces ? CreateFileBaseNameWithNamespace(symbol) : symbol.Name;
            string fileName = EnsureUniqueFileName(baseName, ext, usedNames);
            string candidate = Path.Combine(directory, fileName);

            // Write root file
            WriteResolvedFile(context, candidate, documentationRenderer.GetResult());

            documentationRenderer.Clear();
        }
    }
}
