using System.Numerics;

namespace Boist.Input
{
    /// <summary>
    /// The types of <see cref="InputEvent"/>s that can be processed.
    /// </summary>
    public enum InputEventType
    {
        /// <summary>
        /// Mouse moved.
        /// </summary>
        MouseMotion,
        /// <summary>
        /// Mouse wheel motion.
        /// </summary>
        MouseWheel,
        /// <summary>
        /// Mouse button pressed.
        /// </summary>
        MouseButtonDown,
        /// <summary>
        /// Mouse button released.
        /// </summary>
        MouseButtonUp,
        /// <summary>
        /// Key pressed.
        /// </summary>
        KeyDown,
        /// <summary>
        /// Key released.
        /// </summary>
        KeyUp,
    }
    /// <summary>
    /// An event that can be processed by an <see cref="InputManager"/> to update its input state.
    /// </summary>
    public struct InputEvent
    {
        /// <summary>
        /// The type of event.
        /// </summary>
        public InputEventType Type;
        /// <summary>
        /// The key that was pressed.
        /// </summary>
        public uint Key;
        /// <summary>
        /// The mouse button that was pressed.
        /// </summary>
        public uint MouseButton;
        /// <summary>
        /// The position of the mouse relative to the window.
        /// </summary>
        public Vector2 MousePosition;
        /// <summary>
        /// The scroll delta.
        /// </summary>
        public Vector2 MouseScroll;
    }
#pragma warning disable CS1591

    /// <summary>
    /// Represents keyboard keys.
    /// </summary>
    public enum KeyCode
    {
        // Letters
        A = 4, B = 5, C = 6,
        D = 7, E = 8, F = 9,
        G = 10, H = 11, I = 12,
        J = 13, K = 14, L = 15,
        M = 16, N = 17, O = 18,
        P = 19, Q = 20, R = 21,
        S = 22, T = 23, U = 24,
        V = 25, W = 26, X = 27,
        Y = 28, Z = 29,

        // Numbers (top row)
        D0 = 39, D1 = 30, D2 = 31,
        D3 = 32, D4 = 33, D5 = 34,
        D6 = 35, D7 = 36, D8 = 37,
        D9 = 38,

        // Whitespace / control
        Return = 40,
        Escape = 41,
        Backspace = 42,
        Tab = 43,
        Space = 44,

        // Punctuation
        Minus = 45,
        Equals = 46,
        LeftBracket = 47,
        RightBracket = 48,
        Backslash = 49,
        Semicolon = 51,
        Quote = 52,
        Grave = 53,
        Comma = 54,
        Period = 55,
        Slash = 56,

        // Function keys
        F1 = 58, F2 = 59, F3 = 60,
        F4 = 61, F5 = 62, F6 = 63,
        F7 = 64, F8 = 65, F9 = 66,
        F10 = 67, F11 = 68, F12 = 69,
        F13 = 104, F14 = 105, F15 = 106,
        F16 = 107, F17 = 108, F18 = 109,
        F19 = 110, F20 = 111, F21 = 112,
        F22 = 113, F23 = 114, F24 = 115,

        // Navigation
        Insert = 73, Home = 74,
        PageUp = 75, Delete = 76,
        End = 77, PageDown = 78,
        Right = 79, Left = 80,
        Down = 81, Up = 82,

        // Modifiers
        LeftCtrl = 224, LeftShift = 225,
        LeftAlt = 226, LeftGui = 227,
        RightCtrl = 228, RightShift = 229,
        RightAlt = 230, RightGui = 231,

        // Numpad
        NumLockClear = 83,
        NumpadDivide = 84,
        NumpadMultiply = 85,
        NumpadMinus = 86,
        NumpadPlus = 87,
        NumpadEnter = 88,
        Numpad0 = 98, Numpad1 = 89,
        Numpad2 = 90, Numpad3 = 91,
        Numpad4 = 92, Numpad5 = 93,
        Numpad6 = 94, Numpad7 = 95,
        Numpad8 = 96, Numpad9 = 97,
        NumpadPeriod = 99,

        // Locks / system
        CapsLock = 57,
        PrintScreen = 70,
        ScrollLock = 71,
        Pause = 72,
        Menu = 118,

        Unknown = 0,
    }

    /// <summary>Represents mouse buttons.</summary>
    public enum MouseButton
    {
        Left = 0,
        Middle = 1,
        Right = 2,
        Side1 = 3,
        Side2 = 4,
    }

#pragma warning restore CS1591
}