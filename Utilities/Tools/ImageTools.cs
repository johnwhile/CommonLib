using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Common.Maths;

#if DELETE

namespace Common.Tools
{
    /// <summary>
    /// Implementation tool of DXT algorithm
    /// </summary>
    public static class DXTtools
    {
        const byte C565_5_MASK = 0xF8; // 0xFF minus last three bits 
        const byte C565_6_MASK = 0xFC; // 0xFF minus last two bits 
        //helper values, need a index
        /// <summary> the 4x4 pixels block</summary>
        static Argb32[] pixels4x4 = new Argb32[16];
        /// <summary> the 4 midpoints </summary>
        static Argb32[] palette = new Argb32[4];

        static int Width, Height;     
        static int BytesPerBlock;

        /// <summary>
        /// Return the pixels table. The image must have a size multiple of 4
        /// </summary>
        /// <param name="source">require unmanaged buffer pointer</param>
        public static void DecompressStream(IntPtr sourcePtr , int Width, int Height , int DxtVersion, out Vector4b[,] stream)
        {
            DXTtools.Width = Width;
            DXTtools.Height = Height;

            if (Width % 4 != 0 || Height % 4 != 0) throw new ArgumentException("Texture must have a size multiple of 4 pixels");

            stream = new Vector4b[Width , Height];

            unsafe
            {
                switch (DxtVersion)
                {
                    case 1:
                        {                          
                            // more elegant solution without memorystream
                            BytesPerBlock = 8;
                            ColorBlock8* psource = (ColorBlock8*)sourcePtr.ToPointer();
                            
                            for (int y = 0, count = 0; y < Height; y += 4)
                                for (int x = 0; x < Width; x += 4,count++)
                                    decompressBlockDxt1(psource[count], x, y, ref stream);
                        }
                        break;
                    case 5:
                        {
                            BytesPerBlock = 16;
                            ColorBlock16* psource = (ColorBlock16*)sourcePtr.ToPointer();
                            for (int y = 0, count = 0; y < Height; y += 4)
                                for (int x = 0; x < Width; x += 4,count++)
                                    decompressBlockDxt5(psource[count], x, y, ref stream);
                        }
                        break;

                    default: throw new NotImplementedException("Dxt" + DxtVersion + " decompressor not found");
                }
            }
        }
        /// <summary>
        /// Write the pixels table. The image must have a size multiple of 4
        /// </summary>
        /// <param name="destPtr">require unmanaged buffer pointer</param>
        public static void CompressStream(IntPtr destPtr , int Width, int Height , int DxtVersion, Vector4b[,] stream)
        {
            //throw new NotImplementedException("work in progress...");
            DXTtools.Width = Width;
            DXTtools.Height = Height;
            
            if (Width % 4 != 0 || Height % 4 != 0) throw new ArgumentException("Texture must have a size multiple of 4 pixels");
            if (stream.GetLength(0) != Width || stream.GetLength(1)!= Height) throw new ArgumentOutOfRangeException("stream size wrong");
            unsafe
            {
                switch (DxtVersion)
                {
                    case 1:
                        {
                            BytesPerBlock = 8;
                            ColorBlock8* psource = (ColorBlock8*)destPtr.ToPointer();
                            for (int y = 0, count = 0; y < Height; y += 4)
                                for (int x = 0; x < Width; x += 4)
                                    psource[count++] = compressBlockDxt1(stream, x, y);
                        }
                        break;

                    default: throw new NotImplementedException("Dxt" + DxtVersion + " compressor not found");
                }
            }
        }


        static ColorBlock8 compressBlockDxt1(Vector4b[,] image, int x, int y)
        {
            Argb32 minColor, maxColor;
            ColorBlock8 block = new ColorBlock8();

            // read block
            for (int j = 0, k = 0; j < 4; j++) 
                for (int i = 0; i < 4; i++, k++) 
                    pixels4x4[k] = image[x + i , y + j];

            /*
            GetMinMaxColors_box(out minColor, out maxColor);
            SelectDiagonal(ref minColor, ref maxColor);
            InsetBBox(ref minColor, ref maxColor);

            block.color0 = convert565(maxColor);
            block.color1 = convert565(minColor);

            if (block.color0 < block.color1)
            {
                swap<ushort>(ref block.color0, ref block.color1);
                swap<Argb32>(ref maxColor, ref minColor);
            }

            block.location = findIndices_fast(minColor, maxColor);
            */

            // find min and max colors
            getMinMax_bydist(out minColor, out maxColor);

            block.color0 = convert565(maxColor);
            block.color1 = convert565(minColor);

            if (block.color0 < block.color1)
            {
                swap<ushort>(ref block.color0, ref block.color1);
                swap<Argb32>(ref maxColor, ref minColor);
            }
            block.location = findIndices_slow(minColor, maxColor);

            return block;
        }
        /// <summary>
        /// Unpack the packed 8byte block to 16 unpacked colors, carefully when paste the unpacked colors,
        /// they correspond to a matrix of 4x4 pixels in the final image
        /// </summary>
        /// <param name="x">left corner or 4x4 pixels block</param>
        /// <param name="y">top corner or 4x4 pixels block</param>
        static void decompressBlockDxt1(ColorBlock8 packed, int x, int y, ref Vector4b[,] image)
        {
            getPalette(packed, true);

            // paste the 4 color to image's 4x4 pixels block
            for (int j = 0; j < 4; j++)
                for (int i = 0; i < 4; i++)
                {
                    int index = (int)((packed.location >> (2 * (4 * j + i))) & 0x03);
                    // use i and j offsets to find the location in the stream
                    image[x + i,y + j] = palette[index];
                }
        }
        /// <summary>
        /// <seealso cref="decompressBlockDxt1"/>
        /// </summary>
        static void decompressBlockDxt5(ColorBlock16 packed, int x, int y, ref Vector4b[,] image)
        {
            ColorBlock8 colors = packed.colors;
            AlphaBlock8 alphas = packed.alphas;

            getPalette(colors, false);

            // paste the 4 color to image's 4x4 pixels block
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    byte finalAlpha;
                    int alphaCodeIndex = 3 * (4 * j + i);
                    int alphaCode;

                    if 
                        (alphaCodeIndex <= 12) alphaCode = (alphas.alphaCode2 >> alphaCodeIndex) & 0x07;
                    else if 
                        (alphaCodeIndex == 15) alphaCode = (int)(((int)alphas.alphaCode2 >> 15) | (((int)alphas.alphaCode1 << 1) & 0x06));
                    else 
                        alphaCode = (int)((alphas.alphaCode1 >> (alphaCodeIndex - 16)) & 0x07);

                    if (alphaCode == 0)
                        finalAlpha = alphas.alpha0;
                    else if (alphaCode == 1)
                        finalAlpha = alphas.alpha1;
                    else
                    {
                        if (alphas.alpha0 > alphas.alpha1)
                        {
                            finalAlpha = (byte)(((8 - alphaCode) * alphas.alpha0 + (alphaCode - 1) * alphas.alpha1) / 7);
                        }
                        else
                        {
                            if (alphaCode == 6) finalAlpha = 0;
                            else if (alphaCode == 7) finalAlpha = 255;
                            else finalAlpha = (byte)(((6 - alphaCode) * alphas.alpha0 + (alphaCode - 1) * alphas.alpha1) / 5);
                        }
                    }

                    int index = (int)((colors.location >> (2 * (4 * j + i))) & 0x03);
                    // use i and j offsets to find the location in the stream
                    Argb32 color = palette[index];
                    color.a = finalAlpha;
                    image[x + i,y + j] = color;
                }
            }
        }
        /// <summary>
        /// To default R8G8B8 format
        /// </summary>
        /// <remarks>
        /// There are two way to scale 5bit to 8bit channel 
        /// 1) the true equation is (byte)bit8 = (byte)(bit5 * 255.0 / 31.0) but it's slow.
        /// 2) you can see that 255.0/31.0 = 8.2258.. can be rounded to (8 + 0.25) or (2^3 + 1/2^2)
        ///    the equation (byte)bit8 = (byte)(bit5*8 + bit5*0.25) become bit8 = (bit5&lt;&lt;3) | (bit5&gt;&gt;2)
        ///    and not require cast and multiplications but generate a little error (max+-1%) in the middle of 0-255 value
        /// </remarks>
        static Argb32 convert888(ushort rgb565)
        {
            Argb32 argb = new Argb32();

            //string str = Tool.GetBinaryString(color);
            byte r5 = (byte)((rgb565 >> 11) & 0x1F);
            byte g6 = (byte)((rgb565 >> 5) & 0x3F);
            byte b5 = (byte)(rgb565 & 0x1F);

            // scale up to 8 bits,
            argb.r = (byte)((r5 << 3) | (r5 >> 2));
            argb.g = (byte)((g6 << 2) | (g6 >> 4));
            argb.b = (byte)((b5 << 3) | (b5 >> 2));
            //r8 = (byte)(r5 * 255.0f / 31.0f);
            //g8 = (byte)(g6 * 255.0f / 63.0f);
            //b8 = (byte)(b5 * 255.0f / 31.0f);
            argb.a = 255;

            return argb;
        }
        /// <summary>
        /// To R5G6B5 format
        /// </summary>
        static ushort convert565(Argb32 argb)
        {
            //string str = Tool.GetBinaryString(color);
            ushort r5 = (ushort)(argb.r * 31.0f / 255.0f);
            ushort g6 = (ushort)(argb.g * 63.0f / 255.0f);
            ushort b5 = (ushort)(argb.b * 31.0f / 255.0f);
            return (ushort)(r5 << 11 | g6 << 5 | b5);
        }
        /// <summary>
        /// Populate palette's colors with 4 colors
        /// </summary>
        static void getPalette(ColorBlock8 colorblock,bool isDxt1)
        {
            // unpack the endpoints
            palette[0] = convert888(colorblock.color0);
            palette[1] = convert888(colorblock.color1);

            // generate the midpoints
            if (colorblock.color0 > colorblock.color1 || !isDxt1)
            {
                palette[2].r = (byte)((2 * palette[0].r + palette[1].r) / 3);
                palette[2].g = (byte)((2 * palette[0].g + palette[1].g) / 3);
                palette[2].b = (byte)((2 * palette[0].b + palette[1].b) / 3);
                palette[3].r = (byte)((palette[0].r + 2 * palette[1].r) / 3);
                palette[3].g = (byte)((palette[0].g + 2 * palette[1].g) / 3);
                palette[3].b = (byte)((palette[0].b + 2 * palette[1].b) / 3);
            }
            else
            {
                palette[2].r = (byte)((palette[0].r + palette[1].r) / 2);
                palette[2].g = (byte)((palette[0].g + palette[1].g) / 2);
                palette[2].b = (byte)((palette[0].b + palette[1].b) / 2);                
                palette[3].r = 0;
                palette[3].g = 0;
                palette[3].b = 0;
            }

            // fill in alpha for the intermediate values
            palette[2].a = 255;
            palette[3].a = (byte)((colorblock.color0 > colorblock.color1 || !isDxt1) ? 255 : 0);
        }
        /// <summary>
        /// Brute force using euclidean distance
        /// </summary>
        static void getMinMax_bydist(out Argb32 minColor, out Argb32 maxColor)
        {
            minColor = maxColor = pixels4x4[0];
            int maxDistance = -1;

            // loop all possible combination of i and j
            for (int i = 0; i < 16; i++)
                for (int j = i + 1; j < 16; j++)
                {
                    int distance = colorDistance(pixels4x4[i], pixels4x4[j]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        minColor = pixels4x4[i];
                        maxColor = pixels4x4[j];
                    }
                }
        }
        /// <summary>
        /// Brute force using luminance
        /// </summary>
        static void getMinMax_bylum(out Argb32 minColor, out Argb32 maxColor)
        {
            minColor = maxColor =new Argb32();
            int maxLuminance = -1;
            int minLuminance = int.MaxValue;

            for (int i = 0; i < 16; i++)
            {
                int luminance = colorLuminance(pixels4x4[i]);
                if (luminance > maxLuminance)
                {
                    maxLuminance = luminance;
                    maxColor = pixels4x4[i];
                }
                if (luminance < minLuminance)
                {
                    minLuminance = luminance;
                    minColor = pixels4x4[i];
                }
            }
        }
        /// <summary>
        /// find minimum and maximum colors based on bounding box in color space
        /// </summary>
        static void getMinMax_bybox(out Argb32 maxColor, out Argb32 minColor)
        {
            maxColor = new Argb32(0, 0, 0);
            minColor = new Argb32(255, 255, 255);

            for (int i = 0; i < 16; i++)
            {
                maxColor = Argb32.Max(maxColor, pixels4x4[i]);
                minColor = Argb32.Min(minColor, pixels4x4[i]);
            }
        }
        /// <summary>
        /// Finding Matching Points On The Line Through Color Space
        /// </summary>
        static uint findIndices_slow(Argb32 minColor, Argb32 maxColor)
        {
            int[] indices = new int[16];

            palette[0].r = (byte)((maxColor.r & C565_5_MASK) | (maxColor.r >> 5));
            palette[0].g = (byte)((maxColor.g & C565_6_MASK) | (maxColor.g >> 6));
            palette[0].b = (byte)((maxColor.b & C565_5_MASK) | (maxColor.b >> 5));
            palette[1].r = (byte)((minColor.r & C565_5_MASK) | (minColor.r >> 5));
            palette[1].g = (byte)((minColor.g & C565_6_MASK) | (minColor.g >> 6));
            palette[1].b = (byte)((minColor.b & C565_5_MASK) | (minColor.b >> 5));
            palette[2].r = (byte)((2 * palette[0].r + palette[1].r) / 3);
            palette[2].g = (byte)((2 * palette[0].g + palette[1].g) / 3);
            palette[2].b = (byte)((2 * palette[0].b + palette[1].b) / 3);
            palette[3].r = (byte)((palette[0].r + 2 * palette[1].r) / 3);
            palette[3].g = (byte)((palette[0].g + 2 * palette[1].g) / 3);
            palette[3].b = (byte)((palette[0].b + 2 * palette[1].b) / 3);

            for (int i = 0; i < 16; i++)
            {
                int minDistance = int.MaxValue;
                for (int j = 0; j < 4; j++)
                {
                    int dist = colorDistance(pixels4x4[i], palette[j]);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        indices[i] = j;
                    }
                }
            }

            int result = 0;

            for (int i = 0; i < 16; i++)
            {
                //result |= indices[i] << (i << 1);
                result |= (indices[i] & 0x03) << (i * 2);             
            }

            //string str = Tool.GetBinaryString(result);

            return (uint)result;
        }

        static uint findIndices_fast(Argb32 minColor, Argb32 maxColor)
        {
            palette[0] = maxColor;
            palette[1] = minColor;

            palette[2].r = (byte)((2 * palette[0].r + palette[1].r) / 3);
            palette[2].g = (byte)((2 * palette[0].g + palette[1].g) / 3);
            palette[2].b = (byte)((2 * palette[0].b + palette[1].b) / 3);
            palette[3].r = (byte)((palette[0].r + 2 * palette[1].r) / 3);
            palette[3].g = (byte)((palette[0].g + 2 * palette[1].g) / 3);
            palette[3].b = (byte)((palette[0].b + 2 * palette[1].b) / 3);

            uint indices = 0;
            for (int i = 0; i < 16; i++)
            {
                float d0 = colorDistance(palette[0], pixels4x4[i]);
                float d1 = colorDistance(palette[1], pixels4x4[i]);
                float d2 = colorDistance(palette[2], pixels4x4[i]);
                float d3 = colorDistance(palette[3], pixels4x4[i]);

                bool b0 = d0 > d3;
                bool b1 = d1 > d2;
                bool b2 = d0 > d2;
                bool b3 = d1 > d3;
                bool b4 = d2 > d3;

                uint x0 = (uint)(b1 & b2 ? 1 : 0);
                uint x1 = (uint)(b0 & b3 ? 1 : 0);
                uint x2 = (uint)(b0 & b4 ? 1 : 0);

                indices |= (x2 | ((x0 | x1) << 1)) << (2 * i);
            }

            return indices;
        }
    
        /// <summary>
        /// Euclidean distance = dot(c1,c2)
        /// </summary>
        static int colorDistance(Argb32 c1, Argb32 c2)
        {
            int r = c1.r - c2.r;
            int g = c1.g - c2.g;
            int b = c1.b - c2.b;
            return (r * r + g * g + b * b);
        }
        /// <summary>
        /// </summary>
        static int colorLuminance(Argb32 color)
        {
            return (color.r + color.g * 2 + color.b);
        }

        static void selectDiagonal(ref Argb32 minColor, ref Argb32 maxColor)
        {
            Vector3f colormax = (Vector3f)maxColor;
            Vector3f colormin = (Vector3f)minColor;
            Vector3f center = (colormax + colormin) * 0.5f;

            float covariancex = 0;
            float covariancey = 0;
            for (uint i = 0; i < 16; i++)
            {
                Vector3f t = (Vector3f)pixels4x4[i] - center;
                covariancex = t.x * t.z;
                covariancey = t.y * t.z;
            }

            if (covariancex < 0) swap<float>(ref colormax.x, ref colormin.x);
            if (covariancey < 0) swap<float>(ref colormax.y, ref colormin.y);

            maxColor = new Argb32((byte)colormax.x, (byte)colormax.y, (byte)colormax.z);
            minColor = new Argb32((byte)colormin.x, (byte)colormin.y, (byte)colormin.z);
        }

        static void insetBBox(ref Argb32 minColor, ref Argb32 maxColor)
        {
            float div = 16.0f - (8.0f / 255.0f) / 16.0f;
            float insetx = (maxColor.r - minColor.r) / div;
            float insety = (maxColor.g - minColor.g) / div;
            float insetz = (maxColor.b - minColor.b) / div;

            maxColor.r = clamp(maxColor.r - insetx, 0, 255);
            maxColor.g = clamp(maxColor.g - insety, 0, 255);
            maxColor.b = clamp(maxColor.b - insetz, 0, 255);

            minColor.r = clamp(minColor.r + insetx, 0, 255);
            minColor.g = clamp(minColor.g + insety, 0, 255);
            minColor.b = clamp(minColor.b + insetz, 0, 255);
        }

        static void swap<T>(ref T a, ref T b) { T tmp = a; a = b; b = tmp; }

        static byte clamp(float a,byte min,byte max) { return (byte)(a > max ? max : a < min ? min : a); }


        #region nested struct
        /// <summary>
        /// Is a 8bytes block that rappresent 4x4 pixel
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
        struct ColorBlock8
        {
            /// <summary>
            /// R5G6B5 max color
            /// </summary>
            public ushort color0;
            /// <summary>
            /// R5G6B5 min color
            /// </summary>
            public ushort color1;
            /// <summary>
            /// 2bit x 16 indices
            /// </summary>
            public uint location;

            public override string ToString()
            {
                return string.Format("c0:{0} c1:{1} location:{2}", color0, color1, location);
            }
        }
        /// <summary>
        /// Is a 8bytes block (for dxt5) that rappresent alpha
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
        struct AlphaBlock8
        {
            public byte alpha0;
            public byte alpha1;
            public ushort alphaCode2;
            public uint alphaCode1;
        }
        /// <summary>
        /// dxt5 block
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
        struct ColorBlock16
        {
            public AlphaBlock8 alphas;
            public ColorBlock8 colors;

        }

        /// <summary>
        /// Usefull conversion of Color32 to avoid continuos unpacking uint value, the casting in Color32 is relative fast
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
        struct Argb32
        {
            //  with this sorting, the casting to uint generate a correct argb struct
            public byte b, g, r, a;

            public Argb32(byte r, byte g, byte b)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = 255;
            }
            static byte clamp(int a) { return (byte)(a > 255 ? 255 : a < 0 ? 0 : a); }
            static byte max(byte a, byte b) { return a > b ? a : b; }
            static byte min(byte a, byte b) { return a < b ? a : b; }


            public static implicit operator Vector3f(Argb32 argb) 
            {
                unsafe { return new Vector3f(argb.r, argb.g, argb.b); }
            }
            public static implicit operator Vector4b(Argb32 argb)
            {
                unsafe { return *(Vector4b*)&argb; }
            }
            public static implicit operator Argb32(Vector4b color) 
            {
                unsafe { return *(Argb32*)&color; }
            }

            public override string ToString()
            {
                return string.Format("{0},{1},{2},{3}", r, g, b, a);
            }

            public static Argb32 operator /(Argb32 col, byte div)
            {
                Argb32 res = col;
                res.r /= div;
                res.g /= div;
                res.b /= div;
                return res;
            }
            public static Argb32 operator +(Argb32 col0, Argb32 col1)
            {
                Argb32 res = new Argb32();
                res.r = clamp(col0.r + col1.r);
                res.g = clamp(col0.g + col1.g);
                res.b = clamp(col0.b + col1.b);
                return res;
            }
            public static Argb32 Max(Argb32 a, Argb32 b)
            {
                return new Argb32 { a = 255, r = max(a.r, b.r), g = max(a.g, b.g), b = max(a.b, b.b) };
            }
            public static Argb32 Min(Argb32 a, Argb32 b)
            {
                return new Argb32 { a = 255, r = min(a.r, b.r), g = min(a.g, b.g), b = min(a.b, b.b) };
            }
            public static Argb32 Avarage(Argb32 c0, Argb32 c1)
            {
                byte r = clamp((c0.r + c1.r) / 2);
                byte g = clamp((c0.g + c1.g) / 2);
                byte b = clamp((c0.b + c1.b) / 2);
                return new Argb32(r, g, b);
            }
        }
        #endregion
    }


    public static class PixelTools
    {
        #region utilities structure 
        // usefull when use pointer
        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = sizeof(ushort) * 4)]
        private struct Uint16x4 { public UInt16 x, y, z, w;}
        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = sizeof(byte) * 4)]
        private struct Byte4 { public Byte x, y, z, w;}
        #endregion

        #region Pixels Converters
       
        #region Unpackers
        public static Vector4f unpack_A1R5G5B5(UInt16 value)
        {
            // A = 10000000 00000000
            // R = 01111100 00000000
            // G = 00000011 11100000
            // B = 00000000 00011111
            // example : A:255 R:255 G:0 B:255 = 1111110000011111
            // 31.0f = 2^5-1 
            // b = (1111110000011111)>>0 & (0000000000011111) = 11111

            //string valueString = Tool.GetBinaryString(value);

            Vector4f v = new Vector4f(
                ((value >> 10) & 0x1F) / 31.0f,
                ((value >> 5) & 0x1F) / 31.0f,
                (value & 0x1F) / 31.0f,
                ((value >> 15) & 0x1) == 0 ? 0 : 1.0f);

            return v;
        }
        public static Vector4f unpack_X1R5G5B5(UInt16 value)
        {
            // A = 10000000 00000000
            // R = 01111100 00000000
            // G = 00000011 11100000
            // B = 00000000 00011111
            // example : A:255 R:255 G:0 B:255 = 1111110000011111
            // 31.0f = 2^5-1 

            //string valueString = Tool.GetBinaryString(value);

            Vector4f v = new Vector4f(
                ((value >> 10) & 0x1F) / 31.0f,
                ((value >> 5) & 0x1F) / 31.0f,
                (value & 0x1F) / 31.0f,
                1.0f);
            return v;
        }
        public static Vector4f unpack_A4R4G4B4(UInt16 value)
        {
            //15 = 2^4-1
            return new Vector4f(
                ((value >> 8) & 0x0F) / 15.0f,
                ((value >> 4) & 0x0F) / 15.0f,
                (value & 0x0F) / 15.0f,
                ((value >> 12) & 0x0F) / 15.0f);
        }
        public static Vector4f unpack_X4R4G4B4(UInt16 value)
        {
            return new Vector4f(
                ((value >> 8) & 0x0F) / 15.0f,
                ((value >> 4) & 0x0F) / 15.0f,
                (value & 0x0F) / 15.0f,
                1.0f);
        }
        public static Vector4f unpack_A8R8G8B8(UInt32 value)
        {
            unsafe { Byte4 v = *(Byte4*)&value; return new Vector4f(v.z / 255.0f, v.y / 255.0f, v.x / 255.0f, v.w / 255.0f); }
            /*
            //255 = 2^8-1
            return new Vector4(
                ((value >> 16) & 0xFF) / 255.0f,
                ((value >> 8) & 0xFF) / 255.0f,
                ((value >> 0) & 0xFF) / 255.0f,
                ((value >> 24) & 0xFF) / 255.0f);
            */
        }
        public static Vector4f unpack_X8R8G8B8(UInt32 value)
        {
            unsafe { Byte4 v = *(Byte4*)&value; return new Vector4f(v.z / 255.0f, v.y / 255.0f, v.x / 255.0f, 1.0f); }
            /*
            return new Vector4(
                ((value >> 16) & 0xFF) / 255.0f,
                ((value >> 8) & 0xFF) / 255.0f,
                ((value >> 0) & 0xFF) / 255.0f,
                1.0f);
            */
        }
        public static Vector4f unpack_A8B8G8R8(UInt32 value)
        {
            unsafe { Byte4 v = *(Byte4*)&value; return new Vector4f(v.x / 255.0f, v.y / 255.0f, v.z / 255.0f, v.w / 255.0f); }
            /*
            return new Vector4(
                ((value >> 0) & 0xFF) / 255.0f,
                ((value >> 8) & 0xFF) / 255.0f,
                ((value >> 16) & 0xFF) / 255.0f,
                ((value >> 24) & 0xFF) / 255.0f);
            */
        }
        public static Vector4f unpack_X8B8G8R8(UInt32 value)
        {
            unsafe { Byte4 v = *(Byte4*)&value; return new Vector4f(v.x / 255.0f, v.y / 255.0f, v.z / 255.0f, 1.0f); }
            /*
            return new Vector4(
                ((value >> 0) & mask8bit) / 255.0f,
                ((value >> 8) & mask8bit) / 255.0f,
                ((value >> 16) & mask8bit) / 255.0f,
                1.0f);
            */
        }
        public static Vector4f unpack_A16B16G16R16(UInt64 value)
        {
            // more elegant version
            unsafe
            {
                Uint16x4 v = *(Uint16x4*)&value;
                return new Vector4f(v.x / 65535.0f, v.y / 65535.0f, v.z / 65535.0f, v.w / 65535.0f);
            }
            /*
            //65535 = 2^16-1
            return new Vector4(
                ((value >> 0) & 0xFFFF) / 65535.0f,
                ((value >> 16) & 0xFFFF) / 65535.0f,
                ((value >> 32) & 0xFFFF) / 65535.0f,
                ((value >> 48) & 0xFFFF) / 65535.0f);
             */
        }
        public static Vector4f unpack_A16B16G16R16F(UInt64 value)
        {
            unsafe { VectorHalf4 v = *(VectorHalf4*)&value; return (Vector4f)v; }
        }
        public static Vector4f unpack_A32B32G32R32F(Uint128 value)
        {
            unsafe { return *(Vector4f*)&value; }
        }
        #endregion

        #region Packers
        public static UInt16 pack_A1R5G5B5(Vector4f value)
        {
            // remember that red = [00000000 00000000 01111100 00000000] --> (ushort) = [01111100 00000000] so casting
            // make a simple cut
            uint a = (value.w > 0.5f) ? (uint)(1 << 15) : 0;
            uint r = (uint)(value.x * 31);
            uint g = (uint)(value.y * 31);
            uint b = (uint)(value.z * 31);
            // fix surplus data because value.x can be 1.1f
            if (r > 31) r = 31;
            if (g > 31) g = 31;
            if (b > 31) b = 31;

            //string str = Tool.GetBinaryString(data);

            return (UInt16)(a | r << 10 | g << 5 | b);
        }
        public static UInt16 pack_X1R5G5B5(Vector4f value)
        {
            uint r = (uint)(value.x * 31);
            uint g = (uint)(value.y * 31);
            uint b = (uint)(value.z * 31);
            if (r > 31) r = 31;
            if (g > 31) g = 31;
            if (b > 31) b = 31;
            return (UInt16)(1 << 15 | r << 10 | g << 5 | b);
        }
        public static UInt16 pack_A4R4G4B4(Vector4f value)
        {
            uint a = (uint)(value.w * 15);
            uint r = (uint)(value.x * 15);
            uint g = (uint)(value.y * 15);
            uint b = (uint)(value.z * 15);
            if (a > 15) a = 15;
            if (r > 15) r = 15;
            if (g > 15) g = 15;
            if (b > 15) b = 15;
            return (UInt16)(a << 12 | r << 8 | g << 4 | b);
        }
        public static UInt16 pack_X4R4G4B4(Vector4f value)
        {
            uint r = (uint)(value.x * 15);
            uint g = (uint)(value.y * 15);
            uint b = (uint)(value.z * 15);
            if (r > 15) r = 15;
            if (g > 15) g = 15;
            if (b > 15) b = 15;
            return (UInt16)(61440 | r << 8 | g << 4 | b);
        }
        public static UInt32 pack_A8R8G8B8(Vector4f value)
        {
            uint a = (uint)(value.w * 255.0f);
            uint r = (uint)(value.x * 255.0f);
            uint g = (uint)(value.y * 255.0f);
            uint b = (uint)(value.z * 255.0f);
            if (a > 255) a = 255;
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            return (UInt32)(a << 24 | r << 16 | g << 8 | b);
        }
        public static UInt32 pack_X8R8G8B8(Vector4f value)
        {
            uint r = (uint)(value.x * 255.0f);
            uint g = (uint)(value.y * 255.0f);
            uint b = (uint)(value.z * 255.0f);
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            return (UInt32)(255 << 24 | r << 16 | g << 8 | b);
        }
        public static UInt32 pack_A8B8G8R8(Vector4f value)
        {
            uint a = (uint)(value.w * 255.0f);
            uint r = (uint)(value.x * 255.0f);
            uint g = (uint)(value.y * 255.0f);
            uint b = (uint)(value.z * 255.0f);
            if (a > 255) a = 255;
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            return (UInt32)(a << 24 | b << 16 | g << 8 | r);
        }
        public static UInt32 pack_X8B8G8R8(Vector4f value)
        {
            uint r = (uint)(value.x * 255.0f);
            uint g = (uint)(value.y * 255.0f);
            uint b = (uint)(value.z * 255.0f);
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            return (UInt32)(255 << 24 | b << 16 | g << 8 | r);
        }
        public static UInt64 pack_A16B16G16R16(Vector4f value)
        {
            ulong a = (ulong)(value.w * 65535.0f);
            ulong r = (ulong)(value.x * 65535.0f);
            ulong g = (ulong)(value.y * 65535.0f);
            ulong b = (ulong)(value.z * 65535.0f);
            if (a > 65535) a = 65535;
            if (r > 65535) r = 65535;
            if (g > 65535) g = 65535;
            if (b > 65535) b = 65535;
            return (UInt64)(a << 48 | b << 32 | g << 16 | r);
        }
        public static UInt64 pack_A16B16G16R16F(Vector4f value)
        {
            VectorHalf4 v = (VectorHalf4)value;
            unsafe { return *(UInt64*)&v; }
        }
        public static Uint128 pack_A32B32G32R32F(Vector4f value)
        {
            Vector4f v = value;
            unsafe { return *(Uint128*)&v; }
        }
        #endregion

        #endregion

        #region tools

        public delegate T Packer<T>(Vector4f color) where T : struct;
        public delegate Vector4f UnPacker<T>(T data) where T : struct;

        public static Packer<UInt16> packer16(Format format)
        {
            switch (format)
            {
                case Format.A4R4G4B4: return new  Packer<UInt16>(pack_A4R4G4B4);
                case Format.X4R4G4B4: return new  Packer<UInt16>(pack_X4R4G4B4);
                case Format.A1R5G5B5: return new  Packer<UInt16>(pack_A1R5G5B5);
                case Format.X1R5G5B5: return new  Packer<UInt16>(pack_X1R5G5B5);
                default: throw new NotSupportedException("Decoder not found");
            }
        }
        public static Packer<UInt32> packer32(Format format)
        {
            switch (format)
            {
                case Format.A8R8G8B8: return new  Packer<UInt32>( pack_A8R8G8B8);
                case Format.X8R8G8B8: return new  Packer<UInt32>( pack_X8R8G8B8);
                case Format.A8B8G8R8: return new  Packer<UInt32>( pack_A8B8G8R8);
                case Format.X8B8G8R8: return new  Packer<UInt32>( pack_X8B8G8R8);
                default: throw new NotSupportedException("Decoder not found");
            }
        }
        public static Packer<UInt64> packer64(Format format)
        {
            switch (format)
            {
                case Format.A16B16G16R16: return new  Packer<UInt64>( pack_A16B16G16R16);
                case Format.A16B16G16R16F: return new  Packer<UInt64>( pack_A16B16G16R16F);
                default: throw new NotSupportedException("Decoder not found");
            }
        }
        public static Packer<Uint128> packer128(Format format)
        {
            switch (format)
            {
                case Format.A32B32G32R32F: return new  Packer<Uint128>( pack_A32B32G32R32F);
                default: throw new NotSupportedException("Decoder not found");
            }
        }


        public static UnPacker<UInt16> unpacker16(Format format)
        {
            switch (format)
            {
                case Format.A4R4G4B4: return new  UnPacker<UInt16>(unpack_A4R4G4B4);
                case Format.X4R4G4B4: return new  UnPacker<UInt16>(unpack_X4R4G4B4);
                case Format.A1R5G5B5: return new  UnPacker<UInt16>(unpack_A1R5G5B5);
                case Format.X1R5G5B5: return new  UnPacker<UInt16>(unpack_X1R5G5B5);
                default: throw new NotSupportedException("Decoder not found");
            }
        }
        public static UnPacker<UInt32> unpacker32(Format format)
        {
            switch (format)
            {
                case Format.A8R8G8B8: return new UnPacker<UInt32>(unpack_A8R8G8B8);
                case Format.X8R8G8B8: return new UnPacker<UInt32>(unpack_X8R8G8B8);
                case Format.A8B8G8R8: return new UnPacker<UInt32>(unpack_A8B8G8R8);
                case Format.X8B8G8R8: return new UnPacker<UInt32>(unpack_X8B8G8R8);
                default: throw new NotSupportedException("Decoder not found");
            }
        }
        public static UnPacker<UInt64> unpacker64(Format format)
        {
            switch (format)
            {
                case Format.A16B16G16R16: return new UnPacker<UInt64>(unpack_A16B16G16R16);
                case Format.A16B16G16R16F: return new UnPacker<UInt64>(unpack_A16B16G16R16F);
                default: throw new NotSupportedException("Decoder not found");
            }
        }
        public static UnPacker<Uint128> unpacker128(Format format)
        {
            switch (format)
            {
                case Format.A32B32G32R32F: return new UnPacker<Uint128>(unpack_A32B32G32R32F);
                default: throw new NotSupportedException("Decoder not found");
            }
        }

        public static void BitmapToStream(Bitmap bitmap, Rectangle4i area, out Vector4b[,] stream)
        {
            stream = new Vector4b[area.width, area.height];

            if (area.x + area.width > bitmap.Width || area.y + area.height > bitmap.Height) throw new ArgumentOutOfRangeException();

            BitmapLock bmptool = new BitmapLock(bitmap);
            bmptool.LockBits();
            for (int y = 0; y < area.height; y++)
                for (int x = 0; x < area.width; x++)
                {
                    stream[x, y] = (Vector4b)bmptool.GetPixel(x + area.x, y + area.y);
                }
            bmptool.UnlockBits();

        }
        public static void BitmapToStream(Bitmap bitmap, out Vector4b[,] stream)
        {
            BitmapToStream(bitmap, new Rectangle4i(bitmap.Width, bitmap.Height), out stream);
        }
        
        public static Bitmap StreamToBitmap(Vector4b[,] stream, Rectangle4i area, PixelFormat format)
        {
            if (stream.GetLength(0) < area.width || stream.GetLength(1) < area.height) throw new ArgumentOutOfRangeException("wrong area");

            Bitmap bitmap = new Bitmap(area.width, area.height);

            BitmapLock bmptool = new BitmapLock(bitmap);
            bmptool.LockBits();
            for (int y = 0; y < area.height; y++)
                for (int x = 0; x < area.width; x++)
                    bmptool.SetPixel(x, y, (Color)stream[x + area.x, y + area.y]);

            bmptool.UnlockBits();

            return bitmap;
        }
        public static Bitmap StreamToBitmap(Vector4b[,] stream)
        {
            //return StreamToBitmap(stream, Rectangle4i.FromPoints(0, 0, stream.GetUpperBound(0), stream.GetUpperBound(1)), PixelFormat.Format32bppArgb);
            return StreamToBitmap(stream, new Rectangle4i(stream.GetLength(0), stream.GetLength(1)), PixelFormat.Format32bppArgb);
        }
        #endregion
    }
}

#endif