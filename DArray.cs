using System;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Common
{
    public abstract class ArrayBase
    {
    }


    /// <summary>
    /// Semplified version of List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class DArray<T> : ArrayBase , IEnumerable<T>, ICollection<T>, IList<T>
    {
        Comparer comparer;

        protected int m_count;
        protected T[] m_items;

        protected DArray()
        {
            comparer = new Comparer();
        }

        public DArray(int capacity = 0) : this()
        {
            m_items = new T[capacity];
            m_count = 0;
        }
        public DArray(T[] array) : this()
        {
            m_items = array;
            m_count = array.Length;
        }

        public DArray(IEnumerable<T> enumerable) : this()
        {
            int count = enumerable?.Count() ?? 0;
            m_items = new T[count];
            m_count = count;

            if (enumerable is ICollection<T> collection)
                collection.CopyTo(m_items, 0);
            else
                using (var enumerator = enumerable.GetEnumerator())
                    while (enumerator.MoveNext())
                        Add(enumerator.Current);
        }

        public T this[int index]
        { 
            get => m_items[index]; 
            set => m_items[index] = value;
        }

        /// <summary>
        /// How many elements are in the List
        /// </summary>
        public int Count => m_count;

        /// <summary>
        /// The capacity is the size of the internal array used to hold items. When set, the internal
        /// array of the list is reallocated to the given capacity.
        /// </summary>
        public virtual int Capacity
        {
            get => m_items.Length;
            set
            {
                if (value != m_items.Length)
                {
                    if (value > 0)
                    {
                        T[] tmp = new T[value];
                        Array.Copy(m_items, 0, tmp, 0, m_count);
                        m_items = tmp;
                        m_count = Math.Min(m_count, value);
                    }
                    else
                    {
                        m_items = new T[0];
                        m_count = 0;
                    }
                }
            }
        }

        public bool IsReadOnly => false;
        
        public virtual void Add(T item)
        {
            if (m_count == Capacity) EnsureCapacity(Capacity + 1);
            m_items[m_count++] = item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceIndex">start index of <paramref name="source"/> to copy from</param>
        /// <param name="length">number of element of <paramref name="source"/> to copy</param>
        public virtual void AddRange(IEnumerable<T> source, int sourceIndex=0, int length=-1)
        {
            int count = source?.Count() ?? 0;
            if (count == 0) return;

            if (length < 0) length = count; 
            length = Math.Min(count, length);

            EnsureCapacity(m_count + length);

            if (sourceIndex==0 && length == count && source is ICollection<T> collection)
            {
                collection.CopyTo(m_items, m_count);
                m_count += length;
            }
            else
            {
               
                using (var enumerator = source.GetEnumerator())
                {
                    int i = 0;
                    while (enumerator.MoveNext())
                    {
                        if (i>=sourceIndex && i <(sourceIndex + length))
                            Add(enumerator.Current);
                        i++;
                    }
                }
            }
        }

        /// <summary><inheritdoc/></summary>
        /// <remarks>
        /// Clear the elements so that the gc can reclaim the references.
        /// </remarks>
        public virtual void Clear()
        {
            if (Capacity > 0)
            {
                //if (!typeof(T).IsUnmanaged())
                    Array.Clear(m_items, 0, Capacity);
                m_count = 0;
            }
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < m_count; i++)
                if (item.Equals(m_items[i])) return true;
            return false;
        }

        public virtual void CopyTo(T[] array, int destIndex)=>
            CopyTo(array, destIndex, 0, m_count);
        
        public virtual void CopyTo(T[] array)=>
            CopyTo(array, 0, 0, m_count);

        public virtual void CopyTo(T[] array, int destIndex, int srcIndex, int length) =>
            Array.Copy(m_items, srcIndex, array, destIndex, length);
        

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < m_count; i++) yield return m_items[i];
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(m_items, item, 0, m_count);
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }


        public void Sort(IComparer comparer = null, int startindex = 0, int length = -1)
        {
            if (length < 0) length = m_count;
            Array.Sort(m_items, startindex, length, comparer);
        }
        /// <summary>
        /// Use lambda expressione as comparer.<br/>
        /// to order from smaller to greater:
        /// <code>(x, y) => x.CompareTo(y)</code>
        /// to order from greater to smaller:
        /// <code>(x, y) => y.CompareTo(x)</code> 
        /// </summary>
        /// <param name="comparison"></param>
        /// <param name="startindex"></param>
        /// <param name="length"></param>
        public void Sort(Comparison<T> comparison, int startindex = 0, int length = -1)
        {
            if (length < 0) length = m_count;

            if (comparison != null)
            {
                comparer.function = comparison;
                Array.Sort(m_items, startindex, length, comparer);
            }
        }

        class Comparer : IComparer<T>
        {
            public Comparison<T> function;
            public int Compare(T x, T y)=> function(x, y);
        }

        /// <summary>
        /// Ensures that the capacity of this list is at least the given minimum value.
        /// </summary>
        public void EnsureCapacity(int min)
        {
            if (m_items.Length < min)
            {
                int newCapacity = m_items.Length == 0 ? 4 : (int)(m_items.Length * 1.5);
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }


        public static implicit operator T[](DArray<T> dynamic)=> dynamic.m_items;
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
