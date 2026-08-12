using Raylib_cs;
using RetroEngine.Assets;
using RetroEngine.Audio;
using RetroEngine.Graphics;
using RetroEngine.Input;

namespace RetroEngine.Core;

/// <summary>
/// Owns the Raylib window and drives a fixed-timestep game loop.
/// Logic/physics run at a stable rate; rendering can vary with the display.
/// </summary>
public class Engine
{
    public const int DefaultWidth = 800;
    public const int DefaultHeight = 450;
    public const int DefaultTargetFps = 60;
    public const float FixedDeltaTime = 1f / 60f;

    private readonly string _title;
    private readonly int _width;
    private readonly int _height;
    private readonly int _targetFps;

    private bool _isRunning;

    public Engine(
        string title = "Retro Engine",
        int width = DefaultWidth,
        int height = DefaultHeight,
        int targetFps = DefaultTargetFps)
    {
        _title = title;
        _width = width;
        _height = height;
        _targetFps = targetFps;
    }

    public int ScreenWidth => _width;
    public int ScreenHeight => _height;
    public bool IsRunning => _isRunning;

    /// <summary>Total seconds since the window was initialized.</summary>
    public double TotalTime { get; private set; }

    /// <summary>Elapsed seconds for the most recent render frame (variable).</summary>
    public float FrameDeltaTime { get; private set; }

    /// <summary>
    /// Interpolation factor in [0, 1] between the last and next fixed update.
    /// Useful later for smooth sprite motion between physics ticks.
    /// </summary>
    public float Alpha { get; private set; }

    public void Run()
    {
        Initialize();
        Load();

        _isRunning = true;

        double previousTime = Raylib.GetTime();
        double accumulator = 0.0;

        // Cap catch-up so a long hitch does not spiral into endless updates.
        const double maxFrameTime = 0.25;

        while (_isRunning && !Raylib.WindowShouldClose())
        {
            double currentTime = Raylib.GetTime();
            double frameTime = currentTime - previousTime;
            previousTime = currentTime;

            if (frameTime > maxFrameTime)
            {
                frameTime = maxFrameTime;
            }

            FrameDeltaTime = (float)frameTime;
            TotalTime = currentTime;
            accumulator += frameTime;

            // Poll once per render frame so pressed/released edges stay stable across catch-up ticks.
            InputManager.Update();

            while (accumulator >= FixedDeltaTime)
            {
                Update(FixedDeltaTime);
                accumulator -= FixedDeltaTime;
            }

            // Music streams must be pumped every render frame.
            AudioManager.UpdateMusic();

            Alpha = (float)(accumulator / FixedDeltaTime);

            BeginDraw();
            Draw(Alpha);
            EndDraw();
        }

        Unload();
        Shutdown();
    }

    public void Stop()
    {
        _isRunning = false;
    }

    protected virtual void Initialize()
    {
        Raylib.InitWindow(_width, _height, _title);
        Raylib.SetTargetFPS(_targetFps);
        Raylib.SetExitKey(KeyboardKey.Escape);
        AudioManager.Initialize();
    }

    /// <summary>Load textures, audio, and game state. Called once after the window opens.</summary>
    protected virtual void Load()
    {
        SceneManager.EnsureSceneLoaded();
    }

    /// <summary>Fixed-rate logic and physics. <paramref name="dt"/> is always <see cref="FixedDeltaTime"/>.</summary>
    protected virtual void Update(float dt)
    {
        SceneManager.Update(dt);
    }

    /// <summary>Variable-rate rendering. <paramref name="alpha"/> is the inter-tick blend factor.</summary>
    protected virtual void Draw(float alpha)
    {
        Renderer.Clear();

        if (SceneManager.CurrentScene is null)
        {
            Raylib.DrawText("Retro Engine ready", 24, 24, 20, Color.RayWhite);
            Raylib.DrawText("No scene loaded — call SceneManager.ChangeScene(...)", 24, 56, 16, Color.Gray);
            Raylib.DrawText($"Mouse: {InputManager.MouseX}, {InputManager.MouseY}", 24, 80, 16, Color.Gray);
            return;
        }

        SceneManager.Draw();
    }

    /// <summary>Release game resources before the window closes.</summary>
    protected virtual void Unload()
    {
        SceneManager.Unload();
        AssetManager.UnloadAll();
        AudioManager.UnloadAll();
        InputManager.Reset();
    }

    protected virtual void Shutdown()
    {
        AudioManager.Shutdown();

        if (Raylib.IsWindowReady())
        {
            Raylib.CloseWindow();
        }

        _isRunning = false;
    }

    private static void BeginDraw()
    {
        Raylib.BeginDrawing();
    }

    private static void EndDraw()
    {
        Raylib.EndDrawing();
    }
}
