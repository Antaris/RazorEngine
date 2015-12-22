namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;

#if RAZOR4
    using SectionAction = System.Action<System.IO.TextWriter>;
#else
    using SectionAction = System.Action;
#endif

    /// <summary>
    /// Defines a context for tracking template execution.
    /// </summary>
    public class ExecuteContext
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of ExecuteContext.
        /// </summary>
        public ExecuteContext()
        {
            _currentSectionStack.Push(new HashSet<string>());
        }
        /// <summary>
        /// DO NOT USE, throws NotSupportedException.
        /// </summary>
        /// <param name="viewbag">DO NOT USE, throws NotSupportedException.</param>
        [Obsolete("RUNTIME FAILURE: This kind of usage is no longer supported.")]
        public ExecuteContext(DynamicViewBag viewbag)
        {
            throw new NotSupportedException("This kind of usage is no longer supported!");
        }
        #endregion

        #region Fields
        private readonly Stack<ISet<string>> _currentSectionStack = new Stack<ISet<string>>();
        private ISet<string> _currentSections = new HashSet<string>();
        private readonly IDictionary<string, Stack<SectionAction>> _definedSections = new Dictionary<string, Stack<SectionAction>>();
        private readonly Stack<TemplateWriter> _bodyWriters = new Stack<TemplateWriter>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current writer.
        /// </summary>
        //internal TextWriter CurrentWriter { get { return _writers.Peek(); } }
        internal TextWriter CurrentWriter { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Defines a section used in layouts.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <param name="action">The delegate action used to write the section at a later stage in the template execution.</param>
        public void DefineSection(string name, SectionAction action)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A name is required to define a section.");
            if (_currentSections.Contains(name))
	            throw new ArgumentException("A section has already been defined with name '" + name + "'");

            _currentSections.Add(name);
            Stack<SectionAction> sectionStack;
            if (!_definedSections.TryGetValue(name, out sectionStack))
            {
                sectionStack = new Stack<SectionAction>();
                _definedSections.Add(name, sectionStack);
            }
            sectionStack.Push(action);
        }

        /// <summary>
        /// Gets the section delegate.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <returns>The section delegate.</returns>
        public SectionAction GetSectionDelegate(string name)
        {
            if (_definedSections.ContainsKey(name) && _definedSections[name].Count > 0)
                return _definedSections[name].Peek();

            return null;
        }

        /// <summary>
        /// Allows to pop all the section delegates for the executing action.
        /// This is required for nesting sections.
        /// </summary>
        /// <param name="inner">the executing section delegate.</param>
        /// <param name="innerArg">the parameter for the delegate.</param>
        internal void PopSections(SectionAction inner, TextWriter innerArg)
        {
            var oldsections = _currentSections;
            _currentSections = _currentSectionStack.Pop();
            var poppedSections = new List<Tuple<string, SectionAction>>();
            foreach (var section in _currentSections)
            {
                var item = _definedSections[section].Pop();
                poppedSections.Add(Tuple.Create(section, item));
            }
#if RAZOR4
            inner(innerArg);
#else
            var oldWriter = CurrentWriter;
            try
            {
                CurrentWriter = innerArg;
                inner();
            }
            finally
            {
                CurrentWriter = oldWriter;
            }
#endif
            foreach (var item in poppedSections)
	        {
		        _definedSections[item.Item1].Push(item.Item2);
            }
            _currentSectionStack.Push(_currentSections);
            _currentSections = oldsections;
        }

        /// <summary>
        /// Push the set of current sections to the stack.
        /// </summary>
        internal void PushSections()
        {
            _currentSectionStack.Push(_currentSections);
            _currentSections = new HashSet<string>();
        }

        /// <summary>
        /// Pops the template writer helper off the stack.
        /// </summary>
        /// <returns>The template writer helper.</returns>
        internal TemplateWriter PopBody()
        {
            return _bodyWriters.Pop();
        }

        /// <summary>
        /// Pushes the specified template writer helper onto the stack.
        /// </summary>
        /// <param name="bodyWriter">The template writer helper.</param>
        internal void PushBody(TemplateWriter bodyWriter)
        {
            if (bodyWriter == null)
                throw new ArgumentNullException("bodyWriter");

            _bodyWriters.Push(bodyWriter);
        }
        #endregion
    }
}