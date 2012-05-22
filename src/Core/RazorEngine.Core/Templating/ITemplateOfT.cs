//-----------------------------------------------------------------------------
// <copyright file="ITemplateOfT.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing a template with a model.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public interface ITemplate<T> : ITemplate
    {
        #region Properties

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        T Model { get; set; }

        #endregion
    }
}