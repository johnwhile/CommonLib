using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace Common
{
    /// <summary>
    /// Manage the struct array.
    /// </summary>
    [DebuggerDisplay("Memory = {ByteSize}, {typeof(T).Name} count = {Count}")]
    public class StructBuffer<T> : IEnumerable<T> where T : unmanaged
    {
        const int MAXSIZE = 2146435071;

        public int Version { get;private set; } = 0;
        public int ResizeCount { get; private set; } = 0;

        const int MinBufferLenght = 1;
        int m_bytecount;
        int m_tsize;
        T[] m_buffer;

        public T[] Buffer => m_buffer;

        public StructBuffer(int items = 0, int sizeMuliplier = 1, int reduceMultiplier = 1)
        {
            m_tsize = Marshal.SizeOf<T>();
            if (items > 0) m_buffer = new T[items];
            m_bytecount = 0;
        }

        public static StructBuffer<T> CreateBytes(int bytesize = 0)
        {
            int sizeoft = Marshal.SizeOf<T>();
            return new StructBuffer<T>((bytesize + sizeoft - 1) / sizeoft);
        }

        public static StructBuffer<T> Create<K>(IEnumerable<K> enumerable, int offset = 0, int length = -1) where K : unmanaged
        {
            StructBuffer<T> instance = new StructBuffer<T>();

            if (typeof(T) == typeof(K)) 
                instance.AddRange(enumerable as IEnumerable<T>, offset, length);
            else 
                instance.AddGenericRange(enumerable, Marshal.SizeOf<K>(), offset, length);

            return instance;
        }

        public StructBuffer(IEnumerable<T> enumerable) : this(0)
        {
            AddRange(enumerable);
        }

        public void Write(BinaryWriter writer, int startindex = 0, int lenght = -1)
        {
            if (startindex < 0 || startindex >= Count) throw new ArgumentOutOfRangeException();
            if (lenght < 0) lenght = Count - startindex;
            writer.WriteUnsafe(m_buffer, startindex, lenght);
            Version++;
        }

        public int SizeOfType => m_tsize;

        /// <summary>
        /// valid bytes use, always less than <see cref="BufferSize"/><br/>
        /// It can be truncate if changing the <see cref="BufferSize"/>
        /// </summary>
        public int BytesCount => m_bytecount;

        /// <summary>
        /// number of T element contained, it's <br/>
        /// <b><see cref="BytesCount"/> / <see cref="m_tsize"/></b>
        /// </summary>
        public int Count
        {
            get => m_bytecount / m_tsize;
            set
            {
                if (value < 0) value = 0;
                if (value > Count)  EnsureSize(value * m_tsize);
                if (value < Count) TrimSize(value * m_tsize);
                m_bytecount = value * m_tsize;
            }
        }
        /// <summary>
        /// number of T elements can be containes, it's <br/>
        /// <b><see cref="ByteSize"/> / <see cref="m_tsize"/></b>
        /// </summary>
        public int Capacity
        {
            get => ByteSize / m_tsize;
            set { ByteSize = Math.Min(getNearSquare(value * m_tsize), MAXSIZE); }
        }


        public virtual T this[int index]
        {
            get => Get(index);
            set => Set(value, index);
        }

        public void Add(T item) => Set(item, Count);
        public void AddGeneric<K>(K item, int sizeof_k) where K : unmanaged => SetGeneric(item, m_bytecount / sizeof_k, sizeof_k);
        public void AddRange(IEnumerable<T> enumerable, int offset = 0, int length = -1)
        {
            if (enumerable == null) return;

            if (enumerable is T[] array)
            {
                Set(array, Count, offset, length);
            }
            else
            {
                if (enumerable is ICollection<T> collection)
                {
                    length = length < 0 ? collection.Count : Math.Min(length, collection.Count);
                    EnsureSize(length * m_tsize + m_bytecount);
                }
                foreach(T item in enumerable) Add(item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sizeof_k">must be passed to improve performance</param>
        public void AddGenericRange<K>(IEnumerable<K> enumerable, int sizeof_k = 0, int offset_k = 0, int length_k = -1) where K : unmanaged
        {
            if (sizeof_k<=0) sizeof_k = Marshal.SizeOf<K>();

            if (enumerable is K[] array)
            {
                SetGeneric(array, m_bytecount / sizeof_k, sizeof_k, offset_k, length_k);
            }
            else
            {
                if (enumerable is ICollection<K> collection)
                {
                    length_k = length_k < 0 ? collection.Count : Math.Min(length_k, collection.Count);
                    EnsureSize(length_k * sizeof_k + m_bytecount);
                }
                foreach (K item in enumerable) AddGeneric(item, sizeof_k);
            }
        }

        /// <summary>
        /// Set a range of items from index, resize array if need
        /// </summary>
        public void Set(T[] array, int index, int srcOffset = 0, int srcCount = -1)
        {
            //BlockCopy not work for non-primitive
            if (typeof(T).IsPrimitive)
            {
                if (srcCount < 0) srcCount = array.Length - srcOffset;
                int bytesrequest = (index + srcCount) * m_tsize;
                EnsureSize(bytesrequest);
                System.Buffer.BlockCopy(array, srcOffset * m_tsize, m_buffer, index * m_tsize, srcCount * m_tsize);
                m_bytecount = Math.Max(m_bytecount, bytesrequest);
                Version++;
            }
            else
            {
                SetGeneric(array, index, m_tsize, srcOffset, srcCount);
            }
        }

        /// <summary>
        /// Set the item to index, resize array if need
        /// </summary>
        public void Set(T value, int index)
        {
            int bytesrequest = (index + 1) * m_tsize;
            EnsureSize(bytesrequest);
            m_buffer[index] = value;
            if (bytesrequest > m_bytecount) m_bytecount = bytesrequest;

            Version++;
        }

        /// <summary>
        /// Intentionally no exeption check
        /// </summary>
        public T Get(int index) => m_buffer[index];
        public ref T GetByRef(int index) => ref m_buffer[index];

        /// <summary>
        /// Set a range of items from index, resize array if need
        /// </summary>
        /// <typeparam name="K">interpret the array with another value definition</typeparam>
        /// <param name="index_k">index position if array is K[]</param>
        /// <param name="srcCount">number of array's elements to copy</param>
        /// <param name="srcOffset">offset of array's elements to copy from</param>
        public unsafe void SetGeneric<K>(K[] array, int index_k, int sizeof_k, int srcOffset = 0, int srcCount = -1) where K : unmanaged
        {
            //int sizeofk = sizeofK.HasValue ? sizeofK.Value : Marshal.SizeOf<K>();

            if (srcCount < 0) srcCount = array.Length - srcOffset;

            int bytesrequest = (index_k + srcCount) * sizeof_k;
            EnsureSize(bytesrequest);

            fixed (T* ptr_dst = m_buffer)
            fixed (K* src = array)
            {
                K* dst = (K*)ptr_dst;
                System.Buffer.MemoryCopy(src + srcOffset, dst + index_k, ByteSize, srcCount * sizeof_k);

            }
            if (bytesrequest > m_bytecount) m_bytecount = bytesrequest;

            Version++;
        }
        /// <summary>
        /// Set the item to index, resize array if need
        /// </summary>
        /// <typeparam name="K">interpret the array with another value definition</typeparam>
        /// <param name="index_k">index position if array is K[]</param>
        public unsafe void SetGeneric<K>(K value, int index_k, int sizeofK) where K : unmanaged
        {
            //int sizeof_k = sizeofK.HasValue ? sizeofK.Value : Marshal.SizeOf<K>();

            int bytesrequest = (index_k + 1) * sizeofK;
            EnsureSize(bytesrequest);
            fixed (T* ptr = m_buffer)
            {
                K* ptr_b = (K*)ptr + index_k;
                *ptr_b = value;
            }
            if (bytesrequest > m_bytecount) m_bytecount = bytesrequest;

            Version++;
        }

        /// <summary>
        /// Intentionally no exeption check
        /// </summary>
        /// <typeparam name="K">interpret the array with another value definition</typeparam>
        /// <param name="index_k">index position if array is K[]</param>
        public unsafe K GetGeneric<K>(int index_k) where K : unmanaged
        {
            fixed (T* ptr = m_buffer)
            {
                K* ptr_k = (K*)ptr;
                ptr_k += index_k;
                return *ptr_k;
            }
        }

        public unsafe ref K GetGenericByRef<K>(int index_k) where K : unmanaged
        {
            fixed (T* ptr = m_buffer)
            {
                K* ptr_k = (K*)ptr;
                ptr_k += index_k;
                return ref *ptr_k;
            }
        }

        /// <summary>
        /// Ensures that the buffer contains enought bytes, if need double the size
        /// </summary>
        private void EnsureSize(int size)
        {
            if (ByteSize < size)
            {
                //estimate new size
                int newsize = Math.Min(getNearSquare(size), MAXSIZE);
                //do resize
                ByteSize = newsize;
            }
        }
        /// <summary>
        /// Reduce buffer size
        /// </summary>
        private void TrimSize(int size)
        {
            if (ByteSize > size)
            {
                //estimate new size
                int newsize = Math.Min(getNearSquare(size), MAXSIZE);
                //do resize
                ByteSize = newsize;
            }
        }

        private int getNearSquare(int number)
        {
            int n = 1;
            while (n < number) n <<= 1;
            return n;
        }

        /// <summary>
        /// Return the byte size of buffer. Resize the array if set a different value.
        /// </summary>
        /// <remarks>Of course, the set value will be the minimum possible respect the size of T</remarks>
        public int ByteSize
        {
            get => m_buffer?.Length * m_tsize ?? 0;
            private set
            {
                //2 146 435 071
                if (value > MAXSIZE) throw new OverflowException("array too big");

                int count = (value + m_tsize - 1) / m_tsize;
                int length = m_buffer?.Length ?? 0;
                m_bytecount = Math.Min(m_bytecount, count * m_tsize);


                if (count != length)
                {
                    ResizeCount++;
                    if (count == 0) m_buffer = new T[0];
                    else
                    {
                        if (length == 0) m_buffer = new T[count];
                        else Array.Resize(ref m_buffer, count);
                    }
                }
            }
        }

        public unsafe void CopyTo(Array destination, int srcByteOffset = 0, int srcByteSize = -1, int dstByteOffset = 0)
        {
            if (srcByteSize < 0) srcByteSize = m_bytecount - srcByteOffset;
            Array.Copy(m_buffer, destination, destination.Length);
        }

        public static implicit operator T[](StructBuffer<T> array) => array.m_buffer;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return m_buffer[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Clear()
        {
            m_buffer = null;
            m_bytecount = 0;
            Version++;
        }
    }
}
