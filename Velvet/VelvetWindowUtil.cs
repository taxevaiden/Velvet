using SDL3;

namespace Velvet
{
    public partial class VelvetWindow
    {
        /// <summary>
        /// Returns if the window is currently running.
        /// </summary>
        /// <returns>bool</returns>
        public bool IsRunning() { return _running; }

        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        /// <returns>The width of the window.</returns>
        public int GetWidth()
        {
            SDL.GetWindowSizeInPixels(windowPtr, out int w, out int _);
            return w;
        }

        /// <summary>
        /// Gets the height of the window.
        /// </summary>
        /// <returns>The height of the window.</returns>
        public int GetHeight()
        {
            SDL.GetWindowSizeInPixels(windowPtr, out _, out int h);
            return h;
        }

        /// <summary>
        /// Gets the delta time.
        /// </summary>
        /// <returns>The delta time, in seconds.</returns>
        public float GetDeltaTime() { return _deltaTime; }
    }
}