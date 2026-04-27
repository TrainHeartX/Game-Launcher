using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string dir = @"H:\LaunchBox\LaunchBox\Data\Playlists";
        string targetPlaylist = "Assassin's Creed";
        
        // Emulate Map
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in Directory.GetFiles(dir, "*.xml"))
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
            var doc = new XmlDocument();
            doc.Load(filePath);
            var nameNode = doc.SelectSingleNode("/LaunchBox/Playlist/Name");
            var displayName = nameNode?.InnerText;
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                map[displayName] = fileNameNoExt;
            }
        }
        
        Console.WriteLine($"Map count: {map.Count}");
        
        if (map.TryGetValue(targetPlaylist, out var resolved))
        {
            Console.WriteLine($"Resolved '{targetPlaylist}' to '{resolved}'");
            string exactPath = Path.Combine(dir, resolved + ".xml");
            
            var doc = new XmlDocument();
            doc.Load(exactPath);
            var nodes = doc.SelectNodes("//PlaylistGame");
            Console.WriteLine($"Found {nodes.Count} PlaylistGame entries.");
            
            if (nodes.Count > 0)
            {
                var firstNode = nodes[0];
                var gameIdNode = firstNode.SelectSingleNode("GameId");
                Console.WriteLine($"First GameId: {gameIdNode?.InnerText}");
            }
        }
        else
        {
            Console.WriteLine($"FAILED to resolve '{targetPlaylist}' in map.");
            foreach (var kvp in map)
            {
                if (kvp.Key.Contains("Assassin"))
                    Console.WriteLine($"Partial match: {kvp.Key} -> {kvp.Value}");
            }
        }
    }
}
