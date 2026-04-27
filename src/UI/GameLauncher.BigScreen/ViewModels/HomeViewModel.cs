using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLauncher.BigScreen.Views;
using GameLauncher.Infrastructure.Services;

namespace GameLauncher.BigScreen.ViewModels;

/// <summary>
/// ViewModel for the home screen with dual-card navigation.
/// Allows selection between GameLauncher and Gestión de Juegos.
/// </summary>
public partial class HomeViewModel : ObservableObject
{
    private readonly Frame _navigationFrame;
    private readonly IPlatformManager _platformManager;
    private readonly IGameManager _gameManager;
    private readonly IPlaylistManager? _playlistManager;

    [ObservableProperty]
    private int _selectedIndex = 0; // 0 = GameLauncher, 1 = Gestión

    public HomeViewModel(
        Frame navigationFrame,
        IPlatformManager platformManager,
        IGameManager gameManager,
        IPlaylistManager? playlistManager)
    {
        _navigationFrame = navigationFrame ?? throw new ArgumentNullException(nameof(navigationFrame));
        _platformManager = platformManager ?? throw new ArgumentNullException(nameof(platformManager));
        _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
        _playlistManager = playlistManager;
    }

    [RelayCommand]
    public void NavigateLeft()
    {
        if (SelectedIndex > 0)
            SelectedIndex--;
    }

    [RelayCommand]
    public void NavigateRight()
    {
        if (SelectedIndex < 3) // Now max index is 3
            SelectedIndex++;
    }

    [RelayCommand]
    public async Task NavigateToSelectedCardAsync()
    {
        switch (SelectedIndex)
        {
            case 0: // GameLauncher (Library)
                var platformFiltersVM = new PlatformFiltersViewModel(_platformManager, _gameManager, _playlistManager);
                platformFiltersVM.SetNavigationFrame(_navigationFrame);
                var platformFiltersView = new PlatformFiltersView { DataContext = platformFiltersVM };
                _navigationFrame.Navigate(platformFiltersView);
                await platformFiltersVM.InitializeAsync();
                platformFiltersView.EnableVideoAudio();
                break;

            case 1: // PiviGames
                var piviVM = new SourcesViewModel();
                piviVM.SelectSource("PiviGames");
                var piviView = new SourcesView { DataContext = piviVM };
                _navigationFrame.Navigate(piviView);
                await piviVM.InitializeAsync();
                break;

            case 2: // BlizzBoyGames
                var blizzVM = new SourcesViewModel();
                blizzVM.SelectSource("BlizzBoyGames");
                var blizzView = new SourcesView { DataContext = blizzVM };
                _navigationFrame.Navigate(blizzView);
                await blizzVM.InitializeAsync();
                break;

            case 3: // System Info
                var systemInfoVM = new SystemInfoViewModel();
                var systemInfoView = new SystemInfoView { DataContext = systemInfoVM };
                _navigationFrame.Navigate(systemInfoView);
                await systemInfoVM.InitializeAsync();
                break;
        }
    }
}
