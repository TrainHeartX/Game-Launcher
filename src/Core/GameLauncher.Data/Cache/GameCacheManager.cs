using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GameLauncher.Core.Models;
using GameLauncher.Data.Xml;

namespace GameLauncher.Data.Cache
{
    /// <summary>
    /// Manages in-memory caching of game data to avoid re-parsing large XML files.
    /// Monitors file system changes and invalidates cache when XML files are modified.
    /// </summary>
    public class GameCacheManager : IDisposable
    {
        private readonly XmlDataContext _dataContext;
        private readonly Dictionary<string, List<Game>> _gamesCache;
        private readonly Dictionary<string, DateTime> _cacheTimestamps;
        private readonly ReaderWriterLockSlim _cacheLock;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly string _platformsDirectory;

        private List<Platform>? _platformsCache;
        private DateTime? _platformsCacheTimestamp;

        public GameCacheManager(XmlDataContext dataContext, string dataPath)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _gamesCache = new Dictionary<string, List<Game>>(StringComparer.OrdinalIgnoreCase);
            _cacheTimestamps = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            _cacheLock = new ReaderWriterLockSlim();

            _platformsDirectory = Path.Combine(dataPath, "Platforms");

            // Set up file system watcher to detect changes
            _fileSystemWatcher = new FileSystemWatcher
            {
                Path = dataPath,
                Filter = "*.xml",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                IncludeSubdirectories = true
            };

            _fileSystemWatcher.Changed += OnFileChanged;
            _fileSystemWatcher.Created += OnFileChanged;
            _fileSystemWatcher.Deleted += OnFileDeleted;
            _fileSystemWatcher.Renamed += OnFileRenamed;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        #region Platforms Cache

        /// <summary>
        /// Gets all platforms, using cache if available.
        /// </summary>
        public List<Platform> GetPlatforms()
        {
            _cacheLock.EnterReadLock();
            try
            {
                // Check if cache is valid
                if (_platformsCache != null && IsPlatformsCacheValid())
                {
                    return new List<Platform>(_platformsCache); // Return copy
                }
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }

            // Cache miss or invalid - reload
            _cacheLock.EnterWriteLock();
            try
            {
                _platformsCache = _dataContext.LoadPlatforms();
                _platformsCacheTimestamp = GetFileTimestamp("Platforms.xml");
                return new List<Platform>(_platformsCache); // Return copy
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        private bool IsPlatformsCacheValid()
        {
            if (_platformsCacheTimestamp == null)
                return false;

            var currentTimestamp = GetFileTimestamp("Platforms.xml");
            return currentTimestamp <= _platformsCacheTimestamp;
        }

        /// <summary>
        /// Invalidates the platforms cache.
        /// </summary>
        public void InvalidatePlatformsCache()
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _platformsCache = null;
                _platformsCacheTimestamp = null;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        #endregion

        #region Games Cache

        /// <summary>
        /// Gets games for a specific platform, using cache if available.
        /// </summary>
        public List<Game> GetGames(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            _cacheLock.EnterReadLock();
            try
            {
                // Check if cache is valid
                if (_gamesCache.TryGetValue(platformName, out var cachedGames) && IsGamesCacheValid(platformName))
                {
                    return new List<Game>(cachedGames); // Return copy
                }
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }

            // Cache miss or invalid - reload
            _cacheLock.EnterWriteLock();
            try
            {
                var games = _dataContext.LoadGames(platformName);
                _gamesCache[platformName] = games;
                _cacheTimestamps[platformName] = GetPlatformFileTimestamp(platformName);
                return new List<Game>(games); // Return copy
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        private bool IsGamesCacheValid(string platformName)
        {
            if (!_cacheTimestamps.TryGetValue(platformName, out var cachedTimestamp))
                return false;

            var currentTimestamp = GetPlatformFileTimestamp(platformName);
            return currentTimestamp <= cachedTimestamp;
        }

        /// <summary>
        /// Invalidates the games cache for a specific platform.
        /// </summary>
        public void InvalidateGamesCache(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                return;

            _cacheLock.EnterWriteLock();
            try
            {
                _gamesCache.Remove(platformName);
                _cacheTimestamps.Remove(platformName);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Invalidates all games caches.
        /// </summary>
        public void InvalidateAllGamesCache()
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _gamesCache.Clear();
                _cacheTimestamps.Clear();
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Clears all caches.
        /// </summary>
        public void ClearAllCaches()
        {
            InvalidatePlatformsCache();
            InvalidateAllGamesCache();
        }

        /// <summary>
        /// Returns all cached games across every platform as a flat list.
        /// Used for cross-platform search without re-reading XML files.
        /// Platforms that have not been loaded yet are loaded on-demand.
        /// </summary>
        public System.Collections.Generic.List<Game> GetAllGames()
        {
            var platforms = GetPlatforms();
            var result = new System.Collections.Generic.List<Game>();

            foreach (var platform in platforms)
            {
                try { result.AddRange(GetGames(platform.Name)); }
                catch { /* skip unreadable platforms */ }
            }
            return result;
        }

        #endregion

        #region File System Watching


        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            string fileName = Path.GetFileName(e.FullPath);
            string? directory = Path.GetDirectoryName(e.FullPath);

            if (fileName.Equals("Platforms.xml", StringComparison.OrdinalIgnoreCase))
            {
                InvalidatePlatformsCache();
            }
            else if (directory != null && directory.EndsWith("Platforms", StringComparison.OrdinalIgnoreCase))
            {
                // Platform-specific game file changed
                string platformName = Path.GetFileNameWithoutExtension(fileName);
                InvalidateGamesCache(platformName);
            }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            OnFileChanged(sender, e); // Handle deletion same as change
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            OnFileChanged(sender, e); // Handle rename same as change
        }

        #endregion

        #region Helper Methods

        private DateTime GetFileTimestamp(string fileName)
        {
            string filePath = Path.Combine(Path.GetDirectoryName(_platformsDirectory)!, fileName);
            if (File.Exists(filePath))
                return File.GetLastWriteTimeUtc(filePath);
            return DateTime.MinValue;
        }

        private DateTime GetPlatformFileTimestamp(string platformName)
        {
            string filePath = Path.Combine(_platformsDirectory, $"{platformName}.xml");
            if (File.Exists(filePath))
                return File.GetLastWriteTimeUtc(filePath);
            return DateTime.MinValue;
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _fileSystemWatcher?.Dispose();
                _cacheLock?.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
