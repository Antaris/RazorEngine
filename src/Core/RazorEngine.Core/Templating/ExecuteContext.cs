//-----------------------------------------------------------------------------
// <copyright file="ExecuteContext.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
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

        /// <summary>
        /// The sections
        /// </summary>
        private readonly IDictionary<string, Action> definedSections = new Dictionary<string, Action>();

        /// <summary>
        /// The body writers
        /// </summary>
        private readonly Stack<TemplateWriter> bodyWriters = new Stack<TemplateWriter>();

        /// <summary>
        /// The dynamic view bags
        /// </summary>
        private readonly dynamic dynamicViewBag;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteContext"/> class.
        /// </summary>
        public ExecuteContext()
        {
            this.dynamicViewBag = new DynamicViewBag();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteContext"/> class.
        /// </summary>
        /// <param name="viewBag">The initial view bag data or NULL for an empty ViewBag.</param>
        public ExecuteContext(DynamicViewBag viewBag)
        {
            this.dynamicViewBag = viewBag ?? new DynamicViewBag();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the view bag that allows sharing state.
        /// </summary>
        public dynamic ViewBag
        {
            get
            {
                return this.dynamicViewBag;
            }
        }

        /// <summary>
        /// Gets or sets the current writer.
        /// </summary>
        /// <value>
        /// The current writer.
        /// </value>
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
            {
                throw new ArgumentException("A name is required to define a section.");
            }

            if (this.definedSections.ContainsKey(name))
            {
                throw new ArgumentException("A section has already been defined with name '" + name + "'");
            }

            this.definedSections.Add(name, action);
        }

        /// <summary>
        /// Gets the section delegate.
        /// </summary>
        /// <param name="name">The name of the section.</param>
        /// <returns>The section delegate.</returns>
        public Action GetSectionDelegate(string name)
        {
            if (this.definedSections.ContainsKey(name))
            {
                return this.definedSections[name];
            }

            return null;
        }

        /// <summary>
        /// Pops the template writer helper off the stack.
        /// </summary>
        /// <returns>The template writer helper.</returns>
        internal TemplateWriter PopBody()
        {
            return this.bodyWriters.Pop();
        }

        /// <summary>
        /// Pushes the specified template writer helper onto the stack.
        /// </summary>
        /// <param name="bodyWriter">The template writer helper.</param>
        internal void PushBody(TemplateWriter bodyWriter)
        {
            if (bodyWriter == null)
            {
                throw new ArgumentNullException("bodyWriter");
            }

            this.bodyWriters.Push(bodyWriter);
        }

        #endregion
    }
}