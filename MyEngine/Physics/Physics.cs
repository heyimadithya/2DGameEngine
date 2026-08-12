using System.Numerics;
using Raylib_cs;

namespace RetroEngine.Physics;

/// <summary>
/// Lightweight 2D collision helpers for retro AABB gameplay.
/// Positions/velocities elsewhere should use <see cref="Vector2"/>.
/// </summary>
public static class Physics
{
    /// <summary>
    /// Axis-aligned bounding box overlap test.
    /// </summary>
    public static bool CheckCollision(Rectangle a, Rectangle b)
    {
        return Raylib.CheckCollisionRecs(a, b);
    }

    /// <summary>
    /// Point-in-rectangle test (mouse clicks vs duck hitboxes, etc.).
    /// </summary>
    public static bool CheckPointCollision(Vector2 point, Rectangle rect)
    {
        return Raylib.CheckCollisionPointRec(point, rect);
    }

    /// <summary>
    /// Builds an AABB from a position (top-left) and size.
    /// </summary>
    public static Rectangle CreateAabb(Vector2 position, Vector2 size)
    {
        return new Rectangle(position.X, position.Y, size.X, size.Y);
    }

    /// <summary>
    /// Builds an AABB from a center point and half-extents.
    /// </summary>
    public static Rectangle CreateAabbFromCenter(Vector2 center, Vector2 halfSize)
    {
        return new Rectangle(
            center.X - halfSize.X,
            center.Y - halfSize.Y,
            halfSize.X * 2f,
            halfSize.Y * 2f);
    }
}
