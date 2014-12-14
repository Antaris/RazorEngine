using System;
using System.IO;
namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing a template.
    /// </summary>
    public interface ITemplate
    {
        #region Properties
        /// <summary>
        /// Sets the internal template service.
        /// </summary>
        IInternalTemplateService InternalTemplateService { set; }

        /// <summary>
        /// OBSOLETE: Sets the template service.
        /// </summary>
        [Obsolete("Only provided for backwards compatibility, use CachedTemplateService instead.")]
        ITemplateService TemplateService { set; }

        /// <summary>
        /// Sets the cached template service.
        /// </summary>
        ICachedTemplateService CachedTemplateService { set; }

        #endregion

        #region Methods
        /// <summary>
        /// Set the model of the template (if applicable).
        /// </summary>
        /// <param name="model"></param>
        void SetModel(object model);

        /// <summary>
        /// Executes the compiled template.
        /// </summary>
        void Execute();

        /// <summary>
        /// Runs the template and returns the result.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <returns>The merged result of the template.</returns>
        void Run(ExecuteContext context, TextWriter writer);

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