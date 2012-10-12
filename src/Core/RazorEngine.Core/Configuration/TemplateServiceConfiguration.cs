namespace RazorEngine.Configuration
{
    using System;
    using System.Collections.Generic;

    using Compilation;
    using Compilation.Inspectors;
    using Templating;
    using Text;

    using System.Configuration;
    using Xml;

    /// <summary>
    /// Provides a default implementation of a template service configuration.
    /// </summary>
    public class TemplateServiceConfiguration : ITemplateServiceConfiguration
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateServiceConfiguration"/>
        /// </summary>
        public TemplateServiceConfiguration()
        {
            var config = RazorEngineConfigurationSection.GetConfiguration() ?? new RazorEngineConfigurationSection();
            Activator = string.IsNullOrEmpty(config.ActivatorType) ? new DefaultActivator() : GetActivator(config.ActivatorType);
            EncodedStringFactory = new HtmlEncodedStringFactory();
            CodeInspectors = new List<ICodeInspector>();

            // Add base namespaces
            if (config.Namespaces.Count == 0)
            {
                Namespaces = new HashSet<string>
                             {
                                 "System", 
                                 "System.Collections.Generic", 
                                 "System.Linq"
                             };
            }

            // Add additional names.
            AddNamespaces(config.Namespaces);
            CompilerServiceFactory = string.IsNullOrEmpty(config.CompilerServiceFactoryType) ? new DefaultCompilerServiceFactory() : GetCompilerServiceFactory(config.CompilerServiceFactoryType);
            Language = (Language == Language.CSharp) ? Language.CSharp : config.DefaultLanguage;
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
        IEnumerable<ICodeInspector> ITemplateServiceConfiguration.CodeInspectors { get { return CodeInspectors; } }

        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        public IList<ICodeInspector> CodeInspectors { get; private set; }

        /// <summary>
        /// Gets or sets the compiler service factory.
        /// </summary>
        public ICompilerServiceFactory CompilerServiceFactory { get; set; }

        /// <summary>
        /// Gets whether the template service is operating in debug mode.
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
        private static T GetInstance<T>(Type type)
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
        private static Type GetType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            var type = Type.GetType(typeName);
            if (type == null)
                throw new ConfigurationErrorsException("The type '" + typeName + "' could not be loaded.");

            return type;
        }

        /// <summary>
        /// Gets the activator.
        /// </summary>
        /// <param name="activatorType">The activator type.</param>
        private static IActivator GetActivator(string activatorType)
        {
            var type = GetType(activatorType);
            return type != null ? GetInstance<IActivator>(type) : null;
        }

        /// <summary>
        /// Gets the base template type.
        /// </summary>
        /// <param name="baseTemplateType">The base template type.</param>
        private Type GetBaseTemplateType(string baseTemplateType)
        {
            var type = GetType(baseTemplateType);
            return type;
        }

        /// <summary>
        /// Gets the compiler service factory.
        /// </summary>
        /// <param name="compilerServiceFactoryType">The compiler service factory type.</param>
        private static ICompilerServiceFactory GetCompilerServiceFactory(string compilerServiceFactoryType)
        {
            var type = GetType(compilerServiceFactoryType);
            return type != null ? GetInstance<ICompilerServiceFactory>(type) : null;
        }

        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        /// <param name="encodedStringFactoryType"></param>
        private static IEncodedStringFactory GetEncodedStringFactory(string encodedStringFactoryType)
        {
            var type = GetType(encodedStringFactoryType);
            return type != null ? GetInstance<IEncodedStringFactory>(type) : null;
        }

        /// <summary>
        /// Gets the template resolver.
        /// </summary>
        /// <param name="templateResolverType">The template resolver type.</param>
        private static ITemplateResolver GetTemplateResolver(string templateResolverType)
        {
            var type = GetType(templateResolverType);
            return type != null ? GetInstance<ITemplateResolver>(type) : null;
        }
        #endregion
    }
}