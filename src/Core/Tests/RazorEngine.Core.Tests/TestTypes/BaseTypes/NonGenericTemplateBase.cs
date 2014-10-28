using RazorEngine.Templating;

namespace RazorEngine.Tests.TestTypes.BaseTypes
{
    public abstract class NonGenericTemplateBase : TemplateBase
    {
		public string HelloWorldMessage = "Hello World";
        public string GetHelloWorldText()
        {
			return HelloWorldMessage;
        }
    }
}
