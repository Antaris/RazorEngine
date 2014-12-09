namespace RazorEngine.Tests.TestTypes
{
    using System;
    using System.Threading;

    /// <summary>
    /// Defines a thread pool item.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public class ThreadPoolItem<T>
    {
        #region Fields
        private readonly Action<T> _action;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="ThreadPoolItem{T}"/>.
        /// </summary>
        /// <param name="model">The model instance.</param>
        /// <param name="resetEvent">The reset event.</param>
        /// <param name="action">The action to run.</param>
        public ThreadPoolItem(T model, ManualResetEvent resetEvent, Action<T> action)
        {
            Model = model;
            ResetEvent = resetEvent;
            _action = action;
        }
        #endregion

        #region Methods
        /// <summary>
        /// The callback method invoked by the threadpool.
        /// </summary>
        /// <param name="state">Any current state.</param>
        public void ThreadPoolCallback(object state)
        {
            _action(Model);
            ResetEvent.Set();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the model.
        /// </summary>
        public T Model { get; private set; }

        /// <summary>
        /// Gets the reset event.
        /// </summary>
        public ManualResetEvent ResetEvent { get; private set; }
        #endregion
    }
}
