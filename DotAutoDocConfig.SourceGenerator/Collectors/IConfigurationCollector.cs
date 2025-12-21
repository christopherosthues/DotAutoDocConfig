namespace DotAutoDocConfig.SourceGenerator.Collectors;

using Microsoft.CodeAnalysis;
using Models;

internal interface IConfigurationCollector
{
    // TODO: Delete me
    // Collects documentation rows for the given root type.
    // Returns a DocumentationTablesModel:
    // - For Inline mode: RootRows contains only leaf rows; TypeTables is empty.
    // - For SeparateTables mode: RootRows contains top-level rows; TypeTables has one table per complex type.
    DocumentationTablesModel Collect(INamedTypeSymbol root);
}
