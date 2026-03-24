using System;
using System.Collections;
using System.Collections.Generic;

namespace Common.Tools
{
    /// <summary>
    /// My Stack implementation, is LIFO
    /// </summary>
    public class MyStack<T> : IEnumerable<T>
    {
        const int defaultcapacity = 4;

        int capacity = 0;
        int count = 0;
        T[] elements;

        public MyStack(int Capacity)
        {
            capacity = Capacity > defaultcapacity ? Capacity : defaultcapacity;
            elements = new T[Capacity];
            MaxStackSizeUsed = 0;
            count = 0;
        }

        /// <summary>
        /// Only in DEBUG mode, return the maximum size used. If you compile without DEBUG flag
        /// this check was not computed to increase a little the performance, and it return -1, 
        /// </summary>
        public int MaxStackSizeUsed { get; private set; }

        /// <summary>
        /// Return the size of internal array, if you set a new value this invalidate the array so check
        /// always if is necessary change size
        /// </summary>
        public int Capacity
        {
            get { return capacity; }
            set
            {
                if (count > value) throw new ArgumentOutOfRangeException("to many m_items to copy, missing Clear() ?");
                capacity = value;
                T[] newArray = new T[capacity];
                Array.Copy(elements, 0, newArray, 0, count);
                elements = newArray;
            }
        }
        /// <summary>
        /// Number of elements stored
        /// </summary>
        public int Count
        {
            get { return count; }
        }
        /// <summary>
        /// Remove all items
        /// </summary>
        public void Clear()
        {
            MaxStackSizeUsed = -1;
            Array.Clear(elements, 0, count);
            count = 0;
        }
        /// <summary>
        /// Return topmost but not remove it
        /// </summary>
        public T Peek()
        {
            if (count == 0)
                throw new Exception("Stack is Empty");
            return elements[count - 1];
        }
        /// <summary>
        /// Return topmost and remove it
        /// </summary>
        public T Pop()
        {
            T item = Peek();
            elements[--count] = default(T); // is very important free the class or struct
            return item;
        }
        /// <summary>
        /// Insert item to Last position 
        /// </summary>
        public void Push(T item)
        {
            /* If the stack is full, we cannot push an element into it as there is no space for it.*/
            if (count == capacity)
            {
                capacity = capacity == 0 ? defaultcapacity : 2 * capacity;
                T[] newArray = new T[capacity];
                Array.Copy(elements, 0, newArray, 0, count);
                elements = newArray;
            }

            /* Push an element on the top of it and increase its size by one*/
            elements[count++] = item;
#if DEBUG
            if (MaxStackSizeUsed < count) MaxStackSizeUsed = count;
#endif
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++) yield return elements[i];
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }

}
