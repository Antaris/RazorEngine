//-----------------------------------------------------------------------------
// <copyright file="TemplateServiceConfigurationElement.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Configuration.Xml
{
    using System.Configuration;

    /// <summary>
    /// Defines a configuration of a template service.
    /// </summary>
    public class TemplateServiceConfigurationElement : ConfigurationElement
    {
        #region Fields

        /// <summary>
        /// The string constant
        /// </summary>
        private const string BaseTemplateTypeAttribute = "baseTemplateType";

        /// <summary>
        /// The string constant
        /// </summary>
        private const string CodeInspectorsElement = "codeInspectors";

        /// <summary>
        /// The string constant
        /// </summary>
        private const string DebugAttribute = "debug";

        /// <summary>
        /// The string constant
        /// </summary>
        private const string EncodedStringFactoryAttribute = "encodedStringFactoryType";

        /// <summary>
        /// The string constant
        /// </summary>
        private const string LanguageAttribute = "language";

        /// <summary>
        /// The string constant
        /// </summary>
        private const string NameAttribute = "name";

        /// <summary>
        /// The string constant
        /// </summary>
        private const string NamespacesElement = "namespaces";

        #endregion

        #region Properties
        /// <summary>
        /// Gets the base template type.
        /// </summary>
        [ConfigurationProperty(BaseTemplateTypeAttribute, IsRequired = false)]
        public string BaseTemplateType
        {
            get { return (string)this[BaseTemplateTypeAttribute]; }
        }

        /// <summary>
        /// Gets a value indicating whether the template service is in debug mode.
        /// </summary>
        [ConfigurationProperty(DebugAttribute, IsRequired = false, DefaultValue = false)]
        public bool Debug
        {
            get { return (bool)this[DebugAttribute]; }
        }

        /// <summary>
        /// Gets the encoded string factory type.
        /// </summary>
        [ConfigurationProperty(EncodedStringFactoryAttribute, IsRequired = false)]
        public string EncodedStringFactoryType
        {
            get { return (string)this[EncodedStringFactoryAttribute]; }
        }

        /// <summary>
        /// Gets the language.
        /// </summary>
        [ConfigurationProperty(LanguageAttribute, IsRequired = false, DefaultValue = Language.CSharp)]
        public Language Language
        {
            get { return (Language)this[LanguageAttribute]; }
        }

        /// <summary>
        /// Gets the name of the template service.
        /// </summary>
        [ConfigurationProperty(NameAttribute, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this[NameAttribute]; }
        }

        /// <summary>
        /// Gets the collection of namespaces.
        /// </summary>
        [ConfigurationProperty(NamespacesElement, IsRequired = false)]
        public NamespaceConfigurationElementCollection Namespaces
        {
            get { return (NamespaceConfigurationElementCollection)this[NamespacesElement]; }
        }

        #endregion
    }
}