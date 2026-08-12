namespace ShootTheDuck;

/// <summary>
/// Lightweight easing helpers for intro/UI motion.
/// </summary>
public static class Anim
{
    public static float Clamp01(float t) => t < 0f ? 0f : t > 1f ? 1f : t;

    public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);

    public static float EaseOutBack(float t)
    {
        t = Clamp01(t);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
    }

    public static float EaseOutCubic(float t)
    {
        t = Clamp01(t);
        return 1f - MathF.Pow(1f - t, 3f);
    }

    public static float EaseInOutSine(float t)
    {
        t = Clamp01(t);
        return -(MathF.Cos(MathF.PI * t) - 1f) * 0.5f;
    }

    public static float Bounce(float t)
    {
        t = Clamp01(t);
        return MathF.Abs(MathF.Sin(t * MathF.PI * 3f)) * (1f - t);
    }
}
