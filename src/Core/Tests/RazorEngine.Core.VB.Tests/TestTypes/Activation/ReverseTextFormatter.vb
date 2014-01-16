Imports RazorEngine
Imports RazorEngine.Text


Namespace TestTypes.Activation

    ''' <summary>
    ''' Reverses the contents of the specified string,
    ''' </summary>
    Public Class ReverseTextFormatter
        Implements ITextFormatter
#Region "Methods"
        ''' <summary>
        ''' Formats the specified value.
        ''' </summary>
        ''' <param name="value">The value to format.</param>
        ''' <returns>The formatted value.</returns>
        Public Function Format(value As String) As String Implements ITextFormatter.Format
            Dim content As Char() = value.ToCharArray()
            Array.Reverse(content)
            Return New String(content)
        End Function
#End Region
    End Class
End Namespace
