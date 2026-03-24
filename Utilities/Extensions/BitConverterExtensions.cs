using System;



namespace Common
{
    public static class BitConverterExt
    {
        public static long ToInt64(string eightchars)
        {
            byte[] bytes = new byte[8];
            int lenght = Math.Min(eightchars.Length, 8);
            for (int i = 0; i < lenght; bytes[i] = (byte)eightchars[i++]) ;
            return BitConverter.ToInt64(bytes, 0);
        }
    }

}
