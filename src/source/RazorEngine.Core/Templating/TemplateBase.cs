namespace RazorEngine.Templating
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Security;
    using System.Text;
    using Text;

    /// <summary>
    /// Provides a base implementation of a template.
    /// </summary>
    public abstract class TemplateBase : MarshalByRefObject, ITemplate
    {
        #region Fields
        protected ExecuteContext _context;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateBase"/>.
        /// </summary>
        protected TemplateBase() { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the layout template name.
        /// </summary>
        public string Layout { get; set; }

        internal virtual Type ModeType
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets or sets the template service.
        /// </summary>
        public IInternalTemplateService InternalTemplateService { internal get; set; }

        [Obsolete("Only provided for backwards compatibility, use CachedTemplateService instead.")]
        public ITemplateService TemplateService { get; set; }

        public IRazorEngineService RazorEngine { get; set; }

        /// <summary>
        /// Gets the viewbag that allows sharing state between layout and child templates.
        /// </summary>
        public dynamic ViewBag { get { return _context.ViewBag; } }

        /// <summary>
        /// Gets the current writer.
        /// </summary>
        public TextWriter CurrentWriter { get { return _context.CurrentWriter; } }
        #endregion

        #region Methods
        public virtual void SetModel(object model)
        {

        }

        /// <summary>
        /// Defines a section that can written out to a layout.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="action">The delegate used to write the section.</param>
        public void DefineSection(string name, Action action)
        {
            _context.DefineSection(name, action);
        }

        /// <summary>
        /// Includes the template with the specified name.
        /// </summary>
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <returns>The template writer helper.</returns>
        public virtual TemplateWriter Include(string cacheName, object model = null, Type modelType = null)
        {
            var instance = InternalTemplateService.Resolve(cacheName, model, modelType, ResolveType.Include);
            if (instance == null)
                throw new ArgumentException("No template could be resolved with name '" + cacheName + "'");

            return new TemplateWriter(tw =>
                instance.Run(
                    InternalTemplateService.CreateExecuteContext(ViewBag), tw));
        }

        /// <summary>
        /// Determines if the section with the specified name has been defined.
        /// </summary>
        /// <param name="name">The section name.</param>
        /// <returns></returns>
        public virtual bool IsSectionDefined(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The name of the section to render must be specified.");

            return (_context.GetSectionDelegate(name) != null);
        }

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
        /// Resolves the layout template.
        /// </summary>
        /// <param name="name">The name of the layout template.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        protected virtual ITemplate ResolveLayout(string name)
        {
            return InternalTemplateService.Resolve(name, null, null, ResolveType.Layout);
        }

        private static void StreamToTextWriter(MemoryStream memory, TextWriter writer)
        {
            memory.Position = 0;
            using (var r = new StreamReader(memory))
            {
                while (!r.EndOfStream)
                {
                    writer.Write(r.ReadToEnd());
                }
            }
        }

        /// <summary>
        /// Runs the template and returns the result.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <returns>The merged result of the template.</returns>
        void ITemplate.Run(ExecuteContext context, TextWriter reader)
        {
            _context = context;

            using (var memory = new MemoryStream())
            {
                using (var writer = new StreamWriter(memory))
                {
                    _context.CurrentWriter = writer;
                    Execute();
                    writer.Flush();
                    _context.CurrentWriter = null;


                    if (Layout != null)
                    {
                        // Get the layout template.
                        var layout = ResolveLayout(Layout);

                        if (layout == null)
                        {
                            throw new ArgumentException("Template you are trying to run uses layout, but no layout found in cache or by resolver.");
                        }

                        // Push the current body instance onto the stack for later execution.
                        var body = new TemplateWriter(tw =>
                        {
                            StreamToTextWriter(memory, tw);
                        });
                        context.PushBody(body);
                        context.PushSections();

                        layout.Run(context, reader);
                        return;
                    }

                    StreamToTextWriter(memory, reader);
                }
            }
        }

        /// <summary>
        /// Renders the section with the specified name.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="isRequired">Flag to specify whether the section is required.</param>
        /// <returns>The template writer helper.</returns>
        public virtual TemplateWriter RenderSection(string name, bool isRequired = true)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The name of the section to render must be specified.");

            var action = _context.GetSectionDelegate(name);
            if (action == null && isRequired)
                throw new ArgumentException("No section has been defined with name '" + name + "'");

            if (action == null) action = () => { };

            return new TemplateWriter(tw => {
                _context.PopSections(action);
            });
        }

        /// <summary>
        /// Renders the body of the template.
        /// </summary>
        /// <returns>The template writer helper.</returns>
        public TemplateWriter RenderBody()
        {
            return _context.PopBody();
        }

        /// <summary>
        /// Writes the specified object to the result.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public virtual void Write(object value)
        {
            WriteTo(_context.CurrentWriter, value);
        }

        /// <summary>
        /// Writes the specified template helper result.
        /// </summary>
        /// <param name="helper">The template writer helper.</param>
        public virtual void Write(TemplateWriter helper)
        {
            if (helper == null)
                return;

            helper.WriteTo(_context.CurrentWriter);
        }

        /// <summary>
        /// Writes an attribute to the result.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        public virtual void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            WriteAttributeTo(CurrentWriter, name, prefix, suffix, values);
        }

        /// <summary>
        /// Writes an attribute to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the attribute to be written.</param>
        public virtual void WriteAttributeTo(TextWriter writer, string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            bool first = true;
            bool wroteSomething = false;
            if (values.Length == 0)
            {
                // Explicitly empty attribute, so write the prefix and suffix
                WritePositionTaggedLiteral(writer, prefix);
                WritePositionTaggedLiteral(writer, suffix);
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    AttributeValue attrVal = values[i];
                    PositionTagged<object> val = attrVal.Value;

                    bool? boolVal = null;
                    if (val.Value is bool)
                    {
                        boolVal = (bool)val.Value;
                    }

                    if (val.Value != null && (boolVal == null || boolVal.Value))
                    {
                        string valStr = val.Value as string;
                        string valToString = valStr;
                        if (valStr == null)
                        {
                            valToString = val.Value.ToString();
                        }
                        if (boolVal != null)
                        {
                            Debug.Assert(boolVal.Value);
                            valToString = name;
                        }

                        if (first)
                        {
                            WritePositionTaggedLiteral(writer, prefix);
                            first = false;
                        }
                        else
                        {
                            WritePositionTaggedLiteral(writer, attrVal.Prefix);
                        }

                        if (attrVal.Literal)
                        {
                            WriteLiteralTo(writer, valToString);
                        }
                        else
                        {
                            if (val.Value is IEncodedString && boolVal == null)
                            {
                                WriteTo(writer, val.Value); // Write value
                            }
                            else
                            {
                                WriteTo(writer, valToString); // Write value
                            }
                        }
                        wroteSomething = true;
                    }
                }
                if (wroteSomething)
                {
                    WritePositionTaggedLiteral(writer, suffix);
                }
            }
        }

        /// <summary>
        /// Writes the specified string to the result.
        /// </summary>
        /// <param name="literal">The literal to write.</param>
        public virtual void WriteLiteral(string literal)
        {
            WriteLiteralTo(_context.CurrentWriter, literal);
        }

        /// <summary>
        /// Writes a string literal to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="literal">The literal to be written.</param>
        public virtual void WriteLiteralTo(TextWriter writer, string literal)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (literal == null) return;
            writer.Write(literal);
        }

        /// <summary>
        /// Writes a <see cref="PositionTagged{string}" /> literal to the result.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="literal">The literal to be written.</param>
        private void WritePositionTaggedLiteral(TextWriter writer, PositionTagged<string> value)
        {
            WriteLiteralTo(writer, value.Value);
        }

        /// <summary>
        /// Writes the specified object to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value to be written.</param>
        public virtual void WriteTo(TextWriter writer, object value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (value == null) return;

            var encodedString = value as IEncodedString;
            if (encodedString != null)
            {
                writer.Write(encodedString);
            }
            else
            {
                encodedString = InternalTemplateService.EncodedStringFactory.CreateEncodedString(value);
                writer.Write(encodedString);
            }
        }

        /// <summary>
        /// Writes the specfied template helper result to the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="helper">The template writer helper.</param>
        public virtual void WriteTo(TextWriter writer, TemplateWriter helper)
        {
            if (helper == null) return;

            helper.WriteTo(writer);
        }

        /// <summary>
        /// Resolves the specified path
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The resolved path.</returns>
        public virtual string ResolveUrl(string path)
        {
            // TODO: Actually resolve the url
            if (path.StartsWith("~"))
            {
                path = path.Substring(1);
            }
            return path;
        }
        #endregion
    }
}