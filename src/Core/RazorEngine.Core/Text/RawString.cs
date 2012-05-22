//-----------------------------------------------------------------------------
// <copyright file="RawString.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Text
{
    /// <summary>
    /// Represents an un-encoded string.
    /// </summary>
    public class RawString : IEncodedString
    {
        #region Fields

        /// <summary>
        /// The value
        /// </summary>
        private readonly string value;

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RawString"/> class.
        /// </summary>
        /// <param name="value">The value</param>
        public RawString(string value)
        {
            this.value = value;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Gets the encoded string.
        /// </summary>
        /// <returns>The encoded string.</returns>
        public string ToEncodedString()
        {
            return this.value ?? string.Empty;
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
