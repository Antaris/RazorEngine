
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



''' <summary>
''' Defines a test fixture that Use VB.NET Code
''' </summary>
''' <remarks></remarks>
<TestFixture>
Public Class ForVBCodeTestFixture


    <Test> Public Sub TestMethod1()

        Dim config As New TemplateServiceConfiguration With {.Language = VisualBasic, .Debug = True}
        Using service = New TemplateService(config)

            Const template = "@If Model IsNot Nothing Then " & vbCrLf &
                            "@<h1>Hello @Model.Forename</h1>" &
                            "else" & vbCrLf &
                            "@<h1>Hello World</h1>" &
                            "end if"


            Const expected = "<h1>Hello Matt</h1>"

            Dim model = New With {.Forename = "Matt"}

            Dim result = service.Parse(template, model, Nothing, Nothing)

            Assert.AreEqual(expected, result, "Result does not match expected: " & result)

        End Using

    End Sub

    <Test> Public Sub TestMethod2()


        Dim config As New TemplateServiceConfiguration With {.Language = VisualBasic, .Debug = True}
        Dim service = New TemplateService(config)

        Razor.SetTemplateService(service)

        Const template = "@If Model IsNot Nothing Then " & vbCrLf &
                         "@<h1>Hello @Model.Forename</h1>" &
                         "else" & vbCrLf &
                         "@<h1>Hello World</h1>" &
                         "end if"

        Const expected = "<h1>Hello Matt</h1>"

        Dim model = New With {.Forename = "Matt"}

        Dim result = Razor.Parse(template, model, Nothing, Nothing)

        Assert.AreEqual(expected, result, "Result does not match expected: " & result)


    End Sub


End Class
