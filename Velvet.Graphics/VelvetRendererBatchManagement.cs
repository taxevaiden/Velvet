using System.Runtime.CompilerServices;

using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;

namespace Velvet.Graphics
{
    public partial class VelvetRenderer : IDisposable
    {
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
    }
}