Imports System.Runtime.CompilerServices
Imports RazorEngine
Imports RazorEngine.Templating
Imports RazorEngine.Configuration


''' <summary>
''' For VB Helper
''' </summary>
''' <remarks></remarks>
Public Module RazorHelper



#Region "TemplateServiceConfiguration"

    ''' <summary>
    ''' Get TemplateServiceConfiguration use VisualBasic
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetTemplateServiceConfigurationForVisualBasic() As TemplateServiceConfiguration
        Return New TemplateServiceConfiguration With {.Language = Language.VisualBasic}
    End Function


    ''' <summary>
    ''' Set TemplateServiceConfiguration Debug is True
    ''' </summary>
    ''' <param name="config"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function WithDebug(config As TemplateServiceConfiguration) As TemplateServiceConfiguration
        If config IsNot Nothing Then
            config.Debug = True
        End If
        Return config
    End Function



    ''' <summary>
    ''' Set TemplateServiceConfiguration Language is Language.VisualBasic
    ''' </summary>
    ''' <param name="config"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function WithVisualBasic(config As TemplateServiceConfiguration) As TemplateServiceConfiguration
        If config IsNot Nothing Then
            config.Language = Language.VisualBasic
        End If
        Return config
    End Function


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="config"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetTemplateService(config As TemplateServiceConfiguration) As ITemplateService
        If config Is Nothing Then Return Nothing
        Return New TemplateService(config)
    End Function



#End Region
   

#Region "FluentTemplateServiceConfiguration"

    ''' <summary>
    ''' Get FluentTemplateServiceConfiguration use VisualBasic
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetFluentTemplateServiceConfigurationForVisualBasic() As FluentTemplateServiceConfiguration

        Return New FluentTemplateServiceConfiguration(Sub(c)
                                                          c.WithCodeLanguage(Language.VisualBasic)
                                                      End Sub)
    End Function





#End Region




#Region "Get TemplateService"
    ''' <summary>
    ''' Get TemplateService use VisualBasic
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetTemplateServiceForVisualBasic() As ITemplateService
        Return GetTemplateServiceConfigurationForVisualBasic.GetTemplateService
    End Function



    ''' <summary>
    ''' Get TemplateService use VisualBasic
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetIsolatedTemplateServiceForVisualBasic() As ITemplateService
        Return New IsolatedTemplateService(Language.VisualBasic)
    End Function


#End Region



#Region "Razor"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetRazorForVisualBasic()
        Dim service = GetTemplateServiceForVisualBasic()
        Razor.SetTemplateService(service)
    End Sub




#End Region



End Module
