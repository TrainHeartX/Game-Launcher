using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GameLauncher.Core.Models;
using GameLauncher.Data.Xml;
using GameLauncher.Infrastructure.Services;

namespace GameLauncher.BigScreen.ViewModels
{
    /// <summary>
    /// ViewModel for the full-screen game metadata editor.
    /// Manages sections, fields, navigation, and persistence.
    /// </summary>
    public partial class GameEditorViewModel : ObservableObject
    {
        private readonly Game _editingGame;
        private readonly IGameManager _gameManager;
        private readonly XmlDataContext _dataContext;

        // Backup of original values for cancel
        private readonly Dictionary<string, object?> _originalValues = new();

        public string[] Sections { get; } = { "Metadatos", "Estado", "Notas", "URLs", "Lanzamiento", "Emulación" };

        [ObservableProperty]
        private int _currentSectionIndex;

        [ObservableProperty]
        private string _currentSectionName = "Metadatos";

        [ObservableProperty]
        private ObservableCollection<EditorField> _currentFields = new();

        [ObservableProperty]
        private EditorField? _selectedField;

        [ObservableProperty]
        private bool _isFieldEditing;

        [ObservableProperty]
        private string _gameTitle = string.Empty;

        [ObservableProperty]
        private string _gamePlatform = string.Empty;

        // Emulator data
        private List<Emulator> _allEmulators = new();
        private List<EmulatorPlatform> _emulatorMappings = new();
        private List<Emulator> _platformEmulators = new();

        public GameEditorViewModel(Game game, IGameManager gameManager, XmlDataContext dataContext)
        {
            _editingGame = game ?? throw new ArgumentNullException(nameof(game));
            _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));

            GameTitle = game.Title;
            GamePlatform = game.Platform;

            BackupOriginalValues();
            LoadEmulatorData();
            BuildFields(0);
        }

        private void BackupOriginalValues()
        {
            _originalValues["Title"] = _editingGame.Title;
            _originalValues["Genre"] = _editingGame.Genre;
            _originalValues["Developer"] = _editingGame.Developer;
            _originalValues["Publisher"] = _editingGame.Publisher;
            _originalValues["Series"] = _editingGame.Series;
            _originalValues["Platform"] = _editingGame.Platform;
            _originalValues["ReleaseDate"] = _editingGame.ReleaseDate;
            _originalValues["Region"] = _editingGame.Region;
            _originalValues["PlayMode"] = _editingGame.PlayMode;
            _originalValues["MaxPlayers"] = _editingGame.MaxPlayers;
            _originalValues["ReleaseType"] = _editingGame.ReleaseType;
            _originalValues["Version"] = _editingGame.Version;
            _originalValues["Source"] = _editingGame.Source;
            _originalValues["Status"] = _editingGame.Status;
            _originalValues["Favorite"] = _editingGame.Favorite;
            _originalValues["Completed"] = _editingGame.Completed;
            _originalValues["Installed"] = _editingGame.Installed;
            _originalValues["Broken"] = _editingGame.Broken;
            _originalValues["Hide"] = _editingGame.Hide;
            _originalValues["Portable"] = _editingGame.Portable;
            _originalValues["StarRating"] = _editingGame.StarRating;
            _originalValues["Notes"] = _editingGame.Notes;
            _originalValues["VideoUrl"] = _editingGame.VideoUrl;
            _originalValues["WikipediaURL"] = _editingGame.WikipediaURL;
            _originalValues["ApplicationPath"] = _editingGame.ApplicationPath;
            _originalValues["CommandLine"] = _editingGame.CommandLine;
            _originalValues["ConfigurationPath"] = _editingGame.ConfigurationPath;
            _originalValues["ConfigurationCommandLine"] = _editingGame.ConfigurationCommandLine;
            _originalValues["Emulator"] = _editingGame.Emulator;
        }

        private void LoadEmulatorData()
        {
            try
            {
                var (emulators, mappings) = _dataContext.LoadEmulators();
                _allEmulators = emulators;
                _emulatorMappings = mappings;

                // Get emulators that have a mapping for this game's platform
                var platformEmulatorIds = mappings
                    .Where(m => m.Platform.Equals(_editingGame.Platform, StringComparison.OrdinalIgnoreCase))
                    .Select(m => m.Emulator)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                _platformEmulators = emulators
                    .Where(e => platformEmulatorIds.Contains(e.ID))
                    .OrderBy(e => e.Title)
                    .ToList();
            }
            catch
            {
                _allEmulators = new List<Emulator>();
                _emulatorMappings = new List<EmulatorPlatform>();
                _platformEmulators = new List<Emulator>();
            }
        }

        private void BuildFields(int sectionIndex)
        {
            CurrentFields.Clear();

            switch (sectionIndex)
            {
                case 0: BuildMetadataFields(); break;
                case 1: BuildStatusFields(); break;
                case 2: BuildNotesFields(); break;
                case 3: BuildUrlFields(); break;
                case 4: BuildLaunchFields(); break;
                case 5: BuildEmulatorFields(); break;
            }

            if (CurrentFields.Count > 0)
            {
                SelectedField = CurrentFields[0];
                SelectedField.IsSelected = true;
            }
        }

        private void BuildMetadataFields()
        {
            CurrentFields.Add(new EditorField { Key = "Title", Label = "Título", FieldType = EditorFieldType.Text, Value = _editingGame.Title ?? "" });
            CurrentFields.Add(new EditorField { Key = "Genre", Label = "Género", FieldType = EditorFieldType.Text, Value = _editingGame.Genre ?? "" });
            CurrentFields.Add(new EditorField { Key = "Developer", Label = "Desarrollador", FieldType = EditorFieldType.Text, Value = _editingGame.Developer ?? "" });
            CurrentFields.Add(new EditorField { Key = "Publisher", Label = "Editor", FieldType = EditorFieldType.Text, Value = _editingGame.Publisher ?? "" });
            CurrentFields.Add(new EditorField { Key = "Series", Label = "Serie", FieldType = EditorFieldType.Text, Value = _editingGame.Series ?? "" });
            CurrentFields.Add(new EditorField { Key = "Platform", Label = "Plataforma", FieldType = EditorFieldType.Text, Value = _editingGame.Platform ?? "" });
            CurrentFields.Add(new EditorField { Key = "ReleaseDate", Label = "Fecha Lanzamiento", FieldType = EditorFieldType.Text, Value = _editingGame.ReleaseDate?.ToString("dd/MM/yyyy") ?? "" });
            CurrentFields.Add(new EditorField { Key = "Region", Label = "Región", FieldType = EditorFieldType.Text, Value = _editingGame.Region ?? "" });
            CurrentFields.Add(new EditorField { Key = "PlayMode", Label = "Modo de Juego", FieldType = EditorFieldType.Text, Value = _editingGame.PlayMode ?? "" });
            CurrentFields.Add(new EditorField { Key = "MaxPlayers", Label = "Max. Jugadores", FieldType = EditorFieldType.Text, Value = _editingGame.MaxPlayers.ToString() });
            CurrentFields.Add(new EditorField { Key = "ReleaseType", Label = "Tipo Lanzamiento", FieldType = EditorFieldType.Text, Value = _editingGame.ReleaseType ?? "" });
            CurrentFields.Add(new EditorField { Key = "Version", Label = "Versión", FieldType = EditorFieldType.Text, Value = _editingGame.Version ?? "" });
            CurrentFields.Add(new EditorField { Key = "Source", Label = "Fuente", FieldType = EditorFieldType.Text, Value = _editingGame.Source ?? "" });
            CurrentFields.Add(new EditorField { Key = "Status", Label = "Estado", FieldType = EditorFieldType.Text, Value = _editingGame.Status ?? "" });
        }

        private void BuildStatusFields()
        {
            CurrentFields.Add(new EditorField { Key = "Favorite", Label = "Favorito", FieldType = EditorFieldType.Toggle, Value = _editingGame.Favorite ? "SI" : "NO" });
            CurrentFields.Add(new EditorField { Key = "Completed", Label = "Completado", FieldType = EditorFieldType.Toggle, Value = _editingGame.Completed ? "SI" : "NO" });
            CurrentFields.Add(new EditorField { Key = "Installed", Label = "Instalado", FieldType = EditorFieldType.Toggle, Value = _editingGame.Installed ? "SI" : "NO" });
            CurrentFields.Add(new EditorField { Key = "Broken", Label = "Roto", FieldType = EditorFieldType.Toggle, Value = _editingGame.Broken ? "SI" : "NO" });
            CurrentFields.Add(new EditorField { Key = "Hide", Label = "Ocultar", FieldType = EditorFieldType.Toggle, Value = _editingGame.Hide ? "SI" : "NO" });
            CurrentFields.Add(new EditorField { Key = "Portable", Label = "Portable", FieldType = EditorFieldType.Toggle, Value = _editingGame.Portable ? "SI" : "NO" });
            CurrentFields.Add(new EditorField { Key = "StarRating", Label = "Calificación", FieldType = EditorFieldType.Stars, Value = _editingGame.StarRating.ToString() });

            // Read-only fields
            CurrentFields.Add(new EditorField { Key = "DatabaseID", Label = "Database ID", FieldType = EditorFieldType.ReadOnly, Value = _editingGame.DatabaseID ?? "" });
            CurrentFields.Add(new EditorField { Key = "DateAdded", Label = "Fecha Agregado", FieldType = EditorFieldType.ReadOnly, Value = _editingGame.DateAdded?.ToString("dd/MM/yyyy HH:mm") ?? "" });
            CurrentFields.Add(new EditorField { Key = "DateModified", Label = "Fecha Modificado", FieldType = EditorFieldType.ReadOnly, Value = _editingGame.DateModified?.ToString("dd/MM/yyyy HH:mm") ?? "" });
            CurrentFields.Add(new EditorField { Key = "LastPlayed", Label = "Última Vez Jugado", FieldType = EditorFieldType.ReadOnly, Value = _editingGame.LastPlayed?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca" });
            CurrentFields.Add(new EditorField { Key = "PlayStats", Label = "Veces Jugado / Tiempo", FieldType = EditorFieldType.ReadOnly, Value = $"{_editingGame.PlayCount} veces / {FormatPlayTime(_editingGame.PlayTime)}" });
            CurrentFields.Add(new EditorField { Key = "CommunityStarRating", Label = "Rating Comunidad", FieldType = EditorFieldType.ReadOnly, Value = _editingGame.CommunityStarRating > 0 ? $"{_editingGame.CommunityStarRating:F1} ({_editingGame.CommunityStarRatingTotalVotes} votos)" : "Sin datos" });
        }

        private void BuildNotesFields()
        {
            CurrentFields.Add(new EditorField { Key = "Notes", Label = "Notas", FieldType = EditorFieldType.MultilineText, Value = _editingGame.Notes ?? "" });
        }

        private void BuildUrlFields()
        {
            CurrentFields.Add(new EditorField { Key = "VideoUrl", Label = "URL Video", FieldType = EditorFieldType.Text, Value = _editingGame.VideoUrl ?? "" });
            CurrentFields.Add(new EditorField { Key = "WikipediaURL", Label = "URL Wikipedia", FieldType = EditorFieldType.Text, Value = _editingGame.WikipediaURL ?? "" });
        }

        private void BuildLaunchFields()
        {
            CurrentFields.Add(new EditorField { Key = "ApplicationPath", Label = "Ruta Aplicación", FieldType = EditorFieldType.Text, Value = _editingGame.ApplicationPath ?? "" });
            CurrentFields.Add(new EditorField { Key = "CommandLine", Label = "Línea de Comandos", FieldType = EditorFieldType.Text, Value = _editingGame.CommandLine ?? "" });
            CurrentFields.Add(new EditorField { Key = "ConfigurationPath", Label = "Ruta Configuración", FieldType = EditorFieldType.Text, Value = _editingGame.ConfigurationPath ?? "" });
            CurrentFields.Add(new EditorField { Key = "ConfigurationCommandLine", Label = "Params Configuración", FieldType = EditorFieldType.Text, Value = _editingGame.ConfigurationCommandLine ?? "" });
        }

        private void BuildEmulatorFields()
        {
            // Toggle: use emulator
            bool useEmulator = !string.IsNullOrWhiteSpace(_editingGame.Emulator);
            CurrentFields.Add(new EditorField
            {
                Key = "UseEmulator",
                Label = "Usar emulador para este juego",
                FieldType = EditorFieldType.Toggle,
                Value = useEmulator ? "SI" : "NO"
            });

            // Dropdown: emulator selection
            var options = new List<string> { "(Predeterminado de plataforma)" };
            options.AddRange(_platformEmulators.Select(e => e.Title));

            string currentEmulatorName = "(Predeterminado de plataforma)";
            if (!string.IsNullOrWhiteSpace(_editingGame.Emulator))
            {
                var emu = _platformEmulators.FirstOrDefault(e =>
                    e.ID.Equals(_editingGame.Emulator, StringComparison.OrdinalIgnoreCase));
                if (emu != null)
                    currentEmulatorName = emu.Title;
            }

            CurrentFields.Add(new EditorField
            {
                Key = "Emulator",
                Label = "Escoge un emulador",
                FieldType = EditorFieldType.Dropdown,
                Value = currentEmulatorName,
                DropdownOptions = options.ToArray()
            });

            // Custom command line
            CurrentFields.Add(new EditorField
            {
                Key = "CommandLine",
                Label = "Línea de Comandos Personalizada",
                FieldType = EditorFieldType.Text,
                Value = _editingGame.CommandLine ?? ""
            });
        }

        // ═══════════════════════════════════════════
        //  NAVIGATION
        // ═══════════════════════════════════════════

        public void NavigateUp()
        {
            if (IsFieldEditing || CurrentFields.Count == 0 || SelectedField == null) return;
            int idx = CurrentFields.IndexOf(SelectedField);
            if (idx > 0)
            {
                SelectedField.IsSelected = false;
                SelectedField = CurrentFields[idx - 1];
                SelectedField.IsSelected = true;
            }
        }

        public void NavigateDown()
        {
            if (IsFieldEditing || CurrentFields.Count == 0 || SelectedField == null) return;
            int idx = CurrentFields.IndexOf(SelectedField);
            if (idx < CurrentFields.Count - 1)
            {
                SelectedField.IsSelected = false;
                SelectedField = CurrentFields[idx + 1];
                SelectedField.IsSelected = true;
            }
        }

        public void NavigateLeft()
        {
            if (IsFieldEditing || SelectedField == null) return;

            switch (SelectedField.FieldType)
            {
                case EditorFieldType.Toggle:
                    ToggleField(SelectedField);
                    break;
                case EditorFieldType.Stars:
                    ChangeStars(SelectedField, -1);
                    break;
                case EditorFieldType.Dropdown:
                    CycleDropdown(SelectedField, -1);
                    break;
            }
        }

        public void NavigateRight()
        {
            if (IsFieldEditing || SelectedField == null) return;

            switch (SelectedField.FieldType)
            {
                case EditorFieldType.Toggle:
                    ToggleField(SelectedField);
                    break;
                case EditorFieldType.Stars:
                    ChangeStars(SelectedField, 1);
                    break;
                case EditorFieldType.Dropdown:
                    CycleDropdown(SelectedField, 1);
                    break;
            }
        }

        public void NextSection()
        {
            if (IsFieldEditing) return;
            if (CurrentSectionIndex < Sections.Length - 1)
            {
                CurrentSectionIndex++;
                CurrentSectionName = Sections[CurrentSectionIndex];
                BuildFields(CurrentSectionIndex);
            }
        }

        public void PreviousSection()
        {
            if (IsFieldEditing) return;
            if (CurrentSectionIndex > 0)
            {
                CurrentSectionIndex--;
                CurrentSectionName = Sections[CurrentSectionIndex];
                BuildFields(CurrentSectionIndex);
            }
        }

        // ═══════════════════════════════════════════
        //  FIELD ACTIONS
        // ═══════════════════════════════════════════

        /// <summary>
        /// B button: Enter edit mode or toggle for the selected field.
        /// </summary>
        public void ConfirmField()
        {
            if (SelectedField == null) return;

            switch (SelectedField.FieldType)
            {
                case EditorFieldType.Toggle:
                    ToggleField(SelectedField);
                    break;
                case EditorFieldType.Stars:
                    // B on stars cycles through 0-5
                    ChangeStars(SelectedField, 1);
                    break;
                case EditorFieldType.Text:
                case EditorFieldType.MultilineText:
                    SelectedField.IsEditing = true;
                    IsFieldEditing = true;
                    break;
                case EditorFieldType.Dropdown:
                    CycleDropdown(SelectedField, 1);
                    break;
                case EditorFieldType.ReadOnly:
                    // Do nothing
                    break;
            }
        }

        /// <summary>
        /// Called when the user confirms text editing (Enter key in TextBox).
        /// </summary>
        public void ConfirmTextEdit()
        {
            if (SelectedField == null || !SelectedField.IsEditing) return;
            SelectedField.IsEditing = false;
            IsFieldEditing = false;
            ApplyFieldToGame(SelectedField);
        }

        /// <summary>
        /// Y button while editing a field: cancel the edit.
        /// </summary>
        public void CancelField()
        {
            if (SelectedField != null && SelectedField.IsEditing)
            {
                // Restore value from game model
                SelectedField.IsEditing = false;
                IsFieldEditing = false;
                RestoreFieldFromGame(SelectedField);
            }
        }

        private void ToggleField(EditorField field)
        {
            bool current = field.Value == "SI";
            field.Value = current ? "NO" : "SI";
            ApplyFieldToGame(field);
        }

        private void ChangeStars(EditorField field, int delta)
        {
            if (!int.TryParse(field.Value, out int current)) current = 0;
            int newVal = current + delta;
            if (newVal < 0) newVal = 5;
            if (newVal > 5) newVal = 0;
            field.Value = newVal.ToString();
            ApplyFieldToGame(field);
        }

        private void CycleDropdown(EditorField field, int direction)
        {
            if (field.DropdownOptions == null || field.DropdownOptions.Length == 0) return;
            int idx = Array.IndexOf(field.DropdownOptions, field.Value);
            if (idx < 0) idx = 0;
            idx += direction;
            if (idx < 0) idx = field.DropdownOptions.Length - 1;
            if (idx >= field.DropdownOptions.Length) idx = 0;
            field.Value = field.DropdownOptions[idx];
            ApplyFieldToGame(field);
        }

        // ═══════════════════════════════════════════
        //  APPLY / RESTORE
        // ═══════════════════════════════════════════

        private void ApplyFieldToGame(EditorField field)
        {
            switch (field.Key)
            {
                case "Title": _editingGame.Title = field.Value; GameTitle = field.Value; break;
                case "Genre": _editingGame.Genre = NullIfEmpty(field.Value); break;
                case "Developer": _editingGame.Developer = NullIfEmpty(field.Value); break;
                case "Publisher": _editingGame.Publisher = NullIfEmpty(field.Value); break;
                case "Series": _editingGame.Series = NullIfEmpty(field.Value); break;
                case "Platform": _editingGame.Platform = field.Value; GamePlatform = field.Value; break;
                case "ReleaseDate":
                    if (DateTime.TryParse(field.Value, out var dt))
                        _editingGame.ReleaseDate = dt;
                    else if (string.IsNullOrWhiteSpace(field.Value))
                        _editingGame.ReleaseDate = null;
                    break;
                case "Region": _editingGame.Region = NullIfEmpty(field.Value); break;
                case "PlayMode": _editingGame.PlayMode = NullIfEmpty(field.Value); break;
                case "MaxPlayers":
                    if (int.TryParse(field.Value, out var mp)) _editingGame.MaxPlayers = mp;
                    break;
                case "ReleaseType": _editingGame.ReleaseType = NullIfEmpty(field.Value); break;
                case "Version": _editingGame.Version = NullIfEmpty(field.Value); break;
                case "Source": _editingGame.Source = NullIfEmpty(field.Value); break;
                case "Status": _editingGame.Status = NullIfEmpty(field.Value); break;

                case "Favorite": _editingGame.Favorite = field.Value == "SI"; break;
                case "Completed": _editingGame.Completed = field.Value == "SI"; break;
                case "Installed": _editingGame.Installed = field.Value == "SI"; break;
                case "Broken": _editingGame.Broken = field.Value == "SI"; break;
                case "Hide": _editingGame.Hide = field.Value == "SI"; break;
                case "Portable": _editingGame.Portable = field.Value == "SI"; break;
                case "StarRating":
                    if (int.TryParse(field.Value, out var sr)) _editingGame.StarRating = sr;
                    break;

                case "Notes": _editingGame.Notes = NullIfEmpty(field.Value); break;
                case "VideoUrl": _editingGame.VideoUrl = NullIfEmpty(field.Value); break;
                case "WikipediaURL": _editingGame.WikipediaURL = NullIfEmpty(field.Value); break;

                case "ApplicationPath": _editingGame.ApplicationPath = NullIfEmpty(field.Value); break;
                case "ConfigurationPath": _editingGame.ConfigurationPath = NullIfEmpty(field.Value); break;
                case "ConfigurationCommandLine": _editingGame.ConfigurationCommandLine = NullIfEmpty(field.Value); break;

                case "UseEmulator":
                    if (field.Value == "NO")
                        _editingGame.Emulator = null;
                    break;

                case "Emulator":
                    if (field.Value == "(Predeterminado de plataforma)")
                    {
                        _editingGame.Emulator = null;
                    }
                    else
                    {
                        var emu = _platformEmulators.FirstOrDefault(e =>
                            e.Title.Equals(field.Value, StringComparison.OrdinalIgnoreCase));
                        _editingGame.Emulator = emu?.ID;
                    }
                    // Also set UseEmulator toggle if present
                    var useEmuField = CurrentFields.FirstOrDefault(f => f.Key == "UseEmulator");
                    if (useEmuField != null)
                        useEmuField.Value = string.IsNullOrWhiteSpace(_editingGame.Emulator) ? "NO" : "SI";
                    break;

                case "CommandLine":
                    _editingGame.CommandLine = NullIfEmpty(field.Value);
                    break;
            }
        }

        private void RestoreFieldFromGame(EditorField field)
        {
            switch (field.Key)
            {
                case "Title": field.Value = _editingGame.Title ?? ""; break;
                case "Genre": field.Value = _editingGame.Genre ?? ""; break;
                case "Developer": field.Value = _editingGame.Developer ?? ""; break;
                case "Publisher": field.Value = _editingGame.Publisher ?? ""; break;
                case "Series": field.Value = _editingGame.Series ?? ""; break;
                case "Platform": field.Value = _editingGame.Platform ?? ""; break;
                case "ReleaseDate": field.Value = _editingGame.ReleaseDate?.ToString("dd/MM/yyyy") ?? ""; break;
                case "Region": field.Value = _editingGame.Region ?? ""; break;
                case "PlayMode": field.Value = _editingGame.PlayMode ?? ""; break;
                case "MaxPlayers": field.Value = _editingGame.MaxPlayers.ToString(); break;
                case "ReleaseType": field.Value = _editingGame.ReleaseType ?? ""; break;
                case "Version": field.Value = _editingGame.Version ?? ""; break;
                case "Source": field.Value = _editingGame.Source ?? ""; break;
                case "Status": field.Value = _editingGame.Status ?? ""; break;
                case "Notes": field.Value = _editingGame.Notes ?? ""; break;
                case "VideoUrl": field.Value = _editingGame.VideoUrl ?? ""; break;
                case "WikipediaURL": field.Value = _editingGame.WikipediaURL ?? ""; break;
                case "ApplicationPath": field.Value = _editingGame.ApplicationPath ?? ""; break;
                case "CommandLine": field.Value = _editingGame.CommandLine ?? ""; break;
                case "ConfigurationPath": field.Value = _editingGame.ConfigurationPath ?? ""; break;
                case "ConfigurationCommandLine": field.Value = _editingGame.ConfigurationCommandLine ?? ""; break;
            }
        }

        // ═══════════════════════════════════════════
        //  SAVE / CANCEL
        // ═══════════════════════════════════════════

        /// <summary>
        /// A button: Save all changes to XML.
        /// </summary>
        public async Task SaveAsync()
        {
            // Apply currently editing field if any
            if (SelectedField != null && SelectedField.IsEditing)
            {
                SelectedField.IsEditing = false;
                IsFieldEditing = false;
                ApplyFieldToGame(SelectedField);
            }

            await _gameManager.UpdateGameAsync(_editingGame.Platform, _editingGame);
        }

        /// <summary>
        /// Y button (not in field edit mode): Cancel and restore original values.
        /// </summary>
        public void Cancel()
        {
            // Restore all backed-up values
            _editingGame.Title = (string)_originalValues["Title"]!;
            _editingGame.Genre = (string?)_originalValues["Genre"];
            _editingGame.Developer = (string?)_originalValues["Developer"];
            _editingGame.Publisher = (string?)_originalValues["Publisher"];
            _editingGame.Series = (string?)_originalValues["Series"];
            _editingGame.Platform = (string)_originalValues["Platform"]!;
            _editingGame.ReleaseDate = (DateTime?)_originalValues["ReleaseDate"];
            _editingGame.Region = (string?)_originalValues["Region"];
            _editingGame.PlayMode = (string?)_originalValues["PlayMode"];
            _editingGame.MaxPlayers = (int)_originalValues["MaxPlayers"]!;
            _editingGame.ReleaseType = (string?)_originalValues["ReleaseType"];
            _editingGame.Version = (string?)_originalValues["Version"];
            _editingGame.Source = (string?)_originalValues["Source"];
            _editingGame.Status = (string?)_originalValues["Status"];
            _editingGame.Favorite = (bool)_originalValues["Favorite"]!;
            _editingGame.Completed = (bool)_originalValues["Completed"]!;
            _editingGame.Installed = (bool)_originalValues["Installed"]!;
            _editingGame.Broken = (bool)_originalValues["Broken"]!;
            _editingGame.Hide = (bool)_originalValues["Hide"]!;
            _editingGame.Portable = (bool)_originalValues["Portable"]!;
            _editingGame.StarRating = (int)_originalValues["StarRating"]!;
            _editingGame.Notes = (string?)_originalValues["Notes"];
            _editingGame.VideoUrl = (string?)_originalValues["VideoUrl"];
            _editingGame.WikipediaURL = (string?)_originalValues["WikipediaURL"];
            _editingGame.ApplicationPath = (string?)_originalValues["ApplicationPath"];
            _editingGame.CommandLine = (string?)_originalValues["CommandLine"];
            _editingGame.ConfigurationPath = (string?)_originalValues["ConfigurationPath"];
            _editingGame.ConfigurationCommandLine = (string?)_originalValues["ConfigurationCommandLine"];
            _editingGame.Emulator = (string?)_originalValues["Emulator"];
        }

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;

        private static string FormatPlayTime(long seconds)
        {
            if (seconds == 0) return "0m";
            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);
            return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
        }
    }
}
