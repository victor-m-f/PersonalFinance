using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PersonalFinance.Ui.Shared.Converters;

public sealed class ColorHexToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
        {
            return Brushes.Transparent;
        }

        try
        {
            var normalized = hex.Trim();
            var color = (Color)ColorConverter.ConvertFromString(normalized);
            return new SolidColorBrush(color);
        }
        catch
        {
            return Brushes.Transparent;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
