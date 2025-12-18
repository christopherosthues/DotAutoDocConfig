using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotAutoDocConfig.SourceGenerator.Collectors;
using DotAutoDocConfig.SourceGenerator.DocumentationGenerators;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.TableGenerators;

internal class InlineTableGenerator : TableGeneratorBase
{
    public override void GenerateTable(DocumentationOptionsDataModel docOptions, SourceProductionContext context,
        INamedTypeSymbol classSymbol, string projectDirectory, string repoRoot)
    {
        // Convert RootRows into DocumentationDataModel list for existing generator method
        StringBuilder sb = new();
        LocalFormat fmt = (LocalFormat)docOptions.Format;
        IDocumentationGenerator documentationGenerator = DocumentationGeneratorFactory.CreateGenerator(fmt);
        IConfigurationCollector configurationCollector = new InlineTableCollector();
        DocumentationTablesModel tables = configurationCollector.Collect(classSymbol);
        List<DocumentationDataModel> entries = [..tables.RootRows.Select(row => row.Data)];

        documentationGenerator.Generate(sb, classSymbol, entries, docOptions.IncludeNamespaces);

        // Resolve root output path and write
        string rootFullPath = ComposeRootOutputPath(
            context,
            docOptions.OutputDirectory,
            projectDirectory,
            repoRoot,
            fmt,
            classSymbol,
            docOptions.IncludeNamespaces);

        WriteResolvedFile(context, rootFullPath, sb.ToString());
    }
}
