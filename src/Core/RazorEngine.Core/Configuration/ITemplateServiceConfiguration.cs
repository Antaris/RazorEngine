namespace RazorEngine.Configuration
{
    using System;
    using System.Collections.Generic;

    using Compilation;
    using Compilation.Inspectors;
    using Templating;
    using Text;

    /// <summary>
    /// Defines the required contract for implementing template service configuration.
    /// </summary>
    public interface ITemplateServiceConfiguration
    {
        #region Properties
        /// <summary>
        /// Gets the activator.
        /// </summary>
        IActivator Activator { get; }

        /// <summary>
        /// Gets or sets whether to allow missing properties on dynamic models.
        /// </summary>
        bool AllowMissingPropertiesOnDynamic { get; }

        /// <summary>
        /// Gets the base template type.
        /// </summary>
        Type BaseTemplateType { get; }

        /// <summary>
        /// Gets the code inspectors.
        /// </summary>
        IEnumerable<ICodeInspector> CodeInspectors { get; }

        /// <summary>
        /// Gets the compiler service factory.
        /// </summary>
        ICompilerServiceFactory CompilerServiceFactory { get; }

        /// <summary>
        /// Gets whether the template service is operating in debug mode.
        /// </summary>
        bool Debug { get; }

        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        IEncodedStringFactory EncodedStringFactory { get; }
            
        /// <summary>
        /// Gets the language.
        /// </summary>
        Language Language { get; }

        /// <summary>
        /// Gets the namespaces.
        /// </summary>
        ISet<string> Namespaces { get; }

        /// <summary>
        /// Gets the template resolver.
        /// </summary>
        ITemplateResolver Resolver { get; }
        #endregion
    }
}
