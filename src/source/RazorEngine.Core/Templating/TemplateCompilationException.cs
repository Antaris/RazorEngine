namespace RazorEngine.Templating
{
    using RazorEngine.Compilation;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security;

    /// <summary>
    /// Defines an exception that occurs during compilation of the template.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), Serializable]
    public class TemplateCompilationException : Exception
    {
        internal static string Separate(string rawLines)
        {
            const string seperator = "\n------------- START -----------\n{0}\n------------- END -----------\n";
            return string.Format(seperator, rawLines);
        }

        /// <summary>
        /// Gets a exact error message of the given error collection
        /// </summary>
        /// <param name="errors"></param>
        /// <param name="files"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        internal static string GetMessage(CompilerErrorCollection errors, CompilationData files, ITemplateSource template)
        {
            var errorMsgs = string.Join("\n\t", errors.Cast<CompilerError>().Select(error => string.Format(" - {0}: ({1}, {2}) {3}", error.IsWarning ? "warning" : "error", error.Line, error.Column, error.ErrorText)));

            const string rawTemplateFileMsg = "The template-file we tried to compile is: {0}\n";
            const string rawTemplate = "The template we tried to compile is: {0}\n";
            const string rawTmpFiles = "Temporary files of the compilation can be found in (please delete the folder): {0}\n";
            const string rawSourceCode = "The generated source code is: {0}\n";

            string templateFileMsg;
            if (string.IsNullOrEmpty(template.TemplateFile)) {
                templateFileMsg = string.Format(rawTemplate, Separate(template.Template ?? string.Empty));
	        } else{
                templateFileMsg = string.Format(rawTemplateFileMsg, template.TemplateFile ?? string.Empty);
            }
            string tempFilesMsg = string.Empty;
            if (files.TmpFolder != null)
            {
                tempFilesMsg = string.Format(rawTmpFiles, files.TmpFolder);
            }

            string sourceCodeMessage = string.Empty;
            if (files.SourceCode != null)
            {
                sourceCodeMessage = string.Format(rawSourceCode, Separate(files.SourceCode));
            }

            var rawMessage = @"Errors while compiling a Template.
Please try the following to solve the situation:
  * If the problem is about missing references either try to load the missing references manually (in the current appdomain) or
    Specify your references manually by providing your own IAssemblyReferenceResolver implementation.
    Currently all references have to be available as files!
  * If you get 'class' does not contain a definition for 'member': 
        try another modelType (for example 'typeof(DynamicObject)' to make the model dynamic).
    NOTE: You CANNOT use the generic overloads with the <dynamic> type parameter for this.
    Or try to use static instead of anonymous types.
More details about the error:
{0}
{1}{2}{3}";
            return string.Format(rawMessage, errorMsgs, tempFilesMsg, templateFileMsg, sourceCodeMessage);
        }

        #region Constructors
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateCompilationException"/>.
        /// </summary>
        /// <param name="errors">The set of compiler errors.</param>
        /// <param name="files">The source code that wasn't compiled.</param>
        /// <param name="template">The source template that wasn't compiled.</param>
        internal TemplateCompilationException(CompilerErrorCollection errors, CompilationData files, ITemplateSource template)
            : base(TemplateCompilationException.GetMessage(errors, files, template))
        {
            var list = errors.Cast<CompilerError>().ToList();
            Errors = new ReadOnlyCollection<CompilerError>(list);
            CompilationData = files;
            Template = template.Template;
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
            var sourceCode = info.GetString("SourceCode");
            if (string.IsNullOrEmpty(sourceCode))
            {
                sourceCode = null;
            }
            var tmpFolder = info.GetString("TmpFolder");
            if (string.IsNullOrEmpty(tmpFolder))
            {
                tmpFolder = null;
            }
            CompilationData = new CompilationData(sourceCode, tmpFolder);
            Template = info.GetString("Template");
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the set of compiler errors.
        /// </summary>
        public ReadOnlyCollection<CompilerError> Errors { get; private set; }

        /// <summary>
        /// Gets some copilation specific (temporary) data.
        /// </summary>
        public CompilationData CompilationData { get; private set; }

        /// <summary>
        /// Gets the generated source code.
        /// </summary>
        public string SourceCode 
        { 
            get
            {
                return CompilationData.SourceCode;
            }
        }

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
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Count", Errors.Count);

            for (int i = 0; i < Errors.Count; i++)
                info.AddValue("Errors[" + i + "]", Errors[i]);

            info.AddValue("SourceCode", CompilationData.SourceCode ?? string.Empty);
            info.AddValue("TmpFolder", CompilationData.TmpFolder ?? string.Empty);
            info.AddValue("Template", Template ?? string.Empty);
        }
        #endregion
    }
}