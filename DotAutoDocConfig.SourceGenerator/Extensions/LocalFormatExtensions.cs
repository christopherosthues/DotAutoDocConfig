using DotAutoDocConfig.SourceGenerator.Models;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

internal static class LocalFormatExtensions
{
    extension(LocalFormat localFormat)
    {
        public string ToFileExtension()
        {
            return localFormat switch
            {
                LocalFormat.AsciiDoc => ".adoc",
                LocalFormat.Markdown => ".md",
                LocalFormat.Html => ".html",
                _ => ".adoc"
            };
        }
    }
}
