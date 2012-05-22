//-----------------------------------------------------------------------------
// <copyright file="DelegateTemplateResolver.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides an <see cref="ITemplateResolver"/> that supports delegated template resolution.
    /// </summary>
    public class DelegateTemplateResolver : ITemplateResolver
    {
        #region Fields

        /// <summary>
        /// The resolver
        /// </summary>
        private readonly Func<string, string> resolver;

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateTemplateResolver"/> class.
        /// </summary>
        /// <param name="resolver">The resolver delegate.</param>
        public DelegateTemplateResolver(Func<string, string> resolver)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(resolver != null);
            /* ReSharper restore InvocationIsSkipped */

            this.resolver = resolver;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Resolves the template content with the specified name.
        /// </summary>
        /// <param name="name">The name of the template to resolve.</param>
        /// <returns>The template content.</returns>
        public string Resolve(string name)
        {
            return this.resolver(name);
        }

        #endregion
    }
}