using System.Collections.Generic;
using System.Threading.Tasks;
using GameLauncher.BigScreen.Models;

namespace GameLauncher.BigScreen.Services;

public interface IGameSourceService
{
    string SourceName { get; }
    Task<List<GameSourceItem>> GetLatestGamesAsync();
    Task<GameSourceDetail> GetGameDetailsAsync(string url);
}
