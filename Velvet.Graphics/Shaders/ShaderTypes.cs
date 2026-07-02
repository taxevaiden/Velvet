namespace Velvet.Graphics.Shaders
{
    /// <summary>
    /// The type of a shader uniform. This is used to determine the size and alignment of the uniform in the shader's uniform buffer.
    /// </summary>
    public enum UniformType
    {
        /// <summary>
        /// A single float value.
        /// </summary>
        Float,
        /// <summary>
        /// A single int value.
        /// </summary>
        Int,
        /// <summary>
        /// A single uint value.
        /// </summary>
        UInt,
        /// <summary>
        /// A 2D vector of floats.
        /// </summary>
        Vector2,
        /// <summary>
        /// A 3D vector of floats. Note that in std140 layout, a vec3 takes up 16 bytes of space (the same as a vec4) to ensure proper alignment.
        /// </summary>
        Vector3,
        /// <summary>
        /// A 4D vector of floats.
        /// </summary>
        Vector4,
        /// <summary>
        /// A 4x4 matrix of floats, stored in column-major order. This takes up 64 bytes of space in the uniform buffer.
        /// </summary>
        Matrix4x4
    }

    /// <summary>
    /// A description of a shader uniform, including its name and type.
    /// </summary>
    /// <param name="Name">The name of the uniform.</param>
    /// <param name="Type">The type of the uniform.</param>
    public readonly record struct UniformDescription(
        string Name,
        UniformType Type);

    // Internal layout type

    internal readonly record struct PackedUniform(
        string Name,
        UniformType Type,
        uint Offset,
        uint Size);
}