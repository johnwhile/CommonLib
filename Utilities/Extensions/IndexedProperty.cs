using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class IndexedProperty<Index, Value>
    {
        readonly Action<Index, Value> set;
        readonly Func<Index, Value> get;

        public IndexedProperty(Func<Index, Value> getFunc, Action<Index, Value> setAction)
        {
            get = getFunc;
            set = setAction;
        }

        public Value this[Index i]
        {
            get => get(i);
            set => set(i, value);
        }
    }
}
