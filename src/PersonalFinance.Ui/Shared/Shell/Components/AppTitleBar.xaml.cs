using System.Windows;
using System.Windows.Controls;

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

    public static readonly RoutedEvent BackRequestedEvent = EventManager.RegisterRoutedEvent(
        nameof(BackRequested),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(AppTitleBar)
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

    public event RoutedEventHandler BackRequested
    {
        add => AddHandler(BackRequestedEvent, value);
        remove => RemoveHandler(BackRequestedEvent, value);
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(BackRequestedEvent));
    }
}