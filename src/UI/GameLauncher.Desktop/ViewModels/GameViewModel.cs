using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLauncher.Core.Helpers;
using GameLauncher.Core.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GameLauncher.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel para un juego individual con comandos y propiedades de UI.
    /// </summary>
    public partial class GameViewModel : ObservableObject
    {
        private readonly Game _game;

        // Ruta de LaunchBox (se configura una vez al inicio)
        public static string? LaunchBoxPath { get; set; }

        public GameViewModel(Game game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
        }

        // Imagen del juego (se carga explícitamente desde MainViewModel)
        private BitmapImage? _coverImage;
        public BitmapImage? CoverImage
        {
            get => _coverImage;
            set => SetProperty(ref _coverImage, value);
        }

        // Ruta resuelta de la imagen (para caché a disco)
        public string? ResolvedImagePath { get; set; }

        /// <summary>
        /// Busca la ruta del archivo de imagen de portada en disco.
        /// Es thread-safe (no crea objetos de UI).
        /// </summary>
        public string? ResolveCoverImagePath()
        {
            if (string.IsNullOrEmpty(LaunchBoxPath) || string.IsNullOrEmpty(Platform) || string.IsNullOrEmpty(Title))
                return null;

            string[] imageTypeFolders = { "Box - Front", "Box - Front - Reconstructed" };
            string sanitizedTitle = SanitizeFileName(Title);
            string[] suffixes = { "-01", "-02", "-03", "" };
            string[] extensions = { ".png", ".jpg", ".jpeg" };

            foreach (var imageType in imageTypeFolders)
            {
                string typeFolder = Path.Combine(LaunchBoxPath, "Images", Platform, imageType);

                if (!Directory.Exists(typeFolder))
                    continue;

                // Buscar directamente en la carpeta raíz
                var result = SearchPathInFolder(typeFolder, sanitizedTitle, suffixes, extensions);
                if (result != null) return result;

                // Buscar en subcarpetas de región (Box - Front Y Reconstructed las tienen)
                try
                {
                    foreach (var regionFolder in Directory.GetDirectories(typeFolder))
                    {
                        result = SearchPathInFolder(regionFolder, sanitizedTitle, suffixes, extensions);
                        if (result != null) return result;
                    }
                }
                catch { }
            }

            return null;
        }

        private static string? SearchPathInFolder(string folder, string sanitizedTitle, string[] suffixes, string[] extensions)
        {
            // Buscar con nombre exacto + sufijo
            foreach (var suffix in suffixes)
            {
                foreach (var ext in extensions)
                {
                    string filePath = Path.Combine(folder, sanitizedTitle + suffix + ext);
                    if (File.Exists(filePath))
                        return filePath;
                }
            }

            // Buscar archivos que empiecen con el título sanitizado
            try
            {
                var match = Directory.EnumerateFiles(folder)
                    .FirstOrDefault(f =>
                    {
                        string name = Path.GetFileNameWithoutExtension(f);
                        string ext = Path.GetExtension(f).ToLowerInvariant();
                        return name.StartsWith(sanitizedTitle, StringComparison.OrdinalIgnoreCase)
                               && (ext == ".png" || ext == ".jpg" || ext == ".jpeg");
                    });

                if (match != null)
                    return match;
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Crea un BitmapImage desde una ruta. DEBE llamarse en el hilo de UI.
        /// </summary>
        public static BitmapImage? CreateBitmapFromPath(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.DecodePixelWidth = 150;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            string s = fileName;
            s = s.Replace(":", "_");
            s = s.Replace("/", "_");
            s = s.Replace("\\", "_");
            s = s.Replace("?", "_");
            s = s.Replace("*", "_");
            s = s.Replace("\"", "_");
            s = s.Replace("<", "_");
            s = s.Replace(">", "_");
            s = s.Replace("|", "_");
            s = s.Replace("'", "_");
            return s;
        }

        // Propiedades del juego (delegadas al modelo)
        public string ID => _game.ID;
        public string Title => _game.Title;
        public string Platform => _game.Platform;
        public string? Developer => _game.Developer;
        public string? Publisher => _game.Publisher;
        public string? Genre => _game.Genre;
        public DateTime? ReleaseDate => _game.ReleaseDate;
        public string? ApplicationPath => _game.ApplicationPath;

        // Propiedades adicionales de metadata
        public string? Region => _game.Region;
        public string? Series => _game.Series;
        public string? Version => _game.Version;
        public string? PlayMode => _game.PlayMode;
        public int MaxPlayers => _game.MaxPlayers;
        public string? Status => _game.Status;
        public string? Source => _game.Source;
        public string? ReleaseType => _game.ReleaseType;
        public string? Emulator => _game.Emulator;

        // Fechas
        public DateTime? DateAdded => _game.DateAdded;
        public DateTime? DateModified => _game.DateModified;
        public DateTime? LastPlayed => _game.DateModified;

        public bool Favorite
        {
            get => _game.Favorite;
            set
            {
                if (_game.Favorite != value)
                {
                    _game.Favorite = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Completed
        {
            get => _game.Completed;
            set
            {
                if (_game.Completed != value)
                {
                    _game.Completed = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Broken
        {
            get => _game.Broken;
            set
            {
                if (_game.Broken != value)
                {
                    _game.Broken = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Hide
        {
            get => _game.Hide;
            set
            {
                if (_game.Hide != value)
                {
                    _game.Hide = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Portable
        {
            get => _game.Portable;
            set
            {
                if (_game.Portable != value)
                {
                    _game.Portable = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PlayCount => _game.PlayCount;
        public long PlayTime => _game.PlayTime;
        public float StarRatingFloat => _game.StarRatingFloat;
        public float CommunityStarRating => _game.CommunityStarRating;
        public string? Notes => _game.Notes;
        public string? WikipediaURL => _game.WikipediaURL;
        public string? VideoUrl => _game.VideoUrl;

        // Propiedades calculadas
        public string FormattedPlayTime
        {
            get
            {
                if (PlayTime == 0) return "Sin jugar";
                var ts = TimeSpan.FromSeconds(PlayTime);
                if (ts.TotalHours >= 1)
                    return $"{(int)ts.TotalHours}h {ts.Minutes}m";
                return $"{(int)ts.TotalMinutes}m";
            }
        }

        public string FormattedReleaseDate => ReleaseDate?.ToString("dd/MM/yyyy") ?? "Desconocida";

        /// <summary>
        /// Busca la ruta del archivo de video en disco.
        /// Delega al helper compartido VideoPathResolver.
        /// </summary>
        public string? ResolveVideoPath()
        {
            return VideoPathResolver.Resolve(Title, Platform);
        }

        // Acceso al modelo subyacente
        public Game Model => _game;

        // Eventos para comandos (serán manejados por MainViewModel)
        public event EventHandler? LaunchRequested;
        public event EventHandler? EditRequested;
        public event EventHandler? DeleteRequested;

        [RelayCommand]
        private void Launch()
        {
            LaunchRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Edit()
        {
            EditRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Delete()
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void ToggleFavorite()
        {
            Favorite = !Favorite;
        }

        [RelayCommand]
        private void ToggleCompleted()
        {
            Completed = !Completed;
        }

        public void RefreshStats()
        {
            OnPropertyChanged(nameof(PlayCount));
            OnPropertyChanged(nameof(PlayTime));
            OnPropertyChanged(nameof(FormattedPlayTime));
        }
    }
}
