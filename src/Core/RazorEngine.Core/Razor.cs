namespace RazorEngine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    using Templating;

    /// <summary>
    /// Provides quick access to template functions.
    /// </summary>
    public static class Razor
    {
        #region Fields
        private static ITemplateService _service = new TemplateService();
        private static readonly object _sync = new object();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the template service.
        /// </summary>
        private static ITemplateService TemplateService
        {
            get
            {
                lock (_sync) 
                    return _service;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="name">The name of the template.</param>
        public static void Compile(string razorTemplate, string name)
        {
            TemplateService.Compile(razorTemplate, name);
        }

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="name">The name of the template.</param>
        public static void Compile(string razorTemplate, Type modelType, string name)
        {
            TemplateService.Compile(razorTemplate, modelType, name);
        }

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="name">The name of the template.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "We already provide a non-generic alternative.")]
        public static void Compile<T>(string razorTemplate, string name)
        {
            TemplateService.Compile(razorTemplate, typeof(T), name);
        }

        /// <summary>
        /// Creates an instance of <see cref="ITemplate"/> from the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        public static ITemplate CreateTemplate(string razorTemplate)
        {
            return TemplateService.CreateTemplate(razorTemplate);
        }

        /// <summary>
        /// Creates an instance of <see cref="ITemplate{T}"/> from the specified string template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>An instance of <see cref="ITemplate{T}"/>.</returns>
        public static ITemplate CreateTemplate<T>(string razorTemplate, T model)
        {
            return TemplateService.CreateTemplate(razorTemplate, model);
        }

        /// <summary>
        /// Creates a set of templates from the specified string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="ITemplate"/> instances for.</param>
        /// <param name="parallel">Flag to determine whether to create templates in parallel.</param>
        /// <returns>The enumerable set of template instances.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<ITemplate> CreateTemplates(IEnumerable<string> razorTemplates, bool parallel = false)
        {
            return TemplateService.CreateTemplates(razorTemplates, parallel);
        }

        /// <summary>
        /// Creates a set of templates from the specified string templates and models.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of templates to create <see cref="ITemplate"/> instances for.</param>
        /// <param name="models">The set of models used to assign to templates.</param>
        /// <param name="parallel">Flag to determine whether to create templates in parallel.</param>
        /// <returns>The enumerable set of template instances.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<ITemplate> CreateTemplates<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, bool parallel = false)
        {
            return TemplateService.CreateTemplates(razorTemplates, models, parallel);
        }

        /// <summary>
        /// Creates a <see cref="Type"/> that can be used to instantiate an instance of a template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <returns>An instance of <see cref="Type"/>.</returns>
        public static Type CreateTemplateType(string razorTemplate)
        {
            return TemplateService.CreateTemplateType(razorTemplate);
        }

        /// <summary>
        /// Creates a <see cref="Type"/> that can be used to instantiate an instance of a template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <returns>An instance of <see cref="Type"/>.</returns>
        public static Type CreateTemplateType(string razorTemplate, Type modelType)
        {
            return TemplateService.CreateTemplateType(razorTemplate, modelType);
        }

        /// <summary>
        /// Crates a set of template types from the specfied string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="Type"/> instances for.</param>
        /// <param name="parallel">Flag to determine whether to create template types in parallel.</param>
        /// <returns>The set of <see cref="Type"/> instances.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, bool parallel = false)
        {
            return TemplateService.CreateTemplateTypes(razorTemplates, parallel);
        }

        /// <summary>
        /// Creates a set of template types from the specfied string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="Type"/> instances for.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="parallel">Flag to determine whether to create template types in parallel.</param>
        /// <returns>The set of <see cref="Type"/> instances.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, Type modelType, bool parallel = false)
        {
            return TemplateService.CreateTemplateTypes(razorTemplates, modelType, parallel);
        }

        /// <summary>
        /// Gets an instance of the template using the cached compiled type, or compiles the template type
        /// if it does not exist in the cache.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        public static ITemplate GetTemplate(string razorTemplate, string name)
        {
            return TemplateService.GetTemplate(razorTemplate, name);
        }

        /// <summary>
        /// Gets an instance of the template using the cached compiled type, or compiles the template type
        /// if it does not exist in the cache.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>An instance of <see cref="ITemplate{T}"/>.</returns>
        public static ITemplate GetTemplate<T>(string razorTemplate, T model, string name)
        {
            return TemplateService.GetTemplate(razorTemplate, model, name);
        }

        /// <summary>
        /// Gets the set of template instances for the specified string templates. Cached templates will be considered
        /// and if they do not exist, new types will be created and instantiated.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether to get the templates in parallel.</param>
        /// <returns>The set of <see cref="ITemplate"/> instances.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<ITemplate> GetTemplates(IEnumerable<string> razorTemplates, IEnumerable<string> names, bool parallel = false)
        {
            return TemplateService.GetTemplates(razorTemplates, names, parallel);
        }

        /// <summary>
        /// Gets the set of template instances for the specified string templates. Cached templates will be considered
        /// and if they do not exist, new types will be created and instantiated.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of templates to create.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether to get the templates in parallel.</param>
        /// <returns>The set of <see cref="ITemplate"/> instances.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<ITemplate> GetTemplates<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, IEnumerable<string> names, bool parallel = false)
        {
            return TemplateService.GetTemplates(razorTemplates, models, names, parallel);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <returns>The string result of the template.</returns>
        public static string Parse(string razorTemplate)
        {
            return TemplateService.Parse(razorTemplate);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template. 
        /// This method will provide a cache check to see if the compiled template type already exists and is valid.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="name">The name of the cached template type.</param>
        /// <returns>The string result of the template.</returns>
        public static string Parse(string razorTemplate, string name)
        {
            return TemplateService.Parse(razorTemplate, name);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>The string result of the template.</returns>
        public static string Parse(string razorTemplate, object model)
        {
            return TemplateService.Parse(razorTemplate, model);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>The string result of the template.</returns>
        public static string Parse<T>(string razorTemplate, T model)
        {
            return TemplateService.Parse(razorTemplate, model);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>The string result of the template.</returns>
        public static string Parse<T>(string razorTemplate, T model, string name)
        {
            return TemplateService.Parse(razorTemplate, model, name);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>The string result of the template.</returns>
        public static string Parse(string razorTemplate, object model, string name)
        {
            return TemplateService.Parse(razorTemplate, model, name);
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to partse.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, bool parallel = false)
        {
            return TemplateService.ParseMany(razorTemplates, parallel);
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to partse.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, IEnumerable<string> names, bool parallel = false)
        {
            return TemplateService.ParseMany(razorTemplates, names, parallel);
        }

        /// <summary>
        /// Parses the template and merges with the many models provided.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The razor template.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<string> ParseMany<T>(string razorTemplate, IEnumerable<T> models, bool parallel = false)
        {
            return TemplateService.ParseMany(razorTemplate, models, parallel);
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of string templates to partse.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<string> ParseMany<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, bool parallel = false)
        {
            return TemplateService.ParseMany(razorTemplates, models, parallel);
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of string templates to partse.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IEnumerable<string> ParseMany<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, IEnumerable<string> names, bool parallel = false)
        {
            return TemplateService.ParseMany(razorTemplates, models, names, parallel);
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <returns>The resolved template.</returns>
        public static ITemplate Resolve(string name)
        {
            return TemplateService.Resolve(name);
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model for the template.</param>
        /// <returns>The resolved template.</returns>
        public static ITemplate Resolve(string name, object model)
        {
            return TemplateService.Resolve(name, model);
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model for the template.</param>
        /// <returns>The resolved template.</returns>
        public static ITemplate Resolve<T>(string name, T model)
        {
            return TemplateService.Resolve(name, model);
        }

        /// <summary>
        /// Runs the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <returns>The string result of the template.</returns>
        public static string Run(string name)
        {
            return TemplateService.Run(name);
        }

        /// <summary>
        /// Runs the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model.</param>
        /// <returns>The string result of the template.</returns>
        public static string Run(string name, object model)
        {
            return TemplateService.Run(name, model);
        }

        /// <summary>
        /// Runs the template with the specified name.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model.</param>
        /// <returns>The string result of the template.</returns>
        public static string Run<T>(string name, T model)
        {
            return TemplateService.Run(name, model);
        }

        /// <summary>
        /// Sets the template service.
        /// </summary>
        /// <param name="service">The template service.</param>
        public static void SetTemplateService(ITemplateService service)
        {
            Contract.Requires(service != null);

            lock (_sync)
                _service = service;
        }
        #endregion
    }
}