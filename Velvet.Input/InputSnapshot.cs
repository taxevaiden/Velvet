using System.Numerics;

namespace Velvet.Input
{
    /// <summary>
    /// A snapshot of the input state at a given moment, including keyboard keys, mouse buttons, mouse position, and scroll delta.
    /// </summary>
    public struct InputSnapshot
    {
        /// <summary>
        /// An array indicating which keys are currently held down. Indexed by <see cref="KeyCode"/> values.
        /// </summary>
        public bool[] KeysDown;
        /// <summary>
        /// An array indicating which keys were not held down in the current frame. Indexed by <see cref="KeyCode"/> values.
        /// </summary>
        public bool[] KeysUp;
        /// <summary>
        /// An array indicating which keys were pressed in the current frame. Indexed by <see cref="KeyCode"/> values.
        /// </summary>
        public bool[] KeysPressed;
        /// <summary>
        /// An array indicating which keys were released in the current frame. Indexed by <see cref="KeyCode"/> values.
        /// </summary>
        public bool[] KeysReleased;
        /// <summary>
        /// An array indicating which mouse buttons are currently held down. Indexed by <see cref="MouseButton"/> values.
        /// </summary>
        public bool[] MouseButtonsDown;
        /// <summary>
        /// An array indicating which mouse buttons were not held down in the current frame. Indexed by <see cref="MouseButton"/> values.
        /// </summary>
        public bool[] MouseButtonsUp;
        /// <summary>
        /// An array indicating which mouse buttons were pressed in the current frame. Indexed by <see cref="MouseButton"/> values.
        /// </summary>
        public bool[] MouseButtonsPressed;
        /// <summary>
        /// An array indicating which mouse buttons were released in the current frame. Indexed by <see cref="MouseButton"/> values.
        /// </summary>
        public bool[] MouseButtonsReleased;
        /// <summary>
        /// The current mouse position in window coordinates as a <see cref="Vector2"/>.
        /// </summary>
        public Vector2 MousePosition;
        /// <summary>
        /// How far the mouse moved since the last frame in window coordinates as a <see cref="Vector2"/>.
        /// </summary>
        public Vector2 MouseDelta;
        /// <summary>
        /// How far the mouse wheel scrolled since the last frame in window coordinates as a <see cref="Vector2"/>.
        /// </summary>
        public Vector2 ScrollDelta;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSnapshot"/> struct with the specified input state.
        /// </summary>
        /// <param name="keysDown"></param>
        /// <param name="keysUp"></param>
        /// <param name="keysPressed"></param>
        /// <param name="keysReleased"></param>
        /// <param name="mouseButtonsDown"></param>
        /// <param name="mouseButtonsUp"></param>
        /// <param name="mouseButtonsPressed"></param>
        /// <param name="mouseButtonsReleased"></param>
        /// <param name="mousePosition"></param>
        /// <param name="mouseDelta"></param>
        /// <param name="scrollDelta"></param>
        public InputSnapshot(bool[] keysDown, bool[] keysUp, bool[] keysPressed, bool[] keysReleased,
                      bool[] mouseButtonsDown, bool[] mouseButtonsUp, bool[] mouseButtonsPressed, bool[] mouseButtonsReleased,
                      Vector2 mousePosition, Vector2 mouseDelta, Vector2 scrollDelta)
        {
            KeysDown = keysDown;
            KeysUp = keysUp;
            KeysPressed = keysPressed;
            KeysReleased = keysReleased;
            MouseButtonsDown = mouseButtonsDown;
            MouseButtonsUp = mouseButtonsUp;
            MouseButtonsPressed = mouseButtonsPressed;
            MouseButtonsReleased = mouseButtonsReleased;
            MousePosition = mousePosition;
            MouseDelta = mouseDelta;
            ScrollDelta = scrollDelta;
        }
    }
}