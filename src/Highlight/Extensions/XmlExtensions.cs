using System;
using System.Xml.Linq;

namespace Highlight.Extensions;

internal static class XmlExtensions
{
    public static string GetAttributeValue(this XElement element, XName name)
    {
        ArgumentNullException.ThrowIfNull(element);

        return element.Attribute(name)?.Value;
    }
}
