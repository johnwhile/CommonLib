
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Maths
{
    public enum IndexFormat : byte
    {
        None = 0,
        Index8bit = 1,
        Index16bit = 2,
        Index32bit = 4
    }


    /// <summary>
    /// Auto swith from byte to ushort to int
    /// </summary>
    public class RawIndices : IEnumerable<int>
    {
        public object owner;

        IndexFormat m_format = IndexFormat.None;
        StructBuffer<byte> m_buffer;

        int min = int.MaxValue;
        int max = int.MinValue;

        public int Count
        {
            get
            {
                switch (m_format)
                {
                    case IndexFormat.Index8bit: return m_buffer.BytesCount;
                    case IndexFormat.Index16bit: return m_buffer.BytesCount / 2;
                    case IndexFormat.Index32bit: return m_buffer.BytesCount / 4;
                    default: return 0;
                }
            }
        }

        public RawIndices(IndexFormat format = IndexFormat.Index8bit, int capacity = 0)
        {
            m_buffer = new StructBuffer<byte>(capacity * (int)format);
            Format = format;
        }
        public IndexFormat Format
        {
            get => m_format;
            private set
            {
                //Resize entire array
                if (m_format < value)
                {
                    //Count are calculated with m_format value
                    int tmp_Count = Count;

                    int[] tmp = new int[tmp_Count];
                    for (int i = 0; i < tmp_Count; i++) tmp[i] = Getter(i);

                    switch (value)
                    {
                        case IndexFormat.Index8bit: Getter = Get8; Setter = Set8; break;
                        case IndexFormat.Index16bit: Getter = Get16; Setter = Set16; break;
                        case IndexFormat.Index32bit: Getter = Get32; Setter = Set32; break;
                    }
                    for (int i = 0; i < tmp_Count; i++) Setter(tmp[i], i);
                }
                m_format = value;
            }
        }
        public int this[int index]
        {
            get => Getter(index);
            set
            {
                //change format if required
                var format = IndexFormat.Index8bit;
                if (value > byte.MaxValue) format = IndexFormat.Index16bit;
                if (value > ushort.MaxValue) format = IndexFormat.Index32bit;
                if (format > Format) Format = format;

                Setter(value, index);
            }
        }
        public void Add(int value) { this[Count] = value; }
        public void AddRange(IEnumerable<int> enumerable)
        {
            if (enumerable is int[] array)
            {
                AddRange(array);
                return;
            }

            int e_max = enumerable.Max();

            var format = IndexFormat.Index8bit;
            if (e_max > byte.MaxValue) format = IndexFormat.Index16bit;
            if (e_max > ushort.MaxValue) format = IndexFormat.Index32bit;
            if (format > Format) Format = format;

            using (var enumerator = enumerable.GetEnumerator())
            {
                int i = Count;
                while (enumerator.MoveNext())
                    Setter(enumerator.Current, i++);
            }
        }
        public void AddRange(byte[] array)
        {
            int offset = Count;
            for (int i = 0; i < array.Length; i++)
            {
                Setter(array[i], i + offset);
            }
        }
        public void AddRange(ushort[] array)
        {
            int a_max = array.Max();
            var format = IndexFormat.Index8bit;
            if (a_max > byte.MaxValue) format = IndexFormat.Index16bit;
            if (format < Format) Format = format;

            int offset = Count;
            for (int i = 0; i < array.Length; i++)
                Setter(array[i], i + offset);
        }
        public void AddRange(int[] array)
        {
            int a_max = array.Max();
            var format = IndexFormat.Index8bit;
            if (a_max > byte.MaxValue) format = IndexFormat.Index16bit;
            if (a_max > ushort.MaxValue) format = IndexFormat.Index32bit;
            if (format > Format) Format = format;

            int offset = Count;
            for (int i = 0; i < array.Length; i++)
                Setter(array[i], i + offset);

        }
        public void AddRange(RawIndices attribute)
        {
            if (Format < attribute.Format)
                Format = attribute.Format;

            int offset = Count;
            for (int i = 0; i < attribute.Count; i++)
                Setter(attribute[i], i + offset);

        }


        #region Get Set methods
        delegate int GetterDelegate(int index);
        delegate void SetterDelegate(int value, int index);
        GetterDelegate Getter;
        SetterDelegate Setter;
        int Get8(int index) => m_buffer.Get(index);
        int Get16(int index) => m_buffer.GetGeneric<ushort>(index);
        int Get32(int index) => m_buffer.GetGeneric<int>(index);
        void Set8(int value, int index)
        {
            minmax(value);
            m_buffer.Set((byte)value, index);
        }
        void Set16(int value, int index)
        {
            minmax(value);
            m_buffer.SetGeneric((ushort)value, index, 2);
        }
        void Set32(int value, int index)
        {
            minmax(value);
            m_buffer.SetGeneric(value, index, 4);
        }
        #endregion

        void minmax(int value)
        {
            if (min > value) min = value;
            if (max < value) max = value;
        }
        public int[] ToInt32Array(int offset = 0, int length = -1)
        {
            if (length < 0) length = Count;
            int[] array = new int[length - offset];

            for (int i = offset; i < length; i++)
                array[i - offset] = Getter(i);

            return array;
        }
        public ushort[] ToUint16Array(int offset = 0, int length = -1)
        {
            if (length < 0) length = Count;
            ushort[] array = new ushort[length - offset];

            for (int i = offset; i < length; i++)
                array[i - offset] = (ushort)Getter(i);

            return array;
        }

        /// <summary>
        /// a sort of lossless compression for big integer array
        /// </summary>
        public static RawIndices Read(BinaryReader reader, out int FirstVertex)
        {
            int count = reader.ReadInt32();
            int offset = reader.ReadInt32();
            byte format = reader.ReadByte();

            RawIndices indices = null;
            FirstVertex = offset;

            unchecked
            {
                switch (format)
                {
                    case 8:
                        indices = new RawIndices(IndexFormat.Index8bit, count);
                        for (int i = 0; i < count; i++) indices.Add(reader.ReadByte() + offset);
                        break;
                    case 16:
                        indices = new RawIndices(IndexFormat.Index16bit, count);
                        for (int i = 0; i < count; i++) indices.Add(reader.ReadUInt16() + offset);
                        break;
                    case 32:
                        indices = new RawIndices(IndexFormat.Index32bit, count);
                        if (reader.ReadBoolean())
                            for (int i = 0; i < count; i++) indices.Add(reader.Read7BitEncodedInt() + offset);
                        else
                            for (int i = 0; i < count; i++) indices.Add((int)reader.ReadUInt32() + offset);
                        break;
                }
            };
            return indices;

        }
        public void Write(BinaryWriter writer, bool use7bitencoder = false)
        {
            writer.Write(Count);

            (int offset, int max) = Mathelp.GetMinMax(this);
            max -= offset;
            writer.Write(offset);//don't make sense use offset for 32 bit

            if (max <= byte.MaxValue)
            {
                writer.WriteByte(8);
                foreach (var i in this) writer.Write((byte)(i - offset));
            }
            else if (max <= ushort.MaxValue)
            {
                writer.WriteByte(16);
                foreach (var i in this) writer.Write((ushort)(i - offset));
            }
            else
            {
                writer.WriteByte(32);
                writer.WriteBoolean(use7bitencoder);
                if (use7bitencoder)
                    foreach (var i in this) writer.Write7BitEncodedInt(i);
                else
                    foreach (var i in this) writer.Write(i);
            }
        }

        public virtual IEnumerator<int> GetEnumerator()
        {
            int count = Count;
            for (int i = 0; i < count; i++)
                yield return Getter(i);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
