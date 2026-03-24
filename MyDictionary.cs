using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class WeakCollection<TValue> where TValue : class, IDisposable
    {
        /// <summary>
        /// collection of strongly-typed weak references.
        /// </summary>
        private readonly List<WeakReference<TValue>> list;


        public WeakCollection(int capacity = 0)
        {
            list = new List<WeakReference<TValue>>(capacity);

        }

        /// <summary>
        /// Adds a weak reference to an object to the collection. Does not cause a purge.
        /// </summary>
        /// <param name="item">The object to add a weak reference to.</param>
        public void Add(TValue item)
        {
            list.Add(new WeakReference<TValue>(item));
        }


        /// <summary>
        /// Removes a weak reference to an object from the collection. Does not cause a purge.
        /// </summary>
        /// <param name="item">The object to remove a weak reference to.</param>
        /// <returns>True if the object was found and removed; false if the object was not found.</returns>
        public bool Remove(TValue item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TryGetTarget(out var entry) && entry == item)
                {
                    list.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

    }
}
