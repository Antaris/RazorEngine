namespace RazorEngine.Tests.TestTypes.Activation
{
    /// <summary>
    /// Defines the required contract for implementing a text formatter.
    /// </summary>
    public interface ITextFormatter
    {
        #region Methods
        /// <summary>
        /// Formats the specified value.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>The formatted value.</returns>
        string Format(string value);
        #endregion
    }
}
