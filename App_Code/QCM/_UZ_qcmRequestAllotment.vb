Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.ComponentModel
Imports System.Net.Mail
Imports System.Web.Mail
Namespace SIS.QCM
	Partial Public Class qcmRequestAllotment
		Public ReadOnly Property AllotVisible() As Boolean
			Get
				If RequestStateID = "UNDERALLOT" Then
					Return True
				End If
				Return False
			End Get
		End Property
		Public ReadOnly Property SendSMSVisible() As Boolean
			Get
        If RequestStateID = "ALLOTED" Or RequestStateID = "REALLOTED" Then
          Return True
        End If
        Return False
			End Get
		End Property
		Public Shared Function UZ_qcmPendingInspectionSelectList(ByVal AllotedTo As String) As List(Of SIS.QCM.qcmRequestAllotment)
			Dim Results As List(Of SIS.QCM.qcmRequestAllotment) = Nothing
			Using Con As SqlConnection = New SqlConnection(SIS.SYS.SQLDatabase.DBCommon.GetConnectionString())
				Using Cmd As SqlCommand = Con.CreateCommand()
					Cmd.CommandType = CommandType.StoredProcedure
					Cmd.CommandText = "spqcm_LG_PendingInspectionSelectList"
					SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@AllotedTo", SqlDbType.NVarChar, 8, AllotedTo)
					Cmd.Parameters.Add("@RecordCount", SqlDbType.Int)
					Cmd.Parameters("@RecordCount").Direction = ParameterDirection.Output
					RecordCount = -1
					Results = New List(Of SIS.QCM.qcmRequestAllotment)()
					Con.Open()
					Dim Reader As SqlDataReader = Cmd.ExecuteReader()
					While (Reader.Read())
						Results.Add(New SIS.QCM.qcmRequestAllotment(Reader))
					End While
					Reader.Close()
					RecordCount = Cmd.Parameters("@RecordCount").Value
				End Using
			End Using
			Return Results
		End Function
    Public Shared Function UZ_qcmRequestAllotmentSelectList(ByVal StartRowIndex As Integer, ByVal MaximumRows As Integer, ByVal OrderBy As String, ByVal SearchState As Boolean, ByVal SearchText As String, ByVal ProjectID As String, ByVal SupplierID As String, ByVal RequestStateID As String, ByVal AllotedTo As String, ByVal RegionID As String) As List(Of SIS.QCM.qcmRequestAllotment)
      Dim Results As List(Of SIS.QCM.qcmRequestAllotment) = Nothing
      Using Con As SqlConnection = New SqlConnection(SIS.SYS.SQLDatabase.DBCommon.GetConnectionString())
        Using Cmd As SqlCommand = Con.CreateCommand()
          If OrderBy = String.Empty Then OrderBy = "RequestID DESC"
          Cmd.CommandType = CommandType.StoredProcedure
          If SearchState Then
            Cmd.CommandText = "spqcm_LG_RequestAllotmentSelectListSearch"
            SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@KeyWord", SqlDbType.NVarChar, 250, SearchText)
          Else
            Cmd.CommandText = "spqcm_LG_RequestAllotmentSelectListFilteres"
            SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@Filter_ProjectID", SqlDbType.NVarChar, 6, IIf(ProjectID Is Nothing, String.Empty, ProjectID))
            SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@Filter_AllotedTo", SqlDbType.NVarChar, 8, IIf(AllotedTo Is Nothing, String.Empty, AllotedTo))
            SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@Filter_SupplierID", SqlDbType.NVarChar, 6, IIf(SupplierID Is Nothing, String.Empty, SupplierID))
            SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@Filter_RequestStateID", SqlDbType.NVarChar, 10, IIf(RequestStateID Is Nothing, String.Empty, RequestStateID))
            SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@Filter_RegionID", SqlDbType.NVarChar, 10, IIf(RegionID Is Nothing, String.Empty, RegionID))
          End If
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@StartRowIndex", SqlDbType.Int, -1, StartRowIndex)
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@MaximumRows", SqlDbType.Int, -1, MaximumRows)
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@LoginID", SqlDbType.NVarChar, 9, HttpContext.Current.Session("LoginID"))
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@OrderBy", SqlDbType.NVarChar, 50, OrderBy)
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@RequestStateID", SqlDbType.NVarChar, 10, "UNDERALLOT")
          Cmd.Parameters.Add("@RecordCount", SqlDbType.Int)
          Cmd.Parameters("@RecordCount").Direction = ParameterDirection.Output
          RecordCount = -1
          Results = New List(Of SIS.QCM.qcmRequestAllotment)()
          Con.Open()
          Dim Reader As SqlDataReader = Cmd.ExecuteReader()
          While (Reader.Read())
            Results.Add(New SIS.QCM.qcmRequestAllotment(Reader))
          End While
          Reader.Close()
          RecordCount = Cmd.Parameters("@RecordCount").Value
        End Using
      End Using
      Return Results
    End Function
    Public Shared Function qcmRequestAllotmentSelectCount(ByVal SearchState As Boolean, ByVal SearchText As String, ByVal ProjectID As String, ByVal SupplierID As String, ByVal RequestStateID As String, ByVal AllotedTo As String, ByVal RegionID As String) As Integer
      Return RecordCount
    End Function
    Public Shared Function UZ_qcmRequestAllotmentUpdate(ByVal Record As SIS.QCM.qcmRequestAllotment) As SIS.QCM.qcmRequestAllotment
      Dim _Rec As SIS.QCM.qcmRequestAllotment = SIS.QCM.qcmRequestAllotment.qcmRequestAllotmentGetByID(Record.RequestID)
      With _Rec
        Select Case _Rec.RequestStateID
          Case "ALLOTED", "REALLOTED"
            If _Rec.AllotedTo <> Record.AllotedTo Then
              SendCancellationEMail(_Rec)
              .RequestStateID = "REALLOTED"
              .DocumentID = .AllotedTo
            End If
          Case Else
            .RequestStateID = "ALLOTED"
        End Select
        .AllotedTo = Record.AllotedTo
        .AllotedStartDate = Record.AllotedStartDate
        .AllotedFinishDate = Record.AllotedFinishDate
        .AllotedOn = Now
        .AllotedBy = Global.System.Web.HttpContext.Current.Session("LoginID")
        .AllotmentRemarks = Record.AllotmentRemarks
      End With
      _Rec = SIS.QCM.qcmRequestAllotment.UpdateData(_Rec)
      SendSMSandEMail(_Rec)
      Return _Rec
    End Function
    Public Shared Function SendSMSandEMail(ByVal RequestID As Integer) As String
      Dim _Rec As SIS.QCM.qcmRequestAllotment = SIS.QCM.qcmRequestAllotment.qcmRequestAllotmentGetByID(RequestID)
      If _Rec.RequestStateID = "REALLOTED" Then
        If _Rec.DocumentID <> String.Empty Then
          Try
            _Rec.AllotedTo = _Rec.DocumentID
            SendCancellationEMail(_Rec)
          Catch ex As Exception
          End Try
        End If
      End If
      _Rec = SIS.QCM.qcmRequestAllotment.qcmRequestAllotmentGetByID(RequestID)
      Return SendSMSandEMail(_Rec)
    End Function
    Public Shared Function GetPendigRequestHTML(ByVal EMPID As String) As String
			Dim sb As New StringBuilder
			With sb
				Dim tmps As List(Of SIS.QCM.qcmRequestAllotment) = SIS.QCM.qcmRequestAllotment.UZ_qcmPendingInspectionSelectList(EMPID)
				If tmps.Count > 0 Then
					.AppendLine("<table style=""width:900px;margin-top:15px"" border=""1"" cellspacing=""1"" cellpadding=""1"">")
          .AppendLine("<tr><td colspan=""4"" style=""color:red"" align=""center""><h3><b>PENDING INSPECTION CALL REQUEST</b></h3></td></tr>")
          .AppendLine("<tr><td colspan=""4"" align=""center""><h4>Please Click on Link given in <b>START</b> column against each pending call. It will update call ispection <b>START DATE</b> and click on link in <b>CLOSE</b> column of the started inspection to <b>CLOSE</b> the call.</h4></td></tr>")
          .AppendLine("<tr><td bgcolor=""lightgray""><b>REQUEST DETAILS</b></td>")
					.AppendLine("<td bgcolor=""lightgray""><b>START</b></td>")
          .AppendLine("<td bgcolor=""lightgray""><b>INSPECTION DATA</b></td>")
          .AppendLine("<td bgcolor=""lightgray""><b>CLOSE</b></td></tr>")
          For Each tm As SIS.QCM.qcmRequestAllotment In tmps
						.AppendLine("<tr><td>")
						.AppendLine("<li>" & tm.IDM_Projects6_Description & " [" & tm.ProjectID & "]" & "</li>")
						.AppendLine("<li>Order No: " & tm.OrderNo & "</li>")
						.AppendLine("<li>" & tm.IDM_Vendors7_Description & "</li>")
						.AppendLine("<li>" & tm.AllotmentRemarks & "</li>")
						.AppendLine("<li>Inspection Date: " & tm.AllotedFinishDate & "</li>")
						.AppendLine("</td>")
						.AppendLine("<td style=""vertical-align:middle;text-align:center"">")
            If tm.RequestStateID = "ALLOTED" Or tm.RequestStateID = "REALLOTED" Then
              .AppendLine("<a href=""http://cloud.isgec.co.in/WebQcm1/QcmCallInspected.aspx?start=" & tm.RequestID & "&emp=" & tm.AllotedTo & """ style=""color:Green; font-weight:bold;"">STARTED</a>")
            End If
            .AppendLine("</td>")
            .AppendLine("<td style=""vertical-align:middle;text-align:center"">")
            If tm.RequestStateID = "INSPECTED" Then
              If Convert.ToDateTime(tm.CreatedOn) >= Convert.ToDateTime("26/07/2017") Then
                .AppendLine("<a href=""http://cloud.isgec.co.in/WebQcm1/QcmData.aspx?data=" & tm.RequestID & "&emp=" & tm.AllotedTo & """ style=""color:Green; font-weight:bold;"">INSPECTION DATA</a>")
              End If
            End If
            .AppendLine("</td>")
            .AppendLine("<td style=""vertical-align:middle;text-align:center"">")
            If tm.RequestStateID = "INSPECTED" Then
              Dim tmpIns As List(Of SIS.QCM.qcmInspections) = SIS.QCM.qcmInspections.qcmInspectionsSelectList(0, 10, "", False, "", tm.RequestID)
              If tmpIns.Count > 1 Then
                .AppendLine("<a href=""http://cloud.isgec.co.in/WebQcm1/QcmCallInspected.aspx?stop=" & tm.RequestID & "&emp=" & tm.AllotedTo & """ style=""color:Green; font-weight:bold;"">CLOSED</a>")
              End If
            End If
            .AppendLine("</td>")
						.AppendLine("</tr>")
					Next
					.AppendLine("<tr>")
					.AppendLine("<td colspan=""3"" style=""vertical-align:middle;text-align:center"">")
          .AppendLine("<a href=""http://cloud.isgec.co.in/WebQcm1/QcmCallInspected.aspx?emp=" & EMPID & """ style=""color:Green; font-weight:bold;"">Send me latest Pending List</a>")
          .AppendLine("</td></tr>")
					.AppendLine("</table>")
				End If

			End With
			Return sb.ToString
		End Function
		Public Shared Function RequestClosed(ByVal ReqID As String) As String
			Dim mRet As String = ""
			Try
				Dim oReq As SIS.QCM.qcmRequests = SIS.QCM.qcmRequests.qcmRequestsGetByID(ReqID)
				If oReq.RequestStateID = "CLOSED" Then Return "Already Updated"
				Dim oIns As New SIS.QCM.qcmInspections
				With oIns
					.RequestID = oReq.RequestID
					.InspectionID = 0
					.ProjectID = oReq.ProjectID
					.OrderDate = oReq.OrderDate
					.OrderNo = oReq.OrderNo
					.SupplierID = oReq.SupplierID
					.RequestStateID = "CLOSED"
					.InspectionStatusID = 2
					.EnteredBy = HttpContext.Current.Session("LoginID")
					.EnteredOn = Now
          .InspectedBy = HttpContext.Current.Session("LoginID")
          .InspectedOn = Now
          .InspectionRemarks = "Request closed through E-Mail Link"
          .FileAttached = False
				End With
				SIS.QCM.qcmInspections.InsertData(oIns)
				With oReq
					.InspectionStatusID = 2
					.InspectionFinishDate = Now
					'.InspectionStartDate = Now
					.RequestStateID = "CLOSED"
				End With
				SIS.QCM.qcmRequests.UpdateData(oReq)
				Try
					SendClosedEMail(ReqID)
				Catch ex As Exception
				End Try
			Catch ex As Exception
				mRet = ex.Message
			End Try
			Return mRet
		End Function
		Public Shared Function RequestInspected(ByVal ReqID As String) As String
			Dim mRet As String = ""
			Try
				Dim oReq As SIS.QCM.qcmRequests = SIS.QCM.qcmRequests.qcmRequestsGetByID(ReqID)
				If oReq.RequestStateID = "INSPECTED" Then Return "Already Updated"
				Dim oIns As New SIS.QCM.qcmInspections
				With oIns
					.RequestID = oReq.RequestID
					.InspectionID = 0
					.ProjectID = oReq.ProjectID
					.OrderDate = oReq.OrderDate
					.OrderNo = oReq.OrderNo
					.SupplierID = oReq.SupplierID
					.RequestStateID = "INSPECTED"
					.InspectionStatusID = 10
					.EnteredBy = HttpContext.Current.Session("LoginID")
          .EnteredOn = Now
          .InspectedBy = HttpContext.Current.Session("LoginID")
          .InspectedOn = Now
          .InspectionRemarks = "Request updated through E-Mail Link"
					.FileAttached = False
				End With
				SIS.QCM.qcmInspections.InsertData(oIns)
				With oReq
					.InspectionStatusID = 10
					'.InspectionFinishDate = Now
					.InspectionStartDate = Now
					.RequestStateID = "INSPECTED"
				End With
				SIS.QCM.qcmRequests.UpdateData(oReq)
				Try
          'SendInspectedEMail(ReqID)
        Catch ex As Exception
				End Try
			Catch ex As Exception
				mRet = ex.Message
			End Try
			Return mRet
		End Function
		Public Shared Function SendPendingCallEMail(ByVal EmpID As String) As String
			Dim mRet As String = ""
			Dim ToEMailID As String = ""
			Dim FromEMailID As String = ""
			Dim oEmp As SIS.QCM.qcmEmployees = SIS.QCM.qcmEmployees.qcmEmployeesGetByID(EmpID)
			ToEMailID = oEmp.EMailID
			FromEMailID = oEmp.EMailID
			If ToEMailID <> String.Empty And FromEMailID <> String.Empty Then
				Try
					Dim oClient As SmtpClient = New SmtpClient()

					Dim oMsg As System.Net.Mail.MailMessage = New System.Net.Mail.MailMessage(FromEMailID, ToEMailID)
					With oMsg
						.IsBodyHtml = True
						.Subject = "Pending Inspection Call List"

						Dim sb As New StringBuilder
						With sb
							.Append(GetPendigRequestHTML(EmpID))
						End With
						Try
							Dim Header As String = ""
							Header = Header & "<html xmlns=""http://www.w3.org/1999/xhtml"">"
							Header = Header & "<head>"
							Header = Header & "<title></title>"
							Header = Header & "<style>"
							Header = Header & "table{"

							Header = Header & "border: solid 1pt black;"
							Header = Header & "border-collapse:collapse;"
							Header = Header & "font-family: Tahoma;}"

							Header = Header & "td{"
							Header = Header & "border: solid 1pt black;"
							Header = Header & "font-family: Tahoma;"
							Header = Header & "font-size: 12px;"
							Header = Header & "vertical-align:top;}"

							Header = Header & "</style>"
							Header = Header & "</head>"
							Header = Header & "<body>"
							Header = Header & sb.ToString
							Header = Header & "</body></html>"
							oMsg.Body = Header
						Catch ex As Exception
							mRet = ex.Message
						End Try
					End With
					oClient.Send(oMsg)
				Catch ex As Exception
					mRet = ex.Message
				End Try
			End If
			Return mRet
		End Function
		Public Shared Function SendClosedEMail(ByVal RecordID As Integer) As String
			Dim mRet As String = ""
			Dim ToEMailID As String = ""
			Dim FromEMailID As String = ""
			Dim Record As SIS.QCM.qcmRequestAllotment = SIS.QCM.qcmRequestAllotment.qcmRequestAllotmentGetByID(RecordID)
			ToEMailID = Record.FK_QCM_Requests_Createdby.EMailID
			FromEMailID = Record.FK_QCM_Requests_AllotedTo.EMailID
			If ToEMailID <> String.Empty And FromEMailID <> String.Empty Then
				Try
					Dim oClient As SmtpClient = New SmtpClient()

					Dim oMsg As System.Net.Mail.MailMessage = New System.Net.Mail.MailMessage(FromEMailID, ToEMailID)
					With oMsg
						Try
              '.CC.Add(Record.FK_QCM_Requests_AllotedBy.EMailID)
            Catch ex As Exception
						End Try
						Try
              '.CC.Add(FromEMailID)
            Catch ex As Exception
						End Try
						.IsBodyHtml = True
            .Subject = "Inspection Request " & Record.RequestID & " Completed at Vendor: " & Record.IDM_Vendors7_Description

            Dim sb As New StringBuilder
						With sb
							.AppendLine("<table style=""width:900px"" border=""1"" cellspacing=""1"" cellpadding=""1"">")
              .AppendLine("<tr><td style=""color:green"" colspan=""2"" align=""center""><h3><b>INSPECTION CALL COMPLETED : " & Record.RequestID & "</b></h3></td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Alloted To</b></td>")
							.AppendLine("<td>" & Record.HRM_Employees2_EmployeeName & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Project</b></td>")
							.AppendLine("<td>" & Record.IDM_Projects6_Description & " [" & Record.ProjectID & "]" & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Purchase Order</b></td>")
							.AppendLine("<td>" & Record.OrderNo & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Supplier</b></td>")
							.AppendLine("<td>" & Record.IDM_Vendors7_Description & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Address</b></td>")
							.AppendLine("<td>" & Record.CreationRemarks & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Stage</b></td>")
              .AppendLine("<td>" & Record.QCM_InspectionStages8_Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Stage Quantity</b></td>")
              .AppendLine("<td>" & Record.TotalRequestedQuantity & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Final Quantity</b></td>")
              .AppendLine("<td>" & Record.LotSize & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>UOM</b></td>")
              .AppendLine("<td>" & Record.UOM & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Start Date</b></td>")
              .AppendLine("<td>" & Record.AllotedStartDate & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Finish Date</b></td>")
							.AppendLine("<td>" & Record.AllotedFinishDate & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Description</b></td>")
							.AppendLine("<td>" & Record.Description & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Remarks</b></td>")
							.AppendLine("<td>" & Record.AllotmentRemarks & "</td></tr>")
							.AppendLine("</table>")
						End With
						Try
							Dim Header As String = ""
							Header = Header & "<html xmlns=""http://www.w3.org/1999/xhtml"">"
							Header = Header & "<head>"
							Header = Header & "<title></title>"
							Header = Header & "<style>"
							Header = Header & "table{"

							Header = Header & "border: solid 1pt black;"
							Header = Header & "border-collapse:collapse;"
							Header = Header & "font-family: Tahoma;}"

							Header = Header & "td{"
							Header = Header & "border: solid 1pt black;"
							Header = Header & "font-family: Tahoma;"
							Header = Header & "font-size: 12px;"
							Header = Header & "vertical-align:top;}"

							Header = Header & "</style>"
							Header = Header & "</head>"
							Header = Header & "<body>"
							Header = Header & sb.ToString
							Header = Header & "</body></html>"
							oMsg.Body = Header
						Catch ex As Exception
							mRet = ex.Message
						End Try
					End With
					oClient.Send(oMsg)
				Catch ex As Exception
					mRet = ex.Message
				End Try
			End If
			Return mRet
		End Function
		Public Shared Function SendInspectedEMail(ByVal RecordID As Integer) As String
			Dim mRet As String = ""
			Dim ToEMailID As String = ""
			Dim FromEMailID As String = ""
			Dim Record As SIS.QCM.qcmRequestAllotment = SIS.QCM.qcmRequestAllotment.qcmRequestAllotmentGetByID(RecordID)
			ToEMailID = Record.FK_QCM_Requests_Createdby.EMailID
			FromEMailID = Record.FK_QCM_Requests_AllotedTo.EMailID
			If ToEMailID <> String.Empty And FromEMailID <> String.Empty Then
				Try
					Dim oClient As SmtpClient = New SmtpClient()

					Dim oMsg As System.Net.Mail.MailMessage = New System.Net.Mail.MailMessage(FromEMailID, ToEMailID)
					With oMsg
						Try
							.CC.Add(Record.FK_QCM_Requests_AllotedBy.EMailID)
						Catch ex As Exception
						End Try
						Try
							.CC.Add(FromEMailID)
						Catch ex As Exception
						End Try
						.IsBodyHtml = True
            .Subject = "Inspection Request " & Record.RequestID & " Started at Vendor: " & Record.IDM_Vendors7_Description

            Dim sb As New StringBuilder
						With sb
							.AppendLine("<table style=""width:900px"" border=""1"" cellspacing=""1"" cellpadding=""1"">")
              .AppendLine("<tr><td style=""color:green"" colspan=""2"" align=""center""><h3><b>INSPECTION CALL STARTED : " & Record.RequestID & "</b></h3></td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Alloted To</b></td>")
							.AppendLine("<td>" & Record.HRM_Employees2_EmployeeName & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Project</b></td>")
							.AppendLine("<td>" & Record.IDM_Projects6_Description & " [" & Record.ProjectID & "]" & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Purchase Order</b></td>")
							.AppendLine("<td>" & Record.OrderNo & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Supplier</b></td>")
							.AppendLine("<td>" & Record.IDM_Vendors7_Description & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Address</b></td>")
							.AppendLine("<td>" & Record.CreationRemarks & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Stage</b></td>")
              .AppendLine("<td>" & Record.QCM_InspectionStages8_Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Stage Quantity</b></td>")
              .AppendLine("<td>" & Record.TotalRequestedQuantity & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Final Quantity</b></td>")
              .AppendLine("<td>" & Record.LotSize & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>UOM</b></td>")
              .AppendLine("<td>" & Record.UOM & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Start Date</b></td>")
              .AppendLine("<td>" & Record.AllotedStartDate & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Finish Date</b></td>")
							.AppendLine("<td>" & Record.AllotedFinishDate & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Description</b></td>")
							.AppendLine("<td>" & Record.Description & "</td></tr>")
							.AppendLine("<tr><td bgcolor=""lightgray""><b>Remarks</b></td>")
							.AppendLine("<td>" & Record.AllotmentRemarks & "</td></tr>")
							.AppendLine("</table>")
						End With
						Try
							Dim Header As String = ""
							Header = Header & "<html xmlns=""http://www.w3.org/1999/xhtml"">"
							Header = Header & "<head>"
							Header = Header & "<title></title>"
							Header = Header & "<style>"
							Header = Header & "table{"

							Header = Header & "border: solid 1pt black;"
							Header = Header & "border-collapse:collapse;"
							Header = Header & "font-family: Tahoma;}"

							Header = Header & "td{"
							Header = Header & "border: solid 1pt black;"
							Header = Header & "font-family: Tahoma;"
							Header = Header & "font-size: 12px;"
							Header = Header & "vertical-align:top;}"

							Header = Header & "</style>"
							Header = Header & "</head>"
							Header = Header & "<body>"
							Header = Header & sb.ToString
							Header = Header & "</body></html>"
							oMsg.Body = Header
						Catch ex As Exception
							mRet = ex.Message
						End Try
					End With
					oClient.Send(oMsg)
				Catch ex As Exception
					mRet = ex.Message
				End Try
			End If
			Return mRet
		End Function
    Public Shared Function SendSMSandEMail(ByVal Record As SIS.QCM.qcmRequestAllotment) As String
      Dim mRet As String = ""
      Dim ToEMailID As String = ""
      Dim FromEMailID As String = ""
      ToEMailID = Record.FK_QCM_Requests_AllotedTo.EMailID
      FromEMailID = SIS.QCM.qcmEmployees.qcmEmployeesGetByID(HttpContext.Current.Session("LoginID")).EMailID
      If ToEMailID <> String.Empty And FromEMailID <> String.Empty Then
        Try
          Dim oClient As SmtpClient = New SmtpClient()

          Dim oMsg As System.Net.Mail.MailMessage = New System.Net.Mail.MailMessage(FromEMailID, ToEMailID)
          With oMsg
            .IsBodyHtml = True
            .Subject = "Inspection Request " & Record.RequestID & " Dt: " & Record.AllotedStartDate & " Vendor: " & Record.IDM_Vendors7_Description
            Dim sb As New StringBuilder
            With sb
              .AppendLine("<table style=""width:900px"" border=""1"" cellspacing=""1"" cellpadding=""1"">")
              .AppendLine("<tr><td colspan=""2"" align=""center""><h3><b>INSPECTION CALL REQUEST ID: " & Record.RequestID & "</b></h3></td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Alloted To</b></td>")
              .AppendLine("<td>" & Record.FK_QCM_Requests_AllotedTo.EmployeeName & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Project</b></td>")
              .AppendLine("<td>" & Record.IDM_Projects6_Description & " [" & Record.ProjectID & "]" & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Purchase Order</b></td>")
              .AppendLine("<td>" & Record.OrderNo & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Supplier</b></td>")
              .AppendLine("<td>" & Record.IDM_Vendors7_Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Address</b></td>")
              .AppendLine("<td>" & Record.CreationRemarks & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Stage</b></td>")
              .AppendLine("<td>" & Record.QCM_InspectionStages8_Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Stage Quantity</b></td>")
              .AppendLine("<td>" & Record.TotalRequestedQuantity & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Final Quantity</b></td>")
              .AppendLine("<td>" & Record.LotSize & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>UOM</b></td>")
              .AppendLine("<td>" & Record.UOM & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Start Date</b></td>")
              .AppendLine("<td>" & Record.AllotedStartDate & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Finish Date</b></td>")
              .AppendLine("<td>" & Record.AllotedFinishDate & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Description</b></td>")
              .AppendLine("<td>" & Record.Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Remarks</b></td>")
              .AppendLine("<td>" & Record.AllotmentRemarks & "</td></tr>")
              .AppendLine("</table>")
              .Append(GetPendigRequestHTML(Record.AllotedTo))
            End With
            Try
              Dim Header As String = ""
              Header = Header & "<html xmlns=""http://www.w3.org/1999/xhtml"">"
              Header = Header & "<head>"
              Header = Header & "<title></title>"
              Header = Header & "<style>"
              Header = Header & "table{"

              Header = Header & "border: solid 1pt black;"
              Header = Header & "border-collapse:collapse;"
              Header = Header & "font-family: Tahoma;}"

              Header = Header & "td{"
              Header = Header & "border: solid 1pt black;"
              Header = Header & "font-family: Tahoma;"
              Header = Header & "font-size: 12px;"
              Header = Header & "vertical-align:top;}"

              Header = Header & "</style>"
              Header = Header & "</head>"
              Header = Header & "<body>"
              Header = Header & sb.ToString
              Header = Header & "</body></html>"
              oMsg.Body = Header
              Try
                Dim oAtchs As List(Of SIS.QCM.qcmRequestFiles) = SIS.QCM.qcmRequestFiles.qcmRequestFilesSelectList(0, 99, "", False, "", Record.RequestID)
                For Each atch As SIS.QCM.qcmRequestFiles In oAtchs
                  Dim oAt As New System.Net.Mail.Attachment(ConfigurationManager.AppSettings("RequestDir") & "/" & atch.DiskFIleName)
                  oAt.Name = atch.FileName
                  .Attachments.Add(oAt)
                Next
              Catch ex As Exception
              End Try
            Catch ex As Exception
              mRet = ex.Message
            End Try
          End With
          oClient.Send(oMsg)
        Catch ex As Exception
          mRet = ex.Message
        End Try
      End If
      SendNotificationEMail(Record)
      Return mRet
    End Function
    Public Shared Function SendNotificationEMail(ByVal Record As SIS.QCM.qcmRequestAllotment) As String
      Dim mRet As String = ""
      Dim ToEMailID As String = ""
      Dim FromEMailID As String = ""
      ToEMailID = Record.FK_QCM_Requests_Createdby.EMailID
      FromEMailID = SIS.QCM.qcmEmployees.qcmEmployeesGetByID(HttpContext.Current.Session("LoginID")).EMailID
      If ToEMailID <> String.Empty And FromEMailID <> String.Empty Then
        Try
          Dim oClient As SmtpClient = New SmtpClient()
          Dim oMsg As System.Net.Mail.MailMessage = New System.Net.Mail.MailMessage(FromEMailID, ToEMailID)
          With oMsg
            Try
              '.CC.Add(FromEMailID)
            Catch ex As Exception
            End Try
            .IsBodyHtml = True
            .Subject = "Inspection Request " & Record.RequestID & " Alloted for Dt: " & Record.AllotedStartDate & " Vendor: " & Record.IDM_Vendors7_Description
            Dim sb As New StringBuilder
            With sb
              .AppendLine("<table style=""width:900px"" border=""1"" cellspacing=""1"" cellpadding=""1"">")
              .AppendLine("<tr><td colspan=""2"" align=""center""><h3><b>INSPECTION CALL REQUEST ID: " & Record.RequestID & "</b></h3></td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Alloted To</b></td>")
              .AppendLine("<td>" & Record.FK_QCM_Requests_AllotedTo.EmployeeName & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Project</b></td>")
              .AppendLine("<td>" & Record.IDM_Projects6_Description & " [" & Record.ProjectID & "]" & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Purchase Order</b></td>")
              .AppendLine("<td>" & Record.OrderNo & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Supplier</b></td>")
              .AppendLine("<td>" & Record.IDM_Vendors7_Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Address</b></td>")
              .AppendLine("<td>" & Record.CreationRemarks & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Stage</b></td>")
              .AppendLine("<td>" & Record.QCM_InspectionStages8_Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Stage Quantity</b></td>")
              .AppendLine("<td>" & Record.TotalRequestedQuantity & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Final Quantity</b></td>")
              .AppendLine("<td>" & Record.LotSize & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>UOM</b></td>")
              .AppendLine("<td>" & Record.UOM & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Start Date</b></td>")
              .AppendLine("<td>" & Record.AllotedStartDate & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Finish Date</b></td>")
              .AppendLine("<td>" & Record.AllotedFinishDate & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Description</b></td>")
              .AppendLine("<td>" & Record.Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Remarks</b></td>")
              .AppendLine("<td>" & Record.AllotmentRemarks & "</td></tr>")
              .AppendLine("</table>")
            End With
            Try
              Dim Header As String = ""
              Header = Header & "<html xmlns=""http://www.w3.org/1999/xhtml"">"
              Header = Header & "<head>"
              Header = Header & "<title></title>"
              Header = Header & "<style>"
              Header = Header & "table{"

              Header = Header & "border: solid 1pt black;"
              Header = Header & "border-collapse:collapse;"
              Header = Header & "font-family: Tahoma;}"

              Header = Header & "td{"
              Header = Header & "border: solid 1pt black;"
              Header = Header & "font-family: Tahoma;"
              Header = Header & "font-size: 12px;"
              Header = Header & "vertical-align:top;}"

              Header = Header & "</style>"
              Header = Header & "</head>"
              Header = Header & "<body>"
              Header = Header & sb.ToString
              Header = Header & "</body></html>"
              oMsg.Body = Header
              Try
                Dim oAtchs As List(Of SIS.QCM.qcmRequestFiles) = SIS.QCM.qcmRequestFiles.qcmRequestFilesSelectList(0, 99, "", False, "", Record.RequestID)
                For Each atch As SIS.QCM.qcmRequestFiles In oAtchs
                  Dim oAt As New System.Net.Mail.Attachment(ConfigurationManager.AppSettings("RequestDir") & "/" & atch.DiskFIleName)
                  oAt.Name = atch.FileName
                  .Attachments.Add(oAt)
                Next
              Catch ex As Exception
              End Try
            Catch ex As Exception
              mRet = ex.Message
            End Try
          End With
          oClient.Send(oMsg)
        Catch ex As Exception
          mRet = ex.Message
        End Try
      End If
      Return mRet
    End Function
    Public Shared Function SendCancellationEMail(ByVal Record As SIS.QCM.qcmRequestAllotment) As String
      Dim mRet As String = ""
      Dim ToEMailID As String = ""
      Dim FromEMailID As String = ""
      ToEMailID = Record.FK_QCM_Requests_AllotedTo.EMailID
      FromEMailID = SIS.QCM.qcmEmployees.qcmEmployeesGetByID(HttpContext.Current.Session("LoginID")).EMailID
      If ToEMailID <> String.Empty And FromEMailID <> String.Empty Then
        Try
          Dim oClient As SmtpClient = New SmtpClient()
          Dim oMsg As System.Net.Mail.MailMessage = New System.Net.Mail.MailMessage(FromEMailID, ToEMailID)
          With oMsg
            Try
              .CC.Add(Record.FK_QCM_Requests_Createdby.EMailID)
            Catch ex As Exception
            End Try
            Try
              .CC.Add(FromEMailID)
            Catch ex As Exception
            End Try
            .IsBodyHtml = True
            .Subject = "Inspection Request " & Record.RequestID & " Cancelled for Dt: " & Record.AllotedStartDate & " Vendor: " & Record.IDM_Vendors7_Description
            Dim sb As New StringBuilder
            With sb
              .AppendLine("<table style=""width:900px"" border=""1"" cellspacing=""1"" cellpadding=""1"">")
              .AppendLine("<tr><td colspan=""2"" style=""color:red"" align=""center""><h3><b>INSPECTION CALL CANCELLED FOR REQUEST ID: " & Record.RequestID & "</b></h3></td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Alloted To</b></td>")
              .AppendLine("<td>" & Record.HRM_Employees2_EmployeeName & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Project</b></td>")
              .AppendLine("<td>" & Record.IDM_Projects6_Description & " [" & Record.ProjectID & "]" & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Purchase Order</b></td>")
              .AppendLine("<td>" & Record.OrderNo & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Supplier</b></td>")
              .AppendLine("<td>" & Record.IDM_Vendors7_Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Address</b></td>")
              .AppendLine("<td>" & Record.CreationRemarks & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Stage</b></td>")
              .AppendLine("<td>" & Record.QCM_InspectionStages8_Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Stage Quantity</b></td>")
              .AppendLine("<td>" & Record.TotalRequestedQuantity & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Final Quantity</b></td>")
              .AppendLine("<td>" & Record.LotSize & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>UOM</b></td>")
              .AppendLine("<td>" & Record.UOM & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Start Date</b></td>")
              .AppendLine("<td>" & Record.AllotedStartDate & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Finish Date</b></td>")
              .AppendLine("<td>" & Record.AllotedFinishDate & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Description</b></td>")
              .AppendLine("<td>" & Record.Description & "</td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Remarks</b></td>")
              .AppendLine("<td>" & Record.AllotmentRemarks & "</td></tr>")
              .AppendLine("</table>")
            End With
            Try
              Dim Header As String = ""
              Header = Header & "<html xmlns=""http://www.w3.org/1999/xhtml"">"
              Header = Header & "<head>"
              Header = Header & "<title></title>"
              Header = Header & "<style>"
              Header = Header & "table{"

              Header = Header & "border: solid 1pt black;"
              Header = Header & "border-collapse:collapse;"
              Header = Header & "font-family: Tahoma;}"

              Header = Header & "td{"
              Header = Header & "border: solid 1pt black;"
              Header = Header & "font-family: Tahoma;"
              Header = Header & "font-size: 12px;"
              Header = Header & "vertical-align:top;}"

              Header = Header & "</style>"
              Header = Header & "</head>"
              Header = Header & "<body>"
              Header = Header & sb.ToString
              Header = Header & "</body></html>"
              oMsg.Body = Header
              Try
                'Dim oAtchs As List(Of SIS.QCM.qcmRequestFiles) = SIS.QCM.qcmRequestFiles.qcmRequestFilesSelectList(0, 99, "", False, "", Record.RequestID)
                'For Each atch As SIS.QCM.qcmRequestFiles In oAtchs
                '  Dim oAt As New System.Net.Mail.Attachment(ConfigurationManager.AppSettings("RequestDir") & "/" & atch.DiskFIleName)
                '  oAt.Name = atch.FileName
                '  .Attachments.Add(oAt)
                'Next
              Catch ex As Exception
              End Try
            Catch ex As Exception
              mRet = ex.Message
            End Try
          End With
          oClient.Send(oMsg)
        Catch ex As Exception
          mRet = ex.Message
        End Try
      End If
      Return mRet
    End Function

    Public Shared Function GetSMSData(ByVal RequestID As Integer) As String
			Dim Record As SIS.QCM.qcmRequestAllotment = SIS.QCM.qcmRequestAllotment.qcmRequestAllotmentGetByID(RequestID)
			Dim mRet As String = ""
			mRet = Record.FK_QCM_Requests_AllotedTo.EmployeeName
			mRet = mRet & ", You are requested to attend the inspection call. Project: " & Record.IDM_Projects6_Description
			mRet = mRet & " Supplier: " & Record.IDM_Vendors7_Description
			mRet = mRet & " Place: " & Record.CreationRemarks
			mRet = mRet & " QTY.: " & Record.TotalRequestedQuantity
			mRet = mRet & IIf(Record.AllotedStartDate = Record.AllotedFinishDate, " On Date: " & Record.AllotedStartDate, "From Dt.: " & Record.AllotedStartDate & " To: " & Record.AllotedFinishDate)
			mRet = mRet & " Details: " & Record.Description
			mRet = "http://192.9.200.172:8080/xampp/web2sms.php?toaddress=" & "|" & Record.FK_QCM_Requests_AllotedTo.ContactNumbers & "|" & SIS.QCM.qcmEmployees.qcmEmployeesGetByID(Record.CreatedBy).ContactNumbers & "|" & mRet
			Return mRet
		End Function
	End Class
End Namespace
