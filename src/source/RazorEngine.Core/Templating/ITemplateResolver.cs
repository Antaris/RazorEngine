namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing a template resolver.
    /// </summary>
    public interface ITemplateResolver
    {
        #region Methods
        /// <summary>
        /// Resolves the template content with the specified name.
        /// </summary>
        /// <param name="name">The name of the template to resolve.</param>
        /// <returns>The template content.</returns>
        string Resolve(string name);

        /// <summary>
        /// Resolves the filename of the specified templatename.
        /// Used for debugging and template compile errors.
        /// </summary>
        /// <param name="name">The name of the template to resolve.</param>
        /// <returns>The fullpath to the template.</returns>
        string ResolveFilename(string name);
        #endregion
    }
}