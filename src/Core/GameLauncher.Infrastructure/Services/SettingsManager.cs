using System;
using System.Threading.Tasks;
using GameLauncher.Core.Models;
using GameLauncher.Data.Xml;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Manages application settings.
    /// </summary>
    public class SettingsManager : ISettingsManager
    {
        private readonly XmlDataContext _dataContext;

        public SettingsManager(XmlDataContext dataContext)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<Settings> LoadSettingsAsync()
        {
            var settings = _dataContext.LoadSettings();

            // Apply defaults if needed
            if (string.IsNullOrWhiteSpace(settings.ID))
            {
                settings = GetDefaultSettings();
            }

            return await Task.FromResult(settings);
        }

        public async Task SaveSettingsAsync(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            // Ensure ID exists
            if (string.IsNullOrWhiteSpace(settings.ID))
            {
                settings.ID = Guid.NewGuid().ToString();
            }

            _dataContext.SaveSettings(settings);

            await Task.CompletedTask;
        }

        public async Task<BigBoxSettings> LoadBigBoxSettingsAsync()
        {
            var settings = _dataContext.LoadBigBoxSettings();

            // Apply defaults if needed (check both Theme and FrameRate to detect empty settings)
            if (string.IsNullOrWhiteSpace(settings.Theme) || settings.FrameRate == 0)
            {
                settings = GetDefaultBigBoxSettings();
            }

            return await Task.FromResult(settings);
        }

        public async Task SaveBigBoxSettingsAsync(BigBoxSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _dataContext.SaveBigBoxSettings(settings);

            await Task.CompletedTask;
        }

        public async Task<Settings> ResetToDefaultsAsync()
        {
            var defaults = GetDefaultSettings();
            _dataContext.SaveSettings(defaults);
            return await Task.FromResult(defaults);
        }

        #region Default Settings

        private Settings GetDefaultSettings()
        {
            return new Settings
            {
                ID = Guid.NewGuid().ToString(),
                FormSizeX = 1280,
                FormSizeY = 720,
                FormLocationX = 100,
                FormLocationY = 100,
                FormMaximized = false,
                UseDefaultWindowsBorder = false,
                UseDefaultWindowsStyles = false,

                // Layout
                FiltersWidth = 250,
                SideBarSize = 300,
                ShowSideBar = true,
                ShowFilters = true,
                ScrollBar = true,
                ButtonBar = false,
                SideBarField = "Platform",

                // Display
                BackgroundBoxArt = true,
                BackgroundFade = 138,
                AlwaysShowImagesFullscreen = true,
                ShowIconsOnHover = true,

                // Colors (ARGB integers - LaunchBox format)
                // DarkBackground: #FF002851 (dark navy blue)
                DarkBackgroundColor = -16766127,
                // LightBackground: #FF004589 (medium blue)
                LightBackgroundColor = -16758391,
                // Foreground: #FFEDE1F5 (lavender white)
                ForegroundColor = -1183243,
                // SelectedBackground: #FF2790FF (bright blue)
                SelectedBackgroundColor = -14181633,
                // HighlightedBackground: #FF003BA9 (royal blue)
                HighlightedBackgroundColor = -16751687,
                // HighlightedBorder: #FF005B7A (teal blue)
                HighlightedBorderColor = -16742918,
                // WindowBorder: #FF2790FF (bright blue)
                WindowBorderColor = -14181633,
                // MenuBorder: #FF004B3A (dark teal)
                MenuBorderColor = -16747302,

                // Game Display
                GameAspectRatio = 0.6,
                GameSpacingHorizontal = 30,
                GameSpacingVertical = 30,
                GamePaddingHorizontal = 10,
                GamePaddingVertical = 17,
                GameTextLines = 2,
                GameTextSpacingVertical = 10,

                // Fonts
                GameFont = "Segoe UI, 10.5pt",
                SidebarFont = "Segoe UI, 10.5pt",
                GameDetailsLargeFont = "Segoe UI, 12pt",
                GameDetailsSmallFont = "Segoe UI, 10.5pt",
                GameDetailsTitleFont = "Segoe UI, 13.5pt, style=Bold",

                // Features
                AutoPlayMusic = true,
                EnableGamepad = true,
                MinimizeOnGameLaunch = true,
                RestoreOnGameExit = true,
                CheckForUpdates = true,

                // Gamepad
                GamepadId = "XInput",
                GamepadName = "XInput Xbox 360/Xbox One",
                ControllerSelectButton = 1,
                ControllerContextMenuButton = 2,
                ControllerPlayButton = 3,
                ControllerTopMenuButton = 4,
                ControllerSideBarButton = 5,
                ControllerGameDetailsButton = 6,

                // Sorting
                SortBy = "Title",
                SortByDesc = false
            };
        }

        private BigBoxSettings GetDefaultBigBoxSettings()
        {
            return new BigBoxSettings
            {
                Theme = "Default",
                BackgroundFade = 70,
                FrameRate = 60,
                VideoPlaybackEngine = "VLC",
                CoverFlowReflectionOpacity = 40,
                CoverFlowImageQuality = 400,

                // Sound
                PlayStartupSound = true,
                PlaySelectSound = true,
                PlayBackSound = true,
                PlayNavigationSound = true,
                AutoPlayMusicGamesList = true,
                AutoPlayMusicGameDetails = true,

                // Transitions
                ScreenTransition = "Fade",
                GameBackgroundTransition = "Fade",
                GameMainImageTransition = "Slide Horizontal",
                GameImageVideoTransition = "Slide Horizontal",
                GamesListTransition = "Slide Vertical",
                PlatformsListTransition = "Fade",
                GameDetailsTransition = "Slide Vertical",

                // Display
                ShowBrokenGames = true,
                GameImageType = "Boxes",
                GamesUseBackgroundVideos = true,
                GamesListView = "HorizontalWheel3",
                ShowGamesListTitle = true,
                ShowStarNextToFavoritedGames = true,

                // Game Details
                GameDetailsClearLogos = true,
                ShowGameTitle = true,
                ShowGamePlatform = true,
                ShowGameReleaseDate = true,
                ShowGameGenres = true,
                ShowGameSeries = true,
                ShowGamePlayMode = true,
                ShowGameStatus = true,
                ShowGameSource = true,
                ShowGamePlayTime = true,
                ShowGameNotes = true,
                ShowGameFavorite = true,
                ShowGameCompleted = true,
                ShowGameRating = true,

                // Platforms
                PlatformsUseBackgroundVideos = true,
                PlatformsListView = "PlatformWheel2",
                ShowFiltersListTitle = true,
                ShowRecentGames = true,
                ShowFavoriteGames = true
            };
        }

        #endregion
    }
}
