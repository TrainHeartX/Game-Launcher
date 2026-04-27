using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents an alternate name (regional/localized title) for a game in LaunchBox.
    /// Maps to AlternateName elements in Platforms/{Name}.xml.
    /// </summary>
    [XmlRoot("AlternateName")]
    public class AlternateName
    {
        public string GameID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Region { get; set; }
    }
}
