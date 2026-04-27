using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GameLauncher.Core.Models;

namespace GameLauncher.Desktop.Converters
{
    /// <summary>
    /// Convierte un objeto Game a su imagen de portada (Box Art).
    /// Busca en LaunchBox\Images\{Platform}\Box - Front\
    /// </summary>
    public class GameImageConverter : IValueConverter
    {
        // Ruta de LaunchBox (se establece al iniciar la aplicación)
        public static string? LaunchBoxPath { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Game game)
                return null;

            if (string.IsNullOrEmpty(LaunchBoxPath))
                return null;

            if (string.IsNullOrEmpty(game.Platform) || string.IsNullOrEmpty(game.Title))
                return null;

            try
            {
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"GameImageConverter: Buscando imagen para '{game.Title}' ({game.Platform})");
                System.Diagnostics.Debug.WriteLine($"  LaunchBoxPath: {LaunchBoxPath}");

                // Buscar en Box - Front primero
                string boxFrontFolder = Path.Combine(LaunchBoxPath, "Images", game.Platform, "Box - Front");
                System.Diagnostics.Debug.WriteLine($"  Buscando en: {boxFrontFolder}");
                System.Diagnostics.Debug.WriteLine($"  ¿Carpeta existe?: {Directory.Exists(boxFrontFolder)}");

                if (Directory.Exists(boxFrontFolder))
                {
                    var image = FindImageInFolder(boxFrontFolder, game.Title);
                    if (image != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✓ Imagen encontrada!");
                        return image;
                    }
                }

                // Intentar en Box - Front - Reconstructed (con subcarpetas de región)
                string boxFrontReconstructed = Path.Combine(LaunchBoxPath, "Images", game.Platform, "Box - Front - Reconstructed");

                if (Directory.Exists(boxFrontReconstructed))
                {
                    // Buscar en todas las subcarpetas de región
                    var regionFolders = Directory.GetDirectories(boxFrontReconstructed);
                    foreach (var regionFolder in regionFolders)
                    {
                        var image = FindImageInFolder(regionFolder, game.Title);
                        if (image != null)
                            return image;
                    }
                }
            }
            catch
            {
                // Silenciar errores de búsqueda
            }

            return null;
        }

        private BitmapImage? FindImageInFolder(string folder, string gameTitle)
        {
            if (!Directory.Exists(folder))
                return null;

            // Sanitizar el título
            string sanitizedTitle = SanitizeFileName(gameTitle);

            // LaunchBox usa el formato: "{Título}-01.png", "{Título}-02.png", etc.
            string[] suffixes = { "-01", "-02", "-03", "-04", "-05", "" };
            string[] extensions = { ".png", ".jpg", ".jpeg" };

            // Buscar con nombre exacto
            foreach (var suffix in suffixes)
            {
                foreach (var ext in extensions)
                {
                    string fileName = sanitizedTitle + suffix + ext;
                    string fullPath = Path.Combine(folder, fileName);

                    if (File.Exists(fullPath))
                    {
                        return LoadImage(fullPath);
                    }
                }
            }

            // Si no se encontró con nombre exacto, buscar archivos que empiecen con el título
            try
            {
                var matchingFiles = Directory.GetFiles(folder)
                    .Where(f =>
                    {
                        string fileName = Path.GetFileNameWithoutExtension(f);
                        string ext = Path.GetExtension(f).ToLowerInvariant();
                        return fileName.StartsWith(sanitizedTitle, StringComparison.OrdinalIgnoreCase) &&
                               (ext == ".png" || ext == ".jpg" || ext == ".jpeg");
                    })
                    .OrderBy(f => f)
                    .ToList();

                if (matchingFiles.Count > 0)
                {
                    return LoadImage(matchingFiles[0]);
                }
            }
            catch
            {
                // Ignorar errores
            }

            return null;
        }

        private BitmapImage? LoadImage(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.DecodePixelWidth = 100; // Ancho para lista
                bitmap.EndInit();
                bitmap.Freeze(); // Thread-safe
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            // LaunchBox reemplaza estos caracteres con _
            string sanitized = fileName;
            sanitized = sanitized.Replace(":", "_");
            sanitized = sanitized.Replace("/", "_");
            sanitized = sanitized.Replace("\\", "_");
            sanitized = sanitized.Replace("?", "_");
            sanitized = sanitized.Replace("*", "_");
            sanitized = sanitized.Replace("\"", "_");
            sanitized = sanitized.Replace("<", "_");
            sanitized = sanitized.Replace(">", "_");
            sanitized = sanitized.Replace("|", "_");

            return sanitized;
        }
    }
}
