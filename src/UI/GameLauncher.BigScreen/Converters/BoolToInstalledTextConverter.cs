using System;
using System.Globalization;
using System.Windows.Data;

namespace GameLauncher.BigScreen.Converters
{
    /// <summary>
    /// Convierte bool a texto de instalación.
    /// </summary>
    public class BoolToInstalledTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value is bool v && v;
            return b ? "INSTALADO" : "NO INSTALADO";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
