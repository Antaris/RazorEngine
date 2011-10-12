namespace RazorEngine.Templating
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines an activator that supports delegated activation.
    /// </summary>
    internal class DelegateActivator : IActivator
    {
        #region Fields
        private readonly Func<InstanceContext, ITemplate> _activator;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="DelegateActivator"/>.
        /// </summary>
        /// <param name="activator">The delegated used to create an instance of the template.</param>
        public DelegateActivator(Func<InstanceContext, ITemplate> activator)
        {
            Contract.Requires(activator != null);

            _activator = activator;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the activator.
        /// </summary>
        internal Func<InstanceContext, ITemplate> Activator { get { return _activator; } }
        #endregion

        #region Methods
        /// <summary>
        /// Creates an instance of the specifed template.
        /// </summary>
        /// <param name="context">The instance context.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        [Pure]
        public ITemplate CreateInstance(InstanceContext context)
        {
            return _activator(context);
        }
        #endregion
    }
}