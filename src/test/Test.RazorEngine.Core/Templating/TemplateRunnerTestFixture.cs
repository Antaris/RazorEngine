using System.IO;
using NUnit.Framework;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Tests.TestTypes;

namespace Test.RazorEngine.Templating
{
    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="TemplateRunner{TModel}"/> type.
    /// </summary>
    [TestFixture]
    public class TemplateRunnerTestFixture
    {
        /// <summary>
        /// Tests that the template runner can run the template.
        /// </summary>
        [Test]
        public void TemplateRunner_CanRunTemplateString()
        {
            const string template = "Hello @Model.Forename, welcome to RazorEngine!";

            var configuration = new TemplateServiceConfiguration { Debug = true };
            using (var service = RazorEngineService.Create(configuration))
            {
                var runner = service.CompileRunner<Person>(template);
                var output = runner.Run(new Person { Forename = "Max" });

                Assert.AreEqual("Hello Max, welcome to RazorEngine!", output);
            }
        }

        /// <summary>
        /// Tests that the template runner can run the template on a text writer.
        /// </summary>
        [Test]
        public void TemplateRunner_CanRunTemplateTextWriter()
        {
            const string template = "Hello @Model.Forename, welcome to RazorEngine!";

            var configuration = new TemplateServiceConfiguration { Debug = true };
            using (var service = RazorEngineService.Create(configuration))
            using (var writer = new StringWriter())
            {
                var runner = service.CompileRunner<Person>(template);
                runner.Run(new Person { Forename = "Max" }, writer);

                Assert.AreEqual("Hello Max, welcome to RazorEngine!", writer.ToString());
            }
        }
    }
}