
Partial Class QcmCallInspected
  Inherits System.Web.UI.Page

  Private Sub QcmCallInspected_Load(sender As Object, e As EventArgs) Handles Me.Load
    Dim typ As String = ""
    If Request.QueryString("start") IsNot Nothing Then
      If Request.QueryString("start").ToString <> String.Empty Then
        Try
          Dim mErr As Boolean = False
          Dim emsg As String = ""
          HttpContext.Current.Session("LoginID") = "0340"
          Dim tmp As SIS.QCM.qcmRequestAllotment = SIS.QCM.qcmRequestAllotment.qcmRequestAllotmentGetByID(Request.QueryString("start"))
          Dim fromEmp As String = ""
          If Request.QueryString("emp") IsNot Nothing Then
            fromEmp = Request.QueryString("emp")
          End If
          If tmp.AllotedTo <> fromEmp Then
            mErr = True
            emsg = "Inspection Call is re-alloted to someone else."
          Else
            Select Case tmp.RequestStateID
              Case "ALLOTED", "REALLOTED"
              Case "INSPECTED"
                mErr = True
                emsg = "Inspection already started"
              Case "CLOSED"
                mErr = True
                emsg = "Inspection already closed"
            End Select
          End If
          If Not mErr Then
            HttpContext.Current.Session("LoginID") = tmp.AllotedTo
            Dim mRet As String = ""
            mRet = SIS.QCM.qcmRequestAllotment.RequestInspected(tmp.RequestID)
            If mRet = String.Empty Then
              msg.Text = "<h1>Updated successfully.</h1>"
            End If
          Else
            msg.Text = "<h1>" & emsg & "</h1>"
          End If
        Catch ex As Exception
          msg.Text = ex.Message
        End Try
      End If
    End If
    If Request.QueryString("stop") IsNot Nothing Then
      If Request.QueryString("stop").ToString <> String.Empty Then
        Try
          Dim mErr As Boolean = False
          Dim emsg As String = ""
          HttpContext.Current.Session("LoginID") = "0340"
          Dim mRet As String = ""
          Dim tmp As SIS.QCM.qcmRequestAllotment = SIS.QCM.qcmRequestAllotment.qcmRequestAllotmentGetByID(Request.QueryString("stop"))
          Select Case tmp.RequestStateID
            Case "ALLOTED", "REALLOTED"
              mErr = True
              emsg = "Inspection not started"
            Case "INSPECTED"
            Case "CLOSED"
              mErr = True
              emsg = "Inspection already closed"
          End Select
          If Not mErr Then
            HttpContext.Current.Session("LoginID") = tmp.AllotedTo
            mRet = SIS.QCM.qcmRequestAllotment.RequestClosed(tmp.RequestID)
            If mRet = String.Empty Then
              msg.Text = "<h1>Closed successfully.</h1>"
            End If
          Else
            msg.Text = "<h1>" & emsg & "</h1>"
          End If
        Catch ex As Exception
          msg.Text = ex.Message
        End Try
      End If
    End If
    If Request.QueryString("emp") IsNot Nothing Then
      If Request.QueryString("emp").ToString <> String.Empty Then
        Try
          HttpContext.Current.Session("LoginID") = Request.QueryString("emp").ToString
          Dim mRet As String = SIS.QCM.qcmRequestAllotment.SendPendingCallEMail(Request.QueryString("emp").ToString)
          If mRet = String.Empty Then
            msg.Text = "<h1>Request submitted successfully.</h1>"
          End If
        Catch ex As Exception
          msg.Text = ex.Message
        End Try
      End If
    End If
  End Sub

End Class
