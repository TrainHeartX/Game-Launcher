using CommunityToolkit.Mvvm.ComponentModel;
using GameLauncher.Core.Models;
using System;
using System.IO;
using System.Linq;

namespace GameLauncher.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel para una plataforma en el árbol de navegación.
    /// </summary>
    public partial class PlatformViewModel : ObservableObject
    {
        private readonly Platform _platform;

        public PlatformViewModel(Platform platform)
        {
            _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        }

        public string Name => _platform.Name;
        public string? Category => _platform.Category;
        public string? Developer => _platform.Developer;
        public string? Manufacturer => _platform.Manufacturer;
        public DateTime? ReleaseDate => _platform.ReleaseDate;
        public string? Folder => _platform.Folder;

        // Propiedades calculadas
        public string DisplayName => Name;
        public string DisplayCategory => string.IsNullOrWhiteSpace(Category) ? "Sin Categoría" : Category;

        /// <summary>
        /// Resuelve la ruta del Clear Logo de la plataforma para PlatformTreeView.
        /// </summary>
        public string? LogoPath
        {
            get
            {
                var launchBoxPath = GameViewModel.LaunchBoxPath;
                if (string.IsNullOrEmpty(launchBoxPath) || string.IsNullOrEmpty(Name))
                    return null;

                var clearLogoDir = Path.Combine(launchBoxPath, "Images", "Platforms", Name, "Clear Logo");
                if (!Directory.Exists(clearLogoDir))
                    return null;

                try
                {
                    return Directory.EnumerateFiles(clearLogoDir)
                        .FirstOrDefault(f =>
                        {
                            var ext = Path.GetExtension(f).ToLowerInvariant();
                            return ext == ".png" || ext == ".jpg" || ext == ".jpeg";
                        });
                }
                catch
                {
                    return null;
                }
            }
        }

        // Acceso al modelo subyacente
        public Platform Model => _platform;

        [ObservableProperty]
        private int _gameCount;

        [ObservableProperty]
        private bool _isSelected;
    }
}
