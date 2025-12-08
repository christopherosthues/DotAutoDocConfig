// ...existing code...
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotAutoDocConfig.Core.ComponentModel;
using DotAutoDocConfig.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotAutoDocConfig.SourceGenerator;

internal static class GeneratorHelpers
{
    // Collect documentation entries recursively for a root configuration class.
    public static List<Models.DocumentationDataModel> CollectDocumentationEntries(INamedTypeSymbol root, Compilation compilation)
    {
        List<DocumentationDataModel> result = [];
        HashSet<INamedTypeSymbol> visited = new(SymbolEqualityComparer.Default);

        Recurse(root, string.Empty, visited, root, result);
        return result;
    }

    private static void Recurse(INamedTypeSymbol? current, string prefix, HashSet<INamedTypeSymbol> visited, INamedTypeSymbol root, List<DocumentationDataModel> result)
    {
        if (current is null)
            return;
        if (!visited.Add(current))
            return; // prevent cycles

        foreach (IPropertySymbol? member in current.GetMembers().OfType<IPropertySymbol>())
        {
            // Only public properties
            if (member.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Skip if marked with ExcludeFromDocumentationAttribute
            AttributeData? excludeAttribute = member.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() ==
                                        "DotAutoDocConfig.Core.ComponentModel.Attributes.ExcludeFromDocumentationAttribute");
            if (excludeAttribute != null)
                continue;

            string formattedName = FormatSegment(member.Name);
            string parameterName = string.IsNullOrEmpty(prefix) ? formattedName : prefix + "." + formattedName;

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
            DocumentationDataModel model = new()
            {
                ClassSymbol = root,
                ParameterName = parameterName,
                ParameterType = member.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                DefaultValue = GetDefaultValue(member),
                Summary = GetSummary(member),
                ExampleValue = GetExampleValue(member.Type)
            };

            result.Add(model);
        }
    }

    private static bool ShouldRecurseInto(INamedTypeSymbol type)
    {
        // Do not recurse into system types or enums or delegates
        if (type.TypeKind == TypeKind.Enum)
            return false;
        if (type.SpecialType != SpecialType.None)
            return false;
        // Common framework types we treat as leaves
        string ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (ns.StartsWith("System", StringComparison.Ordinal))
            return false;
        // Treat records/classes defined in user's code as recurse candidates
        return true;
    }

    public static string FormatSegment(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        if (name.Length == 1)
            return name.ToLowerInvariant();
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static string GetSummary(ISymbol symbol)
    {
        try
        {
            string? xml = symbol.GetDocumentationCommentXml();
            if (string.IsNullOrEmpty(xml))
                return string.Empty;
            // naive strip xml tags
            string stripped = Regex.Replace(xml, "<.*?>", string.Empty);
            return Regex.Replace(stripped, @"\s+", " ").Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

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
                return string.Empty;

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
                return "[ ]";
            if (type is INamedTypeSymbol named && named.IsGenericType && named.TypeArguments.Length == 1)
            {
                ITypeSymbol firstArg = named.TypeArguments[0];
                if (firstArg.SpecialType == SpecialType.System_String)
                    return "[ \"en\" ]";
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

    public static string SanitizeFileName(string outputPath, DocumentationFormat format)
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            string ext = format == DocumentationFormat.AsciiDoc ? ".adoc" : ".md";
            return "documentation" + ext;
        }

        // Take only the filename part and replace invalid chars
        try
        {
            string? fileName = System.IO.Path.GetFileName(outputPath);
            if (string.IsNullOrEmpty(fileName))
                fileName = outputPath.Replace(System.IO.Path.DirectorySeparatorChar, '_').Replace(System.IO.Path.AltDirectorySeparatorChar, '_');
            // ensure extension matches format
            string extWanted = format == DocumentationFormat.AsciiDoc ? ".adoc" : ".md";
            string? currentExt = System.IO.Path.GetExtension(fileName);
            if (!currentExt.Equals(extWanted, StringComparison.OrdinalIgnoreCase))
            {
                fileName = System.IO.Path.GetFileNameWithoutExtension(fileName) + extWanted;
            }
            // sanitize
            string sanitized = Regex.Replace(fileName, "[^a-zA-Z0-9._-]", "_");
            return sanitized;
        }
        catch
        {
            return "documentation" + (format == DocumentationFormat.AsciiDoc ? ".adoc" : ".md");
        }
    }
}
