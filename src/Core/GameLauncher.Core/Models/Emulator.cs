using System;
using System.Xml;
using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents an emulator configuration in LaunchBox.
    /// Maps directly to XML elements in Emulators.xml.
    /// </summary>
    [XmlRoot("Emulator")]
    public class Emulator
    {
        // Identifiers
        public string ID { get; set; } = string.Empty; // GUID as string
        public string Title { get; set; } = string.Empty;

        // Application Settings
        public string? ApplicationPath { get; set; }
        public string? CommandLine { get; set; }
        public string? DefaultPlatform { get; set; }

        // Command Line Options
        public bool NoQuotes { get; set; }
        public bool NoSpace { get; set; }
        public bool HideConsole { get; set; }
        public bool FileNameWithoutExtensionAndPath { get; set; }
        public bool AutoExtract { get; set; }

        // AutoHotkey Scripts
        public string? AutoHotkeyScript { get; set; }
        public string? ExitAutoHotkeyScript { get; set; }

        // Startup/Shutdown Settings
        public bool UseStartupScreen { get; set; }
        public bool HideAllNonExclusiveFullscreenWindows { get; set; }
        public int StartupLoadDelay { get; set; }
        public bool HideMouseCursorInGame { get; set; }
        public bool DisableShutdownScreen { get; set; }
        public bool AggressiveWindowHiding { get; set; }

        // Pause Screen Settings
        public bool UsePauseScreen { get; set; }
        public string? PauseAutoHotkeyScript { get; set; }
        public string? ResumeAutoHotkeyScript { get; set; }
        public bool DefaultPauseSettingsPushed { get; set; }
        public bool SuspendProcessOnPause { get; set; }
        public bool ForcefulPauseScreenActivation { get; set; }

        // State Management Scripts
        public string? LoadStateAutoHotkeyScript { get; set; }
        public string? SaveStateAutoHotkeyScript { get; set; }
        public string? ResetAutoHotkeyScript { get; set; }
        public string? SwapDiscsAutoHotkeyScript { get; set; }

        /// <summary>
        /// Preserves any XML elements not explicitly mapped to properties.
        /// Ensures round-trip fidelity with LaunchBox Emulators.xml.
        /// </summary>
        [XmlAnyElement]
        public XmlElement[] UnknownElements { get; set; } = Array.Empty<XmlElement>();
    }
}
