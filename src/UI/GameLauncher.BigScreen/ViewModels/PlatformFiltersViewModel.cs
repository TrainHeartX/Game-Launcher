using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLauncher.BigScreen.Navigation;
using GameLauncher.Core.Helpers;
using GameLauncher.Core.Models;
using GameLauncher.Infrastructure.Services;
using GameLauncher.BigScreen.Views;
using System.Windows.Controls;

namespace GameLauncher.BigScreen.ViewModels
{
    /// <summary>
    /// Item wrapper for navigation node display in BigScreen.
    /// Supports categories, platforms, and playlists.
    /// </summary>
    public partial class NavigationItem : ObservableObject
    {
        public NavigationNode Node { get; }
        public string Name => Node.Name;
        public NavigationNodeType NodeType => Node.NodeType;

        [ObservableProperty]
        private int _gameCount;

        public string Icon => NodeType switch
        {
            NavigationNodeType.Category => "\U0001F4C1",
            NavigationNodeType.Platform => "\U0001F3AE",
            NavigationNodeType.Playlist => "\U0001F4CB",
            _ => ""
        };

        [ObservableProperty]
        private BitmapImage? _logoImage;

        public int ChildrenCount => Node.Children.Count;

        public NavigationItem(NavigationNode node)
        {
            Node = node;
        }
    }

    /// <summary>
    /// ViewModel para la vista de navegación (drill-down) en BigScreen.
    /// Soporta la jerarquía completa del árbol de navegación LaunchBox.
    /// </summary>
    public partial class PlatformFiltersViewModel : ObservableObject, INavigationAware
    {
        private readonly IPlatformManager _platformManager;
        private readonly IGameManager _gameManager;
        private readonly IPlaylistManager? _playlistManager;
        private Frame? _navigationFrame;

        // Drill-down navigation stack
        private readonly Stack<(List<NavigationNode> Nodes, string Title)> _navigationStack = new();
        private List<NavigationNode> _currentNodes = new();

        [ObservableProperty]
        private ObservableCollection<NavigationItem> _items = new();

        [ObservableProperty]
        private NavigationItem? _selectedItem;

        [ObservableProperty]
        private Uri? _platformVideoUri;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private string _currentTitle = "SELECCIONA UNA CATEGORÍA";

        [ObservableProperty]
        private string? _selectedItemNotes;

        [ObservableProperty]
        private string? _selectedItemImagePath;

        // Saga/Playlist preview properties
        [ObservableProperty]
        private bool _isPlaylistSelected;

        [ObservableProperty]
        private string? _sagaLogoPath;

        [ObservableProperty]
        private ObservableCollection<SagaGamePreview> _sagaGames = new();

        [ObservableProperty]
        private int _sagaGameCount;

        [ObservableProperty]
        private string? _sagaYearRange;

        [ObservableProperty]
        private string? _sagaCompletionStatus;

        [ObservableProperty]
        private bool _isSagaLoading;

        [ObservableProperty]
        private string? _sagaDescription;

        [ObservableProperty]
        private string? _sagaCollectionStatus;

        // Parsed Notes metadata
        [ObservableProperty]
        private string? _sagaStatus;

        [ObservableProperty]
        private string? _sagaReviewDate;

        [ObservableProperty]
        private int _sagaMissingCount;

        [ObservableProperty]
        private bool _hasMissingGames;

        [ObservableProperty]
        private ObservableCollection<string> _sagaMissingGames = new();

        [ObservableProperty]
        private string? _sagaLastGame;

        public bool CanGoBack => _navigationStack.Count > 0;

