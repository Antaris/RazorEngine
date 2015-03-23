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
        private readonly RazorEngineCore _service;
        private readonly ITemplateKey _template;
        public InternalTemplateService(RazorEngineCore service, ITemplateKey template)
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
        /// <param name="name">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <param name="modelType"></param>
        /// <param name="viewbag"></param>
        /// <param name="resolveType"></param>
        /// <returns>The resolved template.</returns>
        public ITemplate Resolve(string name, object model, Type modelType, DynamicViewBag viewbag, ResolveType resolveType)
        {
            DynamicWrapperService.CheckModelType(modelType);
            return _service.ResolveInternal(name, DynamicWrapperService.GetDynamicModel(
                    modelType, model, _service.Configuration.AllowMissingPropertiesOnDynamic), modelType, viewbag, resolveType, _template);
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
        /// <returns>The execute context.</returns>
        public ExecuteContext CreateExecuteContext()
        {
            return _service.CreateExecuteContext();
        }
    }

}
