﻿//-----------------------------------------------------------------------------
// <copyright file="TemplateServiceConfiguration.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Configuration
{
    using System;
    using System.Collections.Generic;
    using Compilation;
    using Compilation.Inspectors;
    using Templating;
    using Text;

    /// <summary>
    /// Provides a default implementation of a template service configuration.
    /// </summary>
    public class TemplateServiceConfiguration : ITemplateServiceConfiguration
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateServiceConfiguration"/> class.
        /// </summary>
        public TemplateServiceConfiguration()
        {
            this.Activator = new DefaultActivator();
            this.CompilerServiceFactory = new DefaultCompilerServiceFactory();
            this.EncodedStringFactory = new HtmlEncodedStringFactory();
            this.CodeInspectors = new List<ICodeInspector>();

            this.Namespaces = new HashSet<string>
                             {
                                 "System", 
                                 "System.Collections.Generic", 
                                 "System.Linq"
                             };

            var config = RazorEngineConfigurationSection.GetConfiguration();
            Language = (config == null)
                           ? Language.CSharp
                           : config.DefaultLanguage;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the activator.
        /// </summary>
        public IActivator Activator { get; set; }

        /// <summary>
        /// Gets or sets the base template type.
        /// </summary>
        public Type BaseTemplateType { get; set; }

        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        IEnumerable<ICodeInspector> ITemplateServiceConfiguration.CodeInspectors
        {
            get
            {
                return this.CodeInspectors;
            }
        }

        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        public IList<ICodeInspector> CodeInspectors { get; private set; }

        /// <summary>
        /// Gets or sets the compiler service factory.
        /// </summary>
        public ICompilerServiceFactory CompilerServiceFactory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the template service is operating in debug mode.
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Gets or sets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// Gets or sets the collection of namespaces.
        /// </summary>
        public ISet<string> Namespaces { get; set; }

        /// <summary>
        /// Gets or sets the template resolver.
        /// </summary>
        public ITemplateResolver Resolver { get; set; }
        #endregion
    }
}