namespace RazorEngine.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Inspectors;
    using RazorEngine.Compilation.ReferenceResolver;
    using System.Security;

    /// <summary>
    /// Defines the required contract for implementing a compiler service.
    /// </summary>
    public interface ICompilerService : IDisposable
    {
        #region Properties
#if !RAZOR4
        /// <summary>
        /// Gets or sets the set of code inspectors.
        /// </summary>
        [Obsolete("This API is obsolete and will be removed in the next version (Razor4 doesn't use CodeDom for code-generation)!")]
        IEnumerable<ICodeInspector> CodeInspectors { get; set; }
#endif

        /// <summary>
        /// Gets or sets the reference resolver.
        /// </summary>
        IReferenceResolver ReferenceResolver { get; set; }
        
        /// <summary>
        /// Gets or sets whether the compiler service is operating in debug mode.
        /// </summary>
        bool Debug { get; set; }

        /// <summary>
        /// Gets or sets whether the compiler should load assemblies with Assembly.Load(byte[])
        /// to prevent files from being locked.
        /// </summary>
        bool DisableTempFileLocking { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Compiles the type defined in the specified type context.
        /// </summary>
        /// <param name="context">The type context which defines the type to compile.</param>
        /// <returns>The compiled type.</returns>
        [SecurityCritical]
        Tuple<Type, CompilationData> CompileType(TypeContext context);

        /// <summary>
        /// Returns a set of assemblies that must be referenced by the compiled template.
        /// </summary>
        /// <returns>The set of assemblies.</returns>
        IEnumerable<string> IncludeAssemblies();
        #endregion
    }
}