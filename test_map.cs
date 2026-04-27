using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

class Program
{
    static void Main()
    {
        string dir = @"H:\LaunchBox\LaunchBox\Data\Playlists";
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var filePath in Directory.GetFiles(dir, "*.xml"))
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
            try
            {
                var doc = new XmlDocument();
                doc.Load(filePath);
                var nameNode = doc.SelectSingleNode("/LaunchBox/Playlist/Name");
                var displayName = nameNode?.InnerText;

                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    map[displayName] = fileNameNoExt;
                    if (displayName.Contains("'"))
                    {
                        Console.WriteLine($"Mapped: {displayName} -> {fileNameNoExt}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {fileNameNoExt}: {ex.Message}");
            }
        }
        
        Console.WriteLine($"Lookup 'Assassin's Creed': {map.ContainsKey("Assassin's Creed")}");
    }
}
