using System.Collections.Generic;
using System.Threading.Tasks;
using GameLauncher.Core.Models;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Interface for managing platforms.
    /// </summary>
    public interface IPlatformManager
    {
        /// <summary>
        /// Gets all platforms.
        /// </summary>
        Task<List<Platform>> GetAllPlatformsAsync();

        /// <summary>
        /// Gets platforms grouped by category.
        /// </summary>
        Task<Dictionary<string, List<Platform>>> GetPlatformsByCategoryAsync();

        /// <summary>
        /// Gets platforms grouped by parent category (from Parents.xml hierarchy).
        /// </summary>
        Task<Dictionary<string, List<Platform>>> GetPlatformsByParentCategoryAsync();

        /// <summary>
        /// Builds the full LaunchBox navigation tree from Parents.xml,
        /// PlatformCategories, Platforms and Playlists.
        /// Returns the list of root-level NavigationNodes.
        /// </summary>
        Task<List<NavigationNode>> GetNavigationTreeAsync();

        /// <summary>
        /// Gets a platform by name.
        /// </summary>
        Task<Platform?> GetPlatformByNameAsync(string platformName);

        /// <summary>
        /// Gets statistics for a platform.
        /// </summary>
        Task<PlatformStatistics> GetPlatformStatisticsAsync(string platformName);
    }
}
