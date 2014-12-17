using RazorEngine.Configuration;
using System;
using System.IO;
namespace RazorEngine.Templating
{
    internal interface IRazorEngineCore
    {
        /// <summary>
        /// Gets the template service configuration.
        /// </summary>
        ITemplateServiceConfiguration Configuration { get; }

        ITemplateKey GetKey(string name, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null);

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="cacheName">The name of the template type in the cache.</param>
        ICompiledTemplate Compile(ITemplateKey key, Type modelType);

        /// <summary>
        /// Creates an instance of <see cref="ITemplate{T}"/> from the specified string template.
        /// </summary>
        /// <param name="razorTemplate">
        /// The string template.
        /// If templateType is not NULL (precompiled template), this parameter may be NULL (unused).
        /// </param>
        /// <param name="templateType">
        /// The template type or NULL if the template type should be dynamically created.
        /// If razorTemplate is not NULL, this parameter may be NULL (unused).
        /// </param>
        /// <param name="model">The model instance or NULL if no model exists.</param>
        /// <returns>An instance of <see cref="ITemplate{T}"/>.</returns>
        void RunTemplate(ICompiledTemplate template, TextWriter writer, object model, DynamicViewBag viewBag);
    }
}
