using System;

namespace DotAutoDocConfig.Core.ComponentModel.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class DocumentationAttribute(DocumentationFormat format) : Attribute
{
    public DocumentationFormat Format { get; } = format;
}
