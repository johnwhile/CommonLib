using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Common.Tools;

namespace Common.Maths
{
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{ToStringRounded()}")]
    public struct Vector3f : IEquatable<Vector3f>
    {
        /// <summary>
        /// normalizing a zero-lenght vector is always an error and must be debug...or disable it
        /// </summary>
        public static bool CHECK_ZEROLENGHT_WHEN_NORMALIZE = true;

        [FieldOffset(0)]
        public float x;
        [FieldOffset(4)]
        public float y;
        [FieldOffset(8)]
        public float z;
        [FieldOffset(0)]
        public unsafe fixed float field[3]; // performance comparable using direct access to x y z field

        public const float EPS = 1e-6f;

        public Vector3f(BinaryReader reader)
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
        }

        /// <summary>
        /// Try parse a string with format "x, y, z"
        /// </summary>
        /// <param name="toparse"></param>
        public static bool TryParse(string toparse, out Vector3f value)
        {
            return MathParsers.TryParse(toparse, out value);
        }


        public Vector3f(double x, double y, double z) :
            this((float)x, (float)y, (float)z)
        { }

        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        public bool isZero => LengthSq < EPS;
        
        public bool IsNaN => 
            float.IsInfinity(x) || float.IsNaN(x) ||
            float.IsInfinity(y) || float.IsNaN(y) ||
            float.IsInfinity(z) || float.IsNaN(z);
        
        /// <summary>
        /// Slow method, is equivalent to use switch() of UNSAFE flag in this[int]
        /// </summary>
        public unsafe float GetDim(int i)
        {
            fixed (float* buffer = field)
            {
                return buffer[i];
            }
        }

        /// <summary>
        /// UNSAFE flag don't reveal a performance improvement, i suggest to use <code>float d = vector.Dim[i]</code>
        /// </summary>
        public unsafe float this[int i]
        {
            get { fixed (float* pX = &x) return *(pX + i); }
            set { fixed (float* pX = &x) *(pX + i) = value; }
        }

        public float this[eAxis axe]
        {
            get
            {
                switch (axe)
                {
                    case eAxis.X: return x;
                    case eAxis.Y: return y;
                    case eAxis.Z: return z;
                    default: throw new ArgumentException("Wrong Axe");
                }
            }
            set
            {
                switch (axe)
                {
                    case eAxis.X: x = value; break;
                    case eAxis.Y: y = value; break;
                    case eAxis.Z: z = value; break;
                    default: throw new ArgumentException("Wrong Axe");
                }
            }
        }

