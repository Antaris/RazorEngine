using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace RazorEngine.Configuration.Xml
{
    /// <summary>
    /// This is a simple wrapper around an <see cref="ITemplateResolver"/> to provide
    /// an <see cref="ITemplateManager"/> service.
    /// </summary>
    [Obsolete("Use TemplateManager instead, this api is provided for backwards compatibility.")]
    public class WrapperTemplateManager : ITemplateManager
    {
        #region Fields
        private readonly ITemplateResolver _resolver;

        private readonly System.Collections.Concurrent.ConcurrentDictionary<ITemplateKey, ITemplateSource> _dynamicTemplates =
            new System.Collections.Concurrent.ConcurrentDictionary<ITemplateKey, ITemplateSource>();
        #endregion
        

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="DelegateTemplateResolver"/>.
        /// </summary>
        /// <param name="resolver">The resolver delegate.</param>
        public WrapperTemplateManager(ITemplateResolver resolver)
        {
            Contract.Requires(resolver != null);

            _resolver = resolver;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resolves the template content with the specified key.
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
            var templateString = _resolver.Resolve(key.Name);
            return new LoadedTemplateSource(templateString);
        }

        /// <summary>
        /// Adds a template dynamically.
        /// </summary>
        /// <param name="key">the key of the template</param>
        /// <param name="source">the source of the template</param>
        public void AddDynamic(ITemplateKey key, ITemplateSource source)
        {
            _dynamicTemplates.AddOrUpdate(key, source, (k, oldSource) =>
            {
                if (oldSource.Template != source.Template)
                {
                    throw new InvalidOperationException("The same key was used for another template!");
                }
                return source;
            });
        }

        /// <summary>
        /// Gets the key for a template.
        /// See <see cref="ITemplateManager.GetKey"/>.
        /// </summary>
        /// <param name="name">name of the template</param>
        /// <param name="templateType">the type of the resolve-context</param>
        /// <param name="context">the context (ie. parent template).</param>
        /// <returns></returns>
        public ITemplateKey GetKey(string name, ResolveType templateType, ITemplateKey context)
        {
            return new NameOnlyTemplateKey(name, templateType, context);
        }
        #endregion
    }
}
