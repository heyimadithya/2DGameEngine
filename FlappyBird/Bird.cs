using System.Numerics;
using Raylib_cs;
using RetroEngine.Core;
using RetroEngine.Input;

namespace FlappyBird;

public sealed class Bird : GameObject
{
    public const float Gravity = 980f;
    public const float JumpImpulse = -320f;
    public const float Size = 28f;

    public Bird(Vector2 position)
        : base(position, Size, Size)
    {
        Tint = Color.Gold;
    }

    public override void Update(float deltaTime)
    {
        Velocity.Y += Gravity * deltaTime;

        if (InputManager.IsKeyPressed(KeyboardKey.Space) || InputManager.WasLeftClicked())
        {
            Velocity.Y = JumpImpulse;
        }

        base.Update(deltaTime);

        float screenHeight = Raylib.GetScreenHeight();

        if (Position.Y < 0f)
        {
            Position.Y = 0f;
            Velocity.Y = 0f;
        }

        if (Position.Y + Height > screenHeight)
        {
            Position.Y = screenHeight - Height;
            Velocity.Y = 0f;
        }
    }
}
