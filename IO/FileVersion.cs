using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Common
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FileVersion
    {
        public byte Major;
        public byte Minor;
        public ushort Build;

        public FileVersion(byte major, byte minor, ushort build)
        {
            Major = major;
            Minor = minor;
            Build = build;
        }
        public FileVersion(BinaryReader reader)
        {
            Major = reader.ReadByte();
            Minor = reader.ReadByte();
            Build = reader.ReadUInt16();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Major);
            writer.Write(Minor);
            writer.Write(Build);
        }

        public int ToInt() => Major << 24 | Minor << 16 | Build;

        public override string ToString()=> $"{Major}.{Minor}.{Build}";
        
    }
}
