using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Common.Maths
{
    /// <summary>
    /// i tried to implement a similar method of
    /// https://www.sciencedirect.com/science/article/abs/pii/S0097849312000568
    /// </summary>
    /// <remarks>
    /// after testing performance for all different decoding variant i find the same value.
    /// </remarks>
    public static class UnitSphericalPacker16
    {
        const ushort MASK = 0x1FFF; //13 bit of index (8191)
        const int N = 126; // N=126 generates 8128 points

        /// <summary>
        /// 8127, maximum value of encoded n
        /// </summary>
        public const ushort MAX = (N + 3) * N / 2;

#if DEBUG
        static byte[] i_tab;
        public static void GeneratePrecomputedTab()
        {
            //mantain the precomputed table for debugging
            i_tab = new byte[MAX + 1];
            int n = 0;
            for (byte i = 0; i <= N; i++)
                for (byte j = 0; j <= i; j++)
                    i_tab[n++] = i;
        }
#endif
        /// <summary>
        /// Bhāskara I's sin
        /// </summary>
        static float Sin(float r)
        {
            float n = (Mathelp.PI - r) * r * 4;
            return n * 4 / (5 * Mathelp.PI * Mathelp.PI - n);
        }
        /// <summary>
        /// Bhāskara I's cos
        /// </summary>
        static float Cos(float r)
        {
            return (Mathelp.PI2 - 4 * r * r) / (Mathelp.PI2 + r * r);
        }
        /// <summary>
        /// Trey Reynolds' ArcCos
        /// </summary>
        static float Acos_TR(float x)
        {
            return (float)(8 / 3.0 * Math.Sqrt(2 - Math.Sqrt(2 + 2 * x)) - 1 / 3.0 * Math.Sqrt(2 - 2 * x));
        }
        /// <summary>
        /// Sebastien Lagarde's ArcCos
        /// </summary>
        static float Acos_SL(float x)
        {
            var z = Math.Abs(x);
            z = (float)((-0.168577f * z + 1.56723f) * Math.Sqrt(1 - z));
            return x < 0 ? Mathelp.PI - z : z;
        }
        /// <summary>
        /// </summary>
        /// <param name="x">for x [0,+inf]</param>
        static float Atan(float z, float x)
        {
            if (x < 1e-6f) return Mathelp.PI / 2;
            else x = z / x;
            x = (float)(8.1f * x / (3 + Math.Sqrt(25 + 80 * x * x / 3)));
            if (x > 2) x += 0.02f;
            return x;
        }
        /// <summary>
        /// Padè approximation
        /// </summary>
        static float Asin_P(float r)
        {
            return (float)(3.04 * r / (2 + Math.Sqrt(1 - r * r)));
        }
        /// <summary>
        /// ArcCos = π/2 - ArcSin 
        /// </summary>
        static float Acos_P(float r)
        {
            return Mathelp.Rad90 - Asin_P(r);
        }
        /// <summary>
        /// Separate sign and index
        /// </summary>
        static void masksign(ref ushort n, out int sign)
        {
            sign = n >> 13;
            n &= MASK;
        }

        /// <summary>
        /// convert to cartesian vector
        /// </summary>
        static Vector3f cartesian(int i, int j, int sign)
        {
            float phi = i * Mathelp.Rad90 / N;
            float theta = i > 0 ? j * Mathelp.Rad90 / i : 0;

            var sinphi = Math.Sin(phi);
            var normal = new Vector3f(Math.Cos(theta) * sinphi, Math.Cos(phi), Math.Sin(theta) * sinphi);

            if ((sign & 4) != 0) normal.x *= -1;
            if ((sign & 2) != 0) normal.y *= -1;
            if ((sign & 1) != 0) normal.z *= -1;
            return normal;
        }
        /// <summary>
        /// convert to cartesian vector using trigonometric approximation
        /// </summary>
        static Vector3f cartesian_approx(int i, int j, int sign)
        {
            float phi = i * Mathelp.Rad90 / N;
            float theta = i > 0 ? j * Mathelp.Rad90 / i : 0;

            float sin_phi = Sin(phi);

            var normal = new Vector3f(Cos(theta) * sin_phi, Cos(phi), Sin(theta) * sin_phi);

            if ((sign & 4) != 0) normal.x *= -1;
            if ((sign & 2) != 0) normal.y *= -1;
            if ((sign & 1) != 0) normal.z *= -1;
            return normal;
        }
        static Vector3f cartesian_linear(int i, int j, int sign)
        {
            float phi = i * Mathelp.Rad90 / N;
            float theta = i > 0 ? j * Mathelp.Rad90 / i : 0;

            float sin_phi = phi / Mathelp.Rad90;
            float sin_theta = theta / Mathelp.Rad90;
            float cos_phi = (Mathelp.Rad90 - phi) / Mathelp.Rad90;
            float cos_theta = (Mathelp.Rad90 - theta) / Mathelp.Rad90;

            var normal = new Vector3f(cos_theta * sin_phi, cos_phi, sin_theta * sin_phi);

            if ((sign & 4) != 0) normal.x *= -1;
            if ((sign & 2) != 0) normal.y *= -1;
            if ((sign & 1) != 0) normal.z *= -1;
            return normal;
        }

        static void spherical(ref Vector3f unitvector, out float theta, out float phi)
        {
            //unitvector.Normalize();
            theta = (float)Math.Atan2(unitvector.z, unitvector.x); //work for vector -z
            //float theta = (float)Math.Atan(cartesian.z /cartesian.x);
            phi = (float)Math.Acos(unitvector.y);
        }
        static void spherical_approx(ref Vector3f unitvector, out float theta, out float phi)
        {
            theta = Atan(unitvector.z , unitvector.x);
            phi = Acos_P(unitvector.y);
        }
        static void spherical_linear(ref Vector3f unitvector, out float theta, out float phi)
        {
            theta = Atan(unitvector.z, unitvector.x);
            phi = Acos_P(unitvector.y);
        }

        /// <summary>
        /// Summation of sequence for j=0, or Gauss formula, or triangular number, or...
        /// </summary>
        static int sum(int i) => (i + 1) * i / 2;

        /// <summary>
        /// inverse of summation (ideal inverse, with sqrt)
        /// </summary>
        static void inverse(int n, out int i, out int j)
        {
            //check for n[0,8127]
            i = (int)(Math.Sqrt(1 + 8 * n) - 1) / 2;
            j = n - sum(i);
        }
        /// <summary>
        /// <see cref="inverse"/> (with sqrt)
        /// </summary>
        static void inverse2(int n, out int i, out int j)
        {
            //check for n[0,8127]
            i = (int)(Math.Sqrt(n) * Mathelp.Sqrt2);
            j = n - sum(i);
            if (j < 0) { j += i; i--; }
        }
        /// <summary>
        /// <see cref="inverse"/> (without sqrt)
        /// </summary>
        static void inverse3(int n, out int i, out int j)
        {
            //check for n[0,8127]
            i = (int)(Math.Exp(0.5 * Math.Log(n)) * Mathelp.Sqrt2);
            j = n - sum(i);
            if (j < 0) { j += i; i--; }
        }


        public static ushort Encode(Vector3f normal)
        {
            ushort value = 0;
            if (normal.x < 0) { value |= 4; normal.x *= -1; }
            if (normal.y < 0) { value |= 2; normal.y *= -1; }
            if (normal.z < 0) { value |= 1; normal.z *= -1; }


            spherical(ref normal, out var theta, out var phi);

            int i = (int)Math.Round(phi * N / Mathelp.Rad90);
            int j = (int)Math.Round(theta * i / Mathelp.Rad90);
            int n = sum(i) + j;

            return (ushort)((value << 13) | n);

        }

        [Obsolete("little gap near +z axis, atan don't work whell for z = 1")]
        public static ushort EncodeApprox(Vector3f normal)
        {
            ushort value = 0;
            if (normal.x < 0) { value |= 4; normal.x *= -1; }
            if (normal.y < 0) { value |= 2; normal.y *= -1; }
            if (normal.z < 0) { value |= 1; normal.z *= -1; }

            spherical_approx(ref normal, out var theta, out var phi);

            int i = (int)Math.Round(phi * N / Mathelp.Rad90);
            int j = (int)Math.Round(theta * i / Mathelp.Rad90);
            int n = sum(i) + j;

            return (ushort)((value << 13) | n);
        }

        [Obsolete("just a test")]
        public static ushort EncodeApproxLinear(Vector3f normal)
        {
            ushort value = 0;
            if (normal.x < 0) { value |= 4; normal.x *= -1; }
            if (normal.y < 0) { value |= 2; normal.y *= -1; }
            if (normal.z < 0) { value |= 1; normal.z *= -1; }

            spherical_linear(ref normal, out var theta, out var phi);

            int i = (int)Math.Round(phi * N / Mathelp.Rad90);
            int j = (int)Math.Round(theta * i / Mathelp.Rad90);
            int n = sum(i) + j;

            return (ushort)((value << 13) | n);
        }

        /// <summary>
        /// Decode using trigonometric approximation. Respect <see cref="Decode(ushort)"/> the error is trascurable.
        /// </summary>
        public static Vector3f DecodeApprox(ushort value)
        {
            masksign(ref value, out int sign);
            inverse2(value, out int i, out int j);
            return cartesian_approx(i, j, sign);
        }
        [Obsolete("just a test")]
        public static Vector3f DecodeApproxLinear(ushort value)
        {
            masksign(ref value, out int sign);
            inverse2(value, out int i, out int j);
            return cartesian_linear(i, j, sign);
        }

        /// <summary>
        /// Decode without precomputed table, using ideal inverse function
        /// </summary>
        public static Vector3f Decode(ushort value)
        {
            masksign(ref value, out int sign);
            inverse(value, out int i, out int j);
            return cartesian(i, j, sign);
        }
    }

    /// <summary>
    /// Generalization of <see cref="UnitSphericalPacker16"/>
    /// </summary>
    public static class UnitSphericalPacker24
    {
        const int MASK = 0x1F_FFFF; //21 bit of index 2.097.151
        const int N = 2046;//N 2046 generates 2.096.128 points
        static ushort[] i_tab;

        /// <summary>
        /// 2096127, maximum value of encoded value n
        /// </summary>
        public const int MAX = (N + 3) * N / 2;

        public static void GeneratePrecomputedTab()
        {
            //mantain the precomputed table for debugging
            i_tab = new ushort[MAX + 1];
            int n = 0;
            for (ushort i = 0; i <= N; i++)
                for (ushort j = 0; j <= i; j++)
                    i_tab[n++] = i;
        }
        static void cartesianToSpherical(ref Vector3f unitvector, out float theta, out float phi)
        {
            theta = (float)Math.Atan2(unitvector.z, unitvector.x); //work for vector -z
            phi = (float)Math.Acos(unitvector.y);
        }
        static void sphericalToCartesian(ref float theta, ref float phi, out Vector3f unitvector)
        {
            unitvector = new Vector3f(Math.Cos(theta) * Math.Sin(phi), Math.Cos(phi), Math.Sin(theta) * Math.Sin(phi));
        }

        /// <summary>
        /// Separate sign and index
        /// </summary>
        static void masksign(ref uint n, out byte sign)
        {
            sign = (byte)(n >> 21);
            n &= MASK;
        }
        /// <summary>
        /// convert to cartesian vector
        /// </summary>
        static Vector3f cartesian(uint i, uint j, byte sign)
        {
            float phi = i * Mathelp.Rad90 / N;
            float theta = i > 0 ? j * Mathelp.Rad90 / i : 0;

            //var normal = Mathelp.SphericalToCartesian(1f, theta, phi);
            sphericalToCartesian(ref theta, ref phi, out var normal);

            if ((sign & 4) != 0) normal.x *= -1;
            if ((sign & 2) != 0) normal.y *= -1;
            if ((sign & 1) != 0) normal.z *= -1;

            return normal;
        }

        /// <summary>
        /// Summation of sequence for j=0, or Gauss formula, or triangular number, or...
        /// </summary>
        static uint sum(uint i) => (i + 1) * i / 2;

        /// <summary>
        /// inverse of summation.
        /// </summary>
        static void inverse(uint n, out uint i, out uint j)
        {
            //check for n[0,MAX]
            i = (uint)(Math.Sqrt(1 + 8 * n) - 1) / 2;
            j = n - sum(i);
        }

        public static uint Encode(Vector3f normal)
        {
            uint value = 0;
            if (normal.x < 0) { value |= 4; normal.x *= -1; }
            if (normal.y < 0) { value |= 2; normal.y *= -1; }
            if (normal.z < 0) { value |= 1; normal.z *= -1; }

            //(float r, float theta, float phi) = Mathelp.CartesianToSpherical(normal);
            normal.Normalize();
            cartesianToSpherical(ref normal, out var theta, out var phi);

            uint i = (uint)(phi * N / Mathelp.Rad90);
            uint j = (uint)Math.Round(theta * i / Mathelp.Rad90);
            uint n = sum(i) + j;

            return value << 21 | n;
        }

        /// <summary>
        /// Decode using precomputed table for i
        /// </summary>
        /// <param name="value">only first 24bit contain the data</param>
        public static Vector3f DecodeFast(uint value)
        {
            if (i_tab == null) GeneratePrecomputedTab();

            masksign(ref value, out byte sign);
            uint i = i_tab[value];
            uint j = value - sum(i);
            return cartesian(i, j, sign);
        }

        /// <summary>
        /// Decode without precomputed table, using inverse function
        /// </summary>
        public static Vector3f Decode(uint value)
        {
            masksign(ref value, out byte sign);
            inverse(value, out var i, out var j);
            return cartesian(i, j, sign);
        }
    }

    /// <summary>
    /// Generalization of <see cref="UnitSphericalPacker16"/><br/>
    /// <code>3 bits sign, 14 bits i, 14 bits j = 31 bits</code>
    /// </summary>
    [Obsolete("try instead UnitVectorPacker32")]
    public static class UnitSphericalPacker32
    {
        const int MASK = 0x3FFF;
        const int N = 16383;

        /// <summary>
        /// convert to cartesian vector
        /// </summary>
        static Vector3f cartesian(uint i, uint j, int sign)
        {
            float phi = i * Mathelp.Rad90 / N;
            float theta = i > 0 ? j * Mathelp.Rad90 / i : 0;

            var normal = Mathelp.SphericalToCartesian(1f, theta, phi);

            if ((sign & 4) != 0) normal.x *= -1;
            if ((sign & 2) != 0) normal.y *= -1;
            if ((sign & 1) != 0) normal.z *= -1;

            return normal;
        }

        public static uint Encode(Vector3f normal)
        {
            uint value = 0;
            if (normal.x < 0) { value |= 4; normal.x *= -1; }
            if (normal.y < 0) { value |= 2; normal.y *= -1; }
            if (normal.z < 0) { value |= 1; normal.z *= -1; }

            (float r, float theta, float phi) = Mathelp.CartesianToSpherical(normal);

            uint i = (uint)(phi * N / Mathelp.Rad90);
            uint j = (uint)(theta * i / Mathelp.Rad90);


            if (i < 0 || i > N || j < 0 || j > N || j > i) throw new Exception("fail");

            return value << 29 | i << 14 | j;
        }


        public static Vector3f Decode(uint value)
        {
            var sign = (byte)(value >> 29);
            var i = (value >> 14) & MASK;
            var j = value & MASK;
            if (j > i) throw new Exception("fail");
            return cartesian(i, j, sign);
        }

    }



    public static class UnitVectorPacker32
    {
        /// <summary>
        /// X and Y converted into 15bit, 1 bit for Z sign
        /// </summary>
        /// <remarks>error of 6,1037e-5</remarks>
        public static uint EncodeX15Y15Z1(Vector3f normal)
        {
            uint x = (uint)((normal.x + 1) * 32767f / 2f) & 0x7FFF;
            uint y = (uint)((normal.y + 1) * 32767f / 2f) & 0x7FFF;
            uint z = (uint)(normal.z > 0 ? 0 : 1);
            return x | y << 15 | z << 30;
        }
        public static Vector3f DecodeX15Y15Z1(uint packed)
        {
            Vector3f normal = new Vector3f();
            normal.x = Mathelp.CLAMP(((packed & 0x7FFF) * 2f / 32767f) - 1, -1, 1); packed >>= 15;
            normal.y = Mathelp.CLAMP(((packed & 0x7FFF) * 2f / 32767f) - 1, -1, 1); packed >>= 15;
            normal.z = Mathelp.CLAMP(1 - normal.LengthSq, 0, 1);
            normal.z = normal.z > float.Epsilon ? (float)Math.Sqrt(normal.z) : 0;
            if ((packed & 1) != 0) normal.z *= -1;
            return normal;
        }
    }




    /// <summary>
    /// For normals vector compression<br/>
    /// Attention, ushort and uint use <b>Little Endian encoding</b>
    /// </summary>
    public static class UnitVectorPacker_old
    {
        [Obsolete("not work")]
        public static ushort Encode16bit(Vector3f normal) => Packer16.pack(ref normal);
        [Obsolete("not work")]
        public static Vector3f Decode16bit(ushort value) => Packer16.unpack(value);
        public static uint Encode32bit(Vector3f normal) => Packer32.pack(ref normal.x, ref normal.y, ref normal.z);
        public static Vector3f Decode32bit(uint value) => Packer32.unpack(value);

        public static uint EncodeX15Y15(Vector4f normal) => SpaceEngineers32.pack(normal.x, normal.y, normal.z, normal.w);
        public static Vector4f DecodeX15Y15(uint value) => SpaceEngineers32.unpack(value);
        public static uint EncodeX15Y15(Vector3f normal) => SpaceEngineers32.pack(normal.x, normal.y, normal.z);

        /// <summary>
        /// 16bit normalized vector
        /// from https://archive.gamedev.net/archive/reference/articles/article1191.html
        /// </summary>
        static class Packer16
        {
            static float[] uvadjust;

            // upper 3 bits
            const ushort SIGN_MASK = 0xe000;
            const ushort XSIGN_MASK = 0x8000;
            const ushort YSIGN_MASK = 0x4000;
            const ushort ZSIGN_MASK = 0x2000;
            // middle 6 bits - xbits
            const ushort TOP_MASK = 0x1f80;
            // lower 7 bits - ybits
            const ushort BOTTOM_MASK = 0x007f;


            static Packer16()
            {
                uvadjust = new float[0x2000];

                for (int idx = 0; idx < 0x2000; idx++)
                {
                    long xbits = idx >> 7;
                    long ybits = idx & BOTTOM_MASK;

                    // map the numbers back to the triangle (0,0)-(0,127)-(127,0)
                    if ((xbits + ybits) >= 127)
                    {
                        xbits = 127 - xbits;
                        ybits = 127 - ybits;
                    }

                    // convert to 3D vectors
                    float x = xbits;
                    float y = ybits;
                    float z = 126 - xbits - ybits;

                    // calculate the amount of normalization required
                    uvadjust[idx] = 1.0f / (float)Math.Sqrt(y * y + z * z + x * x);
                }
            }

            static ushort clamp127(float f)
            {
                if (f >= 127) return 127;
                else if (f <= 0) return 0;
                else return (ushort)f;
            }

            public static ushort pack(ref Vector3f normal) => pack(normal.x, normal.y, normal.z);
            public static ushort pack(float vecx, float vecy, float vecz)
            {
                // input vector does not have to be unit length
                // assert( tmp.length() <= 1.001f );      
                ushort value = 0;

                if (vecx < 0) { value |= XSIGN_MASK; vecx = -vecx; }
                if (vecy < 0) { value |= YSIGN_MASK; vecy = -vecy; }
                if (vecz < 0) { value |= ZSIGN_MASK; vecz = -vecz; }

                // project the normal onto the plane that goes through
                // X0=(1,0,0),Y0=(0,1,0),Z0=(0,0,1).

                // on that plane we choose an (projective!) coordinate system
                // such that X0->(0,0), Y0->(126,0), Z0->(0,126),(0,0,0)->Infinity

                // a little slower... old pack was 4 multiplies and 2 adds. 
                // This is 2 multiplies, 2 adds, and a divide....
                float w = 126.0f / (vecx + vecy + vecz);
                ushort xbits = clamp127(vecx * w);
                ushort ybits = clamp127(vecy * w);

                // Now we can be sure that 0<=xp<=126, 0<=yp<=126, 0<=xp+yp<=126

                // however for the sampling we want to transform this triangle 
                // into a rectangle.
                if (xbits >= 64)
                {
                    xbits = (ushort)(127 - xbits);
                    ybits = (ushort)(127 - ybits);
                }

                // now we that have xp in the range (0,127) and yp in the 
                // range (0,63), we can pack all the bits together
                value |= (ushort)(xbits << 7);
                value |= (ushort)ybits;
                return value;
            }

            public static Vector3f unpack(ushort value)
            {
                Vector3f normal = new Vector3f();
                unpack(value, out normal.x, out normal.y, out normal.y);
                return normal;
            }
            public static void unpack(ushort value, out float vecx, out float vecy, out float vecz)
            {
                // if we do a straightforward backward transform
                // we will get points on the plane X0,Y0,Z0
                // however we need points on a sphere that goes through 
                // these points.
                // therefore we need to adjust x,y,z so that x^2+y^2+z^2=1

                // by normalizing the vector. We have already precalculated 
                // the amount by which we need to scale, so all we do is a 
                // table lookup and a multiplication

                // get the x and y bits
                long xbits = ((value & TOP_MASK) >> 7);
                long ybits = (value & BOTTOM_MASK);

                // map the numbers back to the triangle (0,0)-(0,126)-(126,0)
                if ((xbits + ybits) >= 127)
                {
                    xbits = 127 - xbits;
                    ybits = 127 - ybits;
                }

                // do the inverse transform and normalization
                // costs 3 extra multiplies and 2 subtracts. No big deal.         
                float uvadj = uvadjust[value & ~SIGN_MASK];

                vecx = uvadj * (float)xbits;
                vecy = uvadj * (float)ybits;
                vecz = uvadj * (float)(126 - xbits - ybits);


                // set all the sign bits
                if ((value & XSIGN_MASK) != 0) vecx = -vecx;
                if ((value & YSIGN_MASK) != 0) vecy = -vecy;
                if ((value & ZSIGN_MASK) != 0) vecz = -vecz;

            }
        }

        /// <summary>
        /// 32bit normalized vector, encode normals to uint32
        /// from http://www.geometrictools.com/Documentation/CompressedUnitVectors.pdf
        /// </summary>
        static class Packer32
        {
            public static uint pack(ref float x, ref float y, ref float z)
            {
                uint xc = (uint)(1023.0f * (0.5f * (x + 1.0f)));
                uint yc = (uint)(1023.0f * (0.5f * (y + 1.0f)));
                uint zc = (uint)(1023.0f * (0.5f * (z + 1.0f)));
                return xc | (yc << 10) | (zc << 20);
            }

            public static Vector3f unpack(uint value)
            {
                Vector3f normal = new Vector3f()
                {
                    x = 2.0f * (value & 0x000003FF) / 1023.0f - 1.0f,
                    y = 2.0f * ((value & 0x000FFC00) >> 10) / 1023.0f - 1.0f,
                    z = 2.0f * ((value & 0x3FF00000) >> 20) / 1023.0f - 1.0f
                };
                normal.Normalize();
                return normal;
            }
        }

        static class SpaceEngineers32
        {
            const uint n15x15y_wsign_mask = 0x80000000; // 10000000 00000000 00000000 00000000
            const uint n15x15y_zsign_mask = 0x00008000; // 00000000 00000000 10000000 00000000
            const uint n15x15y_mask = 0x00007FFF;       // 00000000 00000000 01111111 11111111

            static uint clamp(uint value, uint min, uint max)
            {
                return value < min ? min : value > max ? max : value;
            }

            /// <summary>
            /// Used in SpaceEngineersGame for normal vector, set 1bit:sign_z ,15bit x value, 1bit sign_w , 15bit y value
            /// z value are calculated from x y
            /// w can be 1 or -1
            /// </summary>
            public static uint pack(float x, float y, float z, float w = 1)
            {
                uint packed = 0;
                if (z > 0) packed |= n15x15y_zsign_mask;
                if (w > 0) packed |= n15x15y_wsign_mask;

                // convert [-1.0f,1.0f] to to [0,32767]
                x = (x + 1) * 0.5f * 32767.0f;
                y = (y + 1) * 0.5f * 32767.0f;

                // fix
                uint _x = clamp((uint)x, 0, 32767);
                uint _y = clamp((uint)y, 0, 32767);

                // instead write first short x and second short y in file, i write integer.
                // when i read uint32 with LittleEndian _x become the smaller value, _y greater
                return packed | (_y << 16) | _x;
            }
            /// <summary>
            /// Used in SpaceEngineersGame, set 1bit:sign_z ,15bit x value, 1bit sign_w , 15bit y value
            /// z value are calculated from x y
            /// w value can be 1 or -1
            /// </summary>
            public static void unpack(uint packed, out float x, out float y, out float z, out float w)
            {
                // convert [0,32767] to [0.0f,1.0f]
                x = (packed & n15x15y_mask) / 32767.0f;
                y = ((packed >> 16) & n15x15y_mask) / 32767.0f;

                // convert [0.0f,1.0f] to [-1.0f,1.0f]
                x = 2 * x - 1;
                y = 2 * y - 1;

                // compute z , example if x or y = 1 , magnitude is ~ 0 and sqrt return NaN
                z = 1 - x * x - y * y;
                z = z > float.Epsilon ? (float)Math.Sqrt(z) : 0.0f;

                if ((packed & n15x15y_zsign_mask) == 0) z *= -1;
                w = 1.0f;
                if ((packed & n15x15y_wsign_mask) == 0) w *= -1;
            }
            /// <summary>
            /// <seealso cref="unpack(uint,out float,out float,out float,out float)"/>
            /// </summary>
            public static Vector4f unpack(uint packed)
            {
                Vector4f vector = Vector4f.Zero;
                unpack(packed, out vector.x, out vector.y, out vector.z, out vector.w);
                return vector;
            }
        }
    }
}
