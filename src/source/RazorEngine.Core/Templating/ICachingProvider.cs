using System;
namespace RazorEngine.Templating
{
    public interface ICachingProvider : IDisposable
    {
        void CacheTemplate(ICompiledTemplate template, ITemplateKey key);

        bool TryRetrieveTemplate(ITemplateKey key, Type modelType, out ICompiledTemplate template);

        TypeLoader TypeLoader { get; }
    }
}
