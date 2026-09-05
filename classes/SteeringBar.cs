using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace ControllerMonitor;

public class SteeringBar : Control
{
    private int value;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get => value;
        set { this.value = Math.Clamp(value, -100, 100); Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BarColor { get; set; } = Color.DodgerBlue;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BarBackgroundColor { get; set; } = Color.FromArgb(50, 50, 50);

    public SteeringBar()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        Height = 30;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int w = ClientSize.Width;
        int h = ClientSize.Height;
        int center = w / 2;

        // Background
        using var bg = new SolidBrush(this.BarBackgroundColor);
        g.FillRectangle(bg, 0, 0, w, h);

        // Filled section
        int fill = (int)(Math.Abs(Value) / 100f * center);
        int x = Value < 0 ? center - fill : center;

        using var bar = new SolidBrush(this.BarColor);
        g.FillRectangle(bar, x, 0, fill, h);

        // Center line
        using var line = new SolidBrush(Color.White);
        g.FillRectangle(line, center - 1, 0, 2, h);
    }
}
