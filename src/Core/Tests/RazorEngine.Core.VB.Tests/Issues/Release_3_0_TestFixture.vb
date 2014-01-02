Imports System.Collections.Generic
Imports NUnit.Framework
Imports RazorEngine
Imports RazorEngine.Configuration
Imports RazorEngine.Templating
Imports RazorEngine.Text



Namespace TestTypes.Issues



    ''' <summary>
    ''' Provides tests for the Release 3.0
    ''' </summary>
    <TestFixture> _
    Public Class Release_3_0_TestFixture
#Region "Tests"
        ''' <summary>
        ''' When using a template layout, the model needs to be passed to the layout template from the child.
        ''' 
        ''' Issue 6: https://github.com/Antaris/RazorEngine/issues/6
        ''' </summary>
        <Test> _
        Public Sub Issue6_ModelShouldBePassedToLayout()


            Using service = RazorHelper.GetTemplateServiceForVisualBasic

                Const layoutTemplate As String = "<h1>@Model.PageTitle</h1> @RenderSection(""Child"")"
                Const childTemplate As String = "@Code " & vbCr &
                                                " Layout = ""Parent"" " & vbCr &
                                                "End Code" & vbCr &
                                                "@section Child" &
                                                "<h2>@Model.PageDescription</h2>" &
                                                "End section"

                Const expected As String = "<h1>Test Page</h1> <h2>Test Page Description</h2>"

                Dim model = New With {.PageTitle = "Test Page",
                                       .PageDescription = "Test Page Description"}

                Dim type = model.[GetType]()

                service.Compile(layoutTemplate, type, "Parent")

                Dim result As String = service.Parse(childTemplate, model, Nothing, Nothing)

                Assert.AreEqual(expected, result, "Result does not match expected: " & result)
            End Using
        End Sub

        ''' <summary>
        ''' A viewbag property is an easy way to share state between layout templates and the rendering template. The ViewBag property
        ''' needs to persist from layouts and child templates.
        ''' 
        ''' Issue 7: https://github.com/Antaris/RazorEngine/issues/7
        ''' </summary>
        <Test> _
        Public Sub Issue7_ViewBagShouldPersistThroughLayout()

            Using service = RazorHelper.GetTemplateServiceForVisualBasic
                Const layoutTemplate As String = "<h1>@ViewBag.Title</h1>@RenderSection(""Child"")"
                Const childTemplate As String = "@Code" & vbCr &
                                                " Layout =  ""Parent"" " & vbCr &
                                                " ViewBag.Title = ""Test"" " & vbCr &
                                                "End Code" & vbCr &
                                                "@section Child " & vbCr &
                                                "End Section"

                service.Compile(layoutTemplate, Nothing, "Parent")

                Dim result As String = service.Parse(childTemplate, Nothing, Nothing, Nothing)

                Assert.That(result.StartsWith("<h1>Test</h1>"))
            End Using
        End Sub




        ''' <summary>
        ''' The template service should have the ability to compile a template with out a model.
        ''' 
        ''' Issue 11: https://github.com/Antaris/RazorEngine/issues/11
        ''' </summary>
        <Test> _
        Public Sub Issue11_TemplateServiceShouldCompileModellessTemplate()
            Using service = RazorHelper.GetTemplateServiceForVisualBasic
                Const template As String = "<h1>Hello World</h1>"

                service.Compile(template, Nothing, "issue11")
            End Using
        End Sub



        ''' <summary>
        ''' We should support overriding the model type for templates that do not specify @model.
        ''' 
        ''' Issue 17: https://github.com/Antaris/RazorEngine/issues/17
        ''' </summary>
        <Test> _
        Public Sub TemplateService_ShouldAllowTypeOverrideForNonGenericCompile()
            Using service = RazorHelper.GetTemplateServiceForVisualBasic
                Const template As String = "@Model.Name"
                Const expected As String = "Matt"

                Dim model = New With {.Name = "Matt"}

                Dim result As String = service.Parse(template, model, Nothing, Nothing)

                Assert.That(result = expected, "Result does not match expected: " & result)
            End Using
        End Sub

        ''' <summary>
        ''' We should support nullable value types in expressions. I think this will work because of the
        ''' change made for Issue 16.
        ''' 
        ''' Issue 18: https://github.com/Antaris/RazorEngine/issues/18
        ''' </summary>
        <Test> _
        Public Sub TemplateService_ShouldEnableNullableValueTypes()
            Using service = RazorHelper.GetTemplateServiceForVisualBasic
                Const template As String = "<h1>Hello @Model.Number</h1>"
                Const expected As String = "<h1>Hello </h1>"

                Dim model = New With {.Number = CType(Nothing, System.Nullable(Of Integer))}

                Dim result As String = service.Parse(template, model, Nothing, Nothing)

                Assert.That(result = expected, "Result does not match expected: " & result)
            End Using
        End Sub

        ''' <summary>
        ''' Subclassed models should be supported in layouts (and also partials).
        ''' 
        ''' Issue 21: https://github.com/Antaris/RazorEngine/issues/21
        ''' </summary>
        <Test> _
        Public Sub Issue21_SubclassModelShouldBeSupportedInLayout()

            Using service = RazorHelper.GetTemplateServiceForVisualBasic
                Const parent As String = "@ModelType   RazorEngine.Tests.TestTypes.Person" & vbCr &
                                          "<h1>@Model.Forename</h1> @RenderSection(""Child"")"

                service.Compile(parent, Nothing, "Parent")

                Const child As String = "@Code" & vbCr &
                                        " Layout = ""Parent"" " & vbCr &
                                        "End Code" & vbCr &
                                        "@Section Child" &
                                        "<h2>@Model.Department</h2>" &
                                        "End Section"

                Const expected As String = "<h1>Matt</h1> <h2>IT</h2>"

                Dim model = New Employee() With {
                        .Age = 27,
                        .Department = "IT",
                        .Forename = "Matt",
                        .Surname = "Abbott"
                   }

                Dim result As String = service.Parse(child, model, Nothing, Nothing)

                Assert.AreEqual(expected, result, "Result does not match expected: " & result)

            End Using
        End Sub

        ''' <summary>
        ''' ViewBag initialization not possible outside of template.
        ''' 
        ''' Issue 26: https://github.com/Antaris/RazorEngine/issues/26
        ''' </summary>
        <Test> _
        Public Sub Issue26_ViewBagInitializationOutsideOfTemplate()
            Using service = RazorHelper.GetTemplateServiceForVisualBasic
                Const template As String = "@ViewBag.TestValue"
                Const expected As String = "This is a test"

                Dim viewBag As New DynamicViewBag()
                viewBag.AddValue("TestValue", "This is a test")

                Dim result As String = service.Parse(template, Nothing, viewBag, Nothing)

                Assert.That(result = expected, "Result does not match expected: " & result)
            End Using
        End Sub

        ''' <summary>
        ''' StreamLining the ITemplateServiceAPI.
        ''' 
        ''' Issue 27: https://github.com/Antaris/RazorEngine/issues/27
        ''' </summary>
        ''' <remarks>
        ''' Streamlining the interface did not change funcionality - it just consolidated
        ''' overloads into a single methods to simplify Interface implementation.
        ''' <br/><br/>
        ''' There is one exception - the CreateTemplates() method.
        ''' This was enhanced to:<br/>
        '''     1) Allow a NULL razorTemplates parameter if templateTypes are specified.<br/>
        '''     2) Allow a NULL templateTypes parameter if razorTemplates are specified.<br/>
        '''     3) Allow both razorTemplates / templateTypes to be specified and have some templates and some templates dynamically compiled.
        ''' <br/><br/>
        ''' This test case tests for success and exception conditions in features 1-3.
        ''' </remarks>
        <Test> _
        Public Sub Issue27_StreamLiningTheITemplateServiceApi_CreateTemplates()
            Dim razorTemplates As String()
            Dim templateTypes As Type()
            Dim index As Integer

            Using service = RazorHelper.GetTemplateServiceForVisualBasic
                ' Success case
                razorTemplates = New String() {"Template1", "Template2", "Template3"}
                templateTypes = New Type() {Nothing, Nothing, Nothing}
                Dim instances As IEnumerable(Of ITemplate) = service.CreateTemplates(razorTemplates, templateTypes, Nothing, False)

                index = 0
                For Each instance As ITemplate In instances
                    Dim expected As String = razorTemplates(index)
                    Dim result As String = service.Run(instance, Nothing)
                    Assert.That(result = expected, "Result does not match expected: " & result)
                    index += 1
                Next

                ' No razorTemplates or templateTypes provided
                Assert.Throws(Of ArgumentException)(Function() service.CreateTemplates(Nothing, Nothing, Nothing, False))

                ' Unbalanced razorTemplates/templateTypes (templateTypes to small)
                Assert.Throws(Of ArgumentException)(Sub()
                                                        razorTemplates = New String() {"Template1", "Template2", "Template3"}
                                                        templateTypes = New Type() {Nothing, Nothing}
                                                        service.CreateTemplates(razorTemplates, templateTypes, Nothing, False)

                                                    End Sub)

                ' Unbalanced razorTemplates/templateTypes (templateTypes too large)
                Assert.Throws(Of ArgumentException)(Sub()
                                                        razorTemplates = New String() {"Template1", "Template2", "Template3"}
                                                        templateTypes = New Type() {Nothing, Nothing, Nothing, Nothing}
                                                        service.CreateTemplates(razorTemplates, templateTypes, Nothing, False)

                                                    End Sub)

                ' Unbalanced razorTemplates/templateTypes (razorTemplates and templateTypes are NULL)
                Assert.Throws(Of ArgumentException)(Sub()
                                                        razorTemplates = New String() {"Template1", "Template2", Nothing}
                                                        templateTypes = New Type() {Nothing, Nothing, Nothing}
                                                        service.CreateTemplates(razorTemplates, templateTypes, Nothing, False)

                                                    End Sub)
            End Using
        End Sub
#End Region
    End Class

End Namespace
