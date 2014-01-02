Imports RazorEngine.Templating

Namespace TestTypes.BaseTypes
    Public MustInherit Class NonGenericTemplateBase
        Inherits TemplateBase
        Public Function GetHelloWorldText() As String
            Return "Hello World"
        End Function
    End Class
End Namespace
