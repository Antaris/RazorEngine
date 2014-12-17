namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing a template resolver.
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
        /// This method has to be implemented so that the manager can control the ITemplateKey implementation.
        /// This way the cache api can rely on the unique string given by GetUniqueKeyString().
        /// </summary>
        /// <remarks>
        /// For example one template manager reads all template from a single folder, then the GetUniqueKeyString() can simply return the template name.
        /// Another template manager can read from different folders depending whether we include a layout or including a template.
        /// In that situation the GetUniqueKeyString() has to take that into account so that templates with the same name can not be confused.
        /// </remarks>
        /// <param name="name">The name of the tempalte</param>
        /// <param name="templateType">how the template is resolved</param>
        /// <param name="context">gets the context for the current resolve operation. 
        /// Which template is resolving another template? (null = we search a global template)
        /// </param>
        /// <returns>the key for the template</returns>
        ITemplateKey GetKey(string name, ResolveType templateType, ITemplateKey context);

        void AddDynamic(ITemplateKey key, ITemplateSource source);

        #endregion
    }
}