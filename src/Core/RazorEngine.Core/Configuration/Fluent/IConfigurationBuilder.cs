namespace RazorEngine.Configuration
{
    using System;

    using Compilation;
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
        IConfigurationBuilder EncodeUsing<TEncodedStringFactory>()
            where TEncodedStringFactory : IEncodedStringFactory, new();

        /// <summary>
        /// Includes the specified namespaces
        /// </summary>
        /// <param name="namespaces">The set of namespaces to include.</param>
        /// <returns>The current configuration builder.</returns>
        IConfigurationBuilder IncludeNamespaces(params string[] namespaces);

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