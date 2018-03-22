Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.ComponentModel
Imports System.Net.Mail
Imports System.Web.Mail

Namespace SIS.QCM
  Partial Public Class qcmRequests
    Public ReadOnly Property NewTotalQuantity As String
      Get
        Dim mRet As String = "0.00"
        If IsNumeric(TotalRequestedQuantity) Then
          If IsNumeric(LotSize) Then
            mRet = Convert.ToDecimal(TotalRequestedQuantity) + Convert.ToDecimal(LotSize)
          Else
            mRet = Convert.ToDecimal(TotalRequestedQuantity)
          End If
        Else
          mRet = TotalRequestedQuantity
        End If
        Return mRet
      End Get
    End Property
    Public Function GetColor() As System.Drawing.Color
      Dim mRet As System.Drawing.Color = Drawing.Color.Blue
      Select Case _RequestStateID
        Case "OPEN"
          mRet = Drawing.Color.Blue
        Case "UNDERALLOT"
          mRet = Drawing.Color.Red
        Case "ALLOTED", "REALLOTED"
          mRet = Drawing.Color.Green
        Case "INSPECTED"
          mRet = Drawing.Color.BlueViolet
        Case "CLOSED"
          mRet = Drawing.Color.Orange
        Case "CANCELLED"
          mRet = Drawing.Color.Brown
        Case "RETURNED"
          mRet = Drawing.Color.Black
      End Select
      Return mRet
    End Function
    Public ReadOnly Property AllotmentDetails As String
      Get
        Dim mRet As String = ""
        Select Case _RequestStateID
          Case "ALLOTED", "REALLOTED", "INSPECTED", "CLOSED"
            mRet = "<b>Alloted To: </b>" & Me.HRM_Employees2_EmployeeName & ", <b>From : </b>" & Me.AllotedStartDate & " <b>To : </b>" & Me.AllotedFinishDate
        End Select
        Return mRet
      End Get
    End Property
    Public ReadOnly Property ReturnDetails As String
      Get
        Dim mRet As String = ""
        Select Case _RequestStateID
          Case "RETURNED"
            mRet = "<img alt='warning' src='../../images/Error.gif' style='height:14px; width:14px' /><b>" & Me.ReturnRemarks & "</b>"
        End Select
        Return mRet
      End Get
    End Property
    Public Function GetVisible() As Boolean
      Dim mRet As Boolean = True
      Return mRet
    End Function
    Public Function GetEnable() As Boolean
      Dim mRet As Boolean = True
      Return mRet
    End Function
    Public Shared Function UZ_qcmRequestsInsert(ByVal Record As SIS.QCM.qcmRequests) As SIS.QCM.qcmRequests
      Dim _Result As SIS.QCM.qcmRequests = qcmRequestsInsert(Record)
      Return _Result
    End Function
    Public Shared Function UZ_qcmRequestsUpdate(ByVal Record As SIS.QCM.qcmRequests) As SIS.QCM.qcmRequests
      Dim _Result As SIS.QCM.qcmRequests = qcmRequestsUpdate(Record)
      Return _Result
    End Function
    Public Shared Function UZ_qcmRequestsDelete(ByVal Record As SIS.QCM.qcmRequests) As Integer
      Dim _Result As Integer = qcmRequests.qcmRequestsDelete(Record)
      Return _Result
    End Function
    Public ReadOnly Property ForwardVisible() As Boolean
      Get
        Select Case RequestStateID
          Case "OPEN", "RETURNED"
            Return True
        End Select
        Return False
      End Get
    End Property
    Public Shared Function ForwardForAllotment(ByVal RequestID As Integer) As Boolean
      Dim oReq As SIS.QCM.qcmRequests = SIS.QCM.qcmRequests.qcmRequestsGetByID(RequestID)
      With oReq
        If Convert.ToDateTime(oReq.RequestedInspectionStartDate).Date <= Now.Date Then
          Throw New Exception("Can NOT forward Inspenction request for today or earlier date. Pl. change requested date, then forward")
        ElseIf Now.Hour > 11 Then
          If Convert.ToDateTime(oReq.RequestedInspectionStartDate).Date <= Now.AddDays(1).Date Then
            Throw New Exception("After 11 AM, you can raise Inspection request for day after tomorrow only. Pl. change requested date, then forward.")
          End If
        End If
        .ReturnRemarks = ""
        .CreatedOn = Now
        .RequestStateID = "UNDERALLOT"
      End With
      Try
        SIS.QCM.qcmRequests.UpdateData(oReq)
      Catch ex As Exception
        Return False
      End Try
      Return True
    End Function
    Public Shared Function ReturnToRequester(ByVal RequestID As Integer, ByVal Remarks As String) As Boolean
      Try
        Dim oReq As SIS.QCM.qcmRequests = SIS.QCM.qcmRequests.qcmRequestsGetByID(RequestID)
        With oReq
          .ReturnRemarks = Remarks
          .AllotedOn = Now
          .AllotedBy = HttpContext.Current.Session("LoginID")
          .RequestStateID = "RETURNED"
        End With
        oReq = SIS.QCM.qcmRequests.UpdateData(oReq)
        SendReturnEMail(oReq)
      Catch ex As Exception
        Return False
      End Try
      Return True
    End Function
    Public Shared Function SendReturnEMail(ByVal Record As SIS.QCM.qcmRequests) As String
      Dim mRet As String = ""
      Dim ToEMailID As String = ""
      Dim FromEMailID As String = ""
      ToEMailID = Record.FK_QCM_Requests_Createdby.EMailID
      FromEMailID = Record.FK_QCM_Requests_AllotedBy.EMailID
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
            .Subject = "Inspection Request " & Record.RequestID & " Returned at Vendor: " & Record.IDM_Vendors7_Description

            Dim sb As New StringBuilder
            With sb
              .AppendLine("<table style=""width:900px"" border=""1"" cellspacing=""1"" cellpadding=""1"">")
              .AppendLine("<tr><td style=""color:red"" colspan=""2"" align=""center""><h3><b>INSPECTION CALL RETURNED : " & Record.RequestID & "</b></h3></td></tr>")
              .AppendLine("<tr><td bgcolor=""lightgray""><b>Reason</b></td>")
              .AppendLine("<td>" & Record.ReturnRemarks & "</td></tr>")
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

    Public Shared Function ReturnToAllotment(ByVal RequestID As Integer) As Boolean
      Try
        Dim oReq As SIS.QCM.qcmRequests = SIS.QCM.qcmRequests.qcmRequestsGetByID(RequestID)
        With oReq
          .AllotedOn = Now
          .AllotedBy = HttpContext.Current.Session("LoginID")
          .RequestStateID = "UNDERALLOT"
        End With
        SIS.QCM.qcmRequests.UpdateData(oReq)
      Catch ex As Exception
        Return False
      End Try
      Return True
    End Function
    Public Shared Function UpdateInspected(ByVal oIns As SIS.QCM.qcmInspections) As Boolean
      Try
        Dim oReq As SIS.QCM.qcmRequests = SIS.QCM.qcmRequests.qcmRequestsGetByID(oIns.RequestID)
        If oReq.InspectionStartDate = String.Empty Then
          oReq.InspectionStartDate = oIns.InspectedOn
          oReq.RequestStateID = "INSPECTED"
          SIS.QCM.qcmRequests.UpdateData(oReq)
        ElseIf Convert.ToDateTime(oReq.InspectionStartDate) > Convert.ToDateTime(oIns.InspectedOn) Then
          oReq.InspectionStartDate = oIns.InspectedOn
          SIS.QCM.qcmRequests.UpdateData(oReq)
        End If
      Catch ex As Exception
        Return False
      End Try
      Return True
    End Function
    Public Shared Function CloseInspectionRequest(ByVal RequestID As Integer, ByVal InspectionStatusID As Integer) As Boolean
      Try
        Dim oReq As SIS.QCM.qcmRequests = SIS.QCM.qcmRequests.qcmRequestsGetByID(RequestID)
        Dim oIns As New SIS.QCM.qcmInspections
        With oIns
          .RequestID = oReq.RequestID
          .InspectionID = 0
          .ProjectID = oReq.ProjectID
          .OrderDate = oReq.OrderDate
          .OrderNo = oReq.OrderNo
          .SupplierID = oReq.SupplierID
          .RequestStateID = "CLOSED"
          .InspectionStatusID = InspectionStatusID
          .EnteredBy = HttpContext.Current.Session("LoginID")
          .EnteredOn = Now
          .InspectionRemarks = "Closed at HO"
          .FileAttached = False
        End With
        SIS.QCM.qcmInspections.InsertData(oIns)
        With oReq
          .InspectionStatusID = InspectionStatusID
          .InspectionFinishDate = Now
          .RequestStateID = "CLOSED"
        End With
        SIS.QCM.qcmRequests.UpdateData(oReq)
      Catch ex As Exception
        Return False
      End Try
      Return True
    End Function
    Public Shared Function GetAllotedToByCreatedByForMonthYear(ByVal ForDate As DateTime) As List(Of SIS.QCM.qcmRequests)
      Dim Results As List(Of SIS.QCM.qcmRequests) = Nothing
      Using Con As SqlConnection = New SqlConnection(SIS.SYS.SQLDatabase.DBCommon.GetConnectionString())
        Using Cmd As SqlCommand = Con.CreateCommand()
          Dim ForMonth As Integer = ForDate.Month
          Dim ForYear As Integer = ForDate.Year
          Cmd.CommandType = CommandType.StoredProcedure
          Cmd.CommandText = "spqcm_LG_RequestsSelectByCreatedByMonthYear"
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@LoginID", SqlDbType.NVarChar, 9, HttpContext.Current.Session("LoginID"))
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@ForMonth", SqlDbType.Int, 10, ForMonth)
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@ForYear", SqlDbType.Int, 10, ForYear)
          Cmd.Parameters.Add("@RecordCount", SqlDbType.Int)
          Cmd.Parameters("@RecordCount").Direction = ParameterDirection.Output
          _RecordCount = -1
          Results = New List(Of SIS.QCM.qcmRequests)()
          Con.Open()
          Dim Reader As SqlDataReader = Cmd.ExecuteReader()
          While (Reader.Read())
            Results.Add(New SIS.QCM.qcmRequests(Reader))
          End While
          Reader.Close()
          _RecordCount = Cmd.Parameters("@RecordCount").Value
        End Using
      End Using
      Return Results
    End Function
    Public Shared Function GetAllotedToByAllotedToForMonthYear(ByVal AllotedTo As String, ByVal ForDate As DateTime) As List(Of SIS.QCM.qcmRequests)
      Dim Results As List(Of SIS.QCM.qcmRequests) = Nothing
      Using Con As SqlConnection = New SqlConnection(SIS.SYS.SQLDatabase.DBCommon.GetConnectionString())
        Using Cmd As SqlCommand = Con.CreateCommand()
          Dim ForMonth As Integer = ForDate.Month
          Dim ForYear As Integer = ForDate.Year
          Cmd.CommandType = CommandType.StoredProcedure
          Cmd.CommandText = "spqcm_LG_RequestsSelectByAllotedToMonthYear"
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@LoginID", SqlDbType.NVarChar, 9, HttpContext.Current.Session("LoginID"))
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@AllotedTo", SqlDbType.NVarChar, 9, AllotedTo)
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@ForMonth", SqlDbType.Int, 10, ForMonth)
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@ForYear", SqlDbType.Int, 10, ForYear)
          Cmd.Parameters.Add("@RecordCount", SqlDbType.Int)
          Cmd.Parameters("@RecordCount").Direction = ParameterDirection.Output
          _RecordCount = -1
          Results = New List(Of SIS.QCM.qcmRequests)()
          Con.Open()
          Dim Reader As SqlDataReader = Cmd.ExecuteReader()
          While (Reader.Read())
            Results.Add(New SIS.QCM.qcmRequests(Reader))
          End While
          Reader.Close()
          _RecordCount = Cmd.Parameters("@RecordCount").Value
        End Using
      End Using
      Return Results
    End Function
    Public Shared Function GetByCreatedOn(ByVal ForDate As DateTime, ByVal OrderBy As String) As List(Of SIS.QCM.qcmRequests)
      Dim Results As List(Of SIS.QCM.qcmRequests) = Nothing
      Using Con As SqlConnection = New SqlConnection(SIS.SYS.SQLDatabase.DBCommon.GetConnectionString())
        Using Cmd As SqlCommand = Con.CreateCommand()
          If OrderBy = String.Empty Then OrderBy = "RequestID DESC"
          Cmd.CommandType = CommandType.StoredProcedure
          Cmd.CommandText = "spqcm_LG_RequestsSelectByCreationDate"
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@ForDate", SqlDbType.DateTime, 21, ForDate)
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@LoginID", SqlDbType.NVarChar, 9, HttpContext.Current.Session("LoginID"))
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@OrderBy", SqlDbType.NVarChar, 50, OrderBy)
          Cmd.Parameters.Add("@RecordCount", SqlDbType.Int)
          Cmd.Parameters("@RecordCount").Direction = ParameterDirection.Output
          _RecordCount = -1
          Results = New List(Of SIS.QCM.qcmRequests)()
          Con.Open()
          Dim Reader As SqlDataReader = Cmd.ExecuteReader()
          While (Reader.Read())
            Results.Add(New SIS.QCM.qcmRequests(Reader))
          End While
          Reader.Close()
          _RecordCount = Cmd.Parameters("@RecordCount").Value
        End Using
      End Using
      Return Results
    End Function
    Public Shared Function GetRequestsGetByIDinXML(ByVal RequestID As Int32) As String
      Dim Results As String = ""
      Using Con As SqlConnection = New SqlConnection(SIS.SYS.SQLDatabase.DBCommon.GetConnectionString())
        Using Cmd As SqlCommand = Con.CreateCommand()
          Cmd.CommandType = CommandType.StoredProcedure
          Cmd.CommandText = "spqcm_LG_RequestsSelectByIDinXML"
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@RequestID", SqlDbType.Int, RequestID.ToString.Length, RequestID)
          SIS.SYS.SQLDatabase.DBCommon.AddDBParameter(Cmd, "@LoginID", SqlDbType.NVarChar, 9, HttpContext.Current.Session("LoginID"))
          Con.Open()
          'Dim strXML As String = Cmd.ExecuteScalar()
          'If strXML IsNot Nothing Then
          '	Results = strXML
          'End If
          Dim rXML As Xml.XmlReader = Cmd.ExecuteXmlReader
          rXML.Read()
          Do While rXML.ReadState <> Xml.ReadState.EndOfFile
            Results &= rXML.ReadOuterXml()
          Loop
          rXML.Close()
        End Using
      End Using
      Return Results
    End Function
    Public Shared Function CloseInspectionRequestAuto(ByVal RequestID As Integer, ByVal InspectionStatusID As Integer) As Boolean
      Try
        Dim oReq As SIS.QCM.qcmRequests = SIS.QCM.qcmRequests.qcmRequestsGetByID(RequestID)
        Dim oIns As New SIS.QCM.qcmInspections
        With oIns
          .RequestID = oReq.RequestID
          .InspectionID = 0
          .ProjectID = oReq.ProjectID
          .OrderDate = oReq.OrderDate
          .OrderNo = oReq.OrderNo
          .SupplierID = oReq.SupplierID
          .RequestStateID = "CLOSED"
          .InspectionStatusID = InspectionStatusID
          .EnteredBy = HttpContext.Current.Session("LoginID")
          .EnteredOn = Now
          .InspectionRemarks = "Closed By Inspector"
          .FileAttached = False
        End With
        SIS.QCM.qcmInspections.InsertData(oIns)
        With oReq
          .InspectionStatusID = InspectionStatusID
          .InspectionFinishDate = Now
          .RequestStateID = "CLOSED"
        End With
        SIS.QCM.qcmRequests.UpdateData(oReq)
      Catch ex As Exception
        Return False
      End Try
      Return True
    End Function

  End Class
End Namespace
