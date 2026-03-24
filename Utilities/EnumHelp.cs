using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tools
{
    /// <summary>
    /// enum extensions
    /// </summary>
    public static class EnumHelp
    {
        /// <summary>
        /// 10x slower
        /// </summary>
        public static bool HasFlag(this byte value, byte flag)
        {
            return (value & flag) > 0;
        }
        /// <summary>
        /// 10x slower
        /// </summary>
        public static bool HasFlag(this ushort value, ushort flag)
        {
            return (value & flag) > 0;
        }
        /// <summary>
        /// 10x slower
        /// </summary>
        public static bool HasFlag(this uint value, uint flag)
        {
            return (value & flag) > 0;
        }

        /// <summary>
        /// 10x slower
        /// </summary>
        public static byte RemoveFlag(byte value, byte flag)
        {
            return (byte)(value & ~flag);
        }
        /// <summary>
        /// 10x slower
        /// </summary>
        public static ushort RemoveFlag(ushort value, ushort flag)
        {
            return (ushort)(value & ~flag);
        }
        /// <summary>
        /// 10x slower
        /// </summary>
        public static int RemoveFlag(int value, int flag)
        {
            return value & ~flag;
        }


        public static T ToEnum<T>(this string toparse)
        {
            return (T)Enum.Parse(typeof(T), toparse);
        }
        /// <summary>
        /// 1000x slower compared to bitwise operations. Also <see cref="Enum.HasFlag(Enum)"/> isn't good enought
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T NegateFlag<T>(this T value, T remove) where T : Enum
        {
            Type enumtype = typeof(T);
            try
            {
                var code = value.GetTypeCode();
                object value_obj = Enum.ToObject(enumtype, value);
                object remove_obj = Enum.ToObject(enumtype, remove);

                if (code >= TypeCode.SByte)
                {
                    if (code <= TypeCode.UInt32)
                    {
                        int i = Convert.ToInt32(value_obj) & ~Convert.ToInt32(remove_obj);
                        return (T)Enum.ToObject(enumtype, i);
                    }
                    else if (code <= TypeCode.UInt64)
                    {
                        long i = Convert.ToInt64(value_obj) & ~Convert.ToInt64(remove_obj);
                        return (T)Enum.ToObject(enumtype, i);
                    }
                }
                throw new ArgumentException("Ttype not a numeric value");
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("Could not remove value from enumerated type '{0}'.", typeof(T).Name), ex);
            }
        }
    }
}
