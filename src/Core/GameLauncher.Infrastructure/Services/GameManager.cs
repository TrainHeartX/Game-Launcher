using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLauncher.Core.Models;
using GameLauncher.Data.Cache;
using GameLauncher.Data.Xml;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Manages game CRUD operations.
    /// </summary>
    public class GameManager : IGameManager
    {
        private readonly XmlDataContext _dataContext;
        private readonly GameCacheManager? _cacheManager;

        public GameManager(XmlDataContext dataContext, GameCacheManager? cacheManager = null)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _cacheManager = cacheManager;
        }

        public async Task<Game> CreateGameAsync(string platformName, Game game)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            if (string.IsNullOrWhiteSpace(game.ID))
                game.ID = Guid.NewGuid().ToString();

            game.Platform = platformName;
            var now = DateTime.UtcNow;
            game.DateAdded = now;
            game.DateModified = now;

            var games = GetGames(platformName);

            if (games.Any(g => g.ID.Equals(game.ID, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Game with ID {game.ID} already exists");

            games.Add(game);
            _dataContext.SaveGames(platformName, games);
            _cacheManager?.InvalidateGamesCache(platformName);

            return await Task.FromResult(game);
        }

        public async Task UpdateGameAsync(string platformName, Game game)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (string.IsNullOrWhiteSpace(game.ID))
                throw new ArgumentException("Game ID cannot be null or empty", nameof(game));

            var games = GetGames(platformName);
            var index = games.FindIndex(g => g.ID.Equals(game.ID, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
                throw new InvalidOperationException($"Game with ID {game.ID} not found in platform {platformName}");

            game.DateModified = DateTime.UtcNow;
            games[index] = game;

            _dataContext.SaveGames(platformName, games);
            _cacheManager?.InvalidateGamesCache(platformName);

            await Task.CompletedTask;
        }

        public async Task DeleteGameAsync(string platformName, string gameId)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));

            var games = GetGames(platformName);
            var index = games.FindIndex(g => g.ID.Equals(gameId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
                throw new InvalidOperationException($"Game with ID {gameId} not found in platform {platformName}");

            games.RemoveAt(index);
            _dataContext.SaveGames(platformName, games);
            _cacheManager?.InvalidateGamesCache(platformName);

            await Task.CompletedTask;
        }

        public async Task<Game?> GetGameByIdAsync(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));

            var platforms = GetPlatforms();

            foreach (var platform in platforms)
            {
                var games = GetGames(platform.Name);
                var game = games.FirstOrDefault(g => g.ID.Equals(gameId, StringComparison.OrdinalIgnoreCase));
                if (game != null)
                    return await Task.FromResult(game);
            }

            return await Task.FromResult<Game?>(null);
        }

        public async Task<List<Game>> SearchGamesAsync(string query, string? platformFilter = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Game>();

            var results = new List<Game>();
            query = query.ToLowerInvariant();

            var platforms = GetPlatforms();

            if (!string.IsNullOrWhiteSpace(platformFilter))
            {
                platforms = platforms
                    .Where(p => p.Name.Equals(platformFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            foreach (var platform in platforms)
            {
                var games = GetGames(platform.Name);
                var matches = games.Where(g =>
                    (g.Title?.ToLowerInvariant().Contains(query) ?? false) ||
                    (g.Developer?.ToLowerInvariant().Contains(query) ?? false) ||
                    (g.Publisher?.ToLowerInvariant().Contains(query) ?? false) ||
                    (g.Genre?.ToLowerInvariant().Contains(query) ?? false) ||
                    (g.Series?.ToLowerInvariant().Contains(query) ?? false)
                ).ToList();

                results.AddRange(matches);
            }

            return await Task.FromResult(results);
        }

        public async Task<List<Game>> GetGamesAsync(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            return await Task.FromResult(new List<Game>(GetGames(platformName)));
        }

        public async Task<List<Game>> GetFavoritesAsync()
        {
            var results = new List<Game>();
            var platforms = GetPlatforms();

            foreach (var platform in platforms)
            {
                var games = GetGames(platform.Name);
                results.AddRange(games.Where(g => g.Favorite));
            }

            return await Task.FromResult(results.OrderBy(g => g.Title).ToList());
        }

        public async Task<List<Game>> GetRecentlyPlayedAsync(int maxCount = 50)
        {
            var results = new List<Game>();
            var platforms = GetPlatforms();

            foreach (var platform in platforms)
            {
                var games = GetGames(platform.Name);
                results.AddRange(games.Where(g => g.PlayCount > 0));
            }

            return await Task.FromResult(
                results.OrderByDescending(g => g.DateModified).Take(maxCount).ToList());
        }

        public async Task<List<Game>> GetCompletedAsync()
        {
            var results = new List<Game>();
            var platforms = GetPlatforms();

            foreach (var platform in platforms)
            {
                var games = GetGames(platform.Name);
                results.AddRange(games.Where(g => g.Completed));
            }

            return await Task.FromResult(results.OrderBy(g => g.Title).ToList());
        }

        public async Task<List<Game>> SearchAllPlatformsAsync(string query, int maxResults = 500)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Game>();

            query = query.Trim().ToLowerInvariant();

            // Use cache's GetAllGames() for instant cross-platform lookup
            var allGames = _cacheManager != null
                ? await Task.Run(() => _cacheManager.GetAllGames())
                : await SearchGamesAsync(query);   // fallback to platform-by-platform

            // Rank: exact title match first, then partial matches
            var exact = new List<Game>();
            var partial = new List<Game>();

            foreach (var g in allGames)
            {
                if (g.Title.Equals(query, StringComparison.OrdinalIgnoreCase))
                    exact.Add(g);
                else if ((g.Title?.ToLowerInvariant().Contains(query) ?? false) ||
                         (g.Developer?.ToLowerInvariant().Contains(query) ?? false) ||
                         (g.Publisher?.ToLowerInvariant().Contains(query) ?? false) ||
                         (g.Genre?.ToLowerInvariant().Contains(query) ?? false) ||
                         (g.Series?.ToLowerInvariant().Contains(query) ?? false))
                    partial.Add(g);
            }

            var results = exact.Concat(partial.OrderBy(g => g.Title)).Take(maxResults).ToList();
            return await Task.FromResult(results);
        }

        private List<Game> GetGames(string platformName)
        {
            return _cacheManager != null
                ? _cacheManager.GetGames(platformName)
                : _dataContext.LoadGames(platformName);
        }

        private List<Platform> GetPlatforms()
        {
            return _cacheManager != null
                ? _cacheManager.GetPlatforms()
                : _dataContext.LoadPlatforms();
        }
    }
}
