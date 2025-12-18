using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Models;

internal sealed class TableRow
{
    public DocumentationDataModel Data { get; set; } = null!;
    // If not null, this row represents a complex property pointing to a nested type table
    public INamedTypeSymbol? ComplexTarget { get; set; }
}
