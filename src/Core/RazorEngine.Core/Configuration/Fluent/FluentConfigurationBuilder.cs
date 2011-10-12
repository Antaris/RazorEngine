namespace RazorEngine.Configuration
{
    using System;
    using System.Diagnostics.Contracts;

    using Compilation;
    using Templating;
    using Text;

    /// <summary>
    /// Provides a default implementation of a <see cref="IConfigurationBuilder"/>.
    /// </summary>
    internal class FluentConfigurationBuilder : IConfigurationBuilder
    {
        #region Fields
        private readonly DefaultTemplateServiceConfiguration _config;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="FluentConfigurationBuilder"/>.
        /// </summary>
        /// <param name="config">The default configuration that we build a new configuration from.</param>
        public FluentConfigurationBuilder(DefaultTemplateServiceConfiguration config)
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
            Contract.Requires(activator != null);

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
            Contract.Requires(activator != null);

            _config.Activator = new DelegateActivator(activator);
            return this;
        }

        /// <summary>
        /// Sets the compiler service factory.
        /// </summary>
        /// <param name="factory">The compiler service factory.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder CompileUsing(ICompilerServiceFactory factory)
        {
            Contract.Requires(factory != null);

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
            Contract.Requires(factory != null);

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
            Contract.Requires(namespaces != null);

            foreach (string ns in namespaces)
                _config.Namespaces.Add(ns);

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