namespace RazorEngine.Configuration
{
    using System;

    using Compilation;
    using Compilation.Inspectors;
    using Templating;
    using Text;

    /// <summary>
    /// Defines the required contract for implementing a configuration builder.
    /// </summary>
    public interface IConfigurationBuilder
    {
        #region Methods
        /// <summary>
        /// Sets the activator.
        /// </summary>
        /// <param name="activator">The activator instance.</param>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder ActivateUsing(IActivator activator);

        /// <summary>
        /// Sets the activator.
        /// </summary>
        /// <typeparam name="TActivator">The activator type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder ActivateUsing<TActivator>() where TActivator : IActivator, new();

        /// <summary>
        /// Sets the activator.
        /// </summary>
        /// <param name="activator">The activator delegate.</param>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder ActivateUsing(Func<InstanceContext, ITemplate> activator);
        
#if !RAZOR4
        /// <summary>
        /// Adds the specified code inspector.
        /// </summary>
        /// <typeparam name="TInspector">The code inspector type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        [Obsolete("This API is obsolete and will be removed in the next version (Razor4 doesn't use CodeDom for code-generation)!")]
        IConfigurationBuilder AddInspector<TInspector>() where TInspector : ICodeInspector, new();

        /// <summary>
        /// Adds the specified code inspector.
        /// </summary>
        /// <param name="inspector">The code inspector.</param>
        /// <returns>The current configuration builder.</returns>
        [Obsolete("This API is obsolete and will be removed in the next version (Razor4 doesn't use CodeDom for code-generation)!")]
        IConfigurationBuilder AddInspector(ICodeInspector inspector);
#endif

        /// <summary>
        /// Sets the compiler service factory.
        /// </summary>
        /// <param name="factory">The compiler service factory.</param>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder CompileUsing(ICompilerServiceFactory factory);

        /// <summary>
        /// Sets the compiler service factory.
        /// </summary>
        /// <typeparam name="TCompilerServiceFactory">The compiler service factory type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder CompileUsing<TCompilerServiceFactory>()
            where TCompilerServiceFactory : ICompilerServiceFactory, new();

        /// <summary>
        /// Sets the encoded string factory.
        /// </summary>
        /// <param name="factory">The encoded string factory.</param>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder EncodeUsing(IEncodedStringFactory factory);

        /// <summary>
        /// Sets the encoded string factory.
        /// </summary>
        /// <typeparam name="TEncodedStringFactory">The encoded string factory type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder EncodeUsing<TEncodedStringFactory>() where TEncodedStringFactory : IEncodedStringFactory, new();

        /// <summary>
        /// Sets the resolve used to locate unknown templates.
        /// </summary>
        /// <typeparam name="TResolver">The resolve type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        [Obsolete("Please use ManageUsing instead")]
        IConfigurationBuilder ResolveUsing<TResolver>() where TResolver : ITemplateResolver, new();

        /// <summary>
        /// Sets the manager used to locate unknown templates.
        /// </summary>
        /// <typeparam name="TManager">The manager type.</typeparam>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder ManageUsing<TManager>() where TManager : ITemplateManager, new();

        /// <summary>
        /// Sets the resolver used to locate unknown templates.
        /// </summary>
        /// <param name="resolver">The resolver instance to use.</param>
        /// <returns>The current configuration builder.</returns>
        [Obsolete("Please use ManageUsing instead")]
        IConfigurationBuilder ResolveUsing(ITemplateResolver resolver);
        
        /// <summary>
        /// Sets the manager used to locate unknown templates.
        /// </summary>
        /// <param name="manager">The manager instance to use.</param>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder ManageUsing(ITemplateManager manager);

        /// <summary>
        /// Sets the resolver delegate used to locate unknown templates.
        /// </summary>
        /// <param name="resolver">The resolver delegate to use.</param>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder ResolveUsing(Func<string, string> resolver);

        /// <summary>
        /// Includes the specified namespaces
        /// </summary>
        /// <param name="namespaces">The set of namespaces to include.</param>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder IncludeNamespaces(params string[] namespaces);

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
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder DisableTempFileLocking();

        /// <summary>
        /// Sets the default activator.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder UseDefaultActivator();

        /// <summary>
        /// Sets the default compiler service factory.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder UseDefaultCompilerServiceFactory();

        /// <summary>
        /// Sets the default encoded string factory.
        /// </summary>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder UseDefaultEncodedStringFactory();

        /// <summary>
        /// Sets the base template type.
        /// </summary>
        /// <param name="baseTemplateType">The base template type.</param>
        /// <returns>The current configuration builder/.</returns>
        IConfigurationBuilder WithBaseTemplateType(Type baseTemplateType);

        /// <summary>
        /// Sets the code language.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder WithCodeLanguage(Language language);

        /// <summary>
        /// Sets the encoding.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder WithEncoding(Encoding encoding);
        #endregion
    }
}