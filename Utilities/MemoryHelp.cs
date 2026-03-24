using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Security;

using Common.Maths;

namespace Common.Tools
{
    public static class MemoryHelp
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr CopyMemory(IntPtr dest, IntPtr src, ulong sizeInBytesToCopy);

        /// <summary>
        /// returns the private memory usage in byte
        /// </summary>
        public static float GetMBMemoryPressure()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                float mb = proc.PrivateMemorySize64 / 1024.0f / 1024.0f;
                return mb;
            }
        }
        

        /// <summary>
        /// Allocate an aligned memory buffer.
        /// </summary>
        /// <returns>A pointer to a buffer aligned.</returns>
        /// <remarks>
        /// The buffer must have been allocated with <see cref="AllocateMemory"/>.
        /// </remarks>
        public unsafe static void FreeMemory(IntPtr alignedBuffer)
        {
            if (alignedBuffer == IntPtr.Zero) return;
            Marshal.FreeHGlobal(((IntPtr*)alignedBuffer)[-1]);
        }

        /// <summary>
        /// Determines whether the specified memory pointer is aligned in memory.
        /// </summary>
        public static bool IsMemoryAligned(IntPtr ptr, int align = 16)
        {
            return (ptr.ToInt64() & (align - 1)) == 0;
        }

        public static bool GetFlag(byte value, byte flag) => (value & flag) != 0;
        public static void SetFlag(ref byte value, byte flag, bool flagval)
        {
            if (flagval) value |= flag;
            else value &= (byte)~flag;
        }
        public static string GetBinaryString(byte value)
        {
            char[] bits = new char[8];
            for (int i = 0; i < 8; i++) bits[7 - i] = (value & (byte)(1 << i)) != 0 ? '1' : '0';
            return new string(bits);
        }
        public static string GetBinaryString(short value)
        {
            return GetBinaryString((short)value);
        }
        public static string GetBinaryString(ushort value)
        {
            char[] bits = new char[16];
            for (int i = 0; i < 16; i++) bits[15 - i] = (value & (ushort)(1 << i)) != 0 ? '1' : '0';
            return new string(bits, 0, 8) + " " + new string(bits, 8, 8);
        }
        public static string GetBinaryString(int value)
        {
            return GetBinaryString((uint)value);
        }
        public static string GetBinaryString(uint value)
        {
            char[] bits = new char[32];
            for (int i = 0; i < 32; i++) bits[31 - i] = (value & (1 << i)) != 0 ? '1' : '0';

            string str = "";
            for (int i = 0; i < 4; i++) str += new string(bits, i * 8, 8) + " ";
            return str;
        }
        public static string GetBinaryString(long value)
        {
            return GetBinaryString((ulong)value);
        }
        public static string GetBinaryString(ulong value)
        {
            char[] bits = new char[64];
            for (int i = 0; i < 64; i++) bits[63 - i] = (value & (ulong)(1 << i)) != 0 ? '1' : '0';
            string str = "";
            for (int i = 0; i < 8; i++) str += new string(bits, i * 8, 8) + " ";
            return str;
        }
        public static string GetBinaryString(UInt128 value)
        {
            return GetBinaryString(value.high) + GetBinaryString(value.low);
        }
        public static string GetBinaryString<T>(T value) where T : struct
        {
            byte[] raw = RawSerialize(value);
            char[] bits = new char[raw.Length];
            for (int i = 0; i < raw.Length; i++) bits[i] = raw[i] != 0 ? '1' : '0';
            return new string(bits);
        }
        public static object RawDeserialize(byte[] rawData, int position, Type anyType)
        {
            int rawsize = Marshal.SizeOf(anyType);
            if (rawsize > rawData.Length) return null;
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, position, buffer, rawsize);
            object retobj = Marshal.PtrToStructure(buffer, anyType);
            Marshal.FreeHGlobal(buffer);
            return retobj;
        }
        public static byte[] RawSerialize(object anything)
        {
            int rawSize = Marshal.SizeOf(anything);
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(anything, buffer, false);
            byte[] rawDatas = new byte[rawSize];
            Marshal.Copy(buffer, rawDatas, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return rawDatas;
        }

        /// <summary>
        /// new version of CopyTo 
        /// </summary>
        public static void CopyStream(Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[1024];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, System.Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }

    }
}
