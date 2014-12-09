namespace RazorEngine.Tests
{
    using System;
    using System.IO;

    using Microsoft.Practices.Unity;
    using Moq;
    using NUnit.Framework;

    using Compilation;
    using Configuration;
    using Templating;
    using TestTypes;
    using TestTypes.Activation;
    using Text;

    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="IActivator"/> type.
    /// </summary>
    [TestFixture]
    public class ActivatorTestFixture
    {
        #region Tests
        /// <summary>
        /// Tests that a custom activator can be used. In this test case, we're using Unity
        /// to handle a instantiation of a custom activator.
        /// </summary>
        [Test]
        public void TemplateService_CanSupportCustomActivator_WithUnity()
        {
            var container = new UnityContainer();
            container.RegisterType(typeof(ITextFormatter), typeof(ReverseTextFormatter));

            var config = new TemplateServiceConfiguration
                             {
                                 Activator = new UnityTemplateActivator(container),
                                 BaseTemplateType = typeof(CustomTemplateBase<>)
                             };

            using (var service = new TemplateService(config))
            {
                const string template = "<h1>Hello @Format(Model.Forename)</h1>";
                const string expected = "<h1>Hello ttaM</h1>";

                var model = new Person { Forename = "Matt" };
                string result = service.Parse(template, model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }
        #endregion
    }
}