namespace RazorEngine.CodeGenerators
{
    using System;
    using System.Globalization;
    using System.Web.Razor.Generator;

    internal class SetModelTypeCodeGenerator : SetBaseTypeCodeGenerator
    {
        private readonly string _genericTypeFormat;

        public SetModelTypeCodeGenerator(string modelType, string genericTypeFormat)
            : base(modelType)
        {
            _genericTypeFormat = genericTypeFormat;
        }

        protected override string ResolveType(CodeGeneratorContext context, string baseType)
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                _genericTypeFormat,
                context.Host.DefaultBaseClass,
                baseType);
        }

        public override bool Equals(object obj)
        {
            SetModelTypeCodeGenerator other = obj as SetModelTypeCodeGenerator;
            return other != null &&
                   base.Equals(obj) &&
                   String.Equals(_genericTypeFormat, other._genericTypeFormat, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (_genericTypeFormat ?? string.Empty).GetHashCode();
        }

        public override string ToString()
        {
            return "Model:" + BaseType;
        }
    }
}
