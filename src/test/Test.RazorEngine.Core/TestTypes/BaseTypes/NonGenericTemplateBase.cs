using RazorEngine.Templating;

namespace RazorEngine.Tests.TestTypes.BaseTypes
{
    /// <summary>
    /// Test class.
    /// </summary>
    public abstract class NonGenericTemplateBase : TemplateBase
    {
        /// <summary>
        /// Test class.
        /// </summary>
        public string GetHelloWorldText()
        {
            return "Hello World";
        }
    }
}
