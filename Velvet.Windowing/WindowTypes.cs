using System.Numerics;

namespace Velvet.Windowing
{
    /// <summary>
    /// Flags that can be applied to a <see cref="Window"/>. 
    /// </summary>
    [Flags]
    public enum WindowFlags
    {
        /// <summary>
        /// The window will be used with OpenGL. This allows you to use its OpenGL functions, such as <see cref="Window.GetGLProcAddress"/>.
        /// Do not combine this with <see cref="Vulkan"/>.
        /// </summary>
        OpenGL = 1 << 0,
        /// <summary>
        /// The window will be used with Vulkan. Do not combine this with <see cref="OpenGL"/>. 
        /// </summary>
        Vulkan = 1 << 1,
        /// <summary>
        /// The window will be minimized.
        /// </summary>
        Minimized = 1 << 2,
        /// <summary>
        /// The window will be maximized.
        /// </summary>
        Maximized = 1 << 3,
        /// <summary>
        /// The window can be resized.
        /// </summary>
        Resizable = 1 << 4,
        /// <summary>
        /// The window has no decoration (no frame, no title bar).
        /// </summary>
        Borderless = 1 << 5,
        /// <summary>
        /// The window will be in fullscreen mode.
        /// </summary>
        Fullscreen = 1 << 6,
    }

    /// <summary>
    /// The types of <see cref="WindowEvent"/>s .
    /// </summary>
    public enum WindowEventType
    {
        /// <summary>
        /// The window was resized.
        /// </summary>
        WindowResized,
        /// <summary>
        /// The window was moved.
        /// </summary>
        WindowMoved,
        /// <summary>
        /// The window has been requested to close by the window manager.
        /// </summary>
        WindowCloseRequested,
        /// <summary>
        /// The window has been requested to close by the user.
        /// </summary>
        Quit,
        /// <summary>
        /// The mouse moved.
        /// </summary>
        MouseMotion,
        /// <summary>
        /// The mouse wheel moved.
        /// </summary>
        MouseWheel,
        /// <summary>
        /// A mouse button was pressed.
        /// </summary>
        MouseButtonDown,
        /// <summary>
        /// A mouse button was released.
        /// </summary>
        MouseButtonUp,
        /// <summary>
        /// A key was pressed.
        /// </summary>
        KeyDown,
        /// <summary>
        /// A key was released.
        /// </summary>
        KeyUp,
    }

    /// <summary>
    /// An event that is fired by a <see cref="Window"/>. You can obtain an instance of this event to process yourself through <see cref="Window.PollEvent(ref WindowEvent)"/> 
    /// </summary>
    public struct WindowEvent
    {
        /// <summary>
        /// The type of event.
        /// </summary>
        public WindowEventType Type;
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
        /// <summary>
        /// The position of the window.
        /// </summary>
        public Vector2 WindowPosition;
        /// <summary>
        /// The size of the window.
        /// </summary>
        public Vector2 WindowSize;
    }
}