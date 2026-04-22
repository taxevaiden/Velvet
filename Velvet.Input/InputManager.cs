using System.Numerics;
using System.Runtime.CompilerServices;

using SDL3;

[assembly: InternalsVisibleTo("Velvet")]

namespace Velvet.Input
{
#pragma warning disable CS1591

    /// <summary>
    /// Represents keyboard keys.
    /// Values correspond to physical key positions rather than character input.
    /// </summary>
    public enum KeyCode
    {
        // Letters
        A = SDL.Scancode.A, B = SDL.Scancode.B, C = SDL.Scancode.C,
        D = SDL.Scancode.D, E = SDL.Scancode.E, F = SDL.Scancode.F,
        G = SDL.Scancode.G, H = SDL.Scancode.H, I = SDL.Scancode.I,
        J = SDL.Scancode.J, K = SDL.Scancode.K, L = SDL.Scancode.L,
        M = SDL.Scancode.M, N = SDL.Scancode.N, O = SDL.Scancode.O,
        P = SDL.Scancode.P, Q = SDL.Scancode.Q, R = SDL.Scancode.R,
        S = SDL.Scancode.S, T = SDL.Scancode.T, U = SDL.Scancode.U,
        V = SDL.Scancode.V, W = SDL.Scancode.W, X = SDL.Scancode.X,
        Y = SDL.Scancode.Y, Z = SDL.Scancode.Z,

        // Numbers (top row)
        D0 = SDL.Scancode.Alpha0, D1 = SDL.Scancode.Alpha1, D2 = SDL.Scancode.Alpha2,
        D3 = SDL.Scancode.Alpha3, D4 = SDL.Scancode.Alpha4, D5 = SDL.Scancode.Alpha5,
        D6 = SDL.Scancode.Alpha6, D7 = SDL.Scancode.Alpha7, D8 = SDL.Scancode.Alpha8,
        D9 = SDL.Scancode.Alpha9,

        // Whitespace / control
        Return = SDL.Scancode.Return,
        Escape = SDL.Scancode.Escape,
        Backspace = SDL.Scancode.Backspace,
        Tab = SDL.Scancode.Tab,
        Space = SDL.Scancode.Space,

        // Punctuation
        Minus = SDL.Scancode.Minus,
        Equals = SDL.Scancode.Equals,
        LeftBracket = SDL.Scancode.Leftbracket,
        RightBracket = SDL.Scancode.Rightbracket,
        Backslash = SDL.Scancode.Backslash,
        Semicolon = SDL.Scancode.Semicolon,
        Quote = SDL.Scancode.Apostrophe,
        Grave = SDL.Scancode.Grave,
        Comma = SDL.Scancode.Comma,
        Period = SDL.Scancode.Period,
        Slash = SDL.Scancode.Slash,

        // Function keys
        F1 = SDL.Scancode.F1, F2 = SDL.Scancode.F2, F3 = SDL.Scancode.F3,
        F4 = SDL.Scancode.F4, F5 = SDL.Scancode.F5, F6 = SDL.Scancode.F6,
        F7 = SDL.Scancode.F7, F8 = SDL.Scancode.F8, F9 = SDL.Scancode.F9,
        F10 = SDL.Scancode.F10, F11 = SDL.Scancode.F11, F12 = SDL.Scancode.F12,

        // Navigation
        Insert = SDL.Scancode.Insert, Home = SDL.Scancode.Home,
        PageUp = SDL.Scancode.Pageup, Delete = SDL.Scancode.Delete,
        End = SDL.Scancode.End, PageDown = SDL.Scancode.Pagedown,
        Right = SDL.Scancode.Right, Left = SDL.Scancode.Left,
        Down = SDL.Scancode.Down, Up = SDL.Scancode.Up,

        // Modifiers
        LeftCtrl = SDL.Scancode.LCtrl, LeftShift = SDL.Scancode.LShift,
        LeftAlt = SDL.Scancode.LAlt, LeftGui = SDL.Scancode.LGUI,
        RightCtrl = SDL.Scancode.RCtrl, RightShift = SDL.Scancode.RShift,
        RightAlt = SDL.Scancode.RAlt, RightGui = SDL.Scancode.RGUI,

        // Numpad
        NumLockClear = SDL.Scancode.NumLockClear,
        NumpadDivide = SDL.Scancode.KpDivide,
        NumpadMultiply = SDL.Scancode.KpMultiply,
        NumpadMinus = SDL.Scancode.KpMinus,
        NumpadPlus = SDL.Scancode.KpPlus,
        NumpadEnter = SDL.Scancode.KpEnter,
        Numpad0 = SDL.Scancode.Kp0, Numpad1 = SDL.Scancode.Kp1,
        Numpad2 = SDL.Scancode.Kp2, Numpad3 = SDL.Scancode.Kp3,
        Numpad4 = SDL.Scancode.Kp4, Numpad5 = SDL.Scancode.Kp5,
        Numpad6 = SDL.Scancode.Kp6, Numpad7 = SDL.Scancode.Kp7,
        Numpad8 = SDL.Scancode.Kp8, Numpad9 = SDL.Scancode.Kp9,
        NumpadPeriod = SDL.Scancode.KpPeriod,

        // Locks / system
        CapsLock = SDL.Scancode.Capslock,
        PrintScreen = SDL.Scancode.Printscreen,
        ScrollLock = SDL.Scancode.Scrolllock,
        Pause = SDL.Scancode.Pause,
        Menu = SDL.Scancode.Menu,

        Unknown = SDL.Scancode.Unknown,
    }

    /// <summary>Represents mouse buttons.</summary>
    public enum MouseButton
    {
        Left = 1,
        Middle = 2,
        Right = 3,
        Side1 = 4,
        Side2 = 5,
    }

#pragma warning restore CS1591

