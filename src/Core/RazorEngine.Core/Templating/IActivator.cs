//-----------------------------------------------------------------------------
// <copyright file="IActivator.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing an activator.
    /// </summary>
    public interface IActivator
    {
        #region Methods

        /// <summary>
        /// Creates an instance of the specified template.
        /// </summary>
        /// <param name="context">The instance context.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        ITemplate CreateInstance(InstanceContext context);

        #endregion
    }
}