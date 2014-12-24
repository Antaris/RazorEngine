namespace RazorEngine.Templating
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides an <see cref="ITemplateResolver"/> that supports delegated template resolution.
    /// </summary>
    [Obsolete("please use DelegateTemplateManager instead!")]
    public class DelegateTemplateResolver : ITemplateResolver
    {
        #region Fields
        private readonly Func<string, string> _resolver;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="DelegateTemplateResolver"/>.
        /// </summary>
        /// <param name="resolver">The resolver delegate.</param>
        public DelegateTemplateResolver(Func<string, string> resolver)
        {
            Contract.Requires(resolver != null);

            _resolver = resolver;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resolves a template.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Resolve(string name)
        {
            return _resolver(name);
        }
        #endregion
    }
}