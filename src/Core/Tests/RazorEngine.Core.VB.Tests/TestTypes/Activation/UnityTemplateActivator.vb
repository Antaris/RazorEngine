Imports Microsoft.Practices.Unity
Imports RazorEngine
Imports RazorEngine.Templating



Namespace TestTypes.Activation



    ''' <summary>
    ''' Defines an activator that supports Unity.
    ''' </summary>
    Public Class UnityTemplateActivator
        Implements IActivator
#Region "Fields"
        Private ReadOnly _container As UnityContainer
#End Region

#Region "Constructor"
        ''' <summary>
        ''' Initialises a new instance of <see cref="UnityTemplateActivator"/>.
        ''' </summary>
        ''' <param name="container">The unity container.</param>
        Public Sub New(container As UnityContainer)
            If container Is Nothing Then
                Throw New ArgumentNullException("container")
            End If

            _container = container
        End Sub
#End Region

#Region "Methods"
        ''' <summary>
        ''' Creates an instance of the specifed template.
        ''' </summary>
        ''' <param name="context">The instance context.</param>
        ''' <returns>An instance of <see cref="ITemplate"/>.</returns>
        Public Function CreateInstance(context As InstanceContext) As ITemplate Implements IActivator.CreateInstance
            Return DirectCast(_container.Resolve(context.TemplateType), ITemplate)
        End Function
#End Region
    End Class
End Namespace
