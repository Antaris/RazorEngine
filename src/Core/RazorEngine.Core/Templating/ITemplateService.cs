namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Generic;

    using Configuration;
    using Text;

    /// <summary>
    /// Defines the required contract for implementing a template service.
    /// </summary>
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
        /// <param name="viewBag">The view bag.</param>
        /// <returns>The instance of <see cref="ExecuteContext"/></returns>
        ExecuteContext CreateExecuteContext(DynamicViewBag viewBag = null);

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="cacheName">The name of the template type in the cache.</param>
        void Compile(string razorTemplate, Type modelType, string cacheName);

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
        ITemplate CreateTemplate(string razorTemplate, Type templateType, object model);

        /// <summary>
        /// Creates a set of templates from the specified string templates.
        /// </summary>
        /// <param name="razorTemplates">
        /// The set of templates to create or NULL if all template types are already created (see templateTypes).
        /// If this parameter is NULL, the the templateTypes parameter may not be NULL. 
        /// Individual elements in this set may be NULL if the corresponding templateTypes[i] is not NULL (precompiled template).
        /// </param>
        /// <param name="models">
        /// The set of models or NULL if no models exist for all templates.
        /// Individual elements in this set may be NULL if no model exists for a specific template.
        /// </param>
        /// <param name="templateTypes">
        /// The set of template types or NULL to dynamically create template types for each template.
        /// If this parameter is NULL, the the razorTemplates parameter may not be NULL. 
        /// Individual elements in this set may be NULL to dynamically create the template if the corresponding razorTemplates[i] is not NULL (dynamically compile template).
        /// </param>
        /// <param name="parallel">Flag to determine whether to create templates in parallel.</param>
        /// <returns>The enumerable set of template instances.</returns>
        IEnumerable<ITemplate> CreateTemplates(IEnumerable<string> razorTemplates, IEnumerable<Type> templateTypes, IEnumerable<object> models, bool parallel = false);

        /// <summary>
        /// Creates a <see cref="Type"/> that can be used to instantiate an instance of a template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type or NULL if no model exists.</param>
        /// <returns>An instance of <see cref="Type"/>.</returns>
        Type CreateTemplateType(string razorTemplate, Type modelType);

        /// <summary>
        /// Creates a set of template types from the specfied string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="Type"/> instances for.</param>
        /// <param name="modelTypes">
        /// The set of model types or NULL if no models exist for all templates.
        /// Individual elements in this set may be NULL if no model exists for a specific template.
        /// </param>
        /// <param name="parallel">Flag to determine whether to create template types in parallel.</param>
        /// <returns>The set of <see cref="Type"/> instances.</returns>
        IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, IEnumerable<Type> modelTypes, bool parallel = false);

        /// <summary>
        /// Gets an instance of the template using the cached compiled type, or compiles the template type
        /// if it does not exist in the cache.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model or NULL if there is no model for this template.</param>
        /// <param name="cacheName">The name of the template type in the cache.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        ITemplate GetTemplate(string razorTemplate, object model, string cacheName);

        /// <summary>
        /// Gets the set of template instances for the specified string templates. Cached templates will be considered
        /// and if they do not exist, new types will be created and instantiated.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create.</param>
        /// <param name="models">
        /// The set of models or NULL if no models exist for all templates.
        /// Individual elements in this set may be NULL if no model exists for a specific template.
        /// </param>
        /// <param name="cacheNames">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether to get the templates in parallel.</param>
        /// <returns>The set of <see cref="ITemplate"/> instances.</returns>
        IEnumerable<ITemplate> GetTemplates(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<string> cacheNames, bool parallel = false);

        /// <summary>
        /// Returns whether or not a template by the specified name has been created already.
        /// </summary>
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <returns>Whether or not the template has been created.</returns>
        bool HasTemplate(string cacheName);

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
        /// <param name="viewBag">The ViewBag contents or NULL for an initially empty ViewBag.</param>
        /// <param name="cacheName">The name of the template type in the cache or NULL if no caching is desired.</param>
        /// <returns>The string result of the template.</returns>
        string Parse(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName);

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <typeparam name="T">Type of the model. Used to find out the type of the model, if model is NULL</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance or NULL if no model exists.</param>
        /// <param name="viewBag">The ViewBag contents or NULL for an initially empty ViewBag.</param>
        /// <param name="cacheName">The name of the template type in the cache or NULL if no caching is desired.</param>
        /// <returns>The string result of the template.</returns>
        string Parse<T>(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName);

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to parse.</param>
        /// <param name="models">
        /// The set of models or NULL if no models exist for all templates.
        /// Individual elements in this set may be NULL if no model exists for a specific template.
        /// </param>
        /// <param name="viewBags">
        /// The set of initial ViewBag contents or NULL for an initially empty ViewBag for all templates.
        /// Individual elements in this set may be NULL if an initially empty ViewBag is desired for a specific template.
        /// </param>
        /// <param name="cacheNames">
        /// The set of cache names or NULL if no caching is desired for templates.
        /// Individual elements in this set may be NULL if caching is not desired for a specific template.
        /// </param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<DynamicViewBag> viewBags, IEnumerable<string> cacheNames, bool parallel);

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <param name="model">The model for the template.</param>
        /// <returns>The resolved template.</returns>
        ITemplate Resolve(string cacheName, object model);

        /// <summary>
        /// Runs the template with the specified cacheName.
        /// </summary>
        /// <param name="cacheName">The name of the template in cache.  The template must be in cache.</param>
        /// <param name="model">The model for the template or NULL if there is no model.</param>
        /// <param name="viewBag">The initial ViewBag contents NULL for an empty ViewBag.</param>
        /// <returns>The string result of the template.</returns>
        string Run(string cacheName, object model, DynamicViewBag viewBag);

        /// <summary>
        /// Runs the specified template.
        /// </summary>
        /// <param name="template">The template to run.</param>
        /// <param name="viewBag">The ViewBag contents or NULL for an initially empty ViewBag.</param>
        /// <returns>The string result of the template.</returns>
        string Run(ITemplate template, DynamicViewBag viewBag);

        #endregion
    }
}