using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLauncher.Core.Models;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Interface for managing games (CRUD operations).
    /// </summary>
    public interface IGameManager
    {
        /// <summary>
        /// Creates a new game in the specified platform.
        /// </summary>
        Task<Game> CreateGameAsync(string platformName, Game game);

        /// <summary>
        /// Updates an existing game.
        /// </summary>
        Task UpdateGameAsync(string platformName, Game game);

        /// <summary>
        /// Deletes a game from a platform.
        /// </summary>
        Task DeleteGameAsync(string platformName, string gameId);

        /// <summary>
        /// Gets a game by its ID (searches all platforms).
        /// </summary>
        Task<Game?> GetGameByIdAsync(string gameId);

        /// <summary>
        /// Searches games across all platforms.
        /// </summary>
        Task<List<Game>> SearchGamesAsync(string query, string? platformFilter = null);

        /// <summary>
        /// Gets all games for a specific platform.
        /// </summary>
        Task<List<Game>> GetGamesAsync(string platformName);

        /// <summary>
        /// Gets all favorite games across all platforms.
        /// </summary>
        Task<List<Game>> GetFavoritesAsync();

        /// <summary>
        /// Gets recently played games across all platforms.
        /// </summary>
        Task<List<Game>> GetRecentlyPlayedAsync(int maxCount = 50);

        /// <summary>
        /// Gets all completed games across all platforms.
        /// </summary>
        Task<List<Game>> GetCompletedAsync();

        /// <summary>
        /// Searches games across ALL platforms simultaneously using the in-memory cache.
        /// Returns up to maxResults matches ordered by relevance (exact title match first).
        /// </summary>
        Task<List<Game>> SearchAllPlatformsAsync(string query, int maxResults = 500);
    }
}
