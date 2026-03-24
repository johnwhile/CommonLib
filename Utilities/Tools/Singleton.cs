
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Tools
{
    public class Singleton<T> where T : class , new()
    {
        internal T unique;

        public T GetSingletonInstance()
        {
            if (unique == null) unique = new T();
            return unique;
        }

    }
}
