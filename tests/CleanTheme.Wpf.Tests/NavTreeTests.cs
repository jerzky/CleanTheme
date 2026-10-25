using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using CleanTheme.Wpf.Controls;
using Xunit;

namespace CleanTheme.Wpf.Tests;

public sealed class NavTreeTests
{
    [Fact]
    public void NavTreeItem_IsSubItem_is_true_when_nested_in_another_NavTreeItem() => Sta.Run(() =>
    {
        var tree = new NavTree();
        var parentItem = new NavTreeItem { Header = "Parent Chat" };
        var childItem = new NavTreeItem { Header = "Sub Chat" };

        parentItem.Items.Add(childItem);
        tree.Items.Add(parentItem);

        Sta.Layout(tree);

        Assert.False(parentItem.IsSubItem);
        Assert.True(childItem.IsSubItem);
        Assert.True(parentItem.HasItems);
        Assert.False(childItem.HasItems);
    });

    [Fact]
    public void NavTreeItem_selecting_child_item_updates_NavTree_SelectedItem() => Sta.Run(() =>
    {
        var tree = new NavTree();
        var parentItem = new NavTreeItem { Header = "Parent Chat" };
        var child1 = new NavTreeItem { Header = "Sub Chat 1" };
        var child2 = new NavTreeItem { Header = "Sub Chat 2" };

        parentItem.Items.Add(child1);
        parentItem.Items.Add(child2);
        tree.Items.Add(parentItem);

        Sta.Layout(tree);

        object? changedTo = null;
        tree.SelectedItemChanged += (s, e) => changedTo = e.NewValue;

        child1.SelectThis();
        Sta.Layout(tree);

        Assert.True(child1.IsSelected);
        Assert.False(child2.IsSelected);
        Assert.False(parentItem.IsSelected);
        Assert.Same(child1, tree.SelectedItem);
        Assert.Same(child1, changedTo);

        // Select child 2
        child2.SelectThis();
        Sta.Layout(tree);

        Assert.False(child1.IsSelected);
        Assert.True(child2.IsSelected);
        Assert.Same(child2, tree.SelectedItem);
    });

    [Fact]
    public void NavTreeItem_expand_and_collapse_events_and_property() => Sta.Run(() =>
    {
        var item = new NavTreeItem { Header = "Chat 1" };
        item.Items.Add(new NavTreeItem { Header = "Sub Chat" });

        bool expandedFired = false;
        bool collapsedFired = false;
        item.Expanded += (s, e) => expandedFired = true;
        item.Collapsed += (s, e) => collapsedFired = true;

        Assert.True(item.IsExpanded);

        item.IsExpanded = false;
        Assert.True(collapsedFired);
        Assert.False(item.IsExpanded);

        item.IsExpanded = true;
        Assert.True(expandedFired);
        Assert.True(item.IsExpanded);
    });

    [Fact]
    public void NavTreeItem_keyboard_arrows_expand_and_collapse() => Sta.Run(() =>
    {
        var item = new NavTreeItem { Header = "Chat 1", IsExpanded = true };
        item.Items.Add(new NavTreeItem { Header = "Sub Chat" });
        Sta.Layout(item);

        using (var source = new System.Windows.Interop.HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero))
        {
            // Left arrow collapses
            item.RaiseEvent(new KeyEventArgs(
                Keyboard.PrimaryDevice, source, 0, Key.Left)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            });
            Assert.False(item.IsExpanded);

            // Right arrow expands
            item.RaiseEvent(new KeyEventArgs(
                Keyboard.PrimaryDevice, source, 0, Key.Right)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            });
            Assert.True(item.IsExpanded);
        }
    });

    [Fact]
    public void NavTreeItemAutomationPeer_supports_TreeItem_ExpandCollapse_and_Selection() => Sta.Run(() =>
    {
        var tree = new NavTree();
        var parentItem = new NavTreeItem { Header = "Chat 1" };
        var childItem = new NavTreeItem { Header = "Sub Chat 1" };
        parentItem.Items.Add(childItem);
        tree.Items.Add(parentItem);
        Sta.Layout(tree);

        var peer = new NavTreeItemAutomationPeer(parentItem);
        Assert.Equal(AutomationControlType.TreeItem, peer.GetAutomationControlType());
        Assert.Equal("Chat 1", peer.GetName());

        var expandCollapse = peer.GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
        Assert.NotNull(expandCollapse);
        Assert.Equal(System.Windows.Automation.ExpandCollapseState.Expanded, expandCollapse.ExpandCollapseState);

        expandCollapse.Collapse();
        Assert.False(parentItem.IsExpanded);

        expandCollapse.Expand();
        Assert.True(parentItem.IsExpanded);

        var selection = peer.GetPattern(PatternInterface.SelectionItem) as ISelectionItemProvider;
        Assert.NotNull(selection);
        selection.Select();
        Assert.True(parentItem.IsSelected);
    });
}
