using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameLauncher.BigScreen.Converters;

/// <summary>
/// Converts an integer value to Visibility based on matching a parameter.
/// Used for showing/hiding sections in SystemInfoView.
/// </summary>
public class ValueToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Visibility.Collapsed;

        if (int.TryParse(value.ToString(), out int intValue) && 
            int.TryParse(parameter.ToString(), out int paramValue))
        {
            return intValue == paramValue ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
