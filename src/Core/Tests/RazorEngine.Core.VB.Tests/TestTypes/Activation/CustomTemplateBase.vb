

Imports RazorEngine.Templating


Namespace TestTypes.Activation




    ''' <summary>
    ''' Defines a test template base.
    ''' </summary>
    ''' <typeparam name="T">The model type.</typeparam>
    Public Class CustomTemplateBase(Of T)
        Inherits TemplateBase(Of T)
#Region "Fields"
        Private ReadOnly _formatter As ITextFormatter
#End Region

#Region "Methods"
        ''' <summary>
        ''' Initialises a new instance of <see cref="CustomTemplateBase{T}"/>
        ''' </summary>
        ''' <param name="formatter">The formatter.</param>
        Public Sub New(formatter As ITextFormatter)
            If formatter Is Nothing Then
                Throw New ArgumentNullException("formatter")
            End If

            _formatter = formatter
        End Sub
#End Region

#Region "Methods"
        ''' <summary>
        ''' Formats the specified object.
        ''' </summary>
        ''' <param name="value">The value to format.</param>
        ''' <returns>The string formatted value.</returns>
        Public Function Format(value As Object) As String
            Return _formatter.Format(value.ToString())
        End Function
#End Region
    End Class
End Namespace
