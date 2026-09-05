using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ControllerMonitor;

public class ColoredProgressBar : Control
{
    private int value;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get => value;
        set { this.value = Math.Clamp(value, 0, 100); Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BarColor { get; set; } = Color.LimeGreen;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BarBackgroundColor { get; set; } = Color.LightGray;

    public ColoredProgressBar()
    {
        DoubleBuffered = true;
        Height = 25;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int w = Width;
        int h = Height;

        using var bg = new SolidBrush(this.BarBackgroundColor);
        g.FillRectangle(bg, 0, 0, w, h);

        int fill = (int)(w * (Value / 100f));

        using var bar = new SolidBrush(this.BarColor);
        g.FillRectangle(bar, 0, 0, fill, h);
    }
}
