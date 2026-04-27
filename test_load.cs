using System;
using System.IO;
using System.Linq;
using GameLauncher.Data.Xml;
using GameLauncher.Core.Models;

class Program
{
    static void Main()
    {
        string dataPath = @"H:\LaunchBox\LaunchBox\Data";
        var ctx = new XmlDataContext(dataPath);
        
        var data = ctx.LoadPlaylistFileData("Assassin's Creed");
        Console.WriteLine($"Games in Assassin's Creed: {data.Games.Count}");
        
        var allPlaylists = ctx.LoadAllPlaylists();
        var playlist = allPlaylists.FirstOrDefault(p => p.Name == "Assassin's Creed");
        Console.WriteLine($"Found playlist: {playlist != null}");
        if (playlist != null) {
            Console.WriteLine($"Playlist Id: {playlist.PlaylistId}");
        }
    }
}
