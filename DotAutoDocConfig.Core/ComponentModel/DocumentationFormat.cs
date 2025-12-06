using System;

namespace DotAutoDocConfig.Core.ComponentModel;

[Flags]
public enum DocumentationFormat : byte
{
    None = 0,
    AsciiDoc = 1 << 0,
    Markdown = 1 << 1,
    Html = 1 << 2
}
