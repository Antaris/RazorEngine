namespace RazorEngine.Templating
{
    using System;

    /// <summary>
    /// Provides a default implementation of an <see cref="AppDomain"/> factory.
    /// </summary>
    public class DefaultAppDomainFactory : IAppDomainFactory
    {
        #region Methods
        /// <summary>
        /// Creates the <see cref="AppDomain"/>.
        /// </summary>
        /// <returns>The <see cref="AppDomain"/> instance.</returns>
        public AppDomain CreateAppDomain()
        {
            var current = AppDomain.CurrentDomain;
            var domain = AppDomain.CreateDomain("RazorHost", current.Evidence, current.SetupInformation);

            return domain;
        }
        #endregion
    }
}
