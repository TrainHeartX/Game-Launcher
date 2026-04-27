using System.Collections.Generic;
using System.Threading.Tasks;
using GameLauncher.Core.Models;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Interface for managing playlists.
    /// </summary>
    public interface IPlaylistManager
    {
        Task<List<Playlist>> GetAllPlaylistsAsync();
        Task<Playlist?> GetPlaylistByNameAsync(string playlistName);
        Task<List<PlaylistGame>> GetPlaylistGamesAsync(string playlistName);
        Task<List<PlaylistFilter>> GetPlaylistFiltersAsync(string playlistName);
        Task SavePlaylistAsync(string playlistName, Playlist playlist, List<PlaylistGame> games, List<PlaylistFilter> filters);
        Task AddGameToPlaylistAsync(string playlistName, PlaylistGame game);
        Task RemoveGameFromPlaylistAsync(string playlistName, string gameId);
    }
}
