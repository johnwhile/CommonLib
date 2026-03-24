using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Common.Tools
{
    public static class FieldTool
    {
        [Obsolete("not completed", true)]
        public static void CheckStructAllignment<T>() where T : struct
        {
            var fields = typeof(T).GetFields();

            foreach (var field in typeof(T).GetFields())
            {
                FieldDesc desc = GetFieldDescForFieldInfo(field);
            }
        }
        public static unsafe FieldDesc GetFieldDescForFieldInfo(FieldInfo fi)
        {
            if (fi.IsLiteral) throw new Exception("Const field");
            FieldDesc* fd = (FieldDesc*)fi.FieldHandle.Value;
            return *fd;
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct FieldDesc
        {
            [FieldOffset(0)] private readonly void* m_pMTOfEnclosingClass;

            // unsigned m_mb                   : 24;
            // unsigned m_isStatic             : 1;
            // unsigned m_isThreadLocal        : 1;
            // unsigned m_isRVA                : 1;
            // unsigned m_prot                 : 3;
            // unsigned m_requiresFullMbValue  : 1;
            [FieldOffset(8)] private readonly uint m_dword1;

            // unsigned m_dwOffset                : 27;
            // unsigned m_type                    : 5;
            [FieldOffset(12)] private readonly uint m_dword2;

            public int Offset => (int)(m_dword2 & 0x7FFFFFF);
            public int MB => (int)(m_dword1 & 0xFFFFFF);
            private bool RequiresFullMBValue => ReadBit(m_dword1, 31);

            static bool ReadBit(uint b, int bitIndex)
            {
                return (b & (1 << bitIndex)) != 0;
            }
        }
    }
}
