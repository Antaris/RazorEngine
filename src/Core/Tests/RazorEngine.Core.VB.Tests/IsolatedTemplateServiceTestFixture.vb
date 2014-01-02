
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
Imports System.Runtime.Serialization




''' <summary>
''' Defines a test fixture that provides tests for the <see cref="IsolatedTemplateService"/> type.
''' </summary>
<TestFixture> _
Public Class IsolatedTemplateServiceTestFixture
#Region "Tests"
    ''' <summary>
    ''' Tests that a simple template without a model can be parsed.
    ''' </summary>
    <Test> _
    Public Sub IsolatedTemplateService_CanParseSimpleTemplate_WithNoModel()
        Using service = RazorHelper.GetIsolatedTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello World</h1>"
            Const expected As String = template

            Dim result As String = service.Parse(template, Nothing, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple template with a model can be parsed.
    ''' </summary>
    <Test> _
    Public Sub IsolatedTemplateService_CanParseSimpleTemplate_WithComplexSerializableModel()
        Using service = RazorHelper.GetIsolatedTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello @Model.Forename</h1>"
            Const expected As String = "<h1>Hello Matt</h1>"

            Dim model = New Person() With {
                .Forename = "Matt"
            }

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple template with a non-serializable model cannot be parsed.
    ''' </summary>
    <Test> _
    Public Sub IsolatedTemplateService_CannotParseSimpleTemplate_WithComplexNonSerializableModel()
        Using service = RazorHelper.GetIsolatedTemplateServiceForVisualBasic
            Const template As String = "<h1>Animal Type: @Model.Type</h1>"

            Assert.Throws(Of SerializationException)(Sub()
                                                         Dim model = New Animal() With {.Type = "Cat"}
                                                         service.Parse(template, model, Nothing, Nothing)

                                                     End Sub)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple template with an anonymous model cannot be parsed.
    ''' </summary>
    ''' <remarks>
    ''' This may seem pointless to test, as the <see cref="IsolatedTemplateService"/> will explicitly
    ''' check and throw the exception, it's worth creating a test for future reference. It's also
    ''' something we can check should we ever find a way to support dynamic/anonymous objects
    ''' across application domain boundaries.
    ''' </remarks>
    <Test> _
    Public Sub IsolatedTemplateService_CannotParseSimpleTemplate_WithAnonymousModel()
        Using service = RazorHelper.GetIsolatedTemplateServiceForVisualBasic
            Const template As String = "<h1>Animal Type: @Model.Type</h1>"

            Assert.Throws(Of ArgumentException)(Sub()
                                                    Dim model = New With {
                                                        .Type = "Cat"
                                                    }
                                                    service.Parse(template, model, Nothing, Nothing)

                                                End Sub)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple template with an expando model cannot be parsed.
    ''' </summary>
    ''' <remarks>
    ''' This may seem pointless to test, as the <see cref="IsolatedTemplateService"/> will explicitly
    ''' check and throw the exception, it's worth creating a test for future reference. It's also
    ''' something we can check should we ever find a way to support dynamic/anonymous objects
    ''' across application domain boundaries.
    ''' </remarks>
    <Test> _
    Public Sub IsolatedTemplateService_CannotParseSimpleTemplate_WithExpandoModel()
        Using service = RazorHelper.GetIsolatedTemplateServiceForVisualBasic
            Const template As String = "<h1>Animal Type: @Model.Type</h1>"

            Assert.Throws(Of ArgumentException)(Sub()
                                                    Dim model As Object = New ExpandoObject()
                                                    model.Type = "Cat"
                                                    service.Parse(template, model, Nothing, Nothing)

                                                End Sub)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple template with a dynamic model cannot be parsed.
    ''' </summary>
    ''' <remarks>
    ''' This may seem pointless to test, as the <see cref="IsolatedTemplateService"/> will explicitly
    ''' check and throw the exception, it's worth creating a test for future reference. It's also
    ''' something we can check should we ever find a way to support dynamic/anonymous objects
    ''' across application domain boundaries.
    ''' </remarks>
    <Test> _
    Public Sub IsolatedTemplateService_CannotParseSimpleTemplate_WithDynamicModel()
        Using service = RazorHelper.GetIsolatedTemplateServiceForVisualBasic
            Const template As String = "<h1>Animal Type: @Model.Type</h1>"

            Assert.Throws(Of ArgumentException)(Sub()
                                                    Dim model As Object = New ValueObject(New Dictionary(Of String, Object)() From {{"Type", "Cat"}})
                                                    service.Parse(template, model, Nothing, Nothing)

                                                End Sub)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that an isolated template service cannot use the same application domain as the 
    ''' main application domain.
    ''' </summary>
    ''' <remarks>
    ''' An isolated template service will unload it's child application domain on Dispose. We need to ensure
    ''' it doesn't attempt to unload the current application domain that it is running in. This may or may
    ''' not be the main application domain (but is very likely to be).
    ''' </remarks>
    <Test> _
    Public Sub IsolatedTemplateService_WillThrowException_WhenUsingMainAppDomain()
        Assert.Throws(Of InvalidOperationException)(Sub()
                                                        Using service = New IsolatedTemplateService(Language.VisualBasic, Function() AppDomain.CurrentDomain)
                                                        End Using

                                                    End Sub)
    End Sub

    ''' <summary>
    ''' Tests that an isolated template service cannot use a null application domain.
    ''' </summary>
    ''' <remarks>
    ''' I had considered using the default <see cref="IAppDomainFactory"/> to spawn a default
    ''' application domain to load templates into when a null value is returned, but behaviourly this didn't 
    ''' seem like the right thing to do. If you're using an <see cref="IsolatedTemplateService"/>, 
    ''' you should expect it to have a valid application domain, so passing null should cause an exception.
    ''' </remarks>
    <Test> _
    Public Sub IsolatedTemplateService_WillThrowException_WhenUsingNullAppDomain()
        Assert.Throws(Of InvalidOperationException)(Sub()
                                                        Using service = New IsolatedTemplateService(Language.VisualBasic, Function() Nothing)
                                                        End Using

                                                    End Sub)
    End Sub

    ''' <summary>
    ''' Tests that a simple template with html-encoding can be parsed.
    ''' </summary>
    ''' <remarks>
    ''' Text encoding is performed when writing objects to the template result (not literals). This test should 
    ''' show that the template service is correctly providing the appropriate encoding factory to process
    ''' the object's .ToString() and automatically encode it.
    ''' </remarks>
    <Test> _
    Public Sub IsolatedTemplateService_CanParseSimpleTemplate_UsingHtmlEncoding()

        Dim obj As IAppDomainFactory = Nothing
        Using service = New IsolatedTemplateService(Language.VisualBasic, Encoding.Html, obj)
            Const template As String = "<h1>Hello @Model.Forename</h1>"
            Const expected As String = "<h1>Hello Matt &amp; World</h1>"

            Dim model = New Person() With { _
                .Forename = "Matt & World" _
            }
            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple template with no-encoding can be parsed.
    ''' </summary>
    ''' <remarks>
    ''' Text encoding is performed when writing objects to the template result (not literals). This test should 
    ''' show that the template service is correctly providing the appropriate encoding factory to process
    ''' the object's .ToString() and automatically encode it.
    ''' </remarks>
    <Test> _
    Public Sub IsolatedTemplateService_CanParseSimpleTemplate_UsingRawEncoding()
        Dim obj As IAppDomainFactory = Nothing
        Using service = New IsolatedTemplateService(Language.VisualBasic, Encoding.Raw, obj)
            Const template As String = "<h1>Hello @Model.Forename</h1>"
            Const expected As String = "<h1>Hello Matt & World</h1>"

            Dim model = New Person() With {.Forename = "Matt & World"}
            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the template service can parse multiple templates in sequence.
    ''' </summary>
    <Test> _
    Public Sub IsolatedTemplateService_CanParseMultipleTemplatesInSequence_WitNoModels()
        Using service = RazorHelper.GetIsolatedTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello World</h1>"
            Dim templates = Enumerable.Repeat(template, 10).ToArray()

            Dim results = service.ParseMany(templates, Nothing, Nothing, Nothing, False)

            Assert.That(templates.SequenceEqual(results), "Rendered templates do not match expected.")
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the template service can parse multiple templates in parallel.
    ''' </summary>
    <Test> _
    Public Sub IsolatedTemplateService_CanParseMultipleTemplatesInParallel_WitNoModels()
        Using service = RazorHelper.GetIsolatedTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello World</h1>"
            Dim templates = Enumerable.Repeat(template, 10).ToArray()

            Dim results = service.ParseMany(templates, Nothing, Nothing, Nothing, True)

            Assert.That(templates.SequenceEqual(results), "Rendered templates do not match expected.")
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the template service can parse multiple templates in sequence with complex models.
    ''' </summary>
    <Test> _
    Public Sub IsolatedTemplateService_CanParseMultipleTemplatesInSequence_WithComplexModels()
        Const maxTemplates As Integer = 10

        Using service = RazorHelper.GetIsolatedTemplateServiceForVisualBasic
            Const template As String = "<h1>Age: @Model.Age</h1>"
            Dim expected = Enumerable.Range(1, maxTemplates).[Select](Function(i) String.Format("<h1>Age: {0}</h1>", i))
            Dim templates = Enumerable.Repeat(template, maxTemplates).ToArray()
            Dim models = Enumerable.Range(1, maxTemplates).[Select](Function(i) New Person() With {.Age = i}).ToArray()

            Dim results = service.ParseMany(templates, models, Nothing, Nothing, False)
            Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.")
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the template service can parse multiple templates in parallel with complex models.
    ''' </summary>
    <Test> _
    Public Sub IsolatedTemplateService_CanParseMultipleTemplatesInParallel_WithComplexModels()
        Const maxTemplates As Integer = 10

        Using service = RazorHelper.GetIsolatedTemplateServiceForVisualBasic
            Const template As String = "<h1>Age: @Model.Age</h1>"
            Dim expected = Enumerable.Range(1, maxTemplates).[Select](Function(i) String.Format("<h1>Age: {0}</h1>", i))
            Dim templates = Enumerable.Repeat(template, maxTemplates).ToArray()
            Dim models = Enumerable.Range(1, maxTemplates).[Select](Function(i) New Person() With {.Age = i}).ToArray()

            Dim results = service.ParseMany(templates, models, Nothing, Nothing, True)
            Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.")
        End Using
    End Sub
#End Region
End Class
