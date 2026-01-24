using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PersonalFinance.Ui.Shared.Converters;

public sealed class IndentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var depth = 0;
        if (value is int intValue)
        {
            depth = intValue;
        }

        var perLevel = 16.0;
        if (parameter is string parameterText && double.TryParse(parameterText, out var parsed))
        {
            perLevel = parsed;
        }

        var left = Math.Max(0, depth) * perLevel;
        return new Thickness(left, 0, 0, 0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
