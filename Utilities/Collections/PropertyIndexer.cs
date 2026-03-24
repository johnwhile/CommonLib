using System;

namespace Common
{
    /// <summary>
    /// https://www.codeproject.com/Tips/319825/Multiple-Indexers-in-Csharp
    /// </summary>
    public class PropertyIndexerGetSet<Tindex, Ttype>
    {
        Func<Tindex, Ttype> m_get;
        Action<Tindex, Ttype> m_set;

        public PropertyIndexerGetSet(Func<Tindex, Ttype> getter, Action<Tindex, Ttype> setter)
        {
            if (getter == null || setter == null) throw new ArgumentNullException();
            m_get = getter;
            m_set = setter;
        }

        public Ttype this[Tindex index]
        {
            get { return m_get(index); }
            set { m_set(index, value); }
        }
    }

    public class PropertyIndexerGet<K, T>
    {
        Func<K, T> m_get;

        public PropertyIndexerGet(Func<K, T> getter)
        {
            if (getter == null) throw new ArgumentNullException();
            m_get = getter;
        }

        public T this[K index]
        {
            get { return m_get(index); }
        }
    }
    
    public class PropertyIndexerSet<K, T>
    {
        Action<K, T> m_set;

        public PropertyIndexerSet(Action<K, T> setter)
        {
            if (setter == null) throw new ArgumentNullException();
            m_set = setter;
        }

        public T this[K index]
        {
            set { m_set(index, value); }
        }
    }
}
