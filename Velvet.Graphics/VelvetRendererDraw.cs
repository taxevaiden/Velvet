using Veldrid;
using System.Numerics;
using System.Drawing;

namespace Velvet.Graphics
{
    public partial class Renderer : IDisposable
    {
        private VelvetTexture _defaultTexture;
        private VelvetTexture _currentTexture;
        private uint _indexOff = 0;

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="pos">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        public void DrawRectangle(Vector2 pos, Vector2 size, System.Drawing.Color color)
        {
            _vertices.Add(new Vertex(TranslateVertex(pos), new Vector2(0.0f, 0.0f), PackColor(color)));
            _vertices.Add(new Vertex(TranslateVertex(pos + size * Vector2.UnitY), new Vector2(0.0f, 1.0f), PackColor(color)));
            _vertices.Add(new Vertex(TranslateVertex(pos + size), new Vector2(1.0f, 1.0f), PackColor(color)));
            _vertices.Add(new Vertex(TranslateVertex(pos + size * Vector2.UnitX), new Vector2(1.0f, 0.0f), PackColor(color)));

            int baseIndex = _vertices.Count - 4;

            _indices.Add((uint)(baseIndex + 0));
            _indices.Add((uint)(baseIndex + 1));
            _indices.Add((uint)(baseIndex + 2));
            _indices.Add((uint)(baseIndex + 2));
            _indices.Add((uint)(baseIndex + 3));
            _indices.Add((uint)(baseIndex + 0));
        }

        /// <summary>
        /// Draws a rectangle, rotated around the center.
        /// </summary>
        /// <param name="pos">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="rotation">The rotation of the rectangle, in radians.</param>
        /// <param name="color">The color of the rectangle.</param>
        public void DrawRectangle(Vector2 pos, Vector2 size, float rotation, System.Drawing.Color color)
        {
            _vertices.Add(new Vertex(TranslateVertex(pos, pos + size / 2, rotation), new Vector2(0.0f, 0.0f), PackColor(color)));
            _vertices.Add(new Vertex(TranslateVertex(pos + size * Vector2.UnitY, pos + size / 2, rotation), new Vector2(0.0f, 1.0f), PackColor(color)));
            _vertices.Add(new Vertex(TranslateVertex(pos + size, pos + size / 2, rotation), new Vector2(1.0f, 1.0f), PackColor(color)));
            _vertices.Add(new Vertex(TranslateVertex(pos + size * Vector2.UnitX, pos + size / 2, rotation), new Vector2(1.0f, 0.0f), PackColor(color)));
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
            int baseIndex = _vertices.Count;
            for (int i = 0; i < segments; i++)
            {
                Vector2 dir = new Vector2(MathF.Sin(360.0f / segments * i * DEG2RAD), MathF.Cos(360.0f / segments * i * DEG2RAD));
                _vertices.Add(new Vertex(TranslateVertex(pos + dir * radius), Vector2.One * 0.5f + dir / 2, PackColor(color)));
                _indices.Add((uint)(baseIndex + 0));
                _indices.Add((uint)(baseIndex + i));
                _indices.Add((uint)(baseIndex + (i + 1) % segments));
            }
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
                _vertices.Add(new Vertex(TranslateVertex(pos + dir * radius), Vector2.One * 0.5f + dir / 2, PackColor(color)));
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
        /// <param name="indices">An array of indices that determine the drawing order of the vertices.</param>
        /// <param name="color">The color of the polygon.</param>
        public void DrawPolygon(Vector2 pos, Vector2[] vertices, uint[] indices, System.Drawing.Color color)
        {
            int baseIndex = _vertices.Count;
            for (int i = 0; i < vertices.Length; i++)
            {
                // TODO: Implement UVs for Polygons so textures can be added onto them
                _vertices.Add(new Vertex(TranslateVertex(pos + vertices[i]), Vector2.Zero, PackColor(color)));
            }

            for (int i = 0; i < indices.Length; i++)
            {
                _indices.Add((uint)(baseIndex + indices[i]));
            }
        }

        private Vector2 TranslateVertex(Vector2 pos)
        {
            Matrix3x2 projection = Matrix3x2.CreateScale(2f / _window.GetWidth(), 2f / _window.GetHeight());
            projection *= Matrix3x2.CreateTranslation(-1f, -1f);
            projection *= Matrix3x2.CreateScale(1.0f, -1.0f);

            pos = Vector2.Transform(pos, projection);

            _logger.Information(pos.ToString());

            return pos;
        }

        private Vector2 TranslateVertex(Vector2 pos, Vector2 anchor, float rotation)
        {
            pos -= anchor;

            float c = MathF.Cos(rotation);
            float s = MathF.Sin(rotation);

            Matrix3x2 rot = new Matrix3x2(
                c,
                s,
                -s,
                c,
                0f,
                0f
            );
            
            pos = Vector2.Transform(pos, rot);
            pos += anchor;

            Matrix3x2 projection = Matrix3x2.CreateScale(2f / _window.GetWidth(), 2f / _window.GetHeight());
            projection *= Matrix3x2.CreateTranslation(-1f, -1f);
            projection *= Matrix3x2.CreateScale(1.0f, -1.0f);

            pos = Vector2.Transform(pos, projection);

            _logger.Information(pos.ToString());

            return pos;
        }

        /// <summary>
        /// Begins the command list and clears the current texture applied. Call this before drawing anything.
        /// </summary>
        public void Begin()
        {
            _currentTexture = _defaultTexture;

            _commandList.Begin();

            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);

            ResolutionData resolutionData = new(
                (uint)_window.GetWidth(),
                (uint)_window.GetHeight()
            );
        }

        /// <summary>
        /// Renders everything to the screen. Call this after drawing.
        /// </summary>
        public void End()
        {
            if (_vertices.Count > 0 && _indices.Count > 0)
                Flush();

            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);

            _graphicsDevice.SwapBuffers();

            _vertices.Clear();
            _indices.Clear();

            _indexOff = 0;
        }

        private void Flush()
        {
            if (!(_vertices.Count > 0)) return;

            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertices.ToArray());
            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, _indices.ToArray());

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetGraphicsResourceSet(0, _currentTexture.ResourceSet);
            _commandList.DrawIndexed(
                indexCount: (uint)_indices.Count,
                instanceCount: 1,
                indexStart: _indexOff,
                vertexOffset: 0,
                instanceStart: 0);

            _indexOff = (uint)_indices.Count();
        }

        /// <summary>
        /// Applies a VelvetTexture.
        /// </summary>
        /// <remarks>When this is called, anything drawn has the provided VelvetTexture applied. To make anything drawn have a solid color again, call <code>ApplyTexture()</code></remarks>
        /// <param name="texture"></param>
        public void ApplyTexture(VelvetTexture? texture = null)
        {
            if (texture != null)
            {
                if (_currentTexture != texture)
                    if (_vertices.Count > 0 && _indices.Count > 0)
                        Flush();

                _currentTexture = texture;
            }
            else
            {
                if (_currentTexture != _defaultTexture)
                    if (_vertices.Count > 0 && _indices.Count > 0)
                        Flush();

                _currentTexture = _defaultTexture;
            }

        }

        /// <summary>
        /// Clears the screen to a color.
        /// </summary>
        /// <param name="color"></param>
        public void ClearColor(System.Drawing.Color color)
        {
            _commandList.ClearColorTarget(0, ToRgbaFloat(color));
        }
    }
}