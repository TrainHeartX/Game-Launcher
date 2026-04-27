using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using GameLauncher.Core.Models;

namespace GameLauncher.Data.Xml
{
    /// <summary>
    /// Provides read/write access to LaunchBox XML data files.
    /// Ensures 100% compatibility by preserving exact XML structure.
    /// </summary>
    public class XmlDataContext
    {
        private readonly string _launchBoxDataPath;
        private readonly XmlWriterSettings _xmlWriterSettings;
        private readonly XmlReaderSettings _xmlReaderSettings;

        // Maps playlist display name (from <Name> tag) → actual file name on disk (without extension).
        // Needed because LaunchBox sanitizes file names (e.g. apostrophes → underscores).
        // e.g. "Assassin's Creed" → "Assassin_s Creed"
        private Dictionary<string, string>? _playlistNameToFileNameMap;

        public XmlDataContext(string dataPath)
        {
            if (string.IsNullOrWhiteSpace(dataPath))
                throw new ArgumentException("Data path cannot be null or empty", nameof(dataPath));

            _launchBoxDataPath = dataPath;

            if (!Directory.Exists(_launchBoxDataPath))
                throw new DirectoryNotFoundException($"Data directory not found: {_launchBoxDataPath}");

            // Configure XML writer to match LaunchBox format exactly
            _xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ", // 2 spaces
                Encoding = new UTF8Encoding(true), // UTF-8 with BOM
                OmitXmlDeclaration = false,
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };

