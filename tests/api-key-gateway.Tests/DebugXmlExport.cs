// Temporary debug file
using System;
using System.IO;
using System.Text;
using System.Xml;
using ApiKeyGateway.Utilities;

var item = new { Id = 1, Name = "Test" };
var result = XmlExportHelper.ToXml(item);

Console.WriteLine("=== OUTPUT START ===");
Console.WriteLine(result);
Console.WriteLine("=== OUTPUT END ===");
Console.WriteLine($"Length: {result.Length}");
Console.WriteLine($"First 200 chars: {result.Substring(0, Math.Min(200, result.Length))}");

try
{
    var doc = new XmlDocument();
    doc.LoadXml(result);
    Console.WriteLine("SUCCESS: XML is valid");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
}
