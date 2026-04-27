using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents a user-defined custom field for a game in LaunchBox.
    /// Maps to CustomField elements in Platforms/{Name}.xml.
    /// </summary>
    [XmlRoot("CustomField")]
    public class CustomField
    {
        public string GameID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }
    }
}
