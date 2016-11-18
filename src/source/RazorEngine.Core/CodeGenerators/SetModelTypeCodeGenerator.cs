namespace RazorEngine.CodeGenerators
{
    using System;
    using System.Globalization;
#if RAZOR4
    using Microsoft.AspNetCore.Razor.Chunks.Generators;
    using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
#else
    using System.Web.Razor.Generator;
#endif
    using Common;
    using System.Security;
    using RazorEngine.Compilation;

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
    [SecurityCritical]
#endif
#if RAZOR4
    internal class SetModelTypeCodeGenerator : SetBaseTypeChunkGenerator
#else
    internal class SetModelTypeCodeGenerator : SetBaseTypeCodeGenerator
#endif
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

#if RAZOR4
        private string GetBaseType(ChunkGeneratorContext context, string baseType)
#else
        private string GetBaseType(CodeGeneratorContext context, string baseType)
#endif
        {
            var host = (RazorEngineHost)context.Host;
            return _builtBaseTypeName(host.DefaultBaseTemplateType, baseType);
        }
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecurityCritical]
#endif
#if RAZOR4
        public override void GenerateChunk(Span target, ChunkGeneratorContext context)
        {
            context.ChunkTreeBuilder.AddSetBaseTypeChunk(GetBaseType(context, BaseType), target);
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
