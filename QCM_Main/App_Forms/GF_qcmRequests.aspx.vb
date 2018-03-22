Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.ComponentModel
Imports System.IO
Imports OfficeOpenXml
Imports System.Web.Script.Serialization
Partial Class GF_qcmRequests
  Inherits SIS.SYS.GridBase
  Protected Sub GVqcmRequests_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GVqcmRequests.RowCommand
    If e.CommandName.ToLower = "lgedit".ToLower Then
      Try
        Dim RequestID As Int32 = GVqcmRequests.DataKeys(e.CommandArgument).Values("RequestID")
        Dim RedirectUrl As String = TBLqcmRequests.EditUrl & "?RequestID=" & RequestID
        Response.Redirect(RedirectUrl)
      Catch ex As Exception
      End Try
    End If
    If e.CommandName.ToLower = "lgfwd".ToLower Then
      Try
        Dim RequestID As Int32 = GVqcmRequests.DataKeys(e.CommandArgument).Values("RequestID")
        SIS.QCM.qcmRequests.ForwardForAllotment(RequestID)
        GVqcmRequests.DataBind()
      Catch ex As Exception
        Dim message As String = New JavaScriptSerializer().Serialize(ex.Message)
        Dim script As String = String.Format("alert({0});", message)
        ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "", script, True)
      End Try
    End If
  End Sub
  Protected Sub GVqcmRequests_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles GVqcmRequests.Init
    DataClassName = "GqcmRequests"
    SetGridView = GVqcmRequests
  End Sub
  Protected Sub TBLqcmRequests_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles TBLqcmRequests.Init
    SetToolBar = TBLqcmRequests
  End Sub
  Protected Sub F_ProjectID_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles F_ProjectID.TextChanged
    Session("F_ProjectID") = F_ProjectID.Text
    Session("F_ProjectID_Display") = F_ProjectID_Display.Text
    InitGridPage()
  End Sub
  <System.Web.Services.WebMethod()>
  <System.Web.Script.Services.ScriptMethod()>
  Public Shared Function ProjectIDCompletionList(ByVal prefixText As String, ByVal count As Integer, ByVal contextKey As String) As String()
    Return SIS.QCM.qcmProjects.SelectqcmProjectsAutoCompleteList(prefixText, count, contextKey)
  End Function
  Protected Sub F_SupplierID_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles F_SupplierID.TextChanged
    Session("F_SupplierID") = F_SupplierID.Text
    Session("F_SupplierID_Display") = F_SupplierID_Display.Text
    InitGridPage()
  End Sub
  <System.Web.Services.WebMethod()>
  <System.Web.Script.Services.ScriptMethod()>
  Public Shared Function SupplierIDCompletionList(ByVal prefixText As String, ByVal count As Integer, ByVal contextKey As String) As String()
    Return SIS.QCM.qcmVendors.SelectqcmVendorsAutoCompleteList(prefixText, count, contextKey)
  End Function
  Protected Sub F_RequestStateID_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles F_RequestStateID.SelectedIndexChanged
    Session("F_RequestStateID") = F_RequestStateID.SelectedValue
    InitGridPage()
  End Sub
  Protected Sub F_InspectionStatusID_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles F_InspectionStatusID.SelectedIndexChanged
    Session("F_InspectionStatusID") = F_InspectionStatusID.SelectedValue
    InitGridPage()
  End Sub
  Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
    F_ProjectID_Display.Text = String.Empty
    If Not Session("F_ProjectID_Display") Is Nothing Then
      If Session("F_ProjectID_Display") <> String.Empty Then
        F_ProjectID_Display.Text = Session("F_ProjectID_Display")
      End If
    End If
    F_ProjectID.Text = String.Empty
    If Not Session("F_ProjectID") Is Nothing Then
      If Session("F_ProjectID") <> String.Empty Then
        F_ProjectID.Text = Session("F_ProjectID")
      End If
    End If
    Dim strScriptProjectID As String = "<script type=""text/javascript""> " &
     "function ACEProjectID_Selected(sender, e) {" &
     "  var F_ProjectID = $get('" & F_ProjectID.ClientID & "');" &
     "  var F_ProjectID_Display = $get('" & F_ProjectID_Display.ClientID & "');" &
     "  var retval = e.get_value();" &
     "  var p = retval.split('|');" &
     "  F_ProjectID.value = p[0];" &
     "  F_ProjectID_Display.innerHTML = e.get_text();" &
     "}" &
     "</script>"
    If Not Page.ClientScript.IsClientScriptBlockRegistered("F_ProjectID") Then
      Page.ClientScript.RegisterClientScriptBlock(GetType(System.String), "F_ProjectID", strScriptProjectID)
    End If
    Dim strScriptPopulatingProjectID As String = "<script type=""text/javascript""> " &
     "function ACEProjectID_Populating(o,e) {" &
     "  var p = $get('" & F_ProjectID.ClientID & "');" &
     "  p.style.backgroundImage  = 'url(../../images/loader.gif)';" &
     "  p.style.backgroundRepeat= 'no-repeat';" &
     "  p.style.backgroundPosition = 'right';" &
     "  o._contextKey = '';" &
     "}" &
     "function ACEProjectID_Populated(o,e) {" &
     "  var p = $get('" & F_ProjectID.ClientID & "');" &
     "  p.style.backgroundImage  = 'none';" &
     "}" &
     "</script>"
    If Not Page.ClientScript.IsClientScriptBlockRegistered("F_ProjectIDPopulating") Then
      Page.ClientScript.RegisterClientScriptBlock(GetType(System.String), "F_ProjectIDPopulating", strScriptPopulatingProjectID)
    End If
    F_SupplierID_Display.Text = String.Empty
    If Not Session("F_SupplierID_Display") Is Nothing Then
      If Session("F_SupplierID_Display") <> String.Empty Then
        F_SupplierID_Display.Text = Session("F_SupplierID_Display")
      End If
    End If
    F_SupplierID.Text = String.Empty
    If Not Session("F_SupplierID") Is Nothing Then
      If Session("F_SupplierID") <> String.Empty Then
        F_SupplierID.Text = Session("F_SupplierID")
      End If
    End If
    Dim strScriptSupplierID As String = "<script type=""text/javascript""> " &
     "function ACESupplierID_Selected(sender, e) {" &
     "  var F_SupplierID = $get('" & F_SupplierID.ClientID & "');" &
     "  var F_SupplierID_Display = $get('" & F_SupplierID_Display.ClientID & "');" &
     "  var retval = e.get_value();" &
     "  var p = retval.split('|');" &
     "  F_SupplierID.value = p[0];" &
     "  F_SupplierID_Display.innerHTML = e.get_text();" &
     "}" &
     "</script>"
    If Not Page.ClientScript.IsClientScriptBlockRegistered("F_SupplierID") Then
      Page.ClientScript.RegisterClientScriptBlock(GetType(System.String), "F_SupplierID", strScriptSupplierID)
    End If
    Dim strScriptPopulatingSupplierID As String = "<script type=""text/javascript""> " &
     "function ACESupplierID_Populating(o,e) {" &
     "  var p = $get('" & F_SupplierID.ClientID & "');" &
     "  p.style.backgroundImage  = 'url(../../images/loader.gif)';" &
     "  p.style.backgroundRepeat= 'no-repeat';" &
     "  p.style.backgroundPosition = 'right';" &
     "  o._contextKey = '';" &
     "}" &
     "function ACESupplierID_Populated(o,e) {" &
     "  var p = $get('" & F_SupplierID.ClientID & "');" &
     "  p.style.backgroundImage  = 'none';" &
     "}" &
     "</script>"
    If Not Page.ClientScript.IsClientScriptBlockRegistered("F_SupplierIDPopulating") Then
      Page.ClientScript.RegisterClientScriptBlock(GetType(System.String), "F_SupplierIDPopulating", strScriptPopulatingSupplierID)
    End If
    F_RequestStateID.SelectedValue = String.Empty
    If Not Session("F_RequestStateID") Is Nothing Then
      If Session("F_RequestStateID") <> String.Empty Then
        F_RequestStateID.SelectedValue = Session("F_RequestStateID")
      End If
    End If
    F_InspectionStatusID.SelectedValue = String.Empty
    If Not Session("F_InspectionStatusID") Is Nothing Then
      If Session("F_InspectionStatusID") <> String.Empty Then
        F_InspectionStatusID.SelectedValue = Session("F_InspectionStatusID")
      End If
    End If
    Dim validateScriptProjectID As String = "<script type=""text/javascript"">" &
     "  function validate_ProjectID(o) {" &
     "    validated_FK_QCM_Requests_ProjectID_main = true;" &
     "    validate_FK_QCM_Requests_ProjectID(o);" &
     "  }" &
     "</script>"
    If Not Page.ClientScript.IsClientScriptBlockRegistered("validateProjectID") Then
      Page.ClientScript.RegisterClientScriptBlock(GetType(System.String), "validateProjectID", validateScriptProjectID)
    End If
    Dim validateScriptSupplierID As String = "<script type=""text/javascript"">" &
     "  function validate_SupplierID(o) {" &
     "    validated_FK_QCM_Requests_SupplierID_main = true;" &
     "    validate_FK_QCM_Requests_SupplierID(o);" &
     "  }" &
      "</script>"
    If Not Page.ClientScript.IsClientScriptBlockRegistered("validateSupplierID") Then
      Page.ClientScript.RegisterClientScriptBlock(GetType(System.String), "validateSupplierID", validateScriptSupplierID)
    End If
    Dim validateScriptFK_QCM_Requests_ProjectID As String = "<script type=""text/javascript"">" &
     "  function validate_FK_QCM_Requests_ProjectID(o) {" &
     "    var value = o.id;" &
     "    var ProjectID = $get('" & F_ProjectID.ClientID & "');" &
     "    try{" &
     "    if(ProjectID.value==''){" &
     "      if(validated_FK_QCM_Requests_ProjectID.main){" &
     "        var o_d = $get(o.id +'_Display');" &
     "        try{o_d.innerHTML = '';}catch(ex){}" &
     "      }" &
     "    }" &
     "    value = value + ',' + ProjectID.value ;" &
     "    }catch(ex){}" &
     "    o.style.backgroundImage  = 'url(../../images/pkloader.gif)';" &
     "    o.style.backgroundRepeat= 'no-repeat';" &
     "    o.style.backgroundPosition = 'right';" &
     "    PageMethods.validate_FK_QCM_Requests_ProjectID(value, validated_FK_QCM_Requests_ProjectID);" &
     "  }" &
     "  validated_FK_QCM_Requests_ProjectID_main = false;" &
     "  function validated_FK_QCM_Requests_ProjectID(result) {" &
     "    var p = result.split('|');" &
     "    var o = $get(p[1]);" &
     "    var o_d = $get(p[1]+'_Display');" &
     "    try{o_d.innerHTML = p[2];}catch(ex){}" &
     "    o.style.backgroundImage  = 'none';" &
     "    if(p[0]=='1'){" &
     "      o.value='';" &
     "      try{o_d.innerHTML = '';}catch(ex){}" &
     "      __doPostBack(o.id, o.value);" &
     "    }" &
     "    else" &
     "      __doPostBack(o.id, o.value);" &
     "  }" &
      "</script>"
    If Not Page.ClientScript.IsClientScriptBlockRegistered("validateFK_QCM_Requests_ProjectID") Then
      Page.ClientScript.RegisterClientScriptBlock(GetType(System.String), "validateFK_QCM_Requests_ProjectID", validateScriptFK_QCM_Requests_ProjectID)
    End If
    Dim validateScriptFK_QCM_Requests_SupplierID As String = "<script type=""text/javascript"">" &
     "  function validate_FK_QCM_Requests_SupplierID(o) {" &
     "    var value = o.id;" &
     "    var SupplierID = $get('" & F_SupplierID.ClientID & "');" &
     "    try{" &
     "    if(SupplierID.value==''){" &
     "      if(validated_FK_QCM_Requests_SupplierID.main){" &
     "        var o_d = $get(o.id +'_Display');" &
     "        try{o_d.innerHTML = '';}catch(ex){}" &
     "      }" &
     "    }" &
     "    value = value + ',' + SupplierID.value ;" &
     "    }catch(ex){}" &
     "    o.style.backgroundImage  = 'url(../../images/pkloader.gif)';" &
     "    o.style.backgroundRepeat= 'no-repeat';" &
     "    o.style.backgroundPosition = 'right';" &
     "    PageMethods.validate_FK_QCM_Requests_SupplierID(value, validated_FK_QCM_Requests_SupplierID);" &
     "  }" &
     "  validated_FK_QCM_Requests_SupplierID_main = false;" &
     "  function validated_FK_QCM_Requests_SupplierID(result) {" &
     "    var p = result.split('|');" &
     "    var o = $get(p[1]);" &
     "    var o_d = $get(p[1]+'_Display');" &
     "    try{o_d.innerHTML = p[2];}catch(ex){}" &
     "    o.style.backgroundImage  = 'none';" &
     "    if(p[0]=='1'){" &
     "      o.value='';" &
     "      try{o_d.innerHTML = '';}catch(ex){}" &
     "      __doPostBack(o.id, o.value);" &
     "    }" &
     "    else" &
     "      __doPostBack(o.id, o.value);" &
     "  }" &
      "</script>"
    If Not Page.ClientScript.IsClientScriptBlockRegistered("validateFK_QCM_Requests_SupplierID") Then
      Page.ClientScript.RegisterClientScriptBlock(GetType(System.String), "validateFK_QCM_Requests_SupplierID", validateScriptFK_QCM_Requests_SupplierID)
    End If
  End Sub
  <System.Web.Services.WebMethod()>
  Public Shared Function validate_FK_QCM_Requests_ProjectID(ByVal value As String) As String
    Dim aVal() As String = value.Split(",".ToCharArray)
    Dim mRet As String = "0|" & aVal(0)
    Dim ProjectID As String = CType(aVal(1), String)
    Dim oVar As SIS.QCM.qcmProjects = SIS.QCM.qcmProjects.qcmProjectsGetByID(ProjectID)
    If oVar Is Nothing Then
      mRet = "1|" & aVal(0) & "|Record not found."
    Else
      mRet = "0|" & aVal(0) & "|" & oVar.DisplayField
    End If
    Return mRet
  End Function
  <System.Web.Services.WebMethod()>
  Public Shared Function validate_FK_QCM_Requests_SupplierID(ByVal value As String) As String
    Dim aVal() As String = value.Split(",".ToCharArray)
    Dim mRet As String = "0|" & aVal(0)
    Dim SupplierID As String = CType(aVal(1), String)
    Dim oVar As SIS.QCM.qcmVendors = SIS.QCM.qcmVendors.qcmVendorsGetByID(SupplierID)
    If oVar Is Nothing Then
      mRet = "1|" & aVal(0) & "|Record not found."
    Else
      mRet = "0|" & aVal(0) & "|" & oVar.DisplayField
    End If
    Return mRet
  End Function
End Class
