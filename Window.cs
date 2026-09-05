using SDL2;
namespace ControllerMonitor;

public partial class Window : Form
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private static readonly nint HWND_TOPMOST = new(-1);
    private const uint SWP_FLAGS = 0x0045; // NOSIZE + NOMOVE + SHOWWINDOW

    private readonly Label brakeLabel = new() { Font = new("Segoe UI", 18), ForeColor = Color.LightGray, AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly Label throttleLabel = new() { Font = new("Segoe UI", 18), ForeColor = Color.LightGray, AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly Label steeringLabel = new() { Font = new("Segoe UI", 18), ForeColor = Color.LightGray, AutoSize = true, Anchor = AnchorStyles.Left };

    private readonly ColoredProgressBar brakeBar = new() { Dock = DockStyle.Fill, Margin = new(0, 0, 0, 5), BarColor = Color.Red, BarBackgroundColor = Color.FromArgb(50, 50, 50) };
    private readonly ColoredProgressBar throttleBar = new() { Dock = DockStyle.Fill, Margin = new(0, 0, 0, 5), BarColor = Color.LimeGreen, BarBackgroundColor = Color.FromArgb(50, 50, 50) };
    private readonly ColoredProgressBar steeringBar = new() { Dock = DockStyle.Fill, Margin = new(0, 0, 0, 5), BarColor = Color.DodgerBlue, BarBackgroundColor = Color.FromArgb(50, 50, 50) };

    private readonly nint controller;
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 10 };

    public Window()
    {
        InitializeComponent();

        // Window
        TopMost = true;
        BackColor = TransparencyKey = Color.Black;
        Text = "Controller Monitor";
        MinimumSize = new(300, 250);
        Width = 400; Height = 320;

        // Layout
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new(30, 20, 30, 20),
            BackColor = Color.Black
        };
        for (int i = 0; i < 6; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.6667f));

        this.brakeLabel.Text = "Brake: 0%";
        this.throttleLabel.Text = "Throttle: 0%";
        this.steeringLabel.Text = "Steering: 0%";

        layout.Controls.Add(this.brakeLabel, 0, 0);
        layout.Controls.Add(this.brakeBar, 0, 1);
        layout.Controls.Add(this.throttleLabel, 0, 2);
        layout.Controls.Add(this.throttleBar, 0, 3);
        layout.Controls.Add(this.steeringLabel, 0, 4);
        layout.Controls.Add(this.steeringBar, 0, 5);
        Controls.Add(layout);

        // SDL
        if (SDL.SDL_Init(SDL.SDL_INIT_GAMECONTROLLER) < 0)
        {
            MessageBox.Show(SDL.SDL_GetError());
            return;
        }

        for (int i = 0; i < SDL.SDL_NumJoysticks(); i++)
            if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_TRUE)
            {
                this.controller = SDL.SDL_GameControllerOpen(i);
                break;
            }

        if (this.controller == 0)
        {
            MessageBox.Show("No compatible controller found.");
            return;
        }

        this.timer.Tick += ReadController;
        this.timer.Start();
    }

    private void ReadController(object? sender, EventArgs e)
    {
        SDL.SDL_GameControllerUpdate();

        double brake = SDL.SDL_GameControllerGetAxis(this.controller, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT) / 32767.0 * 100;
        double throttle = SDL.SDL_GameControllerGetAxis(this.controller, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT) / 32767.0 * 100;

        this.brakeLabel.Text = $"Brake: {brake:F0}%";
        this.throttleLabel.Text = $"Throttle: {throttle:F0}%";
        this.brakeBar.Value = (int)Math.Round(brake);
        this.throttleBar.Value = (int)Math.Round(throttle);

        int steer = (int)Math.Round(SDL.SDL_GameControllerGetAxis(this.controller, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX) / 32767.0 * 100);
        if (Math.Abs(steer) <= 5) steer = 0;

        this.steeringLabel.Text = $"Steering: {steer}%";
        this.steeringBar.Value = steer;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer.Stop();
        if (this.controller != 0) SDL.SDL_GameControllerClose(this.controller);
        SDL.SDL_Quit();
        base.OnFormClosed(e);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        SetWindowPos(this.Handle, HWND_TOPMOST, Left, Top, Width, Height, SWP_FLAGS);
    }

}
