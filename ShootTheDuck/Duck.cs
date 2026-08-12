using System.Numerics;
using Raylib_cs;
using RetroEngine.Core;

namespace ShootTheDuck;

public enum DuckKind
{
    Mallard,
    Golden,
    Swift,
    BonusCarrier
}

public sealed class Duck : GameObject
{
    private float _wingPhase;
    private float _bobPhase;
    private float _escapeTimer;
    private bool _escaping;

    public DuckKind Kind { get; }
    public bool IsDead { get; private set; }
    public bool Escaped { get; private set; }
    public int PointValue { get; }
    public int CoinValue { get; }
    public bool FacingRight { get; private set; }

    public Duck(Vector2 position, DuckKind kind, bool facingRight)
    {
        Kind = kind;
        FacingRight = facingRight;
        Position = position;
        Width = kind == DuckKind.Swift ? 42f : 52f;
        Height = kind == DuckKind.Swift ? 30f : 36f;
        Tint = KindColor(kind);
        PointValue = KindPoints(kind);
        CoinValue = KindCoins(kind);

        float speed = KindSpeed(kind);
        Velocity = new Vector2(facingRight ? speed : -speed, 0f);
        _bobPhase = Random.Shared.NextSingle() * MathF.PI * 2f;
        _wingPhase = Random.Shared.NextSingle() * MathF.PI * 2f;
    }

    public static Duck SpawnRandom(float screenW, float screenH, int wave)
    {
        bool fromLeft = Random.Shared.Next(2) == 0;
        float y = 60f + Random.Shared.NextSingle() * (screenH * 0.45f);
        float x = fromLeft ? -60f : screenW + 20f;

        DuckKind kind = RollKind(wave);
        return new Duck(new Vector2(x, y), kind, fromLeft);
    }

    public override void Update(float deltaTime)
    {
        float dt = deltaTime * WorldClock.EnemyScale;

        if (IsDead || Escaped)
        {
            if (IsDead)
            {
                Velocity = new Vector2(Velocity.X * 0.92f, Velocity.Y + 900f * dt);
                Position += Velocity * dt;
            }

            return;
        }

        _wingPhase += dt * 14f;
        _bobPhase += dt * 3.5f;

        float bob = MathF.Sin(_bobPhase) * 28f * dt;
        Position += new Vector2(Velocity.X * dt, bob + Velocity.Y * dt);

        // Mild vertical drift so paths feel alive.
        Velocity.Y = MathF.Sin(_bobPhase * 0.7f) * 20f;

        if (_escaping)
        {
            _escapeTimer -= dt;
            Velocity.Y = -180f;
            if (_escapeTimer <= 0f || Position.Y < -80f)
            {
                Escaped = true;
                IsActive = false;
            }
        }
    }

    public override void Draw()
    {
        if (!IsVisible || Escaped)
        {
            return;
        }

        Vector2 c = Center;
        float wing = MathF.Sin(_wingPhase) * 10f;
        Color body = IsDead ? Color.DarkGray : Tint;
        Color belly = IsDead ? Color.Gray : Color.RayWhite;

        // Body
        Raylib.DrawEllipse((int)c.X, (int)c.Y, Width * 0.45f, Height * 0.42f, body);
        Raylib.DrawEllipse((int)(c.X + (FacingRight ? 8f : -8f)), (int)(c.Y + 4f), Width * 0.22f, Height * 0.22f, belly);

        // Head
        float headX = c.X + (FacingRight ? Width * 0.28f : -Width * 0.28f);
        Raylib.DrawCircle((int)headX, (int)(c.Y - 4f), 11f, body);
        Raylib.DrawCircle((int)(headX + (FacingRight ? 4f : -4f)), (int)(c.Y - 6f), 2.5f, Color.Black);

        // Beak
        float beakX = headX + (FacingRight ? 12f : -12f);
        Raylib.DrawTriangle(
            new Vector2(beakX, c.Y - 6f),
            new Vector2(beakX + (FacingRight ? 12f : -12f), c.Y - 2f),
            new Vector2(beakX, c.Y + 2f),
            Color.Orange);

        // Wings
        float wingDir = FacingRight ? 1f : -1f;
        Raylib.DrawEllipse(
            (int)(c.X - wingDir * 4f),
            (int)(c.Y - 6f - wing),
            Width * 0.28f,
            8f + MathF.Abs(wing) * 0.3f,
            Color.DarkGreen);

        if (Kind == DuckKind.Golden)
        {
            Raylib.DrawCircleLines((int)c.X, (int)c.Y, Width * 0.55f, Color.Gold);
        }
        else if (Kind == DuckKind.BonusCarrier)
        {
            Raylib.DrawCircleLines((int)c.X, (int)c.Y, Width * 0.55f, Color.Magenta);
        }
    }

    public bool TryHit(Vector2 point)
    {
        if (IsDead || Escaped || !IsActive)
        {
            return false;
        }

        // Slightly generous hitbox for arcade feel.
        var hitbox = new Rectangle(Position.X - 4f, Position.Y - 4f, Width + 8f, Height + 8f);
        if (!Raylib.CheckCollisionPointRec(point, hitbox))
        {
            return false;
        }

        IsDead = true;
        Velocity = new Vector2(Velocity.X * 0.3f, -80f);
        return true;
    }

    public void BeginEscape()
    {
        if (IsDead || Escaped)
        {
            return;
        }

        _escaping = true;
        _escapeTimer = 1.4f;
        Velocity *= 1.35f;
    }

    public bool IsOffScreen(float screenW, float screenH)
    {
        return Position.X < -120f || Position.X > screenW + 120f || Position.Y > screenH + 120f;
    }

    private static DuckKind RollKind(int wave)
    {
        float roll = Random.Shared.NextSingle();
        float goldenChance = Math.Clamp(0.06f + wave * 0.01f, 0.06f, 0.18f);
        float swiftChance = Math.Clamp(0.12f + wave * 0.015f, 0.12f, 0.3f);
        float bonusChance = Math.Clamp(0.05f + wave * 0.008f, 0.05f, 0.14f);

        if (roll < goldenChance)
        {
            return DuckKind.Golden;
        }

        if (roll < goldenChance + bonusChance)
        {
            return DuckKind.BonusCarrier;
        }

        if (roll < goldenChance + bonusChance + swiftChance)
        {
            return DuckKind.Swift;
        }

        return DuckKind.Mallard;
    }

    private static Color KindColor(DuckKind kind) => kind switch
    {
        DuckKind.Golden => new Color(255, 200, 40, 255),
        DuckKind.Swift => new Color(70, 160, 255, 255),
        DuckKind.BonusCarrier => new Color(220, 80, 220, 255),
        _ => new Color(40, 140, 70, 255)
    };

    private static int KindPoints(DuckKind kind) => kind switch
    {
        DuckKind.Golden => 250,
        DuckKind.Swift => 150,
        DuckKind.BonusCarrier => 100,
        _ => 100
    };

    private static int KindCoins(DuckKind kind) => kind switch
    {
        DuckKind.Golden => 5,
        DuckKind.Swift => 2,
        DuckKind.BonusCarrier => 1,
        _ => 1
    };

    private static float KindSpeed(DuckKind kind) => kind switch
    {
        DuckKind.Swift => 220f + Random.Shared.NextSingle() * 60f,
        DuckKind.Golden => 140f,
        DuckKind.BonusCarrier => 120f,
        _ => 150f + Random.Shared.NextSingle() * 40f
    };
}
