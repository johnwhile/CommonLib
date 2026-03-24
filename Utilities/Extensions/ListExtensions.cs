using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class ListExtensions
    {
        public static void AddRange<T>(this List<T> list, IEnumerable<T> collection, int startindex, int count)
        {
            int index = 0;
            list.Capacity = list.Count + count;
            foreach (T item in collection) if (index >= startindex && index++ < startindex + count) list.Add(item);
            while (index++ < startindex + count) list.Add(default(T));
        }

        /// <summary>
        /// Force to set item at index position, create null elements if index is out of range
        /// </summary>
        public static void SetItemAt<T>(this List<T> list, int index, T item)
        {
            if (index >= list.Count) for (int i = list.Count; i <= index; i++) list.Add(default(T));
            list[index] = item;
        }
        /// <summary>
        /// Force get the item, null if not found
        /// </summary>
        public static T GetItemAt<T>(this List<T> list, int index) => index >= list.Count ? default(T) : list[index];
    }
}
