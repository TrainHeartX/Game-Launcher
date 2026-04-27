using System.Threading.Tasks;
using GameLauncher.Core.Models;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Interface for launching games with emulators.
    /// </summary>
    public interface IEmulatorLauncher
    {
        /// <summary>
        /// Launches a game with its configured emulator.
        /// </summary>
        /// <param name="game">The game to launch</param>
        /// <returns>Result of the launch operation including play time</returns>
        Task<LaunchResult> LaunchGameAsync(Game game);

        /// <summary>
        /// Launches a game with a specific emulator (overrides game's default).
        /// </summary>
        Task<LaunchResult> LaunchGameWithEmulatorAsync(Game game, Emulator emulator);

        /// <summary>
        /// Tests if a game can be launched (validates ROM exists, emulator configured, etc.)
        /// </summary>
        Task<(bool CanLaunch, string? Reason)> CanLaunchGameAsync(Game game);

        /// <summary>
        /// Kills the currently running emulator/game process (if any).
        /// </summary>
        void KillCurrentProcess();
    }
}
