using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Shared.Controls;

public sealed class HexColorPickerDialog : ContentDialog
{
    public HexColorPickerDialog(HexColorPickerDialogViewModel viewModel)
    {
        DataContext = viewModel;
        Title = System.Windows.Application.Current.TryFindResource("Categories.ColorPickerTitle") as string ?? "Choose color";
        PrimaryButtonText = System.Windows.Application.Current.TryFindResource("Common.Save") as string ?? "Save";
        CloseButtonText = System.Windows.Application.Current.TryFindResource("Common.Cancel") as string ?? "Cancel";
        DefaultButton = ContentDialogButton.Primary;
        DialogWidth = 640;
        DialogMaxWidth = 640;
        Content = BuildContent(viewModel);

        viewModel.ColorSelected += (_, _) => Hide(ContentDialogResult.Primary);
    }

    private UIElement BuildContent(HexColorPickerDialogViewModel viewModel)
    {
        var card = new Card
        {
            Padding = new Thickness(12),
            BorderThickness = new Thickness(1)
        };

        var stack = new StackPanel();
        card.Content = stack;

        var colorCount = HexColorPicker.GetDefaultColorCount();
        var colors = viewModel.Palette.Take(colorCount).ToList();
        var grays = viewModel.Palette.Skip(colorCount).ToList();

        var itemsControl = new ItemsControl
        {
            ItemsSource = colors
        };

        var panelFactory = new FrameworkElementFactory(typeof(HexagonHoneycombPanel));
        panelFactory.SetValue(HexagonHoneycombPanel.ItemWidthProperty, 28d);
        panelFactory.SetValue(HexagonHoneycombPanel.ItemHeightProperty, 28d);
        panelFactory.SetValue(HexagonHoneycombPanel.HorizontalSpacingProperty, 6d);
        panelFactory.SetValue(HexagonHoneycombPanel.VerticalSpacingProperty, 2d);
        panelFactory.SetValue(HexagonHoneycombPanel.RowLengthsProperty, string.Join(',', HexColorPicker.DefaultColorRowLengths));
        itemsControl.ItemsPanel = new ItemsPanelTemplate(panelFactory);

        var itemFactory = new FrameworkElementFactory(typeof(RadioButton));
        itemFactory.SetValue(FrameworkElement.StyleProperty, System.Windows.Application.Current.FindResource("HexColorRadioStyle"));

        var backgroundBinding = new Binding
        {
            Converter = (IValueConverter)System.Windows.Application.Current.FindResource("ColorHexToBrushConverter")
        };
        itemFactory.SetBinding(Control.BackgroundProperty, backgroundBinding);

        var groupBinding = new Binding(nameof(HexColorPickerDialogViewModel.GroupName))
        {
            Source = viewModel
        };
        itemFactory.SetBinding(RadioButton.GroupNameProperty, groupBinding);

        var commandBinding = new Binding(nameof(HexColorPickerDialogViewModel.SelectColorCommand))
        {
            Source = viewModel
        };
        itemFactory.SetBinding(System.Windows.Controls.Button.CommandProperty, commandBinding);
        itemFactory.SetBinding(System.Windows.Controls.Button.CommandParameterProperty, new Binding());

        var isCheckedMulti = new MultiBinding
        {
            Converter = (IMultiValueConverter)System.Windows.Application.Current.FindResource("ColorHexEqualsConverter"),
            Mode = BindingMode.OneWay
        };
        isCheckedMulti.Bindings.Add(new Binding(nameof(HexColorPickerDialogViewModel.SelectedColorHex)) { Source = viewModel });
        isCheckedMulti.Bindings.Add(new Binding());
        itemFactory.SetBinding(ToggleButton.IsCheckedProperty, isCheckedMulti);

        itemsControl.ItemTemplate = new DataTemplate { VisualTree = itemFactory };
        stack.Children.Add(itemsControl);

        if (grays.Count > 0)
        {
            var grayItems = new ItemsControl
            {
                ItemsSource = grays,
                Margin = new Thickness(0, 6, 0, 0)
            };

            var grayPanelFactory = new FrameworkElementFactory(typeof(HexagonWrapPanel));
            grayPanelFactory.SetValue(HexagonWrapPanel.ItemWidthProperty, 28d);
            grayPanelFactory.SetValue(HexagonWrapPanel.ItemHeightProperty, 28d);
            grayPanelFactory.SetValue(HexagonWrapPanel.HorizontalSpacingProperty, 6d);
            grayPanelFactory.SetValue(HexagonWrapPanel.VerticalSpacingProperty, 2d);
            grayItems.ItemsPanel = new ItemsPanelTemplate(grayPanelFactory);
            grayItems.ItemTemplate = new DataTemplate { VisualTree = itemFactory };
            stack.Children.Add(grayItems);
        }

        var preview = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 8, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        var rectangle = new Rectangle
        {
            Width = 18,
            Height = 18,
            RadiusX = 4,
            RadiusY = 4
        };
        rectangle.SetBinding(Shape.FillProperty, new Binding(nameof(HexColorPickerDialogViewModel.SelectedColorHex))
        {
            Source = viewModel,
            Converter = (IValueConverter)System.Windows.Application.Current.FindResource("ColorHexToBrushConverter")
        });

        var text = new System.Windows.Controls.TextBlock
        {
            Margin = new Thickness(8, 0, 0, 0)
        };
        text.SetBinding(System.Windows.Controls.TextBlock.TextProperty, new Binding(nameof(HexColorPickerDialogViewModel.SelectedColorHex)) { Source = viewModel });
        text.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "TextFillColorSecondaryBrush");

        preview.Children.Add(rectangle);
        preview.Children.Add(text);
        stack.Children.Add(preview);

        return card;
    }
}
