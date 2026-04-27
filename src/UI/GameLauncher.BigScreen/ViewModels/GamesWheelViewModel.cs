using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLauncher.BigScreen.Navigation;
using GameLauncher.BigScreen.Services;
using GameLauncher.BigScreen.Views;
using GameLauncher.Core.Enums;
using GameLauncher.Core.Helpers;
using GameLauncher.Core.Models;
using GameLauncher.Data.Cache;
using GameLauncher.Data.Xml;
using GameLauncher.Infrastructure.Services;

namespace GameLauncher.BigScreen.ViewModels
{
    /// <summary>
    /// Wrapper for Game with cover image for BigScreen display.
    /// </summary>
    public partial class GameItem : ObservableObject
    {
        public Game Model { get; }
        public string Title => Model.Title;
        public string? Developer => Model.Developer;
        public string? Genre => Model.Genre;
        public DateTime? ReleaseDate => Model.ReleaseDate;
        public int PlayCount => Model.PlayCount;
        public long PlayTime => Model.PlayTime;

        public bool Installed
        {
            get => Model.Installed;
            set { Model.Installed = value; OnPropertyChanged(); }
        }
        public bool Favorite
        {
            get => Model.Favorite;
            set { Model.Favorite = value; OnPropertyChanged(); }
        }
        public bool Completed
        {
            get => Model.Completed;
            set { Model.Completed = value; OnPropertyChanged(); }
        }
        public bool Broken => Model.Broken;

        // BUG-11 FIX: needs setter so UI refreshes after SetRating()
        public int StarRating
        {
            get => Model.StarRating;
            set { Model.StarRating = value; OnPropertyChanged(); }
        }
        public float CommunityRating => Model.CommunityStarRating;

        [ObservableProperty]
        private BitmapImage? _coverImage;

        public GameItem(Game game)
        {
            Model = game;
        }

        public string? ResolveCoverImagePath()
        {
            var launchBoxPath = VideoPathResolver.LaunchBoxPath;
            if (string.IsNullOrEmpty(launchBoxPath) || string.IsNullOrEmpty(Model.Platform) || string.IsNullOrEmpty(Model.Title))
                return null;

            string[] imageTypeFolders = {
                "Box - Front",
                "Box - Front - Reconstructed",
                "Fanart - Box - Front",
                "Steam Poster",
                "Epic Games Poster",
                "GOG Poster",
                "Origin Poster",
                "Box - 3D",
                "Banner",
                "Clear Logo"
            };
            string sanitizedTitle = FileNameHelper.SanitizeForLaunchBox(Model.Title);
            // BUG-12 FIX: empty suffix first = main image is preferred over -01, -02, etc.
            string[] suffixes = { "", "-01", "-02", "-03" };
            string[] extensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

            foreach (var imageType in imageTypeFolders)
            {
                string typeFolder = Path.Combine(launchBoxPath, "Images", Model.Platform, imageType);
                if (!Directory.Exists(typeFolder))
                    continue;

                var result = SearchInFolder(typeFolder, sanitizedTitle, suffixes, extensions);
                if (result != null) return result;

                try
                {
                    foreach (var regionFolder in Directory.GetDirectories(typeFolder))
                    {
                        result = SearchInFolder(regionFolder, sanitizedTitle, suffixes, extensions);
                        if (result != null) return result;
                    }
                }
                catch { }
            }

            return null;
        }

        private static string? SearchInFolder(string folder, string sanitizedTitle, string[] suffixes, string[] extensions)
        {
            foreach (var suffix in suffixes)
            {
                foreach (var ext in extensions)
                {
                    string filePath = Path.Combine(folder, sanitizedTitle + suffix + ext);
                    if (File.Exists(filePath))
                        return filePath;
                }
            }
            return null;
        }

        public static BitmapImage? CreateBitmapFromPath(string path, int decodeWidth = 250)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.DecodePixelWidth = decodeWidth;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        // SanitizeFileName removed — use FileNameHelper.SanitizeForLaunchBox() instead
    }

    /// <summary>
    /// ViewModel para la vista de juegos (wheel horizontal).
    /// </summary>
    public partial class GamesWheelViewModel : ObservableObject, INavigationAware
    {
        private readonly GameCacheManager _cacheManager;
        private readonly IEmulatorLauncher _emulatorLauncher;
        private readonly IStatisticsTracker _statisticsTracker;
        private readonly IGameManager _gameManager;
        private readonly XmlDataContext _dataContext;
        private readonly BackgroundMusicService _musicService;
        private readonly SoundEffectService _soundService;
        private CancellationTokenSource? _videoDelayCts;

        // Master list — all games loaded for current platform, unfiltered
        private List<GameItem> _allGames = new();

        // ── Sort & Filter state ──────────────────────────────────────
        [ObservableProperty]
        private SortField _currentSortField = SortField.Title;

        [ObservableProperty]
        private bool _sortDescending;

        [ObservableProperty]
        private GameFilter _activeFilter = GameFilter.All;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _sortFilterStatusText = string.Empty;

        // ── View mode (Wheel vs List) ─────────────────────────────
        [ObservableProperty]
        private BigScreenViewMode _viewMode = BigScreenViewMode.Wheel;

        public bool IsWheelMode => ViewMode == BigScreenViewMode.Wheel;
        public bool IsListMode  => ViewMode == BigScreenViewMode.List;

        [RelayCommand]
        private void ToggleViewMode()
        {
            ViewMode = ViewMode == BigScreenViewMode.Wheel
                ? BigScreenViewMode.List
                : BigScreenViewMode.Wheel;
            OnPropertyChanged(nameof(IsWheelMode));
            OnPropertyChanged(nameof(IsListMode));
            _soundService.Play(SoundEffectService.FilterChanged);
        }
        // ────────────────────────────────────────────────────────

        [ObservableProperty]
        private ObservableCollection<GameItem> _games = new();

        [ObservableProperty]
        private GameItem? _selectedGame;

        [ObservableProperty]
        private Uri? _gameVideoUri;

        [ObservableProperty]
        private string _platformName = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        // BUG-02: Launching overlay state (separate from IsLoading so it doesn't show the spinner)
        [ObservableProperty]
        private bool _isLaunching;

        [ObservableProperty]
        private string _launchingTitle = string.Empty;

        [ObservableProperty]
        private string _statusText = string.Empty;

        // Propiedades del juego seleccionado para el overlay
        public string SelectedGameTitle => SelectedGame?.Title ?? string.Empty;
        public string SelectedGameDeveloper => SelectedGame?.Developer ?? "Desconocido";
        public string SelectedGameGenre => SelectedGame?.Genre ?? string.Empty;
        public string SelectedGameYear => SelectedGame?.ReleaseDate?.Year.ToString() ?? string.Empty;
        public string SelectedGamePlayTime => FormatPlayTime(SelectedGame?.PlayTime ?? 0);
        public int SelectedGamePlayCount => SelectedGame?.PlayCount ?? 0;
        public bool SelectedGameFavorite => SelectedGame?.Favorite ?? false;
        public bool SelectedGameInstalled => SelectedGame?.Installed ?? false;
        public bool SelectedGameCompleted => SelectedGame?.Completed ?? false;
        public bool SelectedGameBroken => SelectedGame?.Broken ?? false;
        public int SelectedGameStarRating => SelectedGame?.StarRating ?? 0;
        public string SelectedGameRatingText => FormatStarRating(SelectedGame?.StarRating ?? 0, SelectedGame?.CommunityRating ?? 0);
        public string SelectedGamePlatform => SelectedGame?.Model.Platform ?? string.Empty;
        public string? SelectedGameDescription => string.IsNullOrWhiteSpace(SelectedGame?.Model.Notes) ? null : SelectedGame!.Model.Notes;
        public string? SelectedGamePublisher => string.IsNullOrWhiteSpace(SelectedGame?.Model.Publisher) ? null : SelectedGame!.Model.Publisher;
        public string? SelectedGameSeries => string.IsNullOrWhiteSpace(SelectedGame?.Model.Series) ? null : SelectedGame!.Model.Series;
        public string SelectedGameLastPlayed => SelectedGame?.Model.LastPlayed?.ToString("dd/MM/yyyy") ?? "Nunca";
        public string? SelectedGameRegion => string.IsNullOrWhiteSpace(SelectedGame?.Model.Region) ? null : SelectedGame!.Model.Region;

        public GamesWheelViewModel(
            GameCacheManager cacheManager,
            IEmulatorLauncher emulatorLauncher,
            IStatisticsTracker statisticsTracker,
            IGameManager gameManager,
            XmlDataContext dataContext,
            BackgroundMusicService? musicService = null,
            SoundEffectService? soundService = null)
        {
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _emulatorLauncher = emulatorLauncher ?? throw new ArgumentNullException(nameof(emulatorLauncher));
            _statisticsTracker = statisticsTracker ?? throw new ArgumentNullException(nameof(statisticsTracker));
            _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _musicService = musicService ?? new BackgroundMusicService();
            _soundService = soundService ?? new SoundEffectService();
        }

        public async Task LoadGamesAsync(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                return;

            IsLoading = true;
            PlatformName = platformName;
            StatusText = $"Cargando juegos de {platformName}...";

            try
            {
                var games = await Task.Run(() => _cacheManager.GetGames(platformName));
                _allGames = games.Select(g => new GameItem(g)).ToList();

                // Reset filter/search when switching platforms
                ActiveFilter = GameFilter.All;
                SearchText = string.Empty;
                ApplyFiltersAndSort();

                StatusText = $"{Games.Count} juegos en {platformName}";

                // Start background music for this platform
                _ = _musicService.PlayForPlatformAsync(platformName);

                // Load cover images in batches (use current visible games)
                _ = LoadCoverImagesAsync(_allGames);
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Applies the current ActiveFilter + SearchText, then sorts by CurrentSortField.
        /// Rebuilds the Games collection in-place without reloading from XML.
        /// </summary>
        private void ApplyFiltersAndSort()
        {
            IEnumerable<GameItem> result = _allGames;

            // 1. Text search
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var q = SearchText.Trim().ToLowerInvariant();
                result = result.Where(g =>
                    (g.Title?.ToLowerInvariant().Contains(q) ?? false) ||
                    (g.Developer?.ToLowerInvariant().Contains(q) ?? false) ||
                    (g.Genre?.ToLowerInvariant().Contains(q) ?? false));
            }

            // 2. Preset filter
            result = ActiveFilter switch
            {
                GameFilter.Favorites      => result.Where(g => g.Favorite),
                GameFilter.Completed      => result.Where(g => g.Completed),
                GameFilter.Installed      => result.Where(g => g.Installed),
                GameFilter.RecentlyPlayed => result.Where(g => g.PlayCount > 0).OrderByDescending(g => g.Model.LastPlayed),
                GameFilter.NeverPlayed    => result.Where(g => g.PlayCount == 0),
                GameFilter.Broken         => result.Where(g => g.Broken),
                _                         => result
            };

            // 3. Sort
            result = (CurrentSortField, SortDescending) switch
            {
                (SortField.Title,       false) => result.OrderBy(g => g.Title),
                (SortField.Title,       true)  => result.OrderByDescending(g => g.Title),
                (SortField.ReleaseDate, false) => result.OrderBy(g => g.ReleaseDate ?? DateTime.MaxValue),
                (SortField.ReleaseDate, true)  => result.OrderByDescending(g => g.ReleaseDate ?? DateTime.MinValue),
                (SortField.LastPlayed,  false) => result.OrderByDescending(g => g.Model.LastPlayed),
                (SortField.LastPlayed,  true)  => result.OrderBy(g => g.Model.LastPlayed),
                (SortField.PlayCount,   false) => result.OrderByDescending(g => g.PlayCount),
                (SortField.PlayCount,   true)  => result.OrderBy(g => g.PlayCount),
                (SortField.PlayTime,    false) => result.OrderByDescending(g => g.PlayTime),
                (SortField.PlayTime,    true)  => result.OrderBy(g => g.PlayTime),
                (SortField.StarRating,  false) => result.OrderByDescending(g => g.StarRating),
                (SortField.StarRating,  true)  => result.OrderBy(g => g.StarRating),
                (SortField.Developer,   false) => result.OrderBy(g => g.Developer),
                (SortField.Developer,   true)  => result.OrderByDescending(g => g.Developer),
                (SortField.Genre,       false) => result.OrderBy(g => g.Genre),
                (SortField.Genre,       true)  => result.OrderByDescending(g => g.Genre),
                (SortField.DateAdded,   false) => result.OrderByDescending(g => g.Model.DateAdded),
                (SortField.DateAdded,   true)  => result.OrderBy(g => g.Model.DateAdded),
                _                              => result.OrderBy(g => g.Title)
            };

            var list = result.ToList();

            Games.Clear();
            foreach (var item in list)
                Games.Add(item);

            SelectedGame = Games.Count > 0 ? Games[0] : null;

            var arrow = SortDescending ? "▼" : "▲";
            var filterLabel = ActiveFilter == GameFilter.All ? "" : $" [{ActiveFilter}]";
            var searchLabel = string.IsNullOrWhiteSpace(SearchText) ? "" : $" \"{SearchText}\"";
            SortFilterStatusText = $"{Games.Count} juegos — {CurrentSortField} {arrow}{filterLabel}{searchLabel}";
            StatusText = SortFilterStatusText;
        }

        // ── Sort / Filter commands ───────────────────────────────

        /// <summary>Cycles to the next sort field and reapplies.</summary>
        [RelayCommand]
        private void CycleSortField()
        {
            var fields = System.Enum.GetValues<SortField>();
            int next = ((int)CurrentSortField + 1) % fields.Length;
            CurrentSortField = fields[next];
            ApplyFiltersAndSort();
            _soundService.Play(SoundEffectService.FilterChanged);
        }

        /// <summary>Toggles ascending / descending and reapplies.</summary>
        [RelayCommand]
        private void ToggleSortDirection()
        {
            SortDescending = !SortDescending;
            ApplyFiltersAndSort();
            _soundService.Play(SoundEffectService.FilterChanged);
        }

        /// <summary>Sets a specific sort field.</summary>
        [RelayCommand]
        private void SetSortField(SortField field)
        {
            if (CurrentSortField == field)
                SortDescending = !SortDescending;
            else
            {
                CurrentSortField = field;
                SortDescending = false;
            }
            ApplyFiltersAndSort();
            _soundService.Play(SoundEffectService.FilterChanged);
        }

        /// <summary>Cycles to the next game filter preset.</summary>
        [RelayCommand]
        private void CycleFilter()
        {
            var filters = System.Enum.GetValues<GameFilter>();
            int next = ((int)ActiveFilter + 1) % filters.Length;
            ActiveFilter = filters[next];
            ApplyFiltersAndSort();
            _soundService.Play(SoundEffectService.FilterChanged);
        }

        /// <summary>Applies a specific filter preset.</summary>
        [RelayCommand]
        private void SetFilter(GameFilter filter)
        {
            ActiveFilter = filter;
            ApplyFiltersAndSort();
            _soundService.Play(SoundEffectService.FilterChanged);
        }

        /// <summary>Applies text search across title, developer and genre.</summary>
        [RelayCommand]
        private void Search(string? query)
        {
            SearchText = query ?? string.Empty;
            ApplyFiltersAndSort();
        }

        /// <summary>Clears search text and resets to All filter.</summary>
        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
            ActiveFilter = GameFilter.All;
            ApplyFiltersAndSort();
        }