        public PlatformFiltersViewModel(
            IPlatformManager platformManager,
            IGameManager gameManager,
            IPlaylistManager? playlistManager = null)
        {
            _platformManager = platformManager ?? throw new ArgumentNullException(nameof(platformManager));
            _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
            _playlistManager = playlistManager;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            StatusText = "Cargando navegación...";

            try
            {
                // Same tree as Desktop's PlatformCategories — no modifications
                var tree = await _platformManager.GetNavigationTreeAsync();

                _currentNodes = tree;
                _navigationStack.Clear();
                CurrentTitle = "SELECCIONA UNA CATEGORÍA";
                DisplayCurrentLevel();
                await LoadGameCountsAsync();
                StatusText = $"{Items.Count} categorías disponibles";
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

        private void DisplayCurrentLevel()
        {
            Items.Clear();
            foreach (var node in _currentNodes)
            {
                Items.Add(new NavigationItem(node));
            }
            if (Items.Count > 0)
                SelectedItem = Items[0];
            _ = LoadPlaylistLogosAsync();
        }

        private async Task LoadGameCountsAsync()
        {
            var platformItems = Items.Where(i => i.NodeType == NavigationNodeType.Platform).ToList();
            if (platformItems.Count == 0) return;

            var platformNames = platformItems.Select(i => i.Name).ToList();
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

            foreach (var item in platformItems)
            {
                if (counts.TryGetValue(item.Name, out var count))
                    item.GameCount = count;
            }
        }

        partial void OnSelectedItemChanged(NavigationItem? value)
        {
            if (value == null)
            {
                PlatformVideoUri = null;
                SelectedItemNotes = null;
                SelectedItemImagePath = null;
                IsPlaylistSelected = false;
                ClearSagaData();
                return;
            }

            SelectedItemNotes = null;
            SelectedItemImagePath = null;

            if (value.NodeType == NavigationNodeType.Platform)
            {
                IsPlaylistSelected = false;
                ClearSagaData();
                ResolvePlatformVideo(value.Name);
                ResolvePlatformImage(value.Name);
            }
            else if (value.NodeType == NavigationNodeType.Playlist)
            {
                IsPlaylistSelected = true;
                var notes = value.Node.Playlist?.Notes;
                SelectedItemNotes = string.IsNullOrWhiteSpace(notes) ? null : notes;

                // Parse Notes metadata and set clean description
                var parsed = ParsePlaylistNotes(notes);
                SagaStatus = parsed.Status;
                SagaReviewDate = parsed.ReviewDate;
                SagaMissingCount = parsed.MissingCount;
                HasMissingGames = parsed.MissingCount > 0;
                SagaMissingGames.Clear();
                foreach (var game in parsed.MissingGames)
                    SagaMissingGames.Add(game);
                SagaLastGame = parsed.LastGame;
                SagaDescription = parsed.Description;

                // SagaCollectionStatus from Notes (will be overridden by LoadSagaGamesAsync if no Notes status)
                SagaCollectionStatus = parsed.Status;

                ResolvePlaylistVideo(value.Name, value.Node.Playlist);
                ResolvePlaylistImage(value.Name, value.Node.Playlist);
                ResolveSagaLogo(value.Name, value.Node.Playlist);
                _ = LoadSagaGamesAsync(value.Name, value.Node.Playlist);
            }
            else if (value.NodeType == NavigationNodeType.Category)
            {
                IsPlaylistSelected = false;
                ClearSagaData();
                PlatformVideoUri = null;
                ResolveCategoryImage(value.Name);
            }
            else
            {
                IsPlaylistSelected = false;
                ClearSagaData();
                PlatformVideoUri = null;
            }
        }

        private void ResolveCategoryImage(string categoryName)
        {
            Task.Run(() =>
            {
                var launchBoxPath = VideoPathResolver.LaunchBoxPath;
                if (string.IsNullOrEmpty(launchBoxPath))
                    return null;

                string[] subfolders = { "Clear Logo", "Fanart", "Banner" };
                foreach (var subfolder in subfolders)
                {
                    var dir = Path.Combine(launchBoxPath, "Images", "Platform Categories", categoryName, subfolder);
                    var image = FileNameHelper.FindFirstImage(dir);
                    if (image != null) return image;
                }
                return null;
            }).ContinueWith(t =>
            {
                var path = t.Result;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedItem?.Name == categoryName)
                        SelectedItemImagePath = path;
                });
            });
        }

