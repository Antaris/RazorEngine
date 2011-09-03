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
        /// Initialises a new instance of <see cref="CachedTemplateItem"/>.
        /// </summary>
        /// <param name="cachedHashCode">The cached hash code.</param>
        /// <param name="templateType">The template type.</param>
        public CachedTemplateItem(int cachedHashCode, Type templateType)
        {
            CachedHashCode = cachedHashCode;
            TemplateType = templateType;
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
