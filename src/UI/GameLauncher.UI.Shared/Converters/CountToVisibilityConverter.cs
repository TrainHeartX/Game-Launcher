using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameLauncher.UI.Shared.Converters
{
    /// <summary>
    /// Convierte un count a Visibility para mostrar el "empty state".
    /// 0 = Visible (muestra mensaje de "sin elementos"), >0 = Collapsed.
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = 0;

            if (value is int intVal)
                count = intVal;
            else if (value is long longVal)
                count = (int)longVal;

            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