        private void ResolvePlatformVideo(string platformName)
        {
            Task.Run(() =>
            {
                var launchBoxPath = VideoPathResolver.LaunchBoxPath;
                if (string.IsNullOrEmpty(launchBoxPath))
                    return null;

                var videosDir = Path.Combine(launchBoxPath, "Videos", "Platforms");
                if (!Directory.Exists(videosDir))
                    return null;

                var videoFile = Path.Combine(videosDir, $"{platformName}.mp4");
                if (File.Exists(videoFile))
                    return videoFile;

                return null;
            }).ContinueWith(t =>
            {
                var path = t.Result;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedItem?.Name == platformName)
                    {
                        PlatformVideoUri = path != null ? new Uri(path) : null;
                    }
                });
            });
        }

        private void ResolvePlaylistVideo(string playlistName, Playlist? playlist)
        {
            Task.Run(() =>
            {
                var launchBoxPath = VideoPathResolver.LaunchBoxPath;
                if (string.IsNullOrEmpty(launchBoxPath))
                    return null;

                // 1. Check Playlist.VideoPath (absolute or relative)
                if (!string.IsNullOrEmpty(playlist?.VideoPath))
                {
                    var videoPath = playlist.VideoPath;
                    if (!Path.IsPathRooted(videoPath))
                        videoPath = Path.Combine(launchBoxPath, videoPath);
                    if (File.Exists(videoPath))
                        return videoPath;
                }

                // 2. Check standard LaunchBox location: Videos/Playlists/{Name}.mp4
                string[] extensions = { ".mp4", ".wmv", ".avi", ".mkv" };
                var playlistsVideoDir = Path.Combine(launchBoxPath, "Videos", "Playlists");
                if (Directory.Exists(playlistsVideoDir))
                {
                    foreach (var ext in extensions)
                    {
                        var videoFile = Path.Combine(playlistsVideoDir, $"{playlistName}{ext}");
                        if (File.Exists(videoFile))
                            return videoFile;
                    }
                }

                return null;
            }).ContinueWith(t =>
            {
                var path = t.Result;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedItem?.Name == playlistName)
                    {
                        PlatformVideoUri = path != null ? new Uri(path) : null;
                    }
                });
            });
        }

        private void ResolvePlatformImage(string platformName)
        {
            Task.Run(() =>
            {
                var launchBoxPath = VideoPathResolver.LaunchBoxPath;
                if (string.IsNullOrEmpty(launchBoxPath))
                    return null;

                // Clear Logo first, then Fanart, then Banner
                string[] subfolders = { "Clear Logo", "Fanart", "Banner" };
                foreach (var subfolder in subfolders)
                {
                    var dir = Path.Combine(launchBoxPath, "Images", "Platforms", platformName, subfolder);
                    var image = FileNameHelper.FindFirstImage(dir);
                    if (image != null) return image;
                }
                return null;
            }).ContinueWith(t =>
            {
                var path = t.Result;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedItem?.Name == platformName)
                        SelectedItemImagePath = path;
                });
            });
        }

        private void ResolvePlaylistImage(string playlistName, Playlist? playlist)
        {
            Task.Run(() =>
            {
                var launchBoxPath = VideoPathResolver.LaunchBoxPath;
                if (string.IsNullOrEmpty(launchBoxPath))
                    return null;

                // Check custom folder from Playlist model first
                string[] subfolders = { "Clear Logo", "Fanart", "Banner" };

                // Custom ClearLogoImagesFolder
                if (!string.IsNullOrEmpty(playlist?.ClearLogoImagesFolder))
                {
                    var customDir = playlist.ClearLogoImagesFolder;
                    if (!Path.IsPathRooted(customDir))
                        customDir = Path.Combine(launchBoxPath, customDir);
                    var image = FileNameHelper.FindFirstImage(customDir);
                    if (image != null) return image;
                }

                // Standard LaunchBox location: Images/Playlists/{Name}/{subfolder}
                foreach (var subfolder in subfolders)
                {
                    var dir = Path.Combine(launchBoxPath, "Images", "Playlists", playlistName, subfolder);
                    var image = FileNameHelper.FindFirstImage(dir);
                    if (image != null) return image;
                }
                return null;
            }).ContinueWith(t =>
            {
                var path = t.Result;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedItem?.Name == playlistName)
                        SelectedItemImagePath = path;
                });
            });
        }

        private void ClearSagaData()
        {
            SagaLogoPath = null;
            SagaGames.Clear();
            SagaGameCount = 0;
            SagaYearRange = null;
            SagaCompletionStatus = null;
            SagaDescription = null;
            SagaCollectionStatus = null;
            SagaStatus = null;
            SagaReviewDate = null;
            SagaMissingCount = 0;
            HasMissingGames = false;
            SagaMissingGames.Clear();
            SagaLastGame = null;
            IsSagaLoading = false;
        }

        private void ResolveSagaLogo(string playlistName, Playlist? playlist)
        {
            Task.Run(() =>
            {
                var launchBoxPath = VideoPathResolver.LaunchBoxPath;
                if (string.IsNullOrEmpty(launchBoxPath))
                    return null;

                // Priority: ClearLogoImagesFolder → Clear Logo → Banner → Fanart
                if (!string.IsNullOrEmpty(playlist?.ClearLogoImagesFolder))
                {
                    var customDir = playlist.ClearLogoImagesFolder;
                    if (!Path.IsPathRooted(customDir))
                        customDir = Path.Combine(launchBoxPath, customDir);
                    var image = FileNameHelper.FindFirstImage(customDir);
                    if (image != null) return image;
                }

                string[] subfolders = { "Clear Logo", "Banner", "Fanart" };
                foreach (var subfolder in subfolders)
                {
                    var dir = Path.Combine(launchBoxPath, "Images", "Playlists", playlistName, subfolder);
                    var image = FileNameHelper.FindFirstImage(dir);
                    if (image != null) return image;
                }
                return null;
            }).ContinueWith(t =>
            {
                var path = t.Result;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedItem?.Name == playlistName)
                        SagaLogoPath = path;
                });
            });
        }

        private async Task LoadSagaGamesAsync(string playlistName, Playlist? playlist)
        {
            if (_playlistManager == null || playlist == null) return;

            IsSagaLoading = true;

            try
            {
                var playlistGames = await _playlistManager.GetPlaylistGamesAsync(playlistName);

                // Race condition guard
                if (SelectedItem?.Name != playlistName) return;

                var sagaPreviews = new List<SagaGamePreview>();
                foreach (var pg in playlistGames)
                {
                    var game = await _gameManager.GetGameByIdAsync(pg.GameId);
                    if (game == null) continue;

                    var preview = new SagaGamePreview
                    {
                        Name = game.Title,
                        Year = game.ReleaseDate?.Year.ToString() ?? "",
                        Platform = game.Platform ?? "",
                        Completed = game.Completed,
                        CoverPath = ResolveCoverImagePath(game)
                    };
                    sagaPreviews.Add(preview);
                }

                // Race condition guard
                if (SelectedItem?.Name != playlistName) return;

                // Sort by year ascending (oldest first), then by name
                sagaPreviews = sagaPreviews
                    .OrderBy(g => string.IsNullOrEmpty(g.Year) ? "9999" : g.Year)
                    .ThenBy(g => g.Name)
                    .ToList();

                // Calculate stats
                var years = sagaPreviews
                    .Where(g => !string.IsNullOrEmpty(g.Year))
                    .Select(g => g.Year)
                    .OrderBy(y => y)
                    .ToList();

                var completedCount = sagaPreviews.Count(g => g.Completed);

                SagaGameCount = sagaPreviews.Count;
                SagaYearRange = years.Count >= 2
                    ? $"{years.First()} - {years.Last()}"
                    : years.Count == 1 ? years.First() : null;
                SagaCompletionStatus = $"{completedCount}/{sagaPreviews.Count} completados";

                // Use parsed Notes status if available, otherwise fallback to computed
                if (string.IsNullOrEmpty(SagaCollectionStatus))
                {
                    SagaCollectionStatus = completedCount == sagaPreviews.Count && sagaPreviews.Count > 0
                        ? "SAGA COMPLETA"
                        : null;
                }

                SagaGames.Clear();
                foreach (var preview in sagaPreviews)
                    SagaGames.Add(preview);

                // Load cover images and platform logos in background
                _ = LoadSagaCoverImagesAsync(playlistName, sagaPreviews);
                _ = LoadSagaPlatformImagesAsync(playlistName, sagaPreviews);
            }
            catch
            {
                // Silently handle errors
            }
            finally
            {
                if (SelectedItem?.Name == playlistName)
                    IsSagaLoading = false;
            }
        }

        private string? ResolveCoverImagePath(Game game)
        {
            var launchBoxPath = VideoPathResolver.LaunchBoxPath;
            if (string.IsNullOrEmpty(launchBoxPath) || string.IsNullOrEmpty(game.Platform) || string.IsNullOrEmpty(game.Title))
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
            string sanitizedTitle = FileNameHelper.SanitizeForLaunchBox(game.Title);
            string[] suffixes = { "-01", "-02", "-03", "" };
            string[] extensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

            foreach (var imageType in imageTypeFolders)
            {
                string typeFolder = Path.Combine(launchBoxPath, "Images", game.Platform, imageType);
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

        // SanitizeFileName removed — use FileNameHelper.SanitizeForLaunchBox() instead

        private async Task LoadSagaCoverImagesAsync(string playlistName, List<SagaGamePreview> previews)
        {
            const int batchSize = 10;
            for (int i = 0; i < previews.Count; i += batchSize)
            {
                if (SelectedItem?.Name != playlistName) return;

                var batch = previews.Skip(i).Take(batchSize).ToList();
                var tasks = batch.Select(preview => Task.Run(() =>
                {
                    if (preview.CoverPath == null) return (Preview: preview, Bitmap: (BitmapImage?)null);
                    var bitmap = GameItem.CreateBitmapFromPath(preview.CoverPath, 120);
                    return (Preview: preview, Bitmap: bitmap);
                })).ToList();

                var results = await Task.WhenAll(tasks);

                if (SelectedItem?.Name != playlistName) return;

                foreach (var result in results)
                {
                    if (result.Bitmap != null)
                        result.Preview.CoverImage = result.Bitmap;
                }
            }
        }

        private async Task LoadSagaPlatformImagesAsync(string playlistName, List<SagaGamePreview> previews)
        {
            // Resolve and cache platform Clear Logo images (one per unique platform)
            var platformCache = await Task.Run(() =>
            {
                var cache = new Dictionary<string, BitmapImage?>();
                var launchBoxPath = VideoPathResolver.LaunchBoxPath;
                if (string.IsNullOrEmpty(launchBoxPath)) return cache;

                foreach (var preview in previews)
                {
                    if (string.IsNullOrEmpty(preview.Platform) || cache.ContainsKey(preview.Platform))
                        continue;

                    string[] subfolders = { "Clear Logo", "Banner" };
                    string? imagePath = null;
                    foreach (var subfolder in subfolders)
                    {
                        var dir = Path.Combine(launchBoxPath, "Images", "Platforms", preview.Platform, subfolder);
                        imagePath = FileNameHelper.FindFirstImage(dir);
                        if (imagePath != null) break;
                    }

                    cache[preview.Platform] = imagePath != null
                        ? GameItem.CreateBitmapFromPath(imagePath, 80)
                        : null;
                }
                return cache;
            });

            if (SelectedItem?.Name != playlistName) return;

            foreach (var preview in previews)
            {
                if (!string.IsNullOrEmpty(preview.Platform)
                    && platformCache.TryGetValue(preview.Platform, out var img)
                    && img != null)
                {
                    preview.PlatformImage = img;
                }
            }
        }

        private record SagaNotesData(
            string? Status,
            string? ReviewDate,
            int MissingCount,
            List<string> MissingGames,
            string? LastGame,
            string? Description
        );

        private static SagaNotesData ParsePlaylistNotes(string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return new SagaNotesData(null, null, 0, new List<string>(), null, null);

            string? status = null;
            string? reviewDate = null;
            int missingCount = 0;
            var missingGames = new List<string>();
            string? lastGame = null;
            string? description = null;

            var lines = notes.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

            // 1. Status: [SAGA COMPLETA], [SEMI COMPLETA], [SAGA PARCIAL]
            var statusMatch = Regex.Match(notes, @"\[(SAGA COMPLETA|SEMI COMPLETA|SAGA PARCIAL)\]");
            if (statusMatch.Success)
                status = statusMatch.Groups[1].Value;

            // 2. Review date: [REVISADO DD/MM/YYYY]
            var reviewMatch = Regex.Match(notes, @"\[REVISADO\s+(\d{2}/\d{2}/\d{4})\]");
            if (reviewMatch.Success)
                reviewDate = reviewMatch.Groups[1].Value;

            // 3. Missing count: JUEGOS FALTANTES: N
            var missingCountMatch = Regex.Match(notes, @"JUEGOS FALTANTES:\s*(\d+)", RegexOptions.IgnoreCase);
            if (missingCountMatch.Success)
                missingCount = int.Parse(missingCountMatch.Groups[1].Value);

            // 4. Missing games: lines between JUEGOS FALTANTES and ULTIMO
            // 5. Last game: line after ULTIMO:
            bool inMissingSection = false;
            bool inUltimoSection = false;
            int lastMetadataLine = -1;
            var descriptionLines = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].Trim();

                // Track metadata lines
                if (Regex.IsMatch(trimmed, @"^\[(SAGA COMPLETA|SEMI COMPLETA|SAGA PARCIAL)\]"))
                {
                    lastMetadataLine = i;
                    continue;
                }
                if (Regex.IsMatch(trimmed, @"^\[REVISADO\s+\d{2}/\d{2}/\d{4}\]"))
                {
                    lastMetadataLine = i;
                    continue;
                }
                if (Regex.IsMatch(trimmed, @"^JUEGOS FALTANTES:\s*\d+", RegexOptions.IgnoreCase))
                {
                    lastMetadataLine = i;
                    inMissingSection = true;
                    inUltimoSection = false;
                    continue;
                }
                if (Regex.IsMatch(trimmed, @"^ULTIMO:", RegexOptions.IgnoreCase))
                {
                    lastMetadataLine = i;
                    inMissingSection = false;
                    inUltimoSection = true;
                    continue;
                }
                if (Regex.IsMatch(trimmed, @"^PLUG AND PLAY:", RegexOptions.IgnoreCase))
                {
                    lastMetadataLine = i;
                    inMissingSection = false;
                    inUltimoSection = false;
                    continue;
                }

                if (inMissingSection)
                {
                    if (string.IsNullOrWhiteSpace(trimmed))
                    {
                        // Empty line ends missing section if no ULTIMO follows
                        inMissingSection = false;
                    }
                    else
                    {
                        missingGames.Add(trimmed);
                        lastMetadataLine = i;
                    }
                    continue;
                }

                if (inUltimoSection)
                {
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        lastGame = trimmed;
                        lastMetadataLine = i;
                    }
                    inUltimoSection = false;
                    continue;
                }
            }

            // Description: everything after metadata section (skip leading blank lines)
            bool foundContent = false;
            for (int i = lastMetadataLine + 1; i < lines.Length; i++)
            {
                var trimmed = lines[i].Trim();
                if (!foundContent && string.IsNullOrWhiteSpace(trimmed))
                    continue;
                foundContent = true;
                descriptionLines.Add(lines[i]);
            }

            if (descriptionLines.Count > 0)
                description = string.Join(Environment.NewLine, descriptionLines).Trim();

            if (string.IsNullOrWhiteSpace(description))
                description = null;

            return new SagaNotesData(status, reviewDate, missingCount, missingGames, lastGame, description);
        }

        private async Task LoadPlaylistLogosAsync()
        {
            var playlistItems = Items.Where(i => i.NodeType == NavigationNodeType.Playlist).ToList();
            if (playlistItems.Count == 0) return;

            var itemsSnapshot = playlistItems.ToList();

            await Task.Run(() =>
            {
                var launchBoxPath = VideoPathResolver.LaunchBoxPath;
                if (string.IsNullOrEmpty(launchBoxPath)) return;

                foreach (var item in itemsSnapshot)
                {
                    var playlist = item.Node.Playlist;
                    string? logoPath = null;

                    // Same resolution logic as ResolvePlaylistImage
                    if (!string.IsNullOrEmpty(playlist?.ClearLogoImagesFolder))
                    {
                        var customDir = playlist.ClearLogoImagesFolder;
                        if (!Path.IsPathRooted(customDir))
                            customDir = Path.Combine(launchBoxPath, customDir);
                        logoPath = FileNameHelper.FindFirstImage(customDir);
                    }

                    if (logoPath == null)
                    {
                        string[] subfolders = { "Clear Logo", "Fanart", "Banner" };
                        foreach (var subfolder in subfolders)
                        {
                            var dir = Path.Combine(launchBoxPath, "Images", "Playlists", item.Name, subfolder);
                            logoPath = FileNameHelper.FindFirstImage(dir);
                            if (logoPath != null) break;
                        }
                    }

                    if (logoPath != null)
                    {
                        var bitmap = GameItem.CreateBitmapFromPath(logoPath, 40);
                        if (bitmap != null)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                item.LogoImage = bitmap;
                            });
                        }
                    }
                }
            });
        }

        // FindFirstImage removed — use FileNameHelper.FindFirstImage() instead

        [RelayCommand]
        private void NavigateUp()
        {
            if (SelectedItem == null || Items.Count == 0) return;
            int idx = Items.IndexOf(SelectedItem);
            if (idx > 0) SelectedItem = Items[idx - 1];
        }

        [RelayCommand]
        private void NavigateDown()
        {
            if (SelectedItem == null || Items.Count == 0) return;
            int idx = Items.IndexOf(SelectedItem);
            if (idx < Items.Count - 1) SelectedItem = Items[idx + 1];
        }

        [RelayCommand]
        private async Task SelectItemAsync()
        {
            if (SelectedItem == null || _navigationFrame == null) return;

            var node = SelectedItem.Node;

            switch (node.NodeType)
            {
                case NavigationNodeType.Category:
                    if (node.Children.Count > 0)
                    {
                        _navigationStack.Push((_currentNodes, CurrentTitle));
                        _currentNodes = node.Children;
                        CurrentTitle = node.Name.ToUpperInvariant();
                        DisplayCurrentLevel();
                        await LoadGameCountsAsync();
                    }
                    break;

                case NavigationNodeType.Platform:
                    var app = (App)System.Windows.Application.Current;
                    var gamesVM = app.CreateGamesWheelViewModel();
                    var gamesView = new GamesWheelView { DataContext = gamesVM };
                    _navigationFrame.Navigate(gamesView);
                    _ = gamesVM.LoadGamesAsync(node.Name);
                    break;

                case NavigationNodeType.Playlist:
                    if (_playlistManager != null && node.Playlist != null)
                    {
                        var appPl = (App)System.Windows.Application.Current;
                        var plGamesVM = appPl.CreateGamesWheelViewModel();
                        var plGamesView = new GamesWheelView { DataContext = plGamesVM };
                        _navigationFrame.Navigate(plGamesView);

                        // BUG-05 FIX: Batch lookup — same pattern as Desktop MainViewModel.
                        // GetGameByIdAsync() is O(n*m); instead build a HashSet of needed IDs
                        // and scan each platform once, stopping as soon as all are found.
                        var playlistEntries = await _playlistManager.GetPlaylistGamesAsync(node.Playlist.Name);
                        try { System.IO.File.AppendAllText(@"h:\GameLauncher\debug_playlist.txt", $"[DEBUG-PLAYLIST] '{node.Playlist.Name}' -> {playlistEntries.Count} entries loaded from XML.\n"); } catch {}

                        var neededIds = new HashSet<string>(
                            playlistEntries.Where(pg => !string.IsNullOrWhiteSpace(pg.GameId))
                                           .Select(pg => pg.GameId),
                            StringComparer.OrdinalIgnoreCase);

                        try { System.IO.File.AppendAllText(@"h:\GameLauncher\debug_playlist.txt", $"[DEBUG-PLAYLIST] '{node.Playlist.Name}' -> {neededIds.Count} valid GameIds needed.\n"); } catch {}

                        var games = new List<Game>();

                        if (neededIds.Count > 0)
                        {
                            var allPlatforms = await _platformManager.GetAllPlatformsAsync();
                            await Task.Run(() =>
                            {
                                foreach (var platform in allPlatforms)
                                {
                                    if (neededIds.Count == 0) break;
                                    var platformGames = _gameManager.GetGamesAsync(platform.Name).GetAwaiter().GetResult();
                                    foreach (var g in platformGames)
                                    {
                                        if (neededIds.Remove(g.ID))
                                            lock (games) games.Add(g);
                                    }
                                }
                            });
                        }

                        try { System.IO.File.AppendAllText(@"h:\GameLauncher\debug_playlist.txt", $"[DEBUG-PLAYLIST] '{node.Playlist.Name}' -> {games.Count} games matched from cache. Missing: {neededIds.Count}\n"); } catch {}

                        await plGamesVM.LoadGamesFromListAsync(games, node.Name);
                    }
                    break;

            }
        }

        [RelayCommand]
        private void PageUp()
        {
            if (Items.Count == 0) return;
            int idx = SelectedItem != null ? Items.IndexOf(SelectedItem) : 0;
            int newIdx = Math.Max(0, idx - 5);
            SelectedItem = Items[newIdx];
        }

        [RelayCommand]
        private void PageDown()
        {
            if (Items.Count == 0) return;
            int idx = SelectedItem != null ? Items.IndexOf(SelectedItem) : 0;
            int newIdx = Math.Min(Items.Count - 1, idx + 5);
            SelectedItem = Items[newIdx];
        }

        [RelayCommand]
        public void GoBack()
        {
            if (_navigationStack.Count == 0) return;

            var (nodes, title) = _navigationStack.Pop();
            _currentNodes = nodes;
            CurrentTitle = title;
            DisplayCurrentLevel();
            _ = LoadGameCountsAsync();
        }

        public void SetNavigationFrame(Frame frame)
        {
            _navigationFrame = frame;
        }

        // INavigationAware
        public void OnNavigatedTo(object? parameter)
        {
            _ = InitializeAsync();
        }

        public void OnNavigatedFrom() { }

        public void OnNavigatedBack(object? parameter)
        {
            _ = InitializeAsync();
        }
    }
}
