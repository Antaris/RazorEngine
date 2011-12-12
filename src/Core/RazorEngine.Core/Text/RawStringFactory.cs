namespace RazorEngine.Text
{
    /// <summary>
    /// Represents a factory that creates <see cref="RawString"/> instances.
    /// </summary>
    public class RawStringFactory : IEncodedStringFactory
    {
        #region Methods
        /// <summary>
        /// Creates a <see cref="IEncodedString"/> instance for the specified raw string.
        /// </summary>
        /// <param name="value">Thevalue.</param>
        /// <returns>An instance of <see cref="IEncodedString"/>.</returns>
        public IEncodedString CreateEncodedString(string value)
        {
            return new RawString(value);
        }

        /// <summary>
        /// Creates a <see cref="IEncodedString"/> instance for the specified object instance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>An instance of <see cref="IEncodedString"/>.</returns>
        public IEncodedString CreateEncodedString(object value)
        {
            return (value == null) 
                ? new RawString(string.Empty)
                : new RawString(value.ToString());
        }
        #endregion
    }
}