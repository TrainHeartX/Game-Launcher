using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents a game controller type in the LaunchBox controller catalog.
    /// Maps to GameController elements in GameControllers.xml.
    /// </summary>
    [XmlRoot("GameController")]
    public class GameController
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? AssociatedPlatforms { get; set; }
    }
}
