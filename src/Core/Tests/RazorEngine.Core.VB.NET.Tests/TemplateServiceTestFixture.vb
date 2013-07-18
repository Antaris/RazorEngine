Imports System.Collections.Generic
Imports System.Dynamic
Imports System.Linq
Imports System.Threading

Imports NUnit.Framework

Imports RazorEngine.Configuration
Imports RazorEngine.Templating
Imports RazorEngine.Text

<TestFixture>
Public Class TemplateServiceTestFixture

    ''' <summary>
    ''' Tests that a simple template with an anonymous model can be parsed.
    ''' </summary>
    <Test>
    Public Sub TemplateService_CanParseSimpleTemplate_WithAnonymousModel()
        Using service = New TemplateService()

            Const template = "<h1>Hello @Model.Forename</h1>"
            Const expected = "<h1>Hello Matt</h1>"

            Dim model = New With {.Forename = "Matt"}

            Dim result = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " + result)
        End Using

    End Sub
End Class
