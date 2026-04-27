using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents a parent-child hierarchy relationship in LaunchBox.
    /// Used to organize platforms, playlists, and categories into a navigable tree.
    /// Maps to Parent elements in Parents.xml.
    /// </summary>
    [XmlRoot("Parent")]
    public class Parent
    {
        public string? PlatformName { get; set; }
        public string? PlaylistId { get; set; }
        public string? PlatformCategoryName { get; set; }
        public string? ParentPlatformName { get; set; }
        public string? ParentPlaylistId { get; set; }
        public string? ParentPlatformCategoryName { get; set; }
    }
}
