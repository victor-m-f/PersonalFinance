using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PersonalFinance.Ui.Shared.Controls;

public sealed class HexColorPickerDialogViewModel : ObservableObject
{
    private string _selectedColorHex = "#FF3B82F6";
    private IReadOnlyList<string> _palette = Array.Empty<string>();
    private string _groupName = string.Empty;

    public string SelectedColorHex
    {
        get => _selectedColorHex;
        set => SetProperty(ref _selectedColorHex, value);
    }

    public IReadOnlyList<string> Palette
    {
        get => _palette;
        set => SetProperty(ref _palette, value);
    }

    public string GroupName
    {
        get => _groupName;
        set => SetProperty(ref _groupName, value);
    }

    public IRelayCommand<string?> SelectColorCommand { get; }

    public event EventHandler? ColorSelected;

    public HexColorPickerDialogViewModel()
    {
        SelectColorCommand = new RelayCommand<string?>(SelectColor);
    }

    private void SelectColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return;
        }

        SelectedColorHex = hex.Trim();
        ColorSelected?.Invoke(this, EventArgs.Empty);
    }
}
