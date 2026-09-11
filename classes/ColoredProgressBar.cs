using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ControllerMonitor;

public class ColoredProgressBar : Control
{
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<ColoredProgressBar, int>(
            nameof(Value),
            defaultValue: 0);

    public static readonly StyledProperty<Color> BarColorProperty =
        AvaloniaProperty.Register<ColoredProgressBar, Color>(
            nameof(BarColor),
            defaultValue: Colors.LimeGreen);

    public static readonly StyledProperty<Color> BarBackgroundColorProperty =
        AvaloniaProperty.Register<ColoredProgressBar, Color>(
            nameof(BarBackgroundColor),
            defaultValue: Color.FromRgb(50, 50, 50));

    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, Math.Clamp(value, 0, 100));
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

    public ColoredProgressBar()
    {
        Height = 25;
        
        AffectsRender<ColoredProgressBar>(ValueProperty, BarColorProperty, BarBackgroundColorProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double width = Bounds.Width;
        double height = Bounds.Height;

        context.FillRectangle(
            new SolidColorBrush(BarBackgroundColor),
            new Rect(0, 0, width, height));

        double fill = width * (Value / 100.0);

        context.FillRectangle(
            new SolidColorBrush(BarColor),
            new Rect(0, 0, fill, height));
    }
}