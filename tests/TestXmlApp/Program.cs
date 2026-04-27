using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using GameLauncher.Core.Models;

try
{
    Console.WriteLine("Testing Platform XML deserialization...");
    string xmlPath = @"H:\LaunchBox\LaunchBox\Data\Platforms.xml";

    if (!File.Exists(xmlPath))
    {
        Console.WriteLine($"ERROR: File not found: {xmlPath}");
        return;
    }

    var doc = new XmlDocument();
    doc.Load(xmlPath);

    var nodes = doc.SelectNodes("//Platform");
    Console.WriteLine($"Found {nodes?.Count ?? 0} platforms");

    if (nodes != null && nodes.Count > 0)
    {
        int success = 0;
        int errors = 0;
        Platform? problematicPlatform = null;
        string? problematicXml = null;

        Console.WriteLine("\nDeserializing all platforms...");

        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i]!;

            try
            {
                using (var stringReader = new StringReader(node.OuterXml))
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    var serializer = new XmlSerializer(typeof(Platform));
                    serializer.UnknownElement += (s, e) => { /* Ignore */ };
                    serializer.UnknownAttribute += (s, e) => { /* Ignore */ };

                    var platform = (Platform)serializer.Deserialize(xmlReader)!;
                    success++;

                    // Check for Windows platform specifically
                    if (platform.Name?.ToLower().Contains("windows") == true)
                    {
                        Console.WriteLine($"  Found Windows platform: {platform.Name}");
                        Console.WriteLine($"    ReleaseDate: {platform.ReleaseDate}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors++;
                if (problematicPlatform == null)
                {
                    problematicXml = node.OuterXml;
                    Console.WriteLine($"\n✗ ERROR at platform {i}:");
                    Console.WriteLine($"  Message: {ex.Message}");
                    Console.WriteLine($"  XML (first 800 chars):\n{node.OuterXml.Substring(0, Math.Min(800, node.OuterXml.Length))}");

                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"\n  Inner exception: {ex.InnerException.Message}");
                    }
                }
            }
        }

        Console.WriteLine($"\n✓ Successfully deserialized: {success} platforms");
        Console.WriteLine($"✗ Errors: {errors} platforms");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ FATAL ERROR: {ex.Message}");
    Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
}

Console.WriteLine("\nTest completed.");
