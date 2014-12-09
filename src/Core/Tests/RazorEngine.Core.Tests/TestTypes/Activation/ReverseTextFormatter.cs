namespace RazorEngine.Tests.TestTypes.Activation
{
    using System;

    /// <summary>
    /// Reverses the contents of the specified string,
    /// </summary>
    public class ReverseTextFormatter : ITextFormatter
    {
        #region Methods
        /// <summary>
        /// Formats the specified value.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>The formatted value.</returns>
        public string Format(string value)
        {
            char[] content = value.ToCharArray();
            Array.Reverse(content);
            return new string(content);
        }
        #endregion
    }
}
