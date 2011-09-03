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
        /// <param name="rawString">The raw string.</param>
        /// <returns>An instance of <see cref="IEncodedString"/>.</returns>
        public IEncodedString CreateEncodedString(string rawString)
        {
            return new RawString(rawString);
        }

        /// <summary>
        /// Creates a <see cref="IEncodedString"/> instance for the specified object instance.
        /// </summary>
        /// <param name="obj">The object instance.</param>
        /// <returns>An instance of <see cref="IEncodedString"/>.</returns>
        public IEncodedString CreateEncodedString(object obj)
        {
            return (obj == null) 
                ? new RawString(string.Empty) 
                : new RawString(obj.ToString());
        }
        #endregion
    }
}