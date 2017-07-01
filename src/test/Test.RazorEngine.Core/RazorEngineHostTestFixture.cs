using RazorEngine.Tests.TestTypes.BaseTypes;

namespace RazorEngine.Tests
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    using Compilation;
    using Configuration;
    using Templating;
    using Text;
    using TestTypes;

    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="TemplateBase"/> type.
    /// </summary>
    [TestFixture]
    public class RazorEngineHostTestFixture
    {
        #region Tests
        /// <summary>
        /// Tests that the <see cref="RazorEngineHost"/> supports the @model directive.
        /// </summary>
        /// <remarks>
        /// As with it's MVC counterpart, we've added support for the @model declaration. This is to enable scenarios
        /// where the model type might be unknown, but we can pass in an instance of <see cref="object" /> and allow
        /// the @model directive to switch the model type.
        /// </remarks>
        [Test]
        public void RazorEngineHost_SupportsModelSpan_UsingCSharpCodeParser()
        {
            using (var service = RazorEngineService.Create())
            using (var writer = new StringWriter())
            {
                const string template = "@model List<RazorEngine.Tests.TestTypes.Person>\n@Model.Count";
                const string expected = "1";

                var model = new List<Person> { new Person() { Forename = "Matt", Age = 27 } };
                var key = service.GetKey(nameof(template));

                service.AddTemplate(key, template);
                service.RunCompile(key, writer, modelType: model.GetType(), model: model);

                string result = writer.ToString();

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

#if !RAZOR4
        /// <summary>
        /// Tests that the <see cref="RazorEngineHost"/> supports the @ModelType directive.
        /// </summary>
        /// <remarks>
        /// As with it's MVC counterpart, we've added support for the @ModelType declaration. This is to enable scenarios
        /// where the model type might be unknown, but we can pass in an instance of <see cref="object" /> and allow
        /// the @model directive to switch the model type.
        /// </remarks>
        [Category("VBNET")]
        [Test]
        public void RazorEngineHost_SupportsModelSpan_UsingVBCodeParser()
        {
            var config = new TemplateServiceConfiguration
                             {
                                 Language = Language.VisualBasic
                             };

            using (var service = RazorEngineService.Create(config))
            {
                const string template = "@ModelType List(Of RazorEngine.Tests.TestTypes.Person)\n@Model.Count";
                const string expected = "1";

                var model = new List<Person> { new Person() { Forename = "Matt", Age = 27 } };
                var key = service.GetKey("template");

                service.AddTemplate(key, new LoadedTemplateSource(template));

                string result = service.RunCompile(key, modelType: typeof(object), model: model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }
#endif
        #endregion
    }
}