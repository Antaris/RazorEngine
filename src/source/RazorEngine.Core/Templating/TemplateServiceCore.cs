using System.Runtime.Remoting.Contexts;

namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    using Compilation;
    using Compilation.Inspectors;
    using Configuration;
    using Parallel;
    using Text;
    internal class TemplateServiceCore : ITemplateServiceCore
    {
        private readonly ITemplateServiceConfiguration _config;
        /// <summary>
        /// We need this for creating the templates.
        /// </summary>
        private readonly CachedTemplateService _cached;

        internal TemplateServiceCore(ITemplateServiceConfiguration config, CachedTemplateService cached)
        {
            Contract.Requires(config != null);
            Contract.Requires(config.TemplateManager != null);

            _config = config;
            _cached = cached;
        }

        /// <summary>
        /// Creates a new <see cref="ExecuteContext"/> for tracking templates.
        /// </summary>
        /// <param name="viewBag">The dynamic view bag.</param>
        /// <returns>The execute context.</returns>
        public virtual ExecuteContext CreateExecuteContext(DynamicViewBag viewBag = null)
        {
            var context = new ExecuteContext(new DynamicViewBag(viewBag));
            return context;
        }

        /// <summary>
        /// Gets the template service configuration.
        /// </summary>
        public ITemplateServiceConfiguration Configuration { get { return _config; } }


        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="cacheName">The name of the template type in the cache.</param>
        public ICompiledTemplate Compile(ITemplateKey razorTemplate, Type modelType)
        {
            Contract.Requires(razorTemplate != null);

            var source = Resolve(razorTemplate);
            Type type = CreateTemplateType(source.Template, modelType);
            return new CompiledTemplate(source, type, modelType);
        }
        
        /// <summary>
        /// Creates an instance of <see cref="ITemplate"/> from the specified string template.
        /// </summary>
        /// <param name="razorTemplate">
        /// The string template.
        /// If templateType is not NULL, this parameter may be NULL (unused).
        /// </param>
        /// <param name="templateType">
        /// The template type or NULL if the template type should be dynamically created.
        /// If razorTemplate is not NULL, this parameter may be NULL (unused).
        /// </param>
        /// <param name="model">The model instance or NULL if no model exists.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        [Pure]
        internal virtual ITemplate CreateTemplate(ICompiledTemplate template, object model)
        {
            var context = CreateInstanceContext(template.TemplateType);
            ITemplate instance = _config.Activator.CreateInstance(context);
            instance.InternalTemplateService = new InternalTemplateService(this, template);
            instance.TemplateService = new TemplateService(_cached);
            instance.CachedTemplateService = _cached;
            if (model != null)
            {
                instance.SetModel(model);
            }
            return instance;
        }

        internal static Type GetTypeFromModelObject(object model, Type modelType = null)
        {
            var actualModelType = (model == null) ? null : model.GetType();
            return modelType ?? actualModelType;
        }

        /// <summary>
        /// Creates a <see cref="Type"/> that can be used to instantiate an instance of a template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type or NULL if no model exists.</param>
        /// <returns>An instance of <see cref="Type"/>.</returns>
        [Pure]
        public virtual Type CreateTemplateType(string razorTemplate, Type modelType)
        {
            var context = new TypeContext
            {
                ModelType = (modelType == null) ? typeof(object) : modelType,
                TemplateContent = razorTemplate,
                TemplateType = (_config.BaseTemplateType) ?? typeof(TemplateBase<>)
            };

            foreach (string ns in _config.Namespaces)
                context.Namespaces.Add(ns);

            var service = _config
                .CompilerServiceFactory
                .CreateCompilerService(_config.Language);
            service.Debug = _config.Debug;
            service.CodeInspectors = _config.CodeInspectors ?? Enumerable.Empty<ICodeInspector>();
            service.ReferenceResolver = _config.ReferenceResolver ?? new Compilation.Resolver.UseCurrentAssembliesReferenceResolver();

            var result = service.CompileType(context);

            //_assemblies.Add(result.Item2);

            return result.Item1;
        }


        /// <summary>
        /// Runs the specified template and returns the result.
        /// </summary>
        /// <param name="template">The template to run.</param>
        /// <param name="viewBag">The ViewBag contents or NULL for an initially empty ViewBag.</param>
        /// <returns>The string result of the template.</returns>
        public void RunTemplate(ICompiledTemplate template, System.IO.TextWriter writer, object model, DynamicViewBag viewBag)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            var instance = CreateTemplate(template, model);
            instance.Run(CreateExecuteContext(viewBag), writer);
        }

        /// <summary>
        /// Creates a new <see cref="InstanceContext"/> for creating template instances.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <returns>An instance of <see cref="InstanceContext"/>.</returns>
        [Pure]
        protected internal virtual InstanceContext CreateInstanceContext(Type templateType)
        {
            return new InstanceContext(_config.CachingProvider.TypeLoader, templateType);
        }

        public ITemplateKey GetKey(string cacheName, ResolveType resolveType = ResolveType.Global, ICompiledTemplate context = null)
        {
            return _config.TemplateManager.GetKey(cacheName, resolveType, context);
        }

        internal virtual ITemplate ResolveInternal(string cacheName, object model, ResolveType resolveType, ICompiledTemplate context)
        {
            var templateKey = GetKey(cacheName, resolveType, context);
            var compiledTemplate = Compile(templateKey, GetTypeFromModelObject(model));
            return CreateTemplate(compiledTemplate, model);
        }

        internal ITemplateSource Resolve(ITemplateKey key)
        {
            return Configuration.TemplateManager.Resolve(key);
        }
    }

}