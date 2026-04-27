using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents a gamepad input binding for LaunchBox/BigBox actions.
    /// Maps to InputBinding elements in InputBindings.xml.
    /// </summary>
    [XmlRoot("InputBinding")]
    public class InputBinding
    {
        public string InputAction { get; set; } = string.Empty;
        public string? ControllerHoldBinding { get; set; }
        public string? ControllerBinding { get; set; }
    }
}
