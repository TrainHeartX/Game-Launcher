using System;
using System.Globalization;
using System.Windows.Data;

namespace GameLauncher.BigScreen.Converters
{
    /// <summary>
    /// Convierte bool a string usando ConverterParameter con formato "TrueText|FalseText".
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            var parts = parameter?.ToString()?.Split('|');

            if (parts == null || parts.Length < 2)
                return boolValue.ToString();

            return boolValue ? parts[0] : parts[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
