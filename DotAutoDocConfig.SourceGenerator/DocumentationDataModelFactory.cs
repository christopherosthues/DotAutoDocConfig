using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotAutoDocConfig.SourceGenerator;

internal static class DocumentationDataModelFactory
{
    public static Models.DocumentationDataModel CreateLeafDocumentationDataModel(
        INamedTypeSymbol classSymbol,
        string parameterName,
        IPropertySymbol property)
    {
        string exampleFromXml = GetExampleFromXml(property);
        return new()
        {
            ClassSymbol = classSymbol,
            ParameterName = parameterName,
            ParameterType = property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            DefaultValue = GetDefaultValue(property),
            Summary = GetSummary(property),
            ExampleValue = string.IsNullOrEmpty(exampleFromXml) ? GetExampleValue(property.Type) : exampleFromXml
        };
    }

    public static Models.DocumentationDataModel CreateDocumentationDataModel(
        INamedTypeSymbol classSymbol,
        string parameterName,
        IPropertySymbol property)
    {
        return new()
        {
            ClassSymbol = classSymbol,
            ParameterName = parameterName,
            ParameterType = property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            DefaultValue = GetDefaultValue(property),
            Summary = GetSummary(property),
            ExampleValue = GetExampleValue(property.Type)
        };
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
