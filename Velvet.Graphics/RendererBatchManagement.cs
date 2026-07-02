using System.Runtime.CompilerServices;

using Velvet.Graphics.Shaders;
using Velvet.Graphics.Textures;

namespace Velvet.Graphics
{
    public partial class Renderer : IDisposable
    {
        // State management

        /// <summary>Sets the current render target to a <see cref="RenderTexture"/>.</summary>
        public void SetRenderTarget(RenderTexture rt)
        {
            if (CurrentRenderTarget == rt) return;
            CurrentRenderTarget = rt;
        }

        /// <summary>Sets the current render target to the screen.</summary>
        public void SetRenderTargetToScreen()
        {
            if (CurrentRenderTarget == null) return;
            CurrentRenderTarget = null;
        }

        /// <summary>Applies a texture for subsequent draw calls. If null, the default white texture is used.</summary>
        public void ApplyTexture(Texture? texture = null)
        {
            texture ??= DefaultTexture;
            if (CurrentTexture == texture) return;
            CurrentTexture = texture;
        }

        /// <summary>Applies a shader for subsequent draw calls. If null, the default shader is used.</summary>
        public void ApplyShader(Shader? shader = null)
        {
            shader ??= DefaultShader;
            if (CurrentShader == shader) return;
            CurrentShader = shader;
        }

        // Buffer / batch management

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Batch Flush()
        {
            var batch = new Batch(CurrentTexture, CurrentShader, CurrentRenderTarget);
            _batches.Add(batch);
            
            _vertexCount = 0;
            _indexCount = 0;
            return batch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureSpaceFor(int verticesNeeded, int indicesNeeded)
        {
            // Automatically flush the current batch if adding the new vertices and indices would eventually exceed the buffer size.
            // This allows for what would be a very large batch to be split into multiple smaller batches, which can then be submitted to the GPU without exceeding the buffer size.
            if ((_vertexCount + verticesNeeded) * Vertex.SizeInBytes > VertexBufferSize - 1024 * 1024 ||
                (_indexCount + indicesNeeded) * sizeof(uint) > IndexBufferSize - 1024 * 1024)
                Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Batch GetCurrentBatch(RenderTexture? renderTarget)
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
            
            return Flush();
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