using Veldrid;
using System.Numerics;
using System.Drawing;

namespace Velvet.Graphics
{
    public partial class VelvetRenderer : IDisposable
    {
        public VelvetTexture DefaultTexture { get; internal set; } = null!;
        internal VelvetTexture CurrentTexture = null!;
        internal VelvetRenderTexture? CurrentRenderTarget = null!;
        public VelvetShader DefaultShader { get; internal set; } = null!;
        private VelvetShader CurrentShader = null!;
        private uint _vertexOff = 0;
        private uint _indexOff = 0;
        private List<Batch> _batches = null!;

        private static Vector2 GetAnchor(AnchorPosition anchor)
        {
            return anchor switch
            {
                AnchorPosition.TopLeft => new Vector2(0f, 0f),
                AnchorPosition.Top => new Vector2(0.5f, 0f),
                AnchorPosition.TopRight => new Vector2(1f, 0f),

                AnchorPosition.Left => new Vector2(0f, 0.5f),
                AnchorPosition.Center => new Vector2(0.5f, 0.5f),
                AnchorPosition.Right => new Vector2(1f, 0.5f),

                AnchorPosition.BottomLeft => new Vector2(0f, 1f),
                AnchorPosition.Bottom => new Vector2(0.5f, 1f),
                AnchorPosition.BottomRight => new Vector2(1f, 1f),

                _ => Vector2.Zero
            };
        }
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="pos">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        public void DrawRectangle(Vector2 pos, Vector2 size, System.Drawing.Color color)
        {
            DrawRectangle(pos, size, 0.0f, AnchorPosition.TopLeft, color);
        }

        /// <summary>
        /// Draws a rectangle, rotated around the top-left of the rectangle.
        /// </summary>
        /// <param name="pos">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="rotation">The rotation of the rectangle, in radians.</param>
        /// <param name="color">The color of the rectangle.</param>
        public void DrawRectangle(Vector2 pos, Vector2 size, float rotation, System.Drawing.Color color)
        {
            DrawRectangle(pos, size, rotation, AnchorPosition.TopLeft, color);
        }

        /// <summary>
        /// Draws a rectangle, rotated around the specified anchor.
        /// </summary>
        /// <param name="pos">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="rotation">The rotation of the rectangle, in radians.</param>
        /// <param name="anchor">Where the rectangle is rotated around.</param>
        /// <param name="color">The color of the rectangle.</param>
        public void DrawRectangle(Vector2 pos, Vector2 size, float rotation, AnchorPosition anchor, System.Drawing.Color color)
        {
            if (CurrentTexture.FromRenderTexture && _graphicsDevice.BackendType == GraphicsBackend.OpenGL)
            {
                _vertices.Add(new Vertex(TranslateVertex(pos, pos + GetAnchor(anchor) * size, rotation), new Vector2(0.0f, 1.0f), ToRgbaFloat(color)));
                _vertices.Add(new Vertex(TranslateVertex(pos + size * Vector2.UnitY, pos + GetAnchor(anchor) * size, rotation), new Vector2(0.0f, 0.0f), ToRgbaFloat(color)));
                _vertices.Add(new Vertex(TranslateVertex(pos + size, pos + GetAnchor(anchor) * size, rotation), new Vector2(1.0f, 0.0f), ToRgbaFloat(color)));
                _vertices.Add(new Vertex(TranslateVertex(pos + size * Vector2.UnitX, pos + GetAnchor(anchor) * size, rotation), new Vector2(1.0f, 1.0f), ToRgbaFloat(color)));
            }
            else
            {
                _vertices.Add(new Vertex(TranslateVertex(pos, pos + GetAnchor(anchor) * size, rotation), new Vector2(0.0f, 0.0f), ToRgbaFloat(color)));
                _vertices.Add(new Vertex(TranslateVertex(pos + size * Vector2.UnitY, pos + GetAnchor(anchor) * size, rotation), new Vector2(0.0f, 1.0f), ToRgbaFloat(color)));
                _vertices.Add(new Vertex(TranslateVertex(pos + size, pos + GetAnchor(anchor) * size, rotation), new Vector2(1.0f, 1.0f), ToRgbaFloat(color)));
                _vertices.Add(new Vertex(TranslateVertex(pos + size * Vector2.UnitX, pos + GetAnchor(anchor) * size, rotation), new Vector2(1.0f, 0.0f), ToRgbaFloat(color)));
            }

            int baseIndex = _vertices.Count - 4;

            _indices.Add((uint)(baseIndex + 0));
            _indices.Add((uint)(baseIndex + 1));
            _indices.Add((uint)(baseIndex + 2));
            _indices.Add((uint)(baseIndex + 2));
            _indices.Add((uint)(baseIndex + 3));
            _indices.Add((uint)(baseIndex + 0));
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="pos">The position of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        public void DrawCircle(Vector2 pos, float radius, System.Drawing.Color color)
        {
            int segments = Math.Max(12, (int)(radius / 50) * (int)(radius / 50 * 2.5));
            DrawCircle(pos, radius, segments, color);
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="pos">The position of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="segments">The amount of segments that make up the circle.</param>
        /// <param name="color">The color of the circle.</param>
        public void DrawCircle(Vector2 pos, float radius, int segments, System.Drawing.Color color)
        {
            int baseIndex = _vertices.Count;
            for (int i = 0; i < segments; i++)
            {
                Vector2 dir = new Vector2(MathF.Sin(360.0f / segments * i * DEG2RAD), MathF.Cos(360.0f / segments * i * DEG2RAD));
                _vertices.Add(new Vertex(TranslateVertex(pos + dir * radius), Vector2.One * 0.5f + dir / 2, ToRgbaFloat(color)));
                _indices.Add((uint)(baseIndex + 0));
                _indices.Add((uint)(baseIndex + i));
                _indices.Add((uint)(baseIndex + (i + 1) % segments));
            }
        }

        /// <summary>
        /// Draws a polygon.
        /// </summary>
        /// <param name="pos">The position of the polygon.</param>
        /// <param name="vertices">An array of vertices to render.</param>
        /// <param name="indices">An array of indices that determine how the vertices connect.</param>
        /// <param name="color">The color of the polygon.</param>
        public void DrawPolygon(Vector2 pos, Vector2[] vertices, uint[] indices, System.Drawing.Color color)
        {
            int baseIndex = _vertices.Count;
            for (int i = 0; i < vertices.Length; i++)
            {
                // TODO: Implement UVs for Polygons so textures can be added onto them
                _vertices.Add(new Vertex(TranslateVertex(pos + vertices[i]), Vector2.Zero, ToRgbaFloat(color)));
            }

            for (int i = 0; i < indices.Length; i++)
            {
                _indices.Add((uint)(baseIndex + indices[i]));
            }
        }

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="a">The starting position of the line.</param>
        /// <param name="b">The ending position of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawLine(Vector2 a, Vector2 b, float thickness, Color color)
        {
            Vector2 dir = b - a;
            float length = dir.Length();
            float rot = MathF.Atan2(dir.Y, dir.X) - MathF.PI * 0.5f;

            DrawRectangle(
                a,
                new Vector2(thickness, length),
                rot,
                AnchorPosition.Top,
                color
            );
        }

        private Vector2 TranslateVertex(Vector2 pos)
        {
            Vector2 viewport = GetRenderSize();
            Matrix3x2 projection = Matrix3x2.CreateScale(2f / viewport.X, 2f / viewport.Y);
            projection *= Matrix3x2.CreateTranslation(-1f, -1f);

            pos = Vector2.Transform(pos, projection);

            return pos;
        }

        private Vector2 TranslateVertex(Vector2 pos, Vector2 anchor, float rotation)
        {
            pos -= anchor;

            float c = MathF.Cos(rotation);
            float s = MathF.Sin(rotation);

            Matrix3x2 rot = new Matrix3x2(
                c, s,
                -s, c,
                0f, 0f
            );

            pos = Vector2.Transform(pos, rot);
            pos += anchor;

            Vector2 viewport = GetRenderSize();
            Matrix3x2 projection = Matrix3x2.CreateScale(2f / viewport.X, 2f / viewport.Y);
            projection *= Matrix3x2.CreateTranslation(-1f, -1f);

            pos = Vector2.Transform(pos, projection);

            return pos;
        }

        /// <summary>
        /// Begins the command list. Call this before drawing anything.
        /// </summary>
        public void Begin()
        {
            CurrentTexture = DefaultTexture;
            CurrentRenderTarget = null;
            CurrentShader = DefaultShader;

            _commandList.Begin();

            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        }

        /// <summary>
        /// Sets the render target to a VelvetRenderTexture.
        /// </summary>
        public void SetRenderTarget(VelvetRenderTexture rt)
        {
            if (CurrentRenderTarget == rt)
                return;

            if (CurrentRenderTarget != rt)
                if (_vertices.Count > 0 && _indices.Count > 0)
                {
                    Flush(CurrentRenderTarget);
                }

            CurrentRenderTarget = rt;
        }

        /// <summary>
        /// Sets the render target to the screen.
        /// </summary>
        public void SetRenderTargetToScreen()
        {
            if (CurrentRenderTarget == null)
                return;

            if (CurrentRenderTarget != null)
                if (_vertices.Count > 0 && _indices.Count > 0)
                {
                    Flush(CurrentRenderTarget);
                }

            CurrentRenderTarget = null;
        }

        /// <summary>
        /// Renders everything to the screen. Call this after drawing.
        /// </summary>
        public void End()
        {
            SubmitBatches();

            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);

            _graphicsDevice.SwapBuffers();

            _vertices.Clear();
            _indices.Clear();
            _batches.Clear();

            _vertexOff = 0;
            _indexOff = 0;
        }

        private void SubmitBatches()
        {
            if (_vertices.Count > 0 && _indices.Count > 0)
                Flush(CurrentRenderTarget);

            foreach (Batch batch in _batches)
            {
                _commandList.GenerateMipmaps(batch.Texture.DeviceTexture);
                batch.Shader.SetTexture(batch.Texture);
                batch.Shader.SetRenderTexture(batch.RenderTarget);
                batch.Shader.Flush();

                _graphicsDevice.UpdateBuffer(_vertexBuffer, _vertexOff * Vertex.SizeInBytes, batch.Vertices);
                _graphicsDevice.UpdateBuffer(_indexBuffer, _indexOff * 4, batch.Indices);

                _commandList.SetVertexBuffer(0, _vertexBuffer);
                _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
                if (batch.RenderTarget != null)
                {
                    _commandList.SetFramebuffer(batch.RenderTarget.Framebuffer);
                    _commandList.SetViewport(
                        0,
                        new Viewport(
                            0,
                            0,
                            batch.RenderTarget.Width,
                            batch.RenderTarget.Height,
                            0,
                            1
                        )
                    );

                    _commandList.SetScissorRect(
                        0,
                        0,
                        0,
                        batch.RenderTarget.Width,
                        batch.RenderTarget.Height
                    );
                }
                else
                {
                    _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
                    _commandList.SetViewport(
                        0,
                        new Viewport(
                            0,
                            0,
                            _graphicsDevice.SwapchainFramebuffer.Width,
                            _graphicsDevice.SwapchainFramebuffer.Height,
                            0,
                            1
                        )
                    );

                    _commandList.SetScissorRect(
                        0,
                        0,
                        0,
                        _graphicsDevice.SwapchainFramebuffer.Width,
                        _graphicsDevice.SwapchainFramebuffer.Height
                    );
                }
                _commandList.SetPipeline(batch.Shader.Pipeline);
                _commandList.SetGraphicsResourceSet(0, batch.Shader.ResourceSet);
                _commandList.DrawIndexed(
                indexCount: (uint)batch.Indices.Length,
                instanceCount: 1,
                indexStart: _indexOff,
                vertexOffset: (int)_vertexOff,
                instanceStart: 0);

                if (batch.RenderTarget != null && batch.RenderTarget.IsMultiSampled) batch.RenderTarget.Resolve(_commandList);
                _vertexOff += (uint)batch.Vertices.Length;
                _indexOff += (uint)batch.Indices.Length;
            }
        }

        private void Flush(VelvetRenderTexture? renderTarget = null)
        {
            if (!(_vertices.Count > 0)) return;

            Batch batch = new([.. _vertices], [.. _indices], CurrentTexture, CurrentShader, renderTarget);

            _batches.Add(batch);

            _vertices.Clear();
            _indices.Clear();
        }

        /// <summary>
        /// Applies a VelvetTexture.
        /// </summary>
        /// <remarks>When this is called, anything drawn has the provided VelvetTexture applied. To make anything drawn have a solid color again, call <code>ApplyTexture()</code></remarks>
        /// <param name="texture">The VelvetTexture to apply. Can be null.</param>
        public void ApplyTexture(VelvetTexture? texture = null)
        {
            texture ??= DefaultTexture;

            if (texture == CurrentTexture) return;

            if (CurrentTexture != texture)
                if (_vertices.Count > 0 && _indices.Count > 0)
                    Flush(CurrentRenderTarget);

            CurrentTexture = texture;

        }

        /// <summary>
        /// Applies a VelvetShader.
        /// </summary>
        /// <remarks>When this is called, anything drawn has the provided VelvetShader applied to it. **This does not apply the VelvetShader to the entire screen.** To make anything drawn use the default shader again, call <code>ApplyShader()</code></remarks>
        /// <param name="shader">The VelvetShader to apply. Can be null.</param>
        public void ApplyShader(VelvetShader? shader = null)
        {
            shader ??= DefaultShader;

            if (shader == CurrentShader) return;

            if (CurrentShader != shader)
                if (_vertices.Count > 0 && _indices.Count > 0)
                    Flush(CurrentRenderTarget);

            CurrentShader = shader;
        }

        /// <summary>
        /// Clears the screen to a color.
        /// </summary>
        /// <param name="color">The color to clear the screen to.</param>
        public void ClearColor(System.Drawing.Color color)
        {
            if (CurrentRenderTarget != null) _commandList.SetFramebuffer(CurrentRenderTarget.Framebuffer); else _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.ClearColorTarget(0, ToRgbaFloat(color));
        }
    }
}