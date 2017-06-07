namespace RazorEngine.Tests
{
    using System;
    using System.IO;


    using Moq;
    using NUnit.Framework;

    using Compilation;
    using Configuration;
    using Templating;
    using TestTypes;
    using TestTypes.Activation;
    using Text;
#if NET45
    using Autofac;
    using Autofac.Features.ResolveAnything;

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
#if RAZOR4
            Assert.Ignore("We need to add roslyn to generate custom constructors!");
#endif

            var container = new ContainerBuilder();
            container.RegisterType<ReverseTextFormatter>()
                .AsSelf()
                .As<ITextFormatter>();
            container.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            var config = new TemplateServiceConfiguration
                             {
                                 Activator = new AutofacTemplateActivator(container.Build()),
                                 BaseTemplateType = typeof(CustomTemplateBase<>)
                             };

#pragma warning disable 0618 // Fine because we still want to test if
            using (var service = new TemplateService(config))
#pragma warning restore 0618 // Fine because we still want to test if
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
#endif
}