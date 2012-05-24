//-----------------------------------------------------------------------------
// <copyright file="IEncodedString.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Text
{
    /// <summary>
    /// Defines the required contract for implementing an encoded string.
    /// </summary>
    public interface IEncodedString
    {
        #region Methods

        /// <summary>
        /// Gets the encoded string.
        /// </summary>
        /// <returns>The encoded string.</returns>
        string ToEncodedString();

        #endregion
    }
}