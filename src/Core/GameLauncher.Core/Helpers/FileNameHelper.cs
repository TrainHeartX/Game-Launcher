using System.IO;

namespace GameLauncher.Core.Helpers
{
    /// <summary>
    /// Utility methods for file name operations compatible with LaunchBox naming conventions.
    /// </summary>
    public static class FileNameHelper
    {
        /// <summary>
        /// Sanitizes a game title to match the file naming convention used by LaunchBox
        /// when storing images, videos, and other media on disk.
        /// Replaces characters that are invalid in Windows file names with underscores.
        /// </summary>
        /// <param name="fileName">The game title or other string to sanitize.</param>
        /// <returns>A sanitized string safe to use as a file name component.</returns>
        public static string SanitizeForLaunchBox(string fileName)
        {
            // LaunchBox replaces these specific characters — order matters, backslash first
            return fileName
                .Replace("\\", "_")
                .Replace(":", "_")
                .Replace("/", "_")
                .Replace("?", "_")
                .Replace("*", "_")
                .Replace("\"", "_")
                .Replace("<", "_")
                .Replace(">", "_")
                .Replace("|", "_")
                .Replace("'", "_");
        }

        /// <summary>
        /// Returns the first image file found in the given directory,
        /// or null if the directory does not exist or contains no supported images.
        /// Supported extensions: .png, .jpg, .jpeg, .gif, .bmp
        /// </summary>
        public static string? FindFirstImage(string directory)
        {
            if (!Directory.Exists(directory))
                return null;

            string[] extensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };
            foreach (var ext in extensions)
            {
                var files = Directory.GetFiles(directory, "*" + ext);
                if (files.Length > 0)
                    return files[0];
            }
            return null;
        }

        /// <summary>
        /// Searches for an image file in a folder using the LaunchBox naming pattern:
        /// {sanitizedTitle}{suffix}{ext}
        /// Tries suffixes "-01", "-02", "-03", "" in order.
        /// </summary>
        public static string? FindImageInFolder(string folder, string sanitizedTitle)
        {
            if (!Directory.Exists(folder))
                return null;

            string[] suffixes = { "-01", "-02", "-03", "" };
            string[] extensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

            foreach (var suffix in suffixes)
            {
                foreach (var ext in extensions)
                {
                    var filePath = Path.Combine(folder, sanitizedTitle + suffix + ext);
                    if (File.Exists(filePath))
                        return filePath;
                }
            }
            return null;
        }
    }
}
