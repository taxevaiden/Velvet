using System.Drawing;
using System.Numerics;

using Boist.Graphics;

namespace Boist.Tests
{
    /// <summary>
    /// A stress test application for Boist, designed to test the performance of the renderer under heavy load by simulating a large number of moving particles on the screen.
    /// </summary>
    class StressTest : Application
    {
        private List<Vector2> particles;
        private List<Vector2> velocities;
        private Random random;
        private uint _vertexCapacity = 1024 * 1024 * 4 / Vertex.SizeInBytes;
        private uint _indexCapacity = 1024 * 1024 * 6 / sizeof(uint);

        public StressTest(GraphicsAPI graphicsAPI, int width = 1600, int height = 900, string title = "Stress Test")
            : base(width, height, title, graphicsAPI)
        {
            particles = new List<Vector2>();
            velocities = new List<Vector2>();
            random = new Random();
        }

        protected override void OnInit(int argc, string[]? argv)
        {
            // Prepopulate some particles
            for (int i = 0; i < 100; i++)
            {
                AddParticle();
            }
        }

        protected override void Update()
        {
            // Add new particles
            for (int i = 0; i < 25; i++)
            {
                AddParticle();
            }

            // Move particles in parallel
            Parallel.For(0, particles.Count, i =>
            {
                particles[i] += velocities[i];
                // Bounds handling
                ClampParticle(i);
            });

            Console.WriteLine($"Frame Data:\n\t{1 / DeltaTime:F1} FPS\n\tParticle count: {particles.Count}\n\tVertices: {particles.Count * 4}\n\tIndices: {particles.Count * 6}\n\tVertex Data Size: {particles.Count * 4 * 4} bytes ( {(particles.Count * 4 * 4) / (1024.0f * 1024.0f):F2} MB )\n\tIndex Data Size: {particles.Count * 6 * 4} bytes ( {(particles.Count * 6 * 4) / (1024.0f * 1024.0f):F2} MB )\n\tTotal Data Size: {(particles.Count * 4 * 4) + (particles.Count * 6 * 4)} bytes ( {((particles.Count * 4 * 4) + (particles.Count * 6 * 4)) / (1024.0f * 1024.0f):F2} MB )\n\n\tVertex Buffer Capacity: {_vertexCapacity} vertices ( {(_vertexCapacity * Vertex.SizeInBytes) / (1024.0f * 1024.0f):F2} MB )\n\tIndex Buffer Capacity: {_indexCapacity} indices ( {(_indexCapacity * sizeof(uint)) / (1024.0f * 1024.0f):F2} MB )");
        }

        private void ClampParticle(int i)
        {
            if (particles[i].X < 0 || particles[i].X > Window.Width)
            {
                velocities[i] = new Vector2(-velocities[i].X, velocities[i].Y);
                particles[i] = new Vector2(Math.Clamp(particles[i].X, 0, Window.Width), particles[i].Y);
            }
            if (particles[i].Y < 0 || particles[i].Y > Window.Height)
            {
                velocities[i] = new Vector2(velocities[i].X, -velocities[i].Y);
                particles[i] = new Vector2(particles[i].X, Math.Clamp(particles[i].Y, 0, Window.Height));
            }
        }

        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(Color.Black);

            // Draw particles
            for (int i = 0; i < particles.Count; i++)
            {
                Renderer.DrawCircle(
                    particles[i],
                    20.0f,
                    3,
                    Color.FromArgb(255, i * 325 % 255, i * 412 % 255, i * 176 % 255)
                );
            }

            Renderer.End();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            particles.Clear();
            velocities.Clear();
        }

        private void AddParticle()
        {
            particles.Add(new Vector2(random.Next(0, Window.Width), random.Next(0, Window.Height)));
            velocities.Add(new Vector2((float)(random.NextDouble() * 20.0 - 10.0),
                                       (float)(random.NextDouble() * 20.0 - 10.0)));
        }
    }
}