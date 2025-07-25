using SDL3;

namespace Velvet
{
    public partial class VelvetWindow
    {
        public bool IsRunning() { return _running; }

        public int GetWidth()
        {
            SDL.GetWindowSizeInPixels(windowPtr, out int w, out int _);
            return w;
        }

        public int GetHeight()
        {
            SDL.GetWindowSizeInPixels(windowPtr, out _, out int h);
            return h;
        }

    }
}