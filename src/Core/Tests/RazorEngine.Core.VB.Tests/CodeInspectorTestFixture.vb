
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
Imports RazorEngine.Tests.TestTypes.Inspectors



''' <summary>
''' Defines a test fixture that provides tests for the <see cref="ICodeInspector"/> type.
''' </summary>
<TestFixture> _
Public Class CodeInspectorTestFixture
#Region "Tests"
    ''' <summary>
    ''' Tests that a code inspector supports add a custom inspector.
    ''' </summary>
    <Test> _
    Public Sub CodeInspector_SupportsAddingCustomInspector()
        Dim config = RazorHelper.GetTemplateServiceConfigurationForVisualBasic
        config.CodeInspectors.Add(New ThrowExceptionCodeInspector())

        Using service = New TemplateService(config)
            Const template As String = "Hello World"

            Assert.Throws(Of InvalidOperationException)(Function() service.Parse(template, Nothing, Nothing, Nothing))
        End Using
    End Sub
#End Region
End Class

