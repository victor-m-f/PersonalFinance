using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;

namespace PersonalFinance.Ui.Shared.Controls;

public partial class HexColorPicker : UserControl
{
    public static readonly DependencyProperty SelectedColorHexProperty = DependencyProperty.Register(
        nameof(SelectedColorHex),
        typeof(string),
        typeof(HexColorPicker),
        new FrameworkPropertyMetadata("#FF3B82F6", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PaletteProperty = DependencyProperty.Register(
        nameof(Palette),
        typeof(IReadOnlyList<string>),
        typeof(HexColorPicker),
        new PropertyMetadata(null));

    public HexColorPicker()
    {
        InitializeComponent();
        SelectColorCommand = new RelayCommand<string?>(SelectColor);

        if (Palette is null)
        {
            Palette = new ReadOnlyCollection<string>(BuildDefaultPalette());
        }
    }

    public string SelectedColorHex
    {
        get => (string)GetValue(SelectedColorHexProperty);
        set => SetValue(SelectedColorHexProperty, value);
    }

    public IReadOnlyList<string> Palette
    {
        get => (IReadOnlyList<string>)GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public ICommand SelectColorCommand { get; }

    private void SelectColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return;
        }

        SelectedColorHex = hex.Trim();
    }

    private static List<string> BuildDefaultPalette()
    {
        var list = new List<string>();
        const int columns = 11;
        const int rows = 6;

        for (var row = 0; row < rows; row++)
        {
            var saturation = 0.85 - (row * 0.08);
            var value = 0.95 - (row * 0.07);

            for (var col = 0; col < columns; col++)
            {
                var hue = col * (360.0 / columns);
                var color = HsvToColor(hue, saturation, value);
                list.Add(ToHex(color));
            }
        }

        for (var i = 0; i < columns; i++)
        {
            var v = 0.95 - (i * 0.08);
            var gray = Color.FromRgb(ToByte(v), ToByte(v), ToByte(v));
            list.Add(ToHex(gray));
        }

        return list;
    }

    private static Color HsvToColor(double hue, double saturation, double value)
    {
        var c = value * saturation;
        var x = c * (1 - Math.Abs((hue / 60.0 % 2) - 1));
        var m = value - c;

        double rPrime = 0;
        double gPrime = 0;
        double bPrime = 0;

        if (hue < 60)
        {
            rPrime = c;
            gPrime = x;
        }
        else if (hue < 120)
        {
            rPrime = x;
            gPrime = c;
        }
        else if (hue < 180)
        {
            gPrime = c;
            bPrime = x;
        }
        else if (hue < 240)
        {
            gPrime = x;
            bPrime = c;
        }
        else if (hue < 300)
        {
            rPrime = x;
            bPrime = c;
        }
        else
        {
            rPrime = c;
            bPrime = x;
        }

        var r = (byte)Math.Round((rPrime + m) * 255);
        var g = (byte)Math.Round((gPrime + m) * 255);
        var b = (byte)Math.Round((bPrime + m) * 255);

        return Color.FromRgb(r, g, b);
    }

    private static string ToHex(Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static byte ToByte(double value)
    {
        var scaled = Math.Clamp(value, 0, 1) * 255;
        return (byte)Math.Round(scaled);
    }
}
