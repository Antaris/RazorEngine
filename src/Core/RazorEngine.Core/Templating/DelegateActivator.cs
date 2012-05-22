//-----------------------------------------------------------------------------
// <copyright file="DelegateActivator.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
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

        /// <summary>
        /// The activator
        /// </summary>
        private readonly Func<InstanceContext, ITemplate> activator;

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateActivator"/> class.
        /// </summary>
        /// <param name="activator">The delegated used to create an instance of the template.</param>
        public DelegateActivator(Func<InstanceContext, ITemplate> activator)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(activator != null);
            /* ReSharper restore InvocationIsSkipped */

            this.activator = activator;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the activator.
        /// </summary>
        internal Func<InstanceContext, ITemplate> Activator
        {
            get
            {
                return this.activator;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the specified template.
        /// </summary>
        /// <param name="context">The instance context.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        [Pure]
        public ITemplate CreateInstance(InstanceContext context)
        {
            return this.activator(context);
        }

        #endregion
    }
}