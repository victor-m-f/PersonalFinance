using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace PersonalFinance.Ui.Shared.Controls;

public sealed class HexagonHoneycombPanel : Panel
{
    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
        nameof(ItemWidth),
        typeof(double),
        typeof(HexagonHoneycombPanel),
        new FrameworkPropertyMetadata(28d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
        nameof(ItemHeight),
        typeof(double),
        typeof(HexagonHoneycombPanel),
        new FrameworkPropertyMetadata(28d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(
        nameof(HorizontalSpacing),
        typeof(double),
        typeof(HexagonHoneycombPanel),
        new FrameworkPropertyMetadata(6d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(
        nameof(VerticalSpacing),
        typeof(double),
        typeof(HexagonHoneycombPanel),
        new FrameworkPropertyMetadata(2d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty RowLengthsProperty = DependencyProperty.Register(
        nameof(RowLengths),
        typeof(string),
        typeof(HexagonHoneycombPanel),
        new FrameworkPropertyMetadata("6,7,8,9,8,7,6", FrameworkPropertyMetadataOptions.AffectsMeasure));

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

    public string RowLengths
    {
        get => (string)GetValue(RowLengthsProperty);
        set => SetValue(RowLengthsProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var itemSize = new Size(ItemWidth, ItemHeight);
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(itemSize);
        }

        var rows = ParseRowLengths();
        var maxColumns = rows.Count == 0 ? 0 : rows.Max();
        var rowHeight = GetRowHeight();

        var width = maxColumns == 0 ? 0 : (maxColumns * ItemWidth) + ((maxColumns - 1) * HorizontalSpacing);
        var height = rows.Count == 0 ? 0 : ItemHeight + (rows.Count - 1) * rowHeight;

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var rows = ParseRowLengths();
        if (rows.Count == 0)
        {
            return finalSize;
        }

        var maxColumns = rows.Max();
        var rowHeight = GetRowHeight();

        var index = 0;
        for (var row = 0; row < rows.Count; row++)
        {
            var columns = rows[row];
            var offset = (maxColumns - columns) * (ItemWidth + HorizontalSpacing) / 2;

            for (var col = 0; col < columns && index < InternalChildren.Count; col++)
            {
                var x = offset + col * (ItemWidth + HorizontalSpacing);
                var y = row * rowHeight;

                InternalChildren[index].Arrange(new Rect(new Point(x, y), new Size(ItemWidth, ItemHeight)));
                index++;
            }
        }

        return finalSize;
    }

    private double GetRowHeight()
    {
        return (ItemHeight * 0.75) + VerticalSpacing;
    }

    private List<int> ParseRowLengths()
    {
        var result = new List<int>();
        if (string.IsNullOrWhiteSpace(RowLengths))
        {
            return result;
        }

        var segments = RowLengths.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var segment in segments)
        {
            if (int.TryParse(segment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                result.Add(value);
            }
        }

        return result;
    }
}
