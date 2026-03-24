using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Common.Maths
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
    public struct UInt24
    {
        public unsafe fixed byte b[3];

        public const int MaxValue = 0xFF_FFFF;

        public unsafe UInt24(int n)
        {
            b[0] = (byte)(n & 0xFF);
            b[1] = (byte)(n >> 8 & 0xFF);
            b[2] = (byte)(n >> 16 & 0xFF);
        }
        public unsafe UInt24(uint n) : this((int)n)
        {

        }
        /// <summary>
        /// since it's a my implementation it doesn't matter the endianess type, i will write always as 3 bytes
        /// </summary>
        /// <param name="reader"></param>
        public unsafe UInt24(BinaryReader reader) : this()
        {
            b[0] = reader.ReadByte();
            b[1] = reader.ReadByte();
            b[2] = reader.ReadByte();
        }

        public unsafe void Write(BinaryWriter writer)
        {
            writer.Write(b[0]);
            writer.Write(b[1]);
            writer.Write(b[2]);
        }

        public static unsafe implicit operator uint(UInt24 value)
        {
            uint result = (uint)(value.b[0] | (value.b[1] << 8) | (value.b[2] << 16));
            return result;

        }
        public static unsafe implicit operator int(UInt24 value)
        {
            int result = value.b[0] | (value.b[1] << 8) | (value.b[2] << 16);
            return result;
        }
        public static implicit operator UInt24(uint value)
        {
            var uint24 = new UInt24((int)value);
            return uint24;
        }
        public static implicit operator UInt24(int value)
        {
            var uint24 = new UInt24(value);
            return uint24;
        }

        public static UInt24 operator +(UInt24 a, UInt24 b) => (UInt24)((uint)a + (uint)b);

        /// <summary>
        /// Not work for Debugger Display return only first byte
        /// </summary>
        public unsafe override string ToString() => ((uint)this).ToString();
    }
}
