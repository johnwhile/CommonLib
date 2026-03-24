using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Common.Maths;
using System.Drawing;

namespace Common.Tools
{
    /// <summary>
    /// Base bit storage class, true = '1' , false = '0'
    /// For all version of bitarrays, the bits are stored in a single linear array
    /// </summary>
    public abstract class BitField : IEnumerable<bool>
    {
        const int FULL = unchecked((int)0xffffffff);

        // i noticed that using int value is a little fast than byte because there aren't cast operators
        protected int[] field;
        // lenght of field array
        protected int length;
        // length of bits list, always less of equal than capacity (length * 32)
        protected int bitCount;



        /// <summary>
        /// Empty constructor
        /// </summary>
        private BitField()
        {

        }

        /// <summary>
        /// Initialize a field with initial value
        /// </summary>
        protected BitField(int count, bool initialvalue)
        {
            bitCount = count;
            length = (count - 1) / 32 + 1;

            if (length < 1) throw new ArgumentOutOfRangeException("wrong Size");
            //0 = false, 1 = true
            field = new int[length];
            if (initialvalue) for (int i = 0; i < length; field[i++] = FULL) ;
        }

        /// <summary>
        /// Initialize a field using a bitfield, if the stream.lenght &lt; size * 8 the remain bits will be set to 0
        /// </summary>
        protected BitField(int count, byte[] stream)
            : this(count, false)
        {
            int max = Maths.Mathelp.MIN(length * 4, stream.Length);
            MemoryTool.WriteStruct(stream, field, max);

        }
        /// <summary>
        /// Initialize a field using a bitfield, if the stream.lenght &lt; size * 32 the remain bits will be set to 0
        /// </summary>
        protected BitField(int count, int[] stream)
            : this(count, false)
        {
            int max = Maths.Mathelp.MIN(length, stream.Length);
            for (int i = 0; i < max; field[i] = stream[i], i++) ;
        }

        /// <summary>
        /// The size in bytes of internal field array
        /// </summary>
        public int BytesSize
        {
            get { return sizeof(int) * (length + 2); }
        }

        /// <summary>
        /// The size of bit's field, can be any number but BytesSize was always a multiple of 4 bytes
        /// </summary>
        public int Count => bitCount;

        /// <summary>
        /// Count numbers of bit 1. It's a calculation, not a simple returned value
        /// </summary>
        public int GetCount1
        {
            get
            {
                int count = 0;
                int mask = 1;
                foreach (int chunk in field)
                {
                    mask = 1;
                    switch (chunk)
                    {
                        case FULL: count += 32; break;
                        case 0: break;
                        default: for (int i = 0; i < 32; i++, mask <<= 1) if ((chunk & mask) != 0) count++; break;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// return true for '1' or false for '0'
        /// </summary>
        public bool this[int i]
        {
            get { return getBit(i / 32, i % 32); }
            set { setBit(i / 32, i % 32, value); }
        }

        public void SetAll(bool value)
        {
            int n = value ? FULL : 0;
            for (int i = 0; i < length; i++) field[i] = n;
        }

        /// <summary>
        /// Do AND operator for all bits, if bitarray is smaller the missing value are considered '1'
        /// </summary>
        public void AND(BitField bitarray)
        {
            int min = length;
            if (min > bitarray.length) min = bitarray.length;
            for (int i = 0; i < min; i++) field[i] = field[i] & bitarray.field[i];
        }
        /// <summary>
        /// Do OR operator for all bits, if bitarray is smaller the missing value are considered '0'
        /// </summary>
        public void OR(BitField bitarray)
        {
            int min = length;
            if (min > bitarray.length) min = bitarray.length;
            for (int i = 0; i < min; i++) field[i] = field[i] | bitarray.field[i];
        }
        /// <summary>
        /// Do NOT operator for all bits
        /// </summary>
        public void NOT()
        {
            for (int i = 0; i < length; i++) field[i] = ~field[i];
        }


        protected void setBit(int ichunk, int ioffset, bool value)
        {
            if (value) field[ichunk] |= 1 << ioffset;
            else field[ichunk] &= ~(1 << ioffset);
        }

        protected bool getBit(int ichunk, int ioffset)
        {
            return (field[ichunk] & (1 << ioffset)) != 0;
        }


        /// <summary>
        /// Find in the integer's chunk the "value" from "ioffset"
        /// </summary>
        protected int search(int ichunk, int ioffset, bool value)
        {
            while (ioffset < 32 && getBit(ichunk, ioffset) != value) { ioffset++; }
            return ioffset >= 32 ? -1 : ioffset + ichunk * 32;
        }
        /// <summary>
        /// Find in the field the "value" from "istart".
        /// Code optimized
        /// </summary>
        protected int search(int istart, bool value)
        {
            // the integer's block containing the istart's index
            int ichunkStart = istart / 32;
            // the bits offset of integer's bits
            int ioffset = istart % 32;
            // the index of integer's array
            int ichunk = ichunkStart;
            // the all '1' bits after ioffset
            int maskR, empty;

            if (value)
            {
                // the all '1' bits after ioffset
                maskR = FULL << ioffset;
                empty = 0;
            }
            else
            {
                maskR = FULL << (ioffset);
                empty = FULL;
            }

            //string test0 = Tool.GetBinaryString((uint)field[ichunk]);
            //string test2 = Tool.GetBinaryString((uint)maskR);


            //--------------------------------------
            // test the list from "start" to "end"
            //--------------------------------------


            // beginning integer contain a '1' after offset (valid for case '1' or '0')
            int chunkInteger = value ? field[ichunk] : ~field[ichunk];

            //string test = Tool.GetBinaryString((uint)(chunkInteger & maskR));

            if ((chunkInteger & maskR) != 0)
            {
                return search(ichunk, ioffset, value);
            }
            ichunk++;

            // filter 32 by 32 indices the list for a fast search
            while (ichunk < length && field[ichunk] == empty) { ichunk++; }

            //--------------------------------------
            // if not found test from "0" to "start"
            //--------------------------------------
            if (ichunk >= length)
            {
                // filter 32 by 32 indices the list for a fast search
                ichunk = 0;
                while (ichunk < ichunkStart && field[ichunk] == empty) { ichunk++; }

                if (ichunk >= ichunkStart)
                {
                    // begining integer contain a '1' before offset
                    chunkInteger = value ? field[ichunk] : ~field[ichunk];
                    if ((chunkInteger & (~maskR)) != 0)
                        return search(ichunk, 0, value);
                }
                else
                {
                    return search(ichunk, 0, value);
                }
                //--------------------------------------
                // if all chunk block are empty return not-found
                //--------------------------------------
                return -1;
            }
            else
            {
                return search(ichunk, 0, value);
            }
        }
        /// <summary>
        /// Find in the field the "value" from "istart".
        /// code use the standard access function
        /// </summary>
        protected int search_old(int istart, bool value)
        {
            int i = istart;
            // first test the list from "istart" to "end"
            while (i < bitCount && this[i] != value) { i++; }

            // if not found test from "0" to "istart"
            if (i >= bitCount)
            {
                i = 0;
                while (i < istart && this[i] != value) { i++; }
                if (i >= istart) return -1;
            }

            return i;
        }

        /// <summary>
        /// Convert bitarray in string with '1' or '0'
        /// </summary>
        /// <param name="compactmode">the 3dstudio maxscript bitarray is the best way to rappresent a random sequence</param>
        protected string getbitstring(int Start, int Size, bool compactmode)
        {
            int End = Start + Size;

            // convert to a string like 3dstudio maxscript bitarray : {1...3,5} where show only true values
            if (compactmode)
            {
                StringBuilder str = new StringBuilder();

                int istart = Start;
                int iend = Start;
                bool firstgroup = true;
                str.Append('{');

                do
                {
                    // jump to next value '1'
                    // remark : the first boolean test is necessary to exit if you reach the last value
                    while (istart < End && !this[istart]) istart++;
                    iend = istart + 1;

                    // jump to next value '0'
                    while (iend < End && this[iend]) iend++;

                    if (istart < End)
                    {
                        if (istart > Start && !firstgroup)
                            str.Append(',');

                        if (istart + 1 < iend)
                            str.Append(string.Format("{0}..{1}", istart - Start, iend - Start - 1));
                        else
                            str.Append(string.Format("{0}", istart - Start));

                        istart = iend;
                        firstgroup = false;
                    }
                    else
                    {
                        break;
                    }
                }
                while (iend < End);

                str.Append('}');

                return str.ToString();
            }
            // write bitsxbits
            else
            {
                char[] chars = new char[Size];
                for (int i = 0; i < Size; i++) chars[i] = this[Start + i] ? '1' : '0';
                return new string(chars);
            }
        }

        /// <summary>
        /// Debugger for all derived bitfield classes, it show the field array in compact mode
        /// </summary>
        public static string ToCompactString(BitField bitfield)
        {
            return bitfield.getbitstring(0, bitfield.bitCount, true);
        }

        public virtual string ToExtendedString()
        {
            return getbitstring(0, length * 32, false);
        }

        public virtual string ToCompactString()
        {
            return ToCompactString(this);
        }

        public static T Clone<T>(T source) where T : BitField
        {
            T copy = (T)Activator.CreateInstance(typeof(T), true);
            copy.field = (int[])source.field.Clone();
            copy.length = source.length;
            copy.bitCount = source.bitCount;
            return copy;
        }


        #region Enumerators
        public IEnumerator<bool> GetEnumerator()
        {
            int i, j;

            for (i = 0; i < length; i++)
            {
                int remain = i < length - 1 ? 32 : bitCount % 32;
                int value = field[i];
                for (j = 0; j < remain; j++)
                    yield return (value & (1 << j)) != 0;
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// yield return implementation, return all bit from 0 to last
        /// </summary>
        public IEnumerable<bool> BooleanList
        {
            get
            {
                int i = 0;
                // from 0 to last full-used chunk
                while (i < length - 1)
                {
                    int chunk = field[i];

                    if (chunk == FULL) for (int j = 0; j < 32; j++) yield return true;
                    else if (chunk == 0) for (int j = 0; j < 32; j++) yield return false;
                    else for (int  j = 0, mask = 1; j < 32; j++, mask <<= 1) yield return (field[i] & mask) != 0;

                    i++;
                }
                // last chunk can not be completly used
                if (i < length)
                {
                    int chunk = field[i];
                    int remain = bitCount % 32;
                    for (int j = 0, mask = 1; j < remain; j++, mask <<= 1) yield return (field[i] & mask) != 0;
                }
                yield break;
            }
        }


        /// <summary>
        /// inverted list of <see cref="IndicesList"/>
        /// </summary>
        public IEnumerable<int> IndicesListInverted
        {
            get
            {
                int i = length - 1;

                // last chunk can not be completly used
                int chunk = field[i];
                int remain = bitCount % 32;
                for (int j = remain, mask = 1 << remain-1; j >= 0; j--, mask >>= 1)
                    if ((chunk & mask) != 0) yield return i * 32 + j;
                i--;

                // from last full-used chunk to 0
                while (i >= 0)
                {
                    chunk = field[i];

                    if (chunk == FULL)
                        for (int j = 31; j >= 0; j--) yield return i * 32 + j;
                    else if (chunk != 0)
                        for (int j  = 31, mask = 1 << 31; j >= 0; j--, mask >>= 1)
                            if ((chunk & mask) != 0) yield return i * 32 + j;
                    i--;
                }
            }
        }



        /// <summary>
        /// yield return implementation, return the bit value as position index, from begin to end
        /// </summary>
        public IEnumerable<int> IndicesList
        {
            get
            {
                int i = 0;

                // from 0 to last full-used chunk
                while (i < length - 1)
                {
                    int chunk = field[i];

                    // if chunk is full return all number
                    if (chunk == FULL)
                        for (int j = 0; j < 32; j++) yield return i * 32 + j;
                    // if chunk is not empty and not full, calculate each bit
                    else if (chunk != 0)
                        for (int j = 0, mask = 1; j < 32; j++, mask <<= 1)
                            if ((chunk & mask) != 0) yield return i * 32 + j;
                    // if chunk is empty move to next
                    i++;
                }

                // last chunk can not be completly used
                if (i < length)
                {
                    int chunk = field[i];
                    int remain = bitCount % 32;
                    for (int j = 0, mask = 1; j < remain; j++, mask <<= 1)
                        if ((chunk & mask) != 0) yield return i * 32 + j;
                }
                yield break;
            }
        }
        #endregion
    }
    /// <summary>
    /// One dimension Bit array
    /// </summary>
    public class BitArray1 : BitField
    {
        public BitArray1(int count, byte[] stream) : base(count,stream) { }

        public BitArray1(int count, bool initialvalue = false) : base(count, initialvalue) { }


        public BitArray1 Clone()
        {
            return BitField.Clone(this);
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T">must be a not negative int, uint, short, ushort, byte, sbyte</typeparam>
        /// <param name="offset">to optimize and reduce size, use the min value of list, all number in list will be subtracted this value</param>
        /// <param name="size">number of bits, generally it's the max value of list, is necessary to initialize the internal array</param>
        /// <returns></returns>
        public static BitArray1 Create<T>(IEnumerable<T> list, int offset, int size) where T : struct, IComparable
        {
            BitArray1 array = new BitArray1(size);
            foreach(var number in list)
            {
                int val = Convert.ToInt32(number) - offset;
                array[val] = true;
            }
            return array;

        }


        /// <summary>
        /// Search the position of a index by boolean value, return -1 if not found
        /// </summary>
        public int SearchNext(int istart, bool value)
        {
            return search(istart, value);
        }
        /// <summary>
        /// Search the position of a index by boolean value, return -1 if not found
        /// </summary>
        public int SearchNext_old(int istart, bool value)
        {
            return search_old(istart, value);
        }
    }
    /// <summary>
    /// Two dimension Bit array
    /// </summary>
    public class BitArray2 : BitField
    {
        // ---------> j (W)
        // |
        // |
        // |
        // |
        // v i(H)

        int width, heigth;

        /// <summary>
        /// num of rows
        /// </summary>
        public int Heigth { get { return heigth; } }
        /// <summary>
        /// num or columns
        /// </summary>
        public int Width { get { return width; } }

        /// <summary>
        /// </summary>
        /// <param name="width">colums</param>
        /// <param name="heigth">rows</param>
        public BitArray2(int width, int heigth, bool initialvalue = false)
            : base(width * heigth, initialvalue)
        {
            this.width = width;
            this.heigth = heigth;
        }

        /// <summary>
        /// </summary>
        /// <param name="i">Row [0-Height]</param>
        /// <param name="j">Column [0-Width]</param>
        /// <returns></returns>
        public bool this[int i, int j]
        {
            get { return base[i * width + j]; }
            set { base[i * width + j] = value; }
        }

        /// <summary>
        /// Convert a bitmap to a 2d grid of bit, set '1' when Brightness of pixel less than threshold
        /// </summary>
        public static BitArray2 LoadFromGrayBitmap(Bitmap map, float threshold = 0.5f)
        {
            BitArray2 densitymap = new BitArray2(map.Width, map.Height);

            int w = map.Width;
            int h = map.Height;

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color p = map.GetPixel(x, h - y - 1);

                    //Console.WriteLine(string.Format("get {0} {1} {2}", x, y,p));

                    if (p.GetBrightness() < threshold)
                    {
                        //Console.WriteLine(string.Format("Set {0} {1}",x,y));
                        densitymap[x, y] = true;
                    }
                }
            return densitymap;
        }



        /// <summary>
        /// Remember that the output is a table with first value the coord[0,0] so match graficaly to a bitmap
        /// coordinates, not cartesian
        /// </summary>
        public override string ToExtendedString()
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < heigth; i++)
                str.AppendLine(base.getbitstring(i * width, width, false));
            return str.ToString();
        }
        public override string ToCompactString()
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < heigth; i++)
                str.AppendLine(i + "." + base.getbitstring(i * width, width, true));
            return str.ToString();
        }

    }
    /// <summary>
    /// Three dimension Bit array
    /// </summary>
    public class BitArray3 : BitField
    {
        // ---------> j (W)
        // |\
        // | \
        // |  v k(D)
        // |
        // v i(H)

        public readonly int width, heigth, depth;

        public BitArray3(int width, int heigth, int depth, bool initialvalue = false)
            : base(width * heigth * depth, initialvalue)
        {
            this.width = width;
            this.heigth = heigth;
            this.depth = depth;
        }


        public BitArray3(int width, int heigth, int depth, byte[] stream)
            : base(width * heigth * depth, stream)
        {
            this.width = width;
            this.heigth = heigth;
            this.depth = depth;
        }


        /// <summary>
        /// </summary>
        /// <param name="i">Row    [0-Heigth]</param>
        /// <param name="j">Column [0-Width]</param>
        /// <param name="k">Depth  [0-Depth]</param>
        public bool this[uint i, uint j, uint k]
        {
            get { return this[(int)i, (int)j, (int)k]; }
            set { this[(int)i, (int)j, (int)k] = value; }
        }

        /// <summary>
        /// </summary>
        /// <param name="i">Row    [0-Heigth]</param>
        /// <param name="j">Column [0-Width]</param>
        /// <param name="k">Depth  [0-Depth]</param>
        public bool this[int i, int j, int k]
        {
            get { return base[k * (width * heigth) + j * width + i]; }
            set { base[k * (width * heigth) + j * width + i] = value; }
        }

        /// <summary>
        /// </summary>

        public bool this[Vector3ui coord]
        {
            get { return this[coord.x, coord.y, coord.z]; }
            set { this[coord.x, coord.y, coord.z] = value; }
        }

        public override string ToExtendedString()
        {
            StringBuilder str = new StringBuilder();
            for (int k = 0; k < depth; k++)
            {
                str.AppendLine("k=" + k);
                for (int i = 0; i < heigth; i++)
                    str.AppendLine(base.getbitstring(k * width * heigth + i * width, width, false));
            }
            return str.ToString();
        }


        public override string ToCompactString()
        {
            StringBuilder str = new StringBuilder();
            for (int k = 0; k < depth; k++)
            {
                str.AppendLine("k=" + k);
                for (int i = 0; i < heigth; i++)
                    str.AppendLine(i + "." + base.getbitstring(k * width * heigth + i * width, width, true));
            }
            return str.ToString();
        }

    }

}
