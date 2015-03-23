namespace RazorEngine.Templating
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Text;
#if RAZOR4
    using SectionAction = System.Action<System.IO.TextWriter>;
#else
    using SectionAction = System.Action;
#endif

    /// <summary>
    /// Provides a base implementation of a template.
    /// NOTE: This class is not serializable to prevent subtle errors 
    /// in user IActivator implementations which would break the sandbox.
    /// (because executed in the wrong <see cref="AppDomain"/>)
    /// </summary>
    public abstract class TemplateBase : ITemplate
    {
        #region Fields
        /// <summary>
        /// Because the old API (TemplateService) is designed in a way to make it impossible to init
        /// the model and the Viewbag at the same time (and because of backwards compatibility),
        /// we need to call the SetData method twice (only from within TemplateService so we can remove this bool once that has been removed).
        /// 
        /// But the secound call we only need to set the Viewbag, therefore we save the state in this bool.
        /// </summary>
        private bool modelInit = false;
        private dynamic viewBag = null;
        /// <summary>
        /// The current context, filled when we are currently writing a template instance.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the template service.
        /// </summary>
        [Obsolete("Only provided for backwards compatibility, use RazorEngine instead.")]
        public ITemplateService TemplateService { get; set; }
#if RAZOR4
#else
        /// <summary>
        /// Gets or sets the current <see cref="IRazorEngineService"/> instance.
        /// </summary>
        [Obsolete("Use the Razor property instead, this is obsolete as it makes it difficult to use the RazorEngine namespace within templates.")]
        public IRazorEngineService RazorEngine { get { return Razor; } set { Razor = value; } }
#endif
        /// <summary>
        /// Gets or sets the current <see cref="IRazorEngineService"/> instance.
        /// </summary>
        public IRazorEngineService Razor { get; set; }

        /// <summary>
        /// Gets the viewbag that allows sharing state between layout and child templates.
        /// </summary>
        public dynamic ViewBag { get { return viewBag; } }

        /// <summary>
        /// Gets the current writer.
        /// </summary>
        public TextWriter CurrentWriter { get { return _context.CurrentWriter; } }
        #endregion

        #region Methods
        /// <summary>
        /// Set the data for this template.
        /// </summary>
        /// <param name="model">the model object for the current run.</param>
        /// <param name="viewbag">the viewbag for the current run.</param>
        public virtual void SetData(object model, DynamicViewBag viewbag)
        {
            this.viewBag = viewbag ?? ViewBag ?? new DynamicViewBag();
            if (!modelInit)
            {
                SetModel(model);
                modelInit = true;
            }
        }

        /// <summary>
        /// Set the current model.
        /// </summary>
        /// <param name="model"></param>
        public virtual void SetModel(object model)
        {

        }

        /// <summary>
        /// Defines a section that can written out to a layout.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="action">The delegate used to write the section.</param>
        public void DefineSection(string name, SectionAction action)
        {
            _context.DefineSection(name, action);
        }

        /// <summary>
        /// Includes the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <param name="modelType"></param>
        /// <returns>The template writer helper.</returns>
        public virtual TemplateWriter Include(string name, object model = null, Type modelType = null)
        {
            var instance = InternalTemplateService.Resolve(name, model, modelType, (DynamicViewBag)ViewBag, ResolveType.Include);
            if (instance == null)
                throw new ArgumentException("No template could be resolved with name '" + name + "'");

            // TODO: make TemplateWriter async?
            return new TemplateWriter(tw =>
                instance.Run(
                    InternalTemplateService.CreateExecuteContext(), tw)
#if RAZOR4
                    .Wait()
#endif
                    );
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
#if RAZOR4
        public virtual Task Execute() { return Task.FromResult(0); }
#else
        public virtual void Execute() { }
#endif

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
            return InternalTemplateService.Resolve(name, null, null, (DynamicViewBag)ViewBag, ResolveType.Layout);
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
        /// <param name="reader"></param>
        /// <returns>The merged result of the template.</returns>
#if RAZOR4
        public async Task Run(ExecuteContext context, TextWriter reader)
#else
        void ITemplate.Run(ExecuteContext context, TextWriter reader)
#endif
        {
            _context = context;

            using (var memory = new MemoryStream())
            {
                using (var writer = new StreamWriter(memory))
                {
                    _context.CurrentWriter = writer;
#if RAZOR4
                    await Execute();
#else
                    Execute();
#endif
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

#if RAZOR4
                        await layout.Run(context, reader);
#else
                        layout.Run(context, reader);
#endif
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

            if (action == null) 
#if RAZOR4
                action = (tw) => { };
#else
                action = () => { };
#endif

            return new TemplateWriter(tw => {
                _context.PopSections(action, tw);
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
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="values"></param>
        public virtual void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            WriteAttributeTo(CurrentWriter, name, prefix, suffix, values);
        }

        /// <summary>
        /// Writes an attribute to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the attribute to be written.</param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <param name="values"></param>
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
        /// Writes a <see cref="PositionTagged{T}" /> literal to the result.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The literal to be written.</param>
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