using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GameLauncher.Core.Models;

namespace GameLauncher.Android.Services
{
    public class AndroidImportService
    {
        private readonly HttpClient _httpClient;

        public AndroidImportService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<AndroidLibraryManifest?> ImportFromLocalServerAsync(string ipAddress, Action<string>? progress = null)
        {
            var url = $"http://{ipAddress}:8080/download";
            var tempZip = Path.Combine(FileSystem.CacheDirectory, "sync.zip");

            try
            {
                progress?.Invoke("Conectando con la PC...");
                var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                progress?.Invoke("Descargando paquete de sincronización...");
                using (var fs = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }

                return await ImportFromZipAsync(tempZip, progress);
            }
            finally
            {
                if (File.Exists(tempZip))
                    File.Delete(tempZip);
            }
        }

        public async Task<AndroidLibraryManifest?> ImportFromZipAsync(string zipPath, Action<string>? progress = null)
        {
            progress?.Invoke("Extrayendo paquete...");
            var extractDir = Path.Combine(FileSystem.AppDataDirectory, "ImportedLibrary");
            
            // Extract the ZIP completely into AppData
            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);
            
            Directory.CreateDirectory(extractDir);
            ZipFile.ExtractToDirectory(zipPath, extractDir, overwriteFiles: true);

            progress?.Invoke("Procesando partidas guardadas (Saves)...");
            await ProcessRetroArchSavesAsync(extractDir);

            progress?.Invoke("Leyendo biblioteca de juegos...");
            var jsonPath = Path.Combine(extractDir, "AndroidLibrary.json");
            if (File.Exists(jsonPath))
            {
                var json = await File.ReadAllTextAsync(jsonPath);
                var manifest = JsonSerializer.Deserialize<AndroidLibraryManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return manifest;
            }

            return null;
        }

        private async Task ProcessRetroArchSavesAsync(string extractDir)
        {
            var publicRetroArchDir = "/storage/emulated/0/RetroArch";
            var destSavesDir = Path.Combine(publicRetroArchDir, "saves");
            var destStatesDir = Path.Combine(publicRetroArchDir, "states");

            var importedSavesDir = Path.Combine(extractDir, "Saves");
            var importedStatesDir = Path.Combine(extractDir, "States");

            bool hasPermission = true; // We assume Permissions.StorageWrite is already granted by the UI.

            await Task.Run(() =>
            {
                if (hasPermission && Directory.Exists(publicRetroArchDir))
                {
                    if (Directory.Exists(importedSavesDir))
                    {
                        Directory.CreateDirectory(destSavesDir);
                        foreach (var file in Directory.GetFiles(importedSavesDir))
                        {
                            var dest = Path.Combine(destSavesDir, Path.GetFileName(file));
                            File.Copy(file, dest, true);
                        }
                    }

                    if (Directory.Exists(importedStatesDir))
                    {
                        Directory.CreateDirectory(destStatesDir);
                        foreach (var file in Directory.GetFiles(importedStatesDir))
                        {
                            var dest = Path.Combine(destStatesDir, Path.GetFileName(file));
                            File.Copy(file, dest, true);
                        }
                    }
                }
            });
        }
    }
}
