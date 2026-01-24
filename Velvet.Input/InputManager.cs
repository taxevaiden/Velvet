using System.Diagnostics.Tracing;
using System.Numerics;
using SDL3;
using static SDL3.SDL;

namespace Velvet.Input
{
    public enum KeyCode
    {
        // Letters
        A = SDL.Scancode.A,
        B = SDL.Scancode.B,
        C = SDL.Scancode.C,
        D = SDL.Scancode.D,
        E = SDL.Scancode.E,
        F = SDL.Scancode.F,
        G = SDL.Scancode.G,
        H = SDL.Scancode.H,
        I = SDL.Scancode.I,
        J = SDL.Scancode.J,
        K = SDL.Scancode.K,
        L = SDL.Scancode.L,
        M = SDL.Scancode.M,
        N = SDL.Scancode.N,
        O = SDL.Scancode.O,
        P = SDL.Scancode.P,
        Q = SDL.Scancode.Q,
        R = SDL.Scancode.R,
        S = SDL.Scancode.S,
        T = SDL.Scancode.T,
        U = SDL.Scancode.U,
        V = SDL.Scancode.V,
        W = SDL.Scancode.W,
        X = SDL.Scancode.X,
        Y = SDL.Scancode.Y,
        Z = SDL.Scancode.Z,

        // Numbers (top row)
        D0 = SDL.Scancode.Alpha0,
        D1 = SDL.Scancode.Alpha1,
        D2 = SDL.Scancode.Alpha2,
        D3 = SDL.Scancode.Alpha3,
        D4 = SDL.Scancode.Alpha4,
        D5 = SDL.Scancode.Alpha5,
        D6 = SDL.Scancode.Alpha6,
        D7 = SDL.Scancode.Alpha7,
        D8 = SDL.Scancode.Alpha8,
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
        F1 = SDL.Scancode.F1,
        F2 = SDL.Scancode.F2,
        F3 = SDL.Scancode.F3,
        F4 = SDL.Scancode.F4,
        F5 = SDL.Scancode.F5,
        F6 = SDL.Scancode.F6,
        F7 = SDL.Scancode.F7,
        F8 = SDL.Scancode.F8,
        F9 = SDL.Scancode.F9,
        F10 = SDL.Scancode.F10,
        F11 = SDL.Scancode.F11,
        F12 = SDL.Scancode.F12,

        // Navigation
        Insert = SDL.Scancode.Insert,
        Home = SDL.Scancode.Home,
        PageUp = SDL.Scancode.Pageup,
        Delete = SDL.Scancode.Delete,
        End = SDL.Scancode.End,
        PageDown = SDL.Scancode.Pagedown,

        Right = SDL.Scancode.Right,
        Left = SDL.Scancode.Left,
        Down = SDL.Scancode.Down,
        Up = SDL.Scancode.Up,

        // Modifiers
        LeftCtrl = SDL.Scancode.LCtrl,
        LeftShift = SDL.Scancode.LShift,
        LeftAlt = SDL.Scancode.LAlt,
        LeftGui = SDL.Scancode.LGUI,

        RightCtrl = SDL.Scancode.RCtrl,
        RightShift = SDL.Scancode.RShift,
        RightAlt = SDL.Scancode.RAlt,
        RightGui = SDL.Scancode.RGUI,

        // Numpad
        NumLockClear = SDL.Scancode.NumLockClear,
        NumpadDivide = SDL.Scancode.KpDivide,
        NumpadMultiply = SDL.Scancode.KpMultiply,
        NumpadMinus = SDL.Scancode.KpMinus,
        NumpadPlus = SDL.Scancode.KpPlus,
        NumpadEnter = SDL.Scancode.KpEnter,

        Numpad1 = SDL.Scancode.Kp1,
        Numpad2 = SDL.Scancode.Kp2,
        Numpad3 = SDL.Scancode.Kp3,
        Numpad4 = SDL.Scancode.Kp4,
        Numpad5 = SDL.Scancode.Kp5,
        Numpad6 = SDL.Scancode.Kp6,
        Numpad7 = SDL.Scancode.Kp7,
        Numpad8 = SDL.Scancode.Kp8,
        Numpad9 = SDL.Scancode.Kp9,
        Numpad0 = SDL.Scancode.Kp0,
        NumpadPeriod = SDL.Scancode.KpPeriod,

        // Locks / system
        CapsLock = SDL.Scancode.Capslock,
        PrintScreen = SDL.Scancode.Printscreen,
        ScrollLock = SDL.Scancode.Scrolllock,
        Pause = SDL.Scancode.Pause,
        Menu = SDL.Scancode.Menu,

        Unknown = SDL.Scancode.Unknown
    }


    public enum MouseButton
    {
        Left = 1,
        Middle = 2,
        Right = 3,
        X1 = 4,
        X2 = 5,
    }

    public static class InputManager
    {
        private static bool[] _keyboardState = new bool[((uint)SDL.Scancode.Count)];

        private static HashSet<byte> _heldButtons = new();
        private static HashSet<byte> _pressedButtons = new();
        private static HashSet<byte> _releasedButtons = new();

        // Called from SDL EventCallback
        public static void ProcessEvent(Event e)
        {
            switch ((EventType)e.Type)
            {
                case EventType.MouseButtonDown:
                    if (_heldButtons.Add(e.Button.Button))
                        _pressedButtons.Add(e.Button.Button);
                    break;

                case EventType.MouseButtonUp:
                    if (_heldButtons.Remove(e.Button.Button))
                        _releasedButtons.Add(e.Button.Button);
                    break;
            }
        }

        public static void Update()
        {
            ReadOnlySpan<bool> keys = SDL.GetKeyboardState(out _);
            for (int i = 0; i < _keyboardState.Length; i++)
                _keyboardState[i] = keys[i];
        }

        // Called ONCE per frame
        public static void EndFrame()
        {
            _pressedButtons.Clear();
            _releasedButtons.Clear();
        }

        // Queries
        public static bool IsKeyDown(KeyCode key) => _keyboardState[(uint)key];
        public static bool IsKeyReleased(KeyCode key) => !_keyboardState[(uint)key];

        public static bool IsMouseButtonDown(MouseButton b) => _heldButtons.Contains((byte)b);
        public static bool IsMouseButtonPressed(MouseButton b) => _pressedButtons.Contains((byte)b);
        public static bool IsMouseButtonReleased(MouseButton b) => _releasedButtons.Contains((byte)b);

        public static Vector2 GetMousePosition()
        {
            float x, y;
            SDL.GetMouseState(out x, out y);
            return new Vector2(x, y);
        }
    }

}
