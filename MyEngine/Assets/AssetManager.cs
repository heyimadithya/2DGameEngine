using Raylib_cs;

namespace RetroEngine.Assets;

/// <summary>
/// Central load-once cache for textures, sounds, and music streams.
/// </summary>
public static class AssetManager
{
    private static readonly Dictionary<string, Texture2D> _textures = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Sound> _sounds = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Music> _music = new(StringComparer.OrdinalIgnoreCase);

    public static Texture2D GetTexture(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (_textures.TryGetValue(path, out Texture2D cached))
        {
            return cached;
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Texture not found: {path}", path);
        }

        Texture2D texture = Raylib.LoadTexture(path);
        if (texture.Id == 0)
        {
            throw new InvalidOperationException($"Failed to load texture: {path}");
        }

        _textures[path] = texture;
        return texture;
    }

    public static Sound GetSound(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!Raylib.IsAudioDeviceReady())
        {
            throw new InvalidOperationException("Audio device is not ready.");
        }

        if (_sounds.TryGetValue(path, out Sound cached))
        {
            return cached;
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Sound not found: {path}", path);
        }

        Sound sound = Raylib.LoadSound(path);
        _sounds[path] = sound;
        return sound;
    }

    public static Music GetMusic(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!Raylib.IsAudioDeviceReady())
        {
            throw new InvalidOperationException("Audio device is not ready.");
        }

        if (_music.TryGetValue(path, out Music cached))
        {
            return cached;
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Music not found: {path}", path);
        }

        Music music = Raylib.LoadMusicStream(path);
        if (!Raylib.IsMusicValid(music))
        {
            throw new InvalidOperationException($"Failed to load music: {path}");
        }

        _music[path] = music;
        return music;
    }

    public static bool TryGetTexture(string path, out Texture2D texture) =>
        _textures.TryGetValue(path, out texture);

    public static bool TryGetSound(string path, out Sound sound) =>
        _sounds.TryGetValue(path, out sound);

    public static bool TryGetMusic(string path, out Music music) =>
        _music.TryGetValue(path, out music);

    public static void UnloadTexture(string path)
    {
        if (_textures.Remove(path, out Texture2D texture))
        {
            Raylib.UnloadTexture(texture);
        }
    }

    public static void UnloadSound(string path)
    {
        if (_sounds.Remove(path, out Sound sound))
        {
            Raylib.UnloadSound(sound);
        }
    }

    public static void UnloadMusic(string path)
    {
        if (_music.Remove(path, out Music music))
        {
            if (Raylib.IsMusicStreamPlaying(music))
            {
                Raylib.StopMusicStream(music);
            }

            Raylib.UnloadMusicStream(music);
        }
    }

    /// <summary>
    /// Updates the cached <see cref="Music.Looping"/> flag (Music is a struct).
    /// </summary>
    public static void SetMusicLooping(string path, bool loop)
    {
        if (!_music.TryGetValue(path, out Music music))
        {
            return;
        }

        music.Looping = loop;
        _music[path] = music;
    }

    public static void UnloadAll()
    {
        foreach (Texture2D texture in _textures.Values)
        {
            Raylib.UnloadTexture(texture);
        }

        _textures.Clear();

        foreach (Sound sound in _sounds.Values)
        {
            Raylib.UnloadSound(sound);
        }

        _sounds.Clear();

        foreach (Music track in _music.Values)
        {
            if (Raylib.IsMusicStreamPlaying(track))
            {
                Raylib.StopMusicStream(track);
            }

            Raylib.UnloadMusicStream(track);
        }

        _music.Clear();
    }
}
