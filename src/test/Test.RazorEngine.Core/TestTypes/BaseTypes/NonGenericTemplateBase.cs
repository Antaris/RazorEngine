using RazorEngine.Templating;

namespace RazorEngine.Tests.TestTypes.BaseTypes
{
    public abstract class NonGenericTemplateBase : TemplateBase
    {
        public string GetHelloWorldText()
        {
            return "Hello World";
        }
    }
}
