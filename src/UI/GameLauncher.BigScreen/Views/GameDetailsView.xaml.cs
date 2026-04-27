using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GameLauncher.BigScreen.ViewModels;

namespace GameLauncher.BigScreen.Views
{
    /// <summary>
    /// Vista de detalles del juego con video y estadísticas.
    /// </summary>
    public partial class GameDetailsView : Page
    {
        public GameDetailsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is GameDetailsViewModel oldVm)
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;

            if (e.NewValue is GameDetailsViewModel newVm)
                newVm.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameDetailsViewModel.VideoUri))
            {
                UpdateVideoPlayer();
            }
        }

        private void UpdateVideoPlayer()
        {
            try
            {
                GameVideoPlayer.Stop();
                GameVideoPlayer.Source = null;

                var vm = DataContext as GameDetailsViewModel;
                var uri = vm?.VideoUri;

                if (uri != null)
                {
                    VideoPlaceholder.Visibility = Visibility.Collapsed;
                    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
                    {
                        var currentVm = DataContext as GameDetailsViewModel;
                        if (currentVm?.VideoUri == uri)
                        {
                            GameVideoPlayer.Source = uri;
                        }
                    });
                }
                else
                {
                    VideoPlaceholder.Visibility = Visibility.Visible;
                }
            }
            catch
            {
            }
        }

        private void GameVideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Iniciar mudo, activar audio despues de un breve delay
            // para evitar sonido sin video visible
            GameVideoPlayer.Volume = 0;
            GameVideoPlayer.Position = TimeSpan.Zero;
            GameVideoPlayer.Play();

            // Activar audio una vez el video esta reproduciendo
            Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => GameVideoPlayer.Volume = 1);
                });
            });
        }

        private void GameVideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            GameVideoPlayer.Position = TimeSpan.Zero;
            GameVideoPlayer.Play();
        }

        private void GameVideoPlayer_MediaFailed(object? sender, ExceptionRoutedEventArgs e)
        {
            // Si falla la reproducción, mostrar placeholder
            VideoPlaceholder.Visibility = Visibility.Visible;
        }
    }
}
