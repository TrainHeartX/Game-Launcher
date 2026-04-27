using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GameLauncher.Infrastructure.Services;

namespace GameLauncher.Desktop.ViewModels
{
    public partial class StatisticsViewModel : ObservableObject
    {
        private readonly IPlatformManager _platformManager;
        private readonly IStatisticsTracker _statisticsTracker;

        [ObservableProperty]
        private string _totalPlayTime = "0h 0m";

        [ObservableProperty]
        private int _totalGamesPlayed;

        [ObservableProperty]
        private int _totalPlatforms;

        [ObservableProperty]
        private ObservableCollection<GameStatItem> _topGames = new();

        [ObservableProperty]
        private ObservableCollection<PlatformStatItem> _topPlatforms = new();

        public StatisticsViewModel(
            IPlatformManager platformManager,
            IStatisticsTracker statisticsTracker)
        {
            _platformManager = platformManager;
            _statisticsTracker = statisticsTracker;
        }

        public async Task LoadStatisticsAsync()
        {
            // Obtener estadísticas globales reales
            var overallStats = await _statisticsTracker.GetOverallStatisticsAsync();

            TotalPlayTime = overallStats.FormattedTotalPlayTime;
            TotalGamesPlayed = overallStats.TotalGames;

            // Obtener plataformas reales
            var platforms = await _platformManager.GetAllPlatformsAsync();
            TotalPlatforms = platforms.Count;

            // Top plataformas por tiempo de juego
            var platformStats = new System.Collections.Generic.List<(string Name, PlatformStatistics Stats)>();
            foreach (var platform in platforms)
            {
                var stats = await _statisticsTracker.GetPlatformStatisticsAsync(platform.Name);
                if (stats.TotalGames > 0)
                    platformStats.Add((platform.Name, stats));
            }

            TopPlatforms.Clear();
            foreach (var ps in platformStats.OrderByDescending(p => p.Stats.TotalPlayTimeSeconds).Take(5))
            {
                TopPlatforms.Add(new PlatformStatItem(ps.Name, ps.Stats.TotalGames, ps.Stats.FormattedTotalPlayTime));
            }

            // Top juegos por tiempo de juego (de las plataformas con más actividad)
            TopGames.Clear();
            var allGamesWithTime = new System.Collections.Generic.List<(string Title, long PlayTime, int PlayCount)>();
            foreach (var platform in platforms)
            {
                var games = await Task.Run(() =>
                {
                    try { return _platformManager.GetPlatformStatisticsAsync(platform.Name).Result; }
                    catch { return null; }
                });

                // Buscar juegos con mayor tiempo desde el servicio de estadísticas
                var platformStat = platformStats.FirstOrDefault(p => p.Name == platform.Name);
                if (platformStat.Stats != null && !string.IsNullOrEmpty(platformStat.Stats.MostPlayedGameTitle))
                {
                    allGamesWithTime.Add((
                        platformStat.Stats.MostPlayedGameTitle,
                        platformStat.Stats.MostPlayedGameTime,
                        0
                    ));
                }
            }

            foreach (var game in allGamesWithTime.OrderByDescending(g => g.PlayTime).Take(5))
            {
                var timeSpan = TimeSpan.FromSeconds(game.PlayTime);
                string formatted = game.PlayTime < 3600
                    ? $"{(int)timeSpan.TotalMinutes}m"
                    : $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
                TopGames.Add(new GameStatItem(game.Title, formatted, game.PlayCount));
            }
        }
    }

    public class GameStatItem
    {
        public string Title { get; set; }
        public string PlayTime { get; set; }
        public int PlayCount { get; set; }

        public GameStatItem(string title, string playTime, int playCount)
        {
            Title = title;
            PlayTime = playTime;
            PlayCount = playCount;
        }
    }

    public class PlatformStatItem
    {
        public string Name { get; set; }
        public int GameCount { get; set; }
        public string TotalPlayTime { get; set; }

        public PlatformStatItem(string name, int gameCount, string totalPlayTime)
        {
            Name = name;
            GameCount = gameCount;
            TotalPlayTime = totalPlayTime;
        }
    }
}
