using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace RazorEngine.Configuration.Xml
{
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
        /// Resolves the template content with the specified name.
        /// </summary>
        /// <param name="name">The name of the template to resolve.</param>
        /// <returns>The template content.</returns>
        public ITemplateSource Resolve(ITemplateKey key)
        {
            ITemplateSource result;
            if (_dynamicTemplates.TryGetValue(key, out result))
            {
                return result;
            }
            var templateString = _resolver.Resolve(key.Name);
            return new TemplateSource(templateString, key.Name);
        }

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

        public ITemplateKey GetKey(string name, ResolveType templateType, ICompiledTemplate context)
        {
            return new NameOnlyTemplateKey(name, templateType, context);
        }
        #endregion
    }
}
