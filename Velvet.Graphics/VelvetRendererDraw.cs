using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using Veldrid;

using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;

namespace Velvet.Graphics
{
    public partial class VelvetRenderer : IDisposable
    {
        /// <summary>
        /// The default texture used when no texture is applied. This is a 1x1 white pixel.
        /// </summary>
        public VelvetTexture DefaultTexture { get; internal set; } = null!;
        internal VelvetTexture CurrentTexture = null!;
        internal VelvetRenderTexture? CurrentRenderTarget = null;
        /// <summary>
        /// The default shader used when no shader is applied. This is a simple shader that supports textured and colored rendering.
        /// </summary>
        public VelvetShader DefaultShader { get; internal set; } = null!;
        private VelvetShader CurrentShader = null!;
        private List<Batch> _batches = new();

        private Vector2 _cachedRenderSize = Vector2.Zero;
        private Matrix3x2 _cachedProjection;

        // Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 GetAnchor(AnchorPosition anchor) => anchor switch
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsRectangleVisible(Vector2 pos, Vector2 size)
        {
            Vector2 rs = GetRenderSize();
            return pos.X + size.X > 0f && pos.X < rs.X &&
                   pos.Y + size.Y > 0f && pos.Y < rs.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Matrix3x2 GetProjection()
        {
            Vector2 viewport = GetRenderSize();
            if (viewport != _cachedRenderSize)
            {
                _cachedRenderSize = viewport;
                _cachedProjection = Matrix3x2.CreateScale(2f / viewport.X, 2f / viewport.Y);
                _cachedProjection *= Matrix3x2.CreateTranslation(-1f, -1f);
                _cachedProjection *= Matrix3x2.CreateScale(1f, -1f);
            }
            return _cachedProjection;
        }

        // Vertex translation

        // 3D methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 TranslateVertex(Vector3 pos)
            => new Vector3(Vector2.Transform(new Vector2(pos.X, pos.Y), GetProjection()), pos.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 TranslateVertex(Vector3 pos, Vector2 anchor, float rotation)
        {
            Vector2 pos2D = new Vector2(pos.X, pos.Y) - anchor;

            float c = MathF.Cos(rotation);
            float s = MathF.Sin(rotation);
            pos2D = new Vector2(pos2D.X * c - pos2D.Y * s, pos2D.X * s + pos2D.Y * c);

            pos2D += anchor;
            return new Vector3(Vector2.Transform(pos2D, GetProjection()), pos.Z);
        }

        // 2D methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 TranslateVertex(Vector2 pos)
            => new Vector3(Vector2.Transform(new Vector2(pos.X, pos.Y), GetProjection()), 0.0f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 TranslateVertex(Vector2 pos, Vector2 anchor, float rotation)
        {
            Vector2 pos2D = new Vector2(pos.X, pos.Y) - anchor;

            float c = MathF.Cos(rotation);
            float s = MathF.Sin(rotation);
            pos2D = new Vector2(pos2D.X * c - pos2D.Y * s, pos2D.X * s + pos2D.Y * c);

            pos2D += anchor;
            return new Vector3(Vector2.Transform(pos2D, GetProjection()), 0.0f);
        }

        // UV helpers

        private (Vector2 uvPos, Vector2 uvSize) NormaliseUV(Rectangle uv, bool flipY)
        {
            var texSize = new Vector2(CurrentTexture.Width, CurrentTexture.Height);
            Vector2 uvPos = new Vector2(uv.X, uv.Y) / texSize;
            Vector2 uvSize = new Vector2(uv.Width, uv.Height) / texSize;

            if (flipY)
            {
                uvPos.Y = 1f - uvPos.Y; uvSize.Y = -uvSize.Y;
            }

            return (uvPos, uvSize);
        }

        // Draw Commands

        /// <summary>Draws a rectangle.</summary>
        public void DrawRectangle(Vector2 pos, Vector2 size, RgbaColor color)
            => DrawRectangle(pos, size, GetFullUV(), 0f, AnchorPosition.TopLeft, color);

        /// <summary>Draws a rectangle with rotation.</summary>
        public void DrawRectangle(Vector2 pos, Vector2 size, float rotation, RgbaColor color)
            => DrawRectangle(pos, size, GetFullUV(), rotation, AnchorPosition.TopLeft, color);

        /// <summary>Draws a rectangle with rotation and an anchor.</summary>
        public void DrawRectangle(Vector2 pos, Vector2 size, float rotation, AnchorPosition anchor, RgbaColor color)
            => DrawRectangle(pos, size, GetFullUV(), rotation, anchor, color);

        /// <summary>Draws a rectangle with a custom UV region.</summary>
        public void DrawRectangle(
            Vector2 pos, Vector2 size, Rectangle uv,
            float rotation, AnchorPosition anchor, RgbaColor color)
        {
            bool flipY = CurrentTexture.FromRenderTexture &&
                         _graphicsDevice.BackendType == GraphicsBackend.OpenGL;

            var (uvPos, uvSize) = NormaliseUV(uv, flipY);

            Vector2 anchorW = pos + GetAnchor(anchor) * size;

            EnsureSpaceFor(4, 6, CurrentRenderTarget);

            uint baseIndex = (uint)_vertexCount;

            _vertices[_vertexCount++] = new Vertex(TranslateVertex(pos, anchorW, rotation), uvPos, color);
            _vertices[_vertexCount++] = new Vertex(TranslateVertex(pos + size * Vector2.UnitY, anchorW, rotation), uvPos + Vector2.UnitY * uvSize, color);
            _vertices[_vertexCount++] = new Vertex(TranslateVertex(pos + size, anchorW, rotation), uvPos + uvSize, color);
            _vertices[_vertexCount++] = new Vertex(TranslateVertex(pos + size * Vector2.UnitX, anchorW, rotation), uvPos + Vector2.UnitX * uvSize, color);

            _indices[_indexCount++] = baseIndex;
            _indices[_indexCount++] = baseIndex + 1;
            _indices[_indexCount++] = baseIndex + 2;
            _indices[_indexCount++] = baseIndex + 2;
            _indices[_indexCount++] = baseIndex + 3;
            _indices[_indexCount++] = baseIndex;
        }

        /// <summary>Draws a circle with an automatic segment count based on radius.</summary>
        public void DrawCircle(Vector2 pos, float radius, RgbaColor color)
        {
            int segments = Math.Max(12, (int)(MathF.Sqrt(radius) * 4f));
            DrawCircle(pos, radius, segments, color);
        }

        /// <summary>Draws a circle with a fixed segment count.</summary>
        public void DrawCircle(Vector2 pos, float radius, int segments, RgbaColor color)
        {
            if (segments < 3) segments = 3;

            EnsureSpaceFor(segments + 1, segments * 3, CurrentRenderTarget);

            uint baseIndex = (uint)_vertexCount;

            _vertices[_vertexCount++] = new Vertex(
                TranslateVertex(pos), new Vector2(0.5f, 0.5f), color);

            float step = MathF.Tau / segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = step * i;
                Vector2 dir = new(MathF.Cos(angle), MathF.Sin(angle));
                _vertices[_vertexCount++] = new Vertex(
                    TranslateVertex(pos + dir * radius),
                    new Vector2(0.5f, 0.5f) + dir * 0.5f,
                    color);
            }

            for (int i = 0; i < segments; i++)
            {
                _indices[_indexCount++] = baseIndex;
                _indices[_indexCount++] = baseIndex + 1 + (uint)((i + 1) % segments);
                _indices[_indexCount++] = baseIndex + 1 + (uint)i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (Vector2 min, Vector2 max) GetVertexBounds(Vertex[] vertices)
        {
            Vector2 min = new Vector2(vertices[0].Position.X, vertices[0].Position.Y);
            Vector2 max = min;
            for (int i = 1; i < vertices.Length; i++)
            {
                Vector2 p = new Vector2(vertices[i].Position.X, vertices[i].Position.Y);
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
            return (min, max);
        }

        /// <summary>Draws an arbitrary polygon.</summary>
        public void DrawPolygon(Vector3 pos, Vertex[] vertices, uint[] indices)
            => DrawPolygonInternal(pos, vertices, indices, null);

        /// <summary>Draws an arbitrary polygon with a color override.</summary>
        public void DrawPolygon(Vector3 pos, Vertex[] vertices, uint[] indices, RgbaColor color)
            => DrawPolygonInternal(pos, vertices, indices, color);

        private void DrawPolygonInternal(Vector3 pos, Vertex[] vertices, uint[] indices, RgbaColor? colorOverride)
        {
            if (vertices.Length == 0 || indices.Length == 0) return;

            Vector2 pos2D = new Vector2(pos.X, pos.Y);

            var (min, max) = GetVertexBounds(vertices);
            if (!IsRectangleVisible(pos2D + min, max - min)) return;

            uint baseIndex = (uint)_vertexCount;

            EnsureSpaceFor(vertices.Length, indices.Length, CurrentRenderTarget);

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 xy = new Vector2(vertices[i].Position.X, vertices[i].Position.Y);
                _vertices[_vertexCount++] = new Vertex(
                    TranslateVertex(new Vector3(pos2D + xy, pos.Z + vertices[i].Position.Z)),
                    vertices[i].UV,
                    colorOverride ?? vertices[i].Color
                );
            }

            for (int i = 0; i < indices.Length; i++)
                _indices[_indexCount++] = baseIndex + indices[i];
        }
        /// <summary>Draws a thick line between two points.</summary>
        public void DrawLine(Vector2 a, Vector2 b, float thickness, RgbaColor color)
        {
            Vector2 min = Vector2.Min(a, b) - Vector2.One * thickness;
            Vector2 max = Vector2.Max(a, b) + Vector2.One * thickness;
            if (!IsRectangleVisible(min, max - min)) return;

            Vector2 dir = b - a;
            float length = dir.Length();
            if (length < 0.0001f) return;

            float rot = MathF.Atan2(dir.Y, dir.X) - MathF.PI * 0.5f;
            DrawRectangle(a, new Vector2(thickness, length), rot, AnchorPosition.Top, color);
        }

        /// <summary>Draws a string using a <see cref="VelvetFont"/>.</summary>
        public void DrawText(VelvetFont font, string text, int pxSize, Vector2 position, RgbaColor color)
        {
            if (string.IsNullOrEmpty(text)) return;

            VelvetTexture previousTex = CurrentTexture;
            ApplyTexture(font.TextureAtlas);

            float scale = pxSize / (float)font.FontSize;
            float x = position.X;
            float y = position.Y;

            foreach (char c in text)
            {
                if (c < 0 || c >= 128) continue;
                var glyph = font.glyphs[c];
                var uv = new Rectangle(glyph.x0, glyph.y0, glyph.x1 - glyph.x0, glyph.y1 - glyph.y0);
                DrawRectangle(
                    new Vector2(x + glyph.x_off * scale, y - glyph.y_off * scale),
                    new Vector2(uv.Width, uv.Height) * scale,
                    uv, 0f, AnchorPosition.TopLeft, color);
                x += glyph.advance * scale;
            }

            ApplyTexture(previousTex);
        }

        // State management

        /// <summary>Sets the current render target to a <see cref="VelvetRenderTexture"/>.</summary>
        public void SetRenderTarget(VelvetRenderTexture rt)
        {
            if (CurrentRenderTarget == rt) return;
            FlushIfPending();
            CurrentRenderTarget = rt;
        }

        /// <summary>Sets the current render target to the screen.</summary>
        public void SetRenderTargetToScreen()
        {
            if (CurrentRenderTarget == null) return;
            FlushIfPending();
            CurrentRenderTarget = null;
        }

        /// <summary>Applies a texture for subsequent draw calls. If null, the default white texture is used.</summary>
        public void ApplyTexture(VelvetTexture? texture = null)
        {
            texture ??= DefaultTexture;
            if (CurrentTexture == texture) return;
            FlushIfPending();
            CurrentTexture = texture;
        }

        /// <summary>Applies a shader for subsequent draw calls. If null, the default shader is used.</summary>
        public void ApplyShader(VelvetShader? shader = null)
        {
            shader ??= DefaultShader;
            if (CurrentShader == shader) return;
            FlushIfPending();
            CurrentShader = shader;
        }

        // Buffer / batch management

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushIfPending()
        {
            if (_vertexCount - _lastFlushedVertexCount > 0 &&
                _indexCount - _lastFlushedIndexCount > 0)
                Flush(CurrentRenderTarget);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureSpaceFor(int verticesNeeded, int indicesNeeded, VelvetRenderTexture? renderTarget)
        {
            if (_vertexCount + verticesNeeded > _vertexCapacity ||
                _indexCount + indicesNeeded > _indexCapacity)
                Flush(renderTarget);
        }

        private void Flush(VelvetRenderTexture? renderTarget)
        {
            int vertexCount = _vertexCount - _lastFlushedVertexCount;
            int indexCount = _indexCount - _lastFlushedIndexCount;
            if (vertexCount <= 0 || indexCount <= 0) return;

            if (_batches.Count > 0)
            {
                Batch last = _batches[_batches.Count - 1];
                if (last.Texture == CurrentTexture &&
                    last.Shader == CurrentShader &&
                    last.RenderTarget == renderTarget)
                {
                    last.VertexCount += vertexCount;
                    last.IndexCount += indexCount;
                    _batches[_batches.Count - 1] = last;
                    _lastFlushedVertexCount = _vertexCount;
                    _lastFlushedIndexCount = _indexCount;
                    return;
                }
            }

            _batches.Add(new Batch(
                _lastFlushedVertexCount, vertexCount,
                _lastFlushedIndexCount, indexCount,
                CurrentTexture, CurrentShader, renderTarget));

            _lastFlushedVertexCount = _vertexCount;
            _lastFlushedIndexCount = _indexCount;
        }

        // Frame lifecycle

        /// <summary>
        /// Begins a new frame. Must be called before any draw calls, and must be
        /// paired with a call to <see cref="End"/> at the end of the frame.
        /// </summary>
        public void Begin()
        {
            CurrentTexture = DefaultTexture;
            CurrentRenderTarget = null;
            CurrentShader = DefaultShader;

            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        }

        /// <summary>Clears the current render target to a solid color.</summary>
        public void ClearColor(RgbaColor color)
        {
            _commandList.SetFramebuffer(
                CurrentRenderTarget?.Framebuffer ?? _graphicsDevice.SwapchainFramebuffer);
            _commandList.ClearColorTarget(0, ToRgbaFloat(color));
        }

        /// <summary>
        /// Ends the current frame, submitting all draw calls to the GPU. Must be
        /// called once per frame, paired with <see cref="Begin"/>.
        /// </summary>
        public void End()
        {
            SubmitBatches();

            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers();

            _vertexCount = 0;
            _indexCount = 0;
            _lastFlushedVertexCount = 0;
            _lastFlushedIndexCount = 0;
            _batches.Clear();
        }

        // Batch submission

        private void SubmitBatches()
        {
            FlushIfPending();
            if (_batches.Count == 0) return;

            if (_vertexCount > 0)
                _graphicsDevice.UpdateBuffer(
                    _vertexBuffer, 0,
                    ref _vertices[0],
                    (uint)(_vertexCount * (int)Vertex.SizeInBytes));

            if (_indexCount > 0)
                _graphicsDevice.UpdateBuffer(
                    _indexBuffer, 0,
                    ref _indices[0],
                    (uint)(_indexCount * sizeof(uint)));

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);

            for (int bi = 0; bi < _batches.Count; bi++)
            {
                Batch batch = _batches[bi];

                batch.Texture.GenerateMipMapsIfNeeded(_commandList);

                batch.Shader.SetTexture(batch.Texture);
                batch.Shader.SetRenderTexture(batch.RenderTarget);
                batch.Shader.Flush();

                if (batch.RenderTarget != null)
                {
                    _commandList.SetFramebuffer(batch.RenderTarget.Framebuffer);
                    _commandList.SetViewport(0, new Viewport(
                        0, 0, batch.RenderTarget.Width, batch.RenderTarget.Height, 0, 1));
                    _commandList.SetScissorRect(0, 0, 0,
                        batch.RenderTarget.Width, batch.RenderTarget.Height);
                }
                else
                {
                    var fb = _graphicsDevice.SwapchainFramebuffer;
                    _commandList.SetFramebuffer(fb);
                    _commandList.SetViewport(0, new Viewport(0, 0, fb.Width, fb.Height, 0, 1));
                    _commandList.SetScissorRect(0, 0, 0, fb.Width, fb.Height);
                }

                _commandList.SetPipeline(batch.Shader.Pipeline);
                _commandList.SetGraphicsResourceSet(0, batch.Shader.ResourceSet);

                _commandList.DrawIndexed(
                    indexCount: (uint)batch.IndexCount,
                    instanceCount: 1,
                    indexStart: (uint)batch.IndexStart,
                    vertexOffset: 0,
                    instanceStart: 0);

                if (batch.RenderTarget?.IsMultiSampled == true)
                    batch.RenderTarget.Resolve(_commandList);

                batch.Texture.MipMapsGenerated = false;
            }
        }
    }
}