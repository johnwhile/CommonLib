using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Common
{
    /// <summary>
    /// A class to dispose <see cref="IDisposable"/> instances AND allocated unmanaged memory.
    /// From SharpDx.Toolkit
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class DisposableCollector : IDisposable
    {
        List<object> disposables;
        //MyList<object> disposables;

        /// <summary>
        /// Gets the number of elements to dispose.
        /// </summary>
        public int Count => disposables == null ? 0 : disposables.Count;

        /// <summary>
        /// Disposes all object collected by this class and clear the list. The collector can still be used for collecting.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// Disposes all object collected by this class and clear the list. The collector can still be used for collecting.
        /// </summary>
        /// <param name="disposeManaged">If true, managed resources should be disposed of in addition to unmanaged resources.</param>
        /// <remarks>
        /// To completely dispose this instance and avoid further dispose, use <see cref="Dispose"/> method instead.
        /// </remarks>
        public void Dispose(bool disposeManaged = true)
        {
            foreach(var obj in disposables)
            {
                if (obj is IDisposable disposable)
                    if (disposeManaged) disposable.Dispose();
                else
                    Tools.MemoryHelp.FreeMemory((IntPtr)obj);
            }
            disposables = null;
        }


        /// <summary>
        /// Adds a <see cref="IDisposable"/> object or a <see cref="IntPtr"/> allocated using <see cref="Utilities.AllocateMemory"/> to the list of the objects to dispose.
        /// </summary>
        /// <exception cref="ArgumentException">If toDispose argument is not IDisposable or a valid memory pointer allocated by <see cref="Utilities.AllocateMemory"/></exception>
        public T Collect<T>(T toDispose)
        {
#if DEBUG
            if (!(toDispose is IDisposable || toDispose is IntPtr))
                throw new ArgumentException("Argument must be IDisposable or IntPtr");
#endif

            // Check memory alignment
            if (toDispose is IntPtr memoryPtr)
            {
                if (!Tools.MemoryHelp.IsMemoryAligned(memoryPtr))
                    throw new ArgumentException("Memory pointer is invalid. Memory must have been allocated with Utilties.AllocateMemory");
            }

            if (!Equals(toDispose, default(T)))
            {
                if (disposables == null) disposables = new List<object>();
                if (!disposables.Contains(toDispose)) disposables.Add(toDispose);
            }
            
            return toDispose;
        }

        /// <summary>
        /// Dispose a disposable object and set the reference to null. Removes this object from disposable list
        /// </summary>
        public void RemoveAndDispose<T>(ref T obj)
        {
            if (disposables != null)
            {
                disposables.Remove(obj);
                if (obj is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                else if (obj is IntPtr pointer)
                {
                    Tools.MemoryHelp.FreeMemory(pointer);
                }
                else
                {
                    throw new ArgumentException("obj must be a IntPtr or IDisposable");
                }
            }
            obj = default(T);
        }

        /// <summary>
        /// Removes a disposable object to the list of the objects to dispose.
        /// </summary>
        public void RemoveToDispose(object obj)
        {
            disposables?.Remove(obj);
        }
        public bool Contains(object obj)
        {
            return disposables != null ? disposables.Contains(obj) : false;
        }
    }
}
