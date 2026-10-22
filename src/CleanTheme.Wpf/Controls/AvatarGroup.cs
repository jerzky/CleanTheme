using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>Displays an overlapping group of <see cref="Avatar"/> controls with an optional +N overflow indicator.</summary>
public class AvatarGroup : ItemsControl
{
    private static readonly DependencyPropertyKey VisibleItemsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(VisibleItems), typeof(IList), typeof(AvatarGroup), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty VisibleItemsProperty = VisibleItemsPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey OverflowCountPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(OverflowCount), typeof(int), typeof(AvatarGroup), new FrameworkPropertyMetadata(0));

    public static readonly DependencyProperty OverflowCountProperty = OverflowCountPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey HasOverflowPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasOverflow), typeof(bool), typeof(AvatarGroup), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasOverflowProperty = HasOverflowPropertyKey.DependencyProperty;

    public static readonly DependencyProperty MaxDisplayProperty = DependencyProperty.Register(
        nameof(MaxDisplay), typeof(int), typeof(AvatarGroup),
        new FrameworkPropertyMetadata(4, (d, _) => ((AvatarGroup)d).UpdateVisibleItems()));

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

    public AvatarGroup()
    {
        SetValue(VisibleItemsPropertyKey, new ObservableCollection<object>());
        ((INotifyCollectionChanged)Items).CollectionChanged += OnItemsCollectionChanged;
    }

    public IList VisibleItems => (IList)GetValue(VisibleItemsProperty);

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

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        if (oldValue is INotifyCollectionChanged oldNotify)
        {
            oldNotify.CollectionChanged -= OnItemsSourceCollectionChanged;
        }
        if (newValue is INotifyCollectionChanged newNotify)
        {
            newNotify.CollectionChanged += OnItemsSourceCollectionChanged;
        }
        UpdateVisibleItems();
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateVisibleItems();

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateVisibleItems();

    private void UpdateVisibleItems()
    {
        var targetList = (ObservableCollection<object>)VisibleItems;
        targetList.Clear();

        var sourceList = new List<object>();
        if (ItemsSource != null)
        {
            foreach (var item in ItemsSource)
            {
                sourceList.Add(item);
            }
        }
        else
        {
            foreach (var item in Items)
            {
                sourceList.Add(item);
            }
        }

        int max = Math.Max(1, MaxDisplay);
        int total = sourceList.Count;

        if (total <= max)
        {
            foreach (var item in sourceList)
            {
                targetList.Add(item);
            }
            SetValue(OverflowCountPropertyKey, 0);
            SetValue(HasOverflowPropertyKey, false);
        }
        else
        {
            for (int i = 0; i < max; i++)
            {
                targetList.Add(sourceList[i]);
            }
            SetValue(OverflowCountPropertyKey, total - max);
            SetValue(HasOverflowPropertyKey, true);
        }
    }
}
