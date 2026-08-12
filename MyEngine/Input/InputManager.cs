using System.Numerics;
using Raylib_cs;

namespace RetroEngine.Input;

/// <summary>
/// Centralized input wrapper. Poll once per render frame via <see cref="Update"/>,
/// then query pressed / held / released state during fixed updates or draw.
/// </summary>
public static class InputManager
{
    private static readonly bool[] _prevKeys = new bool[512];
    private static readonly bool[] _currKeys = new bool[512];

    private static readonly bool[] _prevMouse = new bool[8];
    private static readonly bool[] _currMouse = new bool[8];

    public static Vector2 MousePosition { get; private set; }
    public static int MouseX => (int)MousePosition.X;
    public static int MouseY => (int)MousePosition.Y;
    public static Vector2 MouseDelta { get; private set; }
    public static float MouseWheel { get; private set; }

    /// <summary>
    /// Snapshot Raylib input for this render frame. Call once at the start of each loop iteration,
    /// before fixed updates, so edge detection stays stable across catch-up ticks.
    /// </summary>
    public static void Update()
    {
        Array.Copy(_currKeys, _prevKeys, _currKeys.Length);
        Array.Copy(_currMouse, _prevMouse, _currMouse.Length);

        for (int i = 0; i < _currKeys.Length; i++)
        {
            _currKeys[i] = Raylib.IsKeyDown((KeyboardKey)i);
        }

        for (int i = 0; i < _currMouse.Length; i++)
        {
            _currMouse[i] = Raylib.IsMouseButtonDown((MouseButton)i);
        }

        Vector2 previousMouse = MousePosition;
        MousePosition = Raylib.GetMousePosition();
        MouseDelta = MousePosition - previousMouse;
        MouseWheel = Raylib.GetMouseWheelMove();
    }

    public static bool IsKeyDown(KeyboardKey key) => IsDown(_currKeys, (int)key);

    public static bool IsKeyUp(KeyboardKey key) => !IsKeyDown(key);

    public static bool IsKeyPressed(KeyboardKey key)
    {
        int index = (int)key;
        return IsDown(_currKeys, index) && !IsDown(_prevKeys, index);
    }

    public static bool IsKeyReleased(KeyboardKey key)
    {
        int index = (int)key;
        return !IsDown(_currKeys, index) && IsDown(_prevKeys, index);
    }

    public static bool IsMouseButtonDown(MouseButton button) => IsDown(_currMouse, (int)button);

    public static bool IsMouseButtonUp(MouseButton button) => !IsMouseButtonDown(button);

    public static bool IsMouseButtonPressed(MouseButton button)
    {
        int index = (int)button;
        return IsDown(_currMouse, index) && !IsDown(_prevMouse, index);
    }

    public static bool IsMouseButtonReleased(MouseButton button)
    {
        int index = (int)button;
        return !IsDown(_currMouse, index) && IsDown(_prevMouse, index);
    }

    /// <summary>Convenience for Duck Hunt-style click hits.</summary>
    public static bool WasLeftClicked() => IsMouseButtonPressed(MouseButton.Left);

    public static bool WasRightClicked() => IsMouseButtonPressed(MouseButton.Right);

    public static void Reset()
    {
        Array.Clear(_prevKeys);
        Array.Clear(_currKeys);
        Array.Clear(_prevMouse);
        Array.Clear(_currMouse);
        MousePosition = Vector2.Zero;
        MouseDelta = Vector2.Zero;
        MouseWheel = 0f;
    }

    private static bool IsDown(bool[] states, int index)
    {
        return index >= 0 && index < states.Length && states[index];
    }
}
