namespace RazorEngine.CodeGenerators
{
    using System;
    using System.Globalization;
    using System.Web.Razor.Generator;
    using Common;
    using System.Security;

    [SecurityCritical]
    internal class SetModelTypeCodeGenerator : SetBaseTypeCodeGenerator
    {
        private readonly string _genericTypeFormat;

        [SecurityCritical]
        public SetModelTypeCodeGenerator(string modelType, string genericTypeFormat)
            : base(modelType)
        {
            _genericTypeFormat = genericTypeFormat;
        }

        [SecurityCritical]
        protected override string ResolveType(CodeGeneratorContext context, string baseType)
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                _genericTypeFormat,
                context.Host.DefaultBaseClass,
                baseType);
        }

        [SecuritySafeCritical]
        public override bool Equals(object obj)
        {
            SetModelTypeCodeGenerator other = obj as SetModelTypeCodeGenerator;
            return other != null &&
                   base.Equals(obj) &&
                   String.Equals(_genericTypeFormat, other._genericTypeFormat, StringComparison.Ordinal);
        }

        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            return HashCodeCombiner.Start()
                .Add(base.GetHashCode())
                .Add(_genericTypeFormat)
                .CombinedHash;
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return "Model:" + BaseType;
        }
    }
}
