using System;
using System.IO;
using System.Text;

namespace Common
{
    public class EndianBinaryReader : BinaryReader
    {
        public string FullPath;
        /// <summary>
        /// Can be changed during reading
        /// </summary>
        public EndianType Endianess { get; set; }

        public EndianBinaryReader(Stream stream, EndianType endian = EndianType.BigEndian, bool leaveOpen = false) : 
            base(stream, Encoding.Default, leaveOpen)
        {
            Endianess = endian;
            if (stream is FileStream file) FullPath = Path.GetFullPath(file.Name); 
        }

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }
        public long Length
        {
            get => BaseStream.Length;
        }
        byte[] readbyteinv(int count)
        {
            var buffer = ReadBytes(count);
            Array.Reverse(buffer);
            return buffer;
        }
        public override short ReadInt16() => Endianess == EndianType.BigEndian ? BitConverter.ToInt16(readbyteinv(2), 0) : base.ReadInt16();
        public override ushort ReadUInt16() => (ushort)ReadInt16();
        public override int ReadInt32() => Endianess == EndianType.BigEndian ? BitConverter.ToInt32(readbyteinv(4), 0) : base.ReadInt32();
        public override uint ReadUInt32() => (uint)ReadInt32();
        public override long ReadInt64() => Endianess == EndianType.BigEndian ? BitConverter.ToInt64(readbyteinv(8), 0) : base.ReadInt64();
        public override ulong ReadUInt64() => (ulong)ReadInt64();
        public override float ReadSingle() => Endianess == EndianType.BigEndian ? BitConverter.ToSingle(readbyteinv(4), 0) : base.ReadSingle();
        public override double ReadDouble() => Endianess == EndianType.BigEndian ? BitConverter.ToDouble(readbyteinv(8), 0) : base.ReadDouble();
    }
}
