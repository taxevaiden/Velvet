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
            // State changes are resolved by creating a new batch for the next draw command.
            // No additional action is required in the new per-batch data model.
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureSpaceFor(int verticesNeeded, int indicesNeeded, VelvetRenderTexture? renderTarget)
        {
            if (_vertexCount + verticesNeeded > _vertexCapacity ||
                _indexCount + indicesNeeded > _indexCapacity)
                throw new InvalidOperationException("Frame vertex/index data exceeds buffer capacity.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Batch GetCurrentBatch(VelvetRenderTexture? renderTarget)
        {
            if (_batches.Count > 0)
            {
                Batch last = _batches[^1];
                if (last.Texture == CurrentTexture &&
                    last.Shader == CurrentShader &&
                    last.RenderTarget == renderTarget)
                {
                    return last;
                }
            }

            var batch = new Batch(CurrentTexture, CurrentShader, renderTarget);
            _batches.Add(batch);
            return batch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendVertex(Batch batch, Vertex vertex)
        {
            batch.EnsureCapacity(1, 0);
            batch.Vertices[batch.VertexCount++] = vertex;
            _vertexCount++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendIndex(Batch batch, uint index)
        {
            batch.EnsureCapacity(0, 1);
            batch.Indices[batch.IndexCount++] = index;
            _indexCount++;
        }
    }
}