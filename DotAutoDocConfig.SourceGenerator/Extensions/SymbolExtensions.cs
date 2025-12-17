using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

public static class SymbolExtensions
{
    extension(ISymbol symbol)
    {
        public string GetSummary()
        {
            try
            {
                string? xml = symbol.GetDocumentationCommentXml(expandIncludes: true, cancellationToken: CancellationToken.None);
                if (string.IsNullOrEmpty(xml))
                {
                    return string.Empty;
                }

                try
                {
                    XDocument doc = XDocument.Parse(xml);
                    XElement? summaryEl = doc.Root?.Element("summary");
                    if (summaryEl != null)
                    {
                        string text = summaryEl.Value;
                        return NormalizeWhitespace(StripXmlLikeText(text));
                    }
                }
                catch
                {
                    // Regex-Fallback: ausschließlich den Inhalt aus <summary> extrahieren
                    Match m = Regex.Match(xml, "<summary\\b[^>]*>(?<c>.*?)</summary>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        string onlySummary = m.Groups["c"].Value;
                        return NormalizeWhitespace(StripXmlLikeText(onlySummary));
                    }
                }

                // Kein <summary> gefunden
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetExampleFromXml()
        {
            try
            {
                string? xml = symbol.GetDocumentationCommentXml(expandIncludes: true, cancellationToken: CancellationToken.None);
                if (string.IsNullOrEmpty(xml))
                {
                    return string.Empty;
                }

                try
                {
                    XDocument doc = XDocument.Parse(xml);
                    XElement? exEl = doc.Root?.Elements("example").FirstOrDefault();
                    if (exEl != null)
                    {
                        string text = exEl.Value;
                        return NormalizeWhitespace(StripXmlLikeText(text));
                    }
                }
                catch
                {
                    // Regex-Fallback: ausschließlich den Inhalt aus <example> extrahieren
                    Match m = Regex.Match(xml, "<example\\b[^>]*>(?<c>.*?)</example>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        string onlyExample = m.Groups["c"].Value;
                        return NormalizeWhitespace(StripXmlLikeText(onlyExample));
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    private static string StripXmlLikeText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }
        string withoutTags = Regex.Replace(input, "<.*?>", string.Empty);
        return withoutTags.Trim();
    }

    private static string NormalizeWhitespace(string input) => Regex.Replace(input, @"\s+", " ").Trim();
}
