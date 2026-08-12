using System.Numerics;
using Raylib_cs;
using RetroEngine.Assets;

namespace RetroEngine.Graphics;

/// <summary>
/// Draws textures and sprite-sheet regions. Loading/caching is owned by <see cref="AssetManager"/>.
/// </summary>
public static class Renderer
{
    public static Color ClearColor { get; set; } = Color.Black;

    public static void Clear()
    {
        Raylib.ClearBackground(ClearColor);
    }

    public static void Clear(Color color)
    {
        Raylib.ClearBackground(color);
    }

    public static Texture2D LoadTexture(string path) => AssetManager.GetTexture(path);

    public static bool TryGetTexture(string path, out Texture2D texture) =>
        AssetManager.TryGetTexture(path, out texture);

    public static bool HasTexture(string path) => AssetManager.TryGetTexture(path, out _);

    public static void UnloadTexture(string path) => AssetManager.UnloadTexture(path);

    public static void UnloadAll()
    {
        // Textures are owned by AssetManager; full teardown happens via AssetManager.UnloadAll().
    }

    public static void DrawTexture(Texture2D texture, int x, int y, Color? tint = null)
    {
        Raylib.DrawTexture(texture, x, y, tint ?? Color.White);
    }

    public static void DrawTexture(Texture2D texture, Vector2 position, Color? tint = null)
    {
        Raylib.DrawTextureV(texture, position, tint ?? Color.White);
    }

    /// <summary>
    /// Draws a sub-rectangle of a texture (sprite sheet) into a destination rectangle.
    /// </summary>
    public static void DrawSprite(
        Texture2D texture,
        Rectangle source,
        Rectangle destination,
        Color? tint = null,
        Vector2? origin = null,
        float rotationDegrees = 0f)
    {
        Raylib.DrawTexturePro(
            texture,
            source,
            destination,
            origin ?? Vector2.Zero,
            rotationDegrees,
            tint ?? Color.White);
    }

    /// <summary>
    /// Draws a sprite-sheet region at a world position using the source size (1:1 pixels).
    /// </summary>
    public static void DrawSprite(
        Texture2D texture,
        Rectangle source,
        Vector2 position,
        Color? tint = null)
    {
        var destination = new Rectangle(position.X, position.Y, source.Width, source.Height);
        DrawSprite(texture, source, destination, tint);
    }

    /// <summary>
    /// Draws a flipped sprite. Negative source width/height flips on that axis (Raylib convention).
    /// </summary>
    public static void DrawSpriteFlipped(
        Texture2D texture,
        Rectangle source,
        Rectangle destination,
        bool flipX,
        bool flipY,
        Color? tint = null)
    {
        if (flipX)
        {
            source.X += source.Width;
            source.Width = -source.Width;
        }

        if (flipY)
        {
            source.Y += source.Height;
            source.Height = -source.Height;
        }

        DrawSprite(texture, source, destination, tint);
    }

    public static Rectangle SourceRect(int frameX, int frameY, int frameWidth, int frameHeight)
    {
        return new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
    }

    public static Rectangle DestRect(float x, float y, float width, float height)
    {
        return new Rectangle(x, y, width, height);
    }
}
