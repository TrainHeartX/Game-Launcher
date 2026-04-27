using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLauncher.BigScreen.Navigation;
using GameLauncher.Core.Helpers;
using GameLauncher.Core.Models;
using GameLauncher.Infrastructure.Services;

namespace GameLauncher.BigScreen.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de detalles del juego.
    /// </summary>
    public partial class GameDetailsViewModel : ObservableObject, INavigationAware
    {
        private readonly IEmulatorLauncher _emulatorLauncher;
        private readonly IStatisticsTracker _statisticsTracker;

        [ObservableProperty]
        private Game? _game;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private Uri? _videoUri;

        // Propiedades formateadas
        public string GameTitle => Game?.Title ?? "Sin título";
        public string GameDeveloper => Game?.Developer ?? "Desconocido";
        public string GamePublisher => Game?.Publisher ?? "Desconocido";
        public string GameGenre => Game?.Genre ?? string.Empty;
        public string GameYear => Game?.ReleaseDate?.Year.ToString() ?? string.Empty;
        public string GamePlatform => Game?.Platform ?? string.Empty;
        public string GameRating => FormatRating(Game?.CommunityStarRating ?? 0);
        public string GamePlayTime => FormatPlayTime(Game?.PlayTime ?? 0);
        public int GamePlayCount => Game?.PlayCount ?? 0;
        public string GameLastPlayed => Game?.LastPlayed?.ToString("dd/MM/yyyy") ?? "Nunca";
        public string GameDescription => Game?.Notes ?? "Sin descripción disponible.";
        public bool GameFavorite => Game?.Favorite ?? false;
        public bool GameCompleted => Game?.Completed ?? false;

        public GameDetailsViewModel(
            IEmulatorLauncher emulatorLauncher,
            IStatisticsTracker statisticsTracker)
        {
            _emulatorLauncher = emulatorLauncher ?? throw new ArgumentNullException(nameof(emulatorLauncher));
            _statisticsTracker = statisticsTracker ?? throw new ArgumentNullException(nameof(statisticsTracker));
        }

        [RelayCommand]
        private async Task LaunchGameAsync()
        {
            if (Game == null)
                return;

            try
            {
                StatusText = $"Lanzando {Game.Title}...";

                var result = await _emulatorLauncher.LaunchGameAsync(Game);

                if (result.Success)
                {
                    // Registrar estadísticas
                    await _statisticsTracker.RecordPlaySessionAsync(Game, result.PlayTimeSeconds);

                    StatusText = $"Sesión completada: {FormatPlayTime(result.PlayTimeSeconds)}";

                    // Actualizar propiedades
                    UpdateGameProperties();
                }
                else
                {
                    StatusText = $"Error: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error al lanzar: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ToggleFavorite()
        {
            if (Game == null)
                return;

            Game.Favorite = !Game.Favorite;
            OnPropertyChanged(nameof(GameFavorite));

            // TODO: Guardar cambio en XML
            StatusText = Game.Favorite ? "★ Agregado a favoritos" : "Quitado de favoritos";
        }

        [RelayCommand]
        private void ShowImages()
        {
            // Por ahora muestra status; se puede expandir con un visor de imágenes dedicado
            if (Game == null) return;
            StatusText = $"Presiona X para ver imágenes de {Game.Title}";
        }

        [RelayCommand]
        private void ToggleCompleted()
        {
            if (Game == null)
                return;

            Game.Completed = !Game.Completed;
            OnPropertyChanged(nameof(GameCompleted));

            // TODO: Guardar cambio en XML
            StatusText = Game.Completed ? "✓ Marcado como completado" : "Desmarcado como completado";
        }

        private void UpdateGameProperties()
        {
            OnPropertyChanged(nameof(GamePlayTime));
            OnPropertyChanged(nameof(GamePlayCount));
            OnPropertyChanged(nameof(GameLastPlayed));
        }

        private string FormatRating(double rating)
        {
            if (rating == 0)
                return "Sin calificación";

            int stars = (int)Math.Round(rating);
            return new string('★', stars) + new string('☆', 5 - stars);
        }

        private string FormatPlayTime(long seconds)
        {
            if (seconds == 0)
                return "Sin jugar";

            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);

            if (hours > 0)
                return $"{hours}h {minutes}m";
            else
                return $"{minutes}m";
        }

        // INavigationAware implementation
        public void OnNavigatedTo(object? parameter)
        {
            if (parameter is Game game)
            {
                Game = game;
                UpdateGameProperties();
                StatusText = string.Empty;
                ResolveVideo();
            }
        }

        public void OnNavigatedFrom()
        {
            VideoUri = null;
            Game = null;
        }

        private void ResolveVideo()
        {
            if (Game == null)
            {
                VideoUri = null;
                return;
            }

            var game = Game;
            _ = Task.Run(() =>
            {
                var videoPath = VideoPathResolver.Resolve(game.Title, game.Platform);
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (Game == game)
                    {
                        VideoUri = videoPath != null ? new Uri(videoPath, UriKind.Absolute) : null;
                    }
                });
            });
        }

        public void OnNavigatedBack(object? parameter)
        {
            // Refrescar al volver
            UpdateGameProperties();
        }
    }
}
