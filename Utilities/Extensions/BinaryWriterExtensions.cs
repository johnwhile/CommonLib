using Common.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Common
{
    public static class BinaryWriterExtensions
    {
        static bool validate(Array array, ref int first, ref int length)
        {
            if (array == null) return false;
            return validate(array.Length, ref first, ref length);
        }
        static bool validate(int count, ref int first, ref int length)
        {
            if (count <= 0) return false;
            if (first < 0) first = 0;
            if (length < 0) length = count - first;
            if (length > count + first) throw new ArgumentOutOfRangeException("you are passing a wrong length");
            return length > 0;
        }

        static T[] hackextractlist<T>(List<T> list)
        {
            return (T[])list.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(list);
        }



        #region array
        /// <summary>
        /// Write a null-terminated-string. If string is null it write at least a byte zero.
        /// </summary>
        public static void WriteStringToNull(this BinaryWriter writer, string s = null)
        {
            if (!string.IsNullOrWhiteSpace(s))
            {
                int size = s.Length;
                var array = s.ToCharArray(0, size);
                foreach (char c in array) writer.Write(c);
            }
            writer.Write((byte)0);
        }

        /// <summary>
        /// First byte is length
        /// </summary>
        public static void WriteStringByte(this BinaryWriter writer, string s = null)
        {
            if (string.IsNullOrWhiteSpace(s)) writer.Write(false);
            else
            {
                byte size = (byte)Mathelp.CLAMP(s.Length, 0, byte.MaxValue - 1);
                writer.Write(size);
                writer.Write(s.ToCharArray(0, size));
            }
        }
        /// <summary>
        /// Faster writer of array of struct.<br/>
        /// x5 time faster than writing each single element
        /// </summary>
        public unsafe static void WriteUnsafe<T>(this BinaryWriter writer, T[] data, int firstindex = 0, int length = -1) where T : unmanaged
        {
            if (!validate(data, ref firstindex, ref length)) return;

            var bytesize = Marshal.SizeOf(typeof(T)) * length;
            var destination = new byte[bytesize];
            fixed (byte* dstptr = destination)
            fixed (T* srcptr = data)
            {
                Buffer.MemoryCopy(srcptr + firstindex, dstptr, bytesize, bytesize);
            }
            writer.Write(destination);
        }
        /// <summary>
        /// Little slower than <see cref="WriteUnsafe{T}(BinaryWriter, T[], int, int)"/>
        /// </summary>
        [Obsolete("Can generate FatalExecutionEngineError")]
        public unsafe static void WriteUnsafe2<T>(this BinaryWriter writer, T[] data, int firstindex = 0, int length = -1) where T : unmanaged
        {
            if (!validate(data, ref firstindex, ref length)) return;

            var bytesize = Marshal.SizeOf(typeof(T)) * length;

            long remainbytes = bytesize;

            var destination = new byte[bytesize];
            fixed (byte* dstptr = destination)
            fixed (T* srcptr = data)
            {
                //use the greatest pointer
                if (bytesize / 16 > 0)
                {
                    UInt128* srcptr128 = (UInt128*)(srcptr + firstindex);
                    UInt128* dstptr128 = (UInt128*)dstptr;
                    for (int i = 0; i < bytesize / 16; i++)
                        dstptr128[i] = srcptr128[i];
                    remainbytes = bytesize % 16;
                }
                if (remainbytes > 0)
                {
                    byte* srcptr8 = (byte*)(srcptr + (bytesize - remainbytes));
                    byte* dstptr8 = dstptr + (bytesize - remainbytes);
                    for (int i = 0; i < bytesize - remainbytes; i++)
                        dstptr8[i] = srcptr8[i];
                }
            }
            writer.Write(destination);
        }
        /// <summary>
        /// Slow but comparable performance of <see cref="WriteUnsafe{T}(BinaryWriter, T[], int, int)"/>
        /// </summary>
        public static void Write<T>(this BinaryWriter writer, T[] data, int firstindex = 0, int length = -1) where T : unmanaged
        {
            if (!validate(data, ref firstindex, ref length)) return;
            var sizeofT = Marshal.SizeOf<T>();
            var bytesize = sizeofT * length;
            var destination = new byte[bytesize];
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            Marshal.Copy(IntPtr.Add(handle.AddrOfPinnedObject(), firstindex * sizeofT), destination, 0, bytesize);
            writer.Write(destination);
            handle.Free();
        }
        /// <summary>
        /// Using Reflection to extract the private "_items" array from list
        /// </summary>
        public static void Write<T>(this BinaryWriter writer, List<T> list, int firstindex = 0, int length = -1) where T : unmanaged
        {
            T[] _items = hackextractlist(list);
            writer.WriteUnsafe(_items, firstindex, length);
        }
        public static void Write<T>(this BinaryWriter writer, IEnumerable<T> enumerable) where T : unmanaged
        {
            if (enumerable == null) return; foreach (var v in enumerable) writer.WriteUnsafe(v);
        }
        public static void WriteEnumerable(this BinaryWriter writer, IEnumerable<Color4b> array)
        {
            if (array == null) return; foreach (var v in array) writer.Write(v.rgba);
        }
        public static void WriteEnumerable(this BinaryWriter writer, IEnumerable<Vector3f> array)
        { 
            if (array == null) return; foreach (var v in array) v.Write(writer); 
        }
        public static void WriteEnumerable(this BinaryWriter writer, IEnumerable<Vector2f> array) 
        { 
            if (array == null) return; foreach (var v in array) v.Write(writer); 
        }
        public static void WriteEnumerable(this BinaryWriter writer, IEnumerable<Vector4f> array) 
        { 
            if (array == null) return; foreach (var v in array) v.Write(writer);
        }
        #endregion

        #region single value
        public static void WriteZero(this BinaryWriter bw, int count) { for (int i = 0; i < count; i++) bw.Write(false); }
        public static void WriteBoolean(this BinaryWriter writer, bool state = false) { writer.WriteByte((byte)(state ? 1 : 0)); }
        public static void WriteByte(this BinaryWriter writer, byte value = 0) { writer.Write(value); }
        public static void WriteUShort(this BinaryWriter writer, ushort value = 0) { writer.Write(value); }
        public static void WriteInt(this BinaryWriter writer, int value = 0) { writer.Write(value); }
        public static void WriteLong(this BinaryWriter writer, long value = 0) { writer.Write(value); }
        /// <summary>
        /// Write an int 7 bits encoded. The high bit of the byte, when on, tells writer to continue writes more bytes. Support negative numbers.
        /// </summary>
        public static void Write7BitEncodedInt(this BinaryWriter writer, int value)
        {
            uint v = (uint)value;
            while (v >= 0x80)
            {
                writer.Write((byte)(v | 0x80));
                v >>= 7;
            }
            writer.Write((byte)v);
        }
        /// <summary>
        /// Write a 24bit value
        /// </summary>
        /// <param name="value">extend to int32 for convenience</param>
        public static void Write(this BinaryWriter writer, UInt24 value = default(UInt24))
        {
            value.Write(writer);
        }
        /// <summary>
        /// Write a 24bit value
        /// </summary>
        /// <param name="value">extend to int32 for convenience</param>
        public static void WriteUInt24(this BinaryWriter writer, int value = 0)
        {
            writer.WriteByte((byte)value);
            writer.WriteByte((byte)(value >> 8));
            writer.WriteByte((byte)(value >> 16));
        }
        /// <summary>
        /// <inheritdoc cref="WriteInt24(BinaryWriter, int)"/>
        /// </summary>
        public static void WriteUInt24(this BinaryWriter writer, uint value = 0)
        {
            WriteUInt24(writer, (int)value);
        }

        /// <summary>
        /// x5 time slower than <see cref="WriteUnsafe{T}(BinaryWriter, T)"/>
        /// </summary>
        public static void Write<T>(this BinaryWriter writer, T data) where T : unmanaged
        {
            var bytesize = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(bytesize);
            Marshal.StructureToPtr(data, ptr, false);
            var bytes = new byte[bytesize];
            Marshal.Copy(ptr, bytes, 0, bytesize);
            Marshal.FreeHGlobal(ptr);
            writer.Write(bytes);
        }
        /// <summary>
        /// Slower but comparable than write each element field of struct, example slower than <see cref="Vector3f.Write(BinaryWriter)"/>
        /// </summary>
        public unsafe static void WriteUnsafe<T>(this BinaryWriter writer, T data) where T : unmanaged
        {
            var bytesize = Marshal.SizeOf(typeof(T));
            var destination = new byte[bytesize];
            fixed (byte* dstptr = destination)
            {
                var srcptr = (byte*)&data;
                Buffer.MemoryCopy(srcptr, dstptr, bytesize, bytesize);
            }
            writer.Write(destination);
        }
        #endregion
    }
}
