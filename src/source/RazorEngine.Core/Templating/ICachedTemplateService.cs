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
    public interface ICachedTemplateService : IDisposable
    {
        /// <summary>
        /// Gets the core service.
        /// </summary>
        ITemplateServiceCore Core { get; }
        
        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="cacheName">The name of the template type in the cache.</param>
        ICompiledTemplate CompileAndCache(ITemplateKey key, Type modelType);

        void RunCompileOnDemand(ITemplateKey key, Type modelType, TextWriter writer, object model, DynamicViewBag viewBag);

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
        void RunCachedTemplate(ITemplateKey key, Type modelType, TextWriter writer, object model, DynamicViewBag viewBag);
    }

}