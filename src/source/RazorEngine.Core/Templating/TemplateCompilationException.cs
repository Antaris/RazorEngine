namespace RazorEngine.Templating
{
    using RazorEngine.Compilation;
    using System;
#if !RAZOR4
    using System.CodeDom.Compiler;
#endif
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security;

    /// <summary>
    /// Defines a compiler error.
    /// </summary>
    [Serializable]
    public class RazorEngineCompilerError
    {
        /// <summary>
        /// The error text of the error.
        /// </summary>
        public string ErrorText { get; private set; }
        /// <summary>
        /// The file name of the error source.
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// The line number of the error location
        /// </summary>
        public int Line { get; private set; }
        /// <summary>
        /// The column number of the error location.
        /// </summary>
        public int Column { get; private set; }
        /// <summary>
        /// The number of the error.
        /// </summary>
        public string ErrorNumber { get; private set; }
        /// <summary>
        /// Indicates whether the error is a warning.
        /// </summary>
        public bool IsWarning { get; private set; }
        /// <summary>
        /// Creates a new Compiler error instance.
        /// </summary>
        /// <param name="errorText"></param>
        /// <param name="fileName"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <param name="errorNumber"></param>
        /// <param name="isWarning"></param>
        public RazorEngineCompilerError(string errorText, string fileName, int line, int column, string errorNumber, bool isWarning)
        {
            ErrorText = errorText;
            FileName = fileName;
            Line = line;
            Column = column;
            ErrorNumber = errorNumber;
            IsWarning = isWarning;
        }
    }

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
        internal static string GetMessage(IEnumerable<RazorEngineCompilerError> errors, CompilationData files, ITemplateSource template)
        {
            var errorMsgs = string.Join("\n\t", errors.Select(error =>
                string.Format(
                    " - {0}: ({1}, {2}) {3}", 
                    error.IsWarning ? "warning" : "error", 
                    error.Line, error.Column, error.ErrorText)));

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

            string loadedAssemblies =
                "\nList of loaded Assemblies:\n" +
                string.Join("\n\tLoaded Assembly: ",
                    (new Compilation.ReferenceResolver.UseCurrentAssembliesReferenceResolver())
                    .GetReferences().Select(r => r.GetFile()));

            var rawMessage = @"Errors while compiling a Template.
Please try the following to solve the situation:
  * If the problem is about missing/invalid references or multiple defines either try to load 
    the missing references manually (in the compiling appdomain!) or
    Specify your references manually by providing your own IReferenceResolver implementation.
    See https://antaris.github.io/RazorEngine/ReferenceResolver.html for details.
    Currently all references have to be available as files!
  * If you get 'class' does not contain a definition for 'member': 
        try another modelType (for example 'null' to make the model dynamic).
        NOTE: You CANNOT use typeof(dynamic) to make the model dynamic!
    Or try to use static instead of anonymous/dynamic types.
More details about the error:
{0}
{1}{2}{3}{4}";
            return string.Format(rawMessage, errorMsgs, tempFilesMsg, templateFileMsg, sourceCodeMessage, loadedAssemblies);
        }

        #region Constructors
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateCompilationException"/>.
        /// </summary>
        /// <param name="errors">The set of compiler errors.</param>
        /// <param name="files">The source code that wasn't compiled.</param>
        /// <param name="template">The source template that wasn't compiled.</param>
        public TemplateCompilationException(IEnumerable<RazorEngineCompilerError> errors, CompilationData files, ITemplateSource template)
            : base(TemplateCompilationException.GetMessage(errors, files, template))
        {
            var list = errors.ToList();
            CompilerErrors = new ReadOnlyCollection<RazorEngineCompilerError>(list);
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

            var list = new List<RazorEngineCompilerError>();
            var type = typeof(RazorEngineCompilerError);

            for (int i = 0; i < count; i++)
            {
                list.Add((RazorEngineCompilerError)info.GetValue("CompilerErrors[" + i + "]", type));
            }

            CompilerErrors = new ReadOnlyCollection<RazorEngineCompilerError>(list);
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
        public ReadOnlyCollection<RazorEngineCompilerError> CompilerErrors { get; private set; }

#if !RAZOR4
        /// <summary>
        /// Gets the set of compiler errors.
        /// </summary>
        [Obsolete("Use CompilerErrors instead, will be removed in 4.0.0")]
        public ReadOnlyCollection<CompilerError> Errors
        {
            get
            {
                return new ReadOnlyCollection<CompilerError>(
                    CompilerErrors.Select(error => new CompilerError(error.FileName, error.Line, error.Column, error.ErrorNumber, error.ErrorText)).ToList());
            }
        }
#endif

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

            info.AddValue("Count", CompilerErrors.Count);

            for (int i = 0; i < CompilerErrors.Count; i++)
                info.AddValue("CompilerErrors[" + i + "]", CompilerErrors[i]);

            info.AddValue("SourceCode", CompilationData.SourceCode ?? string.Empty);
            info.AddValue("TmpFolder", CompilationData.TmpFolder ?? string.Empty);
            info.AddValue("Template", Template ?? string.Empty);
        }
        #endregion
    }
}