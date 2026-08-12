using Raylib_cs;

namespace RetroEngine.Core;

/// <summary>
/// Owns the active <see cref="Scene"/> and forwards engine update/draw calls.
/// </summary>
public static class SceneManager
{
    public static Scene? CurrentScene { get; private set; }

    public static void ChangeScene(Scene newScene)
    {
        ArgumentNullException.ThrowIfNull(newScene);

        CurrentScene?.Unload();
        CurrentScene = newScene;

        // Defer Load until the window/audio device exist (safe to call before Engine.Run).
        if (Raylib.IsWindowReady())
        {
            CurrentScene.LoadScene();
        }
    }

    /// <summary>
    /// Ensures the current scene has been loaded after the engine window is ready.
    /// </summary>
    public static void EnsureSceneLoaded()
    {
        if (CurrentScene is null || !Raylib.IsWindowReady())
        {
            return;
        }

        if (!CurrentScene.IsLoaded)
        {
            CurrentScene.LoadScene();
        }
    }

    public static void Update(float deltaTime)
    {
        CurrentScene?.Update(deltaTime);
    }

    public static void Draw()
    {
        CurrentScene?.Draw();
    }

    public static void Unload()
    {
        CurrentScene?.Unload();
        CurrentScene = null;
    }
}
