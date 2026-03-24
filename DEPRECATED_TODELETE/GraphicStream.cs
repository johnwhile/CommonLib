using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;

using Common.Maths;
using Common.Tools;

#if DELETE

namespace Common.GraphicDx9
{
    /// <summary>
    /// Return the wrapper of directx buffer or pinned array
    /// </summary>
    public abstract class BufferStream
    {
        protected IntPtr buffer;
        protected int bufferSize;
        protected bool isReadable;
        protected bool isLocked;
        /// <summary>
        /// Position in bytes in the buffer, used only for write with increment
        /// </summary>
        public int PositionInBytes { get; set; }

        public int flushBytes { get; protected set; }

        public BufferStream(IntPtr bufferPtr, int buffersize, bool isReadable)
        {
            if (bufferPtr == IntPtr.Zero) throw new ArgumentNullException("buffer pointer null");
            this.buffer = bufferPtr;
            this.bufferSize = buffersize;
            this.isReadable = isReadable;
            this.PositionInBytes = 0;
            this.flushBytes = 0;
            this.isLocked = true;
        }

        /// <summary>
        /// Remove all internal references.
        /// </summary>
        public void Destroy()
        {
            isLocked = false;

            // this is correct ?
            //Marshal.FreeHGlobal(buffer);
            
            buffer = IntPtr.Zero;

            bufferSize = 0;
            isReadable = false;
        }

        public override string ToString()
        {
            return string.Format("Size: {0}b , Readable: {1}", bufferSize, isReadable);
        }
    }

    /// <summary>
    /// the vertices stream is a buffer's pointer manager that implement write and read functions
    /// can be used also for pinned array
    /// </summary>
    public class VertexStream : BufferStream
    {
        const int sizefloat = sizeof(float);
        const int sizeint16 = sizeof(short);
        const int sizeint32 = sizeof(int);

        public VertexStream(IntPtr bufferPtr, int buffersize, bool isReadable) : base(bufferPtr, buffersize, isReadable)
        {
        }

        #region Write default compact array
        /// <summary>
        /// Write a generic collection of struct, without VertexElement info the array will be write continuously
        /// </summary>
        /// <param name="Offset">attribute to don't write</param>
        /// <param name="Count">num of attributes to write</param>
        /// <param name="BufferOffset">number of buffer's vertex to jump before start writting</param>
        /// <remarks>
        /// <para>........................+.................................................................+..........</para>
        /// <para>      BufferOffset      | Array[Offset] , Array[Offset+1],      , Array[Offset + Count]   |          </para>
        /// <para>........................+.................................................................+..........</para>
        /// </remarks>
        public void WriteCollection<T>(IList<T> array, int Offset, int Count, int BufferOffset) where T : struct
        {
            if (Count <= 0) return;
            int sourceLenght = array.Count;
            if (Offset < 0 || Count + Offset > sourceLenght)
                throw new ArgumentException("array's Count and Offset wrong");

            int typesize = Marshal.SizeOf(typeof(T));
            int sourceSize = typesize * (Count + Offset);
            int sourceOffset = typesize * Offset;
            int bufferoffset = bufferFormat.bytesize * BufferOffset;

            if (bufferSize - bufferoffset < sourceSize - sourceOffset)
                throw new ArgumentException("not enought buffer space");

            if (array.GetType() == typeof(T[]))
            {
                GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                IntPtr source = handle.AddrOfPinnedObject();
                MemoryTool.WriteStruct(buffer, bufferSize, bufferoffset, source, sourceSize, sourceOffset);
                handle.Free();
            }
            else
            {
                MemoryTool.WriteStructByStruct<T>(buffer, bufferSize, bufferoffset, array, (Offset + Count), Offset);
            }

        }
        #endregion

        #region Write with increment
        public unsafe void WriteAndIncrement(Vector3f vector)
        {
            // pointer arithmetic in bytes
            Vector3f* ptr = (Vector3f*)((byte*)buffer.ToPointer() + PositionInBytes);
            *ptr = vector;
            PositionInBytes += Vector3f.sizeinbyte;
        }
        public unsafe void WriteAndIncrement(Vector4b color)
        {
            Vector4b* ptr = (Vector4b*)((byte*)buffer.ToPointer() + PositionInBytes);
            *ptr = color;
            PositionInBytes += sizeof(int);
        }
        public unsafe void WriteAndIncrement(Vector2f vector)
        {
            Vector2f* ptr = (Vector2f*)((byte*)buffer.ToPointer() + PositionInBytes);
            *ptr = vector;
            PositionInBytes += Vector2f.sizeinbyte;
        }
        public unsafe void WriteAndIncrement(float value)
        {
            float* ptr = (float*)((byte*)buffer.ToPointer() + PositionInBytes);
            *ptr = value;
            PositionInBytes += sizefloat;
        }
        #endregion
    }

    /// <summary>
    /// the pixel stream is a buffer's pointer manager that implement write and read functions
    /// can be used also for pinned array. A particular attention when work with compress Dxt version,
    /// not all functions are implemented yet.
    /// </summary>
    public class TextureStream
    {
        IntPtr bufferPtr;
        bool enabled;

        int Width, Height;
        int boffset, brow;

        int min(int a, int b) { return a < b ? a : b; }
        int max(int a, int b) { return a > b ? a : b; }


        public Format pixelFormat { get; private set; }

        /// <summary> 
        /// </summary>
        /// <param name="bufferPtr">pointer of gpu buffer</param>
        /// <param name="bufferOffsetSize">offset in bytes of buffer, used for pointer aritmetic</param>
        /// <param name="bufferRowSize">bytes of a width row, used to correct jump to next row, 90% match with Width*PixelSize</param>
        /// <param name="pixelFormat"></param>
        /// <param name="Width">Width in pixels of locked texture</param>
        /// <param name="Height">Height in pixels of locked texture</param>
        /// <remarks>
        /// The concept of TextureStream is to separate all depth levels into singular and simple manageable 2d texture, so
        /// we need a bufferoffset (data.SlicePitch) for buffer arithmetic 
        /// </remarks>
        public TextureStream(IntPtr bufferPtr, int bufferOffsetSize , int bufferRowSize , Format pixelFormat, int Width, int Height)
        {
            if (bufferPtr == IntPtr.Zero) throw new ArgumentNullException("buffer pointer null");
            this.bufferPtr = bufferPtr;
            this.enabled = true;
            this.Width = Width;
            this.Height = Height;
            this.pixelFormat = pixelFormat;
            this.boffset = bufferOffsetSize;
            this.brow = bufferRowSize;

            int pxsize = (int)getPixelSize(pixelFormat);

            if (boffset % pxsize != 0 || brow % pxsize != 0) throw new NotImplementedException("comming soon...");

        }


        /// <summary>
        /// Remove all internal references.
        /// </summary>
        public void Destroy()
        {
            bufferPtr = IntPtr.Zero;
            enabled = false;
        }

        public Vector4b[,] GetData()
        {
            if (!enabled) throw new Exception("buffer closed");

            Vector4b[,] data = null;

            if (isDxtcompressed(pixelFormat))
            {
                DXTtools.DecompressStream(bufferPtr, Width, Height, getDxtVersion(pixelFormat), out data);
            }
            else
            {
                int pxsize = (int)getPixelSize(pixelFormat); 
                int pitch = brow / pxsize;
                int offset = boffset / pxsize;
                data = new Vector4b[Width, Height];

                unsafe
                {
                    ///////////////////////////////////////////////// 16bit
                    if (pxsize == 2)
                    {
                        PixelTools.UnPacker<UInt16> unpacker16 = PixelTools.unpacker16(pixelFormat);
                        UInt16* psrc = (UInt16*)bufferPtr.ToPointer();
                        for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) data[x, y] = unpacker16(psrc[offset + x + y * pitch]);
                    }
                    ///////////////////////////////////////////////// 32bit
                    else if (pxsize == 4)
                    {
                        PixelTools.UnPacker<UInt32> unpacker32 = PixelTools.unpacker32(pixelFormat);
                        UInt32* psrc = (UInt32*)bufferPtr.ToPointer();
                        for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) data[x, y] = unpacker32(psrc[offset + x + y * pitch]);
                    }
                    ///////////////////////////////////////////////// 64bit
                    else if (pxsize == 8)
                    {
                        PixelTools.UnPacker<UInt64> unpacker64 = PixelTools.unpacker64(pixelFormat);
                        UInt64* psrc = (UInt64*)bufferPtr.ToPointer();
                        for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) data[x, y] = unpacker64(psrc[offset + x + y * pitch]);
                    }
                    ///////////////////////////////////////////////// 128bit
                    else if (pxsize == 16)
                    {
                        PixelTools.UnPacker<Uint128> unpacker128 = PixelTools.unpacker128(pixelFormat);
                        Uint128* psrc = (Uint128*)bufferPtr.ToPointer();
                        for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) data[x, y] = unpacker128(psrc[offset + x + y * pitch]);
                    }
                }         
            }
            return data;
        }

        public void SetData(Vector4b[,] data)
        {
            if (!enabled) throw new Exception("buffer closed");

            int width = min(data.GetLength(0), Width);
            int height = min(data.GetLength(1), Height);
            
            if (isDxtcompressed(pixelFormat))
            {
                DXTtools.CompressStream(bufferPtr, Width, Height, getDxtVersion(pixelFormat), data);
            }
            else
            {
                int pxsize = (int)getPixelSize(pixelFormat);
                int pitch = brow / pxsize;
                int offset = boffset / pxsize;
                unsafe
                {
                    ///////////////////////////////////////////////// 16bit
                    if (pxsize == 2)
                    {
                        PixelTools.Packer<UInt16> packer16 = PixelTools.packer16(pixelFormat);
                        UInt16* psrc = (UInt16*)bufferPtr.ToPointer();
                        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) psrc[offset + x + y * pitch] = packer16((Vector4f)data[x, y]);
                    }
                    ///////////////////////////////////////////////// 32bit
                    else if (pxsize == 4)
                    {
                        PixelTools.Packer<UInt32> packer32 = PixelTools.packer32(pixelFormat);
                        UInt32* psrc = (UInt32*)bufferPtr.ToPointer();
                        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) psrc[offset + x + y * pitch] = packer32((Vector4f)data[x, y]);
                    }
                    ///////////////////////////////////////////////// 64bit
                    else if (pxsize == 8)
                    {
                        PixelTools.Packer<UInt64> packer64 = PixelTools.packer64(pixelFormat);
                        UInt64* psrc = (UInt64*)bufferPtr.ToPointer();
                        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) psrc[offset + x + y * pitch] = packer64((Vector4f)data[x, y]);
                    }
                    ///////////////////////////////////////////////// 128bit
                    else if (pxsize == 16)
                    {
                        PixelTools.Packer<Uint128> packer128 = PixelTools.packer128(pixelFormat);
                        Uint128* psrc = (Uint128*)bufferPtr.ToPointer();
                        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) psrc[offset + x + y * pitch] = packer128((Vector4f)data[x, y]);
                    }
                }
            }
        }

        public void SetData(Bitmap bitmap)
        {
            if (!enabled) throw new Exception("buffer closed");

            int width = min(bitmap.Width, Width);
            int height = min(bitmap.Height, Height);

            if (isDxtcompressed(pixelFormat))
            {
                throw new NotImplementedException();
                //DXTtools.CompressStream(bufferPtr, Width, Height, getDxtVersion(pixelFormat), data);
            }
            else
            {
                BitmapLock bmptool = new BitmapLock(bitmap);
                bmptool.LockBits();

                int pxsize = (int)getPixelSize(pixelFormat);
                int pitch = brow / pxsize;
                int offset = boffset / pxsize;
                unsafe
                {
                    ///////////////////////////////////////////////// 16bit
                    if (pxsize == 2)
                    {
                        PixelTools.Packer<UInt16> packer16 = PixelTools.packer16(pixelFormat);
                        UInt16* psrc = (UInt16*)bufferPtr.ToPointer();
                        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) psrc[offset + x + y * pitch] = packer16(bmptool.GetPixelV4(x, y));
                    }
                    ///////////////////////////////////////////////// 32bit
                    else if (pxsize == 4)
                    {
                        PixelTools.Packer<UInt32> packer32 = PixelTools.packer32(pixelFormat);
                        UInt32* psrc = (UInt32*)bufferPtr.ToPointer();
                        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) psrc[offset + x + y * pitch] = packer32(bmptool.GetPixelV4(x, y));
                    }
                    ///////////////////////////////////////////////// 64bit
                    else if (pxsize == 8)
                    {
                        PixelTools.Packer<UInt64> packer64 = PixelTools.packer64(pixelFormat);
                        UInt64* psrc = (UInt64*)bufferPtr.ToPointer();
                        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) psrc[offset + x + y * pitch] = packer64(bmptool.GetPixelV4(x, y));
                    }
                    ///////////////////////////////////////////////// 128bit
                    else if (pxsize == 16)
                    {
                        PixelTools.Packer<Uint128> packer128 = PixelTools.packer128(pixelFormat);
                        Uint128* psrc = (Uint128*)bufferPtr.ToPointer();
                        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) psrc[offset + x + y * pitch] = packer128(bmptool.GetPixelV4(x, y));
                    }
                }
                bmptool.UnlockBits();
            }
 
        }


        public static bool isDxtcompressed(Format format)
        {
            return format == Format.Dxt1 ||
                format == Format.Dxt2 ||
                format == Format.Dxt3 ||
                format == Format.Dxt4 ||
                format == Format.Dxt5;
        }


        public static int getDxtVersion(Format format)
        {
            switch (format)
            {
                case Format.Dxt1: return 1;
                case Format.Dxt2: return 2;
                case Format.Dxt3: return 3;
                case Format.Dxt4: return 4;
                case Format.Dxt5: return 5;
                default: throw new NotImplementedException();
            }
        }
        /// <summary>
        /// return the size in bytes of pixels, if size is &lt;2 is compressed
        /// </summary>
        public static float getPixelSize(Format format)
        {
            switch (format)
            {
                // 128bit pixel format
                case Format.A32B32G32R32F:
                    return 16;
                // 64bit
                case Format.A16B16G16R16F:
                case Format.A16B16G16R16:
                    return 8;
                // 32bit
                case Format.A8R8G8B8:
                case Format.A8B8G8R8:
                case Format.X8R8G8B8:
                case Format.X8B8G8R8:
                    return 4;
                // 16bit
                case Format.A4R4G4B4:
                case Format.X4R4G4B4:
                case Format.A1R5G5B5:
                case Format.X1R5G5B5:
                    return 2;
                // compressed
                case Format.Dxt1:
                    return 8.0f / 16; // 8bytes for 4x4 pixels block
                case Format.Dxt2:
                case Format.Dxt3:
                case Format.Dxt4:
                case Format.Dxt5:
                    return 16.0f / 16; // 16bytes for 4x4 pixels block
                default:
                    throw new NotImplementedException("unknow format");
            }
        }

        static int Max(int a, int b) { return a > b ? a : b; }

        /// <summary>
        /// return the size in bytes of rows, from Microsodt sdk
        /// </summary>
        public static int getPitchSize(Format format, int width)
        {
            float pxsize = getPixelSize(format);

            if (pxsize < 2)
            {
                switch (format)
                {
                    case Format.Dxt2:
                    case Format.Dxt3:
                    case Format.Dxt4:
                    case Format.Dxt5:
                        return Max(1, ((width + 3) / 4)) * 16;
                    default:
                        // dxt1
                        return Max(1, ((width + 3) / 4)) * 8;
                }
            }
            else
            {
                return (int)(pxsize * width);
            }
        }
    }

}

#endif