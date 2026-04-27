using CommunityToolkit.Mvvm.ComponentModel;

namespace GameLauncher.BigScreen.ViewModels
{
    /// <summary>
    /// Item del menu de gestion de un juego (A button).
    /// </summary>
    public partial class ManageMenuItem : ObservableObject
    {
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ActiveColor { get; set; } = "#00d4ff";

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private string _statusText = string.Empty;

        /// <summary>Key interna para identificar la accion.</summary>
        public string Key { get; set; } = string.Empty;
    }
}
