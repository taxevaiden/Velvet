using System.Diagnostics.Tracing;
using System.Numerics;
using SDL3;
using static SDL3.SDL;

namespace Velvet.Input
{
    public enum KeyCode
    {
        // Letters
        A = 'a', B = 'b', C = 'c', D = 'd', E = 'e', F = 'f', G = 'g', H = 'h', I = 'i', J = 'j',
        K = 'k', L = 'l', M = 'm', N = 'n', O = 'o', P = 'p', Q = 'q', R = 'r', S = 's', T = 't',
        U = 'u', V = 'v', W = 'w', X = 'x', Y = 'y', Z = 'z',

        // Numbers (Top Row)
        D0 = '0', D1 = '1', D2 = '2', D3 = '3', D4 = '4',
        D5 = '5', D6 = '6', D7 = '7', D8 = '8', D9 = '9',

        // Punctuation
        Return = '\r',
        Escape = 27,
        Backspace = '\b',
        Tab = '\t',
        Space = ' ',

        Minus = '-',
        Equals = '=',
        LeftBracket = '[',
        RightBracket = ']',
        Backslash = '\\',
        Semicolon = ';',
        Quote = '\'',
        Grave = '`',
        Comma = ',',
        Period = '.',
        Slash = '/',

        // Function keys
        F1 = 1073741882,
        F2, F3, F4, F5, F6,
        F7, F8, F9, F10, F11, F12,

        // Control keys
        Insert = 1073741897,
        Home, PageUp, Delete, End, PageDown,
        Right = 1073741903, Left, Down, Up,

        // Modifier keys
        LeftCtrl = 1073742048,
        LeftShift, LeftAlt, LeftGui,
        RightCtrl, RightShift, RightAlt, RightGui,

        // Numpad
        NumLockClear = 1073741907,
        NumpadDivide, NumpadMultiply, NumpadMinus, NumpadPlus,
        NumpadEnter, Numpad1, Numpad2, Numpad3, Numpad4,
        Numpad5, Numpad6, Numpad7, Numpad8, Numpad9, Numpad0,
        NumpadPeriod,

        // Other
        CapsLock = 1073741881,
        PrintScreen = 1073741894,
        ScrollLock = 1073741895,
        Pause = 1073741896,
        Menu = 1073741942,

        Unknown = 0
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
        private static List<Event> _events = [];
        private static HashSet<uint> _heldKeys = [];
        private static HashSet<byte> _heldButtons = [];

        /// <summary>
        /// Clears the event list in the InputManager.
        /// </summary>
        public static void ClearEvents() { _events.Clear(); }

        /// <summary>
        /// Polls for currently pending events and processes them. Necessary for receiving input from the keyboard and mouse.
        /// </summary>
        /// <param name="e"></param>
        public static void PollEvent(Event e)
        {
            _events.Add(e);

            if (e.Type == (uint)EventType.KeyDown && !_heldKeys.Contains((uint)e.Key.Key))
                _heldKeys.Add((uint)e.Key.Key);
            else if (e.Type == (uint)EventType.KeyUp && _heldKeys.Contains((uint)e.Key.Key))
                _heldKeys.Remove((uint)e.Key.Key);

            if (e.Type == (uint)EventType.MouseButtonDown && !_heldButtons.Contains(e.Button.Button))
                _heldButtons.Add(e.Button.Button);
            else if (e.Type == (uint)EventType.MouseButtonUp && _heldButtons.Contains(e.Button.Button))
                _heldButtons.Remove(e.Button.Button);
        }

        /// <summary>
        /// Returns if a key has been pressed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A bool.</returns>
        public static bool IsKeyPressed(KeyCode key)
        {
            foreach (Event e in _events)
            {
                if (e.Type == (uint)EventType.KeyDown && ((uint)e.Key.Key) == ((uint)key)) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Returns if a mouse button has been pressed
        /// </summary>
        /// <param name="mouseButton"></param>
        /// <returns>A bool</returns>
        public static bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            foreach (Event e in _events)
            {
                if (e.Type == (uint)EventType.MouseButtonDown && e.Button.Button == (byte)mouseButton) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Returns if a key is being pressed down.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A bool.</returns>
        public static bool IsKeyDown(KeyCode key) => _heldKeys.Contains((uint)key);
        /// <summary>
        /// Returns if a key is not being pressed down.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsKeyUp(KeyCode key) => !_heldKeys.Contains((uint)key);

        /// <summary>
        /// Returns if a mouse button is being pressed down.
        /// </summary>
        /// <param name="mouseButton"></param>
        /// <returns>A bool.</returns>
        public static bool IsMouseButtonDown(MouseButton mouseButton) => _heldButtons.Contains((byte)mouseButton);
        /// <summary>
        /// Returns if a mouse button is not being pressed down.
        /// </summary>
        /// <param name="mouseButton"></param>
        /// <returns>A bool.</returns>
        public static bool IsMouseButtonUp(MouseButton mouseButton) => !_heldButtons.Contains((byte)mouseButton);

        /// <summary>
        /// Returns the current mouse position.
        /// </summary>
        /// <returns>A Vector2.</returns>
        public static Vector2 GetMousePosition()
        {
            float x, y;
            GetMouseState(out x, out y);
            return new Vector2(x, y);
        }

    }
}
