using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Models;

internal sealed class DocumentationTablesModel
{
    // TODO: Delete me
    // Rows for the root class table
    public List<TableRow> RootRows { get; set; } = [];

    // Unique tables per complex type symbol
    public Dictionary<INamedTypeSymbol, List<TableRow>> TypeTables { get; set; } = new(SymbolEqualityComparer.Default);
}
