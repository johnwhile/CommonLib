using Common.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class BinaryReaderExtensions
    {
        static List<byte> lbuffer = new List<byte>();

        static BinaryReaderExtensions()
        {
        }

        /// <summary>
        /// first byte is the size of string
        /// </summary>
        public static string ReadString(this BinaryReader br, int length)
        {
            if (length > 0) return new string(br.ReadChars(length));
            return null;
        }
        /// <summary>
        /// Read a null-terminated-string
        /// </summary>
        /// <param name="maxLength">safety value if you will never find a zero byte</param>
        public static string ReadStringToNull(this BinaryReader br, int maxLength = 32767)
        {
            byte b = 255;
            lbuffer.Clear();
            while (br.BaseStream.Position != br.BaseStream.Length && lbuffer.Count < maxLength && (b = br.ReadByte()) > 0)
                lbuffer.Add(b);

            return Encoding.UTF8.GetString(lbuffer.ToArray());
        }


        #region arrays
        public static long[] ReadInt64Array(this BinaryReader reader, int length) => ReadArray(reader.ReadInt64, length);
        public static ulong[] ReadUInt64Array(this BinaryReader reader, int length) => ReadArray(reader.ReadUInt64, length);
        public static int[] ReadInt32Array(this BinaryReader reader, int length) => ReadArray(reader.ReadInt32, length);
        public static uint[] ReadUInt32Array(this BinaryReader reader, int length) => ReadArray(reader.ReadUInt32, length);
        public static short[] ReadInt16Array(this BinaryReader reader, int length) => ReadArray(reader.ReadInt16, length);
        public static ushort[] ReadUInt16Array(this BinaryReader reader, int length) => ReadArray(reader.ReadUInt16, length);
        public static bool[] ReadBoolArray(this BinaryReader reader, int length) => ReadArray(reader.ReadBoolean, length);
        public static float[] ReadSingleArray(this BinaryReader reader, int length) => ReadArray(reader.ReadSingle, length);
        public static double[] ReadDoubleArray(this BinaryReader reader, int length) => ReadArray(reader.ReadDouble, length);

        static T[] ReadArray<T>(Func<T> readfn, int length)
        {
            var array = new T[length];
            for (int i = 0; i < length; i++) array[i] = readfn();
            return array;
        }
        #endregion

        #region math

        /// <summary>
        /// Read an Int32 7 bits encoded. The high bit of the byte when on means to continue reading more bytes.
        /// </summary>
        public static int Read7BitEncodedInt(this BinaryReader reader)
        {
            int num = 0;
            int num2 = 0;
            byte b;
            do
            {
                if (num2 == 35)throw new FormatException("Bad 7Bit Encoded Int32 format");
                b = reader.ReadByte();
                num |= (b & 0x7F) << num2;
                num2 += 7;
            }
            while ((b & 0x80u) != 0);
            return num;
        }


        /// <summary>
        /// Read a 24bit value as int32 for convenience
        /// </summary>
        public static uint ReadUInt24asInt(this BinaryReader reader) => (uint)(reader.ReadByte() | reader.ReadByte() << 8 | reader.ReadByte() << 16);
        public static UInt24 ReadUInt24(this BinaryReader reader) => new UInt24(reader);
        public static Matrix4x4f ReadMatrix(this BinaryReader reader) => new Matrix4x4f(reader);
        public static Vector4f ReadVector4f(this BinaryReader reader) => new Vector4f(reader);
        public static Vector3f ReadVector3f(this BinaryReader reader) => new Vector3f(reader);
        public static Vector2f ReadVector2f(this BinaryReader reader) => new Vector2f(reader);
        public static Vector4b ReadVector4b(this BinaryReader reader) => new Vector4b(reader.ReadUInt32());
        public static Color4b ReadColor4b(this BinaryReader reader) => new Color4b(reader.ReadUInt32());
        public static Matrix4x4f[] ReadMatrixArray(this BinaryReader reader, int length) => ReadArray(reader.ReadMatrix, length);
        public static Vector3f[] ReadVector3fArray(this BinaryReader reader, int length) => ReadArray(reader.ReadVector3f, length);
        public static Vector2f[] ReadVector2fArray(this BinaryReader reader, int length) => ReadArray(reader.ReadVector2f, length);
        public static Vector4b[] ReadVector4bArray(this BinaryReader reader, int length) => ReadArray(reader.ReadVector4b, length);
        public static Color4b[] ReadColor4bArray(this BinaryReader reader, int length) => ReadArray(reader.ReadColor4b, length);
        public static Vector4f[] ReadVector4fArray(this BinaryReader reader, int length) => ReadArray(reader.ReadVector4f, length);
        #endregion

        [Obsolete("Use instead ReadPacked<Ttype>")]
        public unsafe static T[] ReadWithSpan<T>(this BinaryReader reader, int length) where T : unmanaged
        {
            if (length < 1) return null;
            int bytesize = length * Marshal.SizeOf<T>();
            ReadOnlySpan<T> dataArray = MemoryMarshal.Cast<byte, T>(new ReadOnlySpan<byte>(reader.ReadBytes(bytesize)));
            return dataArray.ToArray();
        }

        /// <summary>
        /// x10 time faster than reading each single element in a for loop 
        /// </summary>
        public unsafe static T[] ReadUnsafe<T>(this BinaryReader reader, int length) where T : unmanaged
        {
            if (length < 1) return null;
            int bytesize = length * Marshal.SizeOf<T>();
            var source = reader.ReadBytes(bytesize);
            var destination = new T[length];

            fixed (byte* srcptr = source)
            fixed (T* dstptr = destination)
                Buffer.MemoryCopy(srcptr, dstptr, bytesize, bytesize);
            return destination;
        }
        /// <summary>
        /// little slower but  mostly the same than reading specified struct implementation, example <see cref="ReadVector3f(BinaryReader)"/>
        /// </summary>
        public unsafe static T ReadUnsafe<T>(this BinaryReader reader) where T : unmanaged
        {
            var bytesize = Marshal.SizeOf(typeof(T));
            var source = reader.ReadBytes(bytesize);
            var destination = default(T);
            fixed (byte* srcptr = source) { destination = *(T*)srcptr; }
            return destination;
        }

        /// <summary>
        /// Safe version, little faster than <see cref="ReadUnsafe{T}(BinaryReader, int)"/>
        /// </summary>
        public static T[] ReadSafe<T>(this BinaryReader reader, int length) where T : unmanaged
        {
            if (length < 1) return null;
            T[] destination = new T[length];
            var size = Marshal.SizeOf(typeof(T)) * length;
            var source = reader.ReadBytes(size); //affects performance only by 0.7%
            var handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            var pointer = handle.AddrOfPinnedObject();
            Marshal.Copy(source, 0, pointer, source.Length);
            if (handle.IsAllocated) handle.Free();
            return destination;
        }
        /// <summary>
        /// TO TEST PERFORMANCES
        /// </summary>
        public static T[] ReadSafe2<T>(this BinaryReader reader, int length) where T : unmanaged
        {
            if (length < 1) return null;
            T[] destination = new T[length];
            var sizeofT = Marshal.SizeOf(typeof(T));
            for(int i=0;i<length;i++)
                destination[i] = ByteArrayToStructure<T>(reader.ReadBytes(sizeofT));
            return destination;
        }
        /// <summary>
        /// x5 time slower than reading specified struct implementation, example <see cref="ReadVector3f(BinaryReader)"/>
        /// </summary>
        public static T ReadSafe<T>(this BinaryReader reader) where T : unmanaged =>
            ByteArrayToStructure<T>(reader.ReadBytes(Marshal.SizeOf(typeof(T))));
        
        static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T obj = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return obj;
        }

        [Obsolete("Hacking with reflection the list class but performance is equal of inizialize a new list with Items as parameter")]
        public static List<T> ReadList<T>(this BinaryReader reader, int length) where T: unmanaged
        {
            List<T> list = new List<T>();
            if (length > 0)
            {
                T[] array = reader.ReadUnsafe<T>(length);
                list.GetType().GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(list, array.Length);
                list.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(list, array);
            }
            return list;
        }
    }
}
