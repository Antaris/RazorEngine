

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
Public Class RazorEngineHostTestFixture
#Region "Tests"
    ''' <summary>
    ''' Tests that the <see cref="RazorEngineHost"/> supports the @model directive.
    ''' </summary>
    ''' <remarks>
    ''' As with it's MVC counterpart, we've added support for the @model declaration. This is to enable scenarios
    ''' where the model type might be unknown, but we can pass in an instance of <see cref="object" /> and allow
    ''' the @model directive to switch the model type.
    ''' </remarks>
    <Test> _
    Public Sub RazorEngineHost_SupportsModelSpan_UsingCSharpCodeParser()
        Using service = New TemplateService()
            Const template As String = "@model List<RazorEngine.Tests.TestTypes.Person>" & vbLf & "@Model.Count"
            Const expected As String = "1"

            Dim model = New List(Of Person)() From {New Person() With {.Forename = "Matt", .Age = 27}
                                                   }
            Dim result As String = service.Parse(template, DirectCast(model, Object), Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    <Test> _
    Public Sub RazorEngineHost_SupportsModelSpan_WithBaseType_NotGeneric_UsingCSharpCodeParser()
        Dim config = New TemplateServiceConfiguration()
        config.BaseTemplateType = GetType(TemplateBase)
        Using service = New TemplateService(config)
            Const template As String = "@model RazorEngine.Tests.TestTypes.Person" & vbLf & "@Model.Forename"
            Const expected As String = "Matt"

            Dim model = New Person() With {.Forename = "Matt"}
            Dim result As String = service.Parse(template, DirectCast(model, Object), Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the <see cref="RazorEngineHost"/> supports the @ModelType directive.
    ''' </summary>
    ''' <remarks>
    ''' As with it's MVC counterpart, we've added support for the @ModelType declaration. This is to enable scenarios
    ''' where the model type might be unknown, but we can pass in an instance of <see cref="object" /> and allow
    ''' the @model directive to switch the model type.
    ''' </remarks>
    <Test> _
    Public Sub RazorEngineHost_SupportsModelSpan_UsingVBCodeParser()
        Dim config = New TemplateServiceConfiguration() With {.Language = Language.VisualBasic}

        Using service = New TemplateService(config)
            Const template As String = "@ModelType List(Of RazorEngine.Tests.TestTypes.Person)" & vbLf & "@Model.Count"
            Const expected As String = "1"

            Dim model = New List(Of Person)() From {
                New Person() With {
                    .Forename = "Matt",
                    .Age = 27
                }
            }

            Dim result As String = service.Parse(template, DirectCast(model, Object), Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    <Test> _
    Public Sub RazorEngineHost_SupportsModelSpan_WithBaseType_NotGeneric_UsingVBCodeParser()
        Dim config = New TemplateServiceConfiguration()
        config.BaseTemplateType = GetType(TemplateBase)
        config.Language = Language.VisualBasic

        Using service = New TemplateService(config)
            Const template As String = "@ModelType List(Of RazorEngine.Tests.TestTypes.Person)" & vbLf & "@Model.Count"
            Const expected As String = "1"

            Dim model = New List(Of Person)() From {
                New Person() With {
                    .Forename = "Matt",
                    .Age = 27
                }
            }

            Dim result As String = service.Parse(template, DirectCast(model, Object), Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub
#End Region
End Class

