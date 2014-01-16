Imports System.IO

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
Imports Moq
Imports RazorEngine.Compilation






''' <summary>
''' Defines a test fixture that provides tests for the <see cref="ITemplateServiceConfiguration"/> type.
''' </summary>
<TestFixture> _
Public Class ConfigurationTestFixture
#Region "Tests"
    ''' <summary>
    ''' Tests that the fluent configuration supports adding additional namespace imports.
    ''' </summary>
    <Test> _
    Public Sub FluentTemplateServiceConfiguration_CanAddNamespaces()

        Dim config = New FluentTemplateServiceConfiguration(Sub(c)
                                                                c.IncludeNamespaces("RazorEngine.Templating")
                                                                c.WithCodeLanguage(VisualBasic)
                                                            End Sub)

        Assert.That(config.Namespaces.Contains("RazorEngine.Templating"))
    End Sub

    ''' <summary>
    ''' Tests that the fluent configuration can configure a template service with additional namespaces.
    ''' </summary>
    <Test> _
    Public Sub FluentTemplateServiceConfiguration_CanConfigureTemplateService_WithAdditionalNamespaces()
        Dim config = New FluentTemplateServiceConfiguration(Sub(c)
                                                                c.IncludeNamespaces("System.IO")
                                                                c.WithCodeLanguage(VisualBasic)
                                                            End Sub)

        Using service = New TemplateService(config)
            Const template As String = "@Directory.GetFiles(""C:\"", ""*.*"").Length"

            Dim expected As Integer = Directory.GetFiles("C:\", "*.*").Length
            Dim result As String = service.Parse(template, Nothing, Nothing, Nothing)

            Assert.That(expected = Integer.Parse(result))
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the fluent configuration can configure a template service with a specific code language.
    ''' </summary>
    ''' <remarks>
    ''' For this test, we're switching to VB, and using a @Code section:
    '''     <code>
    '''         @Code Dim name = "Matt" End Code
    '''         @name
    '''     </code>
    ''' ... which should result in:
    '''     <code>
    '''         
    '''         Matt
    '''     </code>
    ''' </remarks>
    <Test> _
    Public Sub FluentTemplateServiceConfiguration_CanConfigureTemplateService_WithSpecificCodeLanguage()
        Dim config = New FluentTemplateServiceConfiguration(Function(c) c.WithCodeLanguage(Language.VisualBasic))

        Using service = New TemplateService(config)
            Const template As String = "@Code Dim name = ""Matt"" End Code" & vbLf & "@name"
            Const expected As String = vbLf & "Matt"

            Dim result As String = service.Parse(template, Nothing, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the fluent configuration can configure a template service with a specific encoding.
    ''' </summary>
    <Test> _
    Public Sub FluentTemplateServiceConfiguration_CanConfigureTemplateService_WithSpecificEncoding()
        Dim config = New FluentTemplateServiceConfiguration(Sub(c)
                                                                c.WithEncoding(Encoding.Raw)
                                                                c.WithCodeLanguage(VisualBasic)
                                                            End Sub)

        Using service = New TemplateService(config)
            Const template As String = "<h1>Hello @Model.String</h1>"
            Const expected As String = "<h1>Hello Matt & World</h1>"

            Dim model = New With {.[String] = "Matt & World"}
            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the fluent configuration supports setting a custom activator.
    ''' </summary>
    <Test> _
    Public Sub FluentTemplateServiceConfiguration_CanSetActivator_UsingActivator()
        Dim mock = New Mock(Of IActivator)

        Dim config = New FluentTemplateServiceConfiguration(Function(c) c.ActivateUsing(mock.[Object]))

        Assert.AreSame(mock.[Object], config.Activator)
    End Sub

    ''' <summary>
    ''' Tests that the fluent configuration supports setting the code language.
    ''' </summary>
    <Test> _
    Public Sub FluentTemplateServiceConfiguration_CanSetCodeLanguage()
        Dim config = New FluentTemplateServiceConfiguration(Function(c) c.WithCodeLanguage(Language.VisualBasic))

        Assert.That(config.Language = Language.VisualBasic)
    End Sub

    ''' <summary>
    ''' Tests that the fluent configuration supports setting the compiler service factory.
    ''' </summary>
    <Test> _
    Public Sub FluentTemplateServiceConfiguration_CanSetCompilerServiceFactory()
        Dim mock = New Mock(Of ICompilerServiceFactory)

        Dim config = New FluentTemplateServiceConfiguration(Sub(c)
                                                                c.CompileUsing(mock.[Object])
                                                                c.WithCodeLanguage(VisualBasic)
                                                            End Sub)

        Assert.AreSame(mock.[Object], config.CompilerServiceFactory)
    End Sub

    ''' <summary>
    ''' Tests that the fluent configuration supports setting the encoded string factory.
    ''' </summary>
    <Test> _
    Public Sub FluentTemplateServiceConfiguration_CanSetEncodedStringFactory()
        Dim mock = New Mock(Of IEncodedStringFactory)()

        Dim config = New FluentTemplateServiceConfiguration(Sub(c)
                                                                c.EncodeUsing(mock.[Object])
                                                                c.WithCodeLanguage(VisualBasic)
                                                            End Sub)

        Assert.AreSame(mock.[Object], config.EncodedStringFactory)
    End Sub

    ''' <summary>
    ''' Tests that the fluent configuration supports setting the encoded string factory using a predefined encoding.
    ''' </summary>
    <Test> _
    Public Sub FluentTemplateServiceConfiguration_CanSetEncoding_UsingHtmlEncoding()
        Dim config = New FluentTemplateServiceConfiguration(Sub(c)
                                                                c.WithEncoding(Encoding.Html)
                                                                c.WithCodeLanguage(VisualBasic)
                                                            End Sub)

        Assert.That(TypeOf config.EncodedStringFactory Is HtmlEncodedStringFactory)
    End Sub

    ''' <summary>
    ''' Tests that the fluent configuration supports setting the encoded string factory using a predefined encoding.
    ''' </summary>
    <Test> _
    Public Sub FluentTemplateServiceConfiguration_CanSetEncoding_UsingRawEncoding()
        Dim config = New FluentTemplateServiceConfiguration(Sub(c)
                                                                c.WithEncoding(Encoding.Raw)
                                                                c.WithCodeLanguage(VisualBasic)
                                                            End Sub)

        Assert.That(TypeOf config.EncodedStringFactory Is RawStringFactory)
    End Sub

    ''' <summary>
    ''' Tests that the fluent configuration supports setting a custom activator delegate.
    ''' </summary>
    <Test> _
    Public Sub FluentTemplateServiceConfigutation_CanSetActivator_UsingDelegate()
        Dim activator As Func(Of InstanceContext, ITemplate) = Function(i) (Nothing)

        Dim config = New FluentTemplateServiceConfiguration(Function(c) c.ActivateUsing(activator))
        Dim delegateActivator = DirectCast(config.Activator, DelegateActivator)

        Assert.That(delegateActivator.Activator = activator)
    End Sub
#End Region
End Class
