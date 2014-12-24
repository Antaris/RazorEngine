namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Generic;

    using Configuration;
    using Text;
    using System.IO;


    /// <summary>
    /// Defines the required contract for implementing a template service.
    /// The main API for running templates.
    /// </summary>
    public interface IRazorEngineService : IDisposable
    {
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
        /// Checks if a given template is already cached.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        bool IsTemplateCached(ITemplateKey key, Type modelType);

        /// <summary>
        /// Adds a given template to the template manager as dynamic template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="templateSource"></param>
        void AddTemplate(ITemplateKey key, ITemplateSource templateSource);

        /// <summary>
        /// Compiles the specified template and caches it.
        /// </summary>
        /// <param name="key">The key of the template.</param>
        /// <param name="modelType">The model type.</param>
        void Compile(ITemplateKey key, Type modelType = null);

        /// <summary>
        /// Runs the given cached template.
        /// When the cache does not contain the template 
        /// it will be compiled and cached beforehand.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        void RunCompile(ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null);

        /// <summary>
        /// Runs the given cached template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        void Run(ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null);
    }

}