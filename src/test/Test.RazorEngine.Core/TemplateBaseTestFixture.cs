namespace RazorEngine.Tests
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    using NUnit.Framework;

    using Configuration;
    using Templating;
    using Text;
    using TestTypes;

    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="TemplateBase"/> type.
    /// </summary>
    [TestFixture]
    [System.Obsolete("Needs to be updated/replaced with RazorEngineService.")]
    public class TemplateBaseTestFixture
    {
        #region Tests
        /// <summary>
        /// Tests that a call to <see cref="TemplateBase.Raw" /> will output the raw text when using html encoding.
        /// </summary>
        /// <remarks>
        /// <see cref="TemplateBase"/> includes a <see cref="TemplateBase.Raw"/> method which returns an instance of
        /// <see cref="RawString"/> which should bypass the string encoding when writing values.
        /// </remarks>
        [Test]
        public void TemplateBase_CanUseRawOutput_WithHtmlEncoding()
        {
            using (var service = new TemplateService())
            {
                const string template = "<h1>Hello @Raw(Model.Name)</h1>";
                const string expected = "<h1>Hello <</h1>";

                var model = new { Name = "<" };
                string result = service.Parse(template, model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a template can support a layout.
        /// </summary>
        [Test]
        public void TemplateBase_CanRenderWithLayout_WithSimpleLayout()
        {
            using (var service = new TemplateService())
            {
                const string parent = @"<div>@RenderSection(""TestSection"")</div>@RenderBody()";
                const string template = @"@{Layout = ""Parent"";}@section TestSection {<span>Hello</span>}<h1>Hello World</h1>";
                const string expected = "<div><span>Hello</span></div><h1>Hello World</h1>";

                /* GetTemplate is the simplest method for compiling and caching a template without using a 
                 * resolver to locate the layout template at a later time in exection. */
                service.GetTemplate(parent, null, "Parent");
                string result = service.Parse(template, null, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a template can support a layout. This test uses multiple layout templates in the template hierachy,
        /// and the end result should put child content in both the outer layout (grandparent), and in the inner layout (parent).
        /// </summary>
        /// <remarks>
        /// Outer layout (grandparent):
        ///     &lt;div&gt;Message from Child Template (section): @RenderSection("ChildMessage")&lt;/div&gt;
        ///     &lt;div&gt;Message from Parent Template (section): @RenderSection("ParentMessage")&lt;/div&gt;
        ///     &lt;div&gt;Content from Parent Template (body): @RenderBody()&lt;/div&gt;
        /// 
        /// Inner layout (parent):
        ///     @{
        ///         Layout = "GrandParent";
        ///     }
        ///     @section ParentMessage {
        ///         &lt;span&gt;Hello from Parent&lt;/span&gt;
        ///     }
        ///     &lt;p&gt;Child content: @RenderBody()&lt;/p&gt;
        /// 
        /// Template (child):
        ///     @{
        ///         Layout = "Parent";
        ///     }
        ///     @section ChildMessage {
        ///         &lt;span&gt;Hello from Child&lt;/span&gt;
        ///     }
        ///     &lt;p&gt;This is child content&lt;/p&gt;
        /// 
        /// Expected result:
        ///     &lt;div&gt;Message from Child Template (section):
        ///         &lt;span&gt;Hello from Child&lt;/span&gt;
        ///     &lt;/div&gt;
        ///     &lt;div&gt;Message from Parent Template (section):
        ///         &lt;span&gt;Hello from Parent&lt;/span&gt;
        ///     &lt;/div&gt;
        ///     &lt;div&gt;Content from Parent Template (body):
        ///         &lt;p&gt;Child content: &lt;p&gt;This is child content&lt;/p&gt;&lt;/p&gt;
        ///     &lt;/div&gt;
        /// </remarks>
        [Test]
        public void TemplateBase_CanRenderWithLayout_WithComplexLayout()
        {
            using (var service = new TemplateService())
            {
                const string grandparent = @"<div>Message from Child Template (section): @RenderSection(""ChildMessage"")</div><div>Message from Parent Template (section): @RenderSection(""ParentMessage"")</div><div>Content from Parent Template (body): @RenderBody()</div>";
                const string parent = @"@{Layout = ""GrandParent"";}@section ParentMessage {<span>Hello from Parent</span>}<p>Child content: @RenderBody()</p>";
                const string template = @"@{Layout = ""Parent"";}@section ChildMessage {<span>Hello from Child</span>}<p>This is child content</p>";
                const string expected = "<div>Message from Child Template (section): <span>Hello from Child</span></div><div>Message from Parent Template (section): <span>Hello from Parent</span></div><div>Content from Parent Template (body): <p>Child content: <p>This is child content</p></p></div>";

                service.GetTemplate(parent, null, "Parent");
                service.GetTemplate(grandparent, null, "GrandParent");
                string result = service.Parse(template, null, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a template service can include another template.
        /// </summary>
        [Test]
        public void TemplateBase_CanRenderWithInclude_SimpleInclude()
        {
            using (var service = new TemplateService())
            {
                const string child = "<div>Content from child</div>";
                const string template = "@Include(\"Child\")";
                const string expected = "<div>Content from child</div>";

                service.GetTemplate(child, null, "Child");
                string result = service.Parse(template, null, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }
        
        /// <summary>
        /// Tests that a template service can include another template with current templete model if it was not specified.
        /// </summary>
        [Test]
        public void TemplateBase_CanRenderWithInclude_WithCurrentModel()
        {
            using (var service = new TemplateService())
            {
                const string child = "@model RazorEngine.Tests.TestTypes.Person\n<div>Content from child for @Model.Forename</div>";
                const string template = "@model RazorEngine.Tests.TestTypes.Person\n@Include(\"Child\")";
                const string expected = "<div>Content from child for Test</div>";
                var person = new Person { Forename = "Test" };

                service.GetTemplate(child, person, "Child");
                string result = service.Parse(template, person, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        } 
        
        /// <summary>
        /// Tests that a template service can include another template with current templete model if it was not specified.
        /// </summary>
        [Test]
        public void TemplateBase_CanRenderWithInclude_WithCustomModel()
        {
            using (var service = new TemplateService())
            {
                const string child = "@model RazorEngine.Tests.TestTypes.Person\n<div>Content from child for @Model.Forename</div>";
                const string template = "@Include(\"Child\", new RazorEngine.Tests.TestTypes.Person { Forename = \"Test\" })";
                const string expected = "<div>Content from child for Test</div>";

                service.GetTemplate(child, null, "Child");
                string result = service.Parse(template, null, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a template service can pass inline templates into an included template
        /// and outputs this in the correct order
        /// </summary>
        [Test]
        public void TemplateBase_CanRenderInclude_WithInlineTemplate()
        {
            using (var service = new TemplateService())
            {
                const string child = "@model RazorEngine.Tests.TestTypes.InlineTemplateModel\n@Model.InlineTemplate(Model)";
                const string template = "@model RazorEngine.Tests.TestTypes.InlineTemplateModel\n@{ Model.InlineTemplate = @<h1>@ViewBag.Name</h1>; }@Include(\"Child\", Model)";
                const string expected = "<h1>Matt</h1>";

                dynamic bag = new DynamicViewBag();
                bag.Name = "Matt";

                service.GetTemplate(child, null, "Child");
                string result = service.Parse(template, new InlineTemplateModel(), bag, null);
                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }
        #endregion
    }
}