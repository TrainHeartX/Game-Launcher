using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLauncher.Core.Enums;
using GameLauncher.Core.Models;
using GameLauncher.Data.Cache;
using GameLauncher.Data.Xml;
using GameLauncher.Desktop.Dialogs;
using GameLauncher.Desktop.Models;
using GameLauncher.Infrastructure.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GameLauncher.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel principal de la aplicación Desktop.
    /// Coordina la interacción entre servicios y la UI.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly XmlDataContext _dataContext;
        private readonly GameCacheManager _cacheManager;
        private readonly IEmulatorLauncher _emulatorLauncher;
        private readonly IStatisticsTracker _statisticsTracker;
        private readonly GameManager _gameManager;
        private readonly PlatformManager _platformManager;
        private readonly PlaylistManager _playlistManager;
        private readonly AndroidExportService _exportService;
        private readonly LocalSyncServer _syncServer;

        private bool _isDataLoaded;

        public MainViewModel(
            XmlDataContext dataContext,
            GameCacheManager cacheManager,
            IEmulatorLauncher emulatorLauncher,
            IStatisticsTracker statisticsTracker,
            GameManager gameManager,
            PlatformManager platformManager,
            PlaylistManager playlistManager,
            AndroidExportService exportService,
            LocalSyncServer syncServer)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _emulatorLauncher = emulatorLauncher ?? throw new ArgumentNullException(nameof(emulatorLauncher));
            _statisticsTracker = statisticsTracker ?? throw new ArgumentNullException(nameof(statisticsTracker));
            _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
            _platformManager = platformManager ?? throw new ArgumentNullException(nameof(platformManager));
            _playlistManager = playlistManager ?? throw new ArgumentNullException(nameof(playlistManager));
            _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
            _syncServer = syncServer ?? throw new ArgumentNullException(nameof(syncServer));

            Games = new ObservableCollection<GameViewModel>();
            Platforms = new ObservableCollection<PlatformViewModel>();
            FilteredGames = new ObservableCollection<GameViewModel>();
            PlatformCategories = new ObservableCollection<NavigationNodeViewModel>();
            Playlists = new ObservableCollection<PlaylistViewModel>();
        }

        // Caché en memoria: plataforma -> lista de GameViewModels con imágenes cargadas
        private readonly Dictionary<string, List<GameViewModel>> _platformCache = new();

        // Caché en disco: carpeta donde se guardan los JSON de rutas de imágenes
        private static readonly string _diskCachePath = Path.Combine(AppContext.BaseDirectory, "cache", "images");

        // Colecciones observables
        public ObservableCollection<GameViewModel> Games { get; }
        public ObservableCollection<PlatformViewModel> Platforms { get; }
        public ObservableCollection<GameViewModel> FilteredGames { get; }
        public ObservableCollection<NavigationNodeViewModel> PlatformCategories { get; }
        public ObservableCollection<PlaylistViewModel> Playlists { get; }

        [ObservableProperty]
        private int _totalPlatforms;

        [ObservableProperty]
        private bool _showPlatformsSection = true;

        [ObservableProperty]
        private bool _showPlaylistsSection;

        [ObservableProperty]
        private GameViewModel? _selectedGame;

        [ObservableProperty]
        private PlatformViewModel? _selectedPlatform;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusText = "Listo";

        [ObservableProperty]
        private string _cacheStatusText = "";

        [ObservableProperty]
        private bool _isCaching;

        [ObservableProperty]
        private Uri? _selectedGameVideoUri;

        // ── Sort state (Desktop) ─────────────────────────────────────
        [ObservableProperty]
        private SortField _currentSortField = SortField.Title;

        [ObservableProperty]
        private bool _sortDescending;

        [ObservableProperty]
        private GameFilter _activeFilter = GameFilter.All;

        [ObservableProperty]
        private bool _isGlobalSearchActive;

        [ObservableProperty]
        private string _sortStatusText = string.Empty;
        // ────────────────────────────────────────────────────────

        // Inicialización
        public async Task InitializeAsync()
        {
            IsLoading = true;
            StatusText = "Cargando plataformas...";

            try
            {
                await LoadPlatformsAsync();
                StatusText = $"{TotalPlatforms} plataformas en {PlatformCategories.Count} categorías | {Playlists.Count} playlists";
            }
            catch (Exception ex)
            {
                StatusText = $"Error al cargar: {ex.Message}";
                MessageBox.Show($"Error al inicializar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPlatformsAsync()
        {
            // Load full navigation tree from Parents.xml hierarchy
            var navigationTree = await _platformManager.GetNavigationTreeAsync();

            Platforms.Clear();
            PlatformCategories.Clear();

            var platformNames = new List<string>();
            var platformNodeVMs = new List<NavigationNodeViewModel>();

            foreach (var rootNode in navigationTree)
            {
                var rootVM = ConvertToViewModel(rootNode, platformNames, platformNodeVMs);
                PlatformCategories.Add(rootVM);
            }

            TotalPlatforms = platformNames.Count;

            // Load playlists for sidebar
            Playlists.Clear();
            try
            {
                var allPlaylists = await _playlistManager.GetAllPlaylistsAsync();
                foreach (var playlist in allPlaylists.OrderBy(p => p.Name))
                {
                    Playlists.Add(new PlaylistViewModel(playlist));
                }
            }
            catch
            {
                // Playlists are optional, don't fail startup
            }

            // Load game counts for all platforms in background
            if (platformNames.Count > 0)
            {
                var counts = await Task.Run(() =>
                {
                    var result = new Dictionary<string, int>();
                    System.Threading.Tasks.Parallel.ForEach(platformNames, name =>
                    {
                        try
                        {
                            var games = _gameManager.GetGamesAsync(name).GetAwaiter().GetResult();
                            lock (result) { result[name] = games.Count; }
                        }
                        catch
                        {
                            lock (result) { result[name] = 0; }
                        }
                    });
                    return result;
                });

                foreach (var vm in platformNodeVMs)
                {
                    if (counts.TryGetValue(vm.Name, out var count))
                        vm.GameCount = count;
                }
            }
        }

        private NavigationNodeViewModel ConvertToViewModel(
            GameLauncher.Core.Models.NavigationNode node,
            List<string> platformNames,
            List<NavigationNodeViewModel> platformNodeVMs)
        {
            var vm = new NavigationNodeViewModel
            {
                Name = node.Name,
                NodeType = node.NodeType,
                Platform = node.Platform,
                Playlist = node.Playlist,
                PlaylistId = node.PlaylistId,
            };

            if (node.NodeType == GameLauncher.Core.Models.NavigationNodeType.Platform)
            {
                platformNames.Add(node.Name);
                platformNodeVMs.Add(vm);
            }

            foreach (var child in node.Children)
            {
                vm.Children.Add(ConvertToViewModel(child, platformNames, platformNodeVMs));
            }

            return vm;
        }

        // Comandos
        [RelayCommand]
        private async Task LoadGamesForPlatformAsync(PlatformViewModel? platform)
        {
            if (platform == null) return;

            if (string.IsNullOrEmpty(platform.Name))
            {
                StatusText = "La plataforma seleccionada no tiene un nombre válido";
                Games.Clear();
                ApplyFilter();
                return;
            }

            // Verificar caché primero
            if (_platformCache.TryGetValue(platform.Name, out var cachedGames))
            {
                Games.Clear();
                foreach (var vm in cachedGames)
                    Games.Add(vm);
                ApplyFilter();

                int imgCount = cachedGames.Count(g => g.CoverImage != null);
                StatusText = $"{cachedGames.Count} juegos en {platform.Name} | {imgCount} imágenes";
                CacheStatusText = $"Caché: {platform.Name} ✓";
                return;
            }

            // Sin caché en memoria: cargar desde XML
            IsLoading = true;
            StatusText = $"Cargando juegos de {platform.Name}...";
            CacheStatusText = "";

            try
            {
                var games = await _gameManager.GetGamesAsync(platform.Name);

                // 1. Mostrar juegos inmediatamente (sin imágenes)
                Games.Clear();
                foreach (var game in games.OrderBy(g => g.Title))
                {
                    var gameVM = new GameViewModel(game);
                    gameVM.LaunchRequested += OnGameLaunchRequested;
                    Games.Add(gameVM);
                }

                ApplyFilter();
                IsLoading = false;

                // 2. Intentar cargar desde caché de disco
                var diskCache = await LoadDiskCacheAsync(platform.Name);
                int imgCount;

                if (diskCache != null && diskCache.Count > 0)
                {
                    // Usar rutas cacheadas (solo crear BitmapImages, sin buscar en disco)
                    StatusText = $"{Games.Count} juegos en {platform.Name} - cargando imágenes desde caché...";
                    imgCount = await LoadImagesFromCacheAsync(diskCache);
                    CacheStatusText = $"Caché disco: {platform.Name} ✓";
                }
                else
                {
                    // Sin caché de disco: resolver rutas desde filesystem (lento)
                    StatusText = $"{Games.Count} juegos en {platform.Name} - buscando imágenes...";
                    imgCount = await LoadCoverImagesAsync();

                    // Guardar caché a disco
                    IsCaching = true;
                    CacheStatusText = $"Guardando caché de {platform.Name}...";
                    await SaveDiskCacheAsync(platform.Name);
                    IsCaching = false;
                }

                // 3. Guardar en caché de memoria
                _platformCache[platform.Name] = Games.ToList();
                CacheStatusText = $"Caché: {platform.Name} ✓ ({_platformCache.Count} plataformas en caché)";

                StatusText = $"{Games.Count} juegos en {platform.Name} | {imgCount} imágenes";
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                MessageBox.Show($"Error al cargar juegos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                IsCaching = false;
            }
        }

        [RelayCommand]
        private async Task SearchGamesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ApplyFilter();
                return;
            }

            IsLoading = true;
            StatusText = $"Buscando '{SearchText}'...";

            try
            {
                var platformFilter = SelectedPlatform?.Name;
                var results = await _gameManager.SearchGamesAsync(SearchText, platformFilter);

                Games.Clear();
                foreach (var game in results.OrderBy(g => g.Title))
                {
                    var gameVM = new GameViewModel(game);
                    gameVM.LaunchRequested += OnGameLaunchRequested;
                    Games.Add(gameVM);
                }

                ApplyFilter();
                StatusText = $"{Games.Count} resultados para '{SearchText}'";

                await LoadCoverImagesAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Error en búsqueda: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LaunchSelectedGameAsync()
        {
            if (SelectedGame == null) return;
            await LaunchGameAsync(SelectedGame);
        }

        private async Task LaunchGameAsync(GameViewModel gameVM)
        {
            IsLoading = true;
            StatusText = $"Lanzando {gameVM.Title}...";

            // Minimize window so the emulator has full focus (LaunchBox behaviour)
            var window = System.Windows.Application.Current.MainWindow;
            if (window != null)
                window.WindowState = WindowState.Minimized;

            try
            {
                // Verificar si se puede lanzar
                var (canLaunch, reason) = await _emulatorLauncher.CanLaunchGameAsync(gameVM.Model);
                if (!canLaunch)
                {
                    StatusText = $"No se puede lanzar: {reason}";
                    // Restore before showing dialog
                    if (window != null) { window.WindowState = WindowState.Normal; window.Activate(); }
                    MessageBox.Show($"No se puede lanzar el juego:\n{reason}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Lanzar el juego
                var result = await _emulatorLauncher.LaunchGameAsync(gameVM.Model);

                // Restore window after game exits
                if (window != null) { window.WindowState = WindowState.Normal; window.Activate(); }

                if (result.Success)
                {
                    await _statisticsTracker.RecordPlaySessionAsync(gameVM.Model, result.PlayTimeSeconds);
                    gameVM.RefreshStats();

                    var playTime = TimeSpan.FromSeconds(result.PlayTimeSeconds);
                    StatusText = $"✓ {gameVM.Title} — {playTime:hh\\:mm\\:ss} jugados";

                    // Non-blocking notification in status bar — no modal dialog
                    System.Diagnostics.Debug.WriteLine(
                        $"[Desktop] Session recorded: {gameVM.Title} {result.PlayTimeSeconds}s");
                }
                else
                {
                    StatusText = $"Error al lanzar: {result.ErrorMessage}";
                    MessageBox.Show($"Error al lanzar el juego:\n{result.ErrorMessage}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                if (window != null) { window.WindowState = WindowState.Normal; window.Activate(); }
                MessageBox.Show($"Error inesperado:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleFavorite()
        {
            if (SelectedGame == null) return;
            SelectedGame.ToggleFavoriteCommand.Execute(null);
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
            ApplyFilter();
        }

        [RelayCommand]
        private async Task ShowFavoritesAsync()
        {
            IsLoading = true;
            StatusText = "Cargando favoritos...";
            try
            {
                var favorites = await _gameManager.GetFavoritesAsync();
                Games.Clear();
                foreach (var game in favorites)
                {
                    var gameVM = new GameViewModel(game);
                    gameVM.LaunchRequested += OnGameLaunchRequested;
                    Games.Add(gameVM);
                }
                ApplyFilter();
                StatusText = $"{Games.Count} juegos favoritos";
                await LoadCoverImagesAsync();
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

        [RelayCommand]
        private async Task ShowRecentlyPlayedAsync()
        {
            IsLoading = true;
            StatusText = "Cargando recientes...";
            try
            {
                var recent = await _gameManager.GetRecentlyPlayedAsync();
                Games.Clear();
                foreach (var game in recent)
                {
                    var gameVM = new GameViewModel(game);
                    gameVM.LaunchRequested += OnGameLaunchRequested;
                    Games.Add(gameVM);
                }
                ApplyFilter();
                StatusText = $"{Games.Count} juegos recientes";
                await LoadCoverImagesAsync();
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

        [RelayCommand]
        private async Task ShowCompletedAsync()
        {
            IsLoading = true;
            StatusText = "Cargando completados...";
            try
            {
                var completed = await _gameManager.GetCompletedAsync();
                Games.Clear();
                foreach (var game in completed)
                {
                    var gameVM = new GameViewModel(game);
                    gameVM.LaunchRequested += OnGameLaunchRequested;
                    Games.Add(gameVM);
                }
                ApplyFilter();
                StatusText = $"{Games.Count} juegos completados";
                await LoadCoverImagesAsync();
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

        [RelayCommand]
        private async Task SelectNavigationNodeAsync(NavigationNodeViewModel? node)
        {
            if (node == null) return;

            if (node.NodeType == GameLauncher.Core.Models.NavigationNodeType.Platform)
            {
                // Load games for this platform
                if (node.Platform != null)
                {
                    var platformVM = new PlatformViewModel(node.Platform);
                    SelectedPlatform = platformVM;
                }
            }
            else if (node.NodeType == GameLauncher.Core.Models.NavigationNodeType.Playlist && node.Playlist != null)
            {
                // Load games for this playlist
                var playlistVM = new PlaylistViewModel(node.Playlist);
                await LoadGamesForPlaylistAsync(playlistVM);
            }
        }

        [RelayCommand]
        private async Task LoadGamesForPlaylistAsync(PlaylistViewModel? playlist)
        {
            if (playlist == null) return;

            IsLoading = true;
            StatusText = $"Cargando playlist '{playlist.Name}'...";
            try
            {
                var playlistGames = await _playlistManager.GetPlaylistGamesAsync(playlist.Name);
                if (playlistGames.Count == 0)
                {
                    Games.Clear();
                    ApplyFilter();
                    StatusText = $"La playlist '{playlist.Name}' est\u00e1 vac\u00eda";
                    return;
                }

                // BUG-05 FIX: Build a lookup of all game IDs across ALL platforms in one pass
                // instead of calling GetGameByIdAsync() per entry (which iterates all platforms each time).
                StatusText = $"Indexando biblioteca para playlist '{playlist.Name}'...";
                var neededIds = new HashSet<string>(
                    playlistGames
                        .Where(pg => !string.IsNullOrWhiteSpace(pg.GameId))
                        .Select(pg => pg.GameId),
                    StringComparer.OrdinalIgnoreCase);

                var gameById = await Task.Run(() =>
                {
                    var lookup = new Dictionary<string, Game>(StringComparer.OrdinalIgnoreCase);
                    var platforms = _cacheManager.GetPlatforms();
                    foreach (var platform in platforms)
                    {
                        if (lookup.Count >= neededIds.Count) break; // found all needed
                        try
                        {
                            var games = _cacheManager.GetGames(platform.Name);
                            foreach (var g in games)
                                if (neededIds.Contains(g.ID) && !lookup.ContainsKey(g.ID))
                                    lookup[g.ID] = g;
                        }
                        catch { /* skip unreadable platforms */ }
                    }
                    return lookup;
                });

                Games.Clear();
                // Preserve playlist order
                foreach (var pg in playlistGames)
                {
                    if (string.IsNullOrWhiteSpace(pg.GameId)) continue;
                    if (gameById.TryGetValue(pg.GameId, out var game))
                    {
                        var gameVM = new GameViewModel(game);
                        gameVM.LaunchRequested += OnGameLaunchRequested;
                        Games.Add(gameVM);
                    }
                }

                ApplyFilter();
                StatusText = $"{Games.Count} juegos en playlist '{playlist.Name}'";
                await LoadCoverImagesAsync();
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

        [RelayCommand]
        private void ShowPlatforms()
        {
            ShowPlatformsSection = true;
            ShowPlaylistsSection = false;
        }

        [RelayCommand]
        private void ShowPlaylists()
        {
            ShowPlatformsSection = false;
            ShowPlaylistsSection = true;
        }

        /// <summary>
        /// Resuelve rutas en background, crea BitmapImage en UI thread, progresivo por lotes.
        /// Retorna la cantidad de imágenes cargadas.
        /// </summary>
        private async Task<int> LoadCoverImagesAsync()
        {
            var games = FilteredGames.ToList();
            const int batchSize = 10;
            int loaded = 0;

            for (int i = 0; i < games.Count; i += batchSize)
            {
                var batch = games.Skip(i).Take(batchSize).ToList();

                // Resolver rutas de archivo en background (thread-safe, solo strings)
                var paths = await Task.Run(() =>
                {
                    return batch.Select(g => new { Game = g, Path = g.ResolveCoverImagePath() }).ToList();
                });

                // Crear BitmapImage en UI thread y asignar
                foreach (var item in paths)
                {
                    if (item.Path != null)
                    {
                        item.Game.ResolvedImagePath = item.Path;
                        var bitmap = GameViewModel.CreateBitmapFromPath(item.Path);
                        if (bitmap != null)
                        {
                            item.Game.CoverImage = bitmap;
                            loaded++;
                        }
                    }
                }

                // Actualizar progreso cada lote
                StatusText = $"Cargando imágenes... {loaded} de {games.Count}";
            }

            return loaded;
        }

        // --- Caché de disco: guardar/cargar rutas de imágenes como JSON ---

        private static string GetDiskCacheFilePath(string platformName)
        {
            // Sanitizar nombre de plataforma para nombre de archivo
            string safeName = string.Join("_", platformName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_diskCachePath, safeName + ".json");
        }

        private async Task SaveDiskCacheAsync(string platformName)
        {
            try
            {
                Directory.CreateDirectory(_diskCachePath);

                var cache = new Dictionary<string, string?>();
                foreach (var game in FilteredGames)
                {
                    cache[game.ID] = game.ResolvedImagePath;
                }

                string json = JsonSerializer.Serialize(cache);
                await File.WriteAllTextAsync(GetDiskCacheFilePath(platformName), json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving disk cache for {platformName}: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, string?>?> LoadDiskCacheAsync(string platformName)
        {
            try
            {
                string filePath = GetDiskCacheFilePath(platformName);
                if (!File.Exists(filePath))
                    return null;

                string json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<Dictionary<string, string?>>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Carga imágenes usando rutas cacheadas en disco.
        /// Procesa en lotes para que el UI sea responsivo con 1000+ juegos.
        /// </summary>
        private async Task<int> LoadImagesFromCacheAsync(Dictionary<string, string?> diskCache)
        {
            var games = FilteredGames.ToList();
            const int batchSize = 50;
            int loaded = 0;

            for (int i = 0; i < games.Count; i += batchSize)
            {
                var batch = games.Skip(i).Take(batchSize).ToList();

                // Crear BitmapImages en paralelo en background (Freeze las hace thread-safe)
                var results = await Task.Run(() =>
                {
                    return batch.AsParallel().Select(g =>
                    {
                        string? cachedPath = diskCache.TryGetValue(g.ID, out var p) ? p : null;
                        BitmapImage? bitmap = null;
                        if (cachedPath != null)
                            bitmap = GameViewModel.CreateBitmapFromPath(cachedPath);
                        return (Game: g, Path: cachedPath, Bitmap: bitmap);
                    }).ToList();
                });

                // Asignar en UI thread
                foreach (var item in results)
                {
                    if (item.Bitmap != null)
                    {
                        item.Game.ResolvedImagePath = item.Path;
                        item.Game.CoverImage = item.Bitmap;
                        loaded++;
                    }
                }

                StatusText = $"Cargando imágenes desde caché... {loaded} de {games.Count}";

                // Ceder al UI thread para que renderice
                await Task.Delay(1);
            }

            return loaded;
        }


        // Métodos auxiliares
        private void ApplyFilter()
        {
            FilteredGames.Clear();

            IEnumerable<GameViewModel> filtered = Games;

            // 1. Text search
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(g =>
                    g.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (g.Developer?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (g.Genre?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (g.Model.Publisher?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (g.Model.Series?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // 2. Active filter preset
            filtered = ActiveFilter switch
            {
                GameFilter.Favorites      => filtered.Where(g => g.Model.Favorite),
                GameFilter.Completed      => filtered.Where(g => g.Model.Completed),
                GameFilter.Installed      => filtered.Where(g => g.Model.Installed),
                GameFilter.RecentlyPlayed => filtered.Where(g => g.Model.PlayCount > 0).OrderByDescending(g => g.Model.LastPlayed),
                GameFilter.NeverPlayed    => filtered.Where(g => g.Model.PlayCount == 0),
                GameFilter.Broken         => filtered.Where(g => g.Model.Broken),
                _                         => filtered
            };

            // 3. Sort
            filtered = (CurrentSortField, SortDescending) switch
            {
                (SortField.Title,       false) => filtered.OrderBy(g => g.Title),
                (SortField.Title,       true)  => filtered.OrderByDescending(g => g.Title),
                (SortField.ReleaseDate, false) => filtered.OrderBy(g => g.Model.ReleaseDate ?? DateTime.MaxValue),
                (SortField.ReleaseDate, true)  => filtered.OrderByDescending(g => g.Model.ReleaseDate ?? DateTime.MinValue),
                (SortField.LastPlayed,  false) => filtered.OrderByDescending(g => g.Model.LastPlayed),
                (SortField.LastPlayed,  true)  => filtered.OrderBy(g => g.Model.LastPlayed),
                (SortField.PlayCount,   false) => filtered.OrderByDescending(g => g.Model.PlayCount),
                (SortField.PlayCount,   true)  => filtered.OrderBy(g => g.Model.PlayCount),
                (SortField.PlayTime,    false) => filtered.OrderByDescending(g => g.Model.PlayTime),
                (SortField.PlayTime,    true)  => filtered.OrderBy(g => g.Model.PlayTime),
                (SortField.StarRating,  false) => filtered.OrderByDescending(g => g.Model.StarRating),
                (SortField.StarRating,  true)  => filtered.OrderBy(g => g.Model.StarRating),
                (SortField.Developer,   false) => filtered.OrderBy(g => g.Developer),
                (SortField.Developer,   true)  => filtered.OrderByDescending(g => g.Developer),
                (SortField.Genre,       false) => filtered.OrderBy(g => g.Genre),
                (SortField.Genre,       true)  => filtered.OrderByDescending(g => g.Genre),
                (SortField.DateAdded,   false) => filtered.OrderByDescending(g => g.Model.DateAdded),
                (SortField.DateAdded,   true)  => filtered.OrderBy(g => g.Model.DateAdded),
                _                              => filtered.OrderBy(g => g.Title)
            };

            foreach (var game in filtered)
                FilteredGames.Add(game);

            var arrow = SortDescending ? "▼" : "▲";
            var filterLabel = ActiveFilter == GameFilter.All ? "" : $" [{ActiveFilter}]";
            SortStatusText = $"{FilteredGames.Count} juegos — {CurrentSortField} {arrow}{filterLabel}";
        }

        [RelayCommand]
        private void SortBy(SortField field)
        {
            if (CurrentSortField == field)
                SortDescending = !SortDescending;
            else
            {
                CurrentSortField = field;
                SortDescending = false;
            }
            ApplyFilter();
        }

        [RelayCommand]
        private void ToggleSortDirection()
        {
            SortDescending = !SortDescending;
            ApplyFilter();
        }

        [RelayCommand]
        private void SetFilter(GameFilter filter)
        {
            ActiveFilter = filter;
            ApplyFilter();
        }

        /// <summary>
        /// Searches across ALL platforms using the in-memory cache.
        /// Results replace the current FilteredGames list.
        /// </summary>
        [RelayCommand]
        private async Task SearchAllPlatformsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            IsLoading = true;
            IsGlobalSearchActive = true;
            StatusText = $"Buscando \"{SearchText}\" en todas las plataformas...";

            try
            {
                var results = await _gameManager.SearchAllPlatformsAsync(SearchText);

                Games.Clear();
                FilteredGames.Clear();

                foreach (var game in results)
                {
                    var vm = new GameViewModel(game);
                    vm.LaunchRequested += OnGameLaunchRequested;
                    Games.Add(vm);
                    FilteredGames.Add(vm);
                }

                StatusText = $"{results.Count} resultados para \"{SearchText}\" en todas las plataformas";

                // Load images for results
                _ = LoadCoverImagesAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Error en búsqueda global: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ClearGlobalSearch()
        {
            IsGlobalSearchActive = false;
            SearchText = string.Empty;
            Games.Clear();
            FilteredGames.Clear();
            StatusText = "Búsqueda borrada. Selecciona una plataforma para ver juegos.";
        }

        private void OnGameLaunchRequested(object? sender, EventArgs e)
        {
            if (sender is GameViewModel gameVM)
            {
                _ = LaunchGameAsync(gameVM);
            }
        }

        // Cambios de propiedad
        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        partial void OnSelectedPlatformChanged(PlatformViewModel? value)
        {
            if (value != null)
            {
                _ = LoadGamesForPlatformAsync(value);
            }
        }

        partial void OnSelectedGameChanged(GameViewModel? value)
        {
            if (value == null)
            {
                SelectedGameVideoUri = null;
                return;
            }

            var game = value;
            _ = Task.Run(() =>
            {
                var videoPath = game.ResolveVideoPath();
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (SelectedGame == game)
                    {
                        SelectedGameVideoUri = videoPath != null ? new Uri(videoPath, UriKind.Absolute) : null;
                    }
                });
            });
        }
        [RelayCommand]
        private async Task ExportToAndroidAsync()
        {
            var selectedGames = FilteredGames.Where(g => g.IsSelected).Select(g => g.Model).ToList();
            if (selectedGames.Count == 0)
            {
                System.Windows.MessageBox.Show("Por favor, selecciona al menos un juego (mantén presionado Ctrl y haz clic en los juegos) o exporta una lista completa filtrando primero.", "Exportar a Android", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Title = "Guardar paquete de exportación para Android",
                Filter = "Archivos ZIP (*.zip)|*.zip",
                FileName = $"GameLauncher_AndroidSync_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusText = "Exportando juegos para Android...";

                try
                {
                    await Task.Run(async () =>
                    {
                        await _exportService.ExportGamesToZipAsync(selectedGames, saveFileDialog.FileName, (msg) =>
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() => StatusText = msg);
                        });
                    });

                    // Start local sync server
                    _syncServer.StartServer(saveFileDialog.FileName, 8080);
                    
                    var localIp = GetLocalIpAddress();
                    var qrMessage = $"Exportación completada a:\n{saveFileDialog.FileName}\n\nPara transferir por Wi-Fi, abre GameLauncher en tu Android y entra a Importar.\nServidor Activo en: http://{localIp}:8080";

                    System.Windows.MessageBox.Show(qrMessage, "Exportación Exitosa", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    StatusText = "Servidor Wi-Fi activo en el puerto 8080";
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error durante la exportación:\n{ex.Message}", "Error de Exportación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusText = "Error al exportar a Android";
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private string GetLocalIpAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
    }
}
