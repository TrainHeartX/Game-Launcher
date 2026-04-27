using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLauncher.Core.Models;
using GameLauncher.Data.Xml;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Manages playlist CRUD operations.
    /// </summary>
    public class PlaylistManager : IPlaylistManager
    {
        private readonly XmlDataContext _dataContext;

        public PlaylistManager(XmlDataContext dataContext)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        public async Task<List<Playlist>> GetAllPlaylistsAsync()
        {
            var playlists = _dataContext.LoadAllPlaylists();
            return await Task.FromResult(new List<Playlist>(playlists));
        }

        public async Task<Playlist?> GetPlaylistByNameAsync(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
                throw new ArgumentException("Playlist name cannot be null or empty", nameof(playlistName));

            var data = _dataContext.LoadPlaylistFileData(playlistName);
            return await Task.FromResult(data.Playlist);
        }

        public async Task<List<PlaylistGame>> GetPlaylistGamesAsync(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
                throw new ArgumentException("Playlist name cannot be null or empty", nameof(playlistName));

            var data = _dataContext.LoadPlaylistFileData(playlistName);
            return await Task.FromResult(new List<PlaylistGame>(data.Games));
        }

        public async Task<List<PlaylistFilter>> GetPlaylistFiltersAsync(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
                throw new ArgumentException("Playlist name cannot be null or empty", nameof(playlistName));

            var data = _dataContext.LoadPlaylistFileData(playlistName);
            return await Task.FromResult(new List<PlaylistFilter>(data.Filters));
        }

        public async Task SavePlaylistAsync(string playlistName, Playlist playlist, List<PlaylistGame> games, List<PlaylistFilter> filters)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
                throw new ArgumentException("Playlist name cannot be null or empty", nameof(playlistName));

            var data = new PlaylistFileData
            {
                Playlist = playlist,
                Games = games ?? new List<PlaylistGame>(),
                Filters = filters ?? new List<PlaylistFilter>()
            };

            _dataContext.SavePlaylistFileData(playlistName, data);

            await Task.CompletedTask;
        }

        public async Task AddGameToPlaylistAsync(string playlistName, PlaylistGame game)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
                throw new ArgumentException("Playlist name cannot be null or empty", nameof(playlistName));
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            var data = _dataContext.LoadPlaylistFileData(playlistName);

            // Avoid duplicates
            if (!data.Games.Any(g => g.GameId.Equals(game.GameId, StringComparison.OrdinalIgnoreCase)))
            {
                data.Games.Add(game);
                _dataContext.SavePlaylistFileData(playlistName, data);
            }

            await Task.CompletedTask;
        }

        public async Task RemoveGameFromPlaylistAsync(string playlistName, string gameId)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
                throw new ArgumentException("Playlist name cannot be null or empty", nameof(playlistName));
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));

            var data = _dataContext.LoadPlaylistFileData(playlistName);

            var removed = data.Games.RemoveAll(g => g.GameId.Equals(gameId, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
            {
                _dataContext.SavePlaylistFileData(playlistName, data);
            }

            await Task.CompletedTask;
        }
    }
}
