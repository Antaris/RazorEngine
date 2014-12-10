namespace RazorEngine.Tests.TestTypes.Activation
{
    using System;

    using Templating;

    /// <summary>
    /// Defines a test template base.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public class CustomTemplateBase<T> : TemplateBase<T>
    {
        #region Fields
        private readonly ITextFormatter _formatter;
        #endregion

        #region Methods
        /// <summary>
        /// Initialises a new instance of <see cref="CustomTemplateBase{T}"/>
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        public CustomTemplateBase(ITextFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException("formatter");

            _formatter = formatter;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Formats the specified object.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>The string formatted value.</returns>
        public string Format(object value)
        {
            return _formatter.Format(value.ToString());
        }
        #endregion
    }
}