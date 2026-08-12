using System.Numerics;
using Raylib_cs;
using RetroEngine.Graphics;

namespace RetroEngine.Core;

/// <summary>
/// Base entity for scene-driven gameplay. Override <see cref="Update"/> / <see cref="Draw"/> for behavior.
/// </summary>
public class GameObject
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Width;
    public float Height;
    public Color Tint = Color.White;
    public Texture2D? Texture;
    public bool IsActive = true;
    public bool IsVisible = true;

    public GameObject()
    {
    }

    public GameObject(Vector2 position, float width, float height)
    {
        Position = position;
        Width = width;
        Height = height;
    }

    public Rectangle Bounds => new(Position.X, Position.Y, Width, Height);

    public Vector2 Center
    {
        get => new(Position.X + Width * 0.5f, Position.Y + Height * 0.5f);
        set => Position = new Vector2(value.X - Width * 0.5f, value.Y - Height * 0.5f);
    }

    public virtual void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
    }

    public virtual void Draw()
    {
        if (!IsVisible)
        {
            return;
        }

        if (Texture is { Id: not 0 } texture)
        {
            var source = new Rectangle(0, 0, texture.Width, texture.Height);
            var destination = new Rectangle(Position.X, Position.Y, Width > 0 ? Width : texture.Width, Height > 0 ? Height : texture.Height);
            Renderer.DrawSprite(texture, source, destination, Tint);
            return;
        }

        Raylib.DrawRectangleRec(Bounds, Tint);
    }
}
