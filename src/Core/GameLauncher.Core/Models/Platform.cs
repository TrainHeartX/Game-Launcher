using System;
using System.Xml;
using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents a gaming platform in LaunchBox.
    /// Maps directly to XML elements in Platforms.xml.
    /// </summary>
    [XmlRoot("Platform")]
    public class Platform
    {
        // Basic Information
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? SortTitle { get; set; }
        public bool LocalDbParsed { get; set; }
        public string? LastSelectedChild { get; set; }
        public string? LastGameId { get; set; }

        // Release Information
        public DateTime? ReleaseDate { get; set; }
        public string? Developer { get; set; }
        public string? Manufacturer { get; set; }

        // Technical Specifications
        public string? Cpu { get; set; }
        public string? Memory { get; set; }
        public string? Graphics { get; set; }
        public string? Sound { get; set; }
        public string? Display { get; set; }
        public string? Media { get; set; }
        public string? MaxControllers { get; set; } // Changed from int? to string? for LaunchBox compatibility

        // Paths and Folders
        public string? Folder { get; set; }
        public string? VideosFolder { get; set; }
        public string? FrontImagesFolder { get; set; }
        public string? BackImagesFolder { get; set; }
        public string? ClearLogoImagesFolder { get; set; }
        public string? FanartImagesFolder { get; set; }
        public string? ScreenshotImagesFolder { get; set; }
        public string? BannerImagesFolder { get; set; }
        public string? SteamBannerImagesFolder { get; set; }
        public string? ManualsFolder { get; set; }
        public string? MusicFolder { get; set; }

        // Scraping and Metadata
        public string? ScrapeAs { get; set; }
        public string? Notes { get; set; }

        // Display Settings
        public string? VideoPath { get; set; }
        public string? ImageType { get; set; }

        // BigBox Settings
        public string? BigBoxView { get; set; }
        public string? BigBoxTheme { get; set; }
        public bool HideInBigBox { get; set; }

        // Android Settings
        public string? AndroidThemeVideoPath { get; set; }

        /// <summary>
        /// Preserves any XML elements not explicitly mapped to properties.
        /// Ensures round-trip fidelity with LaunchBox Platforms.xml.
        /// </summary>
        [XmlAnyElement]
        public XmlElement[] UnknownElements { get; set; } = Array.Empty<XmlElement>();
    }
}
