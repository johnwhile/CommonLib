using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Common.Maths
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct UInt128
    {
        public ulong low , high;

        public static UInt128 operator <<(UInt128 value, int i)
        {
            return new UInt128();
        }
        public static UInt128 operator >>(UInt128 value, int i)
        {
            return new UInt128();
        }
        public static UInt128 operator &(UInt128 value, UInt128 mask)
        {
            return new UInt128();
        }
    }
}
