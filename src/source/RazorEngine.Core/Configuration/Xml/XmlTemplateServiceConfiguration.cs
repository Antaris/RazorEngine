﻿namespace RazorEngine.Configuration.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using Compilation;
    using Compilation.Inspectors;
    using Templating;
    using Text;
    using RazorEngine.Compilation.ReferenceResolver;

    /// <summary>
    /// Represents a template service configuration that supports the xml configuration mechanism.
    /// </summary>
    public class XmlTemplateServiceConfiguration : ITemplateServiceConfiguration
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="XmlTemplateServiceConfiguration"/>.
        /// </summary>
        /// <param name="name">The name of the template service configuration.</param>
        public XmlTemplateServiceConfiguration(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("'name' is a required parameter.", "name");

            Namespaces = new HashSet<string>();

            InitialiseConfiguration(name);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the activator.
        /// </summary>
        public IActivator Activator { get; private set; }

        /// <summary>
        /// Gets or sets whether to allow missing properties on dynamic models.
        /// </summary>
        public bool AllowMissingPropertiesOnDynamic { get; private set; }

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
        public bool DisableTempFileLocking { get; private set; }

        /// <summary>
        /// Gets the base template type.
        /// </summary>
        public Type BaseTemplateType { get; private set; }

#if !RAZOR4
        /// <summary>
        /// Gets the code inspectors.
        /// </summary>
        [Obsolete("This API is obsolete and will be removed in the next version (Razor4 doesn't use CodeDom for code-generation)!")]
        public IEnumerable<ICodeInspector> CodeInspectors { get; private set; }
#endif

        /// <summary>
        /// Gets the reference resolver.
        /// </summary>
        public IReferenceResolver ReferenceResolver { get; private set; }

        /// <summary>
        /// Gets the caching provider.
        /// </summary>
        public ICachingProvider CachingProvider { get; private set; }

        /// <summary>
        /// Gets the compiler service factory.
        /// </summary>
        public ICompilerServiceFactory CompilerServiceFactory { get; private set; }

        /// <summary>
        /// Gets whether the template service is operating in debug mode.
        /// </summary>
        public bool Debug { get; private set; }

        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory { get; private set; }

        /// <summary>
        /// Gets the language.
        /// </summary>
        public Language Language { get; private set; }

        /// <summary>
        /// Gets the namespaces.
        /// </summary>
        public ISet<string> Namespaces { get; private set; }

        /// <summary>
        /// Gets the template resolver.
        /// </summary>
        [Obsolete("Please use the TemplateManager property instead")]
        public ITemplateResolver Resolver { get; private set; }

        /// <summary>
        /// Gets the template resolver.
        /// </summary>
        public ITemplateManager TemplateManager { get; private set; }

        public string TemporaryDirectory { get; private set; }

        #endregion

        #region Methods
        /// <summary>
        /// Adds the namespaces from the specified collection.
        /// </summary>
        /// <param name="namespaces">The set of namespace configurations.</param>
        private void AddNamespaces(NamespaceConfigurationElementCollection namespaces)
        {
            if (namespaces == null || namespaces.Count == 0)
                return;

            foreach (NamespaceConfigurationElement config in namespaces)
                Namespaces.Add(config.Namespace);
        }

        /// <summary>
        /// Gets an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The expected instance type.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns>The instance.</returns>
        private T GetInstance<T>(Type type)
        {
            Type instanceType = typeof(T);

            if (!instanceType.IsAssignableFrom(type))
                throw new ConfigurationErrorsException("The type '" + type.FullName + "' is not assignable to type '" + instanceType.FullName + "'");

            return (T)System.Activator.CreateInstance(type);
        }

        /// <summary>
        /// Gets the type with the specified name.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <returns></returns>
        private Type GetType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            var type = Type.GetType(typeName);
            if (type == null)
                throw new ConfigurationErrorsException("The type '" + typeName + "' could not be loaded.");

            return type;
        }

        /// <summary>
        /// Initialises the configuration.
        /// </summary>
        /// <param name="name">The name of the template service configuration.</param>
        private void InitialiseConfiguration(string name)
        {
            var config = RazorEngineConfigurationSection.GetConfiguration();
            if (config == null)
                throw new ConfigurationErrorsException("No <razorEngine> configuration section has been defined.");

            var serviceConfig = config.TemplateServices
                .OfType<TemplateServiceConfigurationElement>()
                .Where(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();

            if (serviceConfig == null)
                throw new ConfigurationErrorsException("No <templateService> configuration element defined with name = '" + name + "'");

            InitialiseConfiguration(config, serviceConfig);
        }

        /// <summary>
        /// Initialises the configuration.
        /// </summary>
        /// <param name="config">The core configuration.</param>
        /// <param name="serviceConfig">The service configuration.</param>
        private void InitialiseConfiguration(RazorEngineConfigurationSection config, TemplateServiceConfigurationElement serviceConfig)
        {
            // Set whether we are allowing missing properties on dynamic.
            AllowMissingPropertiesOnDynamic = config.AllowMissingPropertiesOnDynamic;

            // Set whether we load templates with Assembly.Load(byte[]).
            DisableTempFileLocking = config.DisableTempFileLocking;

            // Sets the tempoaryDirectory
            TemporaryDirectory = config.TemporaryDirectory;

            // Add the global namespaces.
            AddNamespaces(config.Namespaces);

            // Add the specific namespaces.
            AddNamespaces(serviceConfig.Namespaces);
                
            // Sets the activator.
            SetActivator(config.ActivatorType);

            // Sets the base template type.
            SetBaseTemplateType(serviceConfig.BaseTemplateType);
            
            // Sets the compiler service factory.
            SetCompilerServiceFactory(config.CompilerServiceFactoryType);

            Debug = serviceConfig.Debug;
            
            // Sets the encoded string factory.
            SetEncodedStringFactory(serviceConfig.EncodedStringFactoryType);
            
            // Sets the reference resolver.
            SetReferenceResolver(config.ReferenceResolverType);

#pragma warning disable 0618 // Backwards Compat.
            // Sets the template resolver.
            SetTemplateResolver(config.TemplateResolverType);

            if (Resolver != null)
            {
                TemplateManager = new WrapperTemplateManager(Resolver);
            }
#pragma warning restore 0618 // Backwards Compat.

            // Sets the template manager.
            SetTemplateManager(config.TemplateManagerType);

            // Set the language.
            Language = serviceConfig.Language;

            // Sets the activator.
            SetActivator(config.ActivatorType);

            // Sets the compiler service factory.
            SetCompilerServiceFactory(config.CompilerServiceFactoryType);

#pragma warning disable 0618 // Backwards Compat.
            // Sets the template resolver.
            SetTemplateResolver(config.TemplateResolverType);
#pragma warning restore 0618 // Backwards Compat.
        }

        /// <summary>
        /// Sets the activator.
        /// </summary>
        /// <param name="activatorType">The activator type.</param>
        private void SetActivator(string activatorType)
        {
            var type = GetType(activatorType);
            if (type != null)
                Activator = GetInstance<IActivator>(type);
        }

        /// <summary>
        /// Sets the base template type.
        /// </summary>
        /// <param name="baseTemplateType">The base template type.</param>
        private void SetBaseTemplateType(string baseTemplateType)
        {
            var type = GetType(baseTemplateType);
            if (type != null)
                BaseTemplateType = type;
        }

        /// <summary>
        /// Sets the compiler service factory.
        /// </summary>
        /// <param name="compilerServiceFactoryType">The compiler service factory type.</param>
        private void SetCompilerServiceFactory(string compilerServiceFactoryType)
        {
            var type = GetType(compilerServiceFactoryType);
            if (type != null)
                CompilerServiceFactory = GetInstance<ICompilerServiceFactory>(type);
        }


        /// <summary>
        /// Sets the reference resolver.
        /// </summary>
        /// <param name="referenceResolverType">The reference resolver type.</param>
        private void SetReferenceResolver(string referenceResolverType)
        {
            var type = GetType(referenceResolverType);
            if (type != null)
                ReferenceResolver = GetInstance<IReferenceResolver>(type);
        }

        /// <summary>
        /// Sets the encoded string factory.
        /// </summary>
        /// <param name="encodedStringFactoryType"></param>
        private void SetEncodedStringFactory(string encodedStringFactoryType)
        {
            var type = GetType(encodedStringFactoryType);
            if (type != null)
                EncodedStringFactory = GetInstance<IEncodedStringFactory>(type);
        }

        /// <summary>
        /// Sets the template resolver.
        /// </summary>
        /// <param name="templateResolverType">The template resolver type.</param>
        private void SetTemplateResolver(string templateResolverType)
        {
            var type = GetType(templateResolverType);
#pragma warning disable 0618 // Backwards Compat.
            if (type != null)
                Resolver = GetInstance<ITemplateResolver>(type);
#pragma warning restore 0618 // Backwards Compat.
        }

        /// <summary>
        /// Sets the template manager.
        /// </summary>
        /// <param name="templateManagerType">The template manager type.</param>
        private void SetTemplateManager(string templateManagerType)
        {
            var type = GetType(templateManagerType);
            if (type != null)
                TemplateManager = GetInstance<ITemplateManager>(type);
        }
        #endregion
    }
}
