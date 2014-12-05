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
        private readonly Func<string, string> _resolver;
        private readonly Func<string, string> _filenameResolver;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="DelegateTemplateResolver"/>.
        /// </summary>
        /// <param name="resolver">The resolver delegate.</param>
        /// <param name="filenameResolver">The filename resolver delegate.</param>
        public DelegateTemplateResolver(Func<string, string> resolver, Func<string, string> filenameResolver = null)
        {
            Contract.Requires(resolver != null);

            _resolver = resolver;
            _filenameResolver = filenameResolver;
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
            return _resolver(name);
        }

        /// <summary>
        /// Resolves the filename of the specified templatename.
        /// Used for debugging.
        /// </summary>
        /// <param name="name">The name of the template to resolve.</param>
        /// <returns>The fullpath to the template.</returns>
        public string ResolveFilename(string name)
        {
            if (_filenameResolver == null) return null;

            return _filenameResolver(name);
        }
        #endregion
    }
}