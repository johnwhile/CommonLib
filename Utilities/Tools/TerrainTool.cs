using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;


// This tools are used essentially for Height maps

namespace Common.Tools
{
    #region Map's value type
    public interface IMapValue
    {
        float Value { get; set; }
    }
    public struct MapValue8b : IMapValue
    {
        const byte bmin = 0x00;
        const byte bmax = 0xFF;
        byte value;

        public float Value
        {
            get { return value * bmax; }
            set { this.value = value < 0 ? bmin : value > 1 ? bmax : (byte)(value * bmax); }
        }
    }
    public struct MapValue16b : IMapValue
    {
        const ushort bmin = 0x0000;
        const ushort bmax = 0xFFFF;
        ushort value;

        public float Value
        {
            get { return value * bmax; }
            set { this.value = value < 0 ? bmin : value > 1 ? bmax : (ushort)(value * bmax); }
        }
    }
    public struct MapValue24b : IMapValue
    {
        const uint bmin = 0x000000;
        const uint bmax = 0x00FFFF;
        ushort value0;
        byte value1;
        public float Value
        {
            get { return (value0 << 16 | value1) * bmax; }
            set
            {
                if (value < 0) { value0 = value1 = 0; }
                else if (value > 1) { value0 = ushort.MaxValue; value1 = byte.MaxValue; }
                else
                {
                    int i = (int)(value * bmax);
                    value0 = (ushort)(i >> 8);
                    value1 = (byte)(i >> 16);
                }
            }
        }
    }
    public struct MapValue32b : IMapValue
    {
        float value;
        public float Value
        {
            get { return value; }
            set { this.value = value < 0 ? 0.0f : value > 1 ? 1.0f : value; }
        }
    }
    #endregion

    public enum WrapMode
    {
        Clamp, 
        Repeat
    }
    public enum Interpolation
    {
        Point,
        Linear,
        Cubic
    }
    public interface IMap
    {
        /// <summary> wrap mode, if clamp the point out map boundary are zero else i use a cyclic function</summary>
        WrapMode Wrap { get; set; }
        /// <summary> how the intermediate's data are extracted, if Linear i use a linear interpolation</summary>
        Interpolation Filter { get; set; }
        /// <summary> X size </summary>
        int Width { get; }
        /// <summary> Y size </summary>
        int Height { get; }
        /// <summary> Height value encode to 0.0f-1.0f </summary>
        float this[int i, int j] { get; }
        /// <summary> Height value encode to 0.0f-1.0f, using a linear interpolation</summary>
        float this[float i, float j] { get; }
    }

    /// <summary>
    /// A 2D table of 0.0 - 1.0 values
    /// </summary>
    public class Map2D<T> : IMap where T : struct , IMapValue
    {
        int width, height;
        T[,] table;
        WrapMode wrap;
        Interpolation filter;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public WrapMode Wrap
        {
            get { return wrap; }
            set { wrap = value; }
        }
        public Interpolation Filter
        {
            get { return filter; }
            set { filter = value; }
        }

        public Map2D(int Width, int Height)
        {
            width = Width;
            height = Height;
            table = new T[width, height];
        }

        public float this[int i, int j]
        {
            get { throw new NotImplementedException(); }
        }

        public float this[float i, float j]
        {
            get { throw new NotImplementedException(); }
        }


        public static Map2D<K> FromBitmap<K>(Bitmap bmp) where K : struct , IMapValue
        {
            Map2D<K> map = new Map2D<K>(bmp.Width, bmp.Height);

            BitmapLock tool = new BitmapLock(bmp);
            tool.LockBits();
            for (int j = 0; j < map.height; j++)
                for (int i = 0; i < map.width; i++)
                    map.table[i, j].Value = tool.GetPixel(i, j).GetBrightness();
            tool.UnlockBits();

            return map;
        }
    }
}
