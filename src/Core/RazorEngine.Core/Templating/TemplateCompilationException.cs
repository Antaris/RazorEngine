//-----------------------------------------------------------------------------
// <copyright file="TemplateCompilationException.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines an exception that occurs during compilation of the template.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Reviewed. Suppression is OK here."), Serializable]
    public class TemplateCompilationException : Exception
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateCompilationException"/> class.
        /// </summary>
        /// <param name="errors">The set of compiler errors.</param>
        /// <param name="sourceCode">The source code that wasn't compiled.</param>
        /// <param name="template">The source template that wasn't compiled.</param>
        internal TemplateCompilationException(CompilerErrorCollection errors, string sourceCode, string template)
            : base("Unable to compile template. " + errors[0].ErrorText + "\n\nOther compilation errors may have occurred. Check the Errors property for more information.")
        {
            var list = errors.Cast<CompilerError>().ToList();
            this.Errors = new ReadOnlyCollection<CompilerError>(list);
            this.SourceCode = sourceCode;
            this.Template = template;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateCompilationException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        protected TemplateCompilationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int count = info.GetInt32("Count");

            var list = new List<CompilerError>();
            var type = typeof(CompilerError);

            for (int i = 0; i < count; i++)
            {
                list.Add((CompilerError)info.GetValue("Errors[" + i + "]", type));
            }

            this.Errors = new ReadOnlyCollection<CompilerError>(list);

            this.SourceCode = info.GetString("SourceCode");
            this.Template = info.GetString("Template");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the set of compiler errors.
        /// </summary>
        public ReadOnlyCollection<CompilerError> Errors { get; private set; }

        /// <summary>
        /// Gets the source code that wasn't compiled.
        /// </summary>
        public string SourceCode { get; private set; }

        /// <summary>
        /// Gets the source template that wasn't compiled.
        /// </summary>
        public string Template { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the object data for serialization.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Count", this.Errors.Count);

            for (int i = 0; i < this.Errors.Count; i++)
            {
                info.AddValue("Errors[" + i + "]", this.Errors[i]);
            }

            info.AddValue("SourceCode", this.SourceCode ?? string.Empty);
            info.AddValue("Template", this.Template ?? string.Empty);
        }

        #endregion
    }
}