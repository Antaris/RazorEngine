//-----------------------------------------------------------------------------
// <copyright file="IAppDomainFactory.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    using System;

    /// <summary>
    /// Defines the required contract for implementing an <see cref="AppDomain"/> factory.
    /// </summary>
    public interface IAppDomainFactory
    {
        #region Methods

        /// <summary>
        /// Creates the <see cref="AppDomain"/>.
        /// </summary>
        /// <returns>The <see cref="AppDomain"/> instance.</returns>
        AppDomain CreateAppDomain();

        #endregion
    }
}
