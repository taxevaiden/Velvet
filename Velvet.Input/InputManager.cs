using System.Numerics;
using System.Runtime.CompilerServices;

using SDL3;

namespace Velvet.Input
{


    /// <summary>
    /// Provides access to keyboard and mouse input state.
    /// All state reflects the current frame; call <see cref="EndFrame"/> at the end of each frame to advance it.
    /// </summary>
    public class InputManager
    {
        private readonly bool[] _keyboardState = new bool[(uint)SDL.Scancode.Count];
        private readonly bool[] _prevKeyboardState = new bool[(uint)SDL.Scancode.Count];

        private readonly bool[] _buttonState = new bool[5];
        private readonly bool[] _prevButtonState = new bool[5];

        private float _mouseX, _mouseY;
        private float _prevMouseX, _prevMouseY;
        private float _scrollX, _scrollY;

        // Internal event / frame hooks

        // TODO: Abstract this so that it can be used with other windowing systems besides SDL
        /// <summary>
        /// Processes an SDL event
        /// </summary>
        /// <param name="e"></param>
        public void ProcessEvent(SDL.Event e)
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
                    _buttonState[e.Button.Button - 1] = true;
                    break;

                case SDL.EventType.MouseButtonUp:
                    _buttonState[e.Button.Button - 1] = false;
                    break;
            }
        }

        /// <summary>
        /// Updates the input state for the current frame. Should be called once per frame before any input queries.
        /// </summary>
        public void Update()
        {
            // Snapshot previous input state.
            Array.Copy(_keyboardState, _prevKeyboardState, _keyboardState.Length);
            Array.Copy(_buttonState, _prevButtonState, _buttonState.Length);

            ReadOnlySpan<bool> keys = SDL.GetKeyboardState(out _);
            for (int i = 0; i < _keyboardState.Length; i++)
                _keyboardState[i] = keys[i];
        }

        /// <summary>
        /// Advances the input state to the next frame. Should be called once per frame after all input queries.
        /// </summary>
        public void EndFrame()
        {
            _prevMouseX = _mouseX;
            _prevMouseY = _mouseY;
            _scrollX = 0f;
            _scrollY = 0f;
        }

        // Keyboard

        /// <summary>Returns true while the key is held down.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyDown(KeyCode key) => _keyboardState[(uint)key];

        /// <summary>Returns true while the key is not held down.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyUp(KeyCode key) => !_keyboardState[(uint)key];

        /// <summary>Returns true on the first frame the key is pressed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyPressed(KeyCode key)
            => _keyboardState[(uint)key] && !_prevKeyboardState[(uint)key];

        /// <summary>Returns true on the first frame the key is released.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyReleased(KeyCode key)
            => !_keyboardState[(uint)key] && _prevKeyboardState[(uint)key];

        /// <summary>Returns true if any of the specified keys are held down.</summary>
        public bool IsAnyKeyDown(params KeyCode[] keys)
        {
            foreach (var key in keys)
                if (IsKeyDown(key)) return true;
            return false;
        }

        /// <summary>Returns true if all of the specified keys are held down simultaneously.</summary>
        public bool AreAllKeysDown(params KeyCode[] keys)
        {
            foreach (var key in keys)
                if (!IsKeyDown(key)) return false;
            return true;
        }

        // Mouse buttons

        /// <summary>Returns true while the mouse button is held down.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMouseButtonDown(MouseButton b) => _buttonState[(byte)b];

        /// <summary>Returns true while the mouse button is not held down.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMouseButtonUp(MouseButton b) => !_buttonState[(byte)b];

        /// <summary>Returns true on the first frame the mouse button is pressed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMouseButtonPressed(MouseButton b) => _buttonState[(byte)b] && !_prevButtonState[(byte)b];

        /// <summary>Returns true on the first frame the mouse button is released.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMouseButtonReleased(MouseButton b) => !_buttonState[(byte)b] && _prevButtonState[(byte)b];

        // Mouse position

        /// <summary>Current mouse position in window coordinates as a <see cref="Vector2"/>.</summary>
        public Vector2 MousePosition => new(_mouseX, _mouseY);

        /// <summary>Mouse position from the previous frame as a <see cref="Vector2"/>.</summary>
        public Vector2 PreviousMousePosition => new(_prevMouseX, _prevMouseY);

        /// <summary>
        /// How far the mouse moved since the last frame as a <see cref="Vector2"/>.
        /// Useful for camera look, drag operations, etc.
        /// </summary>
        public Vector2 MouseDelta => new(_mouseX - _prevMouseX, _mouseY - _prevMouseY);

        /// <summary>Gets the current mouse position in window coordinates.</summary>
        public void GetMousePosition(out float x, out float y) { x = _mouseX; y = _mouseY; }

        /// <summary>Gets the mouse position from the previous frame in window coordinates.</summary>
        public void GetPreviousMousePosition(out float x, out float y) { x = _prevMouseX; y = _prevMouseY; }

        /// <summary>Gets how far the mouse moved since the last frame in window coordinates.</summary>
        public void GetMouseDelta(out float x, out float y) { x = _mouseX - _prevMouseX; y = _mouseY - _prevMouseY; }

        // Mouse scroll

        /// <summary>Mouse scroll delta for the current frame as a <see cref="Vector2"/>.</summary>
        public Vector2 ScrollDelta => new(_scrollX, _scrollY);

        /// <summary>Gets the mouse scroll delta for the current frame.</summary>
        public void GetScrollDelta(out float x, out float y) { x = _scrollX; y = _scrollY; }

        /// <summary>
        /// Returns a snapshot of the current input state, including keyboard keys, mouse buttons, mouse position, and scroll delta.
        /// </summary>
        public InputSnapshot GetSnapshot()
        {
            var keysDown = (bool[])_keyboardState.Clone();

            var keysUp = new bool[keysDown.Length];
            var keysPressed = new bool[keysDown.Length];
            var keysReleased = new bool[keysDown.Length];

            for (int i = 0; i < keysDown.Length; i++)
            {
                keysUp[i] = !keysDown[i];
                keysPressed[i] = keysDown[i] && !_prevKeyboardState[i];
                keysReleased[i] = !keysDown[i] && _prevKeyboardState[i];
            }

            const int mouseButtonCount = 5; // MouseButton values are 0-5

            var mouseDown = new bool[mouseButtonCount];
            var mouseUp = new bool[mouseButtonCount];
            var mousePressed = new bool[mouseButtonCount];
            var mouseReleased = new bool[mouseButtonCount];

            foreach (MouseButton button in Enum.GetValues<MouseButton>())
            {
                int i = (int)button;

                mouseDown[i] = IsMouseButtonDown(button);
                mouseUp[i] = IsMouseButtonUp(button);
                mousePressed[i] = IsMouseButtonPressed(button);
                mouseReleased[i] = IsMouseButtonReleased(button);
            }

            return new InputSnapshot(
                keysDown,
                keysUp,
                keysPressed,
                keysReleased,
                mouseDown,
                mouseUp,
                mousePressed,
                mouseReleased,
                MousePosition,
                MouseDelta,
                ScrollDelta);
        }
    }
}