using Veldrid;

namespace Velvet.Graphics
{
    public partial class VelvetRenderer : IDisposable
    {
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
        /// Ends the current frame, submitting all draw calls to the GPU. Must be called once per frame, paired with <see cref="Begin"/>.
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
                batch.Shader.SetProjection(GetProjection());
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