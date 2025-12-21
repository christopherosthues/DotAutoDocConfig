using DotAutoDocConfig.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Models;

internal static class DocumentationDataModelFactory
{
    // TODO: Delete me
    public static DocumentationDataModel CreateDocumentationDataModel(
        INamedTypeSymbol classSymbol,
        string parameterName,
        IPropertySymbol property)
    {
        string exampleFromXml = property.GetExampleFromXml();
        return new()
        {
            ClassSymbol = classSymbol,
            ParameterName = parameterName,
            ParameterType = property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            DefaultValue = property.GetDefaultValue(),
            Summary = property.GetSummary(),
            ExampleValue = string.IsNullOrEmpty(exampleFromXml) ? property.Type.GetExampleValue() : exampleFromXml
        };
    }
}
