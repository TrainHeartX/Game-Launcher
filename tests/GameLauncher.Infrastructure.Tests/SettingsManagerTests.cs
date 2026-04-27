using System;
using System.IO;
using GameLauncher.Data.Xml;
using GameLauncher.Infrastructure.Services;
using NUnit.Framework;

namespace GameLauncher.Infrastructure.Tests
{
    [TestFixture]
    public class SettingsManagerTests
    {
        private string _tempDataPath = null!;
        private XmlDataContext _dataContext = null!;
        private SettingsManager _settingsManager = null!;

        [SetUp]
        public void Setup()
        {
            _tempDataPath = Path.Combine(Path.GetTempPath(), "GameLauncher Tests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDataPath);

            var dataDir = Path.Combine(_tempDataPath, "Data");
            Directory.CreateDirectory(dataDir);

            _dataContext = new XmlDataContext(dataDir);
            _settingsManager = new SettingsManager(_dataContext);
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
        public void LoadSettings_NoFile_ReturnsDefaults()
        {
            // Act
            var settings = _settingsManager.LoadSettingsAsync().Result;

            // Assert
            Assert.IsNotNull(settings);
            Assert.IsNotNull(settings.ID);
            Assert.AreEqual(1280, settings.FormSizeX);
            Assert.AreEqual(720, settings.FormSizeY);
            Assert.AreEqual("Title", settings.SortBy);
            Assert.IsTrue(settings.EnableGamepad);
        }

        [Test]
        public void SaveSettings_PersistsToXml()
        {
            // Arrange
            var settings = _settingsManager.LoadSettingsAsync().Result;
            settings.FormSizeX = 1920;
            settings.FormSizeY = 1080;
            settings.SortBy = "DateAdded";

            // Act
            _settingsManager.SaveSettingsAsync(settings).Wait();

            // Assert - reload from XML
            var loaded = _dataContext.LoadSettings();
            Assert.AreEqual(1920, loaded.FormSizeX);
            Assert.AreEqual(1080, loaded.FormSizeY);
            Assert.AreEqual("DateAdded", loaded.SortBy);
        }

        [Test]
        public void LoadBigBoxSettings_NoFile_ReturnsDefaults()
        {
            // Act
            var settings = _settingsManager.LoadBigBoxSettingsAsync().Result;

            // Assert
            Assert.IsNotNull(settings);
            Assert.AreEqual("Default", settings.Theme);
            Assert.AreEqual(60, settings.FrameRate);
            Assert.AreEqual("VLC", settings.VideoPlaybackEngine);
            Assert.AreEqual("HorizontalWheel3", settings.GamesListView);
        }

        [Test]
        public void SaveBigBoxSettings_PersistsToXml()
        {
            // Arrange
            var settings = _settingsManager.LoadBigBoxSettingsAsync().Result;
            settings.Theme = "CustomTheme";
            settings.FrameRate = 120;
            settings.GamesListView = "TextList";

            // Act
            _settingsManager.SaveBigBoxSettingsAsync(settings).Wait();

            // Assert - reload from XML
            var loaded = _dataContext.LoadBigBoxSettings();
            Assert.AreEqual("CustomTheme", loaded.Theme);
            Assert.AreEqual(120, loaded.FrameRate);
            Assert.AreEqual("TextList", loaded.GamesListView);
        }

        [Test]
        public void ResetToDefaults_CreatesNewDefaults()
        {
            // Arrange - save custom settings first
            var settings = _settingsManager.LoadSettingsAsync().Result;
            settings.FormSizeX = 999;
            settings.SortBy = "Custom";
            _settingsManager.SaveSettingsAsync(settings).Wait();

            // Act
            var defaults = _settingsManager.ResetToDefaultsAsync().Result;

            // Assert
            Assert.AreEqual(1280, defaults.FormSizeX);
            Assert.AreEqual("Title", defaults.SortBy);

            // Verify persisted
            var loaded = _dataContext.LoadSettings();
            Assert.AreEqual(1280, loaded.FormSizeX);
        }

        [Test]
        public void Settings_PreservesColors()
        {
            // Arrange
            var settings = _settingsManager.LoadSettingsAsync().Result;
            var testColor = -16766127; // ARGB color from LaunchBox

            settings.DarkBackgroundColor = testColor;

            // Act
            _settingsManager.SaveSettingsAsync(settings).Wait();

            // Assert
            var loaded = _dataContext.LoadSettings();
            Assert.AreEqual(testColor, loaded.DarkBackgroundColor);
        }
    }
}
