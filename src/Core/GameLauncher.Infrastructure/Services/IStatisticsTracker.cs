using System.Threading.Tasks;
using GameLauncher.Core.Models;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Interface for tracking and managing game statistics.
    /// </summary>
    public interface IStatisticsTracker
    {
        /// <summary>
        /// Records a play session for a game.
        /// </summary>
        Task RecordPlaySessionAsync(Game game, int playTimeSeconds);

        /// <summary>
        /// Gets statistics for a specific platform.
        /// </summary>
        Task<PlatformStatistics> GetPlatformStatisticsAsync(string platformName);

        /// <summary>
        /// Gets overall statistics across all platforms.
        /// </summary>
        Task<PlatformStatistics> GetOverallStatisticsAsync();
    }
}
