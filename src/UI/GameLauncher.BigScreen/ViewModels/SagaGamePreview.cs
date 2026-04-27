using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GameLauncher.BigScreen.ViewModels
{
    /// <summary>
    /// Lightweight model for each game shown in the saga/playlist preview panel.
    /// </summary>
    public partial class SagaGamePreview : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string? CoverPath { get; set; }
        public string Platform { get; set; } = string.Empty;
        public bool Completed { get; set; }

        [ObservableProperty]
        private BitmapImage? _coverImage;

        [ObservableProperty]
        private BitmapImage? _platformImage;
    }
}
