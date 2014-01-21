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
        private readonly TemplateServiceConfiguration _innerConfig = new TemplateServiceConfiguration();
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="FluentTemplateServiceConfiguration"/>.
        /// </summary>
        /// <param name="config">The delegate used to create the configuration.</param>
        public FluentTemplateServiceConfiguration(Action<IConfigurationBuilder> config)
        {
            Contract.Requires(config != null);

            config(new FluentConfigurationBuilder(_innerConfig));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the activator.
        /// </summary>
        public IActivator Activator
        {
            get { return _innerConfig.Activator; }
        }
        
        /// <summary>
        /// Gets or sets whether to allow missing properties on dynamic models.
        /// </summary>
        public bool AllowMissingPropertiesOnDynamic
        {
            get { return _innerConfig.AllowMissingPropertiesOnDynamic; }
        }

        /// <summary>
        /// Gets the base template type.
        /// </summary>
        public Type BaseTemplateType
        {
            get { return _innerConfig.BaseTemplateType; }
        }

        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        public IEnumerable<ICodeInspector> CodeInspectors
        {
            get { return _innerConfig.CodeInspectors; }
        }

        /// <summary>
        /// Gets or sets the compiler service factory.
        /// </summary>
        public ICompilerServiceFactory CompilerServiceFactory
        {
            get { return _innerConfig.CompilerServiceFactory; }
        }

        /// <summary>
        /// Gets whether the template service is operating in debug mode.
        /// </summary>
        public bool Debug
        {
            get { return _innerConfig.Debug; }
        }

        /// <summary>
        /// Gets or sets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory
        {
            get { return _innerConfig.EncodedStringFactory; }
        }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public Language Language
        {
            get { return _innerConfig.Language; }
        }

        /// <summary>
        /// Gets or sets the collection of namespaces.
        /// </summary>
        public ISet<string> Namespaces
        {
            get { return _innerConfig.Namespaces; }
        }

        /// <summary>
        /// Gets the resolver.
        /// </summary>
        public ITemplateResolver Resolver
        {
            get { return _innerConfig.Resolver; }
        }
        #endregion
    }
}