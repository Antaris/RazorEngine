Imports System.Threading

Namespace TestTypes

    ''' <summary>
    ''' Defines a thread pool item.
    ''' </summary>
    ''' <typeparam name="T">The model type.</typeparam>
    Public Class ThreadPoolItem(Of T)
#Region "Fields"
        Private ReadOnly _action As Action(Of T)
#End Region

#Region "Constructor"
        ''' <summary>
        ''' Initialises a new instance of <see cref="ThreadPoolItem{T}"/>.
        ''' </summary>
        ''' <param name="model">The model instance.</param>
        ''' <param name="resetEvent">The reset event.</param>
        ''' <param name="action">The action to run.</param>
        Public Sub New(model As T, resetEvent As ManualResetEvent, action As Action(Of T))
            MyBase.New()
            Me.Model = model
            Me.ResetEvent = resetEvent
            Me._action = action

        End Sub
#End Region

#Region "Methods"

        ''' <summary>
        ''' The callback method invoked by the threadpool.
        ''' </summary>
        ''' <param name="state">Any current state.</param>
        Public Sub ThreadPoolCallback(ByVal state As Object)
            Me._action(Me.Model)
            Me.ResetEvent.[Set]()
        End Sub
#End Region

#Region "Properties"
        ''' <summary>
        ''' Gets the model.
        ''' </summary>
        Public Property Model() As T


        ''' <summary>
        ''' Gets the reset event.
        ''' </summary>
        Public Property ResetEvent() As ManualResetEvent

#End Region
    End Class
End Namespace
