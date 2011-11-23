namespace RazorEngine.Text
{
    /// <summary>
    /// Represents a factory that creates <see cref="HtmlEncodedString"/> instances.
    /// </summary>
    public class HtmlEncodedStringFactory : IEncodedStringFactory
    {
        #region Methods
        /// <summary>
        /// Creates a <see cref="IEncodedString"/> instance for the specified raw string.
        /// </summary>
        /// <param name="rawString">The raw string.</param>
        /// <returns>An instance of <see cref="IEncodedString"/>.</returns>
        public IEncodedString CreateEncodedString(string rawString)
        {
            return new HtmlEncodedString(rawString);
        }

        /// <summary>
        /// Creates a <see cref="IEncodedString"/> instance for the specified object instance.
        /// </summary>
        /// <param name="value">The object instance.</param>
        /// <returns>An instance of <see cref="IEncodedString"/>.</returns>
        public IEncodedString CreateEncodedString(object value)
        {
            if (value == null)
                return new HtmlEncodedString(string.Empty);

            var htmlString = value as HtmlEncodedString;
            if (htmlString != null)
                return htmlString;

            return new HtmlEncodedString(value.ToString());
        }
        #endregion
    }
}