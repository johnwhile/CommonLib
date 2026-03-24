using System;
using System.IO;
using System.Text;
using Common.Maths;

namespace Common
{
    public enum EndianType : ushort
    {
        BigEndian,
        LittleEndian
    }
    public class EndianBinaryWriter : BinaryWriter
    {
        public string FullPath;
        public EndianType endian;

        public EndianBinaryWriter(Stream stream, EndianType endian = EndianType.BigEndian, bool leaveOpen = false) : base(stream, Encoding.Default, leaveOpen)
        {
            this.endian = endian;
            if (stream is FileStream file) FullPath = Path.GetFullPath(file.Name);
        }
        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        void writereverse(byte[] buffer) { for (int i = buffer.Length - 1; i >= 0; i--) base.Write(buffer[i]); }


        public override void Write(short value)
        {
            if (endian == EndianType.BigEndian)
                writereverse(BitConverter.GetBytes(value));
            else base.Write(value);
        }
        public override void Write(int value)
        {
            if (endian == EndianType.BigEndian)
                writereverse(BitConverter.GetBytes(value));
            else base.Write(value);
        }
        public override void Write(long value)
        {
            if (endian == EndianType.BigEndian)
                writereverse(BitConverter.GetBytes(value));
            else base.Write(value);
        }

        public override void Write(ushort value) => Write((short)value);
        public override void Write(uint value) => Write((int)value);
        public override void Write(ulong value) => Write((long)value);
        public override void Write(float value) => Write(Mathelp.BitToInt(value));
        public override void Write(double value) => Write(Mathelp.BitToLong(value));
    }


}
