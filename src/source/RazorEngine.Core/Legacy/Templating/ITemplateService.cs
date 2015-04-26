namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Generic;

    using Configuration;
    using Text;

    /// <summary>
    /// Defines the required contract for implementing a template service.
    /// </summary>
    [Obsolete("Please use IRazorEngineService instead.")]
    public interface ITemplateService : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the template service configuration.
        /// </summary>
        ITemplateServiceConfiguration Configuration { get; }

        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        IEncodedStringFactory EncodedStringFactory { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a namespace that will be imported into the template.
        /// </summary>
        /// <param name="ns">The namespace to be imported.</param>
        void AddNamespace(string ns);

        /// <summary>
        /// Creates a new <see cref="ExecuteContext"/> used to tracking templates.
        /// </summary>
        /// <param name="viewBag">This parameter is ignored, set the Viewbag with template.SetData(null, viewBag)</param>
        /// <returns>The instance of <see cref="ExecuteContext"/></returns>
        ExecuteContext CreateExecuteContext(DynamicViewBag viewBag = null);

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="name">The name of the template.</param>
        void Compile(string razorTemplate, Type modelType, string name);

        /// <summary>
        /// Create a template from the given razor code.
        /// </summary>
        /// <param name="razorTemplate">the string template</param>
        /// <param name="templateType">the type of the template</param>
        /// <param name="model">the model.</param>
        /// <returns></returns>
        ITemplate CreateTemplate(string razorTemplate, Type templateType, object model);

        /// <summary>
        /// Create a sequence of templates
        /// </summary>
        /// <param name="razorTemplates">the templates</param>
        /// <param name="templateTypes">the types</param>
        /// <param name="models">the models</param>
        /// <param name="parallel">run in parallel?</param>
        /// <returns></returns>
        IEnumerable<ITemplate> CreateTemplates(IEnumerable<string> razorTemplates, IEnumerable<Type> templateTypes, IEnumerable<object> models, bool parallel = false);

        /// <summary>
        /// Backwards Compat
        /// </summary>
        /// <param name="razorTemplate"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        Type CreateTemplateType(string razorTemplate, Type modelType);

        /// <summary>
        /// Backwards Compat
        /// </summary>
        /// <param name="razorTemplates"></param>
        /// <param name="modelTypes"></param>
        /// <param name="parallel"></param>
        /// <returns></returns>
        IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, IEnumerable<Type> modelTypes, bool parallel = false);

        /// <summary>
        /// Get a given template (compiles the templates if not cached already)
        /// </summary>
        /// <param name="razorTemplate"></param>
        /// <param name="model"></param>
        /// <param name="cacheName"></param>
        /// <returns></returns>
        ITemplate GetTemplate(string razorTemplate, object model, string cacheName);

        /// <summary>
        /// See GetTemplate.
        /// </summary>
        /// <param name="razorTemplates"></param>
        /// <param name="models"></param>
        /// <param name="cacheNames"></param>
        /// <param name="parallel"></param>
        /// <returns></returns>
        IEnumerable<ITemplate> GetTemplates(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<string> cacheNames, bool parallel = false);

        /// <summary>
        /// Returns whether or not a template by the specified name has been created already.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <returns>Whether or not the template has been created.</returns>
        bool HasTemplate(string name);

        /// <summary>
        /// Remove a template by the specified name from the cache.
        /// </summary>
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <returns>Whether or not the template has been removed.</returns>
        bool RemoveTemplate(string cacheName);

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance or NULL if no model exists.</param>
        /// <param name="viewBag">The ViewBag initial contents or NULL for an initially empty ViewBag.</param>
        /// <param name="cacheName">The name of the template type in the cache or NULL if no caching is desired.</param>
        /// <returns>The string result of the template.</returns>
        string Parse(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName);

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <typeparam name="T">Type of the model. Used to find out the type of the model, if model is NULL</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance or NULL if no model exists.</param>
        /// <param name="viewBag">The ViewBag initial contents or NULL for an initially empty ViewBag.</param>
        /// <param name="cacheName">The name of the template type in the cache or NULL if no caching is desired.</param>
        /// <returns>The string result of the template.</returns>
        string Parse<T>(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName);

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to partse.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="viewBags"></param>
        /// <param name="cacheNames">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        /// <returns></returns>
        IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<DynamicViewBag> viewBags, IEnumerable<string> cacheNames, bool parallel);

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model for the template.</param>
        /// <returns>The resolved template.</returns>
        ITemplate Resolve(string name, object model);

        /// <summary>
        /// Runs the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model.</param>
        /// <param name="viewBag">the viewbag</param>
        /// <returns>The string result of the template.</returns>
        string Run(string name, object model, DynamicViewBag viewBag);

        /// <summary>
        /// Runs the specified name.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="viewBag">The viewbag.</param>
        /// <returns>The string result of the template.</returns>
        string Run(ITemplate template, DynamicViewBag viewBag);
        #endregion
    }
}