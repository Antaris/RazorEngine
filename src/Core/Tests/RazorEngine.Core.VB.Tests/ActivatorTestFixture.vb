
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
Imports RazorEngine.Tests.TestTypes.Activation
Imports Microsoft.Practices.Unity


''' <summary>
''' Defines a test fixture that provides tests for the <see cref="IActivator"/> type.
''' </summary>
<TestFixture>
Public Class ActivatorTestFixture
#Region "Tests"
    ''' <summary>
    ''' Tests that a custom activator can be used. In this test case, we're using Unity
    ''' to handle a instantiation of a custom activator.
    ''' </summary>
    <Test>
    Public Sub TemplateService_CanSupportCustomActivator_WithUnity()
        Dim container = New UnityContainer()
        container.RegisterType(GetType(ITextFormatter), GetType(ReverseTextFormatter))

        Dim config = New TemplateServiceConfiguration() With {
            .Activator = New UnityTemplateActivator(container),
            .BaseTemplateType = GetType(CustomTemplateBase(Of )),
            .Language = VisualBasic}

        Using service = New TemplateService(config)
            Const template As String = "<h1>Hello @Format(Model.Forename)</h1>"
            Const expected As String = "<h1>Hello ttaM</h1>"

            Dim model = New Person() With {.Forename = "Matt"}

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub
#End Region
End Class

