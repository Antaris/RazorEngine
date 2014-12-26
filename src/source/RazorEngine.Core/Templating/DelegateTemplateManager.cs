namespace RazorEngine.Templating
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Provides an <see cref="ITemplateManager"/> that supports delegated template resolution.
    /// </summary>
    public class DelegateTemplateManager : ITemplateManager
    {
        #region Fields
        private readonly Func<string, string> _resolver;

        private readonly System.Collections.Concurrent.ConcurrentDictionary<ITemplateKey, ITemplateSource> _dynamicTemplates =
            new System.Collections.Concurrent.ConcurrentDictionary<ITemplateKey, ITemplateSource>();
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="DelegateTemplateResolver"/>.
        /// </summary>
        /// <param name="resolver">The resolver delegate.</param>
        public DelegateTemplateManager(Func<string, string> resolver)
        {
            Contract.Requires(resolver != null);

            _resolver = resolver;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resolves the template content with the specified name.
        /// </summary>
        /// <param name="key">The key of the template to resolve.</param>
        /// <returns>The template content.</returns>
        public ITemplateSource Resolve(ITemplateKey key)
        {
            ITemplateSource result;
            if (_dynamicTemplates.TryGetValue(key, out result))
            {
                return result;
            }
            var templateString = _resolver(key.Name);
            return new LoadedTemplateSource(templateString);
        }

        /// <summary>
        /// Dynamically add a new template.
        /// </summary>
        /// <param name="key">the key of the template</param>
        /// <param name="source">the source-code of the template</param>
        public void AddDynamic(ITemplateKey key, ITemplateSource source)
        {
            _dynamicTemplates.AddOrUpdate(key, source, (k, oldSource) =>
            {
                if (oldSource.Template != source.Template)
                {
                    throw new InvalidOperationException("The same key was already used for another template!");
                }
                return source;
            });
        }

        /// <summary>
        /// Creates a template-key instance (see also <see cref="ITemplateManager.GetKey"/>).
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="templateType">the type of the resolve context.</param>
        /// <param name="context">The context of the template (ie parent template).</param>
        /// <returns>The template-key.</returns>
        public ITemplateKey GetKey(string name, ResolveType templateType, ITemplateKey context)
        {
            return new NameOnlyTemplateKey(name, templateType, context);
        }
        #endregion
    }
}