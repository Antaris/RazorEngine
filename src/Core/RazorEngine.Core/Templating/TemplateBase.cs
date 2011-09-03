namespace RazorEngine.Templating
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;

    using Text;

    /// <summary>
    /// Provides a base implementation of a template.
    /// </summary>
    public abstract class TemplateBase : MarshalByRefObject, ITemplate
    {
        #region Fields
        private readonly StringBuilder _builder;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateBase"/>.
        /// </summary>
        protected TemplateBase()
        {
            _builder = new StringBuilder();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the string result of the template.
        /// </summary>
        public string Result
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);

                return _builder.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the template service.
        /// </summary>
        public ITemplateService TemplateService { get; set; }

        /// <summary>
        /// Gets the string builder.
        /// </summary>
        protected StringBuilder Builder { get { return _builder; } }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the compiled template.
        /// </summary>
        public virtual void Execute() { }

        /// <summary>
        /// Returns the specified string as a raw string. This will ensure it is not encoded.
        /// </summary>
        /// <param name="rawString">The raw string to write.</param>
        /// <returns>An instance of <see cref="IEncodedString"/>.</returns>
        public IEncodedString Raw(string rawString)
        {
            return new RawString(rawString);
        }

        /// <summary>
        /// Runs the template and returns the result.
        /// </summary>
        /// <returns>The merged result of the template.</returns>
        string ITemplate.Run()
        {
            Execute();
            return Result;
        }

        /// <summary>
        /// Writes the specified object to the result.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public virtual void Write(object value)
        {
            if (value == null) return;

            var encodedString = value as IEncodedString;
            if (encodedString != null)
            {
                _builder.Append(encodedString);
            }
            else
            {
                encodedString = TemplateService.EncodedStringFactory.CreateEncodedString(value);
                _builder.Append(encodedString);
            }
        }

        /// <summary>
        /// Writes the specified string to the result.
        /// </summary>
        /// <param name="literal">The literal to write.</param>
        public virtual void WriteLiteral(string literal)
        {
            if (literal == null) return;
            _builder.Append(literal);
        }

        /// <summary>
        /// Writes a string literal to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="literal">The literal to be written.</param>
        [Pure]
        public static void WriteLiteralTo(TextWriter writer, string literal)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (literal == null) return;
            writer.Write(literal);
        }

        /// <summary>
        /// Writes the specified object to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value to be written.</param>
        [Pure]
        public static void WriteTo(TextWriter writer, object value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (value == null) return;
            writer.Write(value);
        }
        #endregion
    }
}