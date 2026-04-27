using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GameLauncher.Core.Models;

namespace GameLauncher.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel para un nodo en el árbol de navegación de LaunchBox.
    /// Puede representar una categoría, plataforma o playlist.
    /// Soporta jerarquía recursiva multinivel.
    /// </summary>
    public partial class NavigationNodeViewModel : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public NavigationNodeType NodeType { get; set; }
        public ObservableCollection<NavigationNodeViewModel> Children { get; set; } = new();

        // Backing model references
        public Platform? Platform { get; set; }
        public Playlist? Playlist { get; set; }
        public string? PlaylistId { get; set; }

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private int _gameCount;

        // For PlatformTreeView compatibility
        public string CategoryName => Name;
        public ObservableCollection<NavigationNodeViewModel> Platforms => Children;

        public string? LogoPath
        {
            get
            {
                if (NodeType != NavigationNodeType.Platform || string.IsNullOrEmpty(Name))
                    return null;

                var launchBoxPath = GameViewModel.LaunchBoxPath;
                if (string.IsNullOrEmpty(launchBoxPath))
                    return null;

                var clearLogoDir = System.IO.Path.Combine(launchBoxPath, "Images", "Platforms", Name, "Clear Logo");
                if (!System.IO.Directory.Exists(clearLogoDir))
                    return null;

                try
                {
                    return System.IO.Directory.EnumerateFiles(clearLogoDir)
                        .FirstOrDefault(f =>
                        {
                            var ext = System.IO.Path.GetExtension(f).ToLowerInvariant();
                            return ext == ".png" || ext == ".jpg" || ext == ".jpeg";
                        });
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Display icon based on node type.
        /// </summary>
        public string Icon => NodeType switch
        {
            NavigationNodeType.Category => "📁",
            NavigationNodeType.Platform => "🎮",
            NavigationNodeType.Playlist => "📋",
            NavigationNodeType.QuickFilter => "🔍",
            _ => ""
        };
    }
}
