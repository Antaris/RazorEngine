namespace RazorEngine.CodeGenerators
{
    using System;
    using System.Globalization;
#if RAZOR4
    using Microsoft.AspNet.Razor.Generator;
    using Microsoft.AspNet.Razor.Parser.SyntaxTree;
#else
    using System.Web.Razor.Generator;
#endif
    using Common;
    using System.Security;
    using RazorEngine.Compilation;

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
    [SecurityCritical]
#endif
    internal class SetModelTypeCodeGenerator : SetBaseTypeCodeGenerator
    {
        private readonly Func<Type, string, string> _builtBaseTypeName;

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecurityCritical]
#endif
        public SetModelTypeCodeGenerator(string modelType, Func<Type, string, string> builtBaseTypeName)
            : base(modelType)
        {
            _builtBaseTypeName = builtBaseTypeName;
        }

        private string GetBaseType(CodeGeneratorContext context, string baseType)
        {
            var host = (RazorEngineHost)context.Host;
            return _builtBaseTypeName(host.DefaultBaseTemplateType, baseType);
        }
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecurityCritical]
#endif
#if RAZOR4
        public override void GenerateCode(Span target, CodeGeneratorContext context)
        {
            context.CodeTreeBuilder.AddSetBaseTypeChunk(GetBaseType(context, BaseType), target);
        }
#else
        protected override string ResolveType(CodeGeneratorContext context, string baseType)
        {
            return GetBaseType(context, baseType);
        }
#endif

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecuritySafeCritical]
#endif
        public override bool Equals(object obj)
        {
            SetModelTypeCodeGenerator other = obj as SetModelTypeCodeGenerator;
            return other != null &&
                   base.Equals(obj);
        }

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecuritySafeCritical]
#endif
        public override int GetHashCode()
        {
            return HashCodeCombiner.Start()
                .Add(base.GetHashCode())
                .CombinedHash;
        }

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecuritySafeCritical]
#endif
        public override string ToString()
        {
            return "Model:" + BaseType;
        }
    }
}
