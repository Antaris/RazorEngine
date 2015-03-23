namespace RazorEngine.Hosts.Console
{
    using System;
    using System.Linq;

    using Compilation;
    using Templating;
    using RazorEngine.Text;
    using RazorEngine.Configuration;
    /// <summary>
    /// A simple helper demonstrating the @Html.Raw
    /// </summary>
    public class MyHtmlHelper
    {
        /// <summary>
        /// A simple helper demonstrating the @Html.Raw
        /// </summary>
        public IEncodedString Raw(string rawString)
        {
            return new RawString(rawString);
        }
    }

    /// <summary>
    /// A simple helper demonstrating the @Html.Raw
    /// </summary>
    public abstract class MyClassImplementingTemplateBase<T> : TemplateBase<T>
    {
        /// <summary>
        /// A simple helper demonstrating the @Html.Raw
        /// </summary>
        public MyClassImplementingTemplateBase()
        {
            Html = new MyHtmlHelper();
        }

        /// <summary>
        /// A simple helper demonstrating the @Html.Raw
        /// </summary>
        public MyHtmlHelper Html { get; set; }
    }

    class Program
    {

        static void QuickStart_1()
        {
            string template = "Hello @Model.Name, welcome to RazorEngine!";
            var result =
                Engine.Razor.RunCompile(template, "templateKey", null, new { Name = "World" });

            Console.WriteLine("Template: {0}", template);
            Console.WriteLine("Result: {0}", result);
        }

        static void QuickStart_2()
        {
            var result =
                Engine.Razor.Run("templateKey", null, new { Name = "Max" });

            Console.WriteLine("Result (templateKey): {0}", result);
        }


        public class MyTemplateManager : ITemplateManager
        {
            public ITemplateSource Resolve(ITemplateKey key)
            {
                // Resolve your template here
                string template = "Hello @Model.Name, welcome to RazorEngine!";
                // Provide a non-null file to improve debugging
                return new LoadedTemplateSource(template, null);
            }

            public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
            {
                // If you can have different templates with the same name depending on the 
                // context or the resolveType you need your own implementation here!
                // Otherwise you can just use NameOnlyTemplateKey.
                return new NameOnlyTemplateKey(name, resolveType, context);
            }

            public void AddDynamic(ITemplateKey key, ITemplateSource source)
            {
                // You can disable dynamic templates completely, but 
                // then all convenience methods (Compile and RunCompile) with
                // a TemplateSource will no longer work (they are not really needed anyway).
                throw new NotImplementedException("dynamic templates are not supported!");
            }
        }
        static void QuickStart_TemplateManager()
        {
            var result =
                Engine.Razor.Run("templateKey", null, new { Name = "Max" });

            Console.WriteLine("Result (templateKey): {0}", result);
        }

        static void RawEncoding()
        {
            string template = "@Raw(Model.Data)";
            var model = new { Data = "My raw double quotes appears here \"hello!\"" };

            string result = Engine.Razor.RunCompile(template, "rawTemplate", null, model);
            Console.WriteLine("Template: {0}", template);
            Console.WriteLine("Result: {0}", result);
        }

        static void HtmlRawEncoding()
        {
            var config = new TemplateServiceConfiguration();
            config.BaseTemplateType = typeof(MyClassImplementingTemplateBase<>);
            using (var service = RazorEngineService.Create(config))
            {
                string template = "@Html.Raw(Model.Data)";
                var model = new { Data = "My raw double quotes appears here \"hello!\"" };

                string result = service.RunCompile(template, "htmlRawTemplate", null, model);
                Console.WriteLine("Template: {0}", template);
                Console.WriteLine("Result: {0}", result);
            }
        }

        static void Main(string[] args)
        {
            QuickStart_1();
            QuickStart_2();
            QuickStart_TemplateManager();
            RawEncoding();
            HtmlRawEncoding();
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Defines a person.
    /// </summary>
    [Serializable]
    public class Person
    {
        #region Properties
        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the forename.
        /// </summary>
        public string Forename { get; set; }

        /// <summary>
        /// Gets or sets the surname.
        /// </summary>
        public string Surname { get; set; }
        #endregion
    }
}
