using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents LaunchBox Desktop application settings.
    /// Maps directly to ALL XML elements in Settings.xml (285 properties).
    /// 100% compatible with LaunchBox format.
    /// </summary>
    [XmlRoot("Settings")]
    public class Settings
    {
        // ── System Identity ──────────────────────────────────────────────
        public string ID { get; set; } = string.Empty;
        public string? NoWelcomeVersion { get; set; }
        public string? Language { get; set; }

        // ── Window Settings ──────────────────────────────────────────────
        public int FormLocationX { get; set; }
        public int FormLocationY { get; set; }
        public int FormSizeX { get; set; }
        public int FormSizeY { get; set; }
        public bool FormMaximized { get; set; }
        public bool UseDefaultWindowsBorder { get; set; }
        public bool UseDefaultWindowsStyles { get; set; }

        // ── Layout ───────────────────────────────────────────────────────
        public int FiltersWidth { get; set; }
        public int SideBarSize { get; set; }
        public bool ShowSideBar { get; set; }
        public bool ShowFilters { get; set; }
        public bool ScrollBar { get; set; }
        public bool ButtonBar { get; set; }
        public string? SideBarField { get; set; }
        public string? SideBarValueId { get; set; }
        public bool ListView { get; set; }
        public int ControlsBarHeight { get; set; }

        // ── Background & Display ─────────────────────────────────────────
        public bool BackgroundWallpaper { get; set; }
        public bool BackgroundBoxArt { get; set; }
        public int BackgroundFade { get; set; }
        public string? BackgroundImage { get; set; }
        public bool AlwaysUseDefaultBackground { get; set; }
        public bool AlwaysShowImagesFullscreen { get; set; }
        public bool ShowIconsOnHover { get; set; }
        public bool ShowVersions { get; set; }
        public bool ShowCommands { get; set; }
        public int BackgroundBlurAmount { get; set; }
        public bool ShowTextOnAllGames { get; set; }
        public bool ShowHiddenGames { get; set; }
        public bool ShowBrokenGames { get; set; }

        // ── Color Theme (ARGB integers) ──────────────────────────────────
        public int DarkBackgroundColor { get; set; }
        public int LightBackgroundColor { get; set; }
        public int ForegroundColor { get; set; }
        public int SelectedBackgroundColor { get; set; }
        public int HighlightedBackgroundColor { get; set; }
        public int HighlightedBorderColor { get; set; }
        public int WindowBorderColor { get; set; }
        public int MenuBorderColor { get; set; }
        public bool ColorizeGameDividers { get; set; }
        public bool ColorizeScrollBar { get; set; }
        public bool ColorizeGameSelections { get; set; }
        public bool ColorizeGameDetailsPopup { get; set; }
        public bool ColorizeBackgroundFade { get; set; }
        public bool ColorizeBoxBackgroundsToMatchImages { get; set; }
        public bool UseRandomColorThemeOnStartup { get; set; }

        // ── Dialog Colors ────────────────────────────────────────────────
        public int DialogAccentColor { get; set; }
        public int DialogHighlightColor { get; set; }
        public int DialogBackgroundColor { get; set; }
        public int DialogBorderColor { get; set; }
        public int DialogForegroundColor { get; set; }
        public double DialogContrastMultiplier { get; set; }

        // ── Game Box Display ─────────────────────────────────────────────
        public double GameAspectRatio { get; set; }
        public int GameSpacingHorizontal { get; set; }
        public int GameSpacingVertical { get; set; }
        public int GamePaddingHorizontal { get; set; }
        public int GamePaddingVertical { get; set; }
        public int GameTextLines { get; set; }
        public int GameTextSpacingVertical { get; set; }
        public bool EnableBoxShadows { get; set; }
        public bool DynamicBoxSizing { get; set; }
        public int BoxBackgroundOpacity { get; set; }
        public int BoxTextOutlineOpacity { get; set; }
        public int BoxTextOutlineThickness { get; set; }
        public bool BoxAlignText { get; set; }
        public bool CenterText { get; set; }
        public bool ShowSubline { get; set; }
        public string? DefaultImageGroup { get; set; }

        // ── Next Box Display ─────────────────────────────────────────────
        public double NextBoxSize { get; set; }
        public double NextBoxAspectRatio { get; set; }
        public int NextBoxSpacingHorizontal { get; set; }
        public int NextBoxSpacingVertical { get; set; }
        public int NextBoxPaddingHorizontal { get; set; }
        public int NextBoxPaddingVertical { get; set; }
        public int NextBoxTextLines { get; set; }
        public int NextBoxTextSpacingVertical { get; set; }
        public int VolumeNext { get; set; }

        // ── Fonts ────────────────────────────────────────────────────────
        public string? GameFont { get; set; }
        public string? SidebarFont { get; set; }
        public string? GameDetailsLargeFont { get; set; }
        public string? GameDetailsSmallFont { get; set; }
        public string? GameDetailsTitleFont { get; set; }
        public bool FontManuallySet { get; set; }

        // ── Sidebar Options ──────────────────────────────────────────────
        public bool HideSidebarNone { get; set; }
        public bool HideSidebarExists { get; set; }
        public bool HideSidebarAll { get; set; }
        public bool ShowSidebarCounts { get; set; }
        public bool AlignSidebarCountsRight { get; set; }
        public string? SidebarNoneText { get; set; }
        public string? SidebarExistsText { get; set; }
        public string? SidebarAllText { get; set; }
        public bool ShowSidebarScreenshots { get; set; }
        public bool ShowSidebarFanart { get; set; }
        public bool ShowSidebarIcons { get; set; }

        // ── Sorting ──────────────────────────────────────────────────────
        public string? SortBy { get; set; }
        public bool SortByDesc { get; set; }

        // ── List View Configuration ──────────────────────────────────────
        public string? ListViewOrderedColumnPriorities { get; set; }
        public string? ListViewVisibleColumnIndexPriorities { get; set; }

        // ── Game Detail Display ──────────────────────────────────────────
        public bool ShowDetailsVideo { get; set; }
        public bool AutoPlayDetailsVideo { get; set; }
        public bool ShowDetailAchievements { get; set; }
        public bool ShowDetails3dModel { get; set; }
        public bool ShowDetailPlatform { get; set; }
        public bool ShowDetailReleaseDate { get; set; }
        public bool ShowDetailDeveloper { get; set; }
        public bool ShowDetailPublisher { get; set; }
        public bool ShowDetailRating { get; set; }
        public bool ShowDetailGenres { get; set; }
        public bool ShowDetailSeries { get; set; }
        public bool ShowDetailPlayMode { get; set; }
        public bool ShowDetailRegion { get; set; }
        public bool ShowDetailStatus { get; set; }
        public bool ShowDetailSource { get; set; }
        public bool ShowDetailPortable { get; set; }
        public bool ShowDetailInstalled { get; set; }
        public bool ShowDetailFileName { get; set; }
        public bool ShowDetailCustomFields { get; set; }
        public bool ShowDetailLastPlayed { get; set; }
        public bool ShowDetailPlayCount { get; set; }
        public bool ShowDetailPlayTime { get; set; }
        public bool ShowDetailStarRating { get; set; }
        public bool ShowDetailNotes { get; set; }
        public bool ShowDetailDates { get; set; }
        public bool ShowDetailReleaseType { get; set; }
        public bool ShowDetailMaxPlayer { get; set; }
        public bool ShowDetailVideoUrl { get; set; }
        public bool ShowDetailWikipediaUrl { get; set; }
        public bool ShowDetailMameAllTimeCommunityLeaderboard { get; set; }
        public bool ShowDetailMameYearlyCommunityLeaderboard { get; set; }
        public bool ShowDetailMameMonthlyCommunityLeaderboard { get; set; }
        public bool ShowDetailMameWeeklyCommunityLeaderboard { get; set; }

        // ── Platform Detail Display ──────────────────────────────────────
        public bool ShowPlatformTotalGames { get; set; }
        public bool ShowPlatformDefaultEmulator { get; set; }
        public bool ShowPlatformGamesCompleted { get; set; }
        public bool ShowPlatformLastPlayedGame { get; set; }
        public bool ShowPlatformLastPlayedDate { get; set; }
        public bool ShowPlatformPlayCount { get; set; }
        public bool ShowPlatformMostPlayedGame { get; set; }
        public bool ShowPlatformReleaseDate { get; set; }
        public bool ShowPlatformDeveloper { get; set; }
        public bool ShowPlatformManufacturer { get; set; }
        public bool ShowPlatformCpu { get; set; }
        public bool ShowPlatformMemory { get; set; }
        public bool ShowPlatformGraphics { get; set; }
        public bool ShowPlatformSound { get; set; }
        public bool ShowPlatformDisplay { get; set; }
        public bool ShowPlatformMedia { get; set; }
        public bool ShowPlatformMaxControllers { get; set; }
        public bool ShowPlatformNotes { get; set; }
        public bool ShowPlatformBanner { get; set; }
        public bool ShowPlatformDevice { get; set; }
        public bool ShowPlatformFanart { get; set; }
        public bool ShowPlatformVideo { get; set; }
        public bool ShowPlatformPlaytime { get; set; }

        // ── Game Filters ─────────────────────────────────────────────────
        public bool HideGamesMissingVideos { get; set; }
        public bool HideGamesMissingBoxFrontImage { get; set; }
        public bool HideGamesMissingScreenshotImage { get; set; }
        public bool HideGamesMissingClearLogoImage { get; set; }
        public bool HideGamesMissingBackgroundImage { get; set; }

        // ── Image Type Priorities ────────────────────────────────────────
        public string? FrontImageTypePriorities { get; set; }
        public string? BackImageTypePriorities { get; set; }
        public string? BackgroundImageTypePriorities { get; set; }
        public string? ScreenshotsImageTypePriorities { get; set; }
        public string? MarqueeImageTypePriorities { get; set; }
        public string? Box3dImageTypePriorities { get; set; }
        public string? CartFrontImageTypePriorities { get; set; }
        public string? CartBackImageTypePriorities { get; set; }
        public string? Cart3dImageTypePriorities { get; set; }

        // ── Video ────────────────────────────────────────────────────────
        public string? VideoPlaybackEngine { get; set; }
        public string? VideoTypePriorities { get; set; }
        public bool VideoCheck { get; set; }

        // ── Features ─────────────────────────────────────────────────────
        public bool AutoPlayMusic { get; set; }
        public bool ShuffleMusic { get; set; }
        public bool AutoBackup { get; set; }
        public bool CheckForUpdates { get; set; }
        public bool BetaUpgrades { get; set; }
        public bool EnableGamepad { get; set; }
        public bool EnableAutomatedImports { get; set; }
        public bool EnableLocalDBSearch { get; set; }
        public bool PauseBeforeCommands { get; set; }
        public bool PauseBeforeExit { get; set; }
        public bool ExitDosBox { get; set; }
        public bool MinimizeOnGameLaunch { get; set; }
        public bool RestoreOnGameExit { get; set; }
        public bool AllowDeletingRoms { get; set; }
        public bool DebugLog { get; set; }
        public bool BackgroundUpdateDownloads { get; set; }
        public bool UseDeferredScrolling { get; set; }

        // ── Startup & Pause Screen ───────────────────────────────────────
        public bool UseStartupScreen { get; set; }
        public bool ShowLaunchBoxSplashScreen { get; set; }
        public string? StartupTheme { get; set; }
        public int MinimumStartupScreenDisplayTime { get; set; }
        public int MinimumShutdownScreenDisplayTime { get; set; }
        public bool HideMouseCursorOnStartupScreens { get; set; }
        public bool UsePauseScreen { get; set; }
        public bool PauseScreenFading { get; set; }
        public bool PauseScreenMuting { get; set; }
        public string? PauseTheme { get; set; }
        public string? Theme { get; set; }
        public int KeyboardGamePause { get; set; }
        public int KeyboardScreenshot { get; set; }
        public int MaxComboBoxItemCount { get; set; }

        // ── Gamepad Controller Buttons ───────────────────────────────────
        public string? GamepadId { get; set; }
        public string? GamepadName { get; set; }
        public int ControllerSelectButton { get; set; }
        public int ControllerContextMenuButton { get; set; }
        public int ControllerPlayButton { get; set; }
        public int ControllerPlayMusicButton { get; set; }
        public int ControllerTopMenuButton { get; set; }
        public int ControllerImagesButton { get; set; }
        public int ControllerSideBarButton { get; set; }
        public int ControllerGameDetailsButton { get; set; }
        public int ControllerFullscreenButton { get; set; }
        public int ControllerZoomInButton { get; set; }
        public int ControllerZoomOutButton { get; set; }
        public int ControllerPageUpButton { get; set; }
        public int ControllerPageDownButton { get; set; }
        public int ControllerFlipBoxButton { get; set; }
        public int ControllerZScroll { get; set; }
        public bool UseAllControllers { get; set; }
        public bool UseControllerAutomation { get; set; }
        public int ControllerAutomationHoldButton { get; set; }
        public int ControllerAutomationCloseButton { get; set; }
        public int ControllerAutomationStartButton { get; set; }
        public int ControllerAutomationPauseButton { get; set; }
        public int ControllerAutomationVolumeUpButton { get; set; }
        public int ControllerAutomationVolumeDownButton { get; set; }

        // ── Online Services & Authentication ─────────────────────────────
        public string? EmuMoviesUserId { get; set; }
        public string? EmuMoviesPassword { get; set; }
        public string? RetroAchievementsUsername { get; set; }
        public string? RetroAchievementsApiKey { get; set; }
        public string? CloudAuthenticationToken { get; set; }
        public bool UploadStarRatings { get; set; }
        public bool ConsiderCommunityStarRatings { get; set; }
        public int MinimumCommunityRatingCountBeforeConsidering { get; set; }
        public bool DownloadMameCommunityHighScores { get; set; }
        public bool UploadMameCommunityHighScores { get; set; }
        public bool DisableCheevoWarnings { get; set; }
        public int LastRaHasherVersion { get; set; }

        // ── Metadata & Updates ───────────────────────────────────────────
        public string? LastMetadataUpdate { get; set; }
        public string? LastMetadataVersion { get; set; }
        public bool HasAppendedSteamImageTypes { get; set; }
        public bool HasAppendedGogImageTypes { get; set; }
        public bool HasAppendedEpicGamesImageTypes { get; set; }
        public bool HasAppendedUplayImageTypes { get; set; }
        public bool HasAppendedOriginImageTypes { get; set; }
        public bool HasAppendedAmazonImageTypes { get; set; }
        public bool HasAppendedAmazonPoster { get; set; }

        // ── Import Defaults ──────────────────────────────────────────────
        public bool ImportDefaultRemoveRoughMatches { get; set; }
        public bool ImportDefaultSearchGamesDatabase { get; set; }
        public bool ImportDefaultFilesFromSpecifiedFoldersOnly { get; set; }
        public bool ImportDefaultAlsoCopyMoveAllFiles { get; set; }
        public bool ImportDefaultLookForPdfFiles { get; set; }
        public bool ImportDefaultCombineRoms { get; set; }
        public bool ImportDefaultRenameGameFolders { get; set; }
        public bool ImportDefaultLookForDosBoxConfFiles { get; set; }
        public bool ImportDefaultLookForGameSetupFiles { get; set; }
        public bool ImportDefaultAutomaticallyMount { get; set; }
        public bool ImportDefaultCinematicThemeVideo { get; set; }
        public bool ImportDefaultSteamTrailerVideo { get; set; }
        public int ImportDefaultMediaLimit { get; set; }

        // ── Store Integration ────────────────────────────────────────────
        public bool GogLaunchWithClient { get; set; }
        public bool OriginLaunchWithClient { get; set; }

        // ── Cloud Sync ───────────────────────────────────────────────────
        public bool EnableGameCloudSync { get; set; }
        public string? LastCloudSync { get; set; }

        // ── System Tray ──────────────────────────────────────────────────
        public bool AlwaysShowSystemTrayIcon { get; set; }
        public bool MinimizeToSystemTray { get; set; }
        public bool CloseToSystemTray { get; set; }
        public bool DontSendTrayReminder { get; set; }

        // ── Badges ───────────────────────────────────────────────────────
        public bool ShowBadges { get; set; }
        public string? EnabledBadges { get; set; }

        // ── OBS & Recording ──────────────────────────────────────────────
        public bool AutoAddObsRecordings { get; set; }
        public bool StartObsWithGames { get; set; }

        // ── LEDBlinky ────────────────────────────────────────────────────
        public bool EnableLedBlinky { get; set; }
        public string? LedBlinkyPath { get; set; }
        public bool LedBlinkyUseAdvanced { get; set; }

        // ── Region & Notification ────────────────────────────────────────
        public string? RegionPriorities { get; set; }
        public int NotificationType { get; set; }

        // ── Similar/Recommended Games ────────────────────────────────────
        public string? SimilarGamesXmlString { get; set; }
        public string? RecommendedGamesXmlString { get; set; }

        // ── Migration Flags ──────────────────────────────────────────────
        public bool ImagesReorganizedFor5 { get; set; }
        public bool ImageFoldersRenamedFor63 { get; set; }
        public bool RotateModelDefaultsApplied { get; set; }
    }
}
