namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing a template resolver.
    /// </summary>
    public interface ITemplateManager
    {
        #region Methods
        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template to resolve.</param>
        /// <returns>The template content.</returns>
        ITemplateSource Resolve(ITemplateKey key);


        ITemplateKey GetKey(string name, ResolveType templateType, ICompiledTemplate context);

        void AddDynamic(ITemplateKey key, ITemplateSource source);
        #endregion
    }
}