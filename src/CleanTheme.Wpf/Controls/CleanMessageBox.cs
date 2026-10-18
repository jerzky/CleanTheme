using System.Windows;
using System.Windows.Controls;

namespace CleanTheme.Wpf.Controls;

/// <summary>Ready-made <see cref="CleanDialog"/>s: messages, confirmations and a single-line text prompt.</summary>
public static class CleanMessageBox
{
    /// <summary>Shows a message with an OK button.</summary>
    public static void Show(Window? owner, string message, string title, string? heading = null) =>
        Create(owner, message, title, heading, "OK", null, isDanger: false).ShowModal();

    /// <summary>Asks a question; true when the confirm button was pressed.</summary>
    public static bool Confirm(Window? owner, string message, string title, string confirmText = "OK", string cancelText = "Cancel",
        string? heading = null, bool isDanger = false) =>
        Create(owner, message, title, heading, confirmText, cancelText, isDanger).ShowModal() == CleanDialogResult.Primary;

    /// <summary>Asks for a line of text (trimmed); null when cancelled. Confirming blank text does nothing.</summary>
    public static string? Prompt(Window? owner, string title, string label, string initialText = "", string confirmText = "OK")
    {
        var dialog = CreatePrompt(owner, title, label, initialText, confirmText);
        return dialog.ShowModal() == CleanDialogResult.Primary ? dialog.Text.Trim() : null;
    }

    /// <summary>The dialog <see cref="Confirm"/> and <see cref="Show"/> display, for callers that customize it further.</summary>
    public static CleanDialog Create(Window? owner, string message, string title, string? heading, string primaryText,
        string? secondaryText, bool isDanger) => new()
        {
            Owner = owner,
            Title = title,
            Heading = heading,
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = primaryText,
            SecondaryButtonText = secondaryText,
            IsDanger = isDanger,
        };

    /// <summary>The dialog <see cref="Prompt"/> displays.</summary>
    public static PromptDialog CreatePrompt(Window? owner, string title, string label, string initialText = "", string confirmText = "OK") =>
        new(label, initialText)
        {
            Owner = owner,
            Title = title,
            PrimaryButtonText = confirmText,
            SecondaryButtonText = "Cancel",
        };
}

/// <summary>A <see cref="CleanDialog"/> asking for one line of text.</summary>
public sealed class PromptDialog : CleanDialog
{
    private readonly TextBox _textBox;

    public PromptDialog(string label, string initialText)
    {
        _textBox = new TextBox { Text = initialText };
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 6), TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(_textBox);
        Content = panel;
        Loaded += (_, _) =>
        {
            _textBox.Focus();
            _textBox.SelectAll();
        };
    }

    /// <summary>The text as typed.</summary>
    public string Text
    {
        get => _textBox.Text;
        set => _textBox.Text = value;
    }

    protected override bool OnPrimary() => !string.IsNullOrWhiteSpace(Text);
}
