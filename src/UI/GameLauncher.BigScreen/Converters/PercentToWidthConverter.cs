using System;
using System.Globalization;
using System.Windows.Data;

namespace GameLauncher.BigScreen.Converters;

/// <summary>
/// Converts a percentage (0-100) and container width to the actual width for a progress bar.
/// </summary>
public class PercentToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] == null || values[1] == null)
            return 0.0;

        if (values[0] is int percent && values[1] is double containerWidth)
        {
            return (percent / 100.0) * containerWidth;
        }

        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
