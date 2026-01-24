using System.Drawing;
using System.Numerics;
using Velvet;
using Velvet.Graphics;
using Velvet.Windowing;
using Velvet.Input;

namespace Velvet.Tests
{
    class BaseTest : VelvetApplication
    {
        public BaseTest(GraphicsAPI graphicsAPI, int width = 1600, int height = 900, string title = "Window")
            : base(width, height, title, graphicsAPI)
        {}
    }
}