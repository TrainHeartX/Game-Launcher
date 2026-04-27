using System;
using System.Globalization;
using System.Windows.Data;

namespace GameLauncher.UI.Shared.Converters
{
    /// <summary>
    /// Convierte segundos de tiempo de juego a formato legible (ej: "5h 23m").
    /// </summary>
    public class PlayTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long seconds = 0;

            if (value is long longVal)
                seconds = longVal;
            else if (value is int intVal)
                seconds = intVal;

            if (seconds <= 0)
                return "Sin jugar";

            var timeSpan = TimeSpan.FromSeconds(seconds);

            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";

            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes}m";

            return $"{seconds}s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
