using RazorEngine.Configuration;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    internal class InternalTemplateService : IInternalTemplateService
    {
        private readonly TemplateServiceCore _service;
        private readonly ICompiledTemplate _template;
        public InternalTemplateService(TemplateServiceCore service, ICompiledTemplate template)
        {
            Contract.Requires(service != null);
            Contract.Requires(template != null);

            _service = service;
            _template = template;
        }

        public ITemplateServiceConfiguration Configuration
        {
            get { return _service.Configuration; }
        }

        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory
        {
            get
            {
                return _service.Configuration.EncodedStringFactory;
            }
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <returns>The resolved template.</returns>
        public ITemplate Resolve(string cacheName, object model, ResolveType resolveType)
        {
            return _service.ResolveInternal(cacheName, model, resolveType, _template);
        }

        /// <summary>
        /// Adds a namespace that will be imported into the template.
        /// </summary>
        /// <param name="ns">The namespace to be imported.</param>
        public void AddNamespace(string ns)
        {
            Configuration.Namespaces.Add(ns);
        }

        /// <summary>
        /// Creates a new <see cref="ExecuteContext"/> for tracking templates.
        /// </summary>
        /// <param name="viewBag">The dynamic view bag.</param>
        /// <returns>The execute context.</returns>
        public ExecuteContext CreateExecuteContext(DynamicViewBag viewBag = null)
        {
            return _service.CreateExecuteContext(viewBag);
        }
    }

}
