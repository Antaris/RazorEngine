Imports System.Collections.Generic
Imports System.Dynamic
Namespace TestTypes

    Public Class ValueObject
        Inherits DynamicObject
#Region "Fields"
        Private ReadOnly _values As IDictionary(Of String, Object)
#End Region

#Region "Constructor"
        Public Sub New(values As IDictionary(Of String, Object))
            _values = values
        End Sub
#End Region

#Region "Methods"
        Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
            If _values.ContainsKey(binder.Name) Then
                result = _values(binder.Name)
                Return True
            End If

            result = Nothing
            Return False
        End Function
#End Region
    End Class
End Namespace
