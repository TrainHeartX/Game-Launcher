using System;
using System.Linq;
using System.Threading.Tasks;
using GameLauncher.Core.Models;
using GameLauncher.Data.Cache;
using GameLauncher.Data.Xml;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Tracks and manages game statistics.
    /// Updates PlayCount, PlayTime, and LastPlayed after game sessions.
    /// </summary>
    public class StatisticsTracker : IStatisticsTracker
    {
        private readonly XmlDataContext _dataContext;
        private readonly GameCacheManager? _cacheManager;

        public StatisticsTracker(XmlDataContext dataContext, GameCacheManager? cacheManager = null)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _cacheManager = cacheManager;
        }

        public async Task RecordPlaySessionAsync(Game game, int playTimeSeconds)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (string.IsNullOrWhiteSpace(game.Platform))
                throw new ArgumentException("Game must have a platform", nameof(game));

            try
            {
                var games = GetGames(game.Platform);

                // BUG-04 FIX: Try ID first; fall back to Title+Platform match
                int index = -1;
                if (!string.IsNullOrWhiteSpace(game.ID))
                    index = games.FindIndex(g => g.ID.Equals(game.ID, StringComparison.OrdinalIgnoreCase));

                if (index < 0)
                    index = games.FindIndex(g =>
                        string.Equals(g.Title, game.Title, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(g.Platform, game.Platform, StringComparison.OrdinalIgnoreCase));

                if (index < 0)
                {
                    // Game genuinely not in XML (e.g. playlist virtual game). Still update in-memory object.
                    game.PlayCount++;
                    game.PlayTime += playTimeSeconds;
                    game.DateModified = DateTime.Now;
                    game.LastPlayedDate = DateTime.Now;
                    await Task.CompletedTask;
                    return;
                }

                var updatedGame = games[index];
                updatedGame.PlayCount++;
                updatedGame.PlayTime += playTimeSeconds;
                updatedGame.DateModified = DateTime.Now;
                updatedGame.LastPlayedDate = DateTime.Now;

                _dataContext.SaveGames(game.Platform, games);
                _cacheManager?.InvalidateGamesCache(game.Platform);

                game.PlayCount   = updatedGame.PlayCount;
                game.PlayTime    = updatedGame.PlayTime;
                game.DateModified  = updatedGame.DateModified;
                game.LastPlayedDate = updatedGame.LastPlayedDate;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StatisticsTracker] Failed to save play session: {ex.Message}");
                // Still update the in-memory object so the UI shows correct data this session
                game.PlayCount++;
                game.PlayTime += playTimeSeconds;
                game.DateModified = DateTime.Now;
                game.LastPlayedDate = DateTime.Now;
            }

            await Task.CompletedTask;
        }

        public async Task<PlatformStatistics> GetPlatformStatisticsAsync(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            var games = GetGames(platformName);

            var stats = new PlatformStatistics
            {
                PlatformName = platformName,
                TotalGames = games.Count,
                FavoriteGames = games.Count(g => g.Favorite),
                CompletedGames = games.Count(g => g.Completed),
                TotalPlayTimeSeconds = games.Sum(g => g.PlayTime),
                TotalPlayCount = games.Sum(g => g.PlayCount)
            };

            var mostPlayed = games
                .Where(g => g.PlayTime > 0)
                .OrderByDescending(g => g.PlayTime)
                .FirstOrDefault();

            if (mostPlayed != null)
            {
                stats.MostPlayedGameTitle = mostPlayed.Title;
                stats.MostPlayedGameTime = mostPlayed.PlayTime;
            }

            var gamesWithPlayDate = games
                .Where(g => g.PlayCount > 0 && (g.LastPlayedDate.HasValue || g.DateModified.HasValue))
                .ToList();

            if (gamesWithPlayDate.Count > 0)
                stats.LastPlayed = gamesWithPlayDate.Max(g => g.LastPlayedDate ?? g.DateModified);

            return await Task.FromResult(stats);
        }

        public async Task<PlatformStatistics> GetOverallStatisticsAsync()
        {
            var platforms = GetPlatforms();

            var overallStats = new PlatformStatistics
            {
                PlatformName = "All Platforms"
            };

            string? mostPlayedGameTitle = null;
            long mostPlayedGameTime = 0;
            DateTime? lastPlayed = null;

            foreach (var platform in platforms)
            {
                var games = GetGames(platform.Name);

                overallStats.TotalGames += games.Count;
                overallStats.FavoriteGames += games.Count(g => g.Favorite);
                overallStats.CompletedGames += games.Count(g => g.Completed);
                overallStats.TotalPlayTimeSeconds += games.Sum(g => g.PlayTime);
                overallStats.TotalPlayCount += games.Sum(g => g.PlayCount);

                var platformMostPlayed = games
                    .Where(g => g.PlayTime > 0)
                    .OrderByDescending(g => g.PlayTime)
                    .FirstOrDefault();

                if (platformMostPlayed != null && platformMostPlayed.PlayTime > mostPlayedGameTime)
                {
                    mostPlayedGameTitle = platformMostPlayed.Title;
                    mostPlayedGameTime = platformMostPlayed.PlayTime;
                }

                var gamesWithPlayDate = games
                    .Where(g => g.PlayCount > 0 && (g.LastPlayedDate.HasValue || g.DateModified.HasValue))
                    .ToList();

                if (gamesWithPlayDate.Count > 0)
                {
                    var platformLastPlayed = gamesWithPlayDate.Max(g => g.LastPlayedDate ?? g.DateModified);
                    if (platformLastPlayed.HasValue && (!lastPlayed.HasValue || platformLastPlayed > lastPlayed))
                        lastPlayed = platformLastPlayed;
                }
            }

            overallStats.MostPlayedGameTitle = mostPlayedGameTitle ?? "";
            overallStats.MostPlayedGameTime = mostPlayedGameTime;
            overallStats.LastPlayed = lastPlayed;

            return await Task.FromResult(overallStats);
        }

        private System.Collections.Generic.List<Game> GetGames(string platformName)
        {
            return _cacheManager != null
                ? _cacheManager.GetGames(platformName)
                : _dataContext.LoadGames(platformName);
        }

        private System.Collections.Generic.List<Platform> GetPlatforms()
        {
            return _cacheManager != null
                ? _cacheManager.GetPlatforms()
                : _dataContext.LoadPlatforms();
        }
    }
}
