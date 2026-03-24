using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.Tools
{
    /// <summary>
    /// TODO:
    /// Generic circular list, the generic parameter T without specification require a internal dictionary to store
    /// the reference of linked node, so it's a little slow
    /// </summary>
    [DebuggerDisplay("Count = {m_count}")]
    public class CircularLinkedList<T> : IEnumerable<T>, IEnumerator<T>
    {
        LinkNode head = null;
        LinkNode current = null;
        Dictionary<T, LinkNode> references;
        int count = 0;

        /// <summary>
        /// Capacity are used for internal dictionary
        /// </summary>
        public CircularLinkedList(int capacity = 0)
        {
            head = null;
            references = new Dictionary<T, LinkNode>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularLinkedList{T}"/> class using existing collection
        /// </summary>
        public CircularLinkedList(IEnumerable<T> collection)
        {
            head = null;
            references = new Dictionary<T, LinkNode>();
            foreach (T item in collection)
                this.Add(item);
        }

        /// <summary>
        /// Get number of elements in the list
        /// </summary>
        public int Count
        {
            get { return count; }
        }
        /// <summary>
        /// Get or Set the first element in the circular link.
        /// </summary>
        /// <value>The head.</value>
        /// <exception cref="System.ArgumentException">This value  + value.ToString() +  isn't in the this list</exception>
        public T Head
        {
            get
            {
                return head.item;
            }
            set
            {
                LinkNode node;
                if (!references.TryGetValue(value, out node)) throw new ArgumentException("This value " + value.ToString() + " isn't in the this list");
                head = node;
            }
        }
        /// <summary>
        /// Add node before head
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(T item)
        {
            if (head == null)
            {
                head = new LinkNode(item);
                references.Add(item, head);
                head.Next = head.Prev = head;
            }
            // example   +- N0 - N1 - N2 -+     where N0 == head and you add N3  
            //           |________________|
            else
            {
                LinkNode node = new LinkNode(item);
                references.Add(item, node);

                //get temp value;
                LinkNode n0 = head;
                LinkNode n2 = head.Prev;

                //set n3 link
                node.Next = n0;
                node.Prev = n2;

                //update old link
                n2.Next = node;
                n0.Prev = node;
            }
            count++;
        }
        /// <summary>
        /// Remove the node
        /// </summary>
        /// <param name="item">The item.</param>
        public void Remove(T item)
        {
        }
        /// <summary>
        /// Inserts "item" before "position"
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="position">The position.</param>
        /// <exception cref="System.ArgumentException">This value  + position.ToString() +   isn't in the this list</exception>
        public void InsertBefore(T item, T position)
        {
            LinkNode node = new LinkNode(item);
            references.Add(item, node);

            LinkNode pos;
            if (!references.TryGetValue(position, out pos)) throw new ArgumentException("This value " + position.ToString() + "  isn't in the this list");

            LinkNode prev = pos.Prev;

            prev.Next = node;
            pos.Prev = node;

            node.Prev = prev;
            node.Next = pos;

        }
        /// <summary>
        /// Inserts "item" after "position"
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="position">The position.</param>
        /// <exception cref="System.ArgumentException">This value  +position.ToString() +   isn't in the this list</exception>
        /// <exception cref="System.NotImplementedException"></exception>
        public void InsertAfter(T item, T position)
        {
            LinkNode node = new LinkNode(item);
            references.Add(item, node);

            LinkNode pos;
            if (!references.TryGetValue(position, out pos)) throw new ArgumentException("This value " + position.ToString() + "  isn't in the this list");

            LinkNode next = pos.Next;

            next.Prev = node;
            pos.Next = node;

            node.Prev = pos;
            node.Next = next;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove all node
        /// </summary>
        public void Clear()
        {
            head = null;
            references.Clear();
            count = 0;
        }

        /// <summary>
        /// Gets the item at the current index O(n) complexity
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>T.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">index</exception>
        public T this[int index]
        {
            get
            {
                if (index >= count || index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                else
                {
                    LinkNode node = head;
                    for (int i = 0; i < index; i++) node = node.Next;
                    return node.item;
                }
            }
        }

        /// <summary>
        /// Convert into array where first element is the Head
        /// </summary>
        public T[] ToArray()
        {
            T[] array = new T[count];
            int i = 0;
            LinkNode current = head;
            do
            {
                array[i] = current.item;
                i++;
                current = current.Next;
            }
            while (current != head);

            return array;
        }


        #region Enumerators

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            while (MoveNext()) yield return current.item;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        public T Current
        {
            get { return current.item; }
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        /// <summary>
        /// Moves the next.
        /// </summary>
        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Class used to store the linked pointers
        /// </summary>
        internal class LinkNode : ILink<LinkNode>
        {
            public LinkNode Next { get; set; }
            public LinkNode Prev { get; set; }

            public T item;

            public LinkNode(T item)
            {
                this.item = item;
                Next = null;
                Prev = null;
            }
        }
    }

    /// <summary>
    /// To maintain a pointer or reference, its require a class
    /// ATTENTION : don't set values, let CircularLinkedList2 do it
    /// </summary>
    public interface ILink<T> where T : class
    {
        /// <summary>
        /// Pointers to next item
        /// </summary>
        T Next { get; set; }
        /// <summary>
        /// Pointers to previous item
        /// </summary>
        T Prev { get; set; }
    }

    /// <summary>
    /// Not Generic circular list, the parameter T contain the next and prev reference, so is not necessary maintain a internal
    /// dictionary of references, all processes are faster but require a class and you MUST not touch the next and prev value
    /// </summary>
    [DebuggerDisplay("Count = {m_count}")]
    public class CircularLinkedList2<T> : IEnumerable<T>
        where T : class, ILink<T>
    {
        int count = 0;
        /// <summary>
        /// The own enumerator used to implement foreach
        /// </summary>
        CircularEnumerator<T> enumerator;

        /// <summary>
        /// Capacity aren't required
        /// </summary>
        public CircularLinkedList2()
        {
            enumerator = new CircularEnumerator<T>(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularLinkedList2{T}"/> class.
        /// </summary>
        public CircularLinkedList2(IEnumerable<T> collection) : this()
        {
            IEnumerator<T> collection_enum = collection.GetEnumerator();
            while (collection_enum.MoveNext()) this.AddLast(collection_enum.Current);
        }

        /// <summary>
        /// Get or Set the first element in the circular link. No check are made.
        /// </summary>
        public T Head
        {
            get { return enumerator.head; }
            set { enumerator.head = value; }
        }

        /// <summary>
        /// Get number of elements in the list
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// first time you add a node
        /// </summary>
        void addfirst(T node)
        {
            enumerator.head = node;
            node.Next = node.Prev = node;
            count = 1;
        }

        /// <summary>
        /// Add node before Head (mean in the last position), if first element added will become head
        /// Is equal to InsertBefore(node,Head)
        /// </summary>
        /// <param name="node">The node.</param>
        public void AddLast(T node)
        {
            InsertBefore(node, enumerator.head);
        }
        /// <summary>
        /// Add node after the node "position"
        /// </summary>
        /// <param name="node">the node to add</param>
        /// <param name="position">the node that will be previous of added node</param>
        /// <remarks>
        /// <para>if position = n2</para>
        /// <para>Head--n1--n2--nx--n3</para>
        /// <para>|_________________|</para>
        /// <para>the new node nx will be insert after n2</para>
        /// </remarks>
        public void InsertAfter(T node, T position)
        {
            if (count == 0)
            {
                addfirst(node);
                return;
            }
            if (position == null) throw new ArgumentNullException("head isn't null so previous can't be null");

            T next = position.Next;

            node.Prev = position;
            node.Next = next;

            next.Prev = node;
            position.Next = node;

            count++;
        }
        /// <summary>
        /// Add node before the node "position"
        /// </summary>
        /// <param name="node">the node to add</param>
        /// <param name="position">the node that will be next of added node</param>
        /// <remarks>
        /// <para>if position = n2</para>
        /// <para>Head--n1--nx--n2--n3</para>
        /// <para>|_________________|</para>
        /// <para>the new node nx will be insert before n2</para>
        /// </remarks>
        public void InsertBefore(T node, T position)
        {
            if (count == 0)
            {
                addfirst(node);
                return;
            }

            if (position == null) throw new ArgumentNullException("head isn't null so antecedent can't be null");
            
            T prev = position.Prev;

            node.Next = position;
            node.Prev = prev;

            prev.Next = node;
            position.Prev = node;

            count++;
        }

        /// <summary>
        /// Remove a node added to this list, no safety test was made so ensure that node is its.
        /// All reference "Next" and "Prev" will set to null. If you remove the head, the new head will be Head.Next.
        /// </summary>
        public void Remove(T node)
        {
            T prev = node.Prev;
            T next = node.Next;

            prev.Next = next;
            next.Prev = prev;

            // if you delete head, assign a new head
            if (enumerator.head == node)
            {
                // to allow removing when you are calling MoveNext(), you need to reset the staring
                enumerator.Reset();
                // if next == head mean are you deleting last node       
                enumerator.head = (next == enumerator.head) ? null : next;
            }
            node.Next = node.Prev = null;

            count--;
        }
        /// <summary>
        /// if fast : set to null all Prev and Next reference, remove head reference and set count = 0
        /// </summary>
        public void Clear(bool fast = false)
        {
            count = 0;
            if (enumerator.head == null) return;

            if (!fast)
            {
                T node = enumerator.head.Next;
                while (node != enumerator.head)
                {
                    node.Prev = null;
                    node = node.Next;
                    node.Prev.Next = null;
                    node.Prev = null;
                }
                enumerator.head.Prev = enumerator.head.Next = null;
            }
            enumerator.head = null;
        }

        /// <summary>
        /// Change the order switching the Prev and Next reference
        /// ATTENTION, can't be called when you are looping with MoveNext() or foreach
        /// </summary>
        public void Invert()
        {
            enumerator.Reset();

            // head set to tail
            enumerator.head = enumerator.head.Prev;

            foreach(T node in enumerator)
            {
                T prev = node.Prev;
                T next = node.Next;
                node.Next = prev;
                node.Prev = next;
            }
        }

        /// <summary>
        /// Change the output direction without switching the Prev and Next reference
        /// ATTENTION, can't be called when you are looping with MoveNext() or foreach
        /// </summary>
        public bool InvertedDirection
        {
            get { return enumerator.InvertedDirection; }
            set { enumerator.InvertedDirection = value; }
        }

        /// <summary>
        /// Gets the item at the current index O(n) complexity, not very useful
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index >= count || index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                else
                {
                    T node = enumerator.head;
                    for (int i = 0; i < index; i++) node = node.Next;
                    return node;
                }
            }
        }

        /// <summary>
        /// Convert to array where first element is the head
        /// </summary>
        public T[] ToArray()
        {
            T[] array = new T[count];
            int i = 0;
            if (enumerator.head != null)
            {
                T node = enumerator.head;
                do
                {
                    array[i++] = node;
                    node = node.Next;
                }
                while (node != enumerator.head);
            }
            return array;
        }

        /// <summary>
        /// Search in this list the node
        /// </summary>
        public bool Contains(T node)
        {
            return enumerator.head != null ? search(enumerator.head, node) : false;
        }

        /// <summary>
        /// Searches the specified node from start node.
        /// </summary>
        bool search(T start, T node)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }


        public IEnumerator<T> GetEnumerator()
        {
            enumerator.Reset();
            while (enumerator.MoveNext()) yield return enumerator.Current;
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }


    /// <summary>
    /// Each enumerator manage a loop separately, can be used example to make more scan of list in same time.
    /// ATTENTION, if you change the head of main circularlinkedlist, u must riassing a new one in this instance
    /// </summary>
    public class CircularEnumerator<T> : IEnumerator<T>, IEnumerable<T> 
        where T : class, ILink<T>
    {
        bool inverted = false;
        internal T head;
        T current, currnext;
        delegate T MoveNextDelegate(T node);
        MoveNextDelegate getNextNode = null;


        public CircularEnumerator(T Head)
        {
            this.head = Head;
            this.getNextNode = getNext;
        }

        public T Current
        {
            get { return current; }
        }

        /// <summary>
        /// Get or Set the first element in the circular link. No check are made.
        /// </summary>
        public T Head
        {
            get { return head; }
            set { head = value; }
        }
        /// <summary>
        /// If true the loop go in opposite direction node.prev -> .prev etc...
        /// </summary>
        public bool InvertedDirection
        {
            get { return inverted; }
            set
            {
                // make change only if need, 
                if (inverted != value)
                {
                    if (value)
                    {
                        // use prev reference
                        getNextNode = getPrev;
                    }
                    else
                    {
                        // usually use next reference
                        getNextNode = getNext;
                    }
                    // head must switch to tail in both case
                    head = getNextNode(head);
                }
                inverted = value;
            }
        }

        T getNext(T node) { return node.Next; }
        T getPrev(T node) { return node.Prev; }

        public void Dispose()
        {

        }

        object IEnumerator.Current
        {
            get { return current; }
        }
        
        /// <summary>
        /// Moves the next until Head
        /// </summary>
        /// <example> 
        /// <para>An example of circular list is</para>
        /// <para>Head--n1--n2--n3--nx</para>
        /// <para> |_________________|</para>
        /// <para>the new node nx will be insert after n3 or before head</para>
        /// The Removing process work also when you are looping with MoveNext()
        /// list.Reset();
        /// <code>
        /// while (list.MoveNext())
        /// {
        /// Console.Write(list.Current);
        /// list.Remove(list.Current);
        /// }
        /// Node[] array = list.ToArray();
        /// </code>
        /// </example>
        public bool MoveNext()
        {
            // at the first MoveNext call
            if (currnext == null)
            {
                current = head;
                if (head != null)
                {
                    currnext = getNextNode(current);
                    return true;
                }
                return false;
            }

            if (currnext == head) return false;

            current = currnext;
            currnext = getNextNode(current);

            return true;
        }
        /// <summary>
        /// Resets the loop
        /// </summary>
        public void Reset()
        {
            current = null;
            currnext = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            while (MoveNext())
                yield return current;
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }
}
