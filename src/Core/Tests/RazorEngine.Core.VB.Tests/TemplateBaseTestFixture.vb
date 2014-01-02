
Imports System.Collections.Generic
Imports System.Dynamic
Imports System.Linq
Imports System.Threading
Imports NUnit.Framework
Imports RazorEngine
Imports RazorEngine.Configuration
Imports RazorEngine.Templating
Imports RazorEngine.Text
Imports RazorEngine.Language
Imports RazorEngine.Tests.TestTypes
Imports RazorEngine.Tests.TestTypes.BaseTypes




''' <summary>
''' Defines a test fixture that provides tests for the <see cref="TemplateBase"/> type.
''' </summary>
<TestFixture> _
Public Class TemplateBaseTestFixture
#Region "Tests"
    ''' <summary>
    ''' Tests that a call to <see cref="TemplateBase.Raw" /> will output the raw text when using html encoding.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="TemplateBase"/> includes a <see cref="TemplateBase.Raw"/> method which returns an instance of
    ''' <see cref="RawString"/> which should bypass the string encoding when writing values.
    ''' </remarks>
    <Test> _
    Public Sub TemplateBase_CanUseRawOutput_WithHtmlEncoding()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello @Raw(Model.Name)</h1>"
            Const expected As String = "<h1>Hello <</h1>"

            Dim model = New With {.Name = "<"}

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a template can support a layout.
    ''' </summary>
    <Test> _
    Public Sub TemplateBase_CanRenderWithLayout_WithSimpleLayout()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const parent As String = "<div>@RenderSection(""TestSection"")</div>@RenderBody()"

            Const template As String = "@Code" & vbCr &
                                        " Layout = ""Parent"" " & vbCr &
                                        "End Code" &
                                        "@Section TestSection" &
                                         "<span>Hello</span>" &
                                         "End Section" &
                                         "<h1>Hello World</h1>"

            Const expected As String = "<div><span>Hello</span></div><h1>Hello World</h1>"

            ' GetTemplate is the simplest method for compiling and caching a template without using a 
            '                 * resolver to locate the layout template at a later time in exection. 

            service.GetTemplate(parent, Nothing, "Parent")
            Dim result As String = service.Parse(template, Nothing, Nothing, Nothing)

            Assert.AreEqual(expected, result, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a template can support a layout. This test uses multiple layout templates in the template hierachy,
    ''' and the end result should put child content in both the outer layout (grandparent), and in the inner layout (parent).
    ''' </summary>
    ''' <remarks>
    ''' Outer layout (grandparent):
    '''     &lt;div&gt;Message from Child Template (section): @RenderSection("ChildMessage")&lt;/div&gt;
    '''     &lt;div&gt;Message from Parent Template (section): @RenderSection("ParentMessage")&lt;/div&gt;
    '''     &lt;div&gt;Content from Parent Template (body): @RenderBody()&lt;/div&gt;
    ''' 
    ''' Inner layout (parent):
    '''     @{
    '''         Layout = "GrandParent";
    '''     }
    '''     @section ParentMessage {
    '''         &lt;span&gt;Hello from Parent&lt;/span&gt;
    '''     }
    '''     &lt;p&gt;Child content: @RenderBody()&lt;/p&gt;
    ''' 
    ''' Template (child):
    '''     @{
    '''         Layout = "Parent";
    '''     }
    '''     @section ChildMessage {
    '''         &lt;span&gt;Hello from Child&lt;/span&gt;
    '''     }
    '''     &lt;p&gt;This is child content&lt;/p&gt;
    ''' 
    ''' Expected result:
    '''     &lt;div&gt;Message from Child Template (section):
    '''         &lt;span&gt;Hello from Child&lt;/span&gt;
    '''     &lt;/div&gt;
    '''     &lt;div&gt;Message from Parent Template (section):
    '''         &lt;span&gt;Hello from Parent&lt;/span&gt;
    '''     &lt;/div&gt;
    '''     &lt;div&gt;Content from Parent Template (body):
    '''         &lt;p&gt;Child content: &lt;p&gt;This is child content&lt;/p&gt;&lt;/p&gt;
    '''     &lt;/div&gt;
    ''' </remarks>
    <Test> _
    Public Sub TemplateBase_CanRenderWithLayout_WithComplexLayout()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const grandparent As String = "<div>Message from Child Template (section): @RenderSection(""ChildMessage"")</div><div>Message from Parent Template (section): @RenderSection(""ParentMessage"")</div><div>Content from Parent Template (body): @RenderBody()</div>"

            Const parent As String = "@Code" & vbCr &
                                    "Layout = ""GrandParent"" " & vbCr &
                                    "End Code" &
                                    "@section ParentMessage" &
                                    "<span>Hello from Parent</span>" &
                                    "End Section" &
                                    "<p>Child content: @RenderBody()</p>"
            Const template As String = "@Code" & vbCr &
                                        "Layout = ""Parent"" " & vbCr &
                                        "End Code" &
                                        "@section ChildMessage" &
                                        "<span>Hello from Child</span>" &
                                        "End section" &
                                        "<p>This is child content</p>"

            Const expected As String = "<div>Message from Child Template (section): <span>Hello from Child</span></div><div>Message from Parent Template (section): <span>Hello from Parent</span></div><div>Content from Parent Template (body): <p>Child content: <p>This is child content</p></p></div>"

            service.GetTemplate(parent, Nothing, "Parent")
            service.GetTemplate(grandparent, Nothing, "GrandParent")
            Dim result As String = service.Parse(template, Nothing, Nothing, Nothing)

            Assert.AreEqual(expected, result, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a template service can include another template.
    ''' </summary>
    <Test> _
    Public Sub TemplateBase_CanRenderWithInclude_SimpleInclude()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const child As String = "<div>Content from child</div>"
            Const template As String = "@Include(""Child"")"
            Const expected As String = "<div>Content from child</div>"

            service.GetTemplate(child, Nothing, "Child")
            Dim result As String = service.Parse(template, Nothing, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a template service can include another template with current templete model if it was not specified.
    ''' </summary>
    <Test> _
    Public Sub TemplateBase_CanRenderWithInclude_WithCurrentModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const child As String = "@modeltype RazorEngine.Tests.TestTypes.Person" & vbLf & "<div>Content from child for @Model.Forename</div>"
            Const template As String = "@modeltype RazorEngine.Tests.TestTypes.Person" & vbLf & "@Include(""Child"")"
            Const expected As String = "<div>Content from child for Test</div>"
            Dim person = New Person() With {.Forename = "Test"}

            service.GetTemplate(child, person, "Child")
            Dim result As String = service.Parse(template, person, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a template service can include another template with current templete model if it was not specified.
    ''' </summary>
    <Test> _
    Public Sub TemplateBase_CanRenderWithInclude_WithCustomModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const child As String = "@ModelType RazorEngine.Tests.TestTypes.Person" & vbLf & "<div>Content from child for @Model.Forename</div>"
            Const template As String = "@Include(""Child"", new RazorEngine.Tests.TestTypes.Person with { .Forename = ""Test"" })"
            Const expected As String = "<div>Content from child for Test</div>"

            service.GetTemplate(child, Nothing, "Child")
            Dim result As String = service.Parse(template, Nothing, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

#End Region
End Class
