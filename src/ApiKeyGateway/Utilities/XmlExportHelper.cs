// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;
using System.Xml;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Helper class to convert objects and collections to XML format.
/// Used for exporting data in XML format for systems that require it
/// or for integration with legacy enterprise systems.
/// </summary>
public static class XmlExportHelper
{
    /// <summary>
    /// Converts a single object to XML representation.
    /// Root element is derived from the object type name.
    /// </summary>
    public static string ToXml<T>(T item, string rootElementName = null) where T : class
    {
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            ConformanceLevel = ConformanceLevel.Document
        }))
        {
            writer.WriteStartDocument();
            var elementName = rootElementName ?? typeof(T).Name;
            WriteElement(writer, elementName, item);
            writer.WriteEndDocument();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Converts a collection of objects to XML with items wrapped in a root element.
    /// </summary>
    public static string ToXml<T>(IEnumerable<T> items, string rootName = "root", string itemName = null) where T : class
    {
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            ConformanceLevel = ConformanceLevel.Document
        }))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(rootName);

            var itemElementName = itemName ?? typeof(T).Name;
            foreach (var item in items)
            {
                WriteElement(writer, itemElementName, item);
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Writes a single object as XML element with its properties as child elements.
    /// Handles nested objects and basic types only - complex types are stringified.
    /// </summary>
    private static void WriteElement<T>(XmlWriter writer, string elementName, T obj) where T : class
    {
        writer.WriteStartElement(elementName);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase);
        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);
            if (value == null)
                continue;

            // Use safe XML element names - replace invalid characters
            var safeElementName = MakeSafeElementName(prop.Name);

            if (value is string || value is IFormattable)
            {
                writer.WriteElementString(safeElementName, value?.ToString() ?? string.Empty);
            }
            else if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                writer.WriteStartElement(safeElementName + "Items");
                foreach (var item in enumerable)
                {
                    writer.WriteElementString("Item", item?.ToString() ?? string.Empty);
                }
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteElementString(safeElementName, value?.ToString() ?? string.Empty);
            }
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Ensures element names are valid XML identifiers.
    /// </summary>
    private static string MakeSafeElementName(string name)
    {
        // Replace invalid characters with underscores
        return System.Text.RegularExpressions.Regex.Replace(name, @"[^\w.]", "_");
    }
}
