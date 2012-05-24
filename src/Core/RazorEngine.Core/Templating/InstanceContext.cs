//-----------------------------------------------------------------------------
// <copyright file="InstanceContext.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines contextual information for a template instance.
    /// </summary>
    public class InstanceContext
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceContext"/> class.
        /// </summary>
        /// <param name="loader">The type loader.</param>
        /// <param name="templateType">The template type.</param>
        internal InstanceContext(TypeLoader loader, Type templateType)
        {
            // ReSharper disable InvocationIsSkipped
            Contract.Requires(loader != null);
            Contract.Requires(templateType != null);
            /* ReSharper restore InvocationIsSkipped */

            this.Loader = loader;
            this.TemplateType = templateType;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the type loader.
        /// </summary>
        public TypeLoader Loader { get; private set; }

        /// <summary>
        /// Gets the template type.
        /// </summary>
        public Type TemplateType { get; private set; }

        #endregion
    }
}