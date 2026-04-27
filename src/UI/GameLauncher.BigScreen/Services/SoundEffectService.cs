using System;
using System.IO;
using System.Windows.Media;

namespace GameLauncher.BigScreen.Services
{
    /// <summary>
    /// Plays BigBox navigation sound effects from the active sound pack.
    /// Reads the pack name from BigBoxSettings and looks up WAV files in Sounds/{PackName}/.
    /// Replicates the LaunchBox BigBox sound system behavior.
    /// </summary>
    public class SoundEffectService : IDisposable
    {
        // Sound event names matching LaunchBox BigBox sound packs
        public const string NavigationLeft     = "NavigationLeft";
        public const string NavigationRight    = "NavigationRight";
        public const string NavigationUp       = "NavigationUp";
        public const string NavigationDown     = "NavigationDown";
        public const string NavigationConfirm  = "NavigationConfirm";
        public const string NavigationBack     = "NavigationBack";
        public const string GameLaunch         = "GameLaunch";
        public const string FilterChanged      = "FilterChanged";

        private string _soundPackDirectory = string.Empty;
        private bool _enabled = true;
        private bool _disposed;

        // Pool of 4 players to allow overlapping short sounds
        private readonly MediaPlayer[] _players;
        private int _playerIndex;

        public bool IsEnabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public float Volume { get; set; } = 0.7f;

        public SoundEffectService()
        {
            _players = new MediaPlayer[4];
            for (int i = 0; i < _players.Length; i++)
                _players[i] = new MediaPlayer();
        }

        /// <summary>
        /// Initializes the service pointing to the given LaunchBox installation
        /// and loading the specified sound pack by name.
        /// </summary>
        public void Initialize(string launchBoxPath, string soundPackName)
        {
            if (string.IsNullOrWhiteSpace(launchBoxPath) || string.IsNullOrWhiteSpace(soundPackName))
                return;

            var dir = Path.Combine(launchBoxPath, "Sounds", soundPackName);
            if (Directory.Exists(dir))
            {
                _soundPackDirectory = dir;
                _enabled = true;
            }
            else
            {
                _enabled = false;
            }
        }

        /// <summary>
        /// Plays a named sound effect (e.g. NavigationLeft, NavigationConfirm).
        /// Does nothing if sounds are disabled or the file is not found.
        /// </summary>
        public void Play(string soundName)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(_soundPackDirectory))
                return;

            var filePath = FindSoundFile(soundName);
            if (filePath == null)
                return;

            // Round-robin across player pool to allow brief overlaps
            var player = _players[_playerIndex];
            _playerIndex = (_playerIndex + 1) % _players.Length;

            System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    player.Stop();
                    player.Open(new Uri(filePath));
                    player.Volume = Volume;
                    player.Play();
                }
                catch { /* ignore playback errors — sounds are non-critical */ }
            });
        }

        private string? FindSoundFile(string soundName)
        {
            string[] extensions = { ".wav", ".mp3", ".ogg" };
            foreach (var ext in extensions)
            {
                var path = Path.Combine(_soundPackDirectory, soundName + ext);
                if (File.Exists(path))
                    return path;
            }
            return null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var player in _players)
            {
                try { player.Stop(); player.Close(); }
                catch { }
            }
            GC.SuppressFinalize(this);
        }
    }
}
