using System.Numerics;
using Raylib_cs;
using RetroEngine.Core;

namespace ShootTheDuck;

public sealed class FloatingText : GameObject
{
    private float _life;
    private readonly float _duration;
    private readonly int _fontSize;
    private readonly string _text;

    public FloatingText(Vector2 position, string text, Color color, float duration = 0.9f, int fontSize = 22)
    {
        Position = position;
        _text = text;
        Tint = color;
        _duration = duration;
        _life = duration;
        _fontSize = fontSize;
        Velocity = new Vector2(0f, -60f);
        Width = Raylib.MeasureText(text, fontSize);
        Height = fontSize;
    }

    public override void Update(float deltaTime)
    {
        _life -= deltaTime;
        Position += Velocity * deltaTime;
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

        float alpha = Anim.Clamp01(_life / _duration);
        var color = new Color(Tint.R, Tint.G, Tint.B, (byte)(alpha * 255));
        Raylib.DrawText(_text, (int)Position.X, (int)Position.Y, _fontSize, color);
    }
}
