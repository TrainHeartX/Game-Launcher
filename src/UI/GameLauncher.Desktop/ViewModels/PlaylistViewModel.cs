using CommunityToolkit.Mvvm.ComponentModel;
using GameLauncher.Core.Models;

namespace GameLauncher.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel wrapper para una playlist en el sidebar del Desktop.
    /// </summary>
    public partial class PlaylistViewModel : ObservableObject
    {
        private readonly Playlist _playlist;

        public PlaylistViewModel(Playlist playlist)
        {
            _playlist = playlist;
        }

        public string Name => _playlist.Name;
        public string PlaylistId => _playlist.PlaylistId;

        [ObservableProperty]
        private int _gameCount;

        [ObservableProperty]
        private bool _isSelected;

        public Playlist Model => _playlist;
    }
}
