using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameLauncher.Desktop.Converters
{
    /// <summary>
    /// Convierte un entero ARGB (formato LaunchBox) a Color de WPF.
    /// </summary>
    public class ArgbIntegerToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int argb)
            {
                byte a = (byte)((argb >> 24) & 0xFF);
                byte r = (byte)((argb >> 16) & 0xFF);
                byte g = (byte)((argb >> 8) & 0xFF);
                byte b = (byte)(argb & 0xFF);

                return Color.FromArgb(a, r, g, b);
            }

            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
            }

            return 0;
        }
    }
}
