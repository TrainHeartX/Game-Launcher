using CommunityToolkit.Mvvm.ComponentModel;

namespace GameLauncher.BigScreen.ViewModels
{
    public enum EditorFieldType
    {
        Text,
        Toggle,
        Stars,
        ReadOnly,
        MultilineText,
        Dropdown
    }

    /// <summary>
    /// Represents a single editable field in the game metadata editor.
    /// </summary>
    public partial class EditorField : ObservableObject
    {
        /// <summary>Property name on the Game model.</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>Display label shown to the user.</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>Type of field control to render.</summary>
        public EditorFieldType FieldType { get; set; }

        /// <summary>Current value as string.</summary>
        [ObservableProperty]
        private string _value = "";

        /// <summary>Whether this field is currently in text-edit mode.</summary>
        [ObservableProperty]
        private bool _isEditing;

        /// <summary>Whether this field is highlighted/selected.</summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>For Dropdown fields: available options to choose from.</summary>
        public string[]? DropdownOptions { get; set; }
    }
}
