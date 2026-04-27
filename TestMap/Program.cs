using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class Playlist {
    public string Name { get; set; }
}

class Program
{
    static void Main()
    {
        string xml = @"<Playlist><Name>Assassin&apos;s Creed</Name></Playlist>";
        
        // 1. XmlDocument InnerText
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var nameNode = doc.SelectSingleNode("/Playlist/Name");
        string innerText = nameNode.InnerText;
        
        // 2. XmlSerializer
        var serializer = new XmlSerializer(typeof(Playlist));
        using (var reader = new StringReader(xml))
        {
            var p = (Playlist)serializer.Deserialize(reader);
            string serText = p.Name;
            
            Console.WriteLine($"InnerText: {innerText}");
            Console.WriteLine($"Serializer: {serText}");
            Console.WriteLine($"Match: {innerText == serText}");
            
            foreach (char c in innerText) Console.Write($"{(int)c} ");
            Console.WriteLine();
            foreach (char c in serText) Console.Write($"{(int)c} ");
            Console.WriteLine();
        }
    }
}
