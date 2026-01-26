using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;

namespace PersonalFinance.Ui.Shared.Controls;

public sealed class HexColorPicker : Control
{
    public static readonly int[] DefaultColorRowLengths = [5, 6, 7, 8, 9, 8, 7, 6, 5];
    private static readonly int DefaultColorCount = DefaultColorRowLengths.Sum();
    private static readonly IReadOnlyList<string> DefaultPalette = new ReadOnlyCollection<string>(BuildDefaultPalette());

    public static readonly DependencyProperty SelectedColorHexProperty = DependencyProperty.Register(
        nameof(SelectedColorHex),
        typeof(string),
        typeof(HexColorPicker),
        new FrameworkPropertyMetadata("#FF3B82F6", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PaletteProperty = DependencyProperty.Register(
        nameof(Palette),
        typeof(IReadOnlyList<string>),
        typeof(HexColorPicker),
        new PropertyMetadata(DefaultPalette, OnPaletteChanged));

    public static readonly DependencyProperty PaletteMainProperty = DependencyProperty.Register(
        nameof(PaletteMain),
        typeof(IReadOnlyList<string>),
        typeof(HexColorPicker),
        new PropertyMetadata(Array.Empty<string>()));

    public static readonly DependencyProperty PaletteGrayProperty = DependencyProperty.Register(
        nameof(PaletteGray),
        typeof(IReadOnlyList<string>),
        typeof(HexColorPicker),
        new PropertyMetadata(Array.Empty<string>()));


    public static readonly DependencyProperty SelectColorCommandProperty = DependencyProperty.Register(
        nameof(SelectColorCommand),
        typeof(ICommand),
        typeof(HexColorPicker));

    public static readonly DependencyProperty OpenDialogCommandProperty = DependencyProperty.Register(
        nameof(OpenDialogCommand),
        typeof(ICommand),
        typeof(HexColorPicker));

    public static readonly DependencyProperty CloseDialogCommandProperty = DependencyProperty.Register(
        nameof(CloseDialogCommand),
        typeof(ICommand),
        typeof(HexColorPicker));

    public static readonly DependencyProperty IsDialogOpenProperty = DependencyProperty.Register(
        nameof(IsDialogOpen),
        typeof(bool),
        typeof(HexColorPicker),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty DialogGroupNameProperty = DependencyProperty.Register(
        nameof(DialogGroupName),
        typeof(string),
        typeof(HexColorPicker),
        new PropertyMetadata(string.Empty));

    static HexColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HexColorPicker), new FrameworkPropertyMetadata(typeof(HexColorPicker)));
    }

    public HexColorPicker()
    {
        SelectColorCommand = new RelayCommand<string?>(SelectColor);
        OpenDialogCommand = new RelayCommand(OpenDialog);
        CloseDialogCommand = new RelayCommand(CloseDialog);
        UpdatePaletteSlices(Palette);
        if (string.IsNullOrWhiteSpace(DialogGroupName))
        {
            SetCurrentValue(DialogGroupNameProperty, $"HexColorDialog_{Guid.NewGuid():N}");
        }
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
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

    public IReadOnlyList<string> PaletteMain
    {
        get => (IReadOnlyList<string>)GetValue(PaletteMainProperty);
        private set => SetValue(PaletteMainProperty, value);
    }

    public IReadOnlyList<string> PaletteGray
    {
        get => (IReadOnlyList<string>)GetValue(PaletteGrayProperty);
        private set => SetValue(PaletteGrayProperty, value);
    }


    public ICommand SelectColorCommand
    {
        get => (ICommand)GetValue(SelectColorCommandProperty);
        set => SetValue(SelectColorCommandProperty, value);
    }

    public ICommand OpenDialogCommand
    {
        get => (ICommand)GetValue(OpenDialogCommandProperty);
        set => SetValue(OpenDialogCommandProperty, value);
    }

    public ICommand CloseDialogCommand
    {
        get => (ICommand)GetValue(CloseDialogCommandProperty);
        set => SetValue(CloseDialogCommandProperty, value);
    }

    public bool IsDialogOpen
    {
        get => (bool)GetValue(IsDialogOpenProperty);
        set => SetValue(IsDialogOpenProperty, value);
    }

    public string DialogGroupName
    {
        get => (string)GetValue(DialogGroupNameProperty);
        set => SetValue(DialogGroupNameProperty, value);
    }

    private void SelectColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return;
        }

        SelectedColorHex = hex.Trim();
        IsDialogOpen = false;
    }

    private void OpenDialog()
    {
        IsDialogOpen = true;
    }

    private void CloseDialog()
    {
        IsDialogOpen = false;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is not Window window)
        {
            return;
        }

        window.StateChanged += OnWindowStateChanged;
        window.Deactivated += OnWindowDeactivated;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is not Window window)
        {
            return;
        }

        window.StateChanged -= OnWindowStateChanged;
        window.Deactivated -= OnWindowDeactivated;
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (sender is Window { WindowState: WindowState.Minimized })
        {
            IsDialogOpen = false;
        }
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        IsDialogOpen = false;
    }

    private static List<string> BuildDefaultPalette()
    {
        return
        [
            "#F87600",
            "#FF4C13",
            "#FE1B1D",
            "#FD154F",
            "#FC0172",

            "#FFB222",
            "#FC9541",
            "#FB7050",
            "#FF526C",
            "#FE4193",
            "#FC22B7",

            "#FCF23A",
            "#FFD560",
            "#FFB77D",
            "#FD8D8F",
            "#FC81B9",
            "#FD5EDB",
            "#FF30F4",

            "#D1FF1F",
            "#E6FF67",
            "#FEF99B",
            "#F7E0BB",
            "#FFBED9",
            "#FD9AFD",
            "#E469F9",
            "#D02DFF",

            "#91FD1B",
            "#A9FF53",
            "#C7FE8B",
            "#E2FFC5",
            "#FFFEFF",
            "#E1C6FF",
            "#C38EFF",
            "#AB53FF",
            "#9019FF",

            "#5FFF2F",
            "#7BFF62",
            "#97FFA0",
            "#C2FEE2",
            "#C3E0FE",
            "#9A9FFE",
            "#7966FC",
            "#602EFE",

            "#31FE3C",
            "#59FF88",
            "#81FECA",
            "#8CFFFD",
            "#81C6FF",
            "#5E87FD",
            "#323EFD",

            "#23FD6C",
            "#3DFEAB",
            "#54FDE6",
            "#54E3FB",
            "#3FABFF",
            "#206BFB",

            "#05FD8D",
            "#7EFFC6",
            "#1AFFFC",
            "#12C7FF",
            "#008DFD"
        ];
    }

    public static int GetDefaultColorCount() => DefaultColorCount;

    private static void OnPaletteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not HexColorPicker picker)
        {
            return;
        }

        var palette = e.NewValue as IReadOnlyList<string> ?? Array.Empty<string>();
        picker.UpdatePaletteSlices(palette);
    }

    private void UpdatePaletteSlices(IReadOnlyList<string> palette)
    {
        if (palette.Count == 0)
        {
            PaletteMain = Array.Empty<string>();
            PaletteGray = Array.Empty<string>();
            return;
        }

        var main = palette.Take(DefaultColorCount).ToList();
        PaletteMain = main;
        PaletteGray = Array.Empty<string>();
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
