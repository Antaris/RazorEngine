namespace RazorEngine.Configuration
{
    using System;
    using System.Diagnostics.Contracts;

    using Compilation;
    using Compilation.Inspectors;
    using Templating;
    using Text;

    /// <summary>
    /// Provides a default implementation of a <see cref="IConfigurationBuilder"/>.
    /// </summary>
    internal class FluentConfigurationBuilder : IConfigurationBuilder
    {
        #region Fields
        private readonly TemplateServiceConfiguration _config;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="FluentConfigurationBuilder"/>.
        /// </summary>
        /// <param name="config">The default configuration that we build a new configuration from.</param>
        public FluentConfigurationBuilder(TemplateServiceConfiguration config)
        {
            Contract.Requires(config != null);

            _config = config;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the activator.
        /// </summary>
        /// <param name="activator">The activator instance.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ActivateUsing(IActivator activator)
        {
            if (activator == null)
                throw new ArgumentNullException("activator");

            _config.Activator = activator;
            return this;
        }

        /// <summary>
        /// Sets the activator.
        /// </summary>
        /// <typeparam name="TActivator">The activator type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ActivateUsing<TActivator>() where TActivator : IActivator, new()
        {
            return ActivateUsing(new TActivator());
        }

        /// <summary>
        /// Sets the activator.
        /// </summary>
        /// <param name="activator">The activator delegate.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ActivateUsing(Func<InstanceContext, ITemplate> activator)
        {
            if (activator == null)
                throw new ArgumentNullException("activator");

            _config.Activator = new DelegateActivator(activator);
            return this;
        }
        
#if !RAZOR4
        /// <summary>
        /// Adds the specified code inspector.
        /// </summary>
        /// <typeparam name="TInspector">The code inspector type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        [Obsolete("This API is obsolete and will be removed in the next version (Razor4 doesn't use CodeDom for code-generation)!")]
        public IConfigurationBuilder AddInspector<TInspector>() where TInspector : ICodeInspector, new()
        {
            return AddInspector(new TInspector());
        }

        /// <summary>
        /// Adds the specified code inspector.
        /// </summary>
        /// <param name="inspector">The code inspector.</param>
        /// <returns>The current configuration builder.</returns>
        [Obsolete("This API is obsolete and will be removed in the next version (Razor4 doesn't use CodeDom for code-generation)!")]
        public IConfigurationBuilder AddInspector(ICodeInspector inspector)
        {
            if (inspector == null)
                throw new ArgumentNullException("inspector");

            _config.CodeInspectors.Add(inspector);
            return this;
        }
#endif

        /// <summary>
        /// Sets that dynamic models should be fault tollerant in accepting missing properties.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder AllowMissingPropertiesOnDynamic()
        {
            _config.AllowMissingPropertiesOnDynamic = true;

            return this;
        }

        /// <summary>
        /// Sets the compiler service factory.
        /// </summary>
        /// <param name="factory">The compiler service factory.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder CompileUsing(ICompilerServiceFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            _config.CompilerServiceFactory = factory;
            return this;
        }

        /// <summary>
        /// Sets the compiler service factory.
        /// </summary>
        /// <typeparam name="TCompilerServiceFactory">The compiler service factory type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder CompileUsing<TCompilerServiceFactory>()
            where TCompilerServiceFactory : ICompilerServiceFactory, new()
        {
            return CompileUsing(new TCompilerServiceFactory());
        }

        /// <summary>
        /// Sets the encoded string factory.
        /// </summary>
        /// <param name="factory">The encoded string factory.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder EncodeUsing(IEncodedStringFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            _config.EncodedStringFactory = factory;
            return this;
        }

        /// <summary>
        /// Sets the encoded string factory.
        /// </summary>
        /// <typeparam name="TEncodedStringFactory">The encoded string factory type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder EncodeUsing<TEncodedStringFactory>()
            where TEncodedStringFactory : IEncodedStringFactory, new()
        {
            return EncodeUsing(new TEncodedStringFactory());
        }

        /// <summary>
        /// Includes the specified namespaces
        /// </summary>
        /// <param name="namespaces">The set of namespaces to include.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder IncludeNamespaces(params string[] namespaces)
        {
            if (namespaces == null)
                throw new ArgumentNullException("namespaces");

            foreach (string ns in namespaces)
                _config.Namespaces.Add(ns);

            return this;
        }

        /// <summary>
        /// Sets the resolve used to locate unknown templates.
        /// </summary>
        /// <typeparam name="TResolver">The resolve type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        [Obsolete("Please use the ManageUsing method instead.")]
        public IConfigurationBuilder ResolveUsing<TResolver>() where TResolver : ITemplateResolver, new()
        {
            _config.Resolver = new TResolver();
            return this;
        }

        /// <summary>
        /// Sets the resolve used to locate unknown templates.
        /// </summary>
        /// <typeparam name="TResolver">The resolve type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ManageUsing<TResolver>() where TResolver : ITemplateManager, new()
        {
            _config.TemplateManager = new TResolver();
            return this;
        }

        /// <summary>
        /// Sets the resolver used to locate unknown templates.
        /// </summary>
        /// <param name="resolver">The resolver instance to use.</param>
        /// <returns>The current configuration builder.</returns>
        [Obsolete("Please use the ManageUsing method instead.")]
        public IConfigurationBuilder ResolveUsing(ITemplateResolver resolver)
        {
            Contract.Requires(resolver != null);

            _config.Resolver = resolver;
            return this;
        }

        /// <summary>
        /// Sets the resolver used to locate unknown templates.
        /// </summary>
        /// <param name="resolver">The resolver instance to use.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ManageUsing(ITemplateManager resolver)
        {
            Contract.Requires(resolver != null);

            _config.TemplateManager = resolver;
            return this;
        }

        /// <summary>
        /// Sets the resolver delegate used to locate unknown templates.
        /// </summary>
        /// <param name="resolver">The resolver delegate to use.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ResolveUsing(Func<string, string> resolver)
        {
            Contract.Requires(resolver != null);

            _config.TemplateManager = new DelegateTemplateManager(resolver);
            return this;
        }

        /// <summary>
        /// Loads all dynamic assemblies with Assembly.Load(byte[]).
        /// This prevents temp files from being locked (which makes it impossible for RazorEngine to delete them).
        /// At the same time this completely shuts down any sandboxing/security.
        /// Use this only if you have a limited amount of static templates (no modifications on rumtime), 
        /// which you fully trust and when a seperate AppDomain is no solution for you!.
        /// This option will also hurt debugging.
        /// 
        /// OK, YOU HAVE BEEN WARNED.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder DisableTempFileLocking()
        {
            _config.DisableTempFileLocking = true;
            return this;
        }

        /// <summary>
        /// Sets the default activator.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder UseDefaultActivator()
        {
            _config.Activator = new DefaultActivator();
            return this;
        }

        /// <summary>
        /// Sets the default compiler service factory.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder UseDefaultCompilerServiceFactory()
        {
            _config.CompilerServiceFactory = new DefaultCompilerServiceFactory();
            return this;
        }

        /// <summary>
        /// Sets the default encoded string factory.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder UseDefaultEncodedStringFactory()
        {
            _config.EncodedStringFactory = new HtmlEncodedStringFactory();
            return this;
        }

        /// <summary>
        /// Sets the base template type.
        /// </summary>
        /// <param name="baseTemplateType">The base template type.</param>
        /// <returns>The current configuration builder/.</returns>
        public IConfigurationBuilder WithBaseTemplateType(Type baseTemplateType)
        {
            _config.BaseTemplateType = baseTemplateType;
            return this;
        }

        /// <summary>
        /// Sets the code language.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder WithCodeLanguage(Language language)
        {
            _config.Language = language;
            return this;
        }

        /// <summary>
        /// Sets the encoding.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder WithEncoding(Encoding encoding)
        {
            switch (encoding)
            {
                case Encoding.Html:
                    _config.EncodedStringFactory = new HtmlEncodedStringFactory();
                    break;
                case Encoding.Raw:
                    _config.EncodedStringFactory = new RawStringFactory();
                    break;
                default:
                    throw new ArgumentException("Unsupported encoding: " + encoding);
            }

            return this;
        }
        #endregion
    }
}