Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class PendingInspections
     Inherits System.Web.Services.WebService

	'<WebMethod()> _
	'Public Function HelloWorld() As String
	'	Return "Hello World"
	'End Function
	<WebMethod(enablesession:=True)> _
	Public Function RequestClosed(ByVal ReqID As String) As String
		HttpContext.Current.Session("LoginID") = "0340"
		Dim mRet As String = ""
		mRet = SIS.QCM.qcmRequestAllotment.RequestClosed(ReqID)
		If mRet = String.Empty Then
			Return "<h1>Closed successfully.</h1>"
		End If
		Return mRet
	End Function
	<WebMethod(enablesession:=True)> _
	Public Function RequestInspected(ByVal ReqID As String) As String
		HttpContext.Current.Session("LoginID") = "0340"
		Dim mRet As String = ""
		mRet = SIS.QCM.qcmRequestAllotment.RequestInspected(ReqID)
		If mRet = String.Empty Then
			Return "<h1>Updated successfully.</h1>"
		End If
		Return mRet
	End Function
	<WebMethod(EnableSEssion:=True)> _
	Public Function SendPendingRequest(ByVal EmpID As String) As String
		HttpContext.Current.Session("LoginID") = "0340"
		Dim mRet As String = SIS.QCM.qcmRequestAllotment.SendPendingCallEMail(EmpID)
		If mRet = String.Empty Then
			Return "<h1>Request submitted successfully.</h1>"
		End If
		Return mRet
	End Function

End Class
