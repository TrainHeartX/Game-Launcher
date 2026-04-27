using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameLauncher.UI.Shared.Converters
{
    /// <summary>
    /// Convierte bool a Visibility (true = Visible, false = Collapsed).
    /// Soporta parameter="Inverse" para invertir la lógica.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value is bool boolValue && boolValue;

            if (parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true)
                isVisible = !isVisible;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value is Visibility visibility && visibility == Visibility.Visible;

            if (parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true)
                isVisible = !isVisible;

            return isVisible;
        }
    }
}
