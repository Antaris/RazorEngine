﻿namespace RazorEngine.Configuration
{
    using System;
    using System.Configuration;
    using Xml;

    /// <summary>
    /// Defines the main configuration section for the RazorEngine.
    /// </summary>
    public class RazorEngineConfigurationSection : ConfigurationSection
    {
        #region Fields
        private const string ActivatorAttribute = "activatorType";
        private const string AllowMissingPropertiesOnDynamicAttribute = "allowMissingPropertiesOnDynamic";
        private const string DisableTempFileLockingAttribute = "disableTempFileLocking";
        private const string CompilerServiceFactoryAttribute = "compilerServiceFactoryType";
        private const string DefaultLanguageAttribute = "defaultLanguage";
        private const string NamespacesElement = "namespaces";
        private const string SectionPath = "razorEngine";
        private const string TemplateResolverAttribute = "templateResolverType";
        private const string TemplateManagerAttribute = "templateManagerType";
        private const string CachingProviderAttribute = "cachingProviderType";
        private const string ReferenceResolverAttribute = "referenceResolverType";
        private const string TemplateServicesElement = "templateServices";
        private const string TemporaryDirectoryAttribute = "temporaryDirectory";
        #endregion

        #region Properties
        /// <summary>
        /// Gets the activator type.
        /// </summary>
        [ConfigurationProperty(ActivatorAttribute, IsRequired = false)]
        public string ActivatorType
        {
            get { return (string)this[ActivatorAttribute]; }
        }

        /// <summary>
        /// Gets or sets whether to allow missing properties on dynamic models.
        /// </summary>
        [ConfigurationProperty(AllowMissingPropertiesOnDynamicAttribute, IsRequired = false, DefaultValue = false)]
        public bool AllowMissingPropertiesOnDynamic
        {
            get { return (bool)this[AllowMissingPropertiesOnDynamicAttribute]; }
        }

        /// <summary>
        /// Gets or sets whether to allow missing properties on dynamic models.
        /// </summary>
        [ConfigurationProperty(DisableTempFileLockingAttribute, IsRequired = false, DefaultValue = false)]
        public bool DisableTempFileLocking
        {
            get { return (bool)this[DisableTempFileLockingAttribute]; }
        }

        /// <summary>
        /// Gets the compiler service factory type.
        /// </summary>
        [ConfigurationProperty(CompilerServiceFactoryAttribute, IsRequired = false)]
        public string CompilerServiceFactoryType
        {
            get { return (string)this[CompilerServiceFactoryAttribute]; }
        }

        /// <summary>
        /// Gets the compiler service factory type.
        /// </summary>
        [ConfigurationProperty(ReferenceResolverAttribute, IsRequired = false)]
        public string ReferenceResolverType
        {
            get { return (string)this[ReferenceResolverAttribute]; }
        }

        /// <summary>
        /// Gets or sets the default language.
        /// </summary>
        [ConfigurationProperty(DefaultLanguageAttribute, DefaultValue = Language.CSharp, IsRequired = false)]
        public Language DefaultLanguage
        {
            get { return (Language)this[DefaultLanguageAttribute]; }
            set { this[DefaultLanguageAttribute] = value; }
        }

        /// <summary>
        /// Gets the collection of namespaces.
        /// </summary>
        [ConfigurationProperty(NamespacesElement, IsRequired = false)]
        public NamespaceConfigurationElementCollection Namespaces
        {
            get { return (NamespaceConfigurationElementCollection)this[NamespacesElement]; }
        }

        /// <summary>
        /// Gets the template resolver type.
        /// </summary>
        [Obsolete("Please use the TemplateManagerType property instead")]
        [ConfigurationProperty(TemplateResolverAttribute, IsRequired = false)]
        public string TemplateResolverType
        {
            get { return (string)this[TemplateResolverAttribute]; }
        }

        /// <summary>
        /// Gets the template resolver type.
        /// </summary>
        [ConfigurationProperty(TemplateManagerAttribute, IsRequired = false)]
        public string TemplateManagerType
        {
            get { return (string)this[TemplateManagerAttribute]; }
        }
        /// <summary>
        /// Gets the collection of template service configurations.
        /// </summary>
        [ConfigurationProperty(TemplateServicesElement, IsRequired = false)]
        public TemplateServiceConfigurationElementCollection TemplateServices
        {
            get { return (TemplateServiceConfigurationElementCollection)this[TemplateServicesElement]; }
        }
        /// <summary>
        /// Sets the location of the temporary directory where temp files will be written.
        /// </summary>
        [ConfigurationProperty(TemporaryDirectoryAttribute, IsRequired = false)]
        public string TemporaryDirectory
        {
            get { return (string) this[TemporaryDirectoryAttribute]; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets an instance of <see cref="RazorEngineConfigurationSection"/> that represents the current configuration.
        /// </summary>
        /// <returns>An instance of <see cref="RazorEngineConfigurationSection"/>, or null if no configuration is specified.</returns>
        public static RazorEngineConfigurationSection GetConfiguration()
        {
            return ConfigurationManager.GetSection(SectionPath) as RazorEngineConfigurationSection;
        }
        #endregion
    }
}