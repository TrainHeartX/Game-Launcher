using System;
using System.Globalization;
using System.Windows.Data;

namespace GameLauncher.BigScreen.Converters
{
    /// <summary>
    /// Converts a numeric string (0-5) to a star display string (★★★☆☆).
    /// </summary>
    public class StarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && int.TryParse(s, out int stars))
            {
                stars = Math.Max(0, Math.Min(5, stars));
                return new string('\u2605', stars) + new string('\u2606', 5 - stars);
            }
            return "\u2606\u2606\u2606\u2606\u2606";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
