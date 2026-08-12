using System.Numerics;
using Raylib_cs;
using RetroEngine.Core;
using RetroEngine.Graphics;
using RetroEngine.Input;

namespace ShootTheDuck;

/// <summary>
/// Animated title card: sky fade, bouncing title, fly-by duck, pulsing start prompt.
/// </summary>
public sealed class IntroScene : Scene
{
    private float _time;
    private float _fade;
    private Duck? _flyby;
    private float _blink;
    private bool _starting;

    protected override void Load()
    {
        Renderer.ClearColor = new Color(20, 30, 60, 255);
        Raylib.HideCursor();
        PlayerProgress.ResetRun();
        WorldClock.EnemyScale = 1f;

        _flyby = new Duck(new Vector2(-80f, 180f), DuckKind.Golden, facingRight: true);
        _flyby.Velocity = new Vector2(170f, 0f);
        AddEntity(_flyby);
    }

    public override void Update(float deltaTime)
    {
        _time += deltaTime;
        _fade = Anim.EaseOutCubic(Math.Min(1f, _time / 0.8f));
        _blink += deltaTime;

        base.Update(deltaTime);

        if (_flyby is { IsActive: true } && _flyby.Position.X > Raylib.GetScreenWidth() + 80f)
        {
            _flyby.IsActive = false;
        }

        if (!_starting && _time > 1.1f &&
            (InputManager.IsKeyPressed(KeyboardKey.Space) ||
             InputManager.IsKeyPressed(KeyboardKey.Enter) ||
             InputManager.WasLeftClicked()))
        {
            _starting = true;
            SceneManager.ChangeScene(new HuntScene());
        }
    }

    public override void Draw()
    {
        DrawSky();
        DrawGrass();
        base.Draw();

        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();

        float titleT = Anim.EaseOutBack((_time - 0.25f) / 0.85f);
        float titleY = Anim.Lerp(-80f, sh * 0.28f, titleT);
        const string title = "SHOOT THE DUCK";
        const int titleSize = 52;
        int titleW = Raylib.MeasureText(title, titleSize);
        var titleColor = new Color(255, 220, 80, (int)(255 * _fade));
        Raylib.DrawText(title, (sw - titleW) / 2 + 3, (int)titleY + 3, titleSize, new Color(0, 0, 0, (int)(140 * _fade)));
        Raylib.DrawText(title, (sw - titleW) / 2, (int)titleY, titleSize, titleColor);

        float subT = Anim.EaseOutCubic((_time - 0.9f) / 0.6f);
        const string sub = "BONUSES  ·  COINS  ·  MULTIPLIERS";
        int subW = Raylib.MeasureText(sub, 20);
        Raylib.DrawText(sub, (sw - subW) / 2, (int)titleY + 64, 20, new Color(230, 240, 255, (int)(255 * subT * _fade)));

        DrawFeatureChip(sw * 0.2f, sh * 0.55f, "x2 COMBO", new Color(255, 140, 60, 255), (_time - 1.2f) / 0.4f);
        DrawFeatureChip(sw * 0.5f, sh * 0.55f, "GOLD DUCKS", new Color(255, 210, 50, 255), (_time - 1.35f) / 0.4f);
        DrawFeatureChip(sw * 0.8f, sh * 0.55f, "BONUS DROPS", new Color(220, 100, 255, 255), (_time - 1.5f) / 0.4f);

        if (_time > 1.8f)
        {
            float pulse = 0.65f + 0.35f * MathF.Sin(_blink * 4f);
            const string prompt = "CLICK / SPACE TO START";
            int pw = Raylib.MeasureText(prompt, 28);
            Raylib.DrawText(prompt, (sw - pw) / 2, (int)(sh * 0.78f), 28, new Color(255, 255, 255, (int)(pulse * 255)));
        }

        if (_fade < 1f)
        {
            Raylib.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, (int)((1f - _fade) * 255)));
        }

        DrawCrosshair(InputManager.MousePosition);
    }

    private static void DrawFeatureChip(float x, float y, string text, Color color, float t)
    {
        t = Anim.Clamp01(t);
        float slide = Anim.Lerp(40f, 0f, Anim.EaseOutCubic(t));
        int a = (int)(255 * t);
        int w = Raylib.MeasureText(text, 18) + 28;
        const int h = 34;
        int px = (int)(x - w * 0.5f);
        int py = (int)(y + slide);
        Raylib.DrawRectangleRounded(new Rectangle(px, py, w, h), 0.5f, 8, new Color(color.R, color.G, color.B, a));
        Raylib.DrawText(text, px + 14, py + 8, 18, new Color(20, 20, 20, a));
    }

    private void DrawSky()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        Renderer.Clear(new Color(110, 180, 235, 255));

        float sunPulse = 1f + 0.03f * MathF.Sin(_time * 2f);
        Raylib.DrawCircle((int)(sw * 0.82f), (int)(sh * 0.18f), 40f * sunPulse, new Color(255, 240, 160, 255));

        for (int i = 0; i < 4; i++)
        {
            float cx = ((i * 220f) + _time * (12f + i * 4f)) % (sw + 160f) - 80f;
            float cy = 70f + i * 28f;
            Raylib.DrawEllipse((int)cx, (int)cy, 50f, 18f, new Color(255, 255, 255, 180));
            Raylib.DrawEllipse((int)(cx + 30), (int)(cy + 4), 40f, 16f, new Color(255, 255, 255, 160));
        }
    }

    private static void DrawGrass()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        Raylib.DrawRectangle(0, sh - 90, sw, 90, new Color(55, 150, 60, 255));
        Raylib.DrawRectangle(0, sh - 90, sw, 14, new Color(40, 120, 45, 255));
        for (int x = 0; x < sw; x += 18)
        {
            Raylib.DrawTriangle(
                new Vector2(x, sh - 90),
                new Vector2(x + 8, sh - 108),
                new Vector2(x + 16, sh - 90),
                new Color(50, 140, 55, 255));
        }
    }

    public static void DrawCrosshair(Vector2 mouse)
    {
        int x = (int)mouse.X;
        int y = (int)mouse.Y;
        Raylib.DrawCircleLines(x, y, 16, Color.Red);
        Raylib.DrawCircleLines(x, y, 4, Color.White);
        Raylib.DrawLine(x - 22, y, x - 8, y, Color.Red);
        Raylib.DrawLine(x + 8, y, x + 22, y, Color.Red);
        Raylib.DrawLine(x, y - 22, x, y - 8, Color.Red);
        Raylib.DrawLine(x, y + 8, x, y + 22, Color.Red);
    }
}
