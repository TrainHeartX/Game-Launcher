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
    public class StatisticsTrackerTests
    {
        private string _tempDataPath = null!;
        private XmlDataContext _dataContext = null!;
        private StatisticsTracker _tracker = null!;

        [SetUp]
        public void Setup()
        {
            _tempDataPath = Path.Combine(Path.GetTempPath(), "GameLauncherTests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDataPath);

            var dataDir = Path.Combine(_tempDataPath, "Data");
            Directory.CreateDirectory(dataDir);
            Directory.CreateDirectory(Path.Combine(dataDir, "Platforms"));

            _dataContext = new XmlDataContext(dataDir);
            _tracker = new StatisticsTracker(_dataContext);
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
        public void RecordPlaySession_UpdatesGameStatistics()
        {
            // Arrange
            var game = new Game
            {
                ID = Guid.NewGuid().ToString(),
                Title = "Test Game",
                Platform = "TestPlatform",
                PlayCount = 0,
                PlayTime = 0
            };

            // Save game first
            _dataContext.SaveGames("TestPlatform", new System.Collections.Generic.List<Game> { game });

            // Act
            _tracker.RecordPlaySessionAsync(game, 3600).Wait(); // 1 hour

            // Assert
            Assert.AreEqual(1, game.PlayCount, "PlayCount should be incremented");
            Assert.AreEqual(3600, game.PlayTime, "PlayTime should be updated");
            Assert.IsNotNull(game.DateModified, "DateModified should be set");

            // Verify it was saved to XML
            var loadedGames = _dataContext.LoadGames("TestPlatform");
            var loadedGame = loadedGames.First();
            Assert.AreEqual(1, loadedGame.PlayCount, "PlayCount should be persisted");
            Assert.AreEqual(3600, loadedGame.PlayTime, "PlayTime should be persisted");
        }

        [Test]
        public void GetPlatformStatistics_CalculatesCorrectly()
        {
            // Arrange
            var games = new System.Collections.Generic.List<Game>
            {
                new Game
                {
                    ID = Guid.NewGuid().ToString(),
                    Title = "Game 1",
                    Platform = "TestPlatform",
                    PlayTime = 7200,  // 2 hours
                    PlayCount = 5,
                    Favorite = true,
                    Completed = false
                },
                new Game
                {
                    ID = Guid.NewGuid().ToString(),
                    Title = "Game 2",
                    Platform = "TestPlatform",
                    PlayTime = 3600,  // 1 hour
                    PlayCount = 2,
                    Favorite = false,
                    Completed = true
                },
                new Game
                {
                    ID = Guid.NewGuid().ToString(),
                    Title = "Game 3",
                    Platform = "TestPlatform",
                    PlayTime = 0,
                    PlayCount = 0,
                    Favorite = true,
                    Completed = false
                }
            };

            _dataContext.SaveGames("TestPlatform", games);

            // Act
            var stats = _tracker.GetPlatformStatisticsAsync("TestPlatform").Result;

            // Assert
            Assert.AreEqual("TestPlatform", stats.PlatformName);
            Assert.AreEqual(3, stats.TotalGames);
            Assert.AreEqual(2, stats.FavoriteGames);
            Assert.AreEqual(1, stats.CompletedGames);
            Assert.AreEqual(10800, stats.TotalPlayTimeSeconds); // 3 hours total
            Assert.AreEqual(7, stats.TotalPlayCount);
            Assert.AreEqual("Game 1", stats.MostPlayedGameTitle);
            Assert.AreEqual(7200, stats.MostPlayedGameTime);
            Assert.AreEqual(33.33, stats.CompletionRate, 0.01); // 1 out of 3 = 33.33%
        }

        [Test]
        public void GetPlatformStatistics_EmptyPlatform_ReturnsZeros()
        {
            // Arrange
            _dataContext.SaveGames("EmptyPlatform", new System.Collections.Generic.List<Game>());

            // Act
            var stats = _tracker.GetPlatformStatisticsAsync("EmptyPlatform").Result;

            // Assert
            Assert.AreEqual(0, stats.TotalGames);
            Assert.AreEqual(0, stats.FavoriteGames);
            Assert.AreEqual(0, stats.CompletedGames);
            Assert.AreEqual(0, stats.TotalPlayTimeSeconds);
            Assert.AreEqual(0, stats.TotalPlayCount);
            Assert.AreEqual(string.Empty, stats.MostPlayedGameTitle);
        }

        [Test]
        public void FormattedTotalPlayTime_FormatsCorrectly()
        {
            // Arrange & Act
            var stats = new PlatformStatistics
            {
                TotalPlayTimeSeconds = 3661 // 1 hour, 1 minute, 1 second
            };

            // Assert
            Assert.AreEqual("1h 1m", stats.FormattedTotalPlayTime);
        }

        [Test]
        public void FormattedTotalPlayTime_UnderOneHour_ShowsMinutes()
        {
            // Arrange & Act
            var stats = new PlatformStatistics
            {
                TotalPlayTimeSeconds = 1800 // 30 minutes
            };

            // Assert
            Assert.AreEqual("30m", stats.FormattedTotalPlayTime);
        }
    }
}
