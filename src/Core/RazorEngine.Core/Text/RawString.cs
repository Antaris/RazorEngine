namespace RazorEngine.Text
{
    /// <summary>
    /// Represents an unencoded string.
    /// </summary>
    public class RawString : IEncodedString
    {
        #region Fields
        private readonly string _value;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="RawString"/>
        /// </summary>
        /// <param name="value">The value</param>
        public RawString(string value)
        {
            _value = value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the encoded string.
        /// </summary>
        /// <returns>The encoded string.</returns>
        public string ToEncodedString()
        {
            return _value ?? string.Empty;
        }

        /// <summary>
        /// Gets the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString()
        {
            return ToEncodedString();
        }
        #endregion
    }
}
