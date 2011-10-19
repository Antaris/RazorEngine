namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing a template.
    /// </summary>
    public interface ITemplate
    {
        #region Properties
        /// <summary>
        /// Sets the template service.
        /// </summary>
        ITemplateService TemplateService { set; }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the compiled template.
        /// </summary>
        void Execute();

        /// <summary>
        /// Runs the template and returns the result.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <returns>The merged result of the template.</returns>
        string Run(ExecuteContext context);

        /// <summary>
        /// Writes the specified object to the result.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void Write(object value);

        /// <summary>
        /// Writes the specified string to the result.
        /// </summary>
        /// <param name="literal">The literal to write.</param>
        void WriteLiteral(string literal);
        #endregion
    }
}