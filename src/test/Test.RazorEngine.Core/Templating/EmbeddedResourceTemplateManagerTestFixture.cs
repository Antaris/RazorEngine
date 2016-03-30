using System;
using NUnit.Framework;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Test.RazorEngine.Templating.Templates;

namespace Test.RazorEngine.Templating
{
    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="EmbeddedResourceTemplateManager"/> type.
    /// </summary>
    [TestFixture]
    public class EmbeddedResourceTemplateManagerTestFixture
    {
        #region Tests

        /// <summary>
        /// Tests rendering of the template that uses no model
        /// </summary>
        [Test]
        public void RendersTemplateWithoutModel()
        {
            // Arrange
            var rootType = typeof(EmbeddedResourceTemplateManagerTestFixture);
            var template = "Templates.NoModel";

            // Act
            var result = Render(rootType, template, null);

            // Assert
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Contains("Template without Model"));
        }

        /// <summary>
        /// Tests rendering of the template that uses a model
        /// </summary>
        [Test]
        public void RendersTemplateWithModel()
        {
            // Arrange
            var rootType = typeof(EmbeddedResourceTemplateManagerTestFixture);
            var template = "Templates.WithModel";

            // Act
            var result = Render(rootType, template, new Model { Name = "Arthur", Answer = 42 });

            // Assert
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Contains("Hello Arthur! Your answer is 42."));
        }

        /// <summary>
        /// Tests rendering of the template that uses a layout
        /// </summary>
        [Test]
        public void RendersTemplateWithLayout()
        {
            // Arrange
            var rootType = typeof(EmbeddedResourceTemplateManagerTestFixture);
            var template = "Templates.WithLayout";

            // Act
            var result = Render(rootType, template, new Model { Name = "Arthur", Answer = 42 });

            // Assert
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Contains("This is a Layout"));
            Assert.IsTrue(result.Contains("Hello Arthur! Your answer is 42."));
        }

        /// <summary>
        /// Tests rendering of the template that uses a partial template.
        /// </summary>
        [Test]
        public void RendersTemplateWithPartial()
        {
            // Arrange
            var rootType = typeof(EmbeddedResourceTemplateManagerTestFixture);
            var template = "Templates.WithPartial";

            // Act
            var result = Render(rootType, template, new Model { Name = "Arthur", Answer = 42 });

            // Assert
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Contains("Hello Arthur!"));
            Assert.IsTrue(result.Contains("Your answer is 42."));
        }
        #endregion

        private static string Render(Type rootType, string templateName, object model)
        {
            var config = new TemplateServiceConfiguration
            {
                TemplateManager = new EmbeddedResourceTemplateManager(rootType)
            };

            IRazorEngineService service = RazorEngineService.Create(config);

            return service.RunCompile(templateName, model: model);
        }
    }
}
