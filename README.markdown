RazorEngine
===========
## For VB.NET ##

A templating engine built on Microsoft's Razor parsing engine, RazorEngine allows you to use Razor syntax to build dynamic templates:

Code1
----------

	Dim config As New TemplateServiceConfiguration With {.Language = RazorEngine.Language.VisualBasic}

	Using service = New TemplateService(config)
      Const template = "@If Model IsNot Nothing Then " & vbCrLf &
                      "@<h1>Hello @Model.Forename</h1>" &
                      "else" & vbCrLf &
                      "@<h1>Hello World</h1>" &
                      "end if"

      Dim model = New With {.Forename = "Matt"}
      Dim result = service.Parse(template, model, Nothing, Nothing)
     
	End Using


Code2

----------
     Dim config As New TemplateServiceConfiguration With {.Language = RazorEngine.Language.VisualBasic, .Debug = True}
     Dim service = New TemplateService(config)

     Razor.SetTemplateService(service)

     Const template = "@If Model IsNot Nothing Then " & vbCrLf &
                      "@<h1>Hello @Model.Forename</h1>" &
                      "else" & vbCrLf &
                      "@<h1>Hello World</h1>" &
                      "end if"

     Dim model = New With {.Forename = "Matt"}

     Dim result = Razor.Parse(template, model, Nothing, Nothing)
     
     
     
     
Helper
----------
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

