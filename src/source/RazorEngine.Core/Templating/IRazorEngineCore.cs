using RazorEngine.Configuration;
using System;
using System.IO;
namespace RazorEngine.Templating
{
#if DISABLED
    /// <summary>
    /// Defines an internal contract for compiling and running templates without caching.
    /// </summary>
    internal interface IRazorEngineCore
    {
        /// <summary>
        /// Gets the template service configuration.
        /// </summary>
        ITemplateServiceConfiguration Configuration { get; }

        /// <summary>
        /// Gets a given key from the <see cref="ITemplateManager"/> implementation.
        /// See <see cref="ITemplateManager.GetKey"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resolveType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        ITemplateKey GetKey(string name, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null);

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="key">The string template key.</param>
        /// <param name="modelType">The model type.</param>
        ICompiledTemplate Compile(ITemplateKey key, Type modelType);

        /// <summary>
        /// Runs the given compiled template.
        /// </summary>
        /// <param name="template">The compiled template.</param>
        /// <param name="writer"></param>
        /// <param name="model">The model instance or NULL if no model exists.</param>
        /// <param name="viewBag"></param>
        void RunTemplate(ICompiledTemplate template, TextWriter writer, object model, DynamicViewBag viewBag);
    }
#endif
}
