using System.Numerics;
using Raylib_cs;
using RetroEngine.Core;

namespace ShootTheDuck;

public sealed class HitSpark : GameObject
{
    private readonly Vector2[] _dirs;
    private readonly float[] _speeds;
    private float _life = 0.35f;

    public HitSpark(Vector2 position, Color color)
    {
        Position = position;
        Tint = color;
        _dirs = new Vector2[10];
        _speeds = new float[10];
        for (int i = 0; i < _dirs.Length; i++)
        {
            float ang = Random.Shared.NextSingle() * MathF.PI * 2f;
            _dirs[i] = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
            _speeds[i] = 120f + Random.Shared.NextSingle() * 180f;
        }
    }

    public override void Update(float deltaTime)
    {
        _life -= deltaTime;
        if (_life <= 0f)
        {
            IsActive = false;
        }
    }

    public override void Draw()
    {
        if (!IsActive)
        {
            return;
        }

        float t = 1f - _life / 0.35f;
        byte a = (byte)((1f - t) * 255);
        var c = new Color(Tint.R, Tint.G, Tint.B, a);
        for (int i = 0; i < _dirs.Length; i++)
        {
            Vector2 p = Position + _dirs[i] * (_speeds[i] * t * 0.35f);
            Raylib.DrawCircle((int)p.X, (int)p.Y, 3f * (1f - t), c);
        }
    }
}
