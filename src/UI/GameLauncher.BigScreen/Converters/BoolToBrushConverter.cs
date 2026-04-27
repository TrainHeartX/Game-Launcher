using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameLauncher.BigScreen.Converters
{
    /// <summary>
    /// Convierte bool a Brush. True = verde (instalado), False = rojo (no instalado).
    /// </summary>
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value is bool v && v;
            return b
                ? new SolidColorBrush(Color.FromArgb(0xCC, 0x27, 0xAE, 0x60))  // verde
                : new SolidColorBrush(Color.FromArgb(0xCC, 0xC0, 0x39, 0x2B)); // rojo
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
