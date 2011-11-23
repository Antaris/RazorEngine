namespace RazorEngine.Text
{
    /// <summary>
    /// Defines the required contract for implementing a factory for building encoded strings.
    /// </summary>
    public interface IEncodedStringFactory
    {
        #region Methods
        /// <summary>
        /// Creates a <see cref="IEncodedString"/> instance for the specified raw string.
        /// </summary>
        /// <param name="value">The raw string.</param>
        /// <returns>An instance of <see cref="IEncodedString"/>.</returns>
        IEncodedString CreateEncodedString(string value);

        /// <summary>
        /// Creates a <see cref="IEncodedString"/> instance for the specified object instance.
        /// </summary>
        /// <param name="value">The object instance.</param>
        /// <returns>An instance of <see cref="IEncodedString"/>.</returns>
        IEncodedString CreateEncodedString(object value);
        #endregion
    }
}
