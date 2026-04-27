using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents controller compatibility information for a game in LaunchBox.
    /// Maps to GameControllerSupport elements in Platforms/{Name}.xml.
    /// </summary>
    [XmlRoot("GameControllerSupport")]
    public class GameControllerSupport
    {
        public string ControllerId { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        public int SupportLevel { get; set; }
    }
}
