﻿namespace RazorEngine.Tests.TestTypes.Issues
{
    using System;
    using System.IO;
    using Microsoft.CSharp.RuntimeBinder;

    using NUnit.Framework;

    using Configuration;
    using Templating;

    /// <summary>
    /// Provides tests for the Release 3.0
    /// </summary>
    [TestFixture]
    public class Release_3_0_TestFixture
    {
        #region Tests
        /// <summary>
        /// When using a template layout, the model needs to be passed to the layout template from the child.
        /// 
        /// Issue 6: https://github.com/Antaris/RazorEngine/issues/6
        /// </summary>
        [Test]
        public void Issue6_ModelShouldBePassedToLayout()
        {
            using (var service = RazorEngineService.Create())
            {
                const string layoutTemplate = "<h1>@Model.PageTitle</h1> @RenderSection(\"Child\")";
                const string childTemplate = "@{ Layout = \"Parent\"; }@section Child {<h2>@Model.PageDescription</h2>}";
                const string expected = "<h1>Test Page</h1> <h2>Test Page Description</h2>";

                var model = new {
                                    PageTitle = "Test Page",
                                    PageDescription = "Test Page Description"
                                };

                var key = service.GetKey("Parent");
                var childKey = service.GetKey("Child");

                service.AddTemplate(key, layoutTemplate);
                service.Compile(key);

                string result = service.RunCompile(templateSource: childTemplate, key: childKey, model: model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// A viewbag property is an easy way to share state between layout templates and the rendering template. The ViewBag property
        /// needs to persist from layouts and child templates.
        /// 
        /// Issue 7: https://github.com/Antaris/RazorEngine/issues/7
        /// </summary>
        [Test]
        public void Issue7_ViewBagShouldPersistThroughLayout()
        {
            using (var service = RazorEngineService.Create())
            using (var writer = new StringWriter())
            {
                const string layoutTemplate = "<h1>@ViewBag.Title</h1>@RenderSection(\"Child\")";
                const string childTemplate = "@{ Layout =  \"Parent\"; ViewBag.Title = \"Test\"; }@section Child {}";

                var key = service.GetKey("Parent");
                var childKey = service.GetKey(nameof(childTemplate));

                service.AddTemplate(key, layoutTemplate);
                service.AddTemplate(childKey, childTemplate);

                service.Compile(key);
                service.Compile(childKey);
                service.Run(childKey, writer);
                string result = writer.ToString();

                Assert.That(result.StartsWith("<h1>Test</h1>"));
            }
        }

        /// <summary>
        /// A viewbag property should not persist through using @Include as this will create a new <see cref="ExecuteContext" />, which
        /// initialises a new viewbag property.
        /// 
        /// Issue 7: https://github.com/Antaris/RazorEngine/issues/7
        /// </summary>
        /// <remarks>
        /// This test no longer pass as we have changed the ViewBag property to be a fault tollerant dynamic dictionary.
        /// </remarks>
        [Test]
        public void Issue7_ViewBagShouldNotPersistThroughInclude_UsingCSharp()
        {
//            using (var service = new TemplateService())
//            {
//                const string parentTemplate = "@{ ViewBag.Title = \"Test\"; }@Include(\"Child\")";
//                const string childTemplate = "@ViewBag.Title";
//
//                service.Compile(childTemplate, "Child");
//
//                // The C# runtime binder will throw a RuntimeBinderException...
//                Assert.Throws<RuntimeBinderException>(() =>
//                {
//                    string result = service.Parse(parentTemplate);
//                });
//            }
        }

        /// <summary>
        /// A viewbag property should not persist through using @Include as this will create a new <see cref="ExecuteContext" />, which
        /// initialises a new viewbag property.
        /// 
        /// Issue 7: https://github.com/Antaris/RazorEngine/issues/7
        /// </summary>
        /// <remarks>
        /// This test no longer pass as we have changed the ViewBag property to be a fault tollerant dynamic dictionary.
        /// </remarks>
        [Test]
        public void Issue7_ViewBagShouldNotPersistThroughInclude_UsingVB()
        {
//            var config = new TemplateServiceConfiguration() { Language = Language.VisualBasic };
//            using (var service = new TemplateService(config))
//            {
//                const string parentTemplate = @"
//@Code 
//    ViewBag.Title = ""Test"" 
//End Code
//@Include(""Child"")";
//                const string childTemplate = "@ViewBag.Title";
//
//                service.Compile(childTemplate, "Child");
//
//                // The VB runtime binder will through a MissingMemberException...
//                Assert.Throws<MissingMemberException>(() =>
//                {
//                    string result = service.Parse(parentTemplate);
//                });
//            }
        }

        /// <summary>
        /// The template service should have the ability to compile a template with out a model.
        /// 
        /// Issue 11: https://github.com/Antaris/RazorEngine/issues/11
        /// </summary>
        [Test]
        public void Issue11_TemplateServiceShouldCompileModellessTemplate()
        {
            using (var service = RazorEngineService.Create())
            {
                const string template = "<h1>Hello World</h1>";

                var key = service.GetKey(nameof(template));
                var result = service.RunCompile(templateSource: template, key: key);

                Assert.AreEqual(template, result);
            }
        }

        /// <summary>
        /// The last property/method result in an expression, which returns a null value
        /// should not write to the template.
        /// 
        /// Issue 16: https://github.com/Antaris/RazorEngine/issues/16
        /// </summary>
        [Test]
        public void Issue16_LastNullValueShouldReturnEmptyString()
        {
            using (var service = RazorEngineService.Create())
            {
                const string template = "<h1>Hello @Model.Person.Forename</h1>";
                const string expected = "<h1>Hello </h1>";

                var model = new { Person = new Person { Forename = null } };
                var key = service.GetKey(nameof(template));
                var result = service.RunCompile(templateSource: template, key: key, model: model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// We should support overriding the model type for templates that do not specify @model.
        /// 
        /// Issue 17: https://github.com/Antaris/RazorEngine/issues/17
        /// </summary>
        [Test]
        public void TemplateService_ShouldAllowTypeOverrideForNonGenericCompile()
        {
            using (var service = RazorEngineService.Create())
            using (var writer = new StringWriter())
            {
                const string template = "@Model.Name";
                const string expected = "Matt";

                object model = new { Name = "Matt" };

                var key = service.GetKey(nameof(template));

                service.AddTemplate(key, template);
                service.RunCompile(key, writer, model: model);

                string result = writer.ToString();

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// We should support nullable value types in expressions. I think this will work because of the
        /// change made for Issue 16.
        /// 
        /// Issue 18: https://github.com/Antaris/RazorEngine/issues/18
        /// </summary>
        [Test]
        public void TemplateService_ShouldEnableNullableValueTypes()
        {
            using (var service = RazorEngineService.Create())
            using (var writer = new StringWriter())
            {
                const string template = "<h1>Hello @Model.Number</h1>";
                const string expected = "<h1>Hello </h1>";

                var model = new { Number = (int?)null };
                var key = service.GetKey(nameof(template));

                service.AddTemplate(key, template);
                service.RunCompile(key, writer, model: model);

                string result = writer.ToString();
                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Subclassed models should be supported in layouts (and also partials).
        /// 
        /// Issue 21: https://github.com/Antaris/RazorEngine/issues/21
        /// </summary>
        [Test]
        public void Issue21_SubclassModelShouldBeSupportedInLayout()
        {
            using (var service = RazorEngineService.Create())
            using (var writer = new StringWriter())
            {
                const string Parent = "@model RazorEngine.Tests.TestTypes.Person\n<h1>@Model.Forename</h1>@RenderSection(\"Child\")";

                var key = service.GetKey(nameof(Parent));

                service.AddTemplate(key, Parent);
                service.Compile(key);

                const string child = "@{ Layout = \"Parent\"; }\n@section Child { <h2>@Model.Department</h2> }";
                const string expected = "<h1>Matt</h1> <h2>IT</h2> ";

                var model = new Employee
                {
                    Age = 27,
                    Department = "IT",
                    Forename = "Matt",
                    Surname = "Abbott"
                };

                var childKey = service.GetKey(nameof(child));

                service.AddTemplate(childKey, child);
                service.RunCompile(childKey, writer, model.GetType(), model: model);

                string result = writer.ToString();
                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }
        #endregion
    }
}
