using Boist.Graphics;

namespace Boist.Tests
{
    /// <summary>
    /// A base test application for Boist. Every test uses this structure.
    /// </summary>
    class BaseTest : Application
    {
        public BaseTest(GraphicsAPI graphicsAPI, int width = 1280, int height = 800, string title = "Window")
            : base(width, height, title, graphicsAPI)
        { }
    }
}