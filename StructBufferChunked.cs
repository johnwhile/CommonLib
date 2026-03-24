using Common.Maths;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Common
{
    /// <summary>
    /// Manage the struct array by chunks.
    /// </summary>
    [DebuggerDisplay("Memory = {ByteSize}, {typeof(T).Name} count = {Count}")]
    public class StructBufferChunked<T> : IEnumerable<T> where T : unmanaged
    {
        int CHUNK = 1024;

        public int ResizeCount { get; private set; } = 0;

        const int MinBufferLenght = 4;
        int m_chunkcount;
        int m_bytecount;
        int m_tsize;
        T[][] m_buffer;


        public StructBufferChunked(int bytesize = 0, int chunksize = 64)
        {
            CHUNK = chunksize;
            m_tsize = Marshal.SizeOf<T>();
            m_bytecount = 0;
            m_chunkcount = 0;
            ByteSize = bytesize;
        }

        public StructBufferChunked(T[] array, int chunksize = 64) :
            this((array.Length + chunksize - 1) / chunksize * chunksize, chunksize)
        {
            Set(array, 0, 0, array.Length);
        }

        /// <summary>
        /// number of T element contained
        /// </summary>
        public int Count => m_bytecount / m_tsize;
        /// <summary>
        /// valid bytes use, always less than <see cref="ByteSize"/><br/>
        /// It can be truncate if changing the <see cref="ByteSize"/>
        /// </summary>
        public int ByteCount => m_bytecount;

        /// <summary>
        /// number of T elements can be containes
        /// </summary>
        public int Capacity
        {
            get => ByteSize / m_tsize;
            set => EnsureSize(value * m_tsize);
        }

        public int TypeCapacity(int sizeofK) => ByteSize / sizeofK;


        public virtual T this[int index]
        {
            get => Get(index);
            set => Set(value, index);
        }

        public void Add(T item)
        {
            this[m_bytecount / m_tsize] = item;
        }

        /// <summary>
        /// Set a range of items from index, increase buffer size if necessary
        /// </summary>
        public void Set(T[] array, int index, int srcOffset = 0, int srcCount = -1)
        {
            SetGeneric(array, index, m_tsize, srcOffset, srcCount);
        }

        /// <summary>
        /// Set the item to index, increase buffer size if necessary
        /// </summary>
        public void Set(T value, int index)
        {
            int requestsize = (index + 1) * m_tsize;
            EnsureSize(requestsize);
            m_buffer[index / CHUNK][index % CHUNK] = value;
            if (requestsize > m_bytecount) m_bytecount = requestsize;
        }

        /// <summary>
        /// Get the item at index position. intentionally no exeption check are made for outofrange
        /// </summary>
        public T Get(int index) => m_buffer[index / CHUNK][index % CHUNK];

        /// <summary>
        /// <inheritdoc cref="Set(T[], int, int, int)"/>
        /// </summary>
        /// <typeparam name="K">interpret the array with another value definition</typeparam>
        /// <param name="index">index position if array is K[]</param>
        /// <param name="srcCount">number of array's elements to copy</param>
        /// <param name="srcOffset">offset of array's elements to copy from</param>
        public unsafe void SetGeneric<K>(K[] array, int index, int? sizeofK, int srcOffset = 0, int srcCount = -1) where K : unmanaged
        {
            int sizeof_k = sizeofK.HasValue ? sizeofK.Value : Marshal.SizeOf<K>();

            if (srcCount < 0) srcCount = array.Length - srcOffset;
            if (srcCount > array.Length) srcCount = array.Length;

            srcOffset *= sizeof_k;
            srcCount *= sizeof_k;

            int requestsize = index * m_tsize + srcCount;
            EnsureSize(requestsize);

            int chunk = index / CHUNK;
            int chunkoffset = (index % CHUNK) * m_tsize;

            while (srcCount > 0)
            {
                int count_b = Math.Min(srcCount, CHUNK * m_tsize - chunkoffset);
                setrange(array, srcOffset, count_b, m_buffer[chunk], chunkoffset);
                chunk++;
                chunkoffset = 0;
                srcOffset += count_b;
                srcCount -= count_b;
            }
            m_bytecount = Math.Max(m_bytecount, requestsize);
        }

        /// <summary>
        /// Set the item to index, increase buffer size if necessary
        /// </summary>
        /// <typeparam name="K">interpret the array with another value definition</typeparam>
        public unsafe void SetGeneric<K>(K value, int index, int sizeof_k) where K : unmanaged
        {
            int requestsize = (index + 1) * sizeof_k;
            EnsureSize(requestsize);

            fixed (T* ptr = m_buffer[index / CHUNK])
                *((K*)ptr + index % CHUNK) = value;

            if (requestsize > m_bytecount) m_bytecount = requestsize;
        }

        /// <summary>
        /// <inheritdoc cref="Get(int)"/>
        /// </summary>
        /// <typeparam name="K">interpret the array with another value definition</typeparam>
        /// <param name="index">index position if array is K[]</param>
        public unsafe K GetGeneric<K>(int index) where K : unmanaged
        {
            fixed (T* ptr = m_buffer[index / CHUNK])
                return *((K*)ptr + index % CHUNK);
        }

        /// <summary>
        /// Ensures that the buffer contains enought bytes, if need double the size
        /// </summary>
        public void EnsureSize(int size)
        {
            if (ByteSize < size)
            {
                if (size > int.MaxValue) throw new OverflowException("array too big");

                //estimate new size
                int newsize = (size + CHUNK - 1) / CHUNK * CHUNK;
                if (newsize > int.MaxValue) newsize = int.MaxValue - 1;

                //do resize
                ByteSize = newsize;
            }
        }

        /// <summary>
        /// Return the bytesize of buffer. Resize the array if set a different value.
        /// </summary>
        /// <remarks>Of course, the set value will be the minimum possible respect the size of T</remarks>
        public int ByteSize
        {
            get => m_buffer?.Length * CHUNK * m_tsize ?? 0;
            set
            {
                if (value > int.MaxValue) throw new OverflowException("array too big");

                int length = m_chunkcount;
                int chunks = (value + CHUNK - 1) / CHUNK;

                if (length != chunks)
                {
                    ResizeCount++;
                    if (chunks > 0) Array.Resize(ref m_buffer, chunks);
                    else m_buffer = new T[0][];
                    m_chunkcount = chunks;

                    if (chunks > length)
                        for (int i = length; i < chunks; i++) m_buffer[i] = new T[CHUNK];
                }
                m_bytecount = Math.Min(m_bytecount, value * m_tsize);
            }
        }

        public unsafe void CopyTo(Array destination, int srcByteOffset = 0, int srcByteSize = -1, int dstByteOffset = 0)
        {
            if (srcByteSize < 0) srcByteSize = m_bytecount - srcByteOffset;
            Array.Copy(m_buffer, destination, destination.Length);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        /// <summary>
        /// copy a part of array to a specified part of buffer (chunk)
        /// </summary>
        /// <param name="srcoffset_b">source offset in bytes</param>
        /// <param name="srccount_b">source bytes to copy, must be less than <see cref="CHUNK"/></param>
        /// <param name="chunk">the single chunk of buffer to copy</param>
        /// <param name="chunkoffset_b">the destination offset in the current chunk</param>
        unsafe void setrange<K>(K[] source, int srcoffset_b, int srccount_b, T[] chunk, int chunkoffset_b) where K : unmanaged
        {
            fixed (T* ptr_dst = chunk)
            fixed (K* ptr_src = source)
            {
                byte* dst = (byte*)ptr_dst + chunkoffset_b;
                byte* src = (byte*)ptr_src + srcoffset_b;
                Buffer.MemoryCopy(src, dst, CHUNK * m_tsize, srccount_b);
            }
        }
    }
}


