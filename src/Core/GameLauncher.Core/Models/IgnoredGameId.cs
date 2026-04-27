using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents a blacklisted game ID that should be ignored during import.
    /// Maps to IgnoredGameId elements in ImportBlacklist.xml.
    /// </summary>
    [XmlRoot("IgnoredGameId")]
    public class IgnoredGameId
    {
        public string GameId { get; set; } = string.Empty;
    }
}
