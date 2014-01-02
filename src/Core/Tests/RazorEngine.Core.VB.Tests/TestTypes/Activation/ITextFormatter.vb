Namespace TestTypes.Activation
    ''' <summary>
    ''' Defines the required contract for implementing a text formatter.
    ''' </summary>
    Public Interface ITextFormatter
#Region "Methods"
        ''' <summary>
        ''' Formats the specified value.
        ''' </summary>
        ''' <param name="value">The value to format.</param>
        ''' <returns>The formatted value.</returns>
        Function Format(value As String) As String
#End Region
    End Interface
End Namespace
