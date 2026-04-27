using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents the mapping between an emulator and a platform.
    /// Maps directly to XML elements in Emulators.xml.
    /// </summary>
    [XmlRoot("EmulatorPlatform")]
    public class EmulatorPlatform
    {
        /// <summary>
        /// GUID of the emulator (as string for XML compatibility).
        /// </summary>
        public string Emulator { get; set; } = string.Empty;

        /// <summary>
        /// Name of the platform this mapping applies to.
        /// </summary>
        public string Platform { get; set; } = string.Empty;

        /// <summary>
        /// Additional command line arguments for this platform.
        /// </summary>
        public string? CommandLine { get; set; }

        /// <summary>
        /// Whether this is the default emulator for the platform.
        /// </summary>
        public bool Default { get; set; }

        /// <summary>
        /// Enable M3U disc loading for multi-disc games.
        /// </summary>
        public bool M3uDiscLoadEnabled { get; set; }

        /// <summary>
        /// Automatically extract compressed ROM files before launching.
        /// </summary>
        public bool AutoExtract { get; set; }
    }
}
