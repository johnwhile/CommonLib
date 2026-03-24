// just to know if System.Array has same performance
#define USESTANDARDARRAY

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Common
{
    /// <summary>
    /// Can improve performance for geometry vertices ?
    /// </summary>
    public class DynamicBuffer<T> where T : struct
    {
        const int CHUNKSIZE = 64;
        int count;
        int sizeofT;

        List<T[]> Chunks;

        public DynamicBuffer(int capacity)
        {
            Chunks = new List<T[]>();
            sizeofT = Marshal.SizeOf<T>();
            Resize(capacity);
        }

        public int Capacity => Chunks.Count * CHUNKSIZE;

        public int Count => count;

        /// <summary>
        /// </summary>
        /// <param name="size">ensure space</param>
        public void Resize(int size)
        {
            (int n, int i) = Address(size - 1);
            for (int j = Chunks.Count; j <= n; j++)
                Chunks.Add(new T[CHUNKSIZE]);
        }

        public void Add(T item)
        {
            count++;
            (int n, int i) = Address(count - 1);
            if (n >= Chunks.Count) Chunks.Add(new T[CHUNKSIZE]);
            Chunks[n][i] = item;
        }

        /// <summary>
        /// </summary>
        /// <param name="ByteSize">The size in byte of destination. (necessary for <see cref="Buffer.MemoryCopy(void*, void*, long, long)"/></param>
        /// <param name="ByteOffset">Byte to skip before write to</param>
        public unsafe void CopyToBuffer(IntPtr destination, long ByteSize, int ByteOffset = 0)
        {
            if (count == 0) return;

            for (int n = 0; n < Chunks.Count; n++)
            {
                var handle = GCHandle.Alloc(Chunks[n], GCHandleType.Pinned);
                var source = handle.AddrOfPinnedObject();
                int sourcesize = sizeofT * (n < Chunks.Count - 1 ? CHUNKSIZE : (count - 1) % CHUNKSIZE + 1);
                Buffer.MemoryCopy(source.ToPointer(), destination.ToPointer(), ByteSize, sourcesize);
                handle.Free();
                destination = IntPtr.Add(destination, sourcesize);
            }
        }

        public T[] ToArray3()
        {
            var array = new T[count];
            int offset = 0;

            
            for (int n = 0; n < Chunks.Count; n++)
            {
                int numofitems = n < Chunks.Count - 1 ? CHUNKSIZE : (count - 1) % CHUNKSIZE + 1;
                var destination = new Span<T>(array, offset, numofitems);
                var source = new Span<T>(Chunks[n], 0, numofitems);

                source.CopyTo(destination);

                offset += numofitems;
            }
            return array;
        }

        public T[] ToArray2()
        {
            var array = new T[count];
            int offset = 0;
            for (int n = 0; n < Chunks.Count; n++)
            {
                int numofitems = n < Chunks.Count - 1 ? CHUNKSIZE : (count - 1) % CHUNKSIZE + 1;
                Array.Copy(Chunks[n], 0, array, offset, numofitems);
                offset += numofitems;
            }
            return array;
        }

        public T[] ToArray()
        {
            var array = new T[count];
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            var pointer = handle.AddrOfPinnedObject();
            CopyToBuffer(pointer, sizeofT * count, 0);
            handle.Free();
            return array;
        }

        public T this[int index]
        {
            get
            {
                (int n, int i) = Address(index);
                return Chunks[n][i];
            }

            set
            {
                (int n, int i) = Address(index);
                Chunks[n][i] = value;
            }
        }

        (int nchunk, int ichunk) Address(int index) => (index / CHUNKSIZE, index % CHUNKSIZE);
    }
}
