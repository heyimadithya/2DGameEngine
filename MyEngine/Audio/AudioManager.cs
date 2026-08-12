using Raylib_cs;
using RetroEngine.Assets;

namespace RetroEngine.Audio;

/// <summary>
/// Plays short sound effects and streaming background music.
/// Asset lifetime is owned by <see cref="AssetManager"/>; call <see cref="UpdateMusic"/> every frame.
/// </summary>
public static class AudioManager
{
    private static string? _currentMusicKey;

    public static bool IsReady => Raylib.IsAudioDeviceReady();

    public static void Initialize()
    {
        if (!Raylib.IsAudioDeviceReady())
        {
            Raylib.InitAudioDevice();
        }
    }

    public static void Shutdown()
    {
        StopMusic();
        _currentMusicKey = null;

        if (Raylib.IsAudioDeviceReady())
        {
            Raylib.CloseAudioDevice();
        }
    }

    public static Sound LoadSound(string path) => AssetManager.GetSound(path);

    public static void PlaySound(string path)
    {
        PlaySound(LoadSound(path));
    }

    public static void PlaySound(Sound sound)
    {
        EnsureAudioReady();
        Raylib.PlaySound(sound);
    }

    public static void UnloadSound(string path) => AssetManager.UnloadSound(path);

    public static Music LoadMusic(string path) => AssetManager.GetMusic(path);

    public static void PlayMusic(string path, bool loop = true)
    {
        EnsureAudioReady();

        if (_currentMusicKey is not null &&
            AssetManager.TryGetMusic(_currentMusicKey, out Music current) &&
            Raylib.IsMusicStreamPlaying(current))
        {
            Raylib.StopMusicStream(current);
        }

        Music music = AssetManager.GetMusic(path);
        AssetManager.SetMusicLooping(path, loop);
        music.Looping = loop;

        _currentMusicKey = path;
        Raylib.PlayMusicStream(music);
    }

    public static void PlayMusic(Music music)
    {
        EnsureAudioReady();
        Raylib.PlayMusicStream(music);
    }

    /// <summary>
    /// Must be called every frame so streamed music keeps buffering.
    /// </summary>
    public static void UpdateMusic()
    {
        if (_currentMusicKey is null)
        {
            return;
        }

        if (!AssetManager.TryGetMusic(_currentMusicKey, out Music music))
        {
            _currentMusicKey = null;
            return;
        }

        Raylib.UpdateMusicStream(music);
    }

    public static void StopMusic()
    {
        if (_currentMusicKey is null)
        {
            return;
        }

        if (AssetManager.TryGetMusic(_currentMusicKey, out Music music))
        {
            Raylib.StopMusicStream(music);
        }
    }

    public static void UnloadMusic(string path)
    {
        if (string.Equals(_currentMusicKey, path, StringComparison.OrdinalIgnoreCase))
        {
            StopMusic();
            _currentMusicKey = null;
        }

        AssetManager.UnloadMusic(path);
    }

    public static void UnloadAll()
    {
        StopMusic();
        _currentMusicKey = null;
    }

    private static void EnsureAudioReady()
    {
        if (!Raylib.IsAudioDeviceReady())
        {
            throw new InvalidOperationException(
                "Audio device is not ready. Engine must call AudioManager.Initialize() during startup.");
        }
    }
}
