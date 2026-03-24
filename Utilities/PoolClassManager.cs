using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Common.Tools
{

    public interface IDestroyable
    {
        void Destroy();
    }


    public abstract class PoolClass<T> : IDisposable where T : class , new()
    {
        protected bool disposed = false;
        
        public Action<T> RePoolingFunction { get; internal set; }

        /// <summary>
        /// Initialize a new Class, assign a RePoolingFunction to work with manager
        /// </summary>
        public PoolClass()
        {
            disposed = false;
        }
        /// <summary>
        /// Finalizer
        /// </summary>
        ~PoolClass()
        {
            Dispose();

        }

        /// <summary>
        /// Called by PoolManager when class will be re-use
        /// </summary>
        public void Resurrect()
        {
            disposed = false;
            Reset();
        }
        /// <summary>
        /// Reseting class function, called when class are initialized or resurected
        /// </summary>
        public abstract void Reset();

        public bool IsDisposed
        {
            get { return disposed; }
        }

        public virtual void Dispose()
        {
            if (!disposed && RePoolingFunction != null)
            {
                disposed = true;
                RePoolingFunction(this as T);
            }
        }


    }
    

    /// <summary>
    /// Manager for classes used in small numbers but very frequently, pre-allocating some classes is not
    /// required initializating new classes but simply reuse some discarded classes. 
    /// </summary>
    public class PoolClassManager<T> : IDisposable where T : PoolClass<T>, new()
    {
        protected bool disposed = false;
        MyStack<T> pool;
        Type type;

        public readonly int Capacity = 0;

        public int InstancesGenerated = 0;

        public PoolClassManager(int Capacity)
        {
            this.Capacity = Capacity;
            disposed = false;
            type = typeof(T);
            pool = new MyStack<T>(10);

        }

        ~PoolClassManager()
        {
            Dispose();
        }

        /// <summary>
        /// Generate a class, if required initialize some new classes
        /// </summary>
        /// <returns></returns>
        public T New()
        {
            T obj;

            if (pool.Count == 0) IncreaseInstances(Capacity / 10);
            obj = pool.Pop();

            //obj = pool.Count == 0 ? generateInstance() : pool.Pop();

            obj.RePoolingFunction = RePoolingObject;
            obj.Resurrect();
            return obj;
        }

        public void RePoolingObject(T obj)
        {
            if (!disposed)
            {
                pool.Push(obj);
                obj.RePoolingFunction = null;
            }
        }

        public void IncreaseInstances(int num)
        {
            if (InstancesGenerated + num > Capacity) num = Capacity - InstancesGenerated;

            if (num <= 0) throw new OutOfMemoryException();

            for (int i = 0; i < num; i++)
            {
                T obj = generateInstance();
                pool.Push(obj);
            }
        }

        protected virtual T generateInstance()
        {
            T obj = (T)Activator.CreateInstance(type);
            InstancesGenerated++;
            return obj;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                while (pool.Count > 0)
                {
                    T obj = pool.Pop();
                    obj.RePoolingFunction = null;
                    obj.Dispose();
                }
                pool.Clear();
            }
        }

        public override string ToString()
        {
            return "PoolClassManager<" + type + ">";
        }
    }

    /// <summary>
    /// <see cref="PoolClassManager{T}"/>. Add UniqueID generator
    /// </summary>
    public class PoolClassIndexedManager<T> : PoolClassManager<T> where T : PoolClass<T>, IUniqueInstanced , new()
    {
        public UniqueIndexManager indexmanager;

        public PoolClassIndexedManager(int Capacity)
            : base(Capacity)
        {
            indexmanager = new UniqueIndexManager(Capacity, false);
        }

        protected override T generateInstance()
        {
            T obj = base.generateInstance();
            int idx = indexmanager.GetNextAvailableIndex(0);
            obj.UniqueID = idx;
            indexmanager.SetIndex(idx, true);
            return obj;
        }

    }


    public class PoolClassExample : PoolClass<PoolClassExample>, IUniqueInstanced
    {
        public int UniqueID { get; set; }

        public byte[] somedata;

        static int counter = 0;
        int instance = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public PoolClassExample()
        {
            UniqueID = -1;
            instance = counter++;
#if DEBUG
            Console.WriteLine("Construttore " + this.ToString());
#endif

            Reset();
        }

        public override void Reset()
        {
            somedata = new byte[1000];
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~PoolClassExample()
        {
#if DEBUG
            Console.WriteLine("Distruttore " + this.ToString());
#endif
        }

        public override string ToString()
        {
            return string.Format("CustomClass_{0}_ID{1}, disposed:{2}", instance, UniqueID, IsDisposed);
        }
    }

}
