using System;
using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents an additional application or alternate version for a game in LaunchBox.
    /// Maps to AdditionalApplication elements in Platforms/{Name}.xml.
    /// </summary>
    [XmlRoot("AdditionalApplication")]
    public class AdditionalApplication
    {
        public string Id { get; set; } = string.Empty;
        public string GameID { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? ApplicationPath { get; set; }
        public string? CommandLine { get; set; }
        public bool AutoRunBefore { get; set; }
        public bool AutoRunAfter { get; set; }
        public bool WaitForExit { get; set; }
        public bool UseDosBox { get; set; }
        public bool UseEmulator { get; set; }
        public string? EmulatorId { get; set; }
        public bool SideA { get; set; }
        public bool SideB { get; set; }
        public int Priority { get; set; }
        public int PlayCount { get; set; }
        public long PlayTime { get; set; }
        public DateTime? LastPlayed { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public string? Region { get; set; }
        public string? Version { get; set; }
        public string? Status { get; set; }
        public string? GogAppId { get; set; }
        public string? OriginAppId { get; set; }
        public string? OriginInstallPath { get; set; }
        public bool HasCloudSynced { get; set; }
    }
}
