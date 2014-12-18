namespace RazorEngine.CodeGenerators
{
    using System;
    using System.Globalization;
    using System.Web.Razor.Generator;
    using Common;
    using System.Security;

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
    [SecurityCritical]
#endif
    internal class SetModelTypeCodeGenerator : SetBaseTypeCodeGenerator
    {
        private readonly string _genericTypeFormat;

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecurityCritical]
#endif
        public SetModelTypeCodeGenerator(string modelType, string genericTypeFormat)
            : base(modelType)
        {
            _genericTypeFormat = genericTypeFormat;
        }

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecurityCritical]
#endif
        protected override string ResolveType(CodeGeneratorContext context, string baseType)
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                _genericTypeFormat,
                context.Host.DefaultBaseClass,
                baseType);
        }

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecuritySafeCritical]
#endif
        public override bool Equals(object obj)
        {
            SetModelTypeCodeGenerator other = obj as SetModelTypeCodeGenerator;
            return other != null &&
                   base.Equals(obj) &&
                   String.Equals(_genericTypeFormat, other._genericTypeFormat, StringComparison.Ordinal);
        }

#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecuritySafeCritical]
#endif
        public override int GetHashCode()
        {
            return HashCodeCombiner.Start()
                .Add(base.GetHashCode())
                .Add(_genericTypeFormat)
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
