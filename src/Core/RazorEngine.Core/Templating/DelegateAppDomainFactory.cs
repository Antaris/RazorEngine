//-----------------------------------------------------------------------------
// <copyright file="DelegateAppDomainFactory.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
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

        /// <summary>
        /// The factory
        /// </summary>
        private readonly Func<AppDomain> factory;

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateAppDomainFactory"/> class.
        /// </summary>
        /// <param name="factory">The factory delegate.</param>
        public DelegateAppDomainFactory(Func<AppDomain> factory)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(factory != null);
            /* ReSharper restore InvocationIsSkipped */

            this.factory = factory;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Creates the <see cref="AppDomain"/>.
        /// </summary>
        /// <returns>The <see cref="AppDomain"/> instance.</returns>
        public AppDomain CreateAppDomain()
        {
            return this.factory();
        }

        #endregion
    }
}
