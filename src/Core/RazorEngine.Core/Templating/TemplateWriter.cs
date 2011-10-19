namespace RazorEngine.Templating
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Defines a template writer used for helper templates.
    /// </summary>
    public class TemplateWriter
    {
        #region Fields
        private readonly Action<TextWriter> writerDelegate;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateWriter"/>.
        /// </summary>
        /// <param name="writer">The writer delegate used to write using the specified <see cref="TextWriter"/>.</param>
        public TemplateWriter(Action<TextWriter> writer)
        {
            Contract.Requires(writer != null);

            writerDelegate = writer;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the write delegate and returns the result of this <see cref="TemplateWriter"/>.
        /// </summary>
        /// <returns>The string result of the helper template.</returns>
        public override string ToString()
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                writerDelegate(writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Writes the helper result of the specified text writer.
        /// </summary>
        /// <param name="writer">The text writer to write the helper result to.</param>
        public void WriteTo(TextWriter writer)
        {
            writerDelegate(writer);
        }
        #endregion
    }
}