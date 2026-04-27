using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GameLauncher.Core.Models;
using GameLauncher.Data.Xml;

namespace GameLauncher.Infrastructure.Services
{
    public class AndroidExportService
    {
        private readonly XmlDataContext _dataContext;
        private readonly string _launchBoxPath;

        public AndroidExportService(XmlDataContext dataContext, string launchBoxPath)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _launchBoxPath = launchBoxPath ?? throw new ArgumentNullException(nameof(launchBoxPath));
        }

        public async Task<string> ExportGamesToZipAsync(IEnumerable<Game> games, string outputZipPath, Action<string>? progressCallback = null)
        {
            var exportTempDir = Path.Combine(Path.GetTempPath(), "GameLauncher_AndroidExport", Guid.NewGuid().ToString());
            Directory.CreateDirectory(exportTempDir);

            try
            {
                var library = new AndroidLibraryManifest();
                var (emulators, emulatorMappings) = _dataContext.LoadEmulators();

                foreach (var game in games)
                {
                    progressCallback?.Invoke($"Exportando: {game.Title}");
                    var exportedGame = await ExportSingleGameAsync(game, exportTempDir, emulators, emulatorMappings);
                    library.Games.Add(exportedGame);
                }

                progressCallback?.Invoke("Generando base de datos...");
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(library, jsonOptions);
                File.WriteAllText(Path.Combine(exportTempDir, "AndroidLibrary.json"), jsonString);

                progressCallback?.Invoke("Comprimiendo paquete...");
                if (File.Exists(outputZipPath)) File.Delete(outputZipPath);
                ZipFile.CreateFromDirectory(exportTempDir, outputZipPath, CompressionLevel.Fastest, includeBaseDirectory: false);

                progressCallback?.Invoke("Exportación completada.");
                return outputZipPath;
            }
            finally
            {
                if (Directory.Exists(exportTempDir))
                    Directory.Delete(exportTempDir, recursive: true);
            }
        }

        private async Task<AndroidExportedGame> ExportSingleGameAsync(Game game, string baseExportDir, List<Emulator> emulators, List<EmulatorPlatform> mappings)
        {
            var result = new AndroidExportedGame
            {
                Id = game.ID,
                Title = game.Title,
                Platform = game.Platform,
                Developer = game.Developer,
                Publisher = game.Publisher,
                ReleaseYear = game.ReleaseDate?.Year,
                Genre = game.Genre,
                Completed = game.Completed,
                Favorite = game.Favorite
            };

            var (emulator, _) = GetEmulatorForGame(game, emulators, mappings);
            
            // Mark if it uses RetroArch
            bool isRetroArch = emulator?.Title?.Contains("RetroArch", StringComparison.OrdinalIgnoreCase) == true;
            result.IsRetroArch = isRetroArch;

            // 1. Export ROM
            if (!string.IsNullOrWhiteSpace(game.ApplicationPath))
            {
                var romSourcePath = ResolvePath(game.ApplicationPath);
                if (File.Exists(romSourcePath))
                {
                    var romFileName = Path.GetFileName(romSourcePath);
                    var romDestDir = Path.Combine(baseExportDir, "Roms", SanitizeFolderName(game.Platform ?? "Unknown"));
                    Directory.CreateDirectory(romDestDir);
                    var romDestPath = Path.Combine(romDestDir, romFileName);
                    
                    File.Copy(romSourcePath, romDestPath, true);
                    result.RomPath = $"Roms/{SanitizeFolderName(game.Platform ?? "Unknown")}/{romFileName}";

                    // 2. If it's RetroArch, try to find and export Save/State files
                    if (isRetroArch && emulator != null)
                    {
                        var emulatorPath = ResolvePath(emulator.ApplicationPath ?? "");
                        var emulatorDir = Path.GetDirectoryName(emulatorPath);
                        if (!string.IsNullOrEmpty(emulatorDir))
                        {
                            var savesDir = Path.Combine(emulatorDir, "saves");
                            var statesDir = Path.Combine(emulatorDir, "states");

                            var romNoExt = Path.GetFileNameWithoutExtension(romSourcePath);

                            // Find Saves
                            if (Directory.Exists(savesDir))
                            {
                                var srmFiles = Directory.GetFiles(savesDir, $"{romNoExt}*.srm", SearchOption.AllDirectories);
                                foreach (var srm in srmFiles)
                                {
                                    var destSavesDir = Path.Combine(baseExportDir, "Saves");
                                    Directory.CreateDirectory(destSavesDir);
                                    File.Copy(srm, Path.Combine(destSavesDir, Path.GetFileName(srm)), true);
                                    result.HasSaves = true;
                                }
                            }

                            // Find States
                            if (Directory.Exists(statesDir))
                            {
                                var stateFiles = Directory.GetFiles(statesDir, $"{romNoExt}*.state*", SearchOption.AllDirectories);
                                foreach (var state in stateFiles)
                                {
                                    var destStatesDir = Path.Combine(baseExportDir, "States");
                                    Directory.CreateDirectory(destStatesDir);
                                    File.Copy(state, Path.Combine(destStatesDir, Path.GetFileName(state)), true);
                                }
                            }
                        }
                    }
                }
            }

            // 3. Export Images (Box Front, Clear Logo)
            result.BoxFrontPath = ExportImage(game.ID, "Box - Front", baseExportDir, game.Platform);
            result.ClearLogoPath = ExportImage(game.ID, "Clear Logo", baseExportDir, game.Platform);

            return await Task.FromResult(result);
        }

        private string? ExportImage(string gameId, string imageType, string baseExportDir, string? platform)
        {
            if (string.IsNullOrWhiteSpace(platform)) return null;

            var imagesDir = Path.Combine(_launchBoxPath, "Images", platform, imageType);
            if (!Directory.Exists(imagesDir)) return null;

            // LaunchBox names images by Title or GameId. The safest way is to search by the known patterns.
            // For simplicity in this exporter, we will look for files starting with the GameId (LaunchBox 13+ standard) 
            // or we would need the game title.
            // Let's search all files and find one containing the ID.
            var files = Directory.GetFiles(imagesDir, $"*-{gameId}.*", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                // Fallback: search by ID at the start (sometimes formatted as GameId-01.png)
                files = Directory.GetFiles(imagesDir, $"{gameId}*.*", SearchOption.TopDirectoryOnly);
            }

            if (files.Length > 0)
            {
                var sourceImage = files[0];
                var destImageDir = Path.Combine(baseExportDir, "Images", SanitizeFolderName(platform), SanitizeFolderName(imageType));
                Directory.CreateDirectory(destImageDir);
                
                var destImageName = Path.GetFileName(sourceImage);
                File.Copy(sourceImage, Path.Combine(destImageDir, destImageName), true);
                
                return $"Images/{SanitizeFolderName(platform)}/{SanitizeFolderName(imageType)}/{destImageName}";
            }

            return null;
        }

        private (Emulator? Emulator, EmulatorPlatform? Mapping) GetEmulatorForGame(Game game, List<Emulator> emulators, List<EmulatorPlatform> mappings)
        {
            if (!string.IsNullOrWhiteSpace(game.Emulator))
            {
                var emu = emulators.FirstOrDefault(e => e.ID.Equals(game.Emulator, StringComparison.OrdinalIgnoreCase));
                if (emu != null)
                {
                    var map = mappings.FirstOrDefault(m => m.Emulator.Equals(emu.ID, StringComparison.OrdinalIgnoreCase) && m.Platform.Equals(game.Platform, StringComparison.OrdinalIgnoreCase));
                    return (emu, map);
                }
            }

            var defaultMapping = mappings.Where(m => m.Platform.Equals(game.Platform, StringComparison.OrdinalIgnoreCase))
                                         .OrderByDescending(m => m.Default).FirstOrDefault();
            if (defaultMapping != null)
            {
                var emu = emulators.FirstOrDefault(e => e.ID.Equals(defaultMapping.Emulator, StringComparison.OrdinalIgnoreCase));
                return (emu, defaultMapping);
            }

            return (null, null);
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;
            if (Path.IsPathRooted(path)) return path;
            return Path.Combine(_launchBoxPath, path);
        }

        private string SanitizeFolderName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
