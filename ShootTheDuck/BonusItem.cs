using System.Numerics;
using Raylib_cs;
using RetroEngine.Core;

namespace ShootTheDuck;

public enum BonusType
{
    DoublePoints,
    SlowMo,
    RapidReload,
    CoinMagnet,
    ExtraAmmo
}

public sealed class BonusItem : GameObject
{
    private float _bob;
    private float _life = 10f;

    public BonusType Type { get; }
    public bool Collected { get; private set; }

    public BonusItem(Vector2 position, BonusType type)
    {
        Position = position;
        Type = type;
        Width = 34f;
        Height = 34f;
        Velocity = new Vector2((Random.Shared.NextSingle() - 0.5f) * 40f, 55f);
        Tint = TypeColor(type);
        _bob = Random.Shared.NextSingle() * MathF.PI * 2f;
    }

    public static BonusType Roll()
    {
        return (BonusType)Random.Shared.Next(0, 5);
    }

    public override void Update(float deltaTime)
    {
        if (Collected)
        {
            return;
        }

        _bob += deltaTime * 4f;
        _life -= deltaTime;
        Position += Velocity * deltaTime;
        Position.Y += MathF.Sin(_bob) * 20f * deltaTime;

        if (Position.Y > Raylib.GetScreenHeight() - 40f || _life <= 0f)
        {
            IsActive = false;
            Collected = true;
        }
    }

    public override void Draw()
    {
        if (Collected || !IsVisible)
        {
            return;
        }

        int cx = (int)Center.X;
        int cy = (int)Center.Y;
        Raylib.DrawRectangleRounded(new Rectangle(Position.X, Position.Y, Width, Height), 0.3f, 6, Tint);
        Raylib.DrawRectangleRoundedLines(new Rectangle(Position.X, Position.Y, Width, Height), 0.3f, 6, Color.White);
        string label = TypeLabel(Type);
        int w = Raylib.MeasureText(label, 12);
        Raylib.DrawText(label, cx - w / 2, cy - 6, 12, Color.Black);
    }

    public bool TryCollect(Vector2 point)
    {
        if (Collected || !IsActive)
        {
            return false;
        }

        if (!Raylib.CheckCollisionPointRec(point, Bounds))
        {
            return false;
        }

        Collected = true;
        IsActive = false;
        return true;
    }

    public static string TypeLabel(BonusType type) => type switch
    {
        BonusType.DoublePoints => "x2",
        BonusType.SlowMo => "SLOW",
        BonusType.RapidReload => "RLD",
        BonusType.CoinMagnet => "MAG",
        BonusType.ExtraAmmo => "+AM",
        _ => "?"
    };

    public static string TypeDescription(BonusType type) => type switch
    {
        BonusType.DoublePoints => "Double Points!",
        BonusType.SlowMo => "Slow Motion!",
        BonusType.RapidReload => "Rapid Reload!",
        BonusType.CoinMagnet => "Coin Magnet!",
        BonusType.ExtraAmmo => "Extra Ammo!",
        _ => "Bonus!"
    };

    private static Color TypeColor(BonusType type) => type switch
    {
        BonusType.DoublePoints => new Color(255, 120, 60, 255),
        BonusType.SlowMo => new Color(120, 200, 255, 255),
        BonusType.RapidReload => new Color(120, 255, 140, 255),
        BonusType.CoinMagnet => new Color(255, 220, 60, 255),
        BonusType.ExtraAmmo => new Color(255, 100, 160, 255),
        _ => Color.White
    };
}
