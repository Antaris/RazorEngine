using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Tests.TestTypes;
using RazorEngine.Tests.TestTypes.BaseTypes;

namespace RazorEngine.Tests
{
	[TestFixture]
	public class TemplateServiceWithPreRunTestFixture
	{
		/// <summary>
		/// Tests that a template service can precompile a template for later execution.
		/// </summary>
		[Test]
		public void TemplateService_CanPrecompileTemplate_WithNoModel()
		{
			using (var service = new TemplateService())
			{
				const string template = "Hello World";
				const string expected = "Hello World";

				service.Compile(template, null, "test");

				string result = service.Run("test", null, null, t =>
				                                                {
					                                                dynamic instance = t;
					                                                instance.Model = "Test";
				                                                });

				Assert.That(result == expected, "Result does not match expected.");
			}
		}

		/// <summary>
		/// Tests that a template service can precompile a template with a non generic base for later execution.
		/// </summary>
		[Test]
		public void TemplateService_CanPrecompileTemplate_WithNoModelAndANonGenericBase()
		{
			var config = new TemplateServiceConfiguration { BaseTemplateType = typeof(NonGenericTemplateBase) };
			using (var service = new TemplateService(config))
			{
				const string template = "<h1>@GetHelloWorldText()</h1>";
				const string expected = "<h1>TestMatched</h1>";

				service.Compile(template, null, "test");

				string result = service.Run("test", null, null, t =>
																												{
																													var instance = t as NonGenericTemplateBase;
																													instance.HelloWorldMessage = "TestMatched";
																												});
				Assert.That(result == expected, "Result does not match expected.");
			}
		}

		/// <summary>
		/// Tests that a template service can precompile a template for later execution.
		/// </summary>
		[Test]
		public void TemplateService_CanPrecompileTemplate_WithSimpleModel()
		{
			using (var service = new TemplateService())
			{
				const string template = "Hello @Model.Forename";
				const string expected = "Hello TestName";

				var model = new Person { Forename = "Matt" };

				service.Compile(template, typeof(Person), "test");

				string result = service.Run("test", model, null,t =>
																												{
																													dynamic instance = t;
																													instance.Model.Forename = "TestName";
																												});

				Assert.That(result == expected, "Result does not match expected.");
			}
		}
	}
}
