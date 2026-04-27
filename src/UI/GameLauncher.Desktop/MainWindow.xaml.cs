using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using GameLauncher.Desktop.ViewModels;

namespace GameLauncher.Desktop;

/// <summary>
/// Ventana principal de GameLauncher Desktop
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += async (s, e) => await viewModel.InitializeAsync();

        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        GameVideoPlayer.MediaFailed += GameVideoPlayer_MediaFailed;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedGameVideoUri))
        {
            UpdateVideoPlayer();
        }
    }

    private void UpdateVideoPlayer()
    {
        try
        {
            // Detener video actual sin Close() (Close libera recursos agresivamente)
            GameVideoPlayer.Stop();
            GameVideoPlayer.Source = null;

            var uri = _viewModel.SelectedGameVideoUri;
            if (uri != null)
            {
                // Esperar a que el Border sea Visible (el binding lo hace Visible)
                // y el MediaElement esté en el visual tree antes de setear Source
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
                {
                    // Verificar que no cambió mientras esperábamos
                    if (_viewModel.SelectedGameVideoUri == uri)
                    {
                        GameVideoPlayer.Source = uri;
                    }
                });
            }
        }
        catch
        {
        }
    }

    private void GameVideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
    {
        GameVideoPlayer.Position = TimeSpan.Zero;
        GameVideoPlayer.Play();
    }

    private void GameVideoPlayer_MediaFailed(object? sender, ExceptionRoutedEventArgs e)
    {
        MessageBox.Show(
            $"Error al reproducir video:\n{e.ErrorException?.Message}\n\nSource: {GameVideoPlayer.Source}",
            "Video Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void GameVideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
    {
        GameVideoPlayer.Position = TimeSpan.Zero;
        GameVideoPlayer.Play();
    }
}
