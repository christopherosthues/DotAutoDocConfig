using System.Collections.Generic;
using System.Linq;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Collectors;

internal class SeparateTableCollector : TableCollectorBase, IConfigurationCollector
{
    // Collect root rows and per-type tables (Tables mode). Deduplicates reused classes by symbol.
    public DocumentationTablesModel Collect(INamedTypeSymbol root)
    {
        DocumentationTablesModel model = new();
        BuildRows(root, string.Empty, model.RootRows, model.TypeTables);
        return model;
    }

    private static void BuildRows(INamedTypeSymbol current, string prefix, List<TableRow> rows,
        Dictionary<INamedTypeSymbol, List<TableRow>> typeTables)
    {
        foreach (IPropertySymbol member in current.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            AttributeData? excludeAttribute = member.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == "DotAutoDocConfig.Core.ComponentModel.Attributes.ExcludeFromDocumentationAttribute");
            if (excludeAttribute != null)
            {
                continue;
            }

            string formattedName = member.Name;
            string separator = ":"; // JsonShort notation
            string parameterName = string.IsNullOrEmpty(prefix) ? formattedName : prefix + separator + formattedName;

            ITypeSymbol propType = member.Type;
            if (propType is IArrayTypeSymbol arrayType)
            {
                propType = arrayType.ElementType;
            }
            else if (propType is INamedTypeSymbol named && named.IsGenericType)
            {
                ITypeSymbol? firstArg = named.TypeArguments.FirstOrDefault();
                if (firstArg != null && named.AllInterfaces.Any(i => i.MetadataName.Contains("IEnumerable")))
                {
                    propType = firstArg;
                }
            }

            if (propType is INamedTypeSymbol namedType && ShouldRecurseInto(namedType))
            {
                // complex property -> add a row that links to namedType's table
                DocumentationDataModel baseData =
                    DocumentationDataModelFactory.CreateDocumentationDataModel(current, parameterName, member);

                rows.Add(new TableRow { Data = baseData, ComplexTarget = namedType });

                // ensure we build the table for namedType once
                if (!typeTables.ContainsKey(namedType))
                {
                    List<TableRow> subRows = [];
                    typeTables[namedType] = subRows;
                    BuildRows(namedType, string.Empty, subRows, typeTables);
                }

                continue;
            }

            // leaf property -> plain row
            DocumentationDataModel model =
                DocumentationDataModelFactory.CreateLeafDocumentationDataModel(current, parameterName, member);

            rows.Add(new TableRow { Data = model, ComplexTarget = null });
        }
    }
}
