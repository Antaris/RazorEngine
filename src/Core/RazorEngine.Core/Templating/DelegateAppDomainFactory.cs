namespace RazorEngine.Templating
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides an <see cref="AppDomain"/> factory that supports delegated <see cref="AppDomain"/> creation.
    /// </summary>
    internal class DelegateAppDomainFactory : IAppDomainFactory
    {
        #region Fields
        private readonly Func<AppDomain> _factory;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="DelegateAppDomainFactory"/>.
        /// </summary>
        /// <param name="factory">The factory delegate.</param>
        public DelegateAppDomainFactory(Func<AppDomain> factory)
        {
            Contract.Requires(factory != null);

            _factory = factory;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates the <see cref="AppDomain"/>.
        /// </summary>
        /// <returns>The <see cref="AppDomain"/> instance.</returns>
        public AppDomain CreateAppDomain()
        {
            return _factory();
        }
        #endregion
    }
}
