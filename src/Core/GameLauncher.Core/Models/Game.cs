using System;
using System.Xml;
using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents a game in the LaunchBox library.
    /// All properties map directly to XML elements to ensure 100% compatibility.
    /// </summary>
    [XmlRoot("Game")]
    public class Game
    {
        // Identifiers
        public string ID { get; set; } = string.Empty; // GUID as string
        public string? DatabaseID { get; set; }

        /// <summary>
        /// Numeric ID from the LaunchBox Games Database (online).
        /// Different from DatabaseID (which is a GUID string).
        /// </summary>
        public int? LaunchBoxDbId { get; set; }

        // Core Information
        public string Title { get; set; } = string.Empty;
        public string? SortTitle { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string? Series { get; set; }
        public string? Version { get; set; }

        // File Paths
        public string? ApplicationPath { get; set; }
        public string? CommandLine { get; set; }
        public string? ConfigurationPath { get; set; }
        public string? ConfigurationCommandLine { get; set; }
        public string? RootFolder { get; set; }
        public string? ManualPath { get; set; }
        public string? MusicPath { get; set; }
        public string? VideoPath { get; set; }
        public string? ThemeVideoPath { get; set; }
        public string? DosBoxConfigurationPath { get; set; }
        public string? CustomDosBoxVersionPath { get; set; }
        public string? ScummVMGameDataFolderPath { get; set; }

        // Emulator Settings
        public string? Emulator { get; set; } // GUID as string

        /// <summary>
        /// When true, the game's own CommandLine is used even if the platform/emulator has a default.
        /// LaunchBox only applies game.CommandLine when this flag is set to true.
        /// </summary>
        public bool UseCustomCommandLine { get; set; }

        public bool UseDosBox { get; set; }
        public bool UseScummVM { get; set; }
        public string? ScummVMGameType { get; set; }
        public bool ScummVMAspectCorrection { get; set; }
        public bool ScummVMFullscreen { get; set; }

        // Metadata
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public string? Genre { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Region { get; set; }
        public string? PlayMode { get; set; }
        public string? Status { get; set; }
        public string? Source { get; set; }
        public string? ReleaseType { get; set; }
        public int MaxPlayers { get; set; }

        // Dates
        public DateTime? DateAdded { get; set; }
        public DateTime? DateModified { get; set; }

        /// <summary>
        /// The last date the game was played. Persisted as &lt;LastPlayedDate&gt; in LaunchBox XML.
        /// </summary>
        public DateTime? LastPlayedDate { get; set; }

        // User Data
        public bool Favorite { get; set; }
        public bool Completed { get; set; }
        public bool Broken { get; set; }
        public bool Hide { get; set; }
        public bool Portable { get; set; }
        public int PlayCount { get; set; }
        public long PlayTime { get; set; } // In seconds
        public string? Rating { get; set; }
        public float StarRatingFloat { get; set; }
        public int StarRating { get; set; }

        // Community Data
        public float CommunityStarRating { get; set; }

        /// <summary>
        /// Mapped to &lt;CommunityStarRatingTotalCount&gt; in LaunchBox XML.
        /// The property is named TotalVotes internally but serializes with the exact LaunchBox element name.
        /// </summary>
        [XmlElement("CommunityStarRatingTotalCount")]
        public int CommunityStarRatingTotalVotes { get; set; }

        // Notes and URLs
        public string? Notes { get; set; }
        public string? WikipediaURL { get; set; }
        public string? VideoUrl { get; set; }

        // Clone Information
        public string? CloneOf { get; set; }

        // Missing Media Flags
        public bool MissingVideo { get; set; }
        public bool MissingBoxFrontImage { get; set; }
        public bool MissingScreenshotImage { get; set; }
        public bool MissingMarqueeImage { get; set; }
        public bool MissingClearLogoImage { get; set; }
        public bool MissingBackgroundImage { get; set; }
        public bool MissingBox3dImage { get; set; }
        public bool MissingCartImage { get; set; }
        public bool MissingCart3dImage { get; set; }
        public bool MissingManual { get; set; }
        public bool MissingBannerImage { get; set; }
        public bool MissingMusic { get; set; }

        // Startup/Shutdown Settings
        public bool UseStartupScreen { get; set; }
        public bool HideAllNonExclusiveFullscreenWindows { get; set; }
        public int StartupLoadDelay { get; set; }
        public bool HideMouseCursorInGame { get; set; }
        public bool DisableShutdownScreen { get; set; }
        public bool AggressiveWindowHiding { get; set; }
        public bool OverrideDefaultStartupScreenSettings { get; set; }

        // Pause Screen Settings
        public bool UsePauseScreen { get; set; }
        public string? PauseAutoHotkeyScript { get; set; }
        public string? ResumeAutoHotkeyScript { get; set; }
        public bool OverrideDefaultPauseScreenSettings { get; set; }
        public bool SuspendProcessOnPause { get; set; }
        public bool ForcefulPauseScreenActivation { get; set; }

        // AutoHotkey Scripts
        public string? LoadStateAutoHotkeyScript { get; set; }
        public string? SaveStateAutoHotkeyScript { get; set; }
        public string? ResetAutoHotkeyScript { get; set; }
        public string? SwapDiscsAutoHotkeyScript { get; set; }

        // Store Integration
        public string? GogAppId { get; set; }
        public string? OriginAppId { get; set; }
        public string? OriginInstallPath { get; set; }

        // Android Paths
        public string? AndroidBoxFrontThumbPath { get; set; }
        public string? AndroidBoxFrontFullPath { get; set; }
        public string? AndroidClearLogoThumbPath { get; set; }
        public string? AndroidClearLogoFullPath { get; set; }
        public string? AndroidBackgroundPath { get; set; }
        public string? AndroidBackgroundThumbPath { get; set; }
        public string? AndroidGameTitleScreenshotThumbPath { get; set; }
        public string? AndroidGameplayScreenshotThumbPath { get; set; }
        public string? AndroidGameTitleScreenshotPath { get; set; }
        public string? AndroidGameplayScreenshotPath { get; set; }
        public string? AndroidVideoPath { get; set; }

        // Installation State
        public bool Installed { get; set; }

        // Cloud Sync
        public bool HasCloudSynced { get; set; }

        /// <summary>
        /// Preserves any XML elements not explicitly mapped to properties.
        /// This ensures round-trip fidelity: unknown LaunchBox fields are not lost when GameLauncher saves.
        /// </summary>
        [XmlAnyElement]
        public XmlElement[] UnknownElements { get; set; } = Array.Empty<XmlElement>();

        /// <summary>
        /// Gets the last time the game was played.
        /// Prefers the persisted LastPlayedDate field; falls back to DateModified for backwards compatibility.
        /// </summary>
        [XmlIgnore]
        public DateTime? LastPlayed => LastPlayedDate ?? (PlayCount > 0 ? DateModified : null);
    }
}
