namespace RazorEngine.Tests
{
    using System;

    using NUnit.Framework;
    
#if !RAZOR4
    using Compilation.Inspectors;
    using Configuration;
    using Templating;
    using TestTypes.Inspectors;

    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="ICodeInspector"/> type.
    /// </summary>
    [TestFixture]
    [Obsolete("Removed eventually.")]
    public class CodeInspectorTestFixture
    {
        #region Tests
        /// <summary>
        /// Tests that a code inspector supports add a custom inspector.
        /// </summary>
        [Test]
        public void CodeInspector_SupportsAddingCustomInspector()
        {
            var config = new TemplateServiceConfiguration();
            config.CodeInspectors.Add(new ThrowExceptionCodeInspector());

            using (var service = RazorEngineService.Create(config))
            {
                const string template = "Hello World";

                Assert.Throws<InvalidOperationException>(() => service.RunCompile(templateSource: template, name: "template"));
            }
        }
        #endregion
    }
#endif
}