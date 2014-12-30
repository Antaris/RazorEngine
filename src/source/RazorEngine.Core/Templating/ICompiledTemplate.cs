using RazorEngine.Compilation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// Represents a compiled template.
    /// </summary>
    public interface ICompiledTemplate
    {
        /// <summary>
        /// The key for the template (used for resolving the source code).
        /// </summary>
        ITemplateKey Key { get; }

        /// <summary>
        /// The source of the template (ie the source code).
        /// </summary>
        ITemplateSource Template { get; }

        /// <summary>
        /// All temporary information about the compilation.
        /// </summary>
        CompilationData CompilationData { get; }

        /// <summary>
        /// The actual Type object of the generated template.
        /// </summary>
        Type TemplateType { get; }

        /// <summary>
        /// The generated assembly of the template.
        /// </summary>
        Assembly TemplateAssembly { get; }

        /// <summary>
        /// The type of the model (null = dynamic).
        /// </summary>
        Type ModelType { get; }
    }
}
