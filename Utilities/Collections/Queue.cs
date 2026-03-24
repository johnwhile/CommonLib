using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Common.Tools
{
    /// <summary>
    /// This interface hide the QueueEnumerator and QueueInvertedEnumerator classes
    /// </summary>
    public interface IQueueEnumerator<T> : IEnumerator<T>, IEnumerable<T>
    {
        T this[int index] { get; set; }
        void SetByRef(int index, ref T item);
    }


    /// <summary>
    /// My Queue implementation, is FIFO so item are usualy insert in tail, and out from the head
    /// Optimized for value with reference. It work with struct too, but all values
    /// will be passed as copy.
    /// <para>--------+-----+-----+-----+---+----------</para>
    /// <para> EXIT&lt;--| head | obj2 | obj1 |  tail |&lt;--INSERT</para>
    /// <para>--------+-----+-----+-----+---+----------</para>
    /// </summary>
    [DebuggerDisplay("Count = {m_count} , Size = {capacity}")]
    public class MyQueue<T> : IEnumerable<T>
    {
        bool isStruct;
        protected const int defaultcapacity = 4;

        /// <summary>
        /// Only in DEBUG mode, return the maximum size used. If you compile without DEBUG flag
        /// this check was not computer to increase a little the performance, and it return -1, 
        /// </summary>
        public int MaxStackSizeUsed { get; protected set; }

        internal int capacity = 0;
        internal int count = 0;

        // is the position of first element
        internal int head;
        // is the position of last element
        internal int tail;
        // is the position where new item will be added
        internal int tailnext;
        // circular array, fast for get and set but slow when capacity increase
        internal T[] elements;
        // the element's index
        internal int calcIndex(int index) { return (head + index) % capacity; ; }
        internal int calcInvIndex(int index) { return (tailnext - 1 - index + capacity) % capacity; }



        public MyQueue(int Capacity)
        {
            isStruct = typeof(T).IsValueType;
            capacity = Capacity > defaultcapacity ? Capacity : defaultcapacity;
            elements = new T[Capacity];
            MaxStackSizeUsed = 0;
            count = 0;
            head = 0;
            tailnext = 0;
            ItemList = new QueueEnumerator<T>(this);
            ItemListInverted = new QueueInvertedEnumerator<T>(this);
        }

        /// <summary>
        /// number of item inside
        /// </summary>
        public int Count
        {
            get { return count; }
        }
        /// <summary>
        /// Can increment
        /// </summary>
        public int Capacity
        {
            get { return capacity; }
        }
        /// <summary>
        /// Remove all items but not change capacity of array.
        /// Optimized for struct
        /// </summary>
        public void Clear()
        {
            MaxStackSizeUsed = -1;

            if (!isStruct)
            {
                if (head < tailnext)
                {
                    Array.Clear(elements, head, count);
                }
                else
                {
                    Array.Clear(elements, head, capacity - head);
                    Array.Clear(elements, 0, tailnext);
                }
            }
            count = 0;
            head = 0;
            tailnext = 0;
        }

        /// <summary>
        /// Enqueue: insert item to the tail
        /// </summary>
        public void AddTail(T item)
        {
            // size is full, increase array
            if (count == capacity)
            {
                increase();
            }
            tail = tailnext;
            elements[tail] = item;
            tailnext = (tailnext + 1) % capacity;
            count++;
        }
        /// <summary>
        /// Dequeue: return item from the head and remove it
        /// </summary>
        public T RemoveHead()
        {
            T item = Head;
            // it's necessary remove reference of a class
            if (!isStruct) elements[head] = default(T); 
            head = (head + 1) % capacity;
            count--;
            return item;
        }
        /// <summary>
        /// InverseDequeue: return item from the tail and remove it
        /// </summary>
        public virtual T RemoveTail()
        {
            T item = Tail;
            // it's necessary remove reference of a class
            if (!isStruct) elements[tail] = default(T);

            if (tail == 0)
            {
                tail = capacity - 1;
                tailnext = 0;
            }
            else if (tailnext == 0)
            {
                tail--;
                tailnext = capacity - 1;
            }
            else
            {
                tail--;
                tailnext--;
            }
            //tail = (tail - 1) % capacity;
            count--;
            return item;
        }
        /// <summary>
        /// Peek : return item from the head but not remove it
        /// </summary>
        public T Head
        {
            get
            {
                if (count == 0)
                    throw new Exception("Queue is Empty");
                return elements[head];
            }
        }
        /// <summary>
        /// Return item from the tail but not remove it
        /// </summary>
        public T Tail
        {
            get
            {
                if (count == 0)
                    throw new Exception("Queue is Empty");
                return elements[tail];
            }
        }
        /// <summary>
        /// If a Queue is empty, you can't get Tail or Head
        /// </summary>
        public bool IsEmpty
        {
            get { return count == 0; }
        }

        /// <summary>
        /// Enumerable list of items from head to tail
        /// </summary>
        public IQueueEnumerator<T> ItemList;
        /// <summary>
        /// Enumerable list of items in inverted order
        /// </summary>
        public IQueueEnumerator<T> ItemListInverted;
        /// <summary>
        /// Increase the array size and arrange the data, is a slow function.
        /// capacity will be increase by x2
        /// </summary>
        protected void increase()
        {
            capacity = capacity == 0 ? defaultcapacity : 2 * capacity;

            T[] newArray = new T[capacity];

            if (count > 0)
            {
                if (head < tailnext)
                {
                    Array.Copy(elements, head, newArray, 0, count);
                }
                else
                {
                    Array.Copy(elements, head, newArray, 0, capacity - head);
                    Array.Copy(elements, 0, newArray, capacity - head, tailnext);
                }
                head = 0;
                tailnext = (count == capacity) ? 0 : count;
            }
            elements = newArray;
        }
        /// <summary>
        /// Default enumerator for normal order
        /// </summary>
        class QueueEnumerator<K> : ListEnumerator<K> , IQueueEnumerator<K>
        {
            protected MyQueue<K> queue;
            protected int index;
            protected int counter;

            public QueueEnumerator(MyQueue<K> queue)
            {
                this.queue = queue;
            }
            /// <summary>
            /// Get or Set items using index from head to tail (or from tail to head if inverted).
            /// If is a struct, item is passed as value
            /// </summary>
            public virtual K this[int index]
            {
                get { return queue.elements[queue.calcIndex(index)]; }
                set { queue.elements[queue.calcIndex(index)] = value; }
            }

            public virtual void SetByRef(int index, ref K item)
            {
                queue.elements[queue.calcIndex(index)] = item;
            }

            public override bool MoveNext()
            {
                index = (index + 1) % queue.capacity;
                counter++;
                current = queue.elements[index];
                return counter <= queue.count;
            }

            public override void Reset()
            {
                index = queue.head - 1;
                counter = 0;
            }
        }
        /// <summary>
        /// Optional enumerator for inverted order
        /// </summary>
        class QueueInvertedEnumerator<K> : QueueEnumerator<K> , IQueueEnumerator<K>
        {
            public QueueInvertedEnumerator(MyQueue<K> queue)
                : base(queue)
            { }

            /// <summary>
            /// Get or Set items using index from head to tail (or from tail to head if inverted).
            /// If is a struct, item is passed as value
            /// </summary>
            public override K this[int index]
            {
                get { return queue.elements[queue.calcInvIndex(index)]; }
                set { queue.elements[queue.calcInvIndex(index)] = value; }
            }

            public override void SetByRef(int index, ref K item)
            {
                queue.elements[queue.calcInvIndex(index)] = item;
            }

            public override bool MoveNext()
            {
                index = (index - 1 + queue.capacity) % queue.capacity;
                counter++;
                current = queue.elements[index];
                return counter <= queue.count;
            }
            public override void Reset()
            {
                index = queue.tailnext;
                counter = 0;
            }
        }
            
        public IEnumerator<T> GetEnumerator()
        {
            if (count == 0) yield break;

            int i = head;
            do
            {
                yield return elements[i];
                i = (i + 1) % capacity;
            }
            while (i != tailnext);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
