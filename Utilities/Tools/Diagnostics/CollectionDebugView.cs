using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Common.Diagnostics
{
    /// <summary>
    /// https://www.codeproject.com/Articles/28405/Make-the-debugger-show-the-contents-of-your-custom
    /// </summary>
    public class CollectionDebugView<T>
    {
        private IEnumerable<T> collection;

        public CollectionDebugView(IEnumerable<T> collection)
        {
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                List<T> list = new List<T>();
                foreach (var item in collection) list.Add(item);
                return list.ToArray();
            }
        }
    }
}
