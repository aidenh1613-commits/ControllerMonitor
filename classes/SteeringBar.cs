using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ControllerMonitor;

public class SteeringBar : Control
{
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<SteeringBar, int>(
            nameof(Value),
            defaultValue: 0);

    public static readonly StyledProperty<Color> BarColorProperty =
        AvaloniaProperty.Register<SteeringBar, Color>(
            nameof(BarColor),
            defaultValue: Colors.DodgerBlue);

    public static readonly StyledProperty<Color> BarBackgroundColorProperty =
        AvaloniaProperty.Register<SteeringBar, Color>(
            nameof(BarBackgroundColor),
            defaultValue: Color.FromRgb(50, 50, 50));

    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, Math.Clamp(value, -100, 100));
    }

    public Color BarColor
    {
        get => GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }

    public Color BarBackgroundColor
    {
        get => GetValue(BarBackgroundColorProperty);
        set => SetValue(BarBackgroundColorProperty, value);
    }

    public SteeringBar()
    {
        Height = 30;

        AffectsRender<SteeringBar>(ValueProperty, BarColorProperty, BarBackgroundColorProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double width = Bounds.Width;
        double height = Bounds.Height;
        double center = width / 2;

        // Background
        context.FillRectangle(
            new SolidColorBrush(BarBackgroundColor),
            new Rect(0, 0, width, height));

        // Filled section
        double fill = Math.Abs(Value) / 100.0 * center;
        double x = Value < 0 ? center - fill : center;

        context.FillRectangle(
            new SolidColorBrush(BarColor),
            new Rect(x, 0, fill, height));

        // Center line
        context.FillRectangle(
            Brushes.White,
            new Rect(center - 1, 0, 2, height));
    }
}