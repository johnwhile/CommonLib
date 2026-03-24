using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Common.Tools
{
    /// <summary>
    /// All node used for my collection manager classes need a uint key and a flag for clean process
    /// </summary>
    public interface IKey
    {
        /// <summary>
        /// Unique key value for a fast access
        /// </summary>
        /// <remarks>
        /// Is equivalent to call GetHashCode() but i prefer use my key generator to avoid identical keys
        /// </remarks>
        uint m_key { get; set; }
        /// <summary>
        /// Set to true to mark as removed in the clean process
        /// </summary>
        bool m_removed { get; set; }
    }

    /// <summary>
    /// manage the HashTable (or Dirctionary for framework > 1.1) algorith, a unique key generator was made to return a key value
    /// every time you add a new value class
    /// </summary>
    public abstract class Collection<V> : IEnumerable<V> where V : class , IKey 
    {
        protected uint keycounter = 0;
        /// <summary>
        /// The hash table for value &lt;=&gt; key association
        /// </summary>
        protected Dictionary<uint, V> hash; //Hashtable hash;

        /// <summary>
        /// set = null to invalidate
        /// </summary>
        protected List<uint> tmp_hashkeys = null; // null to invalidate

        public abstract List<V> ValueList { get; }
        
        public List<uint> KeyList
        {
            get
            {
                if (tmp_hashkeys == null)
                    tmp_hashkeys = new List<uint>(hash.Keys);
                return tmp_hashkeys;
            }
        }

        public Collection(int capacity)
        {
            hash = new Dictionary<uint, V>(capacity);
        }

        public int Count 
        {
            get { return hash.Count; } 
        }
        /// <summary>
        /// Key zero is reserved for not-initialization exception
        /// </summary>
        public bool Contain(uint key)
        {
            if (key == 0) return false;
            return hash.ContainsKey(key);
        }
        public bool Contain(V value)
        {
            return Contain(value.m_key);
        }
        protected V get(uint key)
        {
            return hash[key];
        }
        protected void set(uint key, V value)
        {
            hash[key] = value;
            value.m_key = key;
        }      
        /// <summary>
        /// DEPRECATED, need a new implementation if you use a ordered collector
        /// </summary>
        protected V get(int index)
        {
            foreach(uint k in hash.Keys)
            {
                if (index--<=0)
                return hash[k];
            }
            return null;
        }        
        /// <summary>
        /// DEPRECATED, need a new implementation if you use a ordered collector
        /// The key value will be set using index key value
        /// </summary>
        protected void set(int index, V value)
        {
            foreach (uint k in hash.Keys)
            {
                if (index-- <= 0)
                {
                    hash[k] = value;
                    value.m_key = k;
                }
            }
        }
        /// <summary>
        /// Generate a unique key, 0 is reserved, "uint.maxvalue" is big enougth for all case
        /// </summary>
        protected uint generateUniqueKey()
        {
            for (uint i = 1; i < uint.MaxValue - 1; i++)
            {
                keycounter = unchecked(keycounter + 1); // loop the integer value
                if (keycounter == 0) keycounter++; // zero is reserved
                if (!hash.ContainsKey(keycounter)) return keycounter;
            }
            throw new OutOfMemoryException("reach the maximum number of nodes");
        }

        public abstract IEnumerator<V> GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder("m_count = " + Count + "\r\n");
            foreach (V value in this)
                str.Append(value.ToString() + "\r\n");
            return str.ToString();
        }
    }
    /// <summary>
    /// Implement the Dictionary algorithm when order is't important, in fact i don't implement get by index search function
    /// </summary>
    public abstract class UnsortedCollection<V> : Collection<V> where V : class , IKey 
    {
        public override List<V> ValueList { get { return new List<V>(hash.Values); } }
        public override IEnumerator<V> GetEnumerator()
        {
            foreach (KeyValuePair<uint, V> pair in hash)
            {
                yield return pair.Value;
            }
        }
        public UnsortedCollection(int capacity) : base(capacity)
        {
            tmp_hashkeys = null;
        }
        protected uint add(V value)
        {
            uint key = generateUniqueKey();
            value.m_key = key;
            this.add(key, value);
            return key;
        }
        protected void add(uint key, V value)
        {
            tmp_hashkeys = null;
            hash.Add(key, value);
        }
        protected bool remove(uint key , out V value)
        {
            if (this.Contain(key))
            {
                tmp_hashkeys = null;
                value = hash[key];
                hash.Remove(key);
                return true;
            }
            value = null;

            return false;
        }
        protected bool remove(V value)
        {  
            if (this.Contain(value.m_key))
            {
                tmp_hashkeys = null;
                hash.Remove(value.m_key);
                return true;
            }
            return false;
        }
        protected void clear()
        {
            tmp_hashkeys = null;
            hash = new Dictionary<uint, V>();
            keycounter = 0;
        }   
    }
    /// <summary>
    /// TODO : Sorted collection have a list class to mantain the index value, but need to improve performance.
    /// The main function is Sort() because i need a internal arrangement example when sorting caches by texture id
    /// </summary>
    public abstract class SortedCollection<V> : Collection<V> where V : class , IKey 
    {
        //where store the order
        protected List<V> list;     
        public override List<V> ValueList { get { return list; } }
        
        public override IEnumerator<V> GetEnumerator()
        {
            for (int i = 0; i < list.Count; i++)
            {
                yield return list[i];
            }
        }

        public SortedCollection(int capacity) : base(capacity)
        {
            list = new List<V>(capacity);
        }

        protected new V get(int index)
        {
            return list[index];
        }
        protected new void set(int index, V value)
        {
            list[index] = value;
            hash[value.m_key] = value;
        }
        protected uint add(V value)
        {
            uint key = generateUniqueKey();
            value.m_key = key;
            this.add(key, value);
            return key;
        }
        protected void add(uint key, V value)
        {
            hash.Add(key, value);
            list.Add(value);
        }
        protected void insert(int index, uint key, V value)
        {
            hash.Add(key, value);
            list.Insert(index, value);
        }
        /// <summary>
        /// </summary>
        /// <param name="value">memorize the value affect by removing, can be usefull</param>
        protected bool remove(uint key,out V value)
        {
            return removeAt(indexOfList(key), out value);
        }
        /// <summary>
        /// remove by value is derived from remove by key function
        /// </summary>
        protected bool remove(V value)
        {
            return remove(value.m_key, out value);
        }
        /// <summary>
        /// </summary>
        /// <param name="value">memorize the value affect by removing, can be usefull</param>
        protected bool removeAt(int index , out V value)
        {
            try
            {
                value = list[index];
                hash.Remove(value.m_key);
                list.RemoveAt(index);
                return true;
            }
            // for performance optimization all cecks will be done only when occour an error
            catch(Exception e)
            {
                value = null;
                string error = "";
                if (index >= Count) error = "index of of range";
                else if (index < Count && index >= list.Count) error = "list m_count error";
                else if (!Contain(list[index].m_key)) error = "key not found in the hash table";
                else error = "unknow error : " + e.Message.ToString();

                Console.WriteLine(error);
                return false;
            }
        }
        protected void clear()
        {
            hash = new Dictionary<uint, V>();
            list = new List<V>();
            keycounter = 0;
        }

        int indexOfList(uint key)
        {
            return list.IndexOf(hash[key]);
            //for (int n = 0; n < list.Count; n++)
            //    if (list[n].m_key.Equals(key)) return n;
            //return -1;
        }        
        
        public void Sort(Comparison<V> comparer)
        {
            list.Sort(comparer);
        }
        public void Sort()
        {
            list.Sort();
        }
    }

    /// <summary>
    /// Ordered collection of classes, access by unique key identifier. This key is saved in the "UniqueKey" derived classes
    /// </summary>
    /// <remarks>
    /// When item's class is created, the key value is 0 to understand that isn't added in the collection.
    /// When add the manager generate a unique Uint32 key and save it in the class.
    /// When remove the manager set key to zero but don't destroy class.
    /// If the number of class is bigger than all possible combination of key's value (&gt; uint.MaxValue) generate a OutOfMemoryException.
    /// </remarks>
    /// <typeparam name="V">a class what implement uint key value</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class OrderedCollectionManager<V> : SortedCollection<V> where V : class , IKey 
    {
        /// <summary>
        /// if false, the value must have already a key value set by another collection manager
        /// </summary>
        bool isMainCollector = true;

        public OrderedCollectionManager(bool IsMainCollector)
            : base(0)
        {
            this.isMainCollector = IsMainCollector;
        }
        /// <summary>
        /// Get or Set the node by key
        /// </summary>
        /// <remarks>
        /// if is a main collector, the overwritten node is removed so m_key value set to zero
        /// </remarks>
        public V this[uint key]
        {
            get 
            { 
                return get(key);
            }
            set 
            {
                if (base.Contain(key))
                {
                    V prev = get(key);
                    prev.m_key = 0;
                    set(key, value);
                }
                else
                {
                    this.Add(value);
                }
            }
        }
        /// <summary>
        /// Get or Set the node by index
        /// </summary>
        /// <remarks>
        /// if is a main collector, the overwritten node is removed so m_key value set to zero
        /// </remarks>
        public V this[int index]
        {
            get
            {
                return get(index);
            }
            set
            {
                if (index<0 || index>=Count)
                    throw new IndexOutOfRangeException("");

                if (isMainCollector)
                {
                    V prev = list[index];
                    value.m_key = prev.m_key;
                    prev.m_key = 0;
                }
                set(index, value);
            }
        }
       
        public bool Add(V value)
        {
            if (isMainCollector)
            {
                if (value.m_key != 0 || base.Contain(value.m_key))
                    throw new ArgumentException("class already added in a main collection manager");
            }
            else
            {
                if (value.m_key == 0)
                    throw new ArgumentException("class not added in a main collection manager");
            }
            if (value.m_removed)
            {
                Console.WriteLine("can't add a node marked as removed");
                return false;
            }
            return add(value) > 0;
        }
        public bool Remove(V value)
        {
            // this because remove by value was derived from remove by key in the base class implementations
            return Remove(value.m_key);
        }
        public bool Remove(uint key)
        {
            if (key == 0) throw new ArgumentException("class not added in a main collection manager");

            V value;
            bool result = remove(key, out value);
            if (result && isMainCollector)
            {
                value.m_key = 0;
                value.m_removed = false;
            }
            return result;
        }
        public bool Remove(int index)
        {
            V value;
            bool result = removeAt(index, out value);
            // if is a main collector manager, you can invalidate its key value, so the node can be added in a other collector
            if (result && isMainCollector)
            {
                value.m_key = 0;
                value.m_removed = false;
            }
            return result;
        }
        /// <summary>
        /// Remove all node and set m_key value to 0 only if is a main collector
        /// </summary>
        public void Clear()
        {
            if (isMainCollector)
                foreach (V value in base.list)
                {
                    value.m_key = 0;
                    value.m_removed = false;
                }
            base.clear();
        }
        /// <summary>
        /// Remove all node marked to removed
        /// </summary>
        public void Clean()
        {
            int i = 0;
            while (i < Count)
            {
                if (list[i].m_removed)
                {
                    this.Remove(i);
                    i--;
                }
                i++;
            }
            if (Count == 0) base.clear();
        }
    }

    /// <summary>
    /// Unsorted collection of classes, access by unique key identifier. This key is saved in the "UniqueKey" derived classes
    /// Managed because it accepts only "UniqueKey" class with key set to 0, and it generate a new one.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class UnsortedCollectionManager<V> : UnsortedCollection<V> where V : class , IKey 
    {
        bool isMainCollector = true;

        public UnsortedCollectionManager(bool IsMainCollector) : base(0)
        {
            this.isMainCollector = IsMainCollector;
        }
        public V this[uint key]
        {
            get
            {
                return get(key);
            }
            set
            {
                if (base.Contain(key))
                {
                    V prev = get(key);
                    prev.m_key = 0;
                    set(key, value);
                }
                else
                {
                    this.Add(value);
                }
            }
        }

        public bool Add(V value)
        {
            if (isMainCollector)
            {
                if (value.m_key != 0 || base.Contain(value.m_key))
                {
                    Console.WriteLine("class already added in a main collection manager");
                    return false;
                }
            }
            else
            {
                if (value.m_key == 0)
                {
                    Console.WriteLine("class not added in a main collection manager");
                    return false;
                }
            }
            if (value.m_removed)
            {
                Console.WriteLine("can't add a node marked as removed");
                return false;
            }

            return add(value) > 0;
        }
        public bool Remove(V value)
        {
            // this because remove by value was derived from remove by key in the base class implementations
            return Remove(value.m_key);
        }
        public bool Remove(uint key)
        {
            if (key == 0)
            {
                Console.WriteLine("class not added in a main collection manager");
                return false;
            }
            V value;
            bool result = remove(key, out value);
            if (result && isMainCollector)
            {
                value.m_key = 0;
                value.m_removed = false;
            }
            return result;
        }
        /// <summary>
        /// Remove all node and set m_key to 0 only for main collector
        /// </summary>
        public void Clear()
        {
            if (isMainCollector)
                foreach (V value in this)
                {
                    value.m_key = 0;
                    value.m_removed = false;
                }
            base.clear();
        }
        /// <summary>
        /// TODO : need to improve it, remove all node marked as removed
        /// </summary>
        public void Clean()
        {
            Dictionary<uint, V> tmp_hash = new Dictionary<uint,V>(hash);
            foreach (KeyValuePair<uint, V> item in tmp_hash)
            {
                if (item.Value.m_removed)
                    this.Remove(item.Value);
            }
        }
    }

}
