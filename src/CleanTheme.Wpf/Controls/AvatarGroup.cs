using System;
using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>Displays an overlapping group of <see cref="Avatar"/> controls with an optional +N overflow indicator.</summary>
public class AvatarGroup : ItemsControl
{
    private static readonly DependencyPropertyKey OverflowCountPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(OverflowCount), typeof(int), typeof(AvatarGroup), new FrameworkPropertyMetadata(0));

    public static readonly DependencyProperty OverflowCountProperty = OverflowCountPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey HasOverflowPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasOverflow), typeof(bool), typeof(AvatarGroup), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasOverflowProperty = HasOverflowPropertyKey.DependencyProperty;

    public static readonly DependencyProperty MaxDisplayProperty = DependencyProperty.Register(
        nameof(MaxDisplay), typeof(int), typeof(AvatarGroup),
        new FrameworkPropertyMetadata(4, (d, _) => ((AvatarGroup)d).UpdateContainers()));

    public static readonly DependencyProperty AvatarSizeProperty = DependencyProperty.Register(
        nameof(AvatarSize), typeof(double), typeof(AvatarGroup),
        new FrameworkPropertyMetadata(36.0));

    public static readonly DependencyProperty OverlapProperty = DependencyProperty.Register(
        nameof(Overlap), typeof(double), typeof(AvatarGroup),
        new FrameworkPropertyMetadata(-8.0));

    static AvatarGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AvatarGroup), new FrameworkPropertyMetadata(typeof(AvatarGroup)));
    }

    public int OverflowCount => (int)GetValue(OverflowCountProperty);

    public bool HasOverflow => (bool)GetValue(HasOverflowProperty);

    public int MaxDisplay
    {
        get => (int)GetValue(MaxDisplayProperty);
        set => SetValue(MaxDisplayProperty, value);
    }

    public double AvatarSize
    {
        get => (double)GetValue(AvatarSizeProperty);
        set => SetValue(AvatarSizeProperty, value);
    }

    public double Overlap
    {
        get => (double)GetValue(OverlapProperty);
        set => SetValue(OverlapProperty, value);
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is Avatar;

    protected override DependencyObject GetContainerForItemOverride() => new Avatar();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is Avatar avatar && item is not Avatar)
        {
            avatar.Initials = item?.ToString() ?? string.Empty;
        }

        if (element is FrameworkElement fe)
        {
            if (element is Avatar av)
            {
                av.AvatarSize = AvatarSize;
            }
        }

        UpdateContainers();
    }

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateContainers();
    }

    public void UpdateContainers()
    {
        int count = Items.Count;
        int max = Math.Max(1, MaxDisplay);

        for (int i = 0; i < count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement container)
            {
                container.Visibility = i < max ? Visibility.Visible : Visibility.Collapsed;
                container.Margin = (i == 0) ? new Thickness(0) : new Thickness(Overlap, 0, 0, 0);
            }
        }

        int overflow = Math.Max(0, count - max);
        SetValue(OverflowCountPropertyKey, overflow);
        SetValue(HasOverflowPropertyKey, overflow > 0);
    }
}
