using System;

namespace DotAutoDocConfig.Core.ComponentModel.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class ExcludeFromDocumentationAttribute(string? reason = null) : Attribute
{
    public string? Reason { get; } = reason;
}
