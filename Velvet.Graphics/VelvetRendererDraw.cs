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
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public void DrawRectangle(Vector2 pos, Vector2 size, System.Drawing.Color color)
        {
            _vertices.Add(new VertexPositionColor(pos, pos + size / 2, 0.0f, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size * Vector2.UnitY, pos + size / 2, 0.0f, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size, pos + size / 2, 0.0f, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size * Vector2.UnitX, pos + size / 2, 0.0f, PackColor(color)));

            int baseIndex = _vertices.Count - 4;

            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 0));
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="rotation"></param>
        /// <param name="color"></param>
        public void DrawRectangle(Vector2 pos, Vector2 size, float rotation, System.Drawing.Color color)
        {
            _vertices.Add(new VertexPositionColor(pos, pos + size / 2, rotation, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size * Vector2.UnitY, pos + size / 2, rotation, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size, pos + size / 2, rotation, PackColor(color)));
            _vertices.Add(new VertexPositionColor(pos + size * Vector2.UnitX, pos + size / 2, rotation, PackColor(color)));

            int baseIndex = _vertices.Count - 4;

            _indices.Add((ushort)(baseIndex + 0));
            _indices.Add((ushort)(baseIndex + 1));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 2));
            _indices.Add((ushort)(baseIndex + 3));
            _indices.Add((ushort)(baseIndex + 0));
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public void DrawCircle(Vector2 pos, float radius, System.Drawing.Color color)
        {
            int segments = 24;
            int baseIndex = _vertices.Count;
            for (int i = 0; i < segments; i++)
            {
                _vertices.Add(new VertexPositionColor(pos + new Vector2(MathF.Sin(360.0f / segments * i * DEG2RAD) * radius, MathF.Cos(360.0f / segments * i * DEG2RAD) * radius), pos, 0.0f, PackColor(color)));
                _indices.Add((ushort)(baseIndex + 0));
                _indices.Add((ushort)(baseIndex + i));
                _indices.Add((ushort)(baseIndex + (i + 1) % segments));
            }
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="segments"></param>
        /// <param name="color"></param>
        public void DrawCircle(Vector2 pos, float radius, int segments, System.Drawing.Color color)
        {
            int baseIndex = _vertices.Count;
            for (int i = 0; i < segments; i++)
            {
                _vertices.Add(new VertexPositionColor(pos + new Vector2(MathF.Sin(360.0f / segments * i * DEG2RAD) * radius, MathF.Cos(360.0f / segments * i * DEG2RAD) * radius), pos, 0.0f, PackColor(color)));
                _indices.Add((ushort)(baseIndex + 0));
                _indices.Add((ushort)(baseIndex + i));
                _indices.Add((ushort)(baseIndex + (i + 1) % segments));
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
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
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