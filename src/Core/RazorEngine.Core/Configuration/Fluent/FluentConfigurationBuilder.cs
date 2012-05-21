//-----------------------------------------------------------------------------
// <copyright file="FluentConfigurationBuilder.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
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

        /// <summary>
        /// The Template Service configuration
        /// </summary>
        private readonly TemplateServiceConfiguration configuration;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentConfigurationBuilder"/> class.
        /// </summary>
        /// <param name="config">The default configuration that we build a new configuration from.</param>
        public FluentConfigurationBuilder(TemplateServiceConfiguration config)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(config != null);
            /* ReSharper restore InvocationIsSkipped */

            this.configuration = config;
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
            {
                throw new ArgumentNullException("activator");
            }

            this.configuration.Activator = activator;

            return this;
        }

        /// <summary>
        /// Sets the activator.
        /// </summary>
        /// <typeparam name="TActivator">The activator type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ActivateUsing<TActivator>() where TActivator : IActivator, new()
        {
            return this.ActivateUsing(new TActivator());
        }

        /// <summary>
        /// Sets the activator.
        /// </summary>
        /// <param name="activator">The activator delegate.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ActivateUsing(Func<InstanceContext, ITemplate> activator)
        {
            if (activator == null)
            {
                throw new ArgumentNullException("activator");
            }

            this.configuration.Activator = new DelegateActivator(activator);

            return this;
        }

        /// <summary>
        /// Adds the specified code inspector.
        /// </summary>
        /// <typeparam name="TInspector">The code inspector type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder AddInspector<TInspector>() where TInspector : ICodeInspector, new()
        {
            return AddInspector(new TInspector());
        }

        /// <summary>
        /// Adds the specified code inspector.
        /// </summary>
        /// <param name="inspector">The code inspector.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder AddInspector(ICodeInspector inspector)
        {
            if (inspector == null)
            {
                throw new ArgumentNullException("inspector");
            }

            this.configuration.CodeInspectors.Add(inspector);

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
            {
                throw new ArgumentNullException("factory");
            }

            this.configuration.CompilerServiceFactory = factory;

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
            return this.CompileUsing(new TCompilerServiceFactory());
        }

        /// <summary>
        /// Sets the encoded string factory.
        /// </summary>
        /// <param name="factory">The encoded string factory.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder EncodeUsing(IEncodedStringFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            this.configuration.EncodedStringFactory = factory;

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
            return this.EncodeUsing(new TEncodedStringFactory());
        }

        /// <summary>
        /// Includes the specified namespaces
        /// </summary>
        /// <param name="namespaces">The set of namespaces to include.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder IncludeNamespaces(params string[] namespaces)
        {
            if (namespaces == null)
            {
                throw new ArgumentNullException("namespaces");
            }

            foreach (string ns in namespaces)
            {
                this.configuration.Namespaces.Add(ns);
            }

            return this;
        }

        /// <summary>
        /// Sets the resolve used to locate unknown templates.
        /// </summary>
        /// <typeparam name="TResolver">The resolve type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ResolveUsing<TResolver>() where TResolver : ITemplateResolver, new()
        {
            this.configuration.Resolver = new TResolver();

            return this;
        }

        /// <summary>
        /// Sets the resolver used to locate unknown templates.
        /// </summary>
        /// <param name="resolver">The resolver instance to use.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ResolveUsing(ITemplateResolver resolver)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(resolver != null);
            /* ReSharper restore InvocationIsSkipped */

            this.configuration.Resolver = resolver;

            return this;
        }

        /// <summary>
        /// Sets the resolver delegate used to locate unknown templates.
        /// </summary>
        /// <param name="resolver">The resolver delegate to use.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder ResolveUsing(Func<string, string> resolver)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(resolver != null);
            /* ReSharper restore InvocationIsSkipped */

            this.configuration.Resolver = new DelegateTemplateResolver(resolver);

            return this;
        }

        /// <summary>
        /// Sets the default activator.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder UseDefaultActivator()
        {
            this.configuration.Activator = new DefaultActivator();

            return this;
        }

        /// <summary>
        /// Sets the default compiler service factory.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder UseDefaultCompilerServiceFactory()
        {
            this.configuration.CompilerServiceFactory = new DefaultCompilerServiceFactory();

            return this;
        }

        /// <summary>
        /// Sets the default encoded string factory.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder UseDefaultEncodedStringFactory()
        {
            this.configuration.EncodedStringFactory = new HtmlEncodedStringFactory();

            return this;
        }

        /// <summary>
        /// Sets the base template type.
        /// </summary>
        /// <param name="baseTemplateType">The base template type.</param>
        /// <returns>The current configuration builder/.</returns>
        public IConfigurationBuilder WithBaseTemplateType(Type baseTemplateType)
        {
            this.configuration.BaseTemplateType = baseTemplateType;

            return this;
        }

        /// <summary>
        /// Sets the code language.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <returns>The current configuration builder.</returns>
        public IConfigurationBuilder WithCodeLanguage(Language language)
        {
            this.configuration.Language = language;

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
                    this.configuration.EncodedStringFactory = new HtmlEncodedStringFactory();
                    break;
                case Encoding.Raw:
                    this.configuration.EncodedStringFactory = new RawStringFactory();
                    break;
                default:
                    throw new ArgumentException("Unsupported encoding: " + encoding);
            }

            return this;
        }
        #endregion
    }
}