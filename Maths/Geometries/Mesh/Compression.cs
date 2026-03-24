using System;

namespace Common.Maths
{
    public enum CompressionVertices : byte
    {
        None = 0,
        HalfFloat = 1
    }
    public enum CompressionTransform : byte
    {
        None = 0,
        MatrixTRS = 1
    }
    public enum CompressionIndices : byte
    {
        None = 0,
        Encoded7Bit = 1
    }

    public enum CompressionNormals : byte
    {
        None = 0,
        /// <summary>
        /// lossy compression using my <see cref="UnitSphericalPacker16"/> it seems to work well.
        /// </summary>
        Normals16 = 1,
        /// <summary>
        /// lossy compression using my <see cref="UnitSphericalPacker24"/> it seems to work well.
        /// </summary>
        Normals24 = 2,
        /// <summary>
        /// lossy compression using <see cref="UnitVectorPacker32.EncodeX15Y15Z1(Vector3f)"/>.
        /// </summary>
        NormalsX15Y15Z1 = 3
    }
    public enum CompressionTexCoord : byte
    {
        None = 0,
        HalfFloat = 1
    }
    public enum CompressionColor : byte
    {
        None = 0,
        NoAlpha = 1
    }
}
