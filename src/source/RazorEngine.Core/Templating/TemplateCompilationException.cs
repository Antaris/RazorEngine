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
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), Serializable]
    public class TemplateCompilationException : Exception
    {
        /// <summary>
        /// Gets a exact error message of the given error collection
        /// </summary>
        /// <param name="errors"></param>
        /// <param name="sourceCode"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        internal static string GetMessage(CompilerErrorCollection errors, string sourceCode, string template) {
            var errorMsgs = string.Join("\n\t", errors.Cast<CompilerError>().Select(error => string.Format(" - {0}: ({1}, {2}) {3}", error.IsWarning ? "warning" : "error", error.Line, error.Column, error.ErrorText)));
            return 
                string.Format("Unable to compile template. You can find the generated source code in {0}. \n\t{1}", sourceCode, errorMsgs);
        }

        #region Constructors
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateCompilationException"/>.
        /// </summary>
        /// <param name="errors">The set of compiler errors.</param>
        /// <param name="sourceCode">The source code that wasn't compiled.</param>
        /// <param name="template">The source template that wasn't compiled.</param>
        internal TemplateCompilationException(CompilerErrorCollection errors, string sourceCode, string template)
            : base(TemplateCompilationException.GetMessage(errors, sourceCode, template))
        {
            var list = errors.Cast<CompilerError>().ToList();
            Errors = new ReadOnlyCollection<CompilerError>(list);
            SourceCode = sourceCode;
            Template = template;
        }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateCompilationException"/> from serialised data.
        /// </summary>
        /// <param name="info">The serialisation info.</param>
        /// <param name="context">The streaming context.</param>
        protected TemplateCompilationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            int count = info.GetInt32("Count");

            var list = new List<CompilerError>();
            var type = typeof(CompilerError);

            for (int i = 0; i < count; i++)
            {
                list.Add((CompilerError)info.GetValue("Errors[" + i + "]", type));
            }

            Errors = new ReadOnlyCollection<CompilerError>(list);

            SourceCode = info.GetString("SourceCode");
            Template = info.GetString("Template");
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
        /// Gets the object data for serialisation.
        /// </summary>
        /// <param name="info">The serialisation info.</param>
        /// <param name="context">The streaming context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Count", Errors.Count);

            for (int i = 0; i < Errors.Count; i++)
                info.AddValue("Errors[" + i + "]", Errors[i]);

            info.AddValue("SourceCode", SourceCode ?? string.Empty);
            info.AddValue("Template", Template ?? string.Empty);
        }
        #endregion
    }
}