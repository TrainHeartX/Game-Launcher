using System;
using System.IO;
using System.Linq;
using GameLauncher.Core.Models;
using GameLauncher.Data.Xml;
using GameLauncher.Infrastructure.Services;
using NUnit.Framework;

namespace GameLauncher.Infrastructure.Tests
{
    [TestFixture]
    public class GameManagerTests
    {
        private string _tempDataPath = null!;
        private XmlDataContext _dataContext = null!;
        private GameManager _gameManager = null!;

        [SetUp]
        public void Setup()
        {
            _tempDataPath = Path.Combine(Path.GetTempPath(), "GameLauncherTests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDataPath);

            var dataDir = Path.Combine(_tempDataPath, "Data");
            Directory.CreateDirectory(dataDir);
            Directory.CreateDirectory(Path.Combine(dataDir, "Platforms"));

            _dataContext = new XmlDataContext(dataDir);
            _gameManager = new GameManager(_dataContext);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDataPath))
            {
                Directory.Delete(_tempDataPath, true);
            }
        }

        [Test]
        public void CreateGame_GeneratesId_AndSetsDefaults()
        {
            // Arrange
            var game = new Game
            {
                Title = "New Game",
                ApplicationPath = "C:\\Games\\test.exe"
            };

            // Act
            var result = _gameManager.CreateGameAsync("TestPlatform", game).Result;

            // Assert
            Assert.IsNotNull(result.ID, "ID should be generated");
            Assert.IsNotEmpty(result.ID);
            Assert.AreEqual("TestPlatform", result.Platform);
            Assert.IsNotNull(result.DateAdded);
            Assert.IsNotNull(result.DateModified);
        }

        [Test]
        public void CreateGame_PersistsToXml()
        {
            // Arrange
            var game = new Game
            {
                Title = "New Game",
                Platform = "TestPlatform",
                ApplicationPath = "C:\\Games\\test.exe"
            };

            // Act
            _gameManager.CreateGameAsync("TestPlatform", game).Wait();

            // Assert - reload from XML
            var games = _dataContext.LoadGames("TestPlatform");
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual("New Game", games[0].Title);
        }

        [Test]
        public void UpdateGame_UpdatesExistingGame()
        {
            // Arrange
            var game = new Game
            {
                ID = Guid.NewGuid().ToString(),
                Title = "Original Title",
                Platform = "TestPlatform"
            };

            _dataContext.SaveGames("TestPlatform", new System.Collections.Generic.List<Game> { game });

            // Act
            game.Title = "Updated Title";
            game.Favorite = true;
            _gameManager.UpdateGameAsync("TestPlatform", game).Wait();

            // Assert
            var games = _dataContext.LoadGames("TestPlatform");
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual("Updated Title", games[0].Title);
            Assert.IsTrue(games[0].Favorite);
        }

        [Test]
        public void DeleteGame_RemovesFromXml()
        {
            // Arrange
            var game1 = new Game { ID = Guid.NewGuid().ToString(), Title = "Game 1", Platform = "TestPlatform" };
            var game2 = new Game { ID = Guid.NewGuid().ToString(), Title = "Game 2", Platform = "TestPlatform" };

            _dataContext.SaveGames("TestPlatform", new System.Collections.Generic.List<Game> { game1, game2 });

            // Act
            _gameManager.DeleteGameAsync("TestPlatform", game1.ID).Wait();

            // Assert
            var games = _dataContext.LoadGames("TestPlatform");
            Assert.AreEqual(1, games.Count);
            Assert.AreEqual("Game 2", games[0].Title);
        }

        [Test]
        public void GetGameById_FindsGameAcrossPlatforms()
        {
            // Arrange
            var game1 = new Game { ID = "game1-id", Title = "Game 1", Platform = "Platform1" };
            var game2 = new Game { ID = "game2-id", Title = "Game 2", Platform = "Platform2" };

            var platforms = new System.Collections.Generic.List<Platform>
            {
                new Platform { Name = "Platform1" },
                new Platform { Name = "Platform2" }
            };

            _dataContext.SavePlatforms(platforms);
            _dataContext.SaveGames("Platform1", new System.Collections.Generic.List<Game> { game1 });
            _dataContext.SaveGames("Platform2", new System.Collections.Generic.List<Game> { game2 });

            // Act
            var found = _gameManager.GetGameByIdAsync("game2-id").Result;

            // Assert
            Assert.IsNotNull(found);
            Assert.AreEqual("Game 2", found.Title);
            Assert.AreEqual("Platform2", found.Platform);
        }

        [Test]
        public void GetGameById_NotFound_ReturnsNull()
        {
            // Arrange
            var platforms = new System.Collections.Generic.List<Platform>
            {
                new Platform { Name = "Platform1" }
            };
            _dataContext.SavePlatforms(platforms);
            _dataContext.SaveGames("Platform1", new System.Collections.Generic.List<Game>());

            // Act
            var found = _gameManager.GetGameByIdAsync("nonexistent-id").Result;

            // Assert
            Assert.IsNull(found);
        }

        [Test]
        public void SearchGames_FindsByTitle()
        {
            // Arrange
            var games = new System.Collections.Generic.List<Game>
            {
                new Game { ID = "1", Title = "Super Mario Bros", Platform = "NES" },
                new Game { ID = "2", Title = "Mario Kart", Platform = "SNES" },
                new Game { ID = "3", Title = "Zelda", Platform = "NES" }
            };

            var platforms = new System.Collections.Generic.List<Platform>
            {
                new Platform { Name = "NES" },
                new Platform { Name = "SNES" }
            };

            _dataContext.SavePlatforms(platforms);
            _dataContext.SaveGames("NES", games.Where(g => g.Platform == "NES").ToList());
            _dataContext.SaveGames("SNES", games.Where(g => g.Platform == "SNES").ToList());

            // Act
            var results = _gameManager.SearchGamesAsync("mario").Result;

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(g => g.Title.ToLower().Contains("mario")));
        }

        [Test]
        public void SearchGames_WithPlatformFilter_FindsOnlyInPlatform()
        {
            // Arrange
            var games = new System.Collections.Generic.List<Game>
            {
                new Game { ID = "1", Title = "Game 1", Platform = "Platform1" },
                new Game { ID = "2", Title = "Game 2", Platform = "Platform2" }
            };

            var platforms = new System.Collections.Generic.List<Platform>
            {
                new Platform { Name = "Platform1" },
                new Platform { Name = "Platform2" }
            };

            _dataContext.SavePlatforms(platforms);
            _dataContext.SaveGames("Platform1", new System.Collections.Generic.List<Game> { games[0] });
            _dataContext.SaveGames("Platform2", new System.Collections.Generic.List<Game> { games[1] });

            // Act
            var results = _gameManager.SearchGamesAsync("game", "Platform1").Result;

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Game 1", results[0].Title);
        }
    }
}
