using System.Windows;
using System.Windows.Controls;

namespace PersonalFinance.Ui.Shared.Controls;

public sealed class HexagonWrapPanel : Panel
{
    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
        nameof(ItemWidth),
        typeof(double),
        typeof(HexagonWrapPanel),
        new FrameworkPropertyMetadata(28d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
        nameof(ItemHeight),
        typeof(double),
        typeof(HexagonWrapPanel),
        new FrameworkPropertyMetadata(28d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(
        nameof(HorizontalSpacing),
        typeof(double),
        typeof(HexagonWrapPanel),
        new FrameworkPropertyMetadata(6d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(
        nameof(VerticalSpacing),
        typeof(double),
        typeof(HexagonWrapPanel),
        new FrameworkPropertyMetadata(2d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public double HorizontalSpacing
    {
        get => (double)GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var itemSize = new Size(ItemWidth, ItemHeight);

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(itemSize);
        }

        var columns = CalculateColumns(availableSize.Width);
        var rows = columns == 0 ? 0 : (int)Math.Ceiling((double)InternalChildren.Count / columns);

        var rowHeight = GetRowHeight();
        var height = rows <= 0 ? 0 : ItemHeight + (rows - 1) * rowHeight;
        var width = columns <= 0 ? 0 : (columns * ItemWidth) + ((columns - 1) * HorizontalSpacing) + (ItemWidth + HorizontalSpacing) / 2;

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = CalculateColumns(finalSize.Width);
        if (columns == 0)
        {
            return finalSize;
        }

        var rowHeight = GetRowHeight();
        var column = 0;
        var row = 0;

        foreach (UIElement child in InternalChildren)
        {
            if (column >= columns)
            {
                column = 0;
                row++;
            }

            var offset = row % 2 == 1 ? (ItemWidth + HorizontalSpacing) / 2 : 0;
            var x = (column * (ItemWidth + HorizontalSpacing)) + offset;
            var y = row * rowHeight;

            child.Arrange(new Rect(new Point(x, y), new Size(ItemWidth, ItemHeight)));
            column++;
        }

        return finalSize;
    }

    private int CalculateColumns(double availableWidth)
    {
        if (double.IsInfinity(availableWidth) || availableWidth <= 0)
        {
            return InternalChildren.Count == 0 ? 0 : InternalChildren.Count;
        }

        var columnWidth = ItemWidth + HorizontalSpacing;
        if (columnWidth <= 0)
        {
            return 1;
        }

        return Math.Max(1, (int)Math.Floor((availableWidth + HorizontalSpacing) / columnWidth));
    }

    private double GetRowHeight()
    {
        return (ItemHeight * 0.75) + VerticalSpacing;
    }
}