        /// <summary>
        /// A "null" vector, not zero, used example when you want considerate the value not processed or divided by 0
        /// </summary>
        public static readonly Vector3f NaN = new Vector3f(float.NaN, float.NaN, float.NaN);
        public static readonly Vector3f PosInf = new Vector3f(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public static readonly Vector3f NegInf = new Vector3f(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        public static readonly Vector3f Zero = new Vector3f(0, 0, 0);
        public static readonly Vector3f One = new Vector3f(1, 1, 1);
        public static readonly Vector3f UnitX = new Vector3f(1, 0, 0);
        public static readonly Vector3f UnitY = new Vector3f(0, 1, 0);
        public static readonly Vector3f UnitZ = new Vector3f(0, 0, 1);
        public static readonly Vector3f NormalOne = new Vector3f(1.0f / Mathelp.Sqrt3, 1.0f / Mathelp.Sqrt3, 1.0f / Mathelp.Sqrt3);

        // Directx Left Handled Coordinate System
        public static readonly Vector3f Right = UnitX;
        public static readonly Vector3f Left = -UnitX;
        public static readonly Vector3f Up = UnitY;
        public static readonly Vector3f Down = -UnitY;
        public static readonly Vector3f Backward = UnitZ;
        public static readonly Vector3f Forward = -UnitZ;


        #region operator overload
        public static Vector3f operator +(Vector3f left, Vector3f right)
        {
            left.x += right.x;
            left.y += right.y;
            left.z += right.z;
            return left;
        }
        public static Vector3f operator +(Vector3f v, float scalar)
        {
            v.x += scalar;
            v.y += scalar;
            v.z += scalar;
            return v;
        }

        public static Vector3f operator -(Vector3f left, Vector3f right)
        {
            left.x -= right.x;
            left.y -= right.y;
            left.z -= right.z;
            return left;
        }
        public static Vector3f operator -(Vector3f v, float scalar)
        {
            v.x -= scalar;
            v.y -= scalar;
            v.z -= scalar;
            return v;
        }
        public static Vector3f operator -(Vector3f v)
        {
            v.x = -v.x;
            v.y = -v.y;
            v.z = -v.z;
            return v;
        }

        /// <summary>
        /// Mull left.x*right.x , left.y*right.y , left.z*right.z 
        /// </summary>
        public static Vector3f operator *(Vector3f left, Vector3f right)
        {
            // because struct are passed as copy
            left.x *= right.x;
            left.y *= right.y;
            left.z *= right.z;
            return left;
        }
        public static Vector3f operator *(Vector3f left, float scalar)
        {
            left.x *= scalar;
            left.y *= scalar;
            left.z *= scalar;
            return left;
        }
        public static Vector3f operator *(float scalar, Vector3f right)
        {
            right.x *= scalar;
            right.y *= scalar;
            right.z *= scalar;
            return right;
        }

        /// <summary>
        /// Divide left.x/right.x , left.y/right.y , left.z/right.z 
        /// </summary>
        public static Vector3f operator /(Vector3f left, Vector3f right)
        {
            left.x /= right.x;
            left.y /= right.y;
            left.z /= right.z;
            return left;
        }
        /// <summary>
        /// Vector.X = Vector.X / scalar 
        /// </summary>
        public static Vector3f operator /(Vector3f left, float scalar)
        {
            if (Mathelp.isZero(scalar)) throw new ArgumentException("Cannot divide a Vector3 by zero");
            scalar = 1.0f / scalar;
            left.x *= scalar;
            left.y *= scalar;
            left.z *= scalar;
            return left;
        }
        /// <summary>
        /// Vector.X = Vector.X / scalar 
        /// </summary>
        public static Vector3f operator /(float scalar, Vector3f right)
        {
            if (Mathelp.isZero(scalar)) throw new ArgumentException("Cannot divide a Vector3 by zero");
            scalar = 1.0f / scalar;
            right.x /= scalar;
            right.y /= scalar;
            right.z /= scalar;
            return right;
        }


        public static bool operator ==(Vector3f left, Vector3f right) => left.Equals(ref right);
        public static bool operator !=(Vector3f left, Vector3f right) => !left.Equals(ref right);
        public override bool Equals(object obj) => obj is Vector3f vector && Equals(ref vector);
        public bool Equals(Vector3f vector) => Equals(ref vector);
        public bool Equals(ref Vector3f vector) => x == vector.x && y == vector.y && z == vector.z;


        public static implicit operator Vector3f(Vector3i vector) => new Vector3f(vector.x, vector.y, vector.z);
        public static implicit operator Vector3f(Vector4f vector) => new Vector3f(vector.x, vector.y, vector.z);
        public static implicit operator Vector3f(Vector3d vector) => new Vector3f(vector.x, vector.y, vector.z);


        public bool Similar(Vector3f vector, float epsilon) =>
                (Mathelp.ABS(x - vector.x) < epsilon) &&
                (Mathelp.ABS(y - vector.y) < epsilon) &&
                (Mathelp.ABS(z - vector.z) < epsilon);
        
        #endregion

        #region Math

        /// <summary>
        /// Get the squared length = (x*x + y*y + z*z)
        /// </summary>
        public float LengthSq => x * x + y * y + z * z;

        /// <summary>
        /// Get the length = sqrt(x*x + y*y + z*z)
        /// </summary>
        public float Length => (float)Math.Sqrt(LengthSq);

        /// <summary>
        /// Get the manhattan length = |x| + |y| + |z|
        /// </summary>
        public float ManhattanLength => Mathelp.ABS(x) + Mathelp.ABS(y) + Mathelp.ABS(z);

        /// <summary>
        /// Get the normal of a triangle, if is degeneralized return a Vector3.Zero
        /// </summary>
        public static Vector3f GetNormalTriangle(Vector3f v0, Vector3f v1, Vector3f v2)
        {
            v0 = Cross(v1 - v0, v2 - v0);
            float l = v0.Length;
            return (l > float.Epsilon) ? v0 / l : Zero;
        }

        /// <summary>
        /// Update minimum values
        /// </summary>
        public void Min(float x, float y, float z)
        {
            if (this.x > x) this.x = x;
            if (this.y > y) this.y = y;
            if (this.z > z) this.z = z;
        }

        /// <summary>
        /// Update maximum values
        /// </summary>
        public void Max(float x, float y, float z)
        {
            if (this.x < x) this.x = x;
            if (this.y < y) this.y = y;
            if (this.z < z) this.z = z;
        }
        /// <summary>
        /// Sum
        /// </summary>
        public void Sum(in Vector3f vect)
        {
            x += vect.x;
            y += vect.y;
            z += vect.z;
        }
        /// <summary>
        /// Subtraction
        /// </summary>
        public void Sub(in Vector3f vect)
        {
            x -= vect.x;
            y -= vect.y;
            z -= vect.z;
        }
        /// <summary>
        /// multiple x,y,z * scalar
        /// </summary>
        public void Multiply(float scalar)
        {
            x *= scalar;
            y *= scalar;
            z *= scalar;
        }

        public static Vector3f Multiply(in Matrix4x4f left, in Vector3f right, float w = 1)
        {
            float iw = left.m30 * right.x + left.m31 * right.y + left.m32 * right.z + left.m33 * w;

            iw = iw * iw < 1e-6 ? 1 : 1 / iw;

            return new Vector3f(
                (right.x * left.m00 + right.y * left.m01 + right.z * left.m02 + w * left.m03) * iw,
                (right.x * left.m10 + right.y * left.m11 + right.z * left.m12 + w * left.m13) * iw,
                (right.x * left.m20 + right.y * left.m21 + right.z * left.m22 + w * left.m23) * iw);
        }

        public static double Normalize(ref float x, ref float y, ref float z)
        {
            var length = Math.Sqrt(x * x + y * y + z * z);
            float l = length > 1e-6f ? (float)(1 / length) : 0;

#if DEBUG
            if (length < 1e-6f && CHECK_ZEROLENGHT_WHEN_NORMALIZE) 
                Debugg.Message($"normalizing value {x} {y} {z} return zero length, please check the code");
#endif
            x *= l;
            y *= l;
            z *= l;

            return length;
        }

        /// <summary>
        /// Normalize the vector and get the calculated length. 
        /// </summary>
        public float Normalize() => (float)Normalize(ref x, ref y, ref z);

        public static Vector3f Normalize(Vector3f vector)
        {
            vector.Normalize();
            return vector;
        }
        /// <summary>
        /// Get a new normalized vector3 copy
        /// </summary>
        public Vector3f Normal => Normalize(this);

        /// <summary>
        /// square root distance
        /// </summary>
        public static float Distance(in Vector3f a, in Vector3f b) => (float)Math.Sqrt(DistanceSquare(in a, in b));
        public static float Distance(Vector3f a, Vector3f b) => Distance(in a, in b);
        /// <summary>
        /// square distance
        /// </summary>
        public static float DistanceSquare(in Vector3f a, in Vector3f b) => (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
        public static float DistanceSquare(Vector3f a, Vector3f b) => DistanceSquare(in a, in b);
        /// <summary>
        /// ax*bx + ay*by + az*bz
        /// </summary>
        public static float Dot(Vector3f a, Vector3f b) => Dot(in a, in b);
        public static float Dot(in Vector3f a, in Vector3f b) => a.x * b.x + a.y * b.y + a.z * b.z;

        /// <summary>
        /// remember that b × a = −(a × b) and for a LH coord system :
        /// <para> X = Y × Z </para>
        /// <para> Y = Z × X </para>
        /// <para> Z = X × Y </para>
        /// </summary>
        /// <remarks>
        /// A = direction of thumb, B = index finger, Cross = middle finger
        /// using Left-Hand rule for a Left Hand coordinate system
        /// </remarks>
        public static Vector3f Cross(Vector3f left, Vector3f right) => Cross(in left, in right);

        /// <summary>
        /// Optimized
        /// </summary>
        public static void Cross(in Vector3f left, in Vector3f right, ref Vector3f result)
        {
            result.x = (left.y * right.z) - (left.z * right.y);
            result.y = (left.z * right.x) - (left.x * right.z);
            result.z = (left.x * right.y) - (left.y * right.x);
        }
        public static Vector3f Cross(in Vector3f left, in Vector3f right)
        {
            return new Vector3f(
                (left.y * right.z) - (left.z * right.y),
                (left.z * right.x) - (left.x * right.z),
                (left.x * right.y) - (left.y * right.x));
        }

        #endregion


        /// <summary>
        /// Transform the vector using matrix, then result is Transform * Vector
        /// </summary>
        /// <remarks>
        /// Column-Major: Standard widely used for OpenGL.
        /// Values are stored in column-first order<br/>
        /// The matrix must be to the LEFT of the multiply operator<br/>
        /// The vertex or vector must to the RIGHT of the operator
        /// </remarks>
        public Vector3f TransformCoordinate(in Matrix4x4f transform) => Multiply(in transform, in this, 1);

        /// <summary>
        /// Transform the Normal vector using matrix, remember that the correct matrix for <paramref name="transform"/> is the 
        /// <b><i>WorldInverseTraspose</i></b> instead World, if you want a normal not affected by scale transformation
        /// </summary>
        /// <remarks>
        /// WorldInverseTraspose matrix has traslation zero and last row = [0,0,0,1]. In fact only the 3x3 part of matrix is used
        /// and it's equivalent to multiply a 4x4 matrix with a Vector4(x,y,z,0)
        /// </remarks>
        public Vector3f TransformNormal(in Matrix4x4f transform) => Multiply(in transform, in this, 0);


        public static Vector3f TransformCoordinate(in Vector3f vector, in Quaternion4f rotation)
        {
            float x = rotation.x + rotation.x;
            float y = rotation.y + rotation.y;
            float z = rotation.z + rotation.z;

            float wx = rotation.w * x;
            float wy = rotation.w * y;
            float wz = rotation.w * z;

            float xx = rotation.x * x;
            float xy = rotation.x * y;
            float xz = rotation.x * z;

            float yy = rotation.y * y;
            float yz = rotation.y * z;
            float zz = rotation.z * z;

            return new Vector3f(
                ((vector.x * ((1.0f - yy) - zz)) + (vector.y * (xy - wz))) + (vector.z * (xz + wy)),
                ((vector.x * (xy + wz)) + (vector.y * ((1.0f - xx) - zz))) + (vector.z * (yz - wx)),
                ((vector.x * (xz - wy)) + (vector.y * (yz + wx))) + (vector.z * ((1.0f - xx) - yy)));
        }

        #region Left Hand Rule

        /// <summary>
        /// TO TEST
        /// Return the origin of ray when you click on screen, the point depend by depthZ , default is 0.0
        /// </summary>
        /// <remarks>
        /// the directx big matrix is Proj * View * World , isn't commutative
        /// </remarks>
        /// <param name="depthZ">-1.0 = in the eye postion , 0.0 = nearZ plane , 1.0 = farZ plane</param>
        public static Vector3f Unproject(int mouseX, int mouseY, float depthZ, ViewportClip viewport, Matrix4x4f proj, Matrix4x4f view, Matrix4x4f world)
        {
            Vector4f source = new Vector4f
            {
                x = (mouseX - viewport.X) / (viewport.Width * 2f) - 1f,
                y = ((mouseY - viewport.Y) / (float)viewport.Height) * -2f + 1f,
                z = (depthZ - viewport.MinDepth) / (viewport.MaxDepth - viewport.MinDepth),
                w = 1f
            };

            source = Matrix4x4f.Inverse(proj * view * world) * source;

            // homogenous calculation and test if is a zero-division
            if (source.w * source.w > float.Epsilon)
                source *= 1f / source.w;

            return (Vector3f)source;
        }


        /// <summary>
        /// get the planar projection point.
        /// </summary>
        public static Vector3f Project(Vector3f vector, Matrix4x4f world)
        {
            Vector4f vect = (Vector4f)vector;
            vect = world * vect;
            return (Vector3f)vect;
        }
        /// <summary>
        /// get the screen point, the z value are the depthZ calculate with Logarithmic algorithm
        /// http://www.gamasutra.com/blogs/BranoKemen/20090812/2725/Logarithmic_Depth_Buffer.php
        /// </summary>
        /// <param name="WVP">Proj * View (because camera world is always identity)</param>
        public static Vector3f ProjectLogZ(Vector3f vector, ViewportClip viewport, Matrix4x4f WVP, float FarPlane, float Resolution = 0.001f)
        {
            Vector4f coord = new Vector4f(vector.x, vector.y, vector.z, 1.0f);
            coord = WVP * coord;

            coord.z = (float)(Math.Log10(Resolution * coord.z + 1) / Math.Log10(Resolution * FarPlane + 1) * coord.w);

            float inv_w = coord.w * coord.w > float.Epsilon ? 1.0f / coord.w : 1.0f;
            coord *= inv_w;

            // now you have the X Y coordinate in [-1,1] range(bottomleft system) and Z-depht value in [near=0, far=1] range.
            // Flip y value for a Top-Left coordinates
            vector.x = (coord.x + 1) * 0.5f * viewport.Width + viewport.X;
            vector.y = (1 - coord.y) * 0.5f * viewport.Height + viewport.Y;
            vector.z = coord.z;

            return vector;
        }

        /// <summary>
        /// get the screen point, the z value are the depthZ
        /// </summary>
        /// <param name="WVP">Proj * View (because camera world is always identity)</param>
        /// <remarks>
        /// if in directx (row major) the WVP is defined as W*V*P, in my math notation i use OpenGL colum major (like my school knowledge)
        /// where matrix are trasposed so W^t*V^t*P^t become P*V*W
        /// </remarks>
        public static Vector3f Project(Vector3f vector, ViewportClip viewport, Matrix4x4f WVP)
        {
            // multipling in colum notation we optain the trasformation of vector to wvp system and so the output of vertexshader
            Vector4f coord = new Vector4f(vector.x, vector.y, vector.z, 1.0f);
            coord = WVP * coord;

            // now recent graphics cards auto-calculate the homogeneus value in pixelshader input, otherwise you have to do manualy
            // or like here: http://msdn.microsoft.com/en-us/library/windows/desktop/bb153308(v=vs.85).aspx they suggest
            // to create a compliant matrix
            float inv_w = coord.w * coord.w > float.Epsilon ? 1.0f / coord.w : 1.0f;
            coord *= inv_w;

            // now you have the X Y coordinate in [-1,1] range(bottomleft system) and Z-depht value in [near=0, far=1] range.
            // Flip y value for a Top-Left coordinates
            vector.x = (coord.x + 1) * 0.5f * viewport.Width + viewport.X;
            vector.y = (1 - coord.y) * 0.5f * viewport.Height + viewport.Y;
            vector.z = coord.z;

            return vector;
        }
        /// <summary>
        /// get the screen point
        /// remember that 'vector' must be transformed in world coordinate before pass as parameter
        /// </summary>
        public static Vector3f Project(Vector3f vector, ViewportClip viewport, ICamera camera)
        {
            return Project(vector, viewport, camera.Projection * camera.View);
        }
        /// <summary>
        /// get the screen point using logarithmic z instead linear z.
        /// remember that 'vector' must be transformed in world coordinate before pass as parameter
        /// </summary>
        public static Vector3f ProjectLogZ(Vector3f vector, ViewportClip viewport, ICamera camera, float Resolution = 0.001f)
        {
            return ProjectLogZ(vector, viewport, camera.Projection * camera.View, camera.Far, Resolution);
        }
        #endregion


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                hash = (hash * 16777619) ^ z.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// format for normalized vector where first digit is always in 0.0-1.0 range and decimals is not very important
        /// </summary>
        public string ToStringRounded()
            => string.Format("{0:0.000} {1:0.000} {2:0.000}", x, y, z);

        public override string ToString()
            => IsNaN ? "NaN" : string.Format(Mathelp.DotCulture, "{0} {1} {2}", x, y, z);
    }
}