namespace Boist.Graphics.Textures
{
    /// <summary>
    /// The number of samples to use for MSAA render textures. Higher sample counts can produce smoother edges at the cost of performance and VRAM usage. The default is Count1 (no MSAA).
    /// </summary>
    public enum SampleCount
    {
        /// <summary>
        /// No MSAA. This is the default sample count for render textures.
        /// </summary>
        Count1 = 0,
        /// <summary>
        /// 2x MSAA. This uses 2 samples per pixel for anti-aliasing.
        /// </summary>
        Count2 = 1,
        /// <summary>
        /// 4x MSAA. This uses 4 samples per pixel for anti-aliasing.
        /// </summary>
        Count4 = 2,
        /// <summary>
        /// 8x MSAA. This uses 8 samples per pixel for anti-aliasing.
        /// </summary>
        Count8 = 3,
        /// <summary>
        /// 16x MSAA. This uses 16 samples per pixel for anti-aliasing.
        /// </summary>
        Count16 = 4,
        /// <summary>
        /// 32x MSAA. This uses 32 samples per pixel for anti-aliasing. This is the highest sample count currently supported by Boist.
        /// </summary>
        Count32 = 5,
    }
}