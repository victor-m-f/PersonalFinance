using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PersonalFinance.Ui.Shared.Components;

public partial class PagingFooterControl : UserControl
{
    public static readonly DependencyProperty ShowingTextProperty = DependencyProperty.Register(
        nameof(ShowingText),
        typeof(string),
        typeof(PagingFooterControl),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty PageTextProperty = DependencyProperty.Register(
        nameof(PageText),
        typeof(string),
        typeof(PagingFooterControl),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty PrevCommandProperty = DependencyProperty.Register(
        nameof(PrevCommand),
        typeof(ICommand),
        typeof(PagingFooterControl));

    public static readonly DependencyProperty NextCommandProperty = DependencyProperty.Register(
        nameof(NextCommand),
        typeof(ICommand),
        typeof(PagingFooterControl));

    public PagingFooterControl()
    {
        InitializeComponent();
    }

    public string ShowingText
    {
        get => (string)GetValue(ShowingTextProperty);
        set => SetValue(ShowingTextProperty, value);
    }

    public string PageText
    {
        get => (string)GetValue(PageTextProperty);
        set => SetValue(PageTextProperty, value);
    }

    public ICommand? PrevCommand
    {
        get => (ICommand?)GetValue(PrevCommandProperty);
        set => SetValue(PrevCommandProperty, value);
    }

    public ICommand? NextCommand
    {
        get => (ICommand?)GetValue(NextCommandProperty);
        set => SetValue(NextCommandProperty, value);
    }
}
