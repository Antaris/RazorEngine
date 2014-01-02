Imports System.CodeDom
Imports RazorEngine.Compilation.Inspectors
Imports RazorEngine



Namespace TestTypes.Inspectors


    ''' <summary>
    ''' Defines a code inspector that will insert a throw statement into the generated code.
    ''' </summary>
    Public Class ThrowExceptionCodeInspector
        Implements ICodeInspector


#Region "Methods"
        ''' <summary>
        ''' Inspects the specified code unit.
        ''' </summary>
        ''' <param name="unit">The code unit.</param>
        ''' <param name="ns">The code namespace declaration.</param>
        ''' <param name="type">The code type declaration.</param>
        ''' <param name="executeMethod">The code method declaration for the Execute method.</param>
        Public Sub Inspect(unit As CodeCompileUnit, ns As CodeNamespace, type As CodeTypeDeclaration, executeMethod As CodeMemberMethod) Implements ICodeInspector.Inspect

            Dim statement = New CodeThrowExceptionStatement(New CodeObjectCreateExpression(New CodeTypeReference(GetType(System.InvalidOperationException)),
                                                                                           New CodeExpression() {}))
            executeMethod.Statements.Insert(0, statement)
        End Sub
#End Region

    End Class
End Namespace
