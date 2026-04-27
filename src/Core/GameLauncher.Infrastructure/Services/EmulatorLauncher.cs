using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameLauncher.Core.Models;
using GameLauncher.Data.Xml;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Launches games with their configured emulators.
    /// Handles command line building, process management, and time tracking.
    /// </summary>
    public class EmulatorLauncher : IEmulatorLauncher
    {
        private static readonly string[] CompressedExtensions = { ".rar", ".zip", ".7z" };
        private const string TempFolderName = "GameLauncher_Roms";

        private readonly XmlDataContext _dataContext;
        private readonly string _launchBoxPath;
        private List<Emulator>? _emulators;
        private List<EmulatorPlatform>? _emulatorMappings;
        private Process? _currentProcess;

        public EmulatorLauncher(XmlDataContext dataContext, string launchBoxPath)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _launchBoxPath = launchBoxPath ?? throw new ArgumentNullException(nameof(launchBoxPath));
        }

        public async Task<LaunchResult> LaunchGameAsync(Game game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            // Load emulators if not already loaded
            LoadEmulatorsIfNeeded();

            // Get emulator and platform mapping for this game
            var (emulator, platformMapping) = GetEmulatorForGame(game);

            // No emulator found: try direct launch (Windows games, standalone .exe, etc.)
            if (emulator == null)
                return await LaunchDirectAsync(game);

            return await LaunchGameWithEmulatorAsync(game, emulator, platformMapping);
        }

        public async Task<LaunchResult> LaunchGameWithEmulatorAsync(Game game, Emulator emulator)
        {
            // Find the platform mapping for the command line
            LoadEmulatorsIfNeeded();
            var platformMapping = GetPlatformMapping(game, emulator);
            return await LaunchGameWithEmulatorAsync(game, emulator, platformMapping);
        }

        private async Task<LaunchResult> LaunchGameWithEmulatorAsync(Game game, Emulator emulator, EmulatorPlatform? platformMapping)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (emulator == null)
                throw new ArgumentNullException(nameof(emulator));

            // Validate ROM file exists
            if (string.IsNullOrWhiteSpace(game.ApplicationPath))
                return LaunchResult.Failure("Game has no ROM path configured");

            // Validate emulator exists
            if (string.IsNullOrWhiteSpace(emulator.ApplicationPath))
                return LaunchResult.Failure("Emulator has no application path configured");

            var emulatorPath = ResolvePath(emulator.ApplicationPath);

            if (!File.Exists(emulatorPath))
                return LaunchResult.Failure($"Emulator not found: {emulatorPath}");

            // Resolve ROM path: use direct file or extract from compressed archive
            string romPath = ResolvePath(game.ApplicationPath);
            string? tempExtractDir = null;

            if (!File.Exists(romPath))
            {
                // Check if a compressed version exists
                var compressedPath = FindCompressedRom(romPath);
                if (compressedPath == null)
                    return LaunchResult.Failure($"ROM file not found: {romPath}");

                // Extract and get the ROM path
                var expectedRomName = Path.GetFileName(romPath);
                var extractResult = await ExtractRomAsync(compressedPath, expectedRomName);
                if (extractResult == null)
                    return LaunchResult.Failure($"Failed to extract ROM from: {compressedPath}");

                romPath = extractResult.Value.RomPath;
                tempExtractDir = extractResult.Value.TempDir;
            }

            // Build command line with the resolved ROM path
            var commandLine = BuildCommandLine(emulator, game, platformMapping, romPath);

            // Launch the emulator
            try
            {
                var startTime = DateTime.UtcNow;

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = emulatorPath,
                    Arguments = commandLine,
                    // BUG-08 FIX: UseShellExecute=false is needed for WaitForExitAsync and
                    // HideConsole, but breaks emulators that launch via file-type associations.
                    // Compromise: use false (allows tracking) but fall back on process-launch failure.
                    UseShellExecute = false,
                    CreateNoWindow = emulator.HideConsole,
                    WorkingDirectory = Path.GetDirectoryName(emulatorPath) ?? ""
                };

                Process? process = null;
                try
                {
                    process = Process.Start(processStartInfo);
                }
                catch
                {
                    // BUG-08: Retry with UseShellExecute=true (handles .lnk, file associations, UAC)
                    processStartInfo.UseShellExecute = true;
                    processStartInfo.CreateNoWindow = false; // must be false with UseShellExecute=true
                    process = Process.Start(processStartInfo);
                }

                if (process == null)
                {
                    CleanupTempDir(tempExtractDir);
                    return LaunchResult.Failure("Failed to start emulator process");
                }

                _currentProcess = process;
                try
                {
                    // Wait for process to exit (or be killed externally)
                    await process.WaitForExitAsync();

                    var endTime = DateTime.UtcNow;
                    var playTimeSeconds = (int)(endTime - startTime).TotalSeconds;

                    return LaunchResult.Successful(playTimeSeconds, startTime, endTime, process.ExitCode);
                }
                finally
                {
                    _currentProcess = null;
                    process.Dispose();
                }
            }
            catch (Exception ex)
            {
                return LaunchResult.Failure($"Failed to launch emulator: {ex.Message}");
            }
            finally
            {
                CleanupTempDir(tempExtractDir);
            }
        }

        public void KillCurrentProcess()
        {
            try
            {
                var proc = _currentProcess;
                if (proc != null && !proc.HasExited)
                {
                    proc.Kill(entireProcessTree: true);
                    Debug.WriteLine("[EmulatorLauncher] Process killed by user (Select+Start combo)");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EmulatorLauncher] Error killing process: {ex.Message}");
            }
        }

        public async Task<(bool CanLaunch, string? Reason)> CanLaunchGameAsync(Game game)
        {
            if (game == null)
                return (false, "Game is null");

            // Check ROM path
            if (string.IsNullOrWhiteSpace(game.ApplicationPath))
                return (false, "Game has no ROM path configured");

            var resolvedRomPath = ResolvePath(game.ApplicationPath);
            if (!File.Exists(resolvedRomPath))
            {
                // Check if a compressed version exists
                if (FindCompressedRom(resolvedRomPath) == null)
                    return (false, $"ROM file not found: {resolvedRomPath}");
            }

            // Load emulators if needed
            LoadEmulatorsIfNeeded();

            // Check emulator — if none found, allow direct launch for executable files
            var (emulator, _) = GetEmulatorForGame(game);
            if (emulator == null)
            {
                // No emulator: valid if the application itself is an executable
                if (IsDirectlyLaunchable(resolvedRomPath))
                    return await Task.FromResult<(bool, string?)>((true, null));
                return (false, $"No emulator configured for platform: {game.Platform}");
            }

            if (string.IsNullOrWhiteSpace(emulator.ApplicationPath))
                return (false, "Emulator has no application path configured");

            var resolvedEmulatorPath = ResolvePath(emulator.ApplicationPath);
            if (!File.Exists(resolvedEmulatorPath))
                return (false, $"Emulator not found: {resolvedEmulatorPath}");

            return await Task.FromResult<(bool, string?)>((true, null));
        }

        #region Private Methods

        private static readonly string[] DirectLaunchExtensions = { ".exe", ".bat", ".cmd", ".lnk", ".url" };

        private static bool IsDirectlyLaunchable(string resolvedPath)
        {
            var ext = Path.GetExtension(resolvedPath);
            return !string.IsNullOrEmpty(ext) &&
                   DirectLaunchExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Launches a game directly without an emulator (for Windows games, standalone .exe, etc.)
        /// </summary>
        private async Task<LaunchResult> LaunchDirectAsync(Game game)
        {
            if (string.IsNullOrWhiteSpace(game.ApplicationPath))
                return LaunchResult.Failure("Game has no application path configured");

            var appPath = ResolvePath(game.ApplicationPath);

            if (!File.Exists(appPath))
                return LaunchResult.Failure($"Game file not found: {appPath}");

            if (!IsDirectlyLaunchable(appPath))
                return LaunchResult.Failure($"No emulator configured for platform: {game.Platform}");

            try
            {
                var startTime = DateTime.UtcNow;

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = appPath,
                    Arguments = game.CommandLine ?? "",
                    UseShellExecute = true, // Needed for .lnk and .url files
                    WorkingDirectory = Path.GetDirectoryName(appPath) ?? ""
                };

                var process = Process.Start(processStartInfo);

                if (process == null)
                    return LaunchResult.Failure("Failed to start game process");

                _currentProcess = process;
                try
                {
                    await process.WaitForExitAsync();

                    var endTime = DateTime.UtcNow;
                    var playTimeSeconds = (int)(endTime - startTime).TotalSeconds;

                    return LaunchResult.Successful(playTimeSeconds, startTime, endTime, process.ExitCode);
                }
                finally
                {
                    _currentProcess = null;
                    process.Dispose();
                }
            }
            catch (Exception ex)
            {
                return LaunchResult.Failure($"Failed to launch game: {ex.Message}");
            }
        }

        private void LoadEmulatorsIfNeeded()
        {
            if (_emulators == null || _emulatorMappings == null)
            {
                var (emulators, mappings) = _dataContext.LoadEmulators();
                _emulators = emulators;
                _emulatorMappings = mappings;
            }
        }

        private (Emulator? Emulator, EmulatorPlatform? Mapping) GetEmulatorForGame(Game game)
        {
            if (_emulatorMappings == null || _emulators == null)
                return (null, null);

            // First, try to get the emulator from the game's Emulator property
            if (!string.IsNullOrWhiteSpace(game.Emulator))
            {
                var emulator = _emulators.FirstOrDefault(e =>
                    e.ID.Equals(game.Emulator, StringComparison.OrdinalIgnoreCase));
                if (emulator != null)
                {
                    // Find platform mapping for this emulator + platform combo
                    var mapping = _emulatorMappings.FirstOrDefault(m =>
                        m.Emulator.Equals(emulator.ID, StringComparison.OrdinalIgnoreCase) &&
                        m.Platform.Equals(game.Platform, StringComparison.OrdinalIgnoreCase));
                    return (emulator, mapping);
                }
            }

            // Fall back to platform mapping (find default emulator for platform)
            var defaultMapping = _emulatorMappings
                .Where(m => m.Platform.Equals(game.Platform, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(m => m.Default)
                .FirstOrDefault();

            if (defaultMapping == null)
                return (null, null);

            var defaultEmulator = _emulators.FirstOrDefault(e =>
                e.ID.Equals(defaultMapping.Emulator, StringComparison.OrdinalIgnoreCase));

            return (defaultEmulator, defaultMapping);
        }

        /// <summary>
        /// Finds the EmulatorPlatform mapping for a given game and emulator.
        /// Used when LaunchGameWithEmulatorAsync is called directly with a specific emulator.
        /// </summary>
        private EmulatorPlatform? GetPlatformMapping(Game game, Emulator emulator)
        {
            if (_emulatorMappings == null)
                return null;

            return _emulatorMappings.FirstOrDefault(m =>
                m.Emulator.Equals(emulator.ID, StringComparison.OrdinalIgnoreCase) &&
                m.Platform.Equals(game.Platform, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Finds a compressed archive (.rar, .zip, .7z) that matches the expected ROM file.
        /// Returns null if the ROM file already exists or no compressed version is found.
        /// </summary>
        private static string? FindCompressedRom(string applicationPath)
        {
            if (File.Exists(applicationPath))
                return null;

            var directory = Path.GetDirectoryName(applicationPath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var baseName = Path.GetFileNameWithoutExtension(applicationPath);

            foreach (var ext in CompressedExtensions)
            {
                var compressedPath = Path.Combine(directory, baseName + ext);
                if (File.Exists(compressedPath))
                    return compressedPath;
            }

            return null;
        }

        /// <summary>
        /// Extracts a compressed ROM archive to a temporary directory.
        /// Returns the path to the extracted ROM file and the temp directory for cleanup.
        /// </summary>
        private static async Task<(string RomPath, string TempDir)?> ExtractRomAsync(string compressedPath, string expectedRomName)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), TempFolderName, Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                await Task.Run(() =>
                {
                    using var archive = ArchiveFactory.Open(compressedPath);
                    foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                    {
                        entry.WriteToDirectory(tempDir, new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                });

                // First, try to find the exact expected ROM file
                var expectedExtension = Path.GetExtension(expectedRomName);
                var exactMatch = Directory.GetFiles(tempDir, expectedRomName, SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (exactMatch != null)
                    return (exactMatch, tempDir);

                // Fall back: find any file with the expected extension
                if (!string.IsNullOrEmpty(expectedExtension))
                {
                    var extMatch = Directory.GetFiles(tempDir, "*" + expectedExtension, SearchOption.AllDirectories)
                        .FirstOrDefault();
                    if (extMatch != null)
                        return (extMatch, tempDir);
                }

                // Last resort: return the first file found
                var anyFile = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (anyFile != null)
                    return (anyFile, tempDir);

                // No files extracted
                CleanupTempDir(tempDir);
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting ROM from archive: {ex.Message}");
                CleanupTempDir(tempDir);
                return null;
            }
        }

        private static void CleanupTempDir(string? tempDir)
        {
            if (string.IsNullOrEmpty(tempDir) || !Directory.Exists(tempDir))
                return;

            try
            {
                Directory.Delete(tempDir, recursive: true);
            }
            catch (Exception ex)
            {
                // Best effort cleanup — temp files will be cleaned up by OS eventually
                System.Diagnostics.Debug.WriteLine($"Failed to cleanup temp dir '{tempDir}': {ex.Message}");
            }
        }

        /// <summary>
        /// Resolves a path that may be relative to the LaunchBox directory into an absolute path.
        /// </summary>
        private string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (Path.IsPathRooted(path))
                return path;

            return Path.Combine(_launchBoxPath, path);
        }

        private string BuildCommandLine(Emulator emulator, Game game, EmulatorPlatform? platformMapping, string? romPathOverride = null)
        {
            // Priority: game.CommandLine (only if UseCustomCommandLine=true) > emulatorPlatform.CommandLine > emulator.CommandLine
            // This matches LaunchBox behavior exactly: game-level command line is only applied when UseCustomCommandLine is set.
            var commandLine = (game.UseCustomCommandLine && !string.IsNullOrWhiteSpace(game.CommandLine))
                ? game.CommandLine
                : !string.IsNullOrWhiteSpace(platformMapping?.CommandLine)
                    ? platformMapping!.CommandLine
                    : emulator.CommandLine ?? "";

            // Get emulator directory for {emudir} placeholder
            var resolvedEmuPath = ResolvePath(emulator.ApplicationPath ?? "");
            var emulatorDir = Path.GetDirectoryName(resolvedEmuPath) ?? "";
            var romPath = romPathOverride ?? ResolvePath(game.ApplicationPath ?? "");
            var romDir = Path.GetDirectoryName(romPath) ?? "";
            var romFile = Path.GetFileName(romPath);
            var romFileNoExt = Path.GetFileNameWithoutExtension(romPath);

            // Check if command line contains ROM placeholder before substitution
            bool hasRomPlaceholder = commandLine.Contains("{rom}") ||
                                     commandLine.Contains("{romraw}") ||
                                     commandLine.Contains("{romfile}") ||
                                     commandLine.Contains("{romname}") ||
                                     commandLine.Contains("{rompath}");

            // Apply placeholder substitutions (LaunchBox compatible)
            commandLine = commandLine.Replace("{rom}", QuotePath(romPath, emulator.NoQuotes));
            commandLine = commandLine.Replace("{romraw}", romPath);
            commandLine = commandLine.Replace("{rompath}", QuotePath(romDir, emulator.NoQuotes));
            commandLine = commandLine.Replace("{romfile}", romFile);
            commandLine = commandLine.Replace("{romname}", romFileNoExt);
            commandLine = commandLine.Replace("{emudir}", QuotePath(emulatorDir, emulator.NoQuotes));
            commandLine = commandLine.Replace("{emupath}", QuotePath(resolvedEmuPath, emulator.NoQuotes));
            commandLine = commandLine.Replace("{platform}", game.Platform);
            commandLine = commandLine.Replace("{title}", game.Title);

            // Handle special emulator options
            if (emulator.FileNameWithoutExtensionAndPath)
            {
                // If this option is set, the entire command line is just the ROM name without extension
                commandLine = romFileNoExt;
            }
            else if (!hasRomPlaceholder)
            {
                // BUG-09 FIX: avoid double-space when commandLine is empty
                // LaunchBox behavior: if no ROM placeholder exists, append ROM path at the end
                var romArg = QuotePath(romPath, emulator.NoQuotes);
                commandLine = string.IsNullOrWhiteSpace(commandLine)
                    ? romArg
                    : commandLine.TrimEnd() + " " + romArg;
            }

            return commandLine;
        }

        private string QuotePath(string path, bool noQuotes)
        {
            if (noQuotes || string.IsNullOrWhiteSpace(path))
                return path;

            // Add quotes if path contains spaces
            if (path.Contains(' ') && !path.StartsWith("\""))
                return $"\"{path}\"";

            return path;
        }

        #endregion
    }
}
