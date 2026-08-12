using System.Numerics;
using Raylib_cs;
using RetroEngine.Core;

namespace FlappyBird;

public sealed class Pipe : GameObject
{
    public const float DefaultWidth = 64f;
    public const float MoveSpeed = 160f;

    public bool IsTop { get; }
    public bool HasScored { get; set; }

    public Pipe(Vector2 position, float height, bool isTop)
        : base(position, DefaultWidth, height)
    {
        IsTop = isTop;
        Tint = Color.Lime;
        Velocity = new Vector2(-MoveSpeed, 0f);
    }

    public override void Update(float deltaTime)
    {
        Velocity = new Vector2(-MoveSpeed, 0f);
        base.Update(deltaTime);
    }

    public bool IsOffScreen() => Position.X + Width < 0f;
}
