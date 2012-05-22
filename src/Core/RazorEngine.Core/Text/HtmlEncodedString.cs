//-----------------------------------------------------------------------------
// <copyright file="HtmlEncodedString.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Text
{
    using System.Net;

    /// <summary>
    /// Represents a Html-encoded string.
    /// </summary>
    public class HtmlEncodedString : IEncodedString
    {
        #region Fields

        /// <summary>
        /// The encoded string
        /// </summary>
        private readonly string encodedString;

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlEncodedString"/> class.
        /// </summary>
        /// <param name="value">The raw string to be encoded.</param>
        public HtmlEncodedString(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.encodedString = WebUtility.HtmlEncode(value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the encoded string.
        /// </summary>
        /// <returns>The encoded string.</returns>
        public string ToEncodedString()
        {
            return this.encodedString ?? string.Empty;
        }

        /// <summary>
        /// Gets the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString()
        {
            return this.ToEncodedString();
        }

        #endregion
    }
}