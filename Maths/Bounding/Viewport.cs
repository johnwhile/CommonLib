using Common.Maths;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Common
{

    /// <summary>
    /// Defines the 3D volume dimensions of a render target surface. Same layout of Directx11
    /// </summary>
    /// <remarks>
    /// screen coordinates
    /// <code>
    ///  ┌───► x (Width)
    ///  │
    ///  ▼
    ///  y (Height)
    /// </code>
    /// homogeneus coordinates 
    /// <code>
    ///     y
    ///     ▲
    ///     │
    ///  ───┼──► x
    ///     │
    /// </code>
    /// </remarks>
    /// 
    [DebuggerDisplay("{X} {Y} {Width} {Height} zmin: {MinDepth}, zmax: {MaxDepth}")]
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct ViewportClip : IEquatable<ViewportClip>
    {
        [FieldOffset(0)]
        Rectangle4i rect;
        [FieldOffset(16)]
        Vector2f depth;

        [FieldOffset(0)]
        /// <summary>
        /// Retrieves or sets the pixel coordinate of the upper-left corner of the viewport on the render target surface.
        /// </summary>
        public int X;
        /// <summary>
        /// Retrieves or sets the pixel coordinate of the upper-left corner of the viewport on the render target surface.
        /// </summary>
        [FieldOffset(4)]
        public int Y;
        /// <summary>
        /// Retrieves or sets the width(horizontal x) dimension of the viewport on the render target surface, in pixels.
        /// </summary>
        [FieldOffset(8)]
        public int Width;
        /// <summary>
        /// Retrieves or sets the height(vertical y) dimension of the viewport on the render target surface, in pixels.
        /// </summary>
        [FieldOffset(12)]
        public int Height;
        /// <summary>
        /// Retrieves or sets the minimum value of the clip volume.
        /// </summary>
        [FieldOffset(16)]
        public float MinDepth;
        /// <summary>
        ///  Retrieves or sets the maximum value of the clip volume.
        /// </summary>
        [FieldOffset(20)]
        public float MaxDepth;

        /// <summary>
        /// Return the aspect ratio as Width/Height"
        /// </summary>
        public float Aspect => rect.Aspect;

        /// <summary>
        /// Get the XY rectangle part
        /// </summary>
        public Rectangle4i Rectangle => rect;

        public Vector2i Size => rect.size;

        public ViewportClip(Rectangle4i rect, float mindepth, float maxdepth) :
            this(rect.x, rect.y, rect.width, rect.height, mindepth, maxdepth)
        {

        }

        public ViewportClip(System.Drawing.Rectangle rect) : this(rect.X, rect.Y, rect.Width, rect.Height)
        {

        }

        public ViewportClip(System.Drawing.Size size) : this(size.Width,size.Height)
        {

        }

        public ViewportClip(int width, int height) : this(0, 0, width, height)
        {

        }
        /// <summary>
        /// </summary>
        /// <param name="width">dX length</param>
        /// <param name="height">dY length</param>
        /// <param name="x">horizontal min</param>
        /// <param name="y">vertical min</param>
        public ViewportClip(int x, int y, int width, int height, float minDepth = 0, float maxDepth = 1) : this()
        {
            Height = height;
            Width = width;
            X = x;
            Y = y;
            MaxDepth = maxDepth;
            MinDepth = minDepth;
        }
        public ViewportClip(float x, float y, float width, float height, float minDepth = 0, float maxDepth = 1) :
            this((int)x, (int)y, (int)width, (int)height, minDepth, maxDepth)
        {
        }


        public static readonly ViewportClip Empty = new ViewportClip(0, 0, -1, -1, 0, 0);


        /// <summary>
        /// This matrix scales vertices according to the viewport dimensions and desired depth range and translates them 
        /// to the appropriate location on the render surface
        /// </summary>
        public Matrix4x4f ClipSpaceMatrix
        {
            get
            {
                var m = default(Matrix4x4f);
                m.m00 = Width / 2f;
                m.m11 = -Height / 2f;
                m.m22 = MaxDepth - MinDepth;
                m.m33 = 1f;
                m.m03 = X + m.m00;
                m.m13 = Y - m.m11;
                m.m23 = MinDepth;
                return m;
            }
        }
        public Matrix4x4f InverseClipSpaceMatrix
        {
            get
            {
                var m = default(Matrix4x4f);
                m.m00 = 2f / Width;
                m.m11 = -2f / Height;
                m.m22 = 1f / (MaxDepth - MinDepth);
                m.m33 = 1f;
                m.m03 = -1 - X * m.m00;
                m.m13 = 1 - Y * m.m11;
                m.m23 = -MinDepth * m.m22;
                return m;
            }
        }



        public bool Equals(ViewportClip other)
        {
            return this == other;
        }

        public static bool operator ==(ViewportClip left, ViewportClip right)
        {
            return left.X == right.X && left.Y == right.Y && left.Width == right.Width && left.Height == right.Height;
        }
        public static bool operator !=(ViewportClip left, ViewportClip right)
        {
            return !(left == right);
        }
        public override bool Equals(object obj)
        {
            return obj is ViewportClip ? (ViewportClip)obj == this : false;
        }

        public override int GetHashCode()
        {
            return (((((((((X * 397) ^ Y) * 397) ^Width) * 397) ^ Height) * 397) ^ MinDepth.GetHashCode()) * 397) ^ MaxDepth.GetHashCode();
        }
        public override string ToString()
        {
            return $"{X} {Y} {Width} {Height} , {MinDepth} {MaxDepth}";
        }
    }



    /// <summary>
    /// <inheritdoc cref="ViewportClip"/>
    /// </summary>
    [DebuggerDisplay("{X} {Y} {Width} {Height} zmin: {MinDepth}, zmax: {MaxDepth}")]
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct Viewport6f
    {
        [FieldOffset(0)]
        Rectangle4f rect;
        [FieldOffset(16)]
        Vector2f depth;

        [FieldOffset(0)]
        public float X;
        [FieldOffset(4)]
        public float Y;
        [FieldOffset(8)]
        public float Width;
        [FieldOffset(12)]
        public float Height;
        [FieldOffset(16)]
        public float MinDepth;
        [FieldOffset(20)]
        public float MaxDepth;

        /// <summary>
        /// </summary>
        public Viewport6f(float x, float y, float width, float height, float minDepth = 0, float maxDepth = 1) : this()
        {
            X = x;
            Y = y;
            Height = height;
            Width = width;
            MaxDepth = maxDepth;
            MinDepth = minDepth;
        }
        /// <summary>
        /// Universal calculated as Width / Height
        /// </summary>
        public float AspectRatio => Width / Height;

        public static readonly Viewport6f Empty = new Viewport6f(0, 0, -1, -1, 0, 0);
    }
}
