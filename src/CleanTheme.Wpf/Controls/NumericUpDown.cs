using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CleanTheme.Wpf.Controls;

/// <summary>
/// A number box with up/down buttons. <see cref="Value"/> (two-way by default) stays within
/// [<see cref="Minimum"/>, <see cref="Maximum"/>] and is rounded to <see cref="DecimalPlaces"/>. Typed text commits on
/// Enter or when focus leaves; invalid text reverts. Up/Down keys and the mouse wheel step by <see cref="Increment"/>.
/// </summary>
[TemplatePart(Name = TextBoxPart, Type = typeof(TextBox))]
[TemplatePart(Name = IncreasePart, Type = typeof(ButtonBase))]
[TemplatePart(Name = DecreasePart, Type = typeof(ButtonBase))]
public sealed class NumericUpDown : Control
{
    private const string TextBoxPart = "PART_TextBox";
    private const string IncreasePart = "PART_IncreaseButton";
    private const string DecreasePart = "PART_DecreaseButton";

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(NumericUpDown),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged,
            (d, value) => ((NumericUpDown)d).Coerce((double)value)));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(double.MinValue, OnRangeChanged));

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(double.MaxValue, OnRangeChanged));

    public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
        nameof(Increment), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(1.0));

    public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
        nameof(DecimalPlaces), typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(0, OnRangeChanged), IsValidDecimalPlaces);

    private TextBox? _textBox;
    private ButtonBase? _increase;
    private ButtonBase? _decrease;

    static NumericUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Step for the buttons, arrow keys and mouse wheel. 1 by default.</summary>
    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    /// <summary>Digits after the decimal point, 0–15. 0 by default.</summary>
    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    /// <summary>Adds <see cref="Increment"/> (clamped).</summary>
    public void StepUp() => Value += Increment;

    /// <summary>Subtracts <see cref="Increment"/> (clamped).</summary>
    public void StepDown() => Value -= Increment;

    public override void OnApplyTemplate()
    {
        if (_textBox != null)
        {
            _textBox.LostKeyboardFocus -= OnTextBoxLostFocus;
            _textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
        }
        if (_increase != null)
        {
            _increase.Click -= OnIncreaseClick;
        }
        if (_decrease != null)
        {
            _decrease.Click -= OnDecreaseClick;
        }

        base.OnApplyTemplate();

        _textBox = GetTemplateChild(TextBoxPart) as TextBox;
        _increase = GetTemplateChild(IncreasePart) as ButtonBase;
        _decrease = GetTemplateChild(DecreasePart) as ButtonBase;
        if (_textBox != null)
        {
            _textBox.LostKeyboardFocus += OnTextBoxLostFocus;
            _textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
        }
        if (_increase != null)
        {
            _increase.Click += OnIncreaseClick;
        }
        if (_decrease != null)
        {
            _decrease.Click += OnDecreaseClick;
        }
        ShowValue();
    }

    /// <summary>Parses <paramref name="text"/> (current culture, then invariant) into <see cref="Value"/>; invalid text reverts.</summary>
    internal void CommitText(string text)
    {
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var parsed)
            || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
        {
            Value = parsed;
        }
        ShowValue(); // Shows the coerced value, or reverts invalid text.
    }

    /// <summary>The value as shown in the box.</summary>
    internal string FormattedValue => Value.ToString("F" + DecimalPlaces.ToString(CultureInfo.InvariantCulture), CultureInfo.CurrentCulture);

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (IsKeyboardFocusWithin)
        {
            if (e.Delta > 0)
            {
                StepUp();
            }
            else
            {
                StepDown();
            }
            e.Handled = true;
        }
        base.OnMouseWheel(e);
    }

    private static bool IsValidDecimalPlaces(object value) => value is int places && places is >= 0 and <= 15;

    /// <summary>Clamps to the range and rounds to <see cref="DecimalPlaces"/>; NaN keeps the current value.</summary>
    private double Coerce(double value) => double.IsNaN(value)
        ? Value
        : Math.Round(Math.Clamp(value, Minimum, Math.Max(Minimum, Maximum)), DecimalPlaces, MidpointRounding.AwayFromZero);

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((NumericUpDown)d).ShowValue();

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (NumericUpDown)d;
        box.CoerceValue(ValueProperty);
        box.ShowValue();
    }

    private void ShowValue()
    {
        if (_textBox != null)
        {
            _textBox.Text = FormattedValue;
        }
    }

    private void OnTextBoxLostFocus(object sender, KeyboardFocusChangedEventArgs e) => CommitText(((TextBox)sender).Text);

    private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                CommitText(((TextBox)sender).Text);
                ((TextBox)sender).SelectAll();
                e.Handled = true;
                break;
            case Key.Up:
                CommitText(((TextBox)sender).Text);
                StepUp();
                e.Handled = true;
                break;
            case Key.Down:
                CommitText(((TextBox)sender).Text);
                StepDown();
                e.Handled = true;
                break;
        }
    }

    private void OnIncreaseClick(object sender, RoutedEventArgs e) => StepUp();

    private void OnDecreaseClick(object sender, RoutedEventArgs e) => StepDown();
}