        // ────────────────────────────────────────────────────────────

        public async Task LoadGamesFromListAsync(List<Game> games, string title)
        {
            if (games == null || games.Count == 0)
            {
                StatusText = $"No hay juegos en {title}";
                return;
            }

            IsLoading = true;
            PlatformName = title;
            StatusText = $"Cargando {title}...";

            try
            {
                var items = await Task.Run(() =>
                    games.Select(g => new GameItem(g)).ToList());

                _allGames = items;

                // Reset filter/search when loading a playlist
                ActiveFilter = GameFilter.All;
                SearchText = string.Empty;
                CurrentSortField = SortField.Title;
                SortDescending = false;
                ApplyFiltersAndSort();

                StatusText = $"{Games.Count} juegos en {title}";

                _ = LoadCoverImagesAsync(items);
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCoverImagesAsync(List<GameItem> items)
        {
            const int batchSize = 30;

            for (int i = 0; i < items.Count; i += batchSize)
            {
                var batch = items.Skip(i).Take(batchSize).ToList();

                // Resolve paths in background
                var resolved = await Task.Run(() =>
                    batch.Select(item => new { Item = item, Path = item.ResolveCoverImagePath() }).ToList()
                );

                // Create BitmapImages and assign (Freeze makes them thread-safe)
                foreach (var r in resolved)
                {
                    if (r.Path != null)
                    {
                        var bitmap = await Task.Run(() => GameItem.CreateBitmapFromPath(r.Path));
                        if (bitmap != null)
                            r.Item.CoverImage = bitmap;
                    }
                }

                await Task.Delay(1);
            }
        }

        partial void OnSelectedGameChanged(GameItem? value)
        {
            // Cancel any pending video resolution
            _videoDelayCts?.Cancel();
            GameVideoUri = null;

            if (value == null)
                return;

            var cts = new CancellationTokenSource();
            _videoDelayCts = cts;
            var gameTitle = value.Model.Title;
            var gamePlatform = value.Model.Platform;

            _ = Task.Run(async () =>
            {
                try
                {
                    // Wait briefly before resolving video
                    await Task.Delay(500, cts.Token);

                    var videoPath = VideoPathResolver.Resolve(gameTitle, gamePlatform);
                    if (videoPath != null && !cts.Token.IsCancellationRequested)
                    {
                        var uri = new Uri(videoPath);
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (!cts.Token.IsCancellationRequested && SelectedGame?.Model.Title == gameTitle)
                            {
                                GameVideoUri = uri;
                            }
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch
                {
                }
            });
        }

        [RelayCommand]
        private void NavigateLeft()
        {
            if (SelectedGame == null || Games.Count == 0)
                return;

            int currentIndex = Games.IndexOf(SelectedGame);
            if (currentIndex > 0)
            {
                SelectedGame = Games[currentIndex - 1];
                UpdateSelectedGameProperties();
            }
        }

        [RelayCommand]
        private void NavigateRight()
        {
            if (SelectedGame == null || Games.Count == 0)
                return;

            int currentIndex = Games.IndexOf(SelectedGame);
            if (currentIndex < Games.Count - 1)
            {
                SelectedGame = Games[currentIndex + 1];
                UpdateSelectedGameProperties();
            }
        }

        [RelayCommand]
        private async Task LaunchGameAsync()
        {
            if (SelectedGame == null)
                return;

            var game = SelectedGame;

            try
            {
                // BUG-02: Show launching overlay + minimize window
                LaunchingTitle = game.Title;
                IsLaunching = true;

                // BUG-03: Pause music while game runs
                _musicService.Pause();

                // Minimize BigScreen window so the emulator has focus
                var window = System.Windows.Application.Current.MainWindow;
                if (window != null)
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        window.WindowState = System.Windows.WindowState.Minimized);

                StatusText = $"Lanzando {game.Title}...";

                var result = await _emulatorLauncher.LaunchGameAsync(game.Model);

                if (result.Success)
                {
                    await _statisticsTracker.RecordPlaySessionAsync(game.Model, result.PlayTimeSeconds);
                    StatusText = $"Jugado: {game.Title} ({result.PlayTimeSeconds}s)";

                    // Refresh play stats in the GameItem so the UI updates
                    game.Model.PlayCount = game.Model.PlayCount; // already updated by tracker
                    UpdateSelectedGameProperties();
                }
                else
                {
                    StatusText = $"Error al lanzar: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
            finally
            {
                IsLaunching = false;

                // BUG-02: Restore window after game exits
                var window = System.Windows.Application.Current.MainWindow;
                if (window != null)
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        window.WindowState = System.Windows.WindowState.Maximized;
                        window.Activate();
                    });

                // BUG-03: Resume music
                _musicService.Resume();
            }
        }

        [RelayCommand]
        private void ToggleFavorite()
        {
            if (SelectedGame == null)
                return;

            SelectedGame.Favorite = !SelectedGame.Favorite;
            UpdateSelectedGameProperties();

            StatusText = SelectedGame.Favorite ? "\u2605 Agregado a favoritos" : "Quitado de favoritos";
        }

        // ══════════════════════════════════════════════════════════
        //  MENU DE GESTION (A button)
        // ══════════════════════════════════════════════════════════

        [ObservableProperty]
        private bool _showingManageMenu;

        [ObservableProperty]
        private ObservableCollection<ManageMenuItem> _manageItems = new();

        [ObservableProperty]
        private ManageMenuItem? _selectedManageItem;

        [ObservableProperty]
        private string _manageGameTitle = string.Empty;

        /// <summary>
        /// A button: Abre el menu de gestion del juego seleccionado.
        /// </summary>
        [RelayCommand]
        private void OpenManageMenu()
        {
            if (SelectedGame == null) return;

            var game = SelectedGame;
            ManageGameTitle = game.Title;

            ManageItems = new ObservableCollection<ManageMenuItem>
            {
                new ManageMenuItem
                {
                    Key = "installed",
                    Label = "Instalado",
                    Icon = "\uD83D\uDCE6",
                    ActiveColor = "#27AE60",
                    IsActive = game.Installed,
                    StatusText = game.Installed ? "SI" : "NO"
                },
                new ManageMenuItem
                {
                    Key = "favorite",
                    Label = "Favorito",
                    Icon = "\u2605",
                    ActiveColor = "#c4a000",
                    IsActive = game.Favorite,
                    StatusText = game.Favorite ? "SI" : "NO"
                },
                new ManageMenuItem
                {
                    Key = "completed",
                    Label = "Completado",
                    Icon = "\u2713",
                    ActiveColor = "#2ecc71",
                    IsActive = game.Completed,
                    StatusText = game.Completed ? "SI" : "NO"
                },
                new ManageMenuItem
                {
                    Key = "broken",
                    Label = "Roto / No funciona",
                    Icon = "\u2716",
                    ActiveColor = "#e74c3c",
                    IsActive = game.Broken,
                    StatusText = game.Broken ? "SI" : "NO"
                },
                new ManageMenuItem
                {
                    Key = "rating1",
                    Label = "Rating: 1 estrella",
                    Icon = "\u2606",
                    ActiveColor = "#f1c40f",
                    IsActive = game.StarRating == 1,
                    StatusText = game.StarRating == 1 ? "ACTIVO" : ""
                },
                new ManageMenuItem
                {
                    Key = "rating2",
                    Label = "Rating: 2 estrellas",
                    Icon = "\u2606",
                    ActiveColor = "#f1c40f",
                    IsActive = game.StarRating == 2,
                    StatusText = game.StarRating == 2 ? "ACTIVO" : ""
                },
                new ManageMenuItem
                {
                    Key = "rating3",
                    Label = "Rating: 3 estrellas",
                    Icon = "\u2606",
                    ActiveColor = "#f1c40f",
                    IsActive = game.StarRating == 3,
                    StatusText = game.StarRating == 3 ? "ACTIVO" : ""
                },
                new ManageMenuItem
                {
                    Key = "rating4",
                    Label = "Rating: 4 estrellas",
                    Icon = "\u2606",
                    ActiveColor = "#f1c40f",
                    IsActive = game.StarRating == 4,
                    StatusText = game.StarRating == 4 ? "ACTIVO" : ""
                },
                new ManageMenuItem
                {
                    Key = "rating5",
                    Label = "Rating: 5 estrellas",
                    Icon = "\u2606",
                    ActiveColor = "#f1c40f",
                    IsActive = game.StarRating == 5,
                    StatusText = game.StarRating == 5 ? "ACTIVO" : ""
                },
                new ManageMenuItem
                {
                    Key = "edit",
                    Label = "Editar metadatos",
                    Icon = "\u270E",
                    ActiveColor = "#00d4ff",
                    IsActive = false,
                    StatusText = "\u25B6"
                },
            };

            SelectedManageItem = ManageItems[0];
            ShowingManageMenu = true;
        }

        public void ManageNavigateUp()
        {
            if (ManageItems.Count == 0 || SelectedManageItem == null) return;
            int idx = ManageItems.IndexOf(SelectedManageItem);
            if (idx > 0) SelectedManageItem = ManageItems[idx - 1];
        }

        public void ManageNavigateDown()
        {
            if (ManageItems.Count == 0 || SelectedManageItem == null) return;
            int idx = ManageItems.IndexOf(SelectedManageItem);
            if (idx < ManageItems.Count - 1) SelectedManageItem = ManageItems[idx + 1];
        }

        /// <summary>
        /// B en menu: toggle la opcion seleccionada.
        /// </summary>
        public void ManageToggleSelected()
        {
            if (SelectedManageItem == null || SelectedGame == null) return;

            var game = SelectedGame;
            var item = SelectedManageItem;

            switch (item.Key)
            {
                case "installed":
                    game.Installed = !game.Installed;
                    item.IsActive = game.Installed;
                    item.StatusText = game.Installed ? "SI" : "NO";
                    break;
                case "favorite":
                    game.Favorite = !game.Favorite;
                    item.IsActive = game.Favorite;
                    item.StatusText = game.Favorite ? "SI" : "NO";
                    break;
                case "completed":
                    game.Completed = !game.Completed;
                    item.IsActive = game.Completed;
                    item.StatusText = game.Completed ? "SI" : "NO";
                    break;
                case "broken":
                    game.Model.Broken = !game.Model.Broken;
                    item.IsActive = game.Model.Broken;
                    item.StatusText = game.Model.Broken ? "SI" : "NO";
                    break;
                case "rating1": SetRating(game, 1); break;
                case "rating2": SetRating(game, 2); break;
                case "rating3": SetRating(game, 3); break;
                case "rating4": SetRating(game, 4); break;
                case "rating5": SetRating(game, 5); break;
                case "edit":
                    ShowingManageMenu = false;
                    EditorViewModel = new GameEditorViewModel(game.Model, _gameManager, _dataContext);
                    ShowingEditor = true;
                    return;
            }

            UpdateSelectedGameProperties();
        }

        private void SetRating(GameItem game, int rating)
        {
            // Si ya tiene este rating, quitarlo (toggle)
            int newRating = game.Model.StarRating == rating ? 0 : rating;
            game.Model.StarRating = newRating;

            // Actualizar todos los items de rating
            foreach (var mi in ManageItems)
            {
                if (mi.Key.StartsWith("rating"))
                {
                    int r = int.Parse(mi.Key.Replace("rating", ""));
                    mi.IsActive = newRating == r;
                    mi.StatusText = newRating == r ? "ACTIVO" : "";
                }
            }
        }

        public void CloseManageMenu()
        {
            ShowingManageMenu = false;
            ManageItems.Clear();
            SaveCurrentGame();
            UpdateSelectedGameProperties();
        }

        private async void SaveCurrentGame()
        {
            if (SelectedGame == null) return;
            try { await _gameManager.UpdateGameAsync(SelectedGame.Model.Platform, SelectedGame.Model); }
            catch { }
        }

        // ══════════════════════════════════════════════════════════
        //  EDITOR DE METADATOS
        // ══════════════════════════════════════════════════════════

        [ObservableProperty]
        private bool _showingEditor;

        [ObservableProperty]
        private GameEditorViewModel? _editorViewModel;

        public void EditorNavigateUp() => EditorViewModel?.NavigateUp();
        public void EditorNavigateDown() => EditorViewModel?.NavigateDown();
        public void EditorNavigateLeft() => EditorViewModel?.NavigateLeft();
        public void EditorNavigateRight() => EditorViewModel?.NavigateRight();
        public void EditorConfirm() => EditorViewModel?.ConfirmField();
        public void EditorNextSection() => EditorViewModel?.NextSection();
        public void EditorPreviousSection() => EditorViewModel?.PreviousSection();

        public void EditorConfirmTextEdit() => EditorViewModel?.ConfirmTextEdit();

        public void EditorCancelField()
        {
            if (EditorViewModel == null) return;

            // If a field is being edited, cancel just the field edit
            if (EditorViewModel.IsFieldEditing)
            {
                EditorViewModel.CancelField();
                return;
            }

            // Otherwise cancel the whole editor
            EditorViewModel.Cancel();
            ShowingEditor = false;
            UpdateSelectedGameProperties();
        }

        public async Task EditorSaveAsync()
        {
            if (EditorViewModel == null) return;
            await EditorViewModel.SaveAsync();
            ShowingEditor = false;
            UpdateSelectedGameProperties();
        }

        [RelayCommand]
        private void ShowDetails()
        {
            if (SelectedGame == null)
                return;

            // Navegar a GameDetailsView
            var frame = System.Windows.Application.Current.MainWindow?.FindName("NavigationFrame") as System.Windows.Controls.Frame;
            if (frame != null)
            {
                var app = (App)System.Windows.Application.Current;
                var detailsVM = app.CreateGameDetailsViewModel();
                var detailsView = new GameDetailsView { DataContext = detailsVM };
                detailsVM.OnNavigatedTo(SelectedGame.Model);
                frame.Navigate(detailsView);
            }
        }

        // ══════════════════════════════════════════════════════════
        //  GALERÍA DE IMÁGENES
        // ══════════════════════════════════════════════════════════

        [ObservableProperty]
        private bool _showingGallery;

        [ObservableProperty]
        private bool _showingZoomedImage;

        [ObservableProperty]
        private string? _zoomedImagePath;

        [ObservableProperty]
        private string? _zoomedImageLabel;

        [ObservableProperty]
        private ObservableCollection<GameImageItem> _galleryImages = new();

        [ObservableProperty]
        private GameImageItem? _selectedGalleryImage;

        [ObservableProperty]
        private string _galleryTitle = string.Empty;

        [ObservableProperty]
        private string _galleryCountText = string.Empty;

        private int _galleryColumns = 4;

        /// <summary>
        /// X button: Abre la galería de imágenes del juego seleccionado.
        /// </summary>
        [RelayCommand]
        private void ShowImages()
        {
            if (SelectedGame == null) return;

            var images = ResolveAllGameImages(SelectedGame.Model);
            if (images.Count == 0)
            {
                StatusText = $"No se encontraron imagenes para {SelectedGame.Title}";
                return;
            }

            GalleryImages = new ObservableCollection<GameImageItem>(images);
            SelectedGalleryImage = GalleryImages[0];
            ShowingGallery = true;
            ShowingZoomedImage = false;
            GalleryTitle = $"IMAGENES - {SelectedGame.Title}";
            GalleryCountText = $"{images.Count} imagenes";
        }

        /// <summary>
        /// Navega en la galería con D-pad.
        /// </summary>
        public void GalleryNavigate(int dx, int dy)
        {
            if (GalleryImages.Count == 0 || SelectedGalleryImage == null) return;

            int idx = GalleryImages.IndexOf(SelectedGalleryImage);
            int newIdx = idx + dx + (dy * _galleryColumns);
            newIdx = Math.Max(0, Math.Min(GalleryImages.Count - 1, newIdx));
            SelectedGalleryImage = GalleryImages[newIdx];
        }

        /// <summary>
        /// B en galería: amplía la imagen seleccionada.
        /// </summary>
        public void GallerySelect()
        {
            if (SelectedGalleryImage == null) return;
            ZoomedImagePath = SelectedGalleryImage.FullPath;
            ZoomedImageLabel = SelectedGalleryImage.TypeName;
            ShowingZoomedImage = true;
        }

        /// <summary>
        /// Y en galería: cierra zoom o galería.
        /// </summary>
        public void GalleryBack()
        {
            if (ShowingZoomedImage)
            {
                ShowingZoomedImage = false;
                ZoomedImagePath = null;
                return;
            }

            ShowingGallery = false;
            GalleryImages.Clear();
            StatusText = $"{Games.Count} juegos en {PlatformName}";
        }

        /// <summary>
        /// Navega entre imágenes ampliadas con LB/RB.
        /// </summary>
        public void ZoomedNavigate(int direction)
        {
            if (!ShowingZoomedImage || GalleryImages.Count == 0 || SelectedGalleryImage == null) return;

            int idx = GalleryImages.IndexOf(SelectedGalleryImage);
            int newIdx = (idx + direction + GalleryImages.Count) % GalleryImages.Count;
            SelectedGalleryImage = GalleryImages[newIdx];
            ZoomedImagePath = SelectedGalleryImage.FullPath;
            ZoomedImageLabel = SelectedGalleryImage.TypeName;
        }

        private List<GameImageItem> ResolveAllGameImages(Game game)
        {
            var results = new List<GameImageItem>();
            var launchBoxPath = VideoPathResolver.LaunchBoxPath;
            if (string.IsNullOrEmpty(launchBoxPath) || string.IsNullOrEmpty(game.Platform))
                return results;

            string[] imageTypes = {
                "Box - Front", "Box - Front - Reconstructed", "Box - Back",
                "Screenshot - Gameplay", "Screenshot - Game Title",
                "Screenshot - Game Select", "Screenshot - Game Over",
                "Fanart - Background", "Fanart - Box - Front", "Fanart - Box - Back",
                "Clear Logo", "Banner", "Steam Poster",
                "Box - 3D", "Disc", "Cart - Front"
            };

            var sanitizedTitle = FileNameHelper.SanitizeForLaunchBox(game.Title ?? "");
            string[] extensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };
            string[] suffixes = { "", "-01", "-02", "-03", "-04", "-05" };

            foreach (var imageType in imageTypes)
            {
                var baseDir = Path.Combine(launchBoxPath, "Images", game.Platform, imageType);
                SearchImagesInDir(baseDir, sanitizedTitle, imageType, suffixes, extensions, results);

                if (Directory.Exists(baseDir))
                {
                    try
                    {
                        foreach (var subDir in Directory.GetDirectories(baseDir))
                        {
                            var regionName = Path.GetFileName(subDir);
                            SearchImagesInDir(subDir, sanitizedTitle, $"{imageType} ({regionName})", suffixes, extensions, results);
                        }
                    }
                    catch { }
                }
            }

            return results;
        }

        private static void SearchImagesInDir(string dir, string sanitizedTitle, string typeName,
            string[] suffixes, string[] extensions, List<GameImageItem> results)
        {
            if (!Directory.Exists(dir)) return;
            foreach (var suffix in suffixes)
            {
                foreach (var ext in extensions)
                {
                    var path = Path.Combine(dir, sanitizedTitle + suffix + ext);
                    if (File.Exists(path))
                    {
                        var label = typeName;
                        if (!string.IsNullOrEmpty(suffix))
                            label += $" {suffix.TrimStart('-')}";

                        results.Add(new GameImageItem
                        {
                            FullPath = path,
                            TypeName = label
                        });
                    }
                }
            }
        }

        // SanitizeFileName removed — use FileNameHelper.SanitizeForLaunchBox() instead

        // ══════════════════════════════════════════════════════════
        //  PAGINACIÓN
        // ══════════════════════════════════════════════════════════

        [RelayCommand]
        private void PageLeft()
        {
            if (ShowingGallery || ShowingZoomedImage) { ZoomedNavigate(-1); return; }
            if (Games.Count == 0) return;
            int currentIndex = SelectedGame != null ? Games.IndexOf(SelectedGame) : 0;
            int newIndex = Math.Max(0, currentIndex - 10);
            SelectedGame = Games[newIndex];
            UpdateSelectedGameProperties();
            _soundService.Play(SoundEffectService.NavigationLeft);
        }

        [RelayCommand]
        private void PageRight()
        {
            if (ShowingGallery || ShowingZoomedImage) { ZoomedNavigate(1); return; }
            if (Games.Count == 0) return;
            int currentIndex = SelectedGame != null ? Games.IndexOf(SelectedGame) : 0;
            int newIndex = Math.Min(Games.Count - 1, currentIndex + 10);
            SelectedGame = Games[newIndex];
            UpdateSelectedGameProperties();
            _soundService.Play(SoundEffectService.NavigationRight);
        }

        private void UpdateSelectedGameProperties()
        {
            OnPropertyChanged(nameof(SelectedGameTitle));
            OnPropertyChanged(nameof(SelectedGameDeveloper));
            OnPropertyChanged(nameof(SelectedGameGenre));
            OnPropertyChanged(nameof(SelectedGameYear));
            OnPropertyChanged(nameof(SelectedGamePlayTime));
            OnPropertyChanged(nameof(SelectedGamePlayCount));
            OnPropertyChanged(nameof(SelectedGameFavorite));
            OnPropertyChanged(nameof(SelectedGameInstalled));
            OnPropertyChanged(nameof(SelectedGameCompleted));
            OnPropertyChanged(nameof(SelectedGameBroken));
            OnPropertyChanged(nameof(SelectedGameStarRating));
            OnPropertyChanged(nameof(SelectedGameRatingText));
            OnPropertyChanged(nameof(SelectedGamePlatform));
            OnPropertyChanged(nameof(SelectedGameDescription));
            OnPropertyChanged(nameof(SelectedGamePublisher));
            OnPropertyChanged(nameof(SelectedGameSeries));
            OnPropertyChanged(nameof(SelectedGameLastPlayed));
            OnPropertyChanged(nameof(SelectedGameRegion));
        }

        private string FormatPlayTime(long seconds)
        {
            if (seconds == 0)
                return "Sin jugar";

            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);

            if (hours > 0)
                return $"{hours}h {minutes}m";
            else
                return $"{minutes}m";
        }

        private static string FormatStarRating(int userRating, float communityRating)
        {
            if (userRating > 0)
                return new string('\u2605', userRating) + new string('\u2606', 5 - userRating);
            if (communityRating > 0)
            {
                int stars = (int)Math.Round(communityRating);
                return new string('\u2605', stars) + new string('\u2606', 5 - stars);
            }
            return string.Empty;
        }

        // INavigationAware implementation
        public void OnNavigatedTo(object? parameter)
        {
            if (parameter is string platformName)
            {
                _ = LoadGamesAsync(platformName);
            }
        }

        public void OnNavigatedFrom()
        {
        }

        public void OnNavigatedBack(object? parameter)
        {
            if (!string.IsNullOrEmpty(PlatformName))
            {
                _ = LoadGamesAsync(PlatformName);
            }
        }
    }
}
