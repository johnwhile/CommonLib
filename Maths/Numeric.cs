using System;
using System.Globalization;


namespace Common.Maths
{
    public static partial class Mathelp
    {
        /// <summary>
        /// Use dot for decimal separator as default culture
        /// </summary>
        public static CultureInfo DotCulture = new  CultureInfo("en-US");

        const uint FSIGMASK = 0x80000000; //float sign mask
        const uint FEXPMASK = 0x7F800000; //float exponent mask
        const uint FFRAMASK = 0x007FFFFF; //float fraction mask
        const uint FSIGMASKneg = ~0x80000000; //float sign mask negative

        /// <summary>
        /// fix culture issue
        /// </summary>
        public static bool TryParse(string value, out float result) => float.TryParse(value, NumberStyles.Float, DotCulture, out result);
        /// <summary>
        /// fix culture issue
        /// </summary>
        public static bool TryParse(string value, out int result) => int.TryParse(value, NumberStyles.Integer, DotCulture, out result);

        /// <summary>
        /// Bit to Bit conversion
        /// </summary>
        public static unsafe int BitToInt(float f) => *(int*)&f;
        public static unsafe uint BitToUInt(float f) => *(uint*)&f;
        public static unsafe long BitToLong(double d) => *(long*)&d;
        public static unsafe ulong BitToULong(double d) => *(ulong*)&d;
        public static unsafe float BitToFloat(int i) => *(float*)&i;
        public static unsafe float BitToFloat(uint i) => *(float*)&i;
        public static unsafe double BitToDouble(long l) => *(double*)&l;
        public static unsafe double BitToDouble(ulong l) => *(double*)&l;
    }
}