    /// <summary>
    /// Provides access to keyboard and mouse input state.
    /// All state reflects the current frame; call <see cref="EndFrame"/> at the end of each frame to advance it.
    /// </summary>
    public static class InputManager
    {
        private static readonly bool[] _keyboardState = new bool[(uint)SDL.Scancode.Count];
        private static readonly bool[] _prevKeyboardState = new bool[(uint)SDL.Scancode.Count];

        private static readonly HashSet<byte> _heldButtons = new();
        private static readonly HashSet<byte> _pressedButtons = new();
        private static readonly HashSet<byte> _releasedButtons = new();

        private static float _mouseX, _mouseY;
        private static float _prevMouseX, _prevMouseY;
        private static float _scrollX, _scrollY;

        // Internal event / frame hooks

        internal static void ProcessEvent(SDL.Event e)
        {
            switch ((SDL.EventType)e.Type)
            {
                case SDL.EventType.MouseMotion:
                    _mouseX = e.Motion.X;
                    _mouseY = e.Motion.Y;
                    break;

                case SDL.EventType.MouseWheel:
                    _scrollX = e.Wheel.X;
                    _scrollY = e.Wheel.Y;
                    if (e.Wheel.Direction == SDL.MouseWheelDirection.Flipped)
                    {
                        _scrollX = -_scrollX;
                        _scrollY = -_scrollY;
                    }
                    break;

                case SDL.EventType.MouseButtonDown:
                    if (_heldButtons.Add(e.Button.Button))
                        _pressedButtons.Add(e.Button.Button);
                    break;

                case SDL.EventType.MouseButtonUp:
                    if (_heldButtons.Remove(e.Button.Button))
                        _releasedButtons.Add(e.Button.Button);
                    break;
            }
        }

        internal static void Update()
        {
            // Snapshot previous keyboard state before overwriting.
            Array.Copy(_keyboardState, _prevKeyboardState, _keyboardState.Length);

            ReadOnlySpan<bool> keys = SDL.GetKeyboardState(out _);
            for (int i = 0; i < _keyboardState.Length; i++)
                _keyboardState[i] = keys[i];
        }

        internal static void EndFrame()
        {
            _prevMouseX = _mouseX;
            _prevMouseY = _mouseY;
            _scrollX = 0f;
            _scrollY = 0f;
            _pressedButtons.Clear();
            _releasedButtons.Clear();
        }

        // Keyboard

        /// <summary>Returns true while the key is held down.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyDown(KeyCode key) => _keyboardState[(uint)key];

        /// <summary>Returns true while the key is not held down.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyUp(KeyCode key) => !_keyboardState[(uint)key];

        /// <summary>Returns true on the first frame the key is pressed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyPressed(KeyCode key)
            => _keyboardState[(uint)key] && !_prevKeyboardState[(uint)key];

        /// <summary>Returns true on the first frame the key is released.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyReleased(KeyCode key)
            => !_keyboardState[(uint)key] && _prevKeyboardState[(uint)key];

        /// <summary>Returns true if any of the specified keys are held down.</summary>
        public static bool IsAnyKeyDown(params KeyCode[] keys)
        {
            foreach (var key in keys)
                if (IsKeyDown(key)) return true;
            return false;
        }

        /// <summary>Returns true if all of the specified keys are held down simultaneously.</summary>
        public static bool AreAllKeysDown(params KeyCode[] keys)
        {
            foreach (var key in keys)
                if (!IsKeyDown(key)) return false;
            return true;
        }

        // Mouse buttons

        /// <summary>Returns true while the mouse button is held down.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMouseButtonDown(MouseButton b) => _heldButtons.Contains((byte)b);

        /// <summary>Returns true while the mouse button is not held down.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMouseButtonUp(MouseButton b) => !_heldButtons.Contains((byte)b);

        /// <summary>Returns true on the first frame the mouse button is pressed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMouseButtonPressed(MouseButton b) => _pressedButtons.Contains((byte)b);

        /// <summary>Returns true on the first frame the mouse button is released.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMouseButtonReleased(MouseButton b) => _releasedButtons.Contains((byte)b);

        // Mouse position

        /// <summary>Current mouse position in window coordinates as a <see cref="Vector2"/>.</summary>
        public static Vector2 MousePosition => new(_mouseX, _mouseY);

        /// <summary>Mouse position from the previous frame as a <see cref="Vector2"/>.</summary>
        public static Vector2 PreviousMousePosition => new(_prevMouseX, _prevMouseY);

        /// <summary>
        /// How far the mouse moved since the last frame as a <see cref="Vector2"/>.
        /// Useful for camera look, drag operations, etc.
        /// </summary>
        public static Vector2 MouseDelta => new(_mouseX - _prevMouseX, _mouseY - _prevMouseY);

        /// <summary>Gets the current mouse position in window coordinates.</summary>
        public static void GetMousePosition(out float x, out float y) { x = _mouseX; y = _mouseY; }

        /// <summary>Gets the mouse position from the previous frame in window coordinates.</summary>
        public static void GetPreviousMousePosition(out float x, out float y) { x = _prevMouseX; y = _prevMouseY; }

        /// <summary>Gets how far the mouse moved since the last frame in window coordinates.</summary>
        public static void GetMouseDelta(out float x, out float y) { x = _mouseX - _prevMouseX; y = _mouseY - _prevMouseY; }

        // Mouse scroll

        /// <summary>Mouse scroll delta for the current frame as a <see cref="Vector2"/>.</summary>
        public static Vector2 ScrollDelta => new(_scrollX, _scrollY);

        /// <summary>Gets the mouse scroll delta for the current frame.</summary>
        public static void GetScrollDelta(out float x, out float y) { x = _scrollX; y = _scrollY; }
    }
}