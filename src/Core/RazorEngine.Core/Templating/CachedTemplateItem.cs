//-----------------------------------------------------------------------------
// <copyright file="CachedTemplateItem.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    using System;

    /// <summary>
    /// Defines a cached template item.
    /// </summary>
    internal class CachedTemplateItem
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedTemplateItem"/> class.
        /// </summary>
        /// <param name="cachedHashCode">The cached hash code.</param>
        /// <param name="templateType">The template type.</param>
        public CachedTemplateItem(int cachedHashCode, Type templateType)
        {
            this.CachedHashCode = cachedHashCode;
            this.TemplateType = templateType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the cached hash code of the template.
        /// </summary>
        public int CachedHashCode { get; private set; }

        /// <summary>
        /// Gets the template type.
        /// </summary>
        public Type TemplateType { get; private set; }

        #endregion
    }
}
