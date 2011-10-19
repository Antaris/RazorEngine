namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Defines a context for tracking template execution.
    /// </summary>
    public class ExecuteContext
    {
        #region Fields
        private readonly IDictionary<string, Action> _definedSections = new Dictionary<string, Action>();
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
        public void DefineSection(string name, Action action)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A name is required to define a section.");

            if (_definedSections.ContainsKey(name))
                throw new ArgumentException("A section has already been defined with name '" + name + "'");

            _definedSections.Add(name, action);
        }

        /// <summary>
        /// Gets the section delegate.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <returns>The section delegate.</returns>
        public Action GetSectionDelegate(string name)
        {
            if (_definedSections.ContainsKey(name))
                return _definedSections[name];

            return null;
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