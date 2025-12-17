using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
// removed dependency on DotAutoDocConfig.Core to avoid loading that assembly at generator init
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotAutoDocConfig.SourceGenerator;

internal static class GeneratorHelpers
{
    // Collect documentation entries recursively for a root configuration class.
    public static List<DocumentationDataModel> CollectDocumentationEntries(INamedTypeSymbol root, Compilation compilation)
    {
        List<DocumentationDataModel> result = new();
        HashSet<INamedTypeSymbol> visited = new(SymbolEqualityComparer.Default);

        Recurse(root, string.Empty, visited, root, result);
        return result;
    }

    // New: Collect root rows and per-type tables (Tables mode). Deduplicates reused classes by symbol.
    public static DocumentationTablesModel CollectTables(INamedTypeSymbol root, Compilation compilation)
    {
        DocumentationTablesModel model = new();
        HashSet<INamedTypeSymbol> globalVisited = new(SymbolEqualityComparer.Default);
        BuildRows(root, string.Empty, model.RootRows, model.TypeTables, globalVisited);
        return model;
    }

    private static void BuildRows(INamedTypeSymbol current, string prefix, List<TableRow> rows,
        Dictionary<INamedTypeSymbol, List<TableRow>> typeTables, HashSet<INamedTypeSymbol> globalVisited)
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
                DocumentationDataModel baseData = new()
                {
                    ClassSymbol = current,
                    ParameterName = parameterName,
                    ParameterType = member.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                    DefaultValue = GetDefaultValue(member),
                    Summary = GetSummary(member),
                    ExampleValue = GetExampleValue(member.Type)
                };

                rows.Add(new TableRow { Data = baseData, ComplexTarget = namedType });

                // ensure we build the table for namedType once
                if (!typeTables.ContainsKey(namedType))
                {
                    List<TableRow> subRows = new();
                    typeTables[namedType] = subRows;
                    BuildRows(namedType, string.Empty, subRows, typeTables, globalVisited);
                }

                continue;
            }

            // leaf property -> plain row
            string exampleFromXml = GetExampleFromXml(member);
            DocumentationDataModel model = new()
            {
                ClassSymbol = current,
                ParameterName = parameterName,
                ParameterType = member.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                DefaultValue = GetDefaultValue(member),
                Summary = GetSummary(member),
                ExampleValue = string.IsNullOrEmpty(exampleFromXml) ? GetExampleValue(member.Type) : exampleFromXml
            };

            rows.Add(new TableRow { Data = model, ComplexTarget = null });
        }
    }

    // Recurse stays a class method (not a local function).
    private static void Recurse(INamedTypeSymbol? current, string prefix, HashSet<INamedTypeSymbol> visited, INamedTypeSymbol root, List<DocumentationDataModel> result)
    {
        if (current is null)
        {
            return;
        }

        if (!visited.Add(current))
        {
            return; // prevent cycles
        }

        foreach (IPropertySymbol member in current.GetMembers().OfType<IPropertySymbol>())
        {
            // Only public properties
            if (member.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            // Skip if marked with ExcludeFromDocumentationAttribute (compare by full name string to avoid a type reference)
            AttributeData? excludeAttribute = member.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == "DotAutoDocConfig.Core.ComponentModel.Attributes.ExcludeFromDocumentationAttribute");
            if (excludeAttribute != null)
            {
                continue;
            }

            string formattedName = member.Name;//FormatSegment(member.Name);
            string separator = ":"; // JsonShort notation uses colon
            string parameterName = string.IsNullOrEmpty(prefix) ? formattedName : prefix + separator + formattedName;

            ITypeSymbol propType = member.Type;

            // If array or generic enumerable, get the element type
            if (propType is IArrayTypeSymbol arrayType)
            {
                propType = arrayType.ElementType;
            }
            else if (propType is INamedTypeSymbol named && named.IsGenericType)
            {
                // try to find IEnumerable<T>
                ITypeSymbol? firstArg = named.TypeArguments.FirstOrDefault();
                if (firstArg != null && named.AllInterfaces.Any(i => i.MetadataName.Contains("IEnumerable")))
                {
                    propType = firstArg;
                }
            }

            // If the property type is a class/record (and not system/primitive), recurse into it
            if (propType is INamedTypeSymbol namedType && ShouldRecurseInto(namedType))
            {
                // Recurse into child properties using the parameter name as prefix
                Recurse(namedType, parameterName, visited, root, result);
                continue; // do not emit the parent as a simple property
            }

            // Otherwise emit a documentation entry for this property
            string exampleFromXml = GetExampleFromXml(member);
            DocumentationDataModel model = new()
            {
                ClassSymbol = root,
                ParameterName = parameterName,
                ParameterType = member.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                DefaultValue = GetDefaultValue(member),
                Summary = GetSummary(member),
                ExampleValue = string.IsNullOrEmpty(exampleFromXml) ? GetExampleValue(member.Type) : exampleFromXml
            };

            result.Add(model);
        }
    }

    private static bool ShouldRecurseInto(INamedTypeSymbol type)
    {
        // Do not recurse into system types or enums or delegates
        if (type.TypeKind == TypeKind.Enum)
        {
            return false;
        }

        if (type.SpecialType != SpecialType.None)
        {
            return false;
        }

        // Common framework types we treat as leaves
        string ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (ns.StartsWith("System", StringComparison.Ordinal))
        {
            return false;
        }

        // Treat records/classes defined in user's code as recurse candidates
        return true;
    }

    private static string FormatSegment(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        if (name.Length == 1)
        {
            return name.ToLowerInvariant();
        }

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static string GetSummary(ISymbol symbol)
    {
        try
        {
            string? xml = symbol.GetDocumentationCommentXml();
            if (string.IsNullOrEmpty(xml))
            {
                return string.Empty;
            }

            // Wrap into a root element to ensure valid XML for parsing
            string wrapped = "<root>" + xml + "</root>";
            try
            {
                XDocument doc = XDocument.Parse(wrapped);
                XElement? summaryEl = doc.Root?.Element("summary");
                if (summaryEl != null)
                {
                    string text = summaryEl.Value;
                    return NormalizeWhitespace(StripXmlLikeText(text));
                }
            }
            catch
            {
                // fall through to the naive fallback
            }

            // naive strip xml tags as fallback
            string stripped = Regex.Replace(xml, "<.*?>", string.Empty);
            return NormalizeWhitespace(stripped);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetExampleFromXml(ISymbol symbol)
    {
        try
        {
            string? xml = symbol.GetDocumentationCommentXml();
            if (string.IsNullOrEmpty(xml))
            {
                return string.Empty;
            }

            string wrapped = "<root>" + xml + "</root>";
            try
            {
                XDocument doc = XDocument.Parse(wrapped);
                XElement? exEl = doc.Root?.Element("example");
                if (exEl != null)
                {
                    string text = exEl.Value;
                    return NormalizeWhitespace(StripXmlLikeText(text));
                }
            }
            catch
            {
                // ignore and fallback
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string StripXmlLikeText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // remove common xml tags if any remain
        string withoutTags = Regex.Replace(input, "<.*?>", string.Empty);
        return withoutTags.Trim();
    }

    private static string NormalizeWhitespace(string input) => Regex.Replace(input, @"\s+", " ").Trim();

    private static string GetDefaultValue(IPropertySymbol property)
    {
        // Try to get initializer from syntax
        try
        {
            SyntaxReference? declRef = property.DeclaringSyntaxReferences.FirstOrDefault();
            if (declRef != null)
            {
                SyntaxNode node = declRef.GetSyntax();
                if (node is PropertyDeclarationSyntax propSyntax && propSyntax.Initializer != null)
                {
                    // Return the initializer expression as written in source
                    return propSyntax.Initializer.Value.ToString();
                }
            }

            // Fallbacks
            if (property.Type.TypeKind == TypeKind.Enum)
            {
                // get first enum member
                if (property.Type is INamedTypeSymbol enumType)
                {
                    IFieldSymbol? first = enumType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(f => f.ConstantValue != null);
                    return first?.Name ?? string.Empty;
                }
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetExampleValue(ITypeSymbol? type)
    {
        try
        {
            if (type is null)
            {
                return string.Empty;
            }

            if (type.TypeKind == TypeKind.Enum && type is INamedTypeSymbol enumType)
            {
                IFieldSymbol? first = enumType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(f => f.ConstantValue != null);
                return first?.Name ?? string.Empty;
            }

            switch (type.SpecialType)
            {
                case SpecialType.System_String:
                    return "example";
                case SpecialType.System_Boolean:
                    return "true";
                case SpecialType.System_Char:
                    return "c";
                case SpecialType.System_Double:
                case SpecialType.System_Single:
                case SpecialType.System_Decimal:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Int16:
                case SpecialType.System_Byte:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_UInt16:
                case SpecialType.System_SByte:
                    return "123";
            }

            // Arrays & collections -> example JSON array
            if (type is IArrayTypeSymbol)
            {
                return "[ ]";
            }

            if (type is INamedTypeSymbol named && named.IsGenericType && named.TypeArguments.Length == 1)
            {
                ITypeSymbol firstArg = named.TypeArguments[0];
                if (firstArg.SpecialType == SpecialType.System_String)
                {
                    return "[ \"en\" ]";
                }

                return "[ ]";
            }

            // Default fallback for complex objects
            return "{ }";
        }
        catch
        {
            return string.Empty;
        }
    }
}
