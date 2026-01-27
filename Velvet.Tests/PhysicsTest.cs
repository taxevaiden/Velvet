using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Velvet.Graphics;
using Velvet.Input;
using Vortice.Mathematics;

namespace Velvet.Tests
{
    class VerletPoint
    {
        public Vector2 Position;
        public Vector2 Previous;
        public bool Pinned;

        public VerletPoint(Vector2 pos, bool pinned = false)
        {
            Position = pos;
            Previous = pos;
            Pinned = pinned;
        }

        public void Update(float dt, float prevDT, Vector2 gravity)
        {
            if (Pinned) return;

            Vector2 velocity = Position - Previous;
            Previous = Position;
            Position += velocity * (dt / prevDT) + gravity * dt * (dt + prevDT) * 0.5f;
        }
    }

    struct VerletStick
    {
        public int A;
        public int B;
        public float Length;

        public VerletStick(int a, int b, float length)
        {
            A = a;
            B = b;
            Length = length;
        }
    }

    struct LookupEntry : IComparable<LookupEntry>
    {
        public int index;
        public uint cellKey;

        public LookupEntry(int i, uint key)
        {
            index = i;
            cellKey = key;
        }

        public int CompareTo(LookupEntry other) => cellKey.CompareTo(other.cellKey);
    }

    class PhysicsTest : VelvetApplication
    {
        // Max capacities
        const int MaxPoints = 4096;
        const int MaxSticks = 8192;
        const int MaxSpatialEntries = MaxPoints;

        private VerletPoint[] points = new VerletPoint[MaxPoints];
        private int pointCount;

        private VerletStick[] sticks = new VerletStick[MaxSticks];
        private int stickCount;

        private LookupEntry[] spatialLookup = new LookupEntry[MaxSpatialEntries];
        private int spatialCount;

        private int[] startIndices = new int[MaxSpatialEntries];

        readonly Int2[] cellOffests =
        {
            new(-1, -1), new(0, -1), new(1, -1),
            new(-1,  0), new(0,  0), new(1,  0),
            new(-1,  1), new(0,  1), new(1,  1),
        };

        Vector2 gravity = new(0, 981);
        const int ConstraintIterations = 10;
        const float Radius = 8f;

        private float prevDT = 1 / 240.0f;
        private int pointHeldIndex = -1;

        public PhysicsTest(GraphicsAPI api, int w = 1280, int h = 720)
            : base(w, h, "Verlet Test", api) { }

        (int x, int y) PostionToCellCoord(Vector2 point, float radius)
        {
            int cellX = (int)MathF.Floor(point.X / radius);
            int cellY = (int)MathF.Floor(point.Y / radius);
            return (cellX, cellY);
        }

        uint HashCell(int cellX, int cellY)
        {
            unchecked
            {
                uint x = (uint)(cellX * 73856093);
                uint y = (uint)(cellY * 19349663);
                return x ^ y;
            }
        }

        uint GetKeyFromHash(uint hash) => hash % (uint)pointCount;

        void UpdateSpatialLookup(float radius)
        {
            spatialCount = 0;

            Parallel.For(0, pointCount, i =>
            {
                startIndices[i] = int.MaxValue;
            });

            Parallel.For(0, pointCount, i =>
            {
                var (cx, cy) = PostionToCellCoord(points[i].Position, radius);
                uint key = GetKeyFromHash(HashCell(cx, cy));
                spatialLookup[spatialCount++] = new LookupEntry(i, key);
            });

            Array.Sort(spatialLookup, 0, spatialCount);

            Parallel.For(0, spatialCount, i =>
            {
                uint key = spatialLookup[i].cellKey;
                uint prev = i == 0 ? uint.MaxValue : spatialLookup[i - 1].cellKey;
                if (key != prev)
                    startIndices[key] = i;
            });

        }

        protected override void OnInit()
        {
            base.OnInit();

            // // Simple rope
            // Vector2 start = new(Window.GetWidth() * 0.5f, 50);
            // float spacing = Radius * 2 + 1.0f;

            // for (int i = 0; i < 128; i++)
            // {
            //     bool pinned = i == 0;
            //     points[pointCount++] = new VerletPoint(
            //         start + Vector2.UnitY * spacing * i + Vector2.UnitX * Random.Shared.Next(-1, 1),
            //         pinned
            //     );

            //     if (i > 0)
            //         sticks[stickCount++] = new VerletStick(i - 1, i, spacing);
            // }

            int startingPoints = 1000;
            int sideLength = (int)MathF.Floor(MathF.Sqrt(startingPoints));

            for (int i = 0; i < startingPoints; i++)
            {
                Vector2 start = new Vector2(i % sideLength, MathF.Floor(i / (float)sideLength)) * ((Radius + 2) * 2) + Vector2.UnitX * ((Window.GetWidth() - sideLength * (Radius + 2) * 2) * 0.5f) + Vector2.UnitY * ((Window.GetWidth() - sideLength * (Radius + 2) * 2) * 0.5f);
                start += Vector2.UnitX * Random.Shared.Next(-1, 1) + Vector2.UnitY * Random.Shared.Next(-1, 1);
                points[pointCount++] = new VerletPoint(
                    start,
                    false
                );
            }
        }

