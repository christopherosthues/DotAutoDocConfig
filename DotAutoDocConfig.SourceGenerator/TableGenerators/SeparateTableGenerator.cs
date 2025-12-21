using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotAutoDocConfig.SourceGenerator.Collectors;
using DotAutoDocConfig.SourceGenerator.DocumentationGenerators;
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
        // StringBuilder sb = new();
        LocalFormat fmt = (LocalFormat)docOptions.Format;
        // IDocumentationGenerator documentationGenerator = DocumentationGeneratorFactory.CreateGenerator(fmt);
        IDocumentationRenderer documentationRenderer = DocumentationRendererFactory.CreateRenderer(fmt);
        // IConfigurationCollector configurationCollector = new SeparateTableCollector();
        // DocumentationTablesModel tables = configurationCollector.Collect(classSymbol);
        IDocumentationParser documentationParser = new SeparateTableParser();
        IList<IDocumentationNode> trees = documentationParser.Parse(classSymbol, docOptions.IncludeNamespaces);

        // Prepare file names for each type table
        Dictionary<INamedTypeSymbol, string> typeToFileName = new(SymbolEqualityComparer.Default);
        HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<INamedTypeSymbol, List<TableRow>> kvp in tables.TypeTables)
        {
            string ext = fmt.ToFileExtension();
            string baseName = docOptions.IncludeNamespaces
                ? CreateFileBaseNameWithNamespace(kvp.Key)
                : kvp.Key.Name;
            string fileName = EnsureUniqueFileName(baseName, ext, usedNames);
            typeToFileName[kvp.Key] = fileName;
        }

        // Render root file with links to separate type files
        IDocumentationNode node = null!;
        node.Accept(documentationRenderer);
        // documentationGenerator.GenerateWithFileLinks(sb, classSymbol, tables, typeToFileName,
        //     docOptions.IncludeNamespaces);

        // Resolve root output path (directory or file supported)
        string rootFullPath = ComposeRootOutputPath(
            context,
            docOptions.OutputDirectory,
            projectDirectory,
            repoRoot,
            fmt,
            classSymbol,
            docOptions.IncludeNamespaces);

        // Write root file
        WriteResolvedFile(context, rootFullPath, documentationRenderer.GetResult());

        // Write each type table to its own file next to the root output (same directory)
        foreach (KeyValuePair<INamedTypeSymbol, List<TableRow>> kvp in tables.TypeTables)
        {
            string typeFileName = typeToFileName[kvp.Key];
            string rootPath = rootFullPath; // already resolved file path
            string? rootDir = Path.GetDirectoryName(rootPath);
            string typePath = Path.Combine(rootDir ?? string.Empty, typeFileName);
            documentationRenderer.Clear();

            node = null!;
            node.Accept(documentationRenderer);
            // documentationGenerator.GenerateTypeTable(subSb, kvp.Key, kvp.Value, typeToFileName, docOptions.IncludeNamespaces);

            // Write sub file
            WriteResolvedFile(context, typePath, documentationRenderer.GetResult());
        }
    }
}
