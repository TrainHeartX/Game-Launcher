using System;
using System.IO;
using System.Linq;

namespace GameLauncher.Core.Helpers
{
    /// <summary>
    /// Resuelve rutas de video de LaunchBox para un juego dado.
    /// Thread-safe, no crea objetos de UI.
    /// </summary>
    public static class VideoPathResolver
    {
        public static string? LaunchBoxPath { get; set; }

        private static readonly string TempVideoPath = Path.Combine(Path.GetTempPath(), "GameLauncher_Videos");

        /// <summary>
        /// Busca el archivo de video en disco y retorna una ruta con extensión .mp4
        /// que MediaElement pueda reproducir.
        /// </summary>
        public static string? Resolve(string? title, string? platform)
        {
            if (string.IsNullOrEmpty(LaunchBoxPath) || string.IsNullOrEmpty(platform) || string.IsNullOrEmpty(title))
                return null;

            string videoFolder = Path.Combine(LaunchBoxPath, "Videos", platform);
            if (!Directory.Exists(videoFolder))
                return null;

            // Strategy 1: Try exact name matches (fast path)
            var exactMatch = TryExactMatches(videoFolder, title);
            if (exactMatch != null)
                return EnsurePlayable(exactMatch, title);

            // Strategy 2: Enumerate files and find best match by normalized comparison
            var enumeratedMatch = TryEnumerateMatch(videoFolder, title);
            if (enumeratedMatch != null)
                return EnsurePlayable(enumeratedMatch, title);

            // Strategy 3: Check subdirectories (Theme, Trailer, etc.)
            try
            {
                foreach (var subDir in Directory.GetDirectories(videoFolder))
                {
                    var dirName = Path.GetFileName(subDir);
                    if (dirName.Equals("Recordings", StringComparison.OrdinalIgnoreCase))
                        continue;

                    exactMatch = TryExactMatches(subDir, title);
                    if (exactMatch != null)
                        return EnsurePlayable(exactMatch, title);

                    enumeratedMatch = TryEnumerateMatch(subDir, title);
                    if (enumeratedMatch != null)
                        return EnsurePlayable(enumeratedMatch, title);
                }
            }
            catch { }

            return null;
        }

        private static string? TryExactMatches(string folder, string title)
        {
            // LaunchBox uses different sanitization for videos vs images.
            // Try multiple variants:
            string[] titleVariants = {
                title,                                          // Original: "Batman: Arkham Origins"
                RemoveInvalidChars(title),                      // Removed:  "Batman Arkham Origins"
                ReplaceInvalidCharsWithUnderscore(title),       // Replaced: "Batman_ Arkham Origins"
                RemoveInvalidCharsAndCollapse(title),           // Collapsed:"Batman Arkham Origins"
            };

            string[] extensions = { ".% (ext)s", ".mp4", ".wmv", ".avi", ".mkv", ".flv", ".webm" };
            string[] suffixes = { "", "-01", "-02", "-03" };

            foreach (var variant in titleVariants.Distinct())
            {
                foreach (var suffix in suffixes)
                {
                    foreach (var ext in extensions)
                    {
                        string filePath = Path.Combine(folder, variant + suffix + ext);
                        if (File.Exists(filePath))
                            return filePath;
                    }
                }
            }

            return null;
        }

        private static string? TryEnumerateMatch(string folder, string title)
        {
            try
            {
                string normalizedTitle = NormalizeForComparison(title);

                // Get all video files and find the best match
                var files = Directory.EnumerateFiles(folder);

                // First pass: exact normalized match
                foreach (var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    // Strip trailing -01, -02 etc.
                    string baseName = StripSuffix(fileName);
                    string normalizedFile = NormalizeForComparison(baseName);

                    if (normalizedFile.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase))
                        return file;
                }

                // Second pass: file starts with normalized title (for "Game Name - Subtitle" patterns)
                string? bestMatch = null;
                int bestLength = int.MaxValue;

                foreach (var file in Directory.EnumerateFiles(folder))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string baseName = StripSuffix(fileName);
                    string normalizedFile = NormalizeForComparison(baseName);

                    if (normalizedFile.StartsWith(normalizedTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        // Prefer shortest match (most specific)
                        if (normalizedFile.Length < bestLength)
                        {
                            bestLength = normalizedFile.Length;
                            bestMatch = file;
                        }
                    }
                }

                return bestMatch;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Normalizes a string for comparison by lowering case and removing
        /// all non-alphanumeric characters.
        /// "Batman: Arkham Origins" → "batmanarkhamorigins"
        /// "Batman Arkham Origins"  → "batmanarkhamorigins"
        /// </summary>
        private static string NormalizeForComparison(string input)
        {
            var chars = input.Where(c => char.IsLetterOrDigit(c)).Select(char.ToLowerInvariant);
            return new string(chars.ToArray());
        }

        /// <summary>Strips -01, -02, etc. suffix from filenames.</summary>
        private static string StripSuffix(string name)
        {
            if (name.Length >= 3)
            {
                int dashPos = name.LastIndexOf('-');
                if (dashPos > 0 && dashPos >= name.Length - 3)
                {
                    string suffix = name.Substring(dashPos + 1);
                    if (suffix.All(char.IsDigit))
                        return name.Substring(0, dashPos);
                }
            }
            return name;
        }

        /// <summary>Removes characters invalid in filenames: : / \ ? * " < > |</summary>
        private static string RemoveInvalidChars(string input)
        {
            char[] invalid = { ':', '/', '\\', '?', '*', '"', '<', '>', '|' };
            return new string(input.Where(c => !invalid.Contains(c)).ToArray());
        }

        /// <summary>Removes invalid chars and collapses double spaces.</summary>
        private static string RemoveInvalidCharsAndCollapse(string input)
        {
            string result = RemoveInvalidChars(input);
            while (result.Contains("  "))
                result = result.Replace("  ", " ");
            return result.Trim();
        }

        /// <summary>Replaces invalid chars with underscore (for image-style sanitization).</summary>
        private static string ReplaceInvalidCharsWithUnderscore(string input)
        {
            string s = input;
            s = s.Replace(":", "_");
            s = s.Replace("/", "_");
            s = s.Replace("\\", "_");
            s = s.Replace("?", "_");
            s = s.Replace("*", "_");
            s = s.Replace("\"", "_");
            s = s.Replace("<", "_");
            s = s.Replace(">", "_");
            s = s.Replace("|", "_");
            s = s.Replace("'", "_");
            return s;
        }

        /// <summary>
        /// If file is not .mp4, copies it to temp with .mp4 extension
        /// so MediaElement can play it.
        /// </summary>
        private static string? EnsurePlayable(string filePath, string title)
        {
            if (filePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                return filePath;

            return EnsureMp4Copy(filePath, title);
        }

        private static string? EnsureMp4Copy(string originalPath, string title)
        {
            try
            {
                Directory.CreateDirectory(TempVideoPath);

                // Use a hash of the full path to avoid collisions
                string safeName = NormalizeForComparison(title);
                if (safeName.Length > 60) safeName = safeName.Substring(0, 60);
                string mp4Path = Path.Combine(TempVideoPath, safeName + ".mp4");

                if (File.Exists(mp4Path))
                {
                    var origInfo = new FileInfo(originalPath);
                    var copyInfo = new FileInfo(mp4Path);
                    if (origInfo.Length == copyInfo.Length)
                        return mp4Path;
                    File.Delete(mp4Path);
                }

                File.Copy(originalPath, mp4Path);
                return mp4Path;
            }
            catch
            {
                return null;
            }
        }
    }
}
