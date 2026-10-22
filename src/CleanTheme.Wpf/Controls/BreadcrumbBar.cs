using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>Displays a horizontal trail of breadcrumbs representing navigation hierarchy.</summary>
public class BreadcrumbBar : ItemsControl
{
    static BreadcrumbBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbBar), new FrameworkPropertyMetadata(typeof(BreadcrumbBar)));
    }

    public BreadcrumbBar()
    {
        ((INotifyCollectionChanged)Items).CollectionChanged += (_, _) => UpdateLastItemState();
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is BreadcrumbItem;

    protected override DependencyObject GetContainerForItemOverride() => new BreadcrumbItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is BreadcrumbItem breadcrumbItem && item is not BreadcrumbItem)
        {
            breadcrumbItem.Header = item;
        }
        UpdateLastItemState();
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateLastItemState();
    }

    private void UpdateLastItemState()
    {
        int count = Items.Count;
        for (int i = 0; i < count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is BreadcrumbItem container)
            {
                // Last item is marked as current by default
                bool isLast = (i == count - 1);
                container.IsCurrent = isLast;
            }
            else if (Items[i] is BreadcrumbItem directItem)
            {
                bool isLast = (i == count - 1);
                directItem.IsCurrent = isLast;
            }
        }
    }
}
