
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Tools
{
    public abstract class PoolClass
    {
        protected PoolClass()
        {

        }

        ~PoolClass()
        {
            // Resurrecting the object
            HandleReAddingToPool(true);
        }

        private void HandleReAddingToPool(bool reRegisterForFinalization)
        {
            if (!Disposed)
            {
                // If there is any case that the re-adding to the pool failes, release the resources and set the internal Disposed flag to true
                try
                {
                    // Notifying the pool that this object is ready for re-adding to the pool.
                    ReturnToPool(reRegisterForFinalization);
                }
                catch (Exception)
                {
                    Disposed = true;
                    OnReleaseResources();
                }
            }
        }


        /// <summary>
        /// Need to link to ReturnToPool of PoolManager
        /// </summary>
        /// <param name="reRegisterForFinalization"></param>
        internal abstract void ReturnToPool(bool reRegisterForFinalization);


        /// <summary>
        /// Reset the object state to allow this object to be re-used by other parts of the application.
        /// </summary>
        protected virtual void OnResetState()
        {

        }

        /// <summary>
        /// Releases the object's resources
        /// </summary>
        protected virtual void OnReleaseResources()
        {

        }

        /// <summary>
        /// Internal flag that is being managed by the pool primary used to void resources are being releases twice.
        /// </summary>
        internal bool Disposed { get; set; }

    }



    public class PoolManager<T>
        where T : PoolClass
    {
        Stack<T> stack;

        public PoolManager()
        {
            stack = new Stack<T>();
        }


        private T CreatePooledObject()
        {
            // Throws an exception if the type doesn't have default ctor - on purpose! I've could've add a generic constraint with new (), but I didn't want to limit the user and force a parameterless c'tor
            T newObject  = (T)Activator.CreateInstance(typeof(T));

            // Setting the 'return to pool' action in the newly created pooled object
            newObject.ReturnToPool(true);
            return newObject;
        }
    }
}
