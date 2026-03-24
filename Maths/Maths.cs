using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Common.Maths
{
    /// <summary>
    /// X,Y,Z flags
    /// </summary>
    [Flags]
    public enum eAxis : byte
    {
        /// <summary>000</summary>
        None = 0x00,
        /// <summary>001</summary>
        X = 0x01,
        /// <summary>010</summary>
        Y = 0x02,
        /// <summary>100</summary>
        Z = 0x04,
        /// <summary>011</summary>
        XY = X | Y,
        /// <summary>101</summary>
        XZ = X | Z,
        /// <summary>110</summary>
        YZ = Y | Z,
        /// <summary>111</summary>
        XYZ = X | Y | Z
    }

    /// <summary>
    /// The six faces index of a Texture cube
    /// </summary>
    public enum MapFace : byte
    {
        PositiveX = 0,
        NegativeX = 1,
        PositiveY = 2,
        NegativeY = 3,
        PositiveZ = 4,
        NegativeZ = 5,
    }

    public static class MathParsers
    {
        static char[] splitcomma = new char[] { ',' };

        static string[] splitvector(string vector, int dim)
        {
            string[] split = vector.Split(splitcomma, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != dim) return null;
            return split;
        }

        public static bool TryParse(string toparse, out Matrix4x4f value)
        {
            bool result = true;
            float[] array = new float[16];
            if (!TryParseFloatArray(toparse, ref array)) result = false;
            value = new Matrix4x4f(array);
            return result;
        }

        /// <summary>
        /// Try parse an integer vector write as "x, y, z, w" 
        /// </summary>
        public static bool TryParse(string toparse, out Vector4i value)
        {
            value = default(Vector4i);
            return TryParse4int(toparse, out value.x, out value.y, out value.z, out value.w);
        }
        /// <summary>
        /// Try parse an integer vector write as "x, y, width, height" 
        /// </summary>
        public static bool TryParse(string toparse, out Rectangle4i value)
        {
            value = default(Rectangle4i);
            return TryParse4int(toparse, out value.x, out value.y, out value.width, out value.height);
        }
        /// <summary>
        /// Try parse an integer vector write as "x, y, width, height" 
        /// </summary>
        public static bool TryParse(string toparse, out Rectangle4ui value)
        {
            value = default(Rectangle4ui);
            return TryParse4uint(toparse, out value.x, out value.y, out value.width, out value.height);
        }


        /// <summary>
        /// Try parse an integer vector write as "x, y, width, height" 
        /// </summary>
        public static bool TryParse(string toparse, out Vector3f value)
        {
            value = default(Vector3f);
            return TryParse3float(toparse, out value.x, out value.y, out value.z);
        }
        public static bool TryParse(string toparse, out Vector4f value)
        {
            value = default(Vector4f);
            return TryParse4float(toparse, out value.x, out value.y, out value.z, out value.w);
        }

        static bool TryParse4int(string str, out int x, out int y, out int z, out int w)
        {
            x = y = z = w = 0;
            string[] split = str.Split(splitcomma, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 4) return false;

            if (!int.TryParse(split[0], out x) ||
                !int.TryParse(split[1], out y) ||
                !int.TryParse(split[2], out z) ||
                !int.TryParse(split[3], out w))
                return false;
            return true;
        }
        static bool TryParse4uint(string str, out uint x, out uint y, out uint z, out uint w)
        {
            x = y = z = w = 0;
            string[] split = str.Split(splitcomma, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 4) return false;

            if (!uint.TryParse(split[0], out x) ||
                !uint.TryParse(split[1], out y) ||
                !uint.TryParse(split[2], out z) ||
                !uint.TryParse(split[3], out w))
                return false;
            return true;
        }
        static bool TryParse3int(string str, out int x, out int y, out int z)
        {
            x = y = z = 0;
            string[] split = str.Split(splitcomma, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 3) return false;
            if (!int.TryParse(split[0], out x) ||
                !int.TryParse(split[1], out y) ||
                !int.TryParse(split[2], out z))
                return false;
            return true;
        }
        static bool TryParse2int(string str, out int x, out int y)
        {
            x = y = 0;
            string[] split = str.Split(splitcomma, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2) return false;

            if (!int.TryParse(split[0], out x) ||
                !int.TryParse(split[1], out y))
                return false;
            return true;
        }
        static bool TryParse3float(string str, out float x, out float y, out float z)
        {
            x = y = z = 0;
            string[] split = str.Split(splitcomma, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 3) return false;
            return
                float.TryParse(split[0], out x) &&
                float.TryParse(split[1], out y) &&
                float.TryParse(split[2], out z);
        }
        static bool TryParse4float(string str, out float x, out float y, out float z, out float w)
        {
            x = y = z = w = 0;
            string[] split = str.Split(splitcomma, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 4) return false;

            return
                float.TryParse(split[0], out x) &&
                float.TryParse(split[1], out y) &&
                float.TryParse(split[2], out z) &&
                float.TryParse(split[3], out w);
        }

        static bool TryParseFloatArray(string str, ref float[] array)
        {
            string[] split = str.Split(splitcomma, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < array.Length) return false;
            for (int i = 0; i < array.Length; i++) if (!float.TryParse(split[i], out array[i])) return false;
            return true;
        }
        static bool TryParseIntArray(string str, ref int[] array)
        {
            string[] split = str.Split(splitcomma, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < array.Length) return false;
            for (int i = 0; i < array.Length; i++) if (!int.TryParse(split[i], out array[i])) return false;
            return true;
        }
    }


    /// <summary>
    /// Collections of all Maths utilities
    /// </summary>
    public static partial class Mathelp
    {
        /// <summary>
        /// one random class for all cases
        /// </summary>
        public static Random Rnd = new Random();



        const float PI180 = (float)(Math.PI / 180.0);

        public const float Sqrt2 = 1.41421356237309504880168872420969807856967187537694f;
        public const float Sqrt3 = 1.73205080756887729352744634150587236694280525381038062805580f;

        /// <summary>
        /// constant π = 3.1415926535897931
        /// </summary>
        public const float PI = Rad180;
        /// <summary>
        /// π*π
        /// </summary>
        public const float PI2 = Rad180 * Rad180;
        public const float Rad30 = (float)(Math.PI / 6.0);
        public const float Rad45 = (float)(Math.PI / 3.0);
        public const float Rad60 = (float)(Math.PI / 4.0);
        public const float Rad90 = (float)(Math.PI / 2.0);
        public const float Rad180 = (float)Math.PI;
        public const float Rad225 = (float)(Math.PI * 3.0 / 2.0);
        public const float Rad270 = (float)(Math.PI * 5.0 / 4.0);
        public const float Rad315 = (float)(Math.PI * 7.0 / 4.0);
        public const float Rad360 = (float)(Math.PI * 2.0);

        public static float GetRandomFloat(float min = 0.0f, float max = 1.0f)
        {
            return min + (float)Rnd.NextDouble() * (max - min);
        }
        public static float GetRandomFloat(float max)
        {
            return (float)Rnd.NextDouble() * max;
        }
        public static double GetRandomDouble(double max)
        {
            return Rnd.NextDouble() * max;
        }

        /// <summary>
        /// In my version max value is inclusive, so can be returned
        /// </summary>
        public static int GetRandomInt(int min = 0, int max = 1)
        {
            return Rnd.Next(min, max + 1);
        }

        public static int[] powersOfTwo = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };

        public static bool isZero(double x) { return (x > -1e-7) && (x < 1e-7); }
        public static bool isZero(float x) { return (x > -1e-7) && (x < 1e-7); }
        /// <summary>
        /// Cubic root
        /// </summary>
        public static double Cbrt(double x) { return Math.Pow(x, 1.0 / 3.0); }


        public static float RadianToDegree(float Radian) => (float)(180.0 * Radian / Math.PI);

        public static float DegreeToRadian(float Degree) => Degree * PI180;


        /// <summary>
        /// Directx Left-Handle coordinates<br/>
        /// <b>r</b> = radial distance<br/>
        /// <b>theta</b> = inclination<br/>
        /// <b>phi</b> = azimuth.
        /// </summary>
        public static (float r, float theta, float phi) CartesianToSpherical(Vector3f cartesian)
        {
            float r = cartesian.Normalize(); //how x, y, z are divided by r
            float theta = (float)Math.Atan2(cartesian.z, cartesian.x); //work for vector -z
            //float theta = (float)Math.Atan(cartesian.z /cartesian.x);
            float phi = (float)Math.Acos(cartesian.y);
            return (r, theta, phi);
        }

        /// <summary>
        /// Directx Left-Handle coordinates
        /// </summary>
        /// <param name="r">radius</param>
        /// <param name="theta">inclination</param>
        /// <param name="phi">azimuth. if zero return always <see cref="Vector3f.UnitY"/></param>
        public static Vector3f SphericalToCartesian(float r, float theta, float phi) => new Vector3f(
            r * Math.Cos(theta) * Math.Sin(phi),
            r * Math.Cos(phi),
            r * Math.Sin(theta) * Math.Sin(phi));

        /// <summary>
        /// Directx Left-Handle coordinates<br/>
        ///  <b>r</b> = radial distance<br/>
        /// <b>phi</b> = azimuth<br/>
        /// <b>y</b> = equal to cartesian y.
        /// </summary>
        public static (float r, float phi, float y) CartesianToCylindrical(Vector3f cartesian)
        {
            float r = (float)Math.Sqrt(cartesian.x * cartesian.x + cartesian.z * cartesian.z);
            float phi = r > 0 ? (float)Math.Asin(cartesian.z / r) : 0;
            return (r, phi, cartesian.y);
        }
        /// <summary>
        /// Directx Left-Handle coordinates
        /// </summary>
        /// <param name="r">radial distance</param>
        /// <param name="phi">azimuth</param>
        /// <param name="y">equal to cartesian y</param>
        public static Vector3f CylindricalToCartesian(float r, float phi, float y) => new Vector3f(r * Math.Cos(phi), y, r * Math.Sin(phi));

        /// <summary>
        /// Linearly interpolates between two values.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="amount">amount = 0 return value1 , amount = 1 return value2</param>
        /// <returns></returns>
        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }
        /// <summary>
        /// see also hlsl smoothstep(x) 
        /// </summary>
        /// <returns>return a value in range [0,1]</returns>
        public static float Hermite(float min, float max, float x)
        {
            x = CLAMP((x - min) / (max - min), 0, 1);
            return x * x * (3 - 2 * x);
        }
        /// <summary>
        /// Is power or 2 ?
        /// </summary>
        public static bool IsPowOf2(int value)
        {
            if (value < 1) return false;
            return (value & (value - 1)) == 0;
        }
        /// <summary>
        /// n!
        /// </summary>
        public static float fattoriale(float n)
        {
            float result = 1;
            for (int i = 1; i <= n; i++)
                result *= i;
            return result;
        }
        public static sbyte ABS(sbyte i) { return i > 0 ? i : (sbyte)-i; }
        public static short ABS(short i) { return i > 0 ? i : (short)-i; }
        public static int ABS(int i) { return i > 0 ? i : -i; }
        public static float ABS(float f) { return f > 0 ? f : -f; }
        public static double ABS(double d) { return d > 0 ? d : -d; }

        /// <summary>
        /// using bit operations to remove the bit of sign, X1.5 faster, isn't a very improvement
        /// </summary>
        public static unsafe float ABS2(float f)
        {
            uint i = *((uint*)&f) & FSIGMASKneg;
            return *((float*)&i);
        }

        public static int CLAMP(int value, int min, int max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }

        public static float CLAMP(float value, float min, float max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }
        public static double CLAMP(double value, double min, double max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }
        public static (int min, int max) GetMinMax(IEnumerable<int> indices)
        {
            int min = int.MaxValue;
            int max = -1;
            foreach (int i in indices) { min = MIN(min, i); max = MAX(max, i); }
            return (min, max);
        }
        public static int MIN(int a, int b) { return a < b ? a : b; }
        public static int MIN(int a, int b, int c) { return a < b ? MIN(a, c) : MIN(b, c); }
        public static float MIN(float a, float b) { return a < b ? a : b; }
        public static float MIN(float a, float b, float c) { return a < b ? MIN(a, c) : MIN(b, c); }

        public static int MAX(int a, int b) { return a > b ? a : b; }
        public static int MAX(int a, int b, int c) { return a > b ? MAX(a, c) : MAX(b, c); }
        public static float MAX(float a, float b) { return a > b ? a : b; }
        public static float MAX(float a, float b, float c) { return a > b ? MAX(a, c) : MAX(b, c); }
        public static void MINMAX(float x0, float x1, float x2, out float min, out float max)
        {
            min = max = x0;
            if (x1 < min) min = x1;
            if (x1 > max) max = x1;
            if (x2 < min) min = x2;
            if (x2 > max) max = x2;
        }
        public static void SWAP(ref float val1, ref float val2) { float tmp = val1; val1 = val2; val2 = tmp; }
        public static void SWAP(ref int val1, ref int val2) { int tmp = val1; val1 = val2; val2 = tmp; }
        public static void SWAP<T>(ref T val1, ref T val2) { T tmp = val1; val1 = val2; val2 = tmp; }
        public static void SWAP<T>(List<T> list, int i, int j) { T temp = list[i]; list[i] = list[j]; list[j] = temp; }

        /// <summary>
        /// Swap integer with Xor algorithm, only because is a nice feature
        /// </summary>
        public static void SWAPxor(ref int x, ref int y) { if (x != y) { x ^= y; y ^= x; x ^= y; } }

        /// <summary>
        /// return n^2;
        /// </summary>
        public static int NPow2(int n) => n << 1;

        public static float Interpolate(float x, float miny, float maxy, float minx, float maxx)
        {
            return miny + (maxy - miny) * (x - minx) / (maxx - minx);
        }


        /// <summary>
        /// from http://www.cygnus-software.com/papers/comparingfloats/comparingfloats.htm
        /// Make sure <paramref name="maxUlps"/> is non-negative and small enough that the 
        /// default NAN won't compare as equal to anything. assert(maxUlps > 0 && maxUlps < 4 * 1024 * 1024);
        /// 
        /// </summary>
        /// <param name="maxUlps">if 0, return true if two float have same bits.</param>
        public static bool AlmostEqual(float a, float b, int maxUlps = 1)
        {
#if DEBUG
            Debug.Assert(maxUlps > 0 && maxUlps < 4 * 1024 * 1024);
#endif
            // Make aInt lexicographically ordered as a twos-complement int
            int aInt = BitToInt(a);
            if (aInt < 0) aInt = int.MinValue - aInt; // Int32.MinValue = 0x80000000
            int bInt = BitToInt(b);
            if (bInt < 0) bInt = int.MinValue - bInt;
            int intDiff = ABS(aInt - bInt);
            return intDiff <= (1 << maxUlps);
        }



        /// <summary>
        /// Nice numbers for graph labels http://books.google.com/books?id=fvA7zLEFWZgC&pg=PA61&lpg=PA61#v=onepage&q&f=false
        /// </summary>
        public static void LooseLabel(float graphmin, float graphmax, int ticks,
            out float roundmin, out float roundmax, out float roundelta, out float roundigit)
        {
            double range = nicenumber(graphmax - graphmin, false);
            double delta = nicenumber(range / (ticks - 1));
            roundmin = (float)(System.Math.Floor(graphmin / delta) * delta);
            roundmax = (float)(System.Math.Ceiling(graphmax / delta) * delta);
            roundigit = (float)System.Math.Max(-System.Math.Floor(System.Math.Log10(delta)), 0);
            roundelta = (float)delta;
        }


        static double nicenumber(double num, bool round = true)
        {
            double exp = System.Math.Floor(System.Math.Log10(num));
            double frc = num / System.Math.Pow(10, exp);
            double rnd = 10.0;
            if (round)
            {
                if (frc < 1.5) rnd = 1;
                else if (frc < 3) rnd = 2;
                else if (frc < 7) rnd = 5;
            }
            else
            {
                if (frc <= 1) rnd = 1;
                else if (frc <= 2) rnd = 2;
                else if (frc <= 5) rnd = 5;
            }
            return rnd * Math.Pow(10, exp);
        }


        //System.Math.Sqrt() faster than all custom implementation... also in Release compile mode
        unsafe static float FastSqrt(float x)
        {
            uint i = *(uint*)&x;
            // adjust bias
            i += 127 << 23;
            // approximation of square root
            i >>= 1;
            return *(float*)&i;
        }
        static float FastSqrtQuake3Union(float x)
        {
            if (x == 0) return 0;
            FloatIntUnion u;
            u.tmp = 0;
            float xhalf = 0.5f * x;
            u.f = x;
            u.tmp = 0x5f375a86 - (u.tmp >> 1);
            u.f = u.f * (1.5f - xhalf * u.f * u.f);
            return u.f * x;
        }
        unsafe static float FastSqrtQuake3(float x)
        {
            float xhalf = 0.5f * x;
            int i = *(int*)&x;
            i = 0x5f3759df - (i >> 1);
            x = *(float*)&i;
            x = x * (1.5f - xhalf * x * x);
            return x;
        }
        public static bool IsPointInsideTriangle(Vector2f a, Vector2f b, Vector2f c, Vector2f p)
        {
            return IsPointInsideTriangle(a, b, c, p.x, p.y);
        }
        public static bool IsPointInsideTriangle(Vector2f a, Vector2f b, Vector2f c, float px, float py)
        {
            var dx = px - c.x;
            var dy = py - c.y;

            var dx_cb = c.x - b.x;
            var dy_bc = b.y - c.y;

            var d = dy_bc * (a.x - c.x) + dx_cb * (a.y - c.y);
            var s = dy_bc * dx + dx_cb * dy;
            var t = (c.y - a.y) * dx + (a.x - c.x) * dy;

            if (d < 0) return s <= 0 && t <= 0 && s + t >= d;
            return s >= 0 && t >= 0 && s + t <= d;
        }


        /// <summary>
        /// Convert from homogenus clip space [-1,1] to screen space (pixels)
        /// </summary>
        public static Vector2f ClipSpaceToScreen(float x, float y, Vector2f screensize) =>
            ClipSpaceToScreen(x, y, screensize.width, screensize.height);
        public static Vector2f ClipSpaceToScreen(float x, float y, Vector2i screensize) =>
            ClipSpaceToScreen(x, y, 0, 0, screensize.width, screensize.height);
        public static Vector2f ClipSpaceToScreen(float x, float y, float screenWidth, float screenHeight) =>
            ClipSpaceToScreen(x, y, 0, 0, screenWidth, screenHeight);
        public static Rectangle4f ClipSpaceToScreen(Rectangle4f clip, Vector2f screensize) =>
            ClipSpaceToScreen(clip.x, clip.y, clip.width, clip.height, 0, 0, screensize.width, screensize.height);
        public static Rectangle4f ClipSpaceToScreen(float x, float y, float w, float h, float screenX, float screenY, float screenW, float screenH) =>
            new Rectangle4f((x + 1) * screenW / 2, (1 - y) * screenH / 2, w * screenW / 2, h * screenH / 2);
        public static Vector2f ClipSpaceToScreen(float x, float y, float screenX, float screenY, float screenW, float screenH) =>
            new Vector2f((x + 1) * screenW / 2, (1 - y) * screenH / 2);


        /// <summary>
        /// Convert from screen space (pixels) to homogenus clip space [-1,1]. z is any value.
        /// </summary>
        /// <remarks><inheritdoc cref="ViewportClip"/></remarks>
        public static Vector2f ScreenToClipSpace(float px, float py, Vector2f screensize)
            => ScreenToClipSpace(px, py, 0, 0, screensize.width, screensize.height);
        public static Vector2f ScreenToClipSpace(int px, int py, Vector2f screensize)
            => ScreenToClipSpace(px, py, 0, 0, screensize.width, screensize.height);
        public static Rectangle4f ScreenToClipSpace(Rectangle4i rect, Vector2f screensize)
            => ScreenToClipSpace(rect.x, rect.y, rect.width, rect.height, 0, 0, screensize.width, screensize.height);
        public static Rectangle4f ScreenToClipSpace(Rectangle4f rect, Vector2f screensize)
            => ScreenToClipSpace(rect.x, rect.y, rect.width, rect.height, 0, 0, screensize.width, screensize.height);
        public static Rectangle4f ScreenToClipSpace(float x, float y, float w, float h, float screenX, float screenY, float screenW, float screenH)
            => new Rectangle4f(2 * x / screenW - 1, 1 - 2 * y / screenH, 2 * w / screenW, 2 * h / screenH);
        public static Vector2f ScreenToClipSpace(float x, float y, float screenX, float screenY, float screenW, float screenH)
            => new Vector2f(2 * x / screenW - 1, 1 - 2 * y / screenH);


        /// <summary>
        /// Normalize point into rectangle space. The result will be in [0,1] coordinates
        /// </summary>
        public static Vector2f Normalize(ref Vector2i point, ref Rectangle4i rectangle) =>
            new Vector2f((point.x - rectangle.x) / rectangle.widthF, (point.y - rectangle.y) / rectangle.heightF);


        [StructLayout(LayoutKind.Explicit)]
        struct FloatIntUnion
        {
            [FieldOffset(0)]
            public float f;
            [FieldOffset(0)]
            public int tmp;
        }

        public static string RoundFloat(float f) => f < 99 && f > -99 ? f.ToString("G2") : ((int)f).ToString();

    }
}
