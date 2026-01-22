using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PersonalFinance.Ui.Shared.Shell.Components;

public partial class AppTitleBar : UserControl
{
    public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register(
        nameof(TitleText),
        typeof(string),
        typeof(AppTitleBar),
        new PropertyMetadata("PersonalFinance")
    );

    public static readonly DependencyProperty IsBackEnabledProperty = DependencyProperty.Register(
        nameof(IsBackEnabled),
        typeof(bool),
        typeof(AppTitleBar),
        new PropertyMetadata(false)
    );

    public static readonly DependencyProperty BackCommandProperty = DependencyProperty.Register(
        nameof(BackCommand),
        typeof(ICommand),
        typeof(AppTitleBar),
        new PropertyMetadata(null)
    );

    public AppTitleBar()
    {
        InitializeComponent();
    }

    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    public bool IsBackEnabled
    {
        get => (bool)GetValue(IsBackEnabledProperty);
        set => SetValue(IsBackEnabledProperty, value);
    }

    public ICommand? BackCommand
    {
        get => (ICommand?)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }
}