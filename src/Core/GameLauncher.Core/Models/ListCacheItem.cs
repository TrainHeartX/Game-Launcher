using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents a cached game count entry for a playlist or platform category.
    /// Maps to ListCacheItem elements in ListCache.xml.
    /// </summary>
    [XmlRoot("ListCacheItem")]
    public class ListCacheItem
    {
        public string? PlaylistId { get; set; }
        public int LaunchBoxCount { get; set; }
        public int BigBoxCount { get; set; }
        public bool LaunchBoxIncludeHidden { get; set; }
        public bool BigBoxIncludeHidden { get; set; }
        public bool LaunchBoxIncludeBroken { get; set; }
        public bool BigBoxIncludeBroken { get; set; }
        public bool LaunchBoxExcludeGamesMissingVideos { get; set; }
        public bool BigBoxExcludeGamesMissingVideos { get; set; }
        public bool LaunchBoxExcludeGamesMissingBoxFrontImage { get; set; }
        public bool BigBoxExcludeGamesMissingBoxFrontImage { get; set; }
        public bool LaunchBoxExcludeGamesMissingScreenshotImage { get; set; }
        public bool BigBoxExcludeGamesMissingScreenshotImage { get; set; }
        public bool LaunchBoxExcludeGamesMissingClearLogoImage { get; set; }
        public bool BigBoxExcludeGamesMissingClearLogoImage { get; set; }
        public bool LaunchBoxExcludeGamesMissingBackgroundImage { get; set; }
        public bool BigBoxExcludeGamesMissingBackgroundImage { get; set; }
    }
}
