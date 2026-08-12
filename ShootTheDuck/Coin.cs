using System.Numerics;
using Raylib_cs;
using RetroEngine.Core;

namespace ShootTheDuck;

public sealed class Coin : GameObject
{
    private float _spin;
    private float _life = 8f;
    private bool _magnetized;

    public int Value { get; }
    public bool Collected { get; private set; }

    public Coin(Vector2 position, int value = 1)
    {
        Position = position;
        Width = 18f;
        Height = 18f;
        Value = value;
        Velocity = new Vector2(
            (Random.Shared.NextSingle() - 0.5f) * 120f,
            -180f - Random.Shared.NextSingle() * 80f);
        Tint = Color.Gold;
    }

    public void EnableMagnet() => _magnetized = true;

    public override void Update(float deltaTime)
    {
        if (Collected)
        {
            return;
        }

        _spin += deltaTime * 8f;
        _life -= deltaTime;

        if (_magnetized)
        {
            Vector2 target = new(40f, Raylib.GetScreenHeight() - 36f);
            Vector2 to = target - Center;
            float dist = to.Length();
            if (dist < 18f)
            {
                Collect();
                return;
            }

            Velocity = Vector2.Normalize(to) * 420f;
            Position += Velocity * deltaTime;
            return;
        }

        Velocity.Y += 520f * deltaTime;
        Velocity.X *= 0.98f;
        Position += Velocity * deltaTime;

        float ground = Raylib.GetScreenHeight() - 70f;
        if (Position.Y > ground)
        {
            Position.Y = ground;
            Velocity.Y *= -0.35f;
            Velocity.X *= 0.7f;
            if (MathF.Abs(Velocity.Y) < 40f)
            {
                Velocity = Vector2.Zero;
            }
        }

        if (_life <= 0f)
        {
            // Auto-bank leftover coins so players aren't punished for focus-firing.
            Collect();
        }
    }

    public override void Draw()
    {
        if (Collected || !IsVisible)
        {
            return;
        }

        float scaleX = 0.55f + 0.45f * MathF.Abs(MathF.Cos(_spin));
        int cx = (int)Center.X;
        int cy = (int)Center.Y;
        Raylib.DrawEllipse(cx, cy, 9f * scaleX, 9f, Color.Gold);
        Raylib.DrawEllipseLines(cx, cy, 9f * scaleX, 9f, Color.Orange);
        if (scaleX > 0.75f)
        {
            Raylib.DrawText("$", cx - 4, cy - 6, 14, Color.Orange);
        }
    }

    public bool TryCollect(Vector2 point)
    {
        if (Collected)
        {
            return false;
        }

        if (!Raylib.CheckCollisionPointRec(point, new Rectangle(Position.X - 6, Position.Y - 6, Width + 12, Height + 12)))
        {
            return false;
        }

        Collect();
        return true;
    }

    private void Collect()
    {
        if (Collected)
        {
            return;
        }

        Collected = true;
        IsActive = false;
        PlayerProgress.Coins += Value;
    }
}
