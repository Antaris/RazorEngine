//-----------------------------------------------------------------------------
// <copyright file="FluentTemplateServiceConfiguration.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Compilation;
    using Compilation.Inspectors;
    using Templating;
    using Text;

    /// <summary>
    /// Defines a fluent template service configuration
    /// </summary>
    public class FluentTemplateServiceConfiguration : ITemplateServiceConfiguration
    {
        #region Fields

        /// <summary>
        /// The Template Service Configuration
        /// </summary>
        private readonly TemplateServiceConfiguration innerConfig = new TemplateServiceConfiguration();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentTemplateServiceConfiguration"/> class.
        /// </summary>
        /// <param name="config">The delegate used to create the configuration.</param>
        public FluentTemplateServiceConfiguration(Action<IConfigurationBuilder> config)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(config != null);
            /* ReSharper restore InvocationIsSkipped */

            config(new FluentConfigurationBuilder(this.innerConfig));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the activator.
        /// </summary>
        public IActivator Activator
        {
            get { return this.innerConfig.Activator; }
        }

        /// <summary>
        /// Gets the base template type.
        /// </summary>
        public Type BaseTemplateType
        {
            get { return this.innerConfig.BaseTemplateType; }
        }

        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        public IEnumerable<ICodeInspector> CodeInspectors
        {
            get { return this.innerConfig.CodeInspectors; }
        }

        /// <summary>
        /// Gets the compiler service factory.
        /// </summary>
        public ICompilerServiceFactory CompilerServiceFactory
        {
            get { return this.innerConfig.CompilerServiceFactory; }
        }

        /// <summary>
        /// Gets a value indicating whether the template service is operating in debug mode.
        /// </summary>
        public bool Debug
        {
            get { return this.innerConfig.Debug; }
        }

        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory
        {
            get { return this.innerConfig.EncodedStringFactory; }
        }

        /// <summary>
        /// Gets the language.
        /// </summary>
        public Language Language
        {
            get { return this.innerConfig.Language; }
        }

        /// <summary>
        /// Gets the collection of namespaces.
        /// </summary>
        public ISet<string> Namespaces
        {
            get { return this.innerConfig.Namespaces; }
        }

        /// <summary>
        /// Gets the resolver.
        /// </summary>
        public ITemplateResolver Resolver
        {
            get { return this.innerConfig.Resolver; }
        }
        #endregion
    }
}