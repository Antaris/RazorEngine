Imports RazorEngine.Compilation

Public Module Module1

    Sub Main()


        CompilerServiceBuilder.SetCompilerServiceFactory(New DefaultCompilerServiceFactory())

        Using service = RazorHelper.GetTemplateServiceForVisualBasic
            Const template = "<h1>Age: @Model.Age</h1>"
            Dim expected = Enumerable.Range(1, 10).Select(Function(i) String.Format("<h1>Age: {0}</h1>", i)).ToList()
            Dim templates = Enumerable.Repeat(template, 10).ToList()
            Dim models = Enumerable.Range(1, 10).Select(Function(i) New Person With {.Age = i})


            Dim results = service.ParseMany(templates, models, Nothing, Nothing, True).ToList()

            For i = 0 To 9
                System.Console.WriteLine(templates(i))
                System.Console.WriteLine(expected(i))
                System.Console.WriteLine(results(i))
            Next

        End Using


        System.Console.ReadKey()



    End Sub



    ''' <summary>
    ''' Defines a person.
    ''' </summary>
    <Serializable>
    Public Class Person

#Region "Properties"
        ''' <summary>
        ''' Gets or sets the age.
        ''' </summary>
        Public Property Age As Integer

        ''' <summary>
        ''' Gets or sets the forename.
        ''' </summary>
        Public Property Forename As String

        ''' <summary>
        ''' Gets or sets the surname.
        ''' </summary>
        Public Property Surname As String
#End Region
    End Class


End Module
