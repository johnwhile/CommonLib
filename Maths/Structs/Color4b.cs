using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable

namespace Common.Maths
{
    /// <summary>
    /// Unsinged Bytes items. <br/>
    /// Used also for RGBA color in 32bit. <b>It's implicitly converted from and to <see cref="System.Drawing.Color"/></b>
    /// because will be used as common color value
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("{ToString()}")]
    public struct Color4b : IEquatable<Color4b>
    {
        [FieldOffset(0)]
        public uint rgba;
        [FieldOffset(0)]
        Vector4b vector;

        [FieldOffset(0)]
        public byte r;
        [FieldOffset(1)]
        public byte g;
        [FieldOffset(2)]
        public byte b;
        [FieldOffset(3)]
        public byte a;

        static float fclamp(float a) { return a > 1 ? 1 : (a < 0 ? 0 : a); }


        /// <summary>
        /// Just to remeber: 0xAABBGGRR
        /// <code>
        /// R = 100 = 0x64;
        /// G = 149 = 0x95;
        /// B = 237 = 0xED;
        /// A = 255 = 0xFF;
        /// rgba = 0xFFED9564
        /// </code>
        /// </summary>
        public Color4b(uint rgba) : this()
        {
            this.rgba = rgba;
        }
        /// <summary>
        /// <inheritdoc cref="Vector4b"/><br/><br/>
        /// <b>Floats are clamped to [0,1] then multiply by 255</b>
        /// </summary>
        public Color4b(float x, float y, float z, float w = 1.0f) : this(
            (byte)(fclamp(x) * 255),
            (byte)(fclamp(y) * 255),
            (byte)(fclamp(z) * 255),
            (byte)(fclamp(w) * 255))
        { }

        /// <summary>
        /// <inheritdoc cref="Vector4b"/>
        /// </summary>
        public Color4b(byte r, byte g, byte b, byte a = 255) : this()
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }


        public float fR { get => r / 255f; set => r = (byte)(fclamp(value) * 255); }
        public float fG { get => g / 255f; set => g = (byte)(fclamp(value) * 255); }
        public float fB { get => b / 255f; set => b = (byte)(fclamp(value) * 255); }
        public float fA { get => a / 255f; set => a = (byte)(fclamp(value) * 255); }

        public uint RGBA
        {
            get => rgba;
            set => rgba = value;
        }

        public uint ABGR
        {
            get => (uint)(a << 24 | b << 16 | g << 8 | r);
            set
            {
                r = (byte)(value & 0xFF); value >>= 8;
                g = (byte)(value & 0xFF); value >>= 8;
                b = (byte)(value & 0xFF); value >>= 8;
                a = (byte)(value & 0xFF);
            }
        }
        public uint ARGB
        {
            get => (uint)(a << 24 | r << 16 | g << 8 | b);
            set
            {
                b = (byte)(value & 0xFF); value >>= 8;
                g = (byte)(value & 0xFF); value >>= 8;
                r = (byte)(value & 0xFF); value >>= 8;
                a = (byte)(value & 0xFF);
            }
        }

        public static readonly Color4b White = new Color4b(255, 255, 255);
        public static readonly Color4b Gray = new Color4b(128, 128, 128);
        public static readonly Color4b DarkGray = new Color4b(51, 51, 51);
        public static readonly Color4b Black = new Color4b(0, 0, 0);
        public static readonly Color4b Red = new Color4b(255, 0, 0);
        public static readonly Color4b Green = new Color4b(0, 255, 0);
        public static readonly Color4b Blue = new Color4b(0, 0, 255);
        public static readonly Color4b Yellow = new Color4b(255, 255, 0);
        public static readonly Color4b Cyan = new Color4b(0, 255, 255);
        public static readonly Color4b Magenta = new Color4b(255, 0, 255);
        public static readonly Color4b Trasparent = new Color4b(0, 0, 0, 0);
        public static readonly Color4b CornflowerBlue = new Color4b(0xFFED9564);
        public static readonly Color4b Toy = new Color4b(255, 127, 76);
        /// <summary>
        /// Get random color, also alpha channel is randomized
        /// </summary>
        public static Color4b RandomColor { get => new Color4b() { rgba = (uint)Mathelp.Rnd.Next(int.MinValue, int.MaxValue) }; }
        /// <summary>
        /// hue 0.0 = Red;  hue 1.0 = Blue , saturation = 1(max)
        /// </summary>
        public static Color4b Rainbow(float hue)
        {
            if (hue < 0.00f) return Red;
            if (hue < 0.25f) return new Color4b(1, hue / 0.25f, 0);
            if (hue < 0.50f) return new Color4b(1 - (hue - 0.25f) / 0.25f, 1, 0);
            if (hue < 0.75f) return new Color4b(0, 1, (hue - 0.5f) / 0.25f);
            if (hue < 1.00f) return new Color4b(0, 1 - (hue - 0.75f) / 0.25f, 1);
            return Blue;
        }
        /// <summary>
        /// Additive color mixing <i>left * amound + right * (1-amount)</i>
        /// </summary>
        public static Color4b Mixing(Color4b left, Color4b right, float amount = .5f)
        {
            float r = left.fR * amount + right.fR * (1 - amount);
            float g = left.fG * amount + right.fG * (1 - amount);
            float b = left.fB * amount + right.fB * (1 - amount);
            float a = left.fA * amount + right.fA * (1 - amount);
            return new Color4b(r, g, b, a);
        }

        public float Saturation
        {
            get
            {
                float r = fR;
                float g = fG;
                float b = fB;
                float max = r;
                float min = r;
                float s = 0;

                if (g > max) max = g;
                if (b > max) max = b;

                if (g < min) min = g;
                if (b < min) min = b;

                // if max == min, then there is no color and
                // the saturation is zero.
                //
                if (max != min)
                {
                    float l = (max + min) / 2;
                    s = (l <= 0.5f) ?
                        (max - min) / (max + min) :
                        (max - min) / (2 - max - min);
                }
                return s;
            }
        }
        public float Brightness
        {
            get
            {
                float r = fR;
                float g = fG;
                float b = fB;
                float max, min;
                max = r; min = r;
                if (g > max) max = g;
                if (b > max) max = b;
                if (g < min) min = g;
                if (b < min) min = b;
                return (max + min) / 2;
            }
        }
        public float Hue
        {
            get
            {
                if (r == g && g == b) return 0;

                float fr = fR;
                float fg = fG;
                float fb = fB;

                float max, min;
                float delta;
                float hue = 0.0f;

                max = fr; min = fr;

                if (fg > max) max = fg;
                if (fb > max) max = fb;

                if (fg < min) min = fg;
                if (fb < min) min = fb;

                delta = max - min;

                if (fr == max)
                {
                    hue = (fg - fb) / delta;
                }
                else if (fg == max)
                {
                    hue = 2 + (fb - fr) / delta;
                }
                else if (fb == max)
                {
                    hue = 4 + (fr - fg) / delta;
                }
                hue *= 60;

                if (hue < 0.0f)
                {
                    hue += 360.0f;
                }
                return hue;
            }
        }

        public static bool operator ==(Color4b left, Color4b right) => left.Equals(ref right);
        public static bool operator !=(Color4b left, Color4b right) => !left.Equals(ref right);
        public override bool Equals(object obj) => obj is Color4b v && Equals(ref v);
        public bool Equals(Color4b v) => Equals(ref v);
        public bool Equals(ref Color4b v) => rgba == v.rgba;

        public static implicit operator Color4b(ConsoleColor console)
        {
            Color4b c = consolecolormap[(int)console]; c.a = 255;
            return c;
        }

        public static implicit operator Vector4b(Color4b color) => color.vector;

        public static implicit operator uint(Color4b value) => value.rgba;

        public unsafe static implicit operator int(Color4b value) => *(int*)(&value);

        public unsafe static implicit operator Color4b(int value) => *(Color4b*)(&value);

        public static implicit operator Color4b(Color color) => new Color4b() { ARGB = (uint)color.ToArgb() };

        public static implicit operator Color(Color4b color) => Color.FromArgb((int)color.ARGB);

        public static implicit operator Vector4f(Color4b color) => new Vector4f(color.fR, color.fG, color.fB, color.fA);

        public static implicit operator Vector3f(Color4b color) => new Vector3f(color.fR, color.fG, color.fB);

        /// <summary>
        /// To a common hex representation of color RGBA, example for black= 000000FF
        /// </summary>
        public string ToHexString()=> ABGR.ToString("X8"); //invert order
        public override string ToString()=> a < 255 ? $"{r} {b} {g} {a}" : $"{r} {b} {g}";
        

        /// <summary>
        /// <code>
        /// Name         R    G    B
        /// -------------------------
        /// Black        00   00   00
        /// DarkBlue     00   00   80
        /// DarkGreen    00   80   00
        /// DarkCyan     00   80   80
        /// DarkRed      80   00   00
        /// DarkMagenta  80   00   80
        /// DarkYellow   80   80   00
        /// DarkGray     80   80   80
        /// Blue         00   00   FF
        /// Green        00   FF   00
        /// Cyan         00   FF   FF
        /// Red          FF   00   00
        /// Magenta      FF   00   FF
        /// Yellow       FF   FF   00
        /// Gray         C0   C0   C0
        /// White        FF   FF   FF
        /// </code>
        /// </summary>
        private static readonly Color4b[] consolecolormap = new Color4b[]
        {
            0x000000,
            0x000080,
            0x008000,
            0x008080,
            0x800000,
            0x800080,
            0x808000,
            0x808080,
            0x0000FF,
            0x00FF00,
            0x00FFFF,
            0xFF0000,
            0xFF00FF,
            0xFFFF00,
            0xC0C0C0,
            0xFFFFFF
        };
    }



    /// <summary>
    /// Unsinged Bytes items. <br/>
    /// <b>Same of <see cref="Color4b"/></b>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("ToString()")]
    public struct Vector4b : IEquatable<Vector4b>
    {
        [FieldOffset(0)]
        public byte x;
        [FieldOffset(1)]
        public byte y;
        [FieldOffset(2)]
        public byte z;
        [FieldOffset(3)]
        public byte w;
        [FieldOffset(0)]
        uint data;

        static float fclamp(float a) { return a > 1 ? 1 : (a < 0 ? 0 : a); }


        public Vector4b(int data) : this()
        {
            unchecked { this.data = (uint)data; };
        }
        public Vector4b(uint data) : this()
        {
            this.data = data;
        }
        /// <summary>
        /// <inheritdoc cref="Vector4b"/>
        /// </summary>
        public Vector4b(byte x, byte y, byte z, byte w = 255) : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        /// <summary>
        /// <inheritdoc cref="Vector4b"/><br/><br/>
        /// <b>Integers are clamped to [0,255]</b>
        /// </summary>
        public Vector4b(int x, int y, int z, int w = 255) :
            this((byte)x, (byte)y, (byte)z, (byte)w)
        { }


        public override bool Equals(object obj) => obj is Vector4b v && Equals(ref v);
        public bool Equals(Vector4b v) => Equals(ref v);
        public bool Equals(ref Vector4b v) => data == v.data;

        /// <summary>
        /// values are truncated to the byte maximum value with clamp(255)
        /// </summary>
        public static Vector4b operator +(Vector4b left, Vector4b right)
        {
            left.x = (byte)Mathelp.CLAMP(left.x + right.x, 0, 255);
            left.y = (byte)Mathelp.CLAMP(left.y + right.y, 0, 255);
            left.z = (byte)Mathelp.CLAMP(left.z + right.z, 0, 255);
            left.w = (byte)Mathelp.CLAMP(left.w + right.w, 0, 255);
            return left;
        }
        /// <summary>
        /// values are truncated to the byte maximum value with clamp(255)
        /// </summary>
        public static Vector4b operator -(Vector4b left, Vector4b right)
        {
            left.x = (byte)Mathelp.CLAMP(left.x - right.x, 0, 255);
            left.y = (byte)Mathelp.CLAMP(left.y - right.y, 0, 255);
            left.z = (byte)Mathelp.CLAMP(left.z - right.z, 0, 255);
            left.w = (byte)Mathelp.CLAMP(left.w - right.w, 0, 255);
            return left;
        }

        public static bool operator ==(Vector4b left, Vector4b right) => left.Equals(ref right);
        public static bool operator !=(Vector4b left, Vector4b right) => !left.Equals(ref right);


        public unsafe static implicit operator Color4b(Vector4b vector) => *(Color4b*)(&vector);

        public static implicit operator uint(Vector4b value) => value.data;

        public unsafe static implicit operator int(Vector4b value) => *(int*)(&value);

        public unsafe static implicit operator Vector4b(int value) => *(Vector4b*)(&value);

        public override string ToString() => $"{x} {y} {z} {w}";

    }

    /// <summary>
    /// 3x unsigned 8bit integers
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("ToString()")]
    public struct Vector3b
    {
        [FieldOffset(0)]
        public byte x;
        [FieldOffset(1)]
        public byte y;
        [FieldOffset(2)]
        public byte z;

        /// <summary>
        /// </summary>
        public Vector3b(byte x, byte y, byte z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString() => $"{x} {y} {z}";
    }
}


#pragma warning restore
