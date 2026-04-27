using System.Threading.Tasks;
using GameLauncher.Core.Models;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Interface for persisting game changes.
    /// </summary>
    public interface IGamePersistenceService
    {
        Task SaveGameAsync(Game game);
    }

    /// <summary>
    /// Servicio para persistir cambios de juegos en XML.
    /// Delega al GameManager para guardar el objeto Game completo.
    /// </summary>
    public class GamePersistenceService : IGamePersistenceService
    {
        private readonly IGameManager _gameManager;

        public GamePersistenceService(IGameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public async Task SaveGameAsync(Game game)
        {
            if (game == null)
                throw new System.ArgumentNullException(nameof(game));

            if (string.IsNullOrWhiteSpace(game.Platform))
                throw new System.ArgumentException("Game must have a platform", nameof(game));

            // Delegar al GameManager que ya guarda TODOS los campos
            await _gameManager.UpdateGameAsync(game.Platform, game);
        }
    }
}
