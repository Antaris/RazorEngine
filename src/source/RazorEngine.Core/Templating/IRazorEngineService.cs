namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Generic;

    using Configuration;
    using Text;
    using System.IO;


    /// <summary>
    /// Defines the required contract for implementing a template service.
    /// </summary>
    public interface IRazorEngineService : IDisposable
    {
        ITemplateKey GetKey(string name, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null);

        bool IsTemplateCached(ITemplateKey key, Type modelType);

        void AddTemplate(ITemplateKey key, ITemplateSource templateSource);

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="cacheName">The name of the template type in the cache.</param>
        void CompileAndCache(ITemplateKey key, Type modelType = null);


        void RunCompileOnDemand(ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null);

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
        void RunCachedTemplate(ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null);
    }

}