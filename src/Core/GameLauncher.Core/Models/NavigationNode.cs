using System.Collections.Generic;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Type of navigation node in the LaunchBox tree.
    /// </summary>
    public enum NavigationNodeType
    {
        Category,
        Platform,
        Playlist,
        QuickFilter
    }

    /// <summary>
    /// Represents a node in the LaunchBox navigation tree.
    /// Can be a PlatformCategory, Platform, or Playlist.
    /// Built from Parents.xml hierarchy + Platforms.xml categories + Playlists.
    /// </summary>
    public class NavigationNode
    {
        public string Name { get; set; } = string.Empty;
        public NavigationNodeType NodeType { get; set; }
        public List<NavigationNode> Children { get; set; } = new();

        // Populated depending on NodeType
        public Platform? Platform { get; set; }
        public Playlist? Playlist { get; set; }
        public PlatformCategory? PlatformCategory { get; set; }

        /// <summary>
        /// For Playlist nodes: the PlaylistId from Parents.xml.
        /// </summary>
        public string? PlaylistId { get; set; }

        /// <summary>
        /// For QuickFilter nodes: the filter key ("favorites", "recent", "completed").
        /// </summary>
        public string? QuickFilterKey { get; set; }
    }
}
