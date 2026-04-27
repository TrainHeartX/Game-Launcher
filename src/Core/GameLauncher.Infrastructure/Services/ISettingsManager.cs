using System.Threading.Tasks;
using GameLauncher.Core.Models;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Interface for managing application settings.
    /// </summary>
    public interface ISettingsManager
    {
        /// <summary>
        /// Loads Desktop settings.
        /// </summary>
        Task<Settings> LoadSettingsAsync();

        /// <summary>
        /// Saves Desktop settings.
        /// </summary>
        Task SaveSettingsAsync(Settings settings);

        /// <summary>
        /// Loads BigScreen settings.
        /// </summary>
        Task<BigBoxSettings> LoadBigBoxSettingsAsync();

        /// <summary>
        /// Saves BigScreen settings.
        /// </summary>
        Task SaveBigBoxSettingsAsync(BigBoxSettings settings);

        /// <summary>
        /// Resets settings to defaults.
        /// </summary>
        Task<Settings> ResetToDefaultsAsync();
    }
}
