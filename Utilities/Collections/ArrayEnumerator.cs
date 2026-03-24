using System;
using System.Collections;
using System.Collections.Generic;

namespace Common.Tools
{
     public abstract class ListEnumerator<T> : IEnumerator<T>, IEnumerable<T>
     {
        protected T current;
        protected int count = 0;

        public T Current { get { return current; } }
        
        public abstract bool MoveNext();

        public abstract void Reset();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            while (MoveNext())
                yield return current;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
     }
}
