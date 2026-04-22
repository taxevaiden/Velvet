using System.Drawing;
using System.Numerics;

using Velvet;
using Velvet.Graphics;
using Velvet.Input;
using Velvet.Windowing;

namespace Velvet.Tests
{
    /// <summary>
    /// A base test application for Velvet. Every test uses this structure.
    /// </summary>
    class BaseTest : VelvetApplication
    {
        public BaseTest(GraphicsAPI graphicsAPI, int width = 1600, int height = 900, string title = "Window")
            : base(width, height, title, graphicsAPI)
        { }
    }
}