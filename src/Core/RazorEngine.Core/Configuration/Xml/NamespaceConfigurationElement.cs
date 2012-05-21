//-----------------------------------------------------------------------------
// <copyright file="NamespaceConfigurationElement.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Configuration.Xml
{
    using System.Configuration;

    /// <summary>
    /// Defines a configuration of a namespace.
    /// </summary>
    public class NamespaceConfigurationElement : ConfigurationElement
    {
        #region Fields

        /// <summary>
        /// The resource string
        /// </summary>
        private const string NamespaceAttribute = "namespace";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the namespace.
        /// </summary>
        [ConfigurationProperty(NamespaceAttribute, IsRequired = true)]
        public string Namespace
        {
            get { return (string)this[NamespaceAttribute]; }
        }

        #endregion
    }
}