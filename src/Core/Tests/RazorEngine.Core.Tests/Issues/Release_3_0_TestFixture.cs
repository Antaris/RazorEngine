namespace RazorEngine.Tests.TestTypes.Issues
{
    using System;
    using System.Collections.Generic;
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
            using (var service = new TemplateService())
            {
                const string layoutTemplate = "<h1>@Model.PageTitle</h1> @RenderSection(\"Child\")";
                const string childTemplate = "@{ Layout = \"Parent\"; }@section Child {<h2>@Model.PageDescription</h2>}";
                const string expected = "<h1>Test Page</h1> <h2>Test Page Description</h2>";

                var model = new {
                                    PageTitle = "Test Page",
                                    PageDescription = "Test Page Description"
                                };

                var type = model.GetType();

                service.Compile(layoutTemplate, type, "Parent");

                string result = service.Parse(childTemplate, model, null, null);

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
            using (var service = new TemplateService())
            {
                const string layoutTemplate = "<h1>@ViewBag.Title</h1>@RenderSection(\"Child\")";
                const string childTemplate = "@{ Layout =  \"Parent\"; ViewBag.Title = \"Test\"; }@section Child {}";

                service.Compile(layoutTemplate, null, "Parent");

                string result = service.Parse(childTemplate, null, null, null);

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
            using (var service = new TemplateService())
            {
                const string template = "<h1>Hello World</h1>";

                service.Compile(template, null, "issue11");
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
            using (var service = new TemplateService())
            {
                const string template = "<h1>Hello @Model.Person.Forename</h1>";
                const string expected = "<h1>Hello </h1>";

                var model = new { Person = new Person { Forename = null } };
                string result = service.Parse(template, model, null, null);

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
            using (var service = new TemplateService())
            {
                const string template = "@Model.Name";
                const string expected = "Matt";

                object model = new { Name = "Matt" };

                string result = service.Parse(template, model, null, null);

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
            using (var service = new TemplateService())
            {
                const string template = "<h1>Hello @Model.Number</h1>";
                const string expected = "<h1>Hello </h1>";

                var model = new { Number = (int?)null };
                string result = service.Parse(template, model, null, null);

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
            using (var service = new TemplateService())
            {
                const string parent = "@model RazorEngine.Tests.TestTypes.Person\n<h1>@Model.Forename</h1>@RenderSection(\"Child\")";
                service.Compile(parent, null, "Parent");

                const string child = "@{ Layout = \"Parent\"; }\n@section Child { <h2>@Model.Department</h2> }";
                const string expected = "<h1>Matt</h1> <h2>IT</h2> ";

                var model = new Employee
                {
                    Age = 27,
                    Department = "IT",
                    Forename = "Matt",
                    Surname = "Abbott"
                };

                string result = service.Parse(child, model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Requested functionality to allow deep nulls on dynamic models to fail silently without throwing exceptions.
        /// 
        /// Issue 22: https://github.com/Antaris/RazorEngine/issues/22
        /// </summary>
        [Test]
        public void Issue22_MissingDynamicPropertiesCausesException()
        {
            using (var service = new TemplateService(new TemplateServiceConfiguration()
                                                     {
                                                         AllowMissingPropertiesOnDynamic = true
                                                     }))
            {
                const string template = "Missing property: @Model.Something.SomethingElse";
                const string expected = "Missing property: ";

                var model = new { Name = "Matt" };

                string result = service.Parse(template, model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// ViewBag initialization not possible outside of template.
        /// 
        /// Issue 26: https://github.com/Antaris/RazorEngine/issues/26
        /// </summary>
        [Test]
        public void Issue26_ViewBagInitializationOutsideOfTemplate()
        {
            using (var service = new TemplateService())
            {
                const string template = "@ViewBag.TestValue";
                const string expected = "This is a test";

                DynamicViewBag viewBag = new DynamicViewBag();
                viewBag.AddValue("TestValue", "This is a test");

                string result = service.Parse(template, null, viewBag, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// StreamLining the ITemplateServiceAPI.
        /// 
        /// Issue 27: https://github.com/Antaris/RazorEngine/issues/27
        /// </summary>
        /// <remarks>
        /// Streamlining the interface did not change funcionality - it just consolidated
        /// overloads into a single methods to simplify Interface implementation.
        /// <br/><br/>
        /// There is one exception - the CreateTemplates() method.
        /// This was enhanced to:<br/>
        ///     1) Allow a NULL razorTemplates parameter if templateTypes are specified.<br/>
        ///     2) Allow a NULL templateTypes parameter if razorTemplates are specified.<br/>
        ///     3) Allow both razorTemplates / templateTypes to be specified and have some templates and some templates dynamically compiled.
        /// <br/><br/>
        /// This test case tests for success and exception conditions in features 1-3.
        /// </remarks>
        [Test]
        public void Issue27_StreamLiningTheITemplateServiceApi_CreateTemplates()
        {
            string[] razorTemplates;
            Type[] templateTypes;
            int index;

            using (var service = new TemplateService())
            {
                // Success case
                razorTemplates = new string[] { "Template1", "Template2", "Template3" };
                templateTypes = new Type[] { null, null, null };
                IEnumerable<ITemplate> instances = service.CreateTemplates(razorTemplates, templateTypes, null, false);

                index = 0;
                foreach (ITemplate instance in instances)
                {
                    string expected = razorTemplates[index];
                    string result = service.Run(instance, null);
                    Assert.That(result == expected, "Result does not match expected: " + result);
                    index++;
                }

                // No razorTemplates or templateTypes provided
                Assert.Throws<ArgumentException>(() =>
                {
                    service.CreateTemplates(null, null, null, false);
                });

                // Unbalanced razorTemplates/templateTypes (templateTypes to small)
                Assert.Throws<ArgumentException>(() =>
                {
                    razorTemplates = new string[] { "Template1", "Template2", "Template3" };
                    templateTypes = new Type[] { null, null };
                    service.CreateTemplates(razorTemplates, templateTypes, null, false);
                });

                // Unbalanced razorTemplates/templateTypes (templateTypes too large)
                Assert.Throws<ArgumentException>(() =>
                {
                    razorTemplates = new string[] { "Template1", "Template2", "Template3" };
                    templateTypes = new Type[] { null, null, null, null };
                    service.CreateTemplates(razorTemplates, templateTypes, null, false);
                });

                // Unbalanced razorTemplates/templateTypes (razorTemplates and templateTypes are NULL)
                Assert.Throws<ArgumentException>(() =>
                {
                    razorTemplates = new string[] { "Template1", "Template2", null };
                    templateTypes = new Type[] { null, null, null };
                    service.CreateTemplates(razorTemplates, templateTypes, null, false);
                });
            }
        }

        /// <summary>
        /// Dynamic ViewBag properties should be persisted to the viewbag used for include templates.
        /// 
        /// Issue 133: https://github.com/Antaris/RazorEngine/issues/133
        /// </summary>
        [Test]
        public void Issue133_ViewBagShouldPersistToIncludes()
        {
            using (var service = new TemplateService())
            {
                const string child = "<h1>@ViewBag.Name</h1>";
                const string template = "@Include(\"Child\")";
                const string expected = "<h1>Matt</h1>";

                dynamic bag = new DynamicViewBag();
                bag.Name = "Matt";


                service.GetTemplate(child, null, "Child");
                string result = service.Parse(template, null, bag, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }
        #endregion
    }
}
