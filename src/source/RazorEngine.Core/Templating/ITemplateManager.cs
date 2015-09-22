namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing a template-manager.
    /// </summary>
    public interface ITemplateManager
    {
        #region Methods
        /// <summary>
        /// Resolves the template with the specified key.
        /// </summary>
        /// <param name="key">The key which should be resolved to a template source.</param>
        /// <returns>The template content.</returns>
        ITemplateSource Resolve(ITemplateKey key);

        /// <summary>
        /// Get the key of a template.
        /// This method has to be implemented so that the manager can control the <see cref="ITemplateKey"/> implementation.
        /// This way the cache api can rely on the unique string given by <see cref="ITemplateKey.GetUniqueKeyString"/>.
        /// </summary>
        /// <remarks>
        /// For example one template manager reads all template from a single folder, then the <see cref="ITemplateKey.GetUniqueKeyString"/> can simply return the template name.
        /// Another template manager can read from different folders depending whether we include a layout or including a template.
        /// In that situation the <see cref="ITemplateKey.GetUniqueKeyString"/> has to take that into account so that templates with the same name can not be confused.
        /// </remarks>
        /// <param name="name">The name of the template</param>
        /// <param name="resolveType">how the template is resolved</param>
        /// <param name="context">gets the context for the current resolve operation. 
        /// Which template is resolving another template? (null = we search a global template)
        /// </param>
        /// <returns>the key for the template</returns>
        ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context);

        /// <summary>
        /// Adds a template dynamically to the current manager.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="source"></param>
        void AddDynamic(ITemplateKey key, ITemplateSource source);

        #endregion
    }
}