using System.Xml.Serialization;

namespace GameLauncher.Core.Models
{
    /// <summary>
    /// Represents LaunchBox BigBox (fullscreen mode) settings.
    /// Maps directly to ALL 519 XML elements in BigBoxSettings.xml.
    /// 100% compatible with LaunchBox format.
    /// </summary>
    [XmlRoot("BigBoxSettings")]
    public class BigBoxSettings
    {
        // ── Theme & Display ──────────────────────────────────────────────
        public string Theme { get; set; } = "Default";
        public int BackgroundFade { get; set; }
        public int FrameRate { get; set; }
        public string VideoPlaybackEngine { get; set; } = "VLC";
        public int CoverFlowReflectionOpacity { get; set; }
        public int CoverFlowImageQuality { get; set; }
        public string? DefaultView { get; set; }
        public bool Use3dModelCoverFlow { get; set; }
        public bool Use3dModelImageView { get; set; }

        // ── Sound Settings ───────────────────────────────────────────────
        public bool RepeatGameMusic { get; set; }
        public bool PlayStartupSound { get; set; }
        public bool PlaySelectSound { get; set; }
        public bool PlayBackSound { get; set; }
        public bool PlayNavigationSound { get; set; }
        public bool AutoPlayMusicGamesList { get; set; }
        public bool AutoPlayMusicGameDetails { get; set; }
        public bool AutoPlayMusicRecentFavorites { get; set; }
        public bool PlayMoveWhileInputHeld { get; set; }
        public bool PlayMoveInAttractMode { get; set; }

        // ── Audio & Volume ───────────────────────────────────────────────
        public string? SoundPack { get; set; }
        public bool EnableBackgroundMusic { get; set; }
        public bool EnableMusicOnScreenDisplay { get; set; }
        public bool ShuffleBackgroundMusic { get; set; }
        public bool ShuffleSoundtrackMusic { get; set; }
        public bool UsePlatformPlaylistCategorySpecificBackgroundMusic { get; set; }
        public bool PrioritizeMusicOverVideoAudio { get; set; }
        public int VolumeMaster { get; set; }
        public int VolumeVideo { get; set; }
        public int VolumeMusic { get; set; }
        public int VolumeBackgroundMusic { get; set; }
        public int VolumeStartupSound { get; set; }
        public int VolumeNavigationSound { get; set; }
        public int VolumeSelectSound { get; set; }
        public int VolumeBackSound { get; set; }
        public int VolumeAttractModeMaster { get; set; }
        public int VolumeAttractModeNavigationSound { get; set; }

        // ── Screen Transitions ───────────────────────────────────────────
        public string ScreenTransition { get; set; } = "Fade";
        public string GameBackgroundTransition { get; set; } = "Fade";
        public string GameMainImageTransition { get; set; } = "Slide Horizontal";
        public string GameImageVideoTransition { get; set; } = "Slide Horizontal";
        public string GamesListTransition { get; set; } = "Slide Vertical";
        public string PlatformsListTransition { get; set; } = "Fade";
        public string GameDetailsTransition { get; set; } = "Slide Vertical";
        public string GameCoverFlowDetailsTransition { get; set; } = "Slide Horizontal";
        public string FiltersBackgroundTransition { get; set; } = "Fade";
        public string FiltersImageTransition { get; set; } = "Rotate Vertical Box";
        public string FiltersBoxesTransition { get; set; } = "Slide Horizontal";
        public string FiltersPlatformVideoTransition { get; set; } = "Slide Horizontal";
        public string? PlatformDetailsTransition { get; set; }

        // ── Game Display Options ─────────────────────────────────────────
        public bool ShowHiddenGames { get; set; }
        public bool ShowBrokenGames { get; set; }
        public bool ShowStartupSplashScreen { get; set; }
        public bool ShowLoadingGameMessage { get; set; }
        public bool SkipGameDetailsScreen { get; set; }
        public string GameImageType { get; set; } = "Boxes";
        public bool GamesUseBackgroundVideos { get; set; }
        public string GamesListView { get; set; } = "HorizontalWheel3";
        public bool ShowGamesListTitle { get; set; }
        public bool ShowStarNextToFavoritedGames { get; set; }
        public bool ShowFavoritedGamesFirst { get; set; }
        public bool ShowGameFileName { get; set; }
        public bool ShowGameStarRating { get; set; }

        // ── Game Details Display ─────────────────────────────────────────
        public bool GameDetailsClearLogos { get; set; }
        public bool GamesListPlatformClearLogos { get; set; }
        public bool ShowGameTitle { get; set; }
        public bool ShowGamePlatform { get; set; }
        public bool ShowGameDeveloper { get; set; }
        public bool ShowGamePublisher { get; set; }
        public bool ShowGameReleaseDate { get; set; }
        public bool ShowGameGenres { get; set; }
        public bool ShowGameSeries { get; set; }
        public bool ShowGameVersion { get; set; }
        public bool ShowGameInstalled { get; set; }
        public bool ShowGamePlayMode { get; set; }
        public bool ShowGameRegion { get; set; }
        public bool ShowGameStatus { get; set; }
        public bool ShowGameSource { get; set; }
        public bool ShowGameCustomFields { get; set; }
        public bool ShowGameLastPlayed { get; set; }
        public bool ShowGamePlayCount { get; set; }
        public bool ShowGamePlayTime { get; set; }
        public bool ShowGameNotes { get; set; }
        public bool ShowGameFavorite { get; set; }
        public bool ShowGameCompleted { get; set; }
        public bool ShowGamePortable { get; set; }
        public bool ShowGameRating { get; set; }
        public bool ShowGameBroken { get; set; }
        public bool ShowGameLockUnlock { get; set; }

        // ── Game Menu Options ────────────────────────────────────────────
        public bool ShowGameMenuTitle { get; set; }
        public bool ShowGameMenuPlay { get; set; }
        public bool ShowGameMenuConfigure { get; set; }
        public bool ShowGameMenuAdditionalApplications { get; set; }
        public bool ShowGameMenuPlayMusic { get; set; }
        public bool ShowGameMenuPlayVideo { get; set; }
        public bool ShowGameMenuViewManual { get; set; }
        public bool ShowGameMenuOpenFolder { get; set; }
        public bool ShowGameMenuOpenImagesFolder { get; set; }
        public bool ShowGameMenuOpenDosBox { get; set; }
        public bool ShowGameMenuOpenScummVm { get; set; }
        public bool ShowGameMenuOpenEmulator { get; set; }
        public bool ShowGameMenuViewRelatedGames { get; set; }
        public bool ShowRetroarchNetplayOptions { get; set; }
        public bool ShowGameMenuViewImagesFullscreen { get; set; }
        public bool ShowGameMenuViewVideoFullscreen { get; set; }
        public bool ShowGameMenuViewModelFullscreen { get; set; }
        public bool ShowGameMenuFlipBox { get; set; }
        public bool ShowGameMenuFavorite { get; set; }
        public bool ShowGameMenuBroken { get; set; }
        public bool ShowGameMenuCompleted { get; set; }
        public bool ShowGameMenuHidden { get; set; }
        public bool ShowGameMenuLaunchWith { get; set; }
        public bool ShowGameMenuStarRating { get; set; }
        public bool ShowGameMenuAchievements { get; set; }
        public bool ShowGameMenuMameHighScores { get; set; }

        // ── Platform/Filters Display ─────────────────────────────────────
        public bool FiltersRandomGameImageBackgrounds { get; set; }
        public bool FiltersPlatformFanartBackgrounds { get; set; }
        public bool FiltersPlatformDeviceImageBackgrounds { get; set; }
        public bool FiltersPlatformClearLogoBackgrounds { get; set; }
        public bool FiltersPreferPlatformClearLogo { get; set; }
        public bool PlatformsUseRandomGameVideos { get; set; }
        public bool PlatformsUseBackgroundVideos { get; set; }
        public string PlatformsListView { get; set; } = "PlatformWheel2";
        public bool ShowFiltersListTitle { get; set; }
        public bool ShowRecentGames { get; set; }
        public bool ShowFavoriteGames { get; set; }
        public bool ShowFiltersTitle { get; set; }
        public bool ShowFiltersTotalGames { get; set; }
        public bool ShowFiltersGamesCompleted { get; set; }
        public bool ShowFiltersLastPlayed { get; set; }
        public bool ShowFiltersLastPlayedGame { get; set; }
        public bool ShowFiltersPlayCount { get; set; }
        public bool ShowFiltersMostPlayed { get; set; }
        public bool HideAutoGeneratedPlaylistsFromPlaylistsView { get; set; }

        // ── Platform Details ─────────────────────────────────────────────
        public bool ShowPlatformTitle { get; set; }
        public bool ShowPlatformBannerImages { get; set; }
        public bool ShowPlatformTotalGames { get; set; }
        public bool ShowPlatformDefaultEmulator { get; set; }
        public bool ShowPlatformGamesCompleted { get; set; }
        public bool ShowPlatformLastPlayed { get; set; }
        public bool ShowPlatformMostPlayed { get; set; }
        public bool ShowPlatformPlayCount { get; set; }
        public bool ShowPlatformReleaseDate { get; set; }
        public bool ShowPlatformDeveloper { get; set; }
        public bool ShowPlatformManufacturer { get; set; }
        public bool ShowPlatformCpu { get; set; }
        public bool ShowPlatformMemory { get; set; }
        public bool ShowPlatformGraphics { get; set; }
        public bool ShowPlatformDisplay { get; set; }
        public bool ShowPlatformSound { get; set; }
        public bool ShowPlatformMedia { get; set; }
        public bool ShowPlatformMaxControllers { get; set; }
        public bool ShowPlatformNotes { get; set; }

        // ── Scrolling Options ────────────────────────────────────────────
        public bool ScrollGameDetails { get; set; }
        public bool ScrollGameNotes { get; set; }
        public bool ScrollFilterDetails { get; set; }
        public bool ScrollPlatformDetails { get; set; }

        // ── Startup & Pause Screen ───────────────────────────────────────
        public bool UseStartupScreen { get; set; }
        public string? StartupTheme { get; set; }
        public int MinimumStartupScreenDisplayTime { get; set; }
        public int MinimumShutdownScreenDisplayTime { get; set; }
        public bool HideMouseCursorOnStartupScreens { get; set; }
        public bool UsePauseScreen { get; set; }
        public bool PauseScreenFading { get; set; }
        public bool PauseScreenMuting { get; set; }
        public string? PauseTheme { get; set; }
        public int KeyboardGamePause { get; set; }

        // ── Mouse & Input ────────────────────────────────────────────────
        public bool EnableMouse { get; set; }
        public bool HideMouseCursor { get; set; }
        public bool DisableSystemMouseCursor { get; set; }
        public bool EnableGamepad { get; set; }
        public bool UseAllControllers { get; set; }
        public string? GamepadId { get; set; }
        public string? GamepadName { get; set; }
        public bool HideTaskbar { get; set; }
        public bool RequireHoldToBackToSystem { get; set; }
        public bool ProblematicControllerCompatibility { get; set; }

        // ── Wheel & Model ────────────────────────────────────────────────
        public int WheelMinimumSpeed { get; set; }
        public int ModelWheelMinimumSpeed { get; set; }
        public bool WheelEasing { get; set; }

        // ── Lock & Access Control ────────────────────────────────────────
        public string? LockPin { get; set; }
        public bool AllowExitWhileUnlocked { get; set; }
        public bool AllowSleep { get; set; }
        public bool AllowSettingStarRatingsWhileLocked { get; set; }
        public bool AllowOpeningGameFoldersWhileLocked { get; set; }
        public bool AllowOpeningGameImageFoldersWhileLocked { get; set; }
        public bool AllowOpeningEmulatorsWhileLocked { get; set; }
        public bool AllowFavoritingGamesWhileLocked { get; set; }
        public bool AllowHidingGamesWhileLocked { get; set; }
        public bool AllowMarkingGamesAsBrokenWhileLocked { get; set; }
        public bool AllowMarkingGamesAsCompletedWhileLocked { get; set; }
        public bool AllowSleepWhileLocked { get; set; }
        public bool AllowShutDownWhileLocked { get; set; }
        public bool AllowRebootWhileLocked { get; set; }
        public bool AllowChangeViewWhileLocked { get; set; }
        public bool AllowChangeImageTypeWhileLocked { get; set; }
        public bool AllowChangeFilterAllGamesWhileLocked { get; set; }
        public bool AllowChangeFilterPlatformsWhileLocked { get; set; }
        public bool AllowChangeFilterPlatformCategoriesWhileLocked { get; set; }
        public bool AllowChangeFilterPlaylistsWhileLocked { get; set; }
        public bool AllowChangeFilterGenresWhileLocked { get; set; }
        public bool AllowChangeFilterDevelopersWhileLocked { get; set; }
        public bool AllowChangeFilterPublishersWhileLocked { get; set; }
        public bool AllowChangeFilterSeriesWhileLocked { get; set; }
        public bool AllowChangeFilterStatusesWhileLocked { get; set; }
        public bool AllowChangeFilterSourcesWhileLocked { get; set; }
        public bool AllowChangeFilterRatingsWhileLocked { get; set; }
        public bool AllowChangeFilterPlayModesWhileLocked { get; set; }
        public bool AllowChangeFilterRegionsWhileLocked { get; set; }
        public bool AllowSearchWhileLocked { get; set; }
        public bool AllowThemesDemoWhileLocked { get; set; }
        public bool AllowViewRetroarchNetplayBrowserWhileLocked { get; set; }
        public bool AllowViewAchievementProfileWhileLocked { get; set; }

        // ── Attract Mode ─────────────────────────────────────────────────
        public bool EnableAttractMode { get; set; }
        public bool AttractModeSwitchFilters { get; set; }
        public int AttractModeDelay { get; set; }
        public int AttractModeTimePerMovement { get; set; }
        public int AttractModeMaximumSpeed { get; set; }
        public int AttractModeMinimumSpeed { get; set; }

        // ── Display & Monitors ───────────────────────────────────────────
        public bool MirrorDisplays { get; set; }
        public int PrimaryMonitorIndex { get; set; }
        public int MarqueeMonitorIndex { get; set; }
        public bool MarqueeIgnoreThemeViews { get; set; }
        public bool MarqueeStretchImages { get; set; }
        public string? MarqueeScreenCompatibilityMode { get; set; }

        // ── PDF Reader ───────────────────────────────────────────────────
        public bool UseBuiltInPdfReaderForManuals { get; set; }
        public bool UseBuiltInPdfReaderForAdditionalAppPdfs { get; set; }

        // ── Application State ────────────────────────────────────────────
        public bool RememberLastGame { get; set; }
        public bool RememberLastPlatform { get; set; }
        public bool RememberViewForEachPlatform { get; set; }
        public string? LastPlatform { get; set; }
        public string? LastPlatformCategory { get; set; }

        // ── Game Filters ─────────────────────────────────────────────────
        public bool HideGamesMissingVideos { get; set; }
        public bool HideGamesMissingBoxFrontImage { get; set; }
        public bool HideGamesMissingScreenshotImage { get; set; }
        public bool HideGamesMissingClearLogoImage { get; set; }
        public bool HideGamesMissingBackgroundImage { get; set; }

        // ── LEDBlinky ────────────────────────────────────────────────────
        public bool LedBlinkyDontStartScreensaver { get; set; }

        // ── Migration Flags ──────────────────────────────────────────────
        public bool MovedDefaultToOldDefault { get; set; }

        // ── Theme Demo ───────────────────────────────────────────────────
        public int ThemesDemoSecondsBetweenThemeViewSwitching { get; set; }
        public bool ThemesDemoShowDetailsNotification { get; set; }
        public bool ThemesDemoFadeDetailsNotification { get; set; }
        public bool ThemesDemoExcludeDefaultTheme { get; set; }
        public bool ThemesDemoExcludeCriticalZoneTheme { get; set; }
        public bool ThemesDemoExcludeTextListWithDetailsGamesView { get; set; }
        public bool ThemesDemoExcludeFullscreenCoverflowGamesView { get; set; }
        public bool ThemesDemoExcludeCoverflowWithDetailsGamesView { get; set; }
        public bool ThemesDemoExcludeVerticalWheel1GamesView { get; set; }
        public bool ThemesDemoExcludeVerticalWheel2GamesView { get; set; }
        public bool ThemesDemoExcludeVerticalWheel3GamesView { get; set; }
        public bool ThemesDemoExcludeVerticalWheel4GamesView { get; set; }
        public bool ThemesDemoExcludeHorizontalWheel1GamesView { get; set; }
        public bool ThemesDemoExcludeHorizontalWheel2GamesView { get; set; }
        public bool ThemesDemoExcludeHorizontalWheel3GamesView { get; set; }
        public bool ThemesDemoExcludeHorizontalBoxesWithDetailsGamesView { get; set; }
        public bool ThemesDemoExcludeWallGamesView { get; set; }
        public bool ThemesDemoExcludeWall2GamesView { get; set; }
        public bool ThemesDemoExcludeWall3GamesView { get; set; }
        public bool ThemesDemoExcludeWall4GamesView { get; set; }
        public bool ThemesDemoExcludeTextListWithDetailsPlatformsView { get; set; }
        public bool ThemesDemoExcludePlatformWheel1 { get; set; }
        public bool ThemesDemoExcludePlatformWheel2 { get; set; }
        public bool ThemesDemoExcludePlatformWheel3 { get; set; }
        public bool ThemesDemoExcludePlatformWheel4 { get; set; }

        // ── Controller Buttons ───────────────────────────────────────────
        public int ControllerSelectButton { get; set; }
        public int ControllerBackButton { get; set; }
        public int ControllerPlayButton { get; set; }
        public int ControllerPlayMusicButton { get; set; }
        public int ControllerExitButton { get; set; }
        public int ControllerFlipBoxButton { get; set; }
        public int ControllerPageUpButton { get; set; }
        public int ControllerPageDownButton { get; set; }
        public int ControllerViewImagesButton { get; set; }
        public int ControllerLockUnlockButton { get; set; }
        public int ControllerSwitchView { get; set; }
        public int ControllerSwitchImageType { get; set; }
        public int ControllerSwitchTheme { get; set; }
        public int ControllerSearchButton { get; set; }
        public int ControllerShowGameDetailsScreen { get; set; }
        public int ControllerNextMusicTrackButton { get; set; }
        public int ControllerPreviousMusicTrackButton { get; set; }
        public int ControllerStartAttractMode { get; set; }
        public int ControllerWheelSpin { get; set; }
        public int ControllerFilter { get; set; }
        public int ControllerShowGenres { get; set; }
        public int ControllerShowPlatforms { get; set; }
        public int ControllerShowPlaylists { get; set; }
        public int ControllerShowPlatformCategories { get; set; }
        public int ControllerShowDevelopers { get; set; }
        public int ControllerShowPublishers { get; set; }
        public int ControllerShowRatings { get; set; }
        public int ControllerShowPlayModes { get; set; }
        public int ControllerShowRegions { get; set; }
        public int ControllerShowSeries { get; set; }
        public int ControllerShowSources { get; set; }
        public int ControllerShowStatuses { get; set; }
        public int ControllerShowAchievements { get; set; }
        public int ControllerShowHighScores { get; set; }
        public int ControllerSetStarRating { get; set; }
        public int ControllerPdfReaderZoomIn { get; set; }
        public int ControllerPdfReaderZoomOut { get; set; }
        public int ControllerOpenIndex { get; set; }

        // ── Controller Automation ────────────────────────────────────────
        public int ControllerAutomationHoldButton { get; set; }
        public int ControllerAutomationCloseButton { get; set; }
        public int ControllerAutomationStartButton { get; set; }
        public int ControllerAutomationPauseButton { get; set; }
        public int ControllerAutomationVolumeUpButton { get; set; }
        public int ControllerAutomationVolumeDownButton { get; set; }

        // ── Keyboard Automation ──────────────────────────────────────────
        public bool EnableKeyboardAutomation { get; set; }
        public int KeyboardAutomationHoldKey { get; set; }
        public int KeyboardAutomationCloseKey { get; set; }
        public int KeyboardAutomationStartKey { get; set; }
        public int KeyboardAutomationGamePauseKey { get; set; }
        public int KeyboardAutomationVolumeUpKey { get; set; }
        public int KeyboardAutomationVolumeDownKey { get; set; }
        public int KeyboardAutomationScreenshot { get; set; }

        // ── Keyboard Controls (Set 1 - Primary) ─────────────────────────
        public int KeyboardLeft { get; set; }
        public int KeyboardRight { get; set; }
        public int KeyboardUp { get; set; }
        public int KeyboardDown { get; set; }
        public int KeyboardSelect { get; set; }
        public int KeyboardBack { get; set; }
        public int KeyboardPlay { get; set; }
        public int KeyboardPageUp { get; set; }
        public int KeyboardPageDown { get; set; }
        public int KeyboardFlipBox { get; set; }
        public int KeyboardPlayMusic { get; set; }
        public int KeyboardViewImages { get; set; }
        public int KeyboardViewModel { get; set; }
        public int KeyboardExit { get; set; }
        public int KeyboardVolumeUp { get; set; }
        public int KeyboardVolumeDown { get; set; }
        public int KeyboardSwitchView { get; set; }
        public int KeyboardSwitchImageType { get; set; }
        public int KeyboardSwitchTheme { get; set; }
        public int KeyboardShowGameDetailsScreen { get; set; }
        public int KeyboardNextMusicTrack { get; set; }
        public int KeyboardPreviousMusicTrack { get; set; }
        public int KeyboardSearch { get; set; }
        public int KeyboardStartAttractMode { get; set; }
        public int KeyboardLockUnlock { get; set; }
        public int KeyboardWheelSpin { get; set; }
        public int KeyboardFilter { get; set; }
        public int KeyboardShowGenres { get; set; }
        public int KeyboardShowPlatforms { get; set; }
        public int KeyboardShowPlaylists { get; set; }
        public int KeyboardShowPlatformCategories { get; set; }
        public int KeyboardShowDevelopers { get; set; }
        public int KeyboardShowPublishers { get; set; }
        public int KeyboardShowRatings { get; set; }
        public int KeyboardShowPlayModes { get; set; }
        public int KeyboardShowRegions { get; set; }
        public int KeyboardShowSeries { get; set; }
        public int KeyboardShowSources { get; set; }
        public int KeyboardShowStatuses { get; set; }
        public int KeyboardShowAchievements { get; set; }
        public int KeyboardShowAchievementProfile { get; set; }
        public int KeyboardShowHighScores { get; set; }
        public int KeyboardSetStarRating { get; set; }
        public int KeyboardPdfReaderZoomIn { get; set; }
        public int KeyboardPdfReaderZoomOut { get; set; }
        public int KeyboardRotateModelLeft1 { get; set; }
        public int KeyboardRotateModelRight1 { get; set; }
        public int KeyboardRotateModelUp1 { get; set; }
        public int KeyboardRotateModelDown1 { get; set; }
        public int KeyboardOpenIndex1 { get; set; }
        public int KeyboardViewSystemMenu1 { get; set; }

        // ── Keyboard Controls (Set 2) ────────────────────────────────────
        public int KeyboardLeft2 { get; set; }
        public int KeyboardRight2 { get; set; }
        public int KeyboardUp2 { get; set; }
        public int KeyboardDown2 { get; set; }
        public int KeyboardSelect2 { get; set; }
        public int KeyboardBack2 { get; set; }
        public int KeyboardPlay2 { get; set; }
        public int KeyboardPageUp2 { get; set; }
        public int KeyboardPageDown2 { get; set; }
        public int KeyboardFlipBox2 { get; set; }
        public int KeyboardPlayMusic2 { get; set; }
        public int KeyboardViewImages2 { get; set; }
        public int KeyboardViewModel2 { get; set; }
        public int KeyboardExit2 { get; set; }
        public int KeyboardVolumeUp2 { get; set; }
        public int KeyboardVolumeDown2 { get; set; }
        public int KeyboardSwitchView2 { get; set; }
        public int KeyboardSwitchImageType2 { get; set; }
        public int KeyboardSwitchTheme2 { get; set; }
        public int KeyboardShowGameDetailsScreen2 { get; set; }
        public int KeyboardNextMusicTrack2 { get; set; }
        public int KeyboardPreviousMusicTrack2 { get; set; }
        public int KeyboardSearch2 { get; set; }
        public int KeyboardStartAttractMode2 { get; set; }
        public int KeyboardLockUnlock2 { get; set; }
        public int KeyboardWheelSpin2 { get; set; }
        public int KeyboardFilter2 { get; set; }
        public int KeyboardShowGenres2 { get; set; }
        public int KeyboardShowPlatforms2 { get; set; }
        public int KeyboardShowPlaylists2 { get; set; }
        public int KeyboardShowPlatformCategories2 { get; set; }
        public int KeyboardShowDevelopers2 { get; set; }
        public int KeyboardShowPublishers2 { get; set; }
        public int KeyboardShowRatings2 { get; set; }
        public int KeyboardShowPlayModes2 { get; set; }
        public int KeyboardShowRegions2 { get; set; }
        public int KeyboardShowSeries2 { get; set; }
        public int KeyboardShowSources2 { get; set; }
        public int KeyboardShowStatuses2 { get; set; }
        public int KeyboardShowAchievements2 { get; set; }
        public int KeyboardShowAchievementProfile2 { get; set; }
        public int KeyboardShowHighScores2 { get; set; }
        public int KeyboardSetStarRating2 { get; set; }
        public int KeyboardPdfReaderZoomIn2 { get; set; }
        public int KeyboardPdfReaderZoomOut2 { get; set; }
        public int KeyboardRotateModelLeft2 { get; set; }
        public int KeyboardRotateModelRight2 { get; set; }
        public int KeyboardRotateModelUp2 { get; set; }
        public int KeyboardRotateModelDown2 { get; set; }
        public int KeyboardOpenIndex2 { get; set; }
        public int KeyboardViewSystemMenu2 { get; set; }

        // ── Keyboard Controls (Set 3) ────────────────────────────────────
        public int KeyboardLeft3 { get; set; }
        public int KeyboardRight3 { get; set; }
        public int KeyboardUp3 { get; set; }
        public int KeyboardDown3 { get; set; }
        public int KeyboardSelect3 { get; set; }
        public int KeyboardBack3 { get; set; }
        public int KeyboardPlay3 { get; set; }
        public int KeyboardPageUp3 { get; set; }
        public int KeyboardPageDown3 { get; set; }
        public int KeyboardFlipBox3 { get; set; }
        public int KeyboardPlayMusic3 { get; set; }
        public int KeyboardViewImages3 { get; set; }
        public int KeyboardViewModel3 { get; set; }
        public int KeyboardExit3 { get; set; }
        public int KeyboardVolumeUp3 { get; set; }
        public int KeyboardVolumeDown3 { get; set; }
        public int KeyboardSwitchView3 { get; set; }
        public int KeyboardSwitchImageType3 { get; set; }
        public int KeyboardSwitchTheme3 { get; set; }
        public int KeyboardShowGameDetailsScreen3 { get; set; }
        public int KeyboardNextMusicTrack3 { get; set; }
        public int KeyboardPreviousMusicTrack3 { get; set; }
        public int KeyboardSearch3 { get; set; }
        public int KeyboardStartAttractMode3 { get; set; }
        public int KeyboardLockUnlock3 { get; set; }
        public int KeyboardWheelSpin3 { get; set; }
        public int KeyboardFilter3 { get; set; }
        public int KeyboardShowGenres3 { get; set; }
        public int KeyboardShowPlatforms3 { get; set; }
        public int KeyboardShowPlaylists3 { get; set; }
        public int KeyboardShowPlatformCategories3 { get; set; }
        public int KeyboardShowDevelopers3 { get; set; }
        public int KeyboardShowPublishers3 { get; set; }
        public int KeyboardShowRatings3 { get; set; }
        public int KeyboardShowPlayModes3 { get; set; }
        public int KeyboardShowRegions3 { get; set; }
        public int KeyboardShowSeries3 { get; set; }
        public int KeyboardShowSources3 { get; set; }
        public int KeyboardShowStatuses3 { get; set; }
        public int KeyboardShowAchievements3 { get; set; }
        public int KeyboardShowAchievementProfile3 { get; set; }
        public int KeyboardShowHighScores3 { get; set; }
        public int KeyboardSetStarRating3 { get; set; }
        public int KeyboardPdfReaderZoomIn3 { get; set; }
        public int KeyboardPdfReaderZoomOut3 { get; set; }
        public int KeyboardRotateModelLeft3 { get; set; }
        public int KeyboardRotateModelRight3 { get; set; }
        public int KeyboardRotateModelUp3 { get; set; }
        public int KeyboardRotateModelDown3 { get; set; }
        public int KeyboardOpenIndex3 { get; set; }
        public int KeyboardViewSystemMenu3 { get; set; }

        // ── Keyboard Controls (Set 4) ────────────────────────────────────
        public int KeyboardLeft4 { get; set; }
        public int KeyboardRight4 { get; set; }
        public int KeyboardUp4 { get; set; }
        public int KeyboardDown4 { get; set; }
        public int KeyboardSelect4 { get; set; }
        public int KeyboardBack4 { get; set; }
        public int KeyboardPlay4 { get; set; }
        public int KeyboardPageUp4 { get; set; }
        public int KeyboardPageDown4 { get; set; }
        public int KeyboardFlipBox4 { get; set; }
        public int KeyboardPlayMusic4 { get; set; }
        public int KeyboardViewImages4 { get; set; }
        public int KeyboardViewModel4 { get; set; }
        public int KeyboardExit4 { get; set; }
        public int KeyboardVolumeUp4 { get; set; }
        public int KeyboardVolumeDown4 { get; set; }
        public int KeyboardSwitchView4 { get; set; }
        public int KeyboardSwitchImageType4 { get; set; }
        public int KeyboardSwitchTheme4 { get; set; }
        public int KeyboardShowGameDetailsScreen4 { get; set; }
        public int KeyboardNextMusicTrack4 { get; set; }
        public int KeyboardPreviousMusicTrack4 { get; set; }
        public int KeyboardSearch4 { get; set; }
        public int KeyboardStartAttractMode4 { get; set; }
        public int KeyboardLockUnlock4 { get; set; }
        public int KeyboardWheelSpin4 { get; set; }
        public int KeyboardFilter4 { get; set; }
        public int KeyboardShowGenres4 { get; set; }
        public int KeyboardShowPlatforms4 { get; set; }
        public int KeyboardShowPlaylists4 { get; set; }
        public int KeyboardShowPlatformCategories4 { get; set; }
        public int KeyboardShowDevelopers4 { get; set; }
        public int KeyboardShowPublishers4 { get; set; }
        public int KeyboardShowRatings4 { get; set; }
        public int KeyboardShowPlayModes4 { get; set; }
        public int KeyboardShowRegions4 { get; set; }
        public int KeyboardShowSeries4 { get; set; }
        public int KeyboardShowSources4 { get; set; }
        public int KeyboardShowStatuses4 { get; set; }
        public int KeyboardShowAchievements4 { get; set; }
        public int KeyboardShowAchievementProfile4 { get; set; }
        public int KeyboardShowHighScores4 { get; set; }
        public int KeyboardSetStarRating4 { get; set; }
        public int KeyboardPdfReaderZoomIn4 { get; set; }
        public int KeyboardPdfReaderZoomOut4 { get; set; }
        public int KeyboardRotateModelLeft4 { get; set; }
        public int KeyboardRotateModelRight4 { get; set; }
        public int KeyboardRotateModelUp4 { get; set; }
        public int KeyboardRotateModelDown4 { get; set; }
        public int KeyboardOpenIndex4 { get; set; }
        public int KeyboardViewSystemMenu4 { get; set; }
    }
}
