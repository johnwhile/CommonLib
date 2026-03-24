using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Common.Tools
{
    /// <summary>
    /// A tool to write managed/unmanaged source to unmanaged buffers (COM like directx).
    /// For a better performance there aren't implemented Exeptions
    /// </summary>
    public static class MemoryTool
    {
        #region Generic array
        /// <summary>
        /// Copy the source array in destination array, A and B must be a struct.
        /// If destination size is smaller than required generate and exeption
        /// </summary>
        /// <param name="SourceCount">num of elements to copy</param>
        /// <param name="Source">source array to copy</param>
        /// <param name="Destination">destination array already intialized</param>
        public static void WriteStruct<A, B>(A[] Source, B[] Destination, int SourceCount)
            where A : struct
            where B : struct
        {
            GCHandle srchandle = GCHandle.Alloc(Source, GCHandleType.Pinned);
            GCHandle dsthandle = GCHandle.Alloc(Destination, GCHandleType.Pinned);

            int bytesrc = Marshal.SizeOf(typeof(A)) * SourceCount;
            int bytedst = Marshal.SizeOf(typeof(B)) * Destination.Length;

            if (bytesrc > bytedst) throw new ArgumentOutOfRangeException("m_buffer to copy surpass the destination Size");

            IntPtr sourceptr = srchandle.AddrOfPinnedObject();
            IntPtr destptr = dsthandle.AddrOfPinnedObject();

            WriteStruct(destptr, bytedst, 0, sourceptr, bytesrc, 0);

            srchandle.Free();
            dsthandle.Free();
        }
        #endregion

        #region Fragment Vertex attributes
        /// <summary>
        /// Write fragmented array to buffer
        /// </summary>
        /// <remarks>
        /// bufferOffset is necessary for arithmetic pointer, IntPtr return the pointer are the beginning of buffer
        /// <code>
        ///  _ _ _ _ _ _ _ _ _ ___________________________________ _ _ _ _ _ _ _ _ _
        ///    bufferOffset   | StartSize |  ItemSize  | EndSize |                                 
        ///  _ _ _ _ _ _ _ _ _|___________|____________|_________|_ _ _ _ _ _ _ _ _ _
        /// </code>
        /// </remarks>
        /// <param name="buffer">destination buffer</param>
        /// <param name="bufferSize">size in bytes of destination buffer</param>
        /// <param name="bufferOffset">offset in bytes where write beginning in destination buffer</param>
        /// <param name="source">source buffer to write</param>
        /// <param name="sourceSize">size in bytes of source buffer</param>
        /// <param name="sourceOffset">offset in bytes where read beginning in source buffer</param>
        /// <param name="startBytes">bytes to jump before write</param>
        /// <param name="sourceItem">size in bytes of elements, can be a multiple of source's element size</param>
        /// <param name="endBytes">bytes to jump after write</param>
        public static void WriteFragmentStruct(
            IntPtr buffer, int bufferSize, int bufferOffset,
            IntPtr source, int sourceSize, int sourceOffset,
            int startBytes, int sourceItem, int endBytes)
        {
            if (sourceSize % sourceItem != 0)
                throw new ArgumentException("incorrect multiple value for a singular element, Example : when you have a float Items and element is a vector2 the operation is allowed");

            // Optimize the bus, i notice a good improvement
            unsafe
            {
                // pack bus with 64bit (8bytes)
                if (sourceOffset % 8 == 0 && bufferOffset % 8 == 0 && startBytes % 8 == 0 && sourceItem % 8 == 0 && endBytes % 8 == 0)
                {
                    sourceOffset /= 8;
                    bufferOffset /= 8;
                    sourceSize /= 8; // suppose sourcesize is computed with source_count * source_type_size
                    startBytes /= 8;
                    sourceItem /= 8;
                    endBytes /= 8;
                    UInt64* sourcePtr = (UInt64*)source.ToPointer();
                    UInt64* bufferPtr = (UInt64*)buffer.ToPointer();
                    int i = sourceOffset;
                    int j = bufferOffset;
                    int ii = 0;
                    while (i < sourceSize)
                    {
                        j += startBytes;
                        for (ii = 0; ii < sourceItem; ii++, j++, i++) bufferPtr[j] = sourcePtr[i];
                        j += endBytes;
                    }
                }
                // pack bus with 32bit (4bytes)
                else if (sourceOffset % 4 == 0 && bufferOffset % 4 == 0 && startBytes % 4 == 0 && sourceItem % 4 == 0 && endBytes % 4 == 0)
                {
                    sourceOffset /= 4;
                    bufferOffset /= 4;
                    sourceSize /= 4;
                    startBytes /= 4;
                    sourceItem /= 4;
                    endBytes /= 4;
                    UInt32* sourcePtr = (UInt32*)source.ToPointer();
                    UInt32* bufferPtr = (UInt32*)buffer.ToPointer();
                    int i = sourceOffset;
                    int j = bufferOffset;
                    int ii = 0;
                    while (i < sourceSize)
                    {
                        j += startBytes;
                        for (ii = 0; ii < sourceItem; ii++, j++, i++) bufferPtr[j] = sourcePtr[i];
                        j += endBytes;
                    }
                }
                // pack bus with 16bit (2bytes)
                else if (sourceOffset % 2 == 0 && bufferOffset % 2 == 0 && startBytes % 2 == 0 && sourceItem % 2 == 0 && endBytes % 2 == 0)
                {
                    sourceOffset /= 2;
                    bufferOffset /= 2;
                    sourceSize /= 2;
                    startBytes /= 2;
                    sourceItem /= 2;
                    endBytes /= 2;
                    UInt16* sourcePtr = (UInt16*)source.ToPointer();
                    UInt16* bufferPtr = (UInt16*)buffer.ToPointer();
                    int i = sourceOffset;
                    int j = bufferOffset;
                    int ii = 0;
                    while (i < sourceSize)
                    {
                        j += startBytes;
                        for (ii = 0; ii < sourceItem; ii++, j++, i++) bufferPtr[j] = sourcePtr[i];
                        j += endBytes;
                    }
                }
                // pack bus with 8bit (1byte)
                else
                {
                    byte* sourcePtr = (byte*)source.ToPointer();
                    byte* bufferPtr = (byte*)buffer.ToPointer();
                    int i = sourceOffset;
                    int j = bufferOffset;
                    int ii = 0;
                    while (i < sourceSize)
                    {
                        j += startBytes;
                        for (ii = 0; ii < sourceItem; ii++, j++, i++) bufferPtr[j] = sourcePtr[i];
                        j += endBytes;
                    }
                }
            }
        }

        /// <summary>
        /// <seealso cref="WriteFragmentStruct"/>
        /// </summary>
        /// <param name="sourceCount">number of source's items from 0 (sourceCount = Offset + Count)</param>
        /// <param name="sourceOffset">number of source's items to jump before start writting</param>
        /// <param name="sourceItem">size in bytes of element in which the the source's items are written, must be a multiple of T</param>
        public static void WriteFragmentStructByStruct<T>(
            IntPtr buffer, int bufferSize, int bufferOffset,
            IList<T> source, int sourceCount, int sourceOffset,
            int startBytes, int sourceItem, int endBytes)
            where T : struct
        {
            int sizeofvalue = Marshal.SizeOf(typeof(T));
            if (sizeofvalue > sourceItem)
                throw new NotSupportedException("can't write a struct bigger than targhet space in m_buffer, is supported only if targhet space is a multiple of struct");
            if (sourceItem % sizeofvalue != 0)
                throw new ArgumentException("incorrect multiple value for a singular element, example : when you have a float Items and element is a vector2 the operation is allowed");
            int numofvalues = sourceItem / sizeofvalue;
            if (numofvalues <= 0 || numofvalues > 4) throw new NotFiniteNumberException("something wrong when get number of multiple-struct in each element");

            unsafe
            {
                // allock an unmanaged portion to store the managed T elements
                IntPtr Tptr = Marshal.AllocHGlobal(sourceItem);

                byte* destPtr = (byte*)buffer.ToPointer();
                int i = bufferOffset;
                int index = sourceOffset;

                while (index < sourceCount)
                {
                    i += startBytes;

                    for (int n = 0; n < numofvalues; n++)
                    {
                        T element = source[index];
                        Marshal.StructureToPtr(element, Tptr, true);
                        byte* elemPtr = (byte*)Tptr.ToPointer();
                        for (int j = 0; j < sizeofvalue; j++, i++) destPtr[i] = elemPtr[j];
                        index++;
                    }

                    i += endBytes;
                }

                Marshal.FreeHGlobal(Tptr);
            }
        }

        #endregion

        #region Continued Vertex attributes
        /// <summary>
        /// Write simple array to buffer
        /// </summary>
        /// <remarks>
        /// bufferSize is whole buffer size, from offset = 0 to last byte
        /// sourceSize is the size of source from offset = 0 to last item to write
        /// bufferOffset is necessary for arithmetic pointer, IntPtr return the pointer are the beginning of buffer
        ///  _ _ _ _ _ _ _ _ ___________________________________ _ _ _ _ _ _ 
        ///    bufferOffset   |  sourceOffset |  writesize     |                          
        ///  _ _ _ _ _ _ _ _ _|___________sourcesize___________|_ _ _ _ _ _ _ 
        /// 
        /// </remarks>
        /// <param name="dest">destination</param>
        /// <param name="destSize">bytes of whole destination buffer</param>
        /// <param name="destOffset">bytes where write beginning in destination buffer</param>
        /// <param name="source">data to write</param>
        /// <param name="sourceSize">bytes of whole source buffer, (from offset = 0 to source count)</param>
        /// <param name="sourceOffset">bytes where read beginning in source buffer</param>
        public static void WriteStruct(
            IntPtr dest, int destSize, int destOffset,
            IntPtr source, int sourceSize, int sourceOffset)
        {
            //if (sourceSize - sourceOffset > destSize - destOffset) throw new ArgumentOutOfRangeException("Buffer to copy surpass the destination size");

            // Optimize the bus, i notice a good improvement
            unsafe
            {
                // pack bus with 64bit (8bytes)
                if (sourceOffset % 8 == 0 && sourceSize % 8 == 0 && destOffset % 8 == 0)
                {
                    sourceOffset /= 8;
                    destOffset /= 8;
                    sourceSize /= 8;
                    UInt64* sourcePtr = (UInt64*)source.ToPointer();
                    UInt64* bufferPtr = (UInt64*)dest.ToPointer();
                    for (int i = sourceOffset, j = destOffset; i < sourceSize; i++, j++) bufferPtr[j] = sourcePtr[i];
                }
                // pack bus with 32bit (4bytes)
                else if (sourceOffset % 4 == 0 && sourceSize % 4 == 0 && destOffset % 4 == 0)
                {
                    sourceOffset /= 4;
                    destOffset /= 4;
                    sourceSize /= 4;
                    UInt32* sourcePtr = (UInt32*)source.ToPointer();
                    UInt32* bufferPtr = (UInt32*)dest.ToPointer();
                    for (int i = sourceOffset, j = destOffset; i < sourceSize; i++, j++) bufferPtr[j] = sourcePtr[i];
                }
                // pack bus with 16bit (2bytes)
                else if (sourceOffset % 2 == 0 && sourceSize % 2 == 0 && destOffset % 2 == 0)
                {
                    sourceOffset /= 2;
                    destOffset /= 2;
                    sourceSize /= 2;
                    UInt16* sourcePtr = (UInt16*)source.ToPointer();
                    UInt16* bufferPtr = (UInt16*)dest.ToPointer();
                    for (int i = sourceOffset, j = destOffset; i < sourceSize; i++, j++) bufferPtr[j] = sourcePtr[i];
                }
                // pack bus with 8bit (1byte)
                else
                {
                    byte* sourcePtr = (byte*)source.ToPointer();
                    byte* bufferPtr = (byte*)dest.ToPointer();
                    for (int i = sourceOffset, j = destOffset; i < sourceSize; i++, j++) bufferPtr[j] = sourcePtr[i];
                }
            }
        }
        /// <summary>
        /// Write simple list to buffer
        /// </summary>
        /// <remarks>
        /// bufferOffset is necessary for arithmetic pointer, IntPtr return the pointer are the beginning of buffer
        /// </remarks>
        /// <param name="buffer">destination</param>
        /// <param name="bufferSize">bytes of whole destination buffer</param>
        /// <param name="bufferOffset">bytes where write beginning in destination buffer</param>
        /// <param name="source">list to write</param>
        /// <param name="sourceSizeCount">number of whole list's elements, NOT IN BYTES</param>
        /// <param name="sourceOffsetCount">number of list's elements where read beginning, NOT IN BYTES</param>
        public static void WriteStructByStruct<T>(
            IntPtr buffer, int bufferSize, int bufferOffset,
            IList<T> source, int sourceSizeCount, int sourceOffsetCount)
            where T : struct
        {
            int sizeofT = Marshal.SizeOf(typeof(T));
            int sourceSize = sizeofT * source.Count;

            if (sourceSize < 1) throw new ArgumentException("you pass a empty list");

            unsafe
            {
                // allock an unmanaged portion to store the managed T elements
                IntPtr Tptr = Marshal.AllocHGlobal(sizeofT);

                // pack bus of 64bit
                if (sizeofT % 8 == 0 && bufferOffset % 8 == 0 && bufferSize % 8 == 0)
                {
                    sourceSize /= 8;
                    sizeofT /= 8;
                    bufferOffset /= 8;
                    bufferSize /= 8;
                    UInt64* destPtr = (UInt64*)buffer.ToPointer();
                    int i = bufferOffset;

                    for (int index = sourceOffsetCount; index < sourceSizeCount; index++)
                    {
                        T element = source[index];
                        Marshal.StructureToPtr(element, Tptr, true);
                        UInt64* elemPtr = (UInt64*)Tptr.ToPointer();
                        for (int j = 0; j < sizeofT; j++, i++) destPtr[i] = elemPtr[j];
                    }

                }
                // pack bus of 32bit
                else if (sizeofT % 4 == 0 && bufferOffset % 4 == 0 && bufferSize % 4 == 0)
                {
                    sourceSize /= 4;
                    sizeofT /= 4;
                    bufferOffset /= 4;
                    bufferSize /= 4;
                    UInt32* destPtr = (UInt32*)buffer.ToPointer();
                    int i = bufferOffset;

                    for (int index = sourceOffsetCount; index < sourceSizeCount; index++)
                    {
                        T element = source[index];
                        Marshal.StructureToPtr(element, Tptr, true);
                        UInt32* elemPtr = (UInt32*)Tptr.ToPointer();
                        for (int j = 0; j < sizeofT; j++, i++) destPtr[i] = elemPtr[j];
                    }

                }
                // pack bus of 16bit
                else if (sizeofT % 2 == 0 && bufferOffset % 2 == 0 && bufferSize % 2 == 0)
                {
                    sourceSize /= 2;
                    sizeofT /= 2;
                    bufferOffset /= 2;
                    bufferSize /= 2;
                    UInt16* destPtr = (UInt16*)buffer.ToPointer();
                    int i = bufferOffset;

                    for (int index = sourceOffsetCount; index < sourceSizeCount; index++)
                    {
                        T element = source[index];
                        Marshal.StructureToPtr(element, Tptr, true);
                        UInt16* elemPtr = (UInt16*)Tptr.ToPointer();
                        for (int j = 0; j < sizeofT; j++, i++) destPtr[i] = elemPtr[j];
                    }

                }
                // pack bus of 8bit
                else
                {
                    byte* destPtr = (byte*)buffer.ToPointer();
                    int i = bufferOffset;
                    for (int index = sourceOffsetCount; index < sourceSizeCount; index++)
                    {
                        T element = source[index];
                        Marshal.StructureToPtr(element, Tptr, true);
                        byte* elemPtr = (byte*)Tptr.ToPointer();
                        for (int j = 0; j < sizeofT; j++, i++) destPtr[i] = elemPtr[j];
                    }
                }
                Marshal.FreeHGlobal(Tptr);
            }
        }

        #endregion

        #region Continued Index attributes
        /// <summary>
        /// Can copy 16bit index to a 32bit index, but not viceversa
        /// </summary>
        /// <param name="buffer">destination buffer</param>
        /// <param name="bufferFormat">buffer element format</param>
        /// <param name="bufferSize">bytes of destination buffer</param>
        /// <param name="bufferOffset">bytes of destination buffer where write beginning</param>
        /// <param name="source">source to write</param>
        /// <param name="sourceFormat">source elements format</param>
        /// <param name="sourceSize">bytes of source buffer, can be &lt; whole size if you want write only some elements</param>
        /// <param name="sourceOffset">bytes of source buffer where copy beginning</param>
        /// <param name="IndexOffset">a value to sum to each buffer's elements, can be negative, usefull for batch algorithm.</param>
        /// <remarks>
        /// IndexOffset can be negative, but is your care check if generate a wrong cast conversion
        /// Directx use int as bytes counter [−2,147,483,648 : 2,147,483,647] , 2GB of size is enought for all type of resources
        /// _ _ _ _________________________________________________________________ _ _ _
        ///   offsetBuffer   |  sourceFormat.bytesize ? bufferFormat.bytesize   |
        /// _ _ _ ___________|________________________?_________________________|__ _ _ _
        /// </remarks>
        public static void WriteIndex(
            IntPtr buffer, bool buffer32Bit, int bufferSize, int bufferOffset,
            IntPtr source, bool source32Bit, int sourceNumOfindis, int sourceSize, int sourceOffset,
            int IndexOffset)
        {
            int sourceBytesize = source32Bit ? 4 : 2;

            // number of source's indices (singular index, ushort or uint value)
            int count = (sourceSize - sourceOffset) / sourceBytesize * sourceNumOfindis;

            unsafe
            {
                uint Ioffset = (uint)IndexOffset;
                // Both use 32bit array
                if (buffer32Bit && source32Bit)
                {
                    // pointer arithmetic is in bytes, so buffer offset must be recalculate for new size;
                    uint* src = (uint*)source.ToPointer() + sourceOffset / sizeof(uint);
                    uint* dest = (uint*)buffer.ToPointer() + bufferOffset / sizeof(uint);
                    // The unchecked don't take care about maximum index value
                    unchecked
                    {
                        for (int i = 0, j = 0; j < count; i++, j++)
                            dest[j] = src[i] + Ioffset;
                    }
                }
                // write 16bit array to 32bit array
                else if (buffer32Bit && !source32Bit)
                {
                    ushort* src = (ushort*)source.ToPointer() + sourceOffset / sizeof(ushort);
                    uint* dest = (uint*)buffer.ToPointer() + bufferOffset / sizeof(uint);
                    unchecked
                    {
                        for (int i = 0, j = 0; j < count; i++, j++)
                            dest[j] = src[i] + Ioffset;
                    }
                }
                // Both use 16bit array
                else if (!buffer32Bit && !source32Bit)
                {
                    ushort* src = (ushort*)source.ToPointer() + sourceOffset / sizeof(ushort);
                    ushort* dest = (ushort*)buffer.ToPointer() + bufferOffset / sizeof(ushort);
                    unchecked
                    {
                        // the sum always return uint, so the cast can be done at end of operation
                        for (int i = 0, j = 0; j < count; i++, j++)
                            dest[j] = (ushort)(src[i] + Ioffset);
                    }
                }
                // write 32bit array to 16bit array is possible but don't have sense, you can avoid it using a unsigned short 
                // index buffer only for 16bit geometries
                else
                {
                    throw new InvalidCastException("Casting a uint (32bit) index to ushort (16bit) can be wrong if interger is >= ushort.MaxValue");
                    /*
                    uint* src = (uint*)source.ToPointer() + sourceOffset / sizeof(uint);
                    ushort* dest = (ushort*)buffer.ToPointer() + bufferOffset / sizeof(ushort);
                    unchecked
                    {
                        for (int i = 0, j = 0; j < count; i++, j++)
                            dest[j] = (ushort)(src[i] + Ioffset); 
                    }
                    */
                }
            }
        }

        /// <summary>
        /// Can copy 16bit index to a 32bit index, but not viceversa
        /// </summary>
        /// <param name="sourceSize">number of list's elements from offset = 0</param>
        /// <param name="sourceOffset">number of list's element where copy beginning</param>
        public static void WriteIndexByIndex<T>(
            IntPtr buffer, bool buffer32Bit, int bufferSize, int bufferOffset,
            IList<T> source, bool source32Bit, int sourceNumOfindis, int sourceSize, int sourceOffset,
            int IndexOffset)
            where T : struct
        {
            int sourceBytesize = source32Bit ? 4 : 2;

            unsafe
            {
                // allock an unmanaged portion to store the managed T elements
                IntPtr Tptr = Marshal.AllocHGlobal(sourceBytesize * sourceNumOfindis);
                uint Ioffset = (uint)IndexOffset;
                int i = 0;

                // Both use 32bit array
                if (buffer32Bit && source32Bit)
                {
                    // pointer arithmetic is in bytes, so buffer offset must be recalculate for new size;
                    uint* dest = (uint*)buffer.ToPointer() + bufferOffset / sizeof(uint);
                    for (int index = sourceOffset; index < sourceSize; index++)
                    {
                        Marshal.StructureToPtr(source[index], Tptr, true);
                        uint* src = (uint*)Tptr.ToPointer();
                        // The unchecked don't take care about maximum index value
                        unchecked
                        {
                            for (int j = 0; j < sourceNumOfindis; j++, i++)
                                dest[i] = src[j] + Ioffset;
                        }
                    }
                }
                // write 16bit array to 32bit array
                else if (buffer32Bit && !source32Bit)
                {
                    uint* dest = (uint*)buffer.ToPointer() + bufferOffset / sizeof(uint);
                    for (int index = sourceOffset; index < sourceSize; index++)
                    {
                        Marshal.StructureToPtr(source[index], Tptr, true);
                        ushort* src = (ushort*)Tptr.ToPointer();
                        unchecked
                        {
                            for (int j = 0; j < sourceNumOfindis; j++, i++)
                                dest[i] = src[j] + Ioffset;
                        }
                    }
                }
                // Both use 16bit array
                else if (!buffer32Bit && !source32Bit)
                {
                    ushort* dest = (ushort*)buffer.ToPointer() + bufferOffset / sizeof(ushort);
                    for (int index = sourceOffset; index < sourceSize; index++)
                    {
                        Marshal.StructureToPtr(source[index], Tptr, true);
                        ushort* src = (ushort*)Tptr.ToPointer();
                        unchecked
                        {
                            for (int j = 0; j < sourceNumOfindis; j++, i++)
                                dest[i] = (ushort)(src[j] + Ioffset);
                        }
                    }
                }
                // write 32bit array to 16bit array is possible but don't have sense, you can avoid it using a unsigned short 
                // index buffer only for 16bit geometries
                else
                {
                    throw new InvalidCastException("casting x" + source32Bit + " index to x" + buffer32Bit + " index can be wrong");
                    /*
                    ushort* dest = (ushort*)buffer.ToPointer() + bufferOffset / sizeof(ushort);
                    for (int index = sourceOffset; index < sourceSize; index++)
                    {
                        Marshal.StructureToPtr(source[index], Tptr, true);
                        uint* src = (uint*)Tptr.ToPointer();
                        unchecked
                        {
                            for (int j = 0; j < sourceFormat.numOfIndis; j++, i++)
                                dest[i] = (ushort)(src[j] + Ioffset);
                        }
                    }
                    */
                }
                Marshal.FreeHGlobal(Tptr);
            }
        }
        #endregion
    }


}
