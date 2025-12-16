using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Models;

public sealed class TableRow
{
    public DocumentationDataModel Data { get; set; } = null!;
    // If not null, this row represents a complex property pointing to a nested type table
    public INamedTypeSymbol? ComplexTarget { get; set; }
}

public sealed class DocumentationTablesModel
{
    // Rows for the root class table
    public List<TableRow> RootRows { get; set; } = new();

    // Unique tables per complex type symbol
    public Dictionary<INamedTypeSymbol, List<TableRow>> TypeTables { get; set; } = new(SymbolEqualityComparer.Default);
}