            _xmlReaderSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = false,
                IgnoreComments = false
            };
        }

        #region Platforms

        /// <summary>
        /// Loads all platforms from Platforms.xml.
        /// Uses direct child XPath to avoid matching nested Platform elements inside PlatformFolder.
        /// </summary>
        public List<Platform> LoadPlatforms()
        {
            string filePath = Path.Combine(_launchBoxDataPath, "Platforms.xml");
            if (!File.Exists(filePath))
                return new List<Platform>();

            var doc = new XmlDocument();
            doc.Load(filePath);

            var items = new List<Platform>();
            var nodes = doc.SelectNodes("/LaunchBox/Platform");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    items.Add(DeserializeNode<Platform>(node));
                }
            }
            return items;
        }

        /// <summary>
        /// Loads all platform categories from Platforms.xml.
        /// PlatformCategory entries coexist with Platform entries in the same file.
        /// </summary>
        public List<PlatformCategory> LoadPlatformCategories()
        {
            string filePath = Path.Combine(_launchBoxDataPath, "Platforms.xml");
            if (!File.Exists(filePath))
                return new List<PlatformCategory>();

            var doc = new XmlDocument();
            doc.Load(filePath);
            return DeserializeNodesFromDoc<PlatformCategory>(doc);
        }

        /// <summary>
        /// Saves platforms to Platforms.xml
        /// </summary>
        public void SavePlatforms(List<Platform> platforms)
        {
            string filePath = Path.Combine(_launchBoxDataPath, "Platforms.xml");
            SerializeList(filePath, platforms, "Platform");
        }

        #endregion

        #region Games

        /// <summary>
        /// Loads games for a specific platform from Data/Platforms/{PlatformName}.xml
        /// </summary>
        public List<Game> LoadGames(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            string filePath = GetPlatformFilePath(platformName);
            if (!File.Exists(filePath))
                return new List<Game>();

            return DeserializeList<Game>(filePath);
        }

        /// <summary>
        /// Saves games for a specific platform to Data/Platforms/{PlatformName}.xml
        /// </summary>
        public void SaveGames(string platformName, List<Game> games)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            var data = LoadPlatformFileData(platformName);
            data.Games = games;
            SavePlatformFileData(platformName, data);
        }

        /// <summary>
        /// Loads additional applications for a specific platform from Data/Platforms/{PlatformName}.xml
        /// </summary>
        public List<AdditionalApplication> LoadAdditionalApplications(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            string filePath = GetPlatformFilePath(platformName);
            if (!File.Exists(filePath))
                return new List<AdditionalApplication>();

            return DeserializeList<AdditionalApplication>(filePath);
        }

        /// <summary>
        /// Loads custom fields for a specific platform from Data/Platforms/{PlatformName}.xml
        /// </summary>
        public List<CustomField> LoadCustomFields(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            string filePath = GetPlatformFilePath(platformName);
            if (!File.Exists(filePath))
                return new List<CustomField>();

            return DeserializeList<CustomField>(filePath);
        }

        /// <summary>
        /// Loads controller support entries for a specific platform from Data/Platforms/{PlatformName}.xml
        /// </summary>
        public List<GameControllerSupport> LoadGameControllerSupport(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            string filePath = GetPlatformFilePath(platformName);
            if (!File.Exists(filePath))
                return new List<GameControllerSupport>();

            return DeserializeList<GameControllerSupport>(filePath);
        }

        /// <summary>
        /// Loads alternate names for a specific platform from Data/Platforms/{PlatformName}.xml
        /// </summary>
        public List<AlternateName> LoadAlternateNames(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            string filePath = GetPlatformFilePath(platformName);
            if (!File.Exists(filePath))
                return new List<AlternateName>();

            return DeserializeList<AlternateName>(filePath);
        }

        /// <summary>
        /// Loads all entity types from a platform file at once for efficiency.
        /// Returns games, additional apps, custom fields, controller support, and alternate names.
        /// </summary>
        public PlatformFileData LoadPlatformFileData(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            string filePath = GetPlatformFilePath(platformName);
            if (!File.Exists(filePath))
                return new PlatformFileData();

            var doc = new XmlDocument();
            doc.Load(filePath);

            return new PlatformFileData
            {
                Games = DeserializeNodesFromDoc<Game>(doc),
                AdditionalApplications = DeserializeNodesFromDoc<AdditionalApplication>(doc),
                CustomFields = DeserializeNodesFromDoc<CustomField>(doc),
                GameControllerSupports = DeserializeNodesFromDoc<GameControllerSupport>(doc),
                AlternateNames = DeserializeNodesFromDoc<AlternateName>(doc)
            };
        }

        /// <summary>
        /// Saves all entity types to a platform file at once, preserving the multi-entity structure.
        /// </summary>
        public void SavePlatformFileData(string platformName, PlatformFileData data)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            string platformsDir = Path.Combine(_launchBoxDataPath, "Platforms");
            if (!Directory.Exists(platformsDir))
                Directory.CreateDirectory(platformsDir);

            string filePath = Path.Combine(platformsDir, $"{platformName}.xml");

            using (var writer = XmlWriter.Create(filePath, _xmlWriterSettings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("LaunchBox");

                var emptyNamespaces = new XmlSerializerNamespaces();
                emptyNamespaces.Add("", "");

                SerializeItems(writer, data.Games, "Game", emptyNamespaces);
                SerializeItems(writer, data.AdditionalApplications, "AdditionalApplication", emptyNamespaces);
                SerializeItems(writer, data.CustomFields, "CustomField", emptyNamespaces);
                SerializeItems(writer, data.GameControllerSupports, "GameControllerSupport", emptyNamespaces);
                SerializeItems(writer, data.AlternateNames, "AlternateName", emptyNamespaces);

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        #endregion

        #region Emulators

        /// <summary>
        /// Loads emulator configurations and platform mappings from Emulators.xml
        /// </summary>
        public (List<Emulator> emulators, List<EmulatorPlatform> mappings) LoadEmulators()
        {
            string filePath = Path.Combine(_launchBoxDataPath, "Emulators.xml");
            if (!File.Exists(filePath))
                return (new List<Emulator>(), new List<EmulatorPlatform>());

            var doc = new XmlDocument();
            doc.Load(filePath);

            var emulators = new List<Emulator>();
            var mappings = new List<EmulatorPlatform>();

            var emulatorNodes = doc.SelectNodes("//Emulator");
            if (emulatorNodes != null)
            {
                foreach (XmlNode node in emulatorNodes)
                {
                    emulators.Add(DeserializeNode<Emulator>(node));
                }
            }

            var mappingNodes = doc.SelectNodes("//EmulatorPlatform");
            if (mappingNodes != null)
            {
                foreach (XmlNode node in mappingNodes)
                {
                    mappings.Add(DeserializeNode<EmulatorPlatform>(node));
                }
            }

            return (emulators, mappings);
        }

        /// <summary>
        /// Saves emulator configurations and platform mappings to Emulators.xml
        /// </summary>
        public void SaveEmulators(List<Emulator> emulators, List<EmulatorPlatform> mappings)
        {
            string filePath = Path.Combine(_launchBoxDataPath, "Emulators.xml");

            // Ensure directory exists
            string? directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var writer = XmlWriter.Create(filePath, _xmlWriterSettings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("LaunchBox");

                var emptyNamespaces = new XmlSerializerNamespaces();
                emptyNamespaces.Add("", "");

                SerializeItems(writer, emulators, "Emulator", emptyNamespaces);
                SerializeItems(writer, mappings, "EmulatorPlatform", emptyNamespaces);

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        #endregion

        #region Parents (Hierarchy)

        /// <summary>
        /// Loads all parent-child hierarchy relationships from Parents.xml.
        /// Used to build the navigation tree of platforms, categories, and playlists.
        /// </summary>
        public List<Parent> LoadParents()
        {
            string filePath = Path.Combine(_launchBoxDataPath, "Parents.xml");
            if (!File.Exists(filePath))
                return new List<Parent>();

            return DeserializeList<Parent>(filePath);
        }

        /// <summary>
        /// Saves parent-child hierarchy relationships to Parents.xml.
        /// </summary>
        public void SaveParents(List<Parent> parents)
        {
            string filePath = Path.Combine(_launchBoxDataPath, "Parents.xml");
            SerializeList(filePath, parents, "Parent");
        }

        #endregion

        #region Playlists

        /// <summary>
        /// Gets the list of all playlist file names in the Playlists directory.
        /// </summary>
        public List<string> GetPlaylistFileNames()
        {
            string playlistsDir = Path.Combine(_launchBoxDataPath, "Playlists");
            if (!Directory.Exists(playlistsDir))
                return new List<string>();

            return Directory.GetFiles(playlistsDir, "*.xml")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .OrderBy(n => n)
                .ToList();
        }

        /// <summary>
        /// Loads all data from a single playlist file: the playlist metadata, its games, and its filters.
        /// </summary>
        public PlaylistFileData LoadPlaylistFileData(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
                throw new ArgumentException("Playlist name cannot be null or empty", nameof(playlistName));

            string filePath = GetPlaylistFilePath(playlistName);
            if (!File.Exists(filePath))
                return new PlaylistFileData();

            var doc = new XmlDocument();
            doc.Load(filePath);

            var playlists = DeserializeNodesFromDoc<Playlist>(doc);

            return new PlaylistFileData
            {
                Playlist = playlists.FirstOrDefault(),
                Games = DeserializeNodesFromDoc<PlaylistGame>(doc),
                Filters = DeserializeNodesFromDoc<PlaylistFilter>(doc)
            };
        }

        /// <summary>
        /// Saves a complete playlist file with metadata, games, and filters.
        /// </summary>
        public void SavePlaylistFileData(string playlistName, PlaylistFileData data)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
                throw new ArgumentException("Playlist name cannot be null or empty", nameof(playlistName));

            string playlistsDir = Path.Combine(_launchBoxDataPath, "Playlists");
            if (!Directory.Exists(playlistsDir))
                Directory.CreateDirectory(playlistsDir);

            string filePath = Path.Combine(playlistsDir, $"{playlistName}.xml");

            using (var writer = XmlWriter.Create(filePath, _xmlWriterSettings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("LaunchBox");

                var emptyNamespaces = new XmlSerializerNamespaces();
                emptyNamespaces.Add("", "");

                if (data.Playlist != null)
                {
                    var serializer = new XmlSerializer(typeof(Playlist), new XmlRootAttribute("Playlist"));
                    serializer.Serialize(writer, data.Playlist, emptyNamespaces);
                }

                SerializeItems(writer, data.Filters, "PlaylistFilter", emptyNamespaces);
                SerializeItems(writer, data.Games, "PlaylistGame", emptyNamespaces);

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Loads all playlists (metadata only, without games/filters) from all playlist files.
        /// </summary>
        public List<Playlist> LoadAllPlaylists()
        {
            var playlists = new List<Playlist>();
            foreach (var name in GetPlaylistFileNames())
            {
                var data = LoadPlaylistFileData(name);
                if (data.Playlist != null)
                    playlists.Add(data.Playlist);
            }
            return playlists;
        }

        #endregion

        #region Game Controllers

        /// <summary>
        /// Loads the controller type catalog from GameControllers.xml
        /// </summary>
        public List<GameController> LoadGameControllers()
        {
            string filePath = Path.Combine(_launchBoxDataPath, "GameControllers.xml");
            if (!File.Exists(filePath))
                return new List<GameController>();

            return DeserializeList<GameController>(filePath);
        }

        /// <summary>
        /// Saves the controller type catalog to GameControllers.xml
        /// </summary>
        public void SaveGameControllers(List<GameController> controllers)
        {
            string filePath = Path.Combine(_launchBoxDataPath, "GameControllers.xml");
            SerializeList(filePath, controllers, "GameController");
        }

        #endregion

        #region Input Bindings

        /// <summary>
        /// Loads all gamepad input bindings from InputBindings.xml
        /// </summary>
        public List<InputBinding> LoadInputBindings()
        {
            string filePath = Path.Combine(_launchBoxDataPath, "InputBindings.xml");
            if (!File.Exists(filePath))
                return new List<InputBinding>();

            return DeserializeList<InputBinding>(filePath);
        }

        /// <summary>
        /// Saves gamepad input bindings to InputBindings.xml
        /// </summary>
        public void SaveInputBindings(List<InputBinding> bindings)
        {
            string filePath = Path.Combine(_launchBoxDataPath, "InputBindings.xml");
            SerializeList(filePath, bindings, "InputBinding");
        }

        #endregion

        #region List Cache

        /// <summary>
        /// Loads cached game counts from ListCache.xml
        /// </summary>
        public List<ListCacheItem> LoadListCache()
        {
            string filePath = Path.Combine(_launchBoxDataPath, "ListCache.xml");
            if (!File.Exists(filePath))
                return new List<ListCacheItem>();

            return DeserializeList<ListCacheItem>(filePath);
        }

        /// <summary>
        /// Saves cached game counts to ListCache.xml
        /// </summary>
        public void SaveListCache(List<ListCacheItem> items)
        {
            string filePath = Path.Combine(_launchBoxDataPath, "ListCache.xml");
            SerializeList(filePath, items, "ListCacheItem");
        }

        #endregion

        #region Import Blacklist

        /// <summary>
        /// Loads the import blacklist from ImportBlacklist.xml
        /// </summary>
        public List<IgnoredGameId> LoadImportBlacklist()
        {
            string filePath = Path.Combine(_launchBoxDataPath, "ImportBlacklist.xml");
            if (!File.Exists(filePath))
                return new List<IgnoredGameId>();

            return DeserializeList<IgnoredGameId>(filePath);
        }

        /// <summary>
        /// Saves the import blacklist to ImportBlacklist.xml
        /// </summary>
        public void SaveImportBlacklist(List<IgnoredGameId> items)
        {
            string filePath = Path.Combine(_launchBoxDataPath, "ImportBlacklist.xml");
            SerializeList(filePath, items, "IgnoredGameId");
        }

        #endregion

        #region Settings

        /// <summary>
        /// Loads Desktop settings from Settings.xml
        /// </summary>
        public Settings LoadSettings()
        {
            string filePath = Path.Combine(_launchBoxDataPath, "Settings.xml");
            if (!File.Exists(filePath))
                return new Settings();

            var doc = new XmlDocument();
            doc.Load(filePath);

            var settingsNode = doc.SelectSingleNode("//Settings");
            if (settingsNode == null)
                return new Settings();

            return DeserializeNode<Settings>(settingsNode);
        }

        /// <summary>
        /// Saves Desktop settings to Settings.xml
        /// </summary>
        public void SaveSettings(Settings settings)
        {
            string filePath = Path.Combine(_launchBoxDataPath, "Settings.xml");
            SerializeSingle(filePath, settings, "Settings");
        }

        /// <summary>
        /// Loads BigScreen settings from BigBoxSettings.xml
        /// </summary>
        public BigBoxSettings LoadBigBoxSettings()
        {
            string filePath = Path.Combine(_launchBoxDataPath, "BigBoxSettings.xml");
            if (!File.Exists(filePath))
                return new BigBoxSettings();

            var doc = new XmlDocument();
            doc.Load(filePath);

            var settingsNode = doc.SelectSingleNode("//BigBoxSettings");
            if (settingsNode == null)
                return new BigBoxSettings();

            return DeserializeNode<BigBoxSettings>(settingsNode);
        }

        /// <summary>
        /// Saves BigScreen settings to BigBoxSettings.xml
        /// </summary>
        public void SaveBigBoxSettings(BigBoxSettings settings)
        {
            string filePath = Path.Combine(_launchBoxDataPath, "BigBoxSettings.xml");
            SerializeSingle(filePath, settings, "BigBoxSettings");
        }

        #endregion

        #region Private Helpers

        private string GetPlatformFilePath(string platformName)
        {
            return Path.Combine(_launchBoxDataPath, "Platforms", $"{platformName}.xml");
        }

        private string GetPlaylistFilePath(string playlistName)
        {
            var dir = Path.Combine(_launchBoxDataPath, "Playlists");

            // Fast path: name matches file name exactly
            var exactPath = Path.Combine(dir, $"{playlistName}.xml");
            if (File.Exists(exactPath)) return exactPath;

            // Slow path: LaunchBox may have sanitized the file name (e.g. ' → _).
            // Build a lazy map from display name → file name and look it up.
            var map = EnsurePlaylistNameToFileNameMap(dir);
            if (map.TryGetValue(playlistName, out var resolvedFileName))
                return Path.Combine(dir, $"{resolvedFileName}.xml");

            // Fallback: return the original path so the caller sees "file not found"
            return exactPath;
        }

        /// <summary>
        /// Builds (once) a dictionary mapping each playlist's display name to its file name on disk.
        /// This handles LaunchBox's file-name sanitization (apostrophes, colons, etc. become underscores).
        /// </summary>
        private Dictionary<string, string> EnsurePlaylistNameToFileNameMap(string playlistsDir)
        {
            if (_playlistNameToFileNameMap != null) return _playlistNameToFileNameMap;

            _playlistNameToFileNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!Directory.Exists(playlistsDir)) return _playlistNameToFileNameMap;

            foreach (var filePath in Directory.GetFiles(playlistsDir, "*.xml"))
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
                try
                {
                    // Read only the <Name> element — fast XPath, no full deserialization
                    var doc = new XmlDocument();
                    doc.Load(filePath);
                    var nameNode = doc.SelectSingleNode("/LaunchBox/Playlist/Name");
                    var displayName = nameNode?.InnerText;

                    if (!string.IsNullOrWhiteSpace(displayName))
                        _playlistNameToFileNameMap[displayName] = fileNameNoExt;

                    // Also register the file name itself in case the caller already uses it
                    _playlistNameToFileNameMap.TryAdd(fileNameNoExt, fileNameNoExt);
                }
                catch
                {
                    // If a file is corrupt / unreadable, skip it gracefully
                    _playlistNameToFileNameMap.TryAdd(fileNameNoExt, fileNameNoExt);
                }
            }

            return _playlistNameToFileNameMap;
        }

        #endregion

        #region Private Serialization Methods

        private List<T> DeserializeList<T>(string filePath) where T : class
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(filePath);

                return DeserializeNodesFromDoc<T>(doc);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error loading {typeof(T).Name} from {filePath}: {ex.Message}", ex);
            }
        }

        private List<T> DeserializeNodesFromDoc<T>(XmlDocument doc) where T : class
        {
            var items = new List<T>();
            var typeName = typeof(T).Name;
            var nodes = doc.SelectNodes($"//{typeName}");

            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    items.Add(DeserializeNode<T>(node));
                }
            }

            return items;
        }

        private T DeserializeNode<T>(XmlNode node) where T : class
        {
            try
            {
                using (var stringReader = new StringReader(node.OuterXml))
                using (var xmlReader = XmlReader.Create(stringReader, _xmlReaderSettings))
                {
                    var serializer = new XmlSerializer(typeof(T));

                    // Ignore unknown elements and attributes for compatibility
                    serializer.UnknownElement += (sender, e) => { /* Ignore */ };
                    serializer.UnknownAttribute += (sender, e) => { /* Ignore */ };
                    serializer.UnknownNode += (sender, e) => { /* Ignore */ };

                    return (T)serializer.Deserialize(xmlReader)!;
                }
            }
            catch (Exception ex)
            {
                // Show first 500 chars of the problematic XML for debugging
                var xmlPreview = node.OuterXml.Length > 500 ? node.OuterXml.Substring(0, 500) + "..." : node.OuterXml;
                throw new InvalidOperationException($"Error deserializing {typeof(T).Name}: {ex.Message}\n\nXML Preview:\n{xmlPreview}", ex);
            }
        }

        private void SerializeList<T>(string filePath, List<T> items, string elementName) where T : class
        {
            using (var writer = XmlWriter.Create(filePath, _xmlWriterSettings))
            {
                writer.WriteStartDocument(true); // standalone="yes"
                writer.WriteStartElement("LaunchBox");

                var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(elementName));
                var emptyNamespaces = new XmlSerializerNamespaces();
                emptyNamespaces.Add("", ""); // Remove default namespaces

                foreach (var item in items)
                {
                    serializer.Serialize(writer, item, emptyNamespaces);
                }

                writer.WriteEndElement(); // </LaunchBox>
                writer.WriteEndDocument();
            }
        }

        private void SerializeItems<T>(XmlWriter writer, List<T> items, string elementName, XmlSerializerNamespaces namespaces) where T : class
        {
            if (items == null || items.Count == 0)
                return;

            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(elementName));
            foreach (var item in items)
            {
                serializer.Serialize(writer, item, namespaces);
            }
        }

        private void SerializeSingle<T>(string filePath, T item, string elementName) where T : class
        {
            using (var writer = XmlWriter.Create(filePath, _xmlWriterSettings))
            {
                writer.WriteStartDocument(true); // standalone="yes"
                writer.WriteStartElement("LaunchBox");

                var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(elementName));
                var emptyNamespaces = new XmlSerializerNamespaces();
                emptyNamespaces.Add("", "");

                serializer.Serialize(writer, item, emptyNamespaces);

                writer.WriteEndElement(); // </LaunchBox>
                writer.WriteEndDocument();
            }
        }

        #endregion
    }

    /// <summary>
    /// Contains all entity types loaded from a single platform XML file.
    /// </summary>
    public class PlatformFileData
    {
        public List<Game> Games { get; set; } = new List<Game>();
        public List<AdditionalApplication> AdditionalApplications { get; set; } = new List<AdditionalApplication>();
        public List<CustomField> CustomFields { get; set; } = new List<CustomField>();
        public List<GameControllerSupport> GameControllerSupports { get; set; } = new List<GameControllerSupport>();
        public List<AlternateName> AlternateNames { get; set; } = new List<AlternateName>();
    }

    /// <summary>
    /// Contains all entity types loaded from a single playlist XML file.
    /// </summary>
    public class PlaylistFileData
    {
        public Playlist? Playlist { get; set; }
        public List<PlaylistGame> Games { get; set; } = new List<PlaylistGame>();
        public List<PlaylistFilter> Filters { get; set; } = new List<PlaylistFilter>();
    }
}
