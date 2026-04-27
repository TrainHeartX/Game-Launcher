using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using GameLauncher.Core.Models;

namespace GameLauncher.BigScreen.Services
{
    /// <summary>
    /// Attract Mode: after a configurable idle period, plays game videos in a loop
    /// across the screen. Any user input stops it.
    /// 
    /// Usage:
    ///   1. Call Initialize(launchBoxPath, allGames) once.
    ///   2. Call ResetIdleTimer() on every user input.
    ///   3. Subscribe to AttractModeStarted / AttractModeStopped events.
    ///   4. When AttractModeStarted fires, show AttractModeOverlay and call PlayNext().
    ///   5. Call Stop() to exit attract mode programmatically.
    /// </summary>
    public class AttractModeService : IDisposable
    {
        // ── Configuration ────────────────────────────────────────────
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(3);
        public bool IsEnabled { get; set; } = true;

        // ── Events ───────────────────────────────────────────────────
        public event EventHandler? AttractModeStarted;
        public event EventHandler? AttractModeStopped;
        public event EventHandler<AttractModeGameArgs>? GameChanged;

        // ── State ────────────────────────────────────────────────────
        public bool IsActive { get; private set; }

        // ── Internal ─────────────────────────────────────────────────
        private readonly DispatcherTimer _idleTimer;
        private readonly MediaPlayer _videoPlayer;
        private string _launchBoxPath = string.Empty;
        private List<Game> _gamesWithVideo = new();
        private int _currentIndex;
        private bool _disposed;
        private readonly Random _rng = new();
        private DateTime _lastInputTime; // BUG-13 FIX

        public AttractModeService()
        {
            // BUG-13 FIX: tick every 5 s and compare against real wall-clock time
            _idleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _idleTimer.Tick += OnIdleTimerTick;
            _lastInputTime = DateTime.UtcNow;

            // Note: _videoPlayer is kept for internal HasVideo() probing only.
            // Actual video playback is done by the AttractVideoPlayer MediaElement in the XAML.
            _videoPlayer = new MediaPlayer();
            _videoPlayer.MediaEnded += OnVideoEnded;
        }

        // ── Public API ───────────────────────────────────────────────

        /// <summary>
        /// Initializes with the full game list. Filters to games that have a known video file.
        /// Call this after the library is loaded.
        /// </summary>
        public void Initialize(string launchBoxPath, IEnumerable<Game> allGames)
        {
            _launchBoxPath = launchBoxPath;
            RefreshGameList(allGames);

            if (IsEnabled && _gamesWithVideo.Count > 0)
                _idleTimer.Start();
        }

        /// <summary>
        /// Refreshes the internal pool of games that have videos.
        /// </summary>
        public void RefreshGameList(IEnumerable<Game> allGames)
        {
            _gamesWithVideo = allGames
                .Where(g => !string.IsNullOrWhiteSpace(g.Platform) && HasVideo(g))
                .ToList();

            // Shuffle pool
            for (int i = _gamesWithVideo.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (_gamesWithVideo[i], _gamesWithVideo[j]) = (_gamesWithVideo[j], _gamesWithVideo[i]);
            }
            _currentIndex = 0;
        }

        /// <summary>
        /// Must be called on every user input (key, mouse, gamepad) to reset the idle countdown.
        /// </summary>
        public void ResetIdleTimer()
        {
            // BUG-13 FIX: stamp the wall-clock time so OnIdleTimerTick can compare correctly
            _lastInputTime = DateTime.UtcNow;

            if (IsActive)
            {
                Stop();
                return;
            }

            // Restart the periodic checker (it was already running — just updating _lastInputTime is enough,
            // but we restart to avoid a race where Stop() had stopped it)
            _idleTimer.Stop();
            if (IsEnabled && _gamesWithVideo.Count > 0)
                _idleTimer.Start();
        }

        /// <summary>Plays the next game video in the shuffled pool.</summary>
        public void PlayNext()
        {
            if (_gamesWithVideo.Count == 0 || !IsActive) return;

            var game = _gamesWithVideo[_currentIndex];
            _currentIndex = (_currentIndex + 1) % _gamesWithVideo.Count;

            var videoPath = ResolveVideoPath(game);
            if (videoPath == null) { PlayNext(); return; }

            GameChanged?.Invoke(this, new AttractModeGameArgs(game, videoPath));

            try
            {
                _videoPlayer.Open(new Uri(videoPath));
                _videoPlayer.Volume = 0.5;
                _videoPlayer.Play();
            }
            catch { PlayNext(); }
        }

        /// <summary>Stops attract mode immediately.</summary>
        public void Stop()
        {
            if (!IsActive) return;
            IsActive = false;

            _videoPlayer.Stop();
            _videoPlayer.Close();

            AttractModeStopped?.Invoke(this, EventArgs.Empty);

            // Restart idle timer
            _idleTimer.Stop();
            if (IsEnabled && _gamesWithVideo.Count > 0)
                _idleTimer.Start();
        }

        // ── Private ──────────────────────────────────────────────────

        private void OnIdleTimerTick(object? sender, EventArgs e)
        {
            if (!IsEnabled || _gamesWithVideo.Count == 0 || IsActive) return;

            // BUG-13 FIX: check real elapsed time
            var elapsed = DateTime.UtcNow - _lastInputTime;
            if (elapsed < IdleTimeout) return;

            IsActive = true;
            _idleTimer.Stop(); // don't keep ticking during attract mode
            AttractModeStarted?.Invoke(this, EventArgs.Empty);
        }

        private void OnVideoEnded(object? sender, EventArgs e)
        {
            if (IsActive) PlayNext();
        }

        private bool HasVideo(Game game)
            => ResolveVideoPath(game) != null;

        private string? ResolveVideoPath(Game game)
        {
            if (string.IsNullOrWhiteSpace(_launchBoxPath) || string.IsNullOrWhiteSpace(game.Title) || string.IsNullOrWhiteSpace(game.Platform))
                return null;

            var sanitized = Core.Helpers.FileNameHelper.SanitizeForLaunchBox(game.Title);
            string[] extensions = { ".mp4", ".avi", ".mkv", ".mov", ".wmv" };
            string[] suffixes = { "", "-01", "-02" };
            var videoDir = Path.Combine(_launchBoxPath, "Videos", game.Platform);

            foreach (var suffix in suffixes)
                foreach (var ext in extensions)
                {
                    var path = Path.Combine(videoDir, sanitized + suffix + ext);
                    if (File.Exists(path)) return path;
                }
            return null;
        }

        // ── IDisposable ──────────────────────────────────────────────
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _idleTimer.Stop();
            _videoPlayer.Stop();
            _videoPlayer.MediaEnded -= OnVideoEnded;
            GC.SuppressFinalize(this);
        }
    }

    public class AttractModeGameArgs : EventArgs
    {
        public Game Game { get; }
        public string VideoPath { get; }
        public AttractModeGameArgs(Game game, string videoPath) { Game = game; VideoPath = videoPath; }
    }
}
