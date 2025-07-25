using Veldrid;
using System.Numerics;
using System.Drawing;

namespace Velvet.Graphics
{
    public partial class Renderer
    {
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="pos">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        public void DrawRectangle(Vector2 pos, Vector2 size, System.Drawing.Color color)
        {
            _vertices.Add(new VertexPositionColor(pos, pos + size / 2, 0.0f, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size * Vector2.UnitY, pos + size / 2, 0.0f, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size, pos + size / 2, 0.0f, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size * Vector2.UnitX, pos + size / 2, 0.0f, PackColor(color)));

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
            _vertices.Add(new VertexPositionColor(pos, pos + size / 2, rotation, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size * Vector2.UnitY, pos + size / 2, rotation, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size, pos + size / 2, rotation, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size * Vector2.UnitX, pos + size / 2, rotation, PackColor(color)));

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
                _vertices.Add(new VertexPositionColor(pos + new Vector2(MathF.Sin(360.0f / segments * i * DEG2RAD) * radius, MathF.Cos(360.0f / segments * i * DEG2RAD) * radius), pos, 0.0f, PackColor(color)));
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
                _vertices.Add(new VertexPositionColor(pos + new Vector2(MathF.Sin(360.0f / segments * i * DEG2RAD) * radius, MathF.Cos(360.0f / segments * i * DEG2RAD) * radius), pos, 0.0f, PackColor(color)));
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
                _vertices.Add(new VertexPositionColor(pos + vertices[i], pos, 0.0f, PackColor(color)));
            }

            for (int i = 0; i < indices.Length; i++)
            {
                _indices.Add((uint)(baseIndex + indices[i]));
            }
        }

        /// <summary>
        /// Begins the command list. Call this before drawing anything.
        /// </summary>
        public void Begin()
        {
            _commandList.Begin();

            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);

            ResolutionData resolutionData = new(
                (uint)_window.GetWidth(),
                (uint)_window.GetHeight()
            );

            _graphicsDevice.UpdateBuffer(_uniformBuffer, 0, ref resolutionData);
        }

        /// <summary>
        /// Renders everything to the screen. Call this after drawing.
        /// </summary>
        public void End()
        {
            if (_vertices.Count > 0)
                _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertices.ToArray());

            if (_indices.Count > 0)
                _graphicsDevice.UpdateBuffer(_indexBuffer, 0, _indices.ToArray());

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetGraphicsResourceSet(0, _resourceSet);
            _commandList.DrawIndexed(
                indexCount: (uint)_indices.Count,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);

            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);

            _graphicsDevice.SwapBuffers();

            _vertices.Clear();
            _indices.Clear();
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