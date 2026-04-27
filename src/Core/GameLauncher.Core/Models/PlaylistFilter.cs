using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents an auto-populate filter rule for a LaunchBox playlist.
    /// Maps to PlaylistFilter elements in Playlists/{Name}.xml.
    /// </summary>
    [XmlRoot("PlaylistFilter")]
    public class PlaylistFilter
    {
        public string? Value { get; set; }
        public string? FieldKey { get; set; }
        public string? ComparisonTypeKey { get; set; }
    }
}
