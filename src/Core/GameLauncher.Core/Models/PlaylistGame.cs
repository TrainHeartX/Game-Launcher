using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents a game entry within a LaunchBox playlist.
    /// Maps to PlaylistGame elements in Playlists/{Name}.xml.
    /// </summary>
    [XmlRoot("PlaylistGame")]
    public class PlaylistGame
    {
        public string GameId { get; set; } = string.Empty;
        public int LaunchBoxDbId { get; set; }
        public string? GameTitle { get; set; }
        public string? GameFileName { get; set; }
        public string? GamePlatform { get; set; }
        public int ManualOrder { get; set; }
    }
}
