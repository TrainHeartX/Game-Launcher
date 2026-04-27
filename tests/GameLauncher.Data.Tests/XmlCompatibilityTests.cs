using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameLauncher.Core.Models;
using GameLauncher.Data.Xml;
using NUnit.Framework;

namespace GameLauncher.Data.Tests
{
    [TestFixture]
    public class XmlCompatibilityTests
    {
        private string _testDataPath = null!;
        private string _tempDataPath = null!;
        private XmlDataContext _dataContext = null!;

        [SetUp]
        public void Setup()
        {
            // Create temporary directory for test data
            _tempDataPath = Path.Combine(Path.GetTempPath(), "GameLauncherTests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDataPath);

            var dataDir = Path.Combine(_tempDataPath, "Data");
            Directory.CreateDirectory(dataDir);
            Directory.CreateDirectory(Path.Combine(dataDir, "Platforms"));

            _dataContext = new XmlDataContext(dataDir);
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
        public void PlatformsXml_RoundTrip_PreservesStructure()
        {
            // Arrange - Create test platforms
            var platforms = new List<Platform>
            {
                new Platform
                {
                    Name = "Test Platform 1",
                    Category = "Test Category",
                    Developer = "Test Dev",
                    Manufacturer = "Test Mfg",
                    ReleaseDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Cpu = "Test CPU",
                    Memory = "Test Memory",
                    Folder = "Games\\TestPlatform1"
                },
                new Platform
                {
                    Name = "Test Platform 2",
                    BigBoxView = "TextListWithDetails",
                    BigBoxTheme = "Default"
                }
            };

            // Act - Save and reload
            _dataContext.SavePlatforms(platforms);
            var loadedPlatforms = _dataContext.LoadPlatforms();

            // Assert
            Assert.AreEqual(platforms.Count, loadedPlatforms.Count, "Platform count should match");

            for (int i = 0; i < platforms.Count; i++)
            {
                Assert.AreEqual(platforms[i].Name, loadedPlatforms[i].Name, $"Platform {i} name should match");
                Assert.AreEqual(platforms[i].Category, loadedPlatforms[i].Category, $"Platform {i} category should match");
                Assert.AreEqual(platforms[i].Developer, loadedPlatforms[i].Developer, $"Platform {i} developer should match");
            }

            // Verify XML structure
            string xmlPath = Path.Combine(_tempDataPath, "Data", "Platforms.xml");
            Assert.IsTrue(File.Exists(xmlPath), "Platforms.xml should exist");

            string xmlContent = File.ReadAllText(xmlPath);
            Assert.IsTrue(xmlContent.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>"),
                "XML should start with proper declaration");
            Assert.IsTrue(xmlContent.Contains("<LaunchBox>"), "XML should contain LaunchBox root element");
            Assert.IsTrue(xmlContent.Contains("<Platform>"), "XML should contain Platform elements");
        }

        [Test]
        public void GamesXml_RoundTrip_PreservesAllFields()
        {
            // Arrange - Create test games with various field types
            var games = new List<Game>
            {
                new Game
                {
                    ID = Guid.NewGuid().ToString(),
                    Title = "Test Game 1",
                    Platform = "Test Platform",
                    ApplicationPath = "C:\\Games\\test1.exe",
                    Developer = "Test Dev",
                    Publisher = "Test Pub",
                    Genre = "Action",
                    ReleaseDate = new DateTime(2021, 5, 15, 12, 30, 0, DateTimeKind.Utc),
                    DateAdded = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                    Favorite = true,
                    Completed = false,
                    PlayCount = 5,
                    PlayTime = 3600, // 1 hour in seconds
                    StarRatingFloat = 4.5f,
                    CommunityStarRating = 4.2f,
                    CommunityStarRatingTotalVotes = 100
                },
                new Game
                {
                    ID = Guid.NewGuid().ToString(),
                    Title = "Test Game 2",
                    Platform = "Test Platform",
                    ApplicationPath = "C:\\Games\\test2.exe",
                    Favorite = false,
                    PlayCount = 0,
                    PlayTime = 0,
                    Hide = true,
                    Broken = false
                }
            };

            // Act - Save and reload
            _dataContext.SaveGames("TestPlatform", games);
            var loadedGames = _dataContext.LoadGames("TestPlatform");

            // Assert
            Assert.AreEqual(games.Count, loadedGames.Count, "Game count should match");

            // Verify first game in detail
            var game1 = games[0];
            var loaded1 = loadedGames[0];
            Assert.AreEqual(game1.ID, loaded1.ID, "Game ID should match");
            Assert.AreEqual(game1.Title, loaded1.Title, "Game title should match");
            Assert.AreEqual(game1.Platform, loaded1.Platform, "Game platform should match");
            Assert.AreEqual(game1.Favorite, loaded1.Favorite, "Game favorite flag should match");
            Assert.AreEqual(game1.PlayCount, loaded1.PlayCount, "Game play count should match");
            Assert.AreEqual(game1.PlayTime, loaded1.PlayTime, "Game play time should match");
            Assert.AreEqual(game1.StarRatingFloat, loaded1.StarRatingFloat, 0.01f, "Game star rating should match");

            // Verify XML structure
            string xmlPath = Path.Combine(_tempDataPath, "Data", "Platforms", "TestPlatform.xml");
            Assert.IsTrue(File.Exists(xmlPath), "TestPlatform.xml should exist");

            string xmlContent = File.ReadAllText(xmlPath);
            Assert.IsTrue(xmlContent.Contains("<Game>"), "XML should contain Game elements");
            Assert.IsTrue(xmlContent.Contains("<Title>Test Game 1</Title>"), "XML should contain game title");
            Assert.IsTrue(xmlContent.Contains("<Favorite>true</Favorite>"), "XML should contain boolean fields");
        }

        [Test]
        public void Settings_RoundTrip_PreservesColors()
        {
            // Arrange - Test with ARGB color values (as int)
            var settings = new Settings
            {
                ID = Guid.NewGuid().ToString(),
                DarkBackgroundColor = -16766127, // Example ARGB value from actual LaunchBox
                LightBackgroundColor = -16758391,
                SelectedBackgroundColor = -14181633,
                FormSizeX = 1366,
                FormSizeY = 720,
                ShowFilters = true,
                SortBy = "Title"
            };

            // Act
            _dataContext.SaveSettings(settings);
            var loadedSettings = _dataContext.LoadSettings();

            // Assert
            Assert.AreEqual(settings.DarkBackgroundColor, loadedSettings.DarkBackgroundColor, "Dark color should match");
            Assert.AreEqual(settings.LightBackgroundColor, loadedSettings.LightBackgroundColor, "Light color should match");
            Assert.AreEqual(settings.FormSizeX, loadedSettings.FormSizeX, "Form width should match");
            Assert.AreEqual(settings.ShowFilters, loadedSettings.ShowFilters, "ShowFilters should match");
        }

        [Test]
        public void EmptyLists_DoNotThrow()
        {
            // Arrange & Act - Save empty lists
            _dataContext.SavePlatforms(new List<Platform>());
            _dataContext.SaveGames("EmptyPlatform", new List<Game>());

            // Assert - Should not throw, files should be created
            var platforms = _dataContext.LoadPlatforms();
            var games = _dataContext.LoadGames("EmptyPlatform");

            Assert.IsNotNull(platforms, "Platforms list should not be null");
            Assert.IsNotNull(games, "Games list should not be null");
            Assert.AreEqual(0, platforms.Count, "Platforms should be empty");
            Assert.AreEqual(0, games.Count, "Games should be empty");
        }

        [Test]
        public void MissingFiles_ReturnEmptyLists()
        {
            // Act - Load from non-existent files
            var platforms = _dataContext.LoadPlatforms();
            var games = _dataContext.LoadGames("NonExistentPlatform");
            var settings = _dataContext.LoadSettings();

            // Assert
            Assert.IsNotNull(platforms, "Platforms should not be null");
            Assert.IsNotNull(games, "Games should not be null");
            Assert.IsNotNull(settings, "Settings should not be null");
            Assert.AreEqual(0, platforms.Count, "Platforms should be empty");
            Assert.AreEqual(0, games.Count, "Games should be empty");
        }

        /// <summary>
        /// Integration test using actual LaunchBox data if available.
        /// This test will be skipped if LaunchBox is not found at the expected path.
        /// </summary>
        [Test]
        [Ignore("Integration test - requires actual LaunchBox installation")]
        public void ActualLaunchBoxData_CanBeLoaded()
        {
            // This test should be run manually with the actual LaunchBox path
            string launchBoxPath = @"H:\LaunchBox\LaunchBox";

            if (!Directory.Exists(launchBoxPath))
            {
                Assert.Ignore("LaunchBox not found at expected path");
                return;
            }

            // Arrange
            var realDataContext = new XmlDataContext(launchBoxPath);

            // Act - Load platforms
            var platforms = realDataContext.LoadPlatforms();

            // Assert
            Assert.IsNotNull(platforms, "Platforms should be loaded");
            Assert.Greater(platforms.Count, 0, "Should have at least one platform");

            // Try to load a platform's games (use a small one for testing)
            var firstPlatform = platforms.FirstOrDefault(p => p.Name != "Arcade"); // Skip Arcade (too large)
            if (firstPlatform != null)
            {
                var games = realDataContext.LoadGames(firstPlatform.Name);
                Assert.IsNotNull(games, "Games should be loaded");
                Console.WriteLine($"Loaded {games.Count} games from {firstPlatform.Name}");
            }
        }
    }
}
