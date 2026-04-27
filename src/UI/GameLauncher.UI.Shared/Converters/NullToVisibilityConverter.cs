using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameLauncher.UI.Shared.Converters
{
    /// <summary>
    /// Convierte null a Collapsed y cualquier valor no-null a Visible.
    /// Soporta parameter="Inverse" para invertir la lógica.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value != null;

            if (parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true)
                isVisible = !isVisible;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
