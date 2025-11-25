using System.Drawing;
using System.Numerics;
using System.Security.Principal;
using Velvet.Graphics;
using Velvet.Input;

namespace Velvet.Tests
{
    class MultiWindowTest : BaseTest
    {
        public MultiWindowTest() {}
        public override void Run(RendererAPI rendererAPI)
        {
            var winA = new VelvetWindow("one window", 500, 500);
            var rendererA = new Renderer(rendererAPI, winA);
            var winB = new VelvetWindow("two window", 500, 500);
            var rendererB = new Renderer(rendererAPI, winB);

            while (winA.Running && winB.Running)
            {
                winA.PollEvents();  

                rendererA.Begin();
                rendererA.ClearColor(Color.Red);
                rendererA.DrawRectangle(Vector2.One * 50.0f, Vector2.One * 400.0f, Color.Green);
                rendererA.End();

                winB.PollEvents();

                rendererB.Begin();
                rendererB.ClearColor(Color.Blue);
                rendererB.DrawRectangle(Vector2.One * 50.0f, Vector2.One * 400.0f, Color.Orange);
                rendererB.End();
            }

            rendererA.Dispose();
            rendererB.Dispose();
            winA.Dispose();
            winB.Dispose();
        }
    }
}