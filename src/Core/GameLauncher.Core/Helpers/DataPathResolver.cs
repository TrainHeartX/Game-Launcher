using System;
using System.IO;

namespace GameLauncher.Core.Helpers
{
    public static class DataPathResolver
    {
        public static string? FindProjectDataPath()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            while (dir != null)
            {
                var candidate = Path.Combine(dir, "Data");
                if (File.Exists(Path.Combine(candidate, "Platforms.xml")))
                    return candidate;
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }
    }
}