        protected override void Update(float dt)
        {

            InputManager.GetMousePosition(out float mx, out float my);

            Parallel.For(0, pointCount, j =>
                {
                    ref var p = ref points[j];

                    if (InputManager.IsMouseButtonDown(MouseButton.Left) && pointHeldIndex == -1)
                    {
                        // if ((new Vector2(mx, my) - p.Position).Length() < Radius)
                        //     pointHeldIndex = j;
                        Vector2 m = new Vector2(mx, my);
                        float dist = MathF.Max(0, 500 - (m - p.Previous).Length()) / 500;
                        p.Previous -= Vector2.Normalize(m - p.Previous) * dist * 1000.0f * dt * dt;
                    }
                    // else if (InputManager.IsMouseButtonReleased(MouseButton.Left))
                    //     pointHeldIndex = -1;

                    if (InputManager.IsMouseButtonPressed(MouseButton.Side1) && pointHeldIndex == j)
                        p.Pinned = !p.Pinned;

                    if (InputManager.IsMouseButtonPressed(MouseButton.Middle))
                        p.Previous += new Vector2(Random.Shared.Next(-25, 25), Random.Shared.Next(-25, 25));

                    InputManager.GetMouseScroll(out float sx, out float sy);
                    p.Previous += new Vector2(sx, sy);


                    p.Update(dt, prevDT, gravity);
                });


            if (pointHeldIndex >= 0)
            {
                InputManager.GetMousePosition(out float x, out float y);
                points[pointHeldIndex].Position = new Vector2(x, y);
            }

            UpdateSpatialLookup(Radius * 4);

            for (int i = 0; i < ConstraintIterations; i++)
                SolveConstraints();

            prevDT = dt;
            Console.WriteLine($"{1 / dt:F1} FPS");
        }

        void SolveConstraints()
        {
            // Stick constraints
            for (int i = 0; i < stickCount; i++)
            {
                ref var s = ref sticks[i];
                ref var a = ref points[s.A];
                ref var b = ref points[s.B];

                Vector2 delta = b.Position - a.Position;
                float dist = delta.Length();
                if (dist < 0.01f) continue;

                float diff = (dist - s.Length) / dist;

                if (a.Pinned)
                    b.Position -= delta * diff;
                else if (b.Pinned)
                    a.Position += delta * diff;
                else
                {
                    Vector2 half = delta * 0.5f * diff;
                    a.Position += half;
                    b.Position -= half;
                }
            }

            // Point collisions
            for (int i = 0; i < pointCount; i++)
            {
                ref var p = ref points[i];

                if (InputManager.IsMouseButtonDown(MouseButton.Right) && pointHeldIndex == -1)
                {
                    InputManager.GetMousePosition(out float mx, out float my);
                    Vector2 delta = new Vector2(mx, my) - p.Position;
                    float dist = delta.Length();
                    if (dist < 50f)
                        p.Position -= Vector2.Normalize(delta) * (50f - dist);
                }

                var (cx, cy) = PostionToCellCoord(p.Position, Radius * 4);

                foreach (var offset in cellOffests)
                {
                    uint key = GetKeyFromHash(HashCell(cx + offset.X, cy + offset.Y));
                    int start = startIndices[key];
                    if (start == int.MaxValue) continue;

                    for (int j = start; j < spatialCount && spatialLookup[j].cellKey == key; j++)
                    {
                        int idx = spatialLookup[j].index;
                        if (idx == i) continue;

                        ref var op = ref points[idx];
                        Vector2 delta = p.Position - op.Position;
                        float dist = delta.Length();
                        if (dist < 0.01f || dist >= Radius * 2) continue;

                        float diff = (dist - Radius * 2) / dist;
                        Vector2 change = delta * diff;

                        if (p.Pinned)
                            op.Position += change;
                        else if (op.Pinned)
                            p.Position -= change;
                        else
                        {
                            Vector2 half = change * 0.5f;
                            op.Position += half;
                            p.Position -= half;
                        }
                    }
                }

                // Clamp bounds
                p.Position.X = Math.Clamp(p.Position.X, 10, Window.GetWidth() - 10);
                p.Position.Y = Math.Clamp(p.Position.Y, 10, Window.GetHeight() - 10);
            }
        }

        protected override void Draw()
        {
            Renderer.Begin();
            Renderer.ClearColor(System.Drawing.Color.White);

            for (int i = 0; i < stickCount; i++)
            {
                var s = sticks[i];
                Renderer.DrawLine(points[s.A].Position, points[s.B].Position, 3f, System.Drawing.Color.Black);
            }

            for (int i = 0; i < pointCount; i++)
            {
                Renderer.DrawCircle(points[i].Position, Radius, 12, points[i].Pinned ? System.Drawing.Color.Red : System.Drawing.Color.Blue);
            }

            InputManager.GetMousePosition(out float mx, out float my);
            Renderer.DrawCircle(new Vector2(mx, my), 6f, System.Drawing.Color.Black);

            Renderer.End();
        }
    }
}
