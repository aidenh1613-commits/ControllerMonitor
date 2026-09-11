using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Threading;

using SDL2;

namespace ControllerMonitor;

public partial class Window : Avalonia.Controls.Window
{
    public string LanguageFile = "en_us";
    public string CurrentLanguageFile { get; private set; } = "";
    public bool MessageBeingShown { get; private set; } = false;
    public string NoCompatibleControllerMessage = "placeholder";
    private nint controller = nint.Zero;

    private readonly DispatcherTimer Timer_ReadController =
        new() { Interval = TimeSpan.FromMilliseconds(10) };

    private readonly DispatcherTimer Timer_ReloadLanguage =
        new() { Interval = TimeSpan.FromMilliseconds(250) };

    public Window()
    {
        InitializeComponent();

        // SDL
        if (SDL.SDL_Init(SDL.SDL_INIT_GAMECONTROLLER) < 0)
        {
            return;
        }

        // Enable controller hotplug events
        SDL.SDL_GameControllerEventState(SDL.SDL_ENABLE);

        // Try to find an initial controller
        this.FindController();

        this.Timer_ReadController.Tick += this.ReadController;
        this.Timer_ReadController.Start();

        this.Timer_ReloadLanguage.Tick += this.ReloadLanguage;
        this.Timer_ReloadLanguage.Start();

        this.Closed += this.Window_Closed;
    }

    private void FindController()
    {
        if (this.controller != nint.Zero)
            return;

        for (int i = 0; i < SDL.SDL_NumJoysticks(); i++)
        {
            if (SDL.SDL_IsGameController(i) != SDL.SDL_bool.SDL_TRUE)
                continue;

            nint Controller = SDL.SDL_GameControllerOpen(i);

            if (Controller != nint.Zero)
            {
                this.controller = Controller;
                return;
            }
        }
    }

    private async void ReadController(object? sender, EventArgs e)
    {
        SDL.SDL_GameControllerUpdate();
        if (this.controller == 0 || SDL.SDL_GameControllerGetAttached(this.controller) == SDL.SDL_bool.SDL_FALSE)
        {
            Environment.Exit(0);
            return;
        }

        if (this.controller == nint.Zero)
            return;

        double brake = SDL.SDL_GameControllerGetAxis(
            this.controller,
            SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT
        ) / 32767.0 * 100;

        double throttle = SDL.SDL_GameControllerGetAxis(
            this.controller,
            SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT
        ) / 32767.0 * 100;

        string[] brakeLabelParts = (this.BrakeLabel.Text ?? string.Empty).Split(" ");
        this.BrakeLabel.Text = $"{brakeLabelParts[0]} {brake:F0}%";

        string[] throttleLabelParts = (this.ThrottleLabel.Text ?? string.Empty).Split(" ");
        this.ThrottleLabel.Text = $"{throttleLabelParts[0]} {throttle:F0}%";

        this.BrakeBar.Value = (int)Math.Round(brake);
        this.ThrottleBar.Value = (int)Math.Round(throttle);

        int steer = (int)Math.Round(
            SDL.SDL_GameControllerGetAxis(
                this.controller,
                SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX
            ) / 32767.0 * 100
        );

        if (Math.Abs(steer) <= 5)
            steer = 0;

        string[] steeringLabelParts = (this.SteeringLabel.Text ?? string.Empty).Split(" ");
        this.SteeringLabel.Text = $"{steeringLabelParts[0]} {steer:F0}%";

        this.SteeringBar.Value = steer;
    }

    private void ReloadLanguage(object? sender, EventArgs e)
    {
        if (this.CurrentLanguageFile != this.LanguageFile)
        {
            string LanguagePath = Path.Combine("lang", $"{this.LanguageFile}.json");

            if (!File.Exists(LanguagePath))
                return;

            string Json = File.ReadAllText(LanguagePath);

            Dictionary<string, string>? Keys =
                JsonSerializer.Deserialize<Dictionary<string, string>>(Json);

            if (Keys is null)
                return;

            this.BrakeLabel.Text = this.GetValue(Keys, "app.label.inputs.l2", ": 0%");
            this.ThrottleLabel.Text = this.GetValue(Keys, "app.label.inputs.r2", ": 0%");
            this.SteeringLabel.Text = this.GetValue(Keys, "app.label.inputs.l", ": 0%");
            this.NoCompatibleControllerMessage = this.GetValue(Keys, "app.messagebox.no_compatible_controller");
        }

        this.CurrentLanguageFile = this.LanguageFile;
    }

    private string GetValue(Dictionary<string, string> Keys, string GetValueOf, string Suffix = "")
    {
        return Keys.TryGetValue(GetValueOf, out string? Value)
            ? $"{Value ?? string.Empty}{Suffix}"
            : Suffix;
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        this.Timer_ReadController.Stop();
        this.Timer_ReloadLanguage.Stop();

        if (this.controller != nint.Zero)
        {
            SDL.SDL_GameControllerClose(this.controller);
            this.controller = nint.Zero;
        }

        SDL.SDL_Quit();
    }
}
