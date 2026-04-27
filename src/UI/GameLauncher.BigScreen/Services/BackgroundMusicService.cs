using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GameLauncher.BigScreen.Services
{
    /// <summary>
    /// Plays background music from the LaunchBox Music folder.
    /// Shuffles tracks from Music/{Platform}/ first, then Music/ as fallback.
    /// Automatically stops during game launch and resumes after.
    /// </summary>
    public class BackgroundMusicService : IDisposable
    {
        private readonly MediaPlayer _player;
        private string[] _tracks = Array.Empty<string>();
        private int _currentTrack;
        private string _launchBoxPath = string.Empty;
        private bool _disposed;
        private bool _enabled = true;

        public bool IsEnabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (!_enabled) Stop();
            }
        }

        public float Volume
        {
            get => (float)_player.Volume;
            set => _player.Volume = Math.Clamp(value, 0f, 1f);
        }

        public BackgroundMusicService()
        {
            _player = new MediaPlayer();
            _player.MediaEnded += OnTrackEnded;
        }

        public void Initialize(string launchBoxPath)
        {
            _launchBoxPath = launchBoxPath;
        }

        /// <summary>
        /// Loads and starts playing music for the given platform.
        /// Falls back to general Music/ folder if no platform-specific music exists.
        /// </summary>
        public async Task PlayForPlatformAsync(string platformName)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(_launchBoxPath))
                return;

            await Task.Run(() =>
            {
                var tracks = new System.Collections.Generic.List<string>();

                // 1. Platform-specific music folder
                var platformMusicDir = Path.Combine(_launchBoxPath, "Music", platformName);
                if (Directory.Exists(platformMusicDir))
                    tracks.AddRange(GetMusicFiles(platformMusicDir));

                // 2. General music folder as fallback
                if (tracks.Count == 0)
                {
                    var generalMusicDir = Path.Combine(_launchBoxPath, "Music");
                    if (Directory.Exists(generalMusicDir))
                        tracks.AddRange(GetMusicFiles(generalMusicDir, recursive: false));
                }

                if (tracks.Count == 0)
                    return;

                // Shuffle tracks (Fisher-Yates)
                var rng = new Random();
                for (int i = tracks.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    (tracks[i], tracks[j]) = (tracks[j], tracks[i]);
                }

                _tracks = tracks.ToArray();
                _currentTrack = 0;
            });

            PlayCurrentTrack();
        }

        public void Stop()
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                _player.Stop();
                _player.Close();
            });
        }

        public void Pause()
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() => _player.Pause());
        }

        public void Resume()
        {
            if (!_enabled) return;
            System.Windows.Application.Current?.Dispatcher.Invoke(() => _player.Play());
        }

        private void PlayCurrentTrack()
        {
            if (_tracks.Length == 0) return;

            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                try
                {
                    _player.Open(new Uri(_tracks[_currentTrack]));
                    _player.Play();
                }
                catch
                {
                    // If track fails, skip to next
                    AdvanceToNextTrack();
                }
            });
        }

        private void OnTrackEnded(object? sender, EventArgs e)
        {
            AdvanceToNextTrack();
            PlayCurrentTrack();
        }

        private void AdvanceToNextTrack()
        {
            if (_tracks.Length == 0) return;
            _currentTrack = (_currentTrack + 1) % _tracks.Length;
        }

        private static string[] GetMusicFiles(string directory, bool recursive = true)
        {
            string[] extensions = { "*.mp3", "*.ogg", "*.wav", "*.flac", "*.wma", "*.m4a" };
            var files = new System.Collections.Generic.List<string>();
            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var ext in extensions)
            {
                try { files.AddRange(Directory.GetFiles(directory, ext, option)); }
                catch { /* ignore access errors */ }
            }
            return files.ToArray();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
            _player.MediaEnded -= OnTrackEnded;
            GC.SuppressFinalize(this);
        }
    }
}
