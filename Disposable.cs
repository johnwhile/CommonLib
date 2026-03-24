using System;
using System.Diagnostics;

namespace Common
{
    /// <summary>
    /// A disposable base class
    /// </summary>
    public abstract class Disposable : IDisposable
    {
        public static int MaxInstancesCount = 0;
        /// <summary>
        /// Gets or sets the disposables.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DisposableCollector DisposeCollector;

        /// <summary>
        /// Initializes a new instance of the <see cref="Disposable"/> class.
        /// </summary>
        protected internal Disposable() { }

        /// <summary>
        /// GC.Collect() call the finalizer at some point of life...
        /// </summary>
        ~Disposable() 
        {
            if (!IsDisposed)
            {
                Debugg.Warning($"Dispose object {this} because called by the finalizer, this is correct ?");
                Dispose();
            }
        }

        /// <summary>
        /// Check is object isn't null or disposed
        /// </summary>
        public static bool IsValid(Disposable obj) => obj != null && !obj.IsDisposed;

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsDisposed { get; protected set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsDisposing { get; private set; }

        /// <summary>
        /// Occurs when when Dispose is called. The action is called before this disposing. It's used to dispose all related resources. Not call this.<see cref="Dispose"/>
        /// </summary>
        public event Action<object> Disposing;

        /// <summary>
        /// trigger the <see cref="Disposing"/> events (for derived class).
        /// </summary>
        /// <remarks>It's used to dispose all related resources. It doesn't call this.<see cref="Dispose"/></remarks>
        public virtual void OnDisposing() 
        {
            Disposing?.Invoke(this);
        }

        /// <summary>
        /// Releases unmanaged and managed resources in the collector and dispose this instance.
        /// It call at first <see cref="OnDisposing"/>. Check if it's already disposed
        /// </summary>
        /// <remarks>It's called only if <code><see cref="IsDisposed"/>= true</code></remarks>
        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposing = true;
                OnDisposing();
                Dispose(true);
                IsDisposed = true;
            }
            IsDisposing = false;
        }

        /// <summary>
        /// Dispose all ComObjects.
        /// </summary>
        /// <param name="disposemanaged">If true, managed resources should be
        /// disposed of in addition to unmanaged resources.</param>
        protected virtual void Dispose(bool disposemanaged)
        {
            DisposeCollector?.Dispose(disposemanaged);
            if (disposemanaged) DisposeCollector = null;
        }

        /// <summary>
        /// Adds a disposable object to the list of the objects to dispose.
        /// </summary>
        protected virtual T ToDispose<T>(T obj)
        {
            if (!ReferenceEquals(obj, null))
            {
                MaxInstancesCount++;
                if (DisposeCollector == null) DisposeCollector = new DisposableCollector();
                return DisposeCollector.Collect(obj);
            }
            return default(T);
        }

        /// <summary>
        /// <inheritdoc cref="RemoveAndDispose{T}(T)"/><br/>
        /// <b>... and set the reference to null.</b>
        /// </summary>
        protected virtual void RemoveAndDispose<T>(ref T obj)
        {
            if (!ReferenceEquals(obj, null))
                DisposeCollector?.RemoveAndDispose(ref obj);
        }
        /// <summary>
        /// Dispose a disposable object. Removes this object from the disposable list.
        /// </summary>
        protected void RemoveAndDispose<T>(T obj)
        {
            RemoveAndDispose(ref obj);
        }
        /// <summary>
        /// <inheritdoc cref="DisposableCollector.RemoveToDispose(object)"/>
        /// </summary>
        protected virtual void RemoveToDispose<T>(T obj)
        {
            if (!ReferenceEquals(obj, null))
                DisposeCollector?.RemoveToDispose(obj);
        }

        protected bool Contains(object obj)
        {
            return DisposeCollector != null ? DisposeCollector.Contains(obj) : false;
        }
    }
}
