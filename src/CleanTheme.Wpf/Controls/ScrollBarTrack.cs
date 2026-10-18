using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A scroll bar <see cref="Track"/> whose thumb never gets shorter than <see cref="MinThumbLength"/>. The stock track
/// sizes the thumb purely by viewport/extent, which shrinks it to a few pixels over long content.
/// </summary>
public sealed class ScrollBarTrack : Track
{
    public static readonly DependencyProperty MinThumbLengthProperty = DependencyProperty.Register(
        nameof(MinThumbLength), typeof(double), typeof(ScrollBarTrack),
        new FrameworkPropertyMetadata(28.0, FrameworkPropertyMetadataOptions.AffectsArrange));

    // Value per pixel of thumb travel while the thumb is enlarged; NaN when the stock layout is in effect.
    private double _density = double.NaN;

    /// <summary>Shortest thumb layout slot (including the thumb's margin), in pixels. 28 by default.</summary>
    public double MinThumbLength
    {
        get => (double)GetValue(MinThumbLengthProperty);
        set => SetValue(MinThumbLengthProperty, value);
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var result = base.ArrangeOverride(arrangeSize);
        _density = double.NaN;

        var vertical = Orientation == Orientation.Vertical;
        var trackLength = vertical ? arrangeSize.Height : arrangeSize.Width;
        var range = Maximum - Minimum;
        if (Thumb is null || double.IsNaN(ViewportSize) || range <= 0 || trackLength <= MinThumbLength)
        {
            return result; // Slider-style track, nothing to scroll, or no room: keep the stock layout.
        }

        var slot = LayoutInformation.GetLayoutSlot(Thumb);
        var thumbLength = vertical ? slot.Height : slot.Width;
        if (thumbLength <= 0 || thumbLength >= MinThumbLength)
        {
            return result;
        }

        var travel = trackLength - MinThumbLength;
        var ratio = Math.Clamp((Value - Minimum) / range, 0, 1);
        var decrease = travel * ratio;
        var increase = travel - decrease;

        // Vertical tracks put the decrease button at the bottom unless reversed; horizontal ones at the left unless reversed.
        var decreaseFirst = vertical ? IsDirectionReversed : !IsDirectionReversed;
        var (first, firstLength, last, lastLength) = decreaseFirst
            ? (DecreaseRepeatButton, decrease, IncreaseRepeatButton, increase)
            : (IncreaseRepeatButton, increase, DecreaseRepeatButton, decrease);

        var offset = 0.0;
        Place(first, ref offset, firstLength, vertical, arrangeSize);
        Place(Thumb, ref offset, MinThumbLength, vertical, arrangeSize);
        Place(last, ref offset, lastLength, vertical, arrangeSize);

        _density = range / travel;
        return result;
    }

    /// <summary>Converts a drag distance to a value change, matching the enlarged thumb so it stays under the pointer.</summary>
    public override double ValueFromDistance(double horizontal, double vertical)
    {
        if (double.IsNaN(_density))
        {
            return base.ValueFromDistance(horizontal, vertical);
        }
        var scale = IsDirectionReversed ? -1 : 1;
        return Orientation == Orientation.Horizontal
            ? scale * horizontal * _density
            : -1 * scale * vertical * _density;
    }

    private static void Place(UIElement? element, ref double offset, double length, bool vertical, Size arrangeSize)
    {
        var rect = vertical
            ? new Rect(0, offset, arrangeSize.Width, length)
            : new Rect(offset, 0, length, arrangeSize.Height);
        element?.Arrange(rect);
        offset += length;
    }
}
