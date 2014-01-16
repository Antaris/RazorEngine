
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
''' Defines a test fixture that provides tests for the <see cref="TemplateService"/> type.
''' </summary>
<TestFixture> _
Public Class TemplateServiceTestFixture
#Region "Tests"
    ''' <summary>
    ''' Tests that a simple template without a model can be parsed.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseSimpleTemplate_WithNoModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
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
    Public Sub TemplateService_CanParseSimpleTemplate_WithComplexModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello @Model.Forename</h1>"
            Const expected As String = "<h1>Hello Matt</h1>"

            Dim model = New Person() With {.Forename = "Matt"}

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple template with an anonymous model can be parsed.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseSimpleTemplate_WithAnonymousModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello @Model.Forename</h1>"
            Const expected As String = "<h1>Hello Matt</h1>"

            Dim model = New With {.Forename = "Matt"}

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple template with a expando model can be parsed.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseSimpleTemplate_WithExpandoModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello @Model.Forename</h1>"
            Const expected As String = "<h1>Hello Matt</h1>"

            Dim model As Object = New ExpandoObject()
            model.Forename = "Matt"

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple template with a dynamic model can be parsed.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseSimpleTemplate_WithDynamicModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello @Model.Forename</h1>"
            Const expected As String = "<h1>Hello Matt</h1>"

            Dim model As Object = New ValueObject(New Dictionary(Of String, Object)() From {{"Forename", "Matt"}})

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple template with an iterator model can be parsed.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseSimpleTemplate_WithIteratorModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "@For Each item In Model " & "@item" & " Next"
            Const expected As String = "One Two Three"

            Dim model = CreateIterator("One ", "Two ", "Three")
            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    Private Shared Function CreateIterator(Of T)(ParamArray items As T()) As IEnumerable(Of T)
        If items IsNot Nothing Then Return items.AsEnumerable
        Return Enumerable.Empty(Of T)()
    End Function

    ''' <summary>
    ''' Tests that a simple template with html-encoding can be parsed.
    ''' </summary>
    ''' <remarks>
    ''' Text encoding is performed when writing objects to the template result (not literals). This test should 
    ''' show that the template service is correctly providing the appropriate encoding factory to process
    ''' the object's .ToString() and automatically encode it.
    ''' </remarks>
    <Test> _
    Public Sub TemplateService_CanParseSimpleTemplate_UsingHtmlEncoding()

        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello @Model.String</h1>"
            Const expected As String = "<h1>Hello Matt &amp; World</h1>"

            Dim model = New With {.[String] = "Matt & World"}

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
    Public Sub TemplateService_CanParseSimpleTemplate_UsingRawEncoding()

        Dim config = New TemplateServiceConfiguration() With {.EncodedStringFactory = New RawStringFactory(), .Language = VisualBasic}

        Using service = config.GetTemplateService
            Const template As String = "<h1>Hello @Model.String</h1>"
            Const expected As String = "<h1>Hello Matt & World</h1>"

            Dim model = New With {.[String] = "Matt & World"}

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the template service can parse multiple templates in sequence.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseMultipleTemplatesInSequence_WitNoModels()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic

            Const template As String = "<h1>Hello World</h1>"
            Dim templates = Enumerable.Repeat(template, 10)

            Dim results = service.ParseMany(templates, Nothing, Nothing, Nothing, False)

            Assert.That(templates.SequenceEqual(results), "Rendered templates do not match expected.")
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the template service can parse multiple templates in parallel.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseMultipleTemplatesInParallel_WitNoModels()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello World</h1>"
            Dim templates = Enumerable.Repeat(template, 10)

            Dim results = service.ParseMany(templates, Nothing, Nothing, Nothing, True)

            Assert.That(templates.SequenceEqual(results), "Rendered templates do not match expected.")
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the template service can parse multiple templates in sequence with complex models.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseMultipleTemplatesInSequence_WithComplexModels()
        Const maxTemplates As Integer = 10

        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>Age: @Model.Age</h1>"

            Dim expected = Enumerable.Range(1, maxTemplates).Select(Function(i) String.Format("<h1>Age: {0}</h1>", i))

            Dim templates = Enumerable.Repeat(template, maxTemplates)

            Dim models = Enumerable.Range(1, maxTemplates).Select(Function(i) New Person() With {.Age = i})

            Dim results = service.ParseMany(templates, models, Nothing, Nothing, False)
            Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.")
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the template service can parse multiple templates in parallel with complex models.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseMultipleTemplatesInParallel_WithComplexModels()
        Const maxTemplates As Integer = 10

        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>Age: @Model.Age</h1>"
            Dim expected = Enumerable.Range(1, maxTemplates).Select(Function(i) String.Format("<h1>Age: {0}</h1>", i))
            Dim templates = Enumerable.Repeat(template, maxTemplates)
            Dim models = Enumerable.Range(1, maxTemplates).Select(Function(i) New Person() With {.Age = i})

            Dim results = service.ParseMany(templates, models, Nothing, Nothing, True)
            Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.")
        End Using
    End Sub

    ''' <summary>
    ''' Tests that the template service can parse templates when using a manual threading model (i.e. manually creating <see cref="Thread"/>
    ''' instances and maintaining their lifetime.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseTemplatesInParallel_WithManualThreadModel()
        Dim service = New TemplateService()

        Const threadCount As Integer = 10
        Const template As String = "<h1>Hello you are @Model.Age</h1>"

        Dim threads = New List(Of Thread)

        For i As Integer = 0 To threadCount - 1
            ' Capture enumerating index here to avoid closure issues.
            Dim index As Integer = i

            Dim thread = New Thread(Sub()
                                        Dim model = New Person() With {.Age = index}
                                        Dim expected As String = "<h1>Hello you are " & index & "</h1>"
                                        Dim result As String = service.Parse(template, model, Nothing, Nothing)

                                        Assert.AreEqual(expected, result, "Result does not match expected: " & result)

                                    End Sub)

            threads.Add(thread)
            thread.Start()
        Next

        ' Block until all threads have joined.
        threads.ForEach(Sub(t) t.Join())

        service.Dispose()
    End Sub


    ''' <summary>
    ''' Tests that the template service can parse templates when using the threadpool.
    ''' </summary>
    <Test>
    Public Sub TemplateService_CanParseTemplatesInParallel_WithThreadPool()
        Dim service = New TemplateService()

        Const count As Integer = 10
        Const template As String = "<h1>Hello you are @Model.Age</h1>"

        ' As we are leaving the threading to the pool, we need a way of coordinating the execution
        '             * of the test after the threadpool has done its work. ManualResetEvent instances are the way. 

        Dim resetEvents = New ManualResetEvent(count - 1) {}

        For i As Integer = 0 To count - 1
            ' Capture enumerating index here to avoid closure issues.
            Dim index As Integer = i

            Dim expected As String = "<h1>Hello you are " & index & "</h1>"

            resetEvents(index) = New ManualResetEvent(False)

            Dim model = New Person() With {.Age = index}

            Dim item = New ThreadPoolItem(Of Person)(model, resetEvents(index), Sub()
                                                                                    Dim result As String = service.Parse(template, model, Nothing, Nothing)

                                                                                    Assert.That(result = expected, "Result does not match expected: " & result)

                                                                                End Sub)

            ThreadPool.QueueUserWorkItem(AddressOf item.ThreadPoolCallback)
        Next

        ' Block until all events have been set.
        WaitHandle.WaitAll(resetEvents)

        service.Dispose()
    End Sub

    ''' <summary>
    ''' Tests that a template service can precompile a template for later execution.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanPrecompileTemplate_WithNoModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "Hello World"
            Const expected As String = "Hello World"

            service.Compile(template, Nothing, "test")

            Dim result As String = service.Run("test", Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected.")
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a template service can precompile a template with a non generic base for later execution.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanPrecompileTemplate_WithNoModelAndANonGenericBase()
        Dim config = New TemplateServiceConfiguration() With {.BaseTemplateType = GetType(NonGenericTemplateBase),
                                                              .Language = VisualBasic}

        Using service = config.GetTemplateService
            Const template As String = "<h1>@GetHelloWorldText()</h1>"
            Const expected As String = "<h1>Hello World</h1>"

            service.Compile(template, Nothing, "test")

            Dim result As String = service.Run("test", Nothing, Nothing)
            Assert.That(result = expected, "Result does not match expected.")
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a template service can precompile a template for later execution.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanPrecompileTemplate_WithSimpleModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "Hello @Model.Forename"
            Const expected As String = "Hello Matt"

            Dim model = New Person() With {.Forename = "Matt"}

            service.Compile(template, GetType(Person), "test")

            Dim result As String = service.Run("test", model, Nothing)

            Assert.That(result = expected, "Result does not match expected.")
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple helper template with html-encoding can be parsed.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseSimpleHelperTemplate_UsingHtmlEncoding()
        Using service = RazorHelper.GetTemplateServiceConfigurationForVisualBasic.WithDebug.GetTemplateService

            Const template As String = "<h1>Hello @NameHelper()</h1>" &
                                        "@Helper NameHelper() " & vbCr &
                                          "@Model.String" & vbCr &
                                         "End Helper"
            Const expected As String = "<h1>Hello Matt &amp; World</h1>"

            Dim model = New With {.[String] = "Matt & World"}

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.AreEqual(expected, result, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a simple helper template with no-encoding can be parsed.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseSimpleHelperTemplate_UsingRawEncoding()
        Dim config = New TemplateServiceConfiguration() With {
            .EncodedStringFactory = New RawStringFactory(),
            .Language = VisualBasic
        }

        Using service = config.GetTemplateService

            Const template As String = "<h1>Hello @NameHelper()</h1>" &
                                      "@Helper NameHelper() " & vbCr &
                                        "@Model.String " &
                                       "End Helper"
            Const expected As String = "<h1>Hello Matt & World</h1>"

            Dim model = New With {.[String] = "Matt & World"}

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.AreEqual(expected, result, "Result does not match expected: " & result)
        End Using
    End Sub

    <Test> _
    Public Sub TemplateService_CanParseSimpleTemplate_WithCorrectBaseTypeFromModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>Hello @Model.Forename</h1>"

            Dim model = New Person() With {.Forename = "Matt"}

            Dim templateInstance = service.CreateTemplate(template, Nothing, model)

            Assert.NotNull(TryCast(templateInstance, ITemplate(Of Person)), "Template is not derived from the correct base type")
        End Using
    End Sub

    <Test> _
    Public Sub TemplateService_CanParseSimpleTemplate_WithNonGenericBaseType()
        Dim config = New TemplateServiceConfiguration() With {.BaseTemplateType = GetType(NonGenericTemplateBase),
                                                              .Language = VisualBasic
                                                             }

        Using service = New TemplateService(config)
            Const template As String = "<h1>@GetHelloWorldText()</h1>"

            Dim templateInstance = service.CreateTemplate(template, Nothing, Nothing)

            Assert.NotNull(TryCast(templateInstance, NonGenericTemplateBase), "Template is not derived from the correct base type")
        End Using
    End Sub

    <Test> _
    Public Sub TemplateService_CanParseSimpleTemplate_UsingLinqExtensionMethodOnArrayTypeModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>There are @Model.Take(2).ToList().Count() animals</h1>"
            Const expected As String = "<h1>There are 2 animals</h1>"

            Dim model = New List(Of Animal)
            model.Add(New Animal With {.Type = "Cat"})
            model.Add(New Animal With {.Type = "Dog"})

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    <Test> _
    Public Sub TemplateService_CanParseSimpleTemplate_UsingLinqExtensionMethodOnArrayTypeFromModel()
        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<h1>There are @Model.Animals.Take(2).ToList().Count() animals</h1>"
            Const expected As String = "<h1>There are 2 animals</h1>"

            Dim model = New AnimalViewModel() With {.Animals = (New List(Of Animal) From {New Animal() With {.Type = "Cat"}, New Animal() With {.Type = "Dog"}}).ToArray()}


            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a tilde is expanded with html-encoding.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseTildeInTemplate_UsingHtmlEncoding()

        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template As String = "<a href=""~/index.html"">@Model.String</a>"
            Const expected As String = "<a href=""/index.html"">Matt</a>"

            Dim model = New With {.[String] = "Matt"}
            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub

    ''' <summary>
    ''' Tests that a tilde is expanded with no-encoding.
    ''' </summary>
    <Test> _
    Public Sub TemplateService_CanParseTildeInTemplate_UsingRawEncoding()
        Dim config = New TemplateServiceConfiguration() With {
            .EncodedStringFactory = New RawStringFactory(),
            .Language = VisualBasic
        }

        Using service = New TemplateService(config)
            Const template As String = "<a href=""~/index.html"">@Model.String</a>"
            Const expected As String = "<a href=""/index.html"">Matt</a>"

            Dim model = New With {.[String] = "Matt"}

            Dim result As String = service.Parse(template, model, Nothing, Nothing)

            Assert.That(result = expected, "Result does not match expected: " & result)
        End Using
    End Sub
#End Region
End Class

