Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Contensive.Addons.aoBlogs2
    '
    ' Sample Vb addon
    '
    Public Class SubscribeEmailClass
        Inherits AddonBaseClass
        '
        '=====================================================================================
        ' addon api
        '=====================================================================================
        '
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim returnHtml As String
            Try
                Dim email As String = CP.Doc.GetText("email")
                Dim blogId As Integer = CP.Doc.GetInteger("blogId")
                Dim blogName As String = ""
                Dim cs As CPCSBaseClass = CP.CSNew()
                Dim groupId As Integer = 0
                Dim userId As Integer = 0
                '
                If CP.User.IsRecognized() And Not CP.User.IsAuthenticated Then
                    Call CP.User.Logout()
                End If
                '
                If email <> "" And blogId <> 0 Then
                    If cs.Open("blogs", "id=" & blogId) Then
                        blogName = cs.GetText("name")
                        groupId = cs.GetInteger("emailSubscribeGroupId")
                    End If
                    Call cs.Close()
                    If Not cs.Open("groups", "id=" & groupId) Then
                        Call cs.Close()
                        If cs.Insert("Groups") Then
                            groupId = cs.GetInteger("id")
                            Call cs.SetField("name", "Email Subscriptions for Blog " & blogName)
                            Call cs.SetField("caption", "Email Subscriptions for Blog " & blogName)
                            Call cs.SetField("allowbulkemail", "1")
                            Call cs.SetField("publicjoin", "1")
                        End If
                        Call cs.Close()
                        If cs.Open("blogs","id=" &  blogId) Then
                            Call cs.SetField("emailSubscribeGroupId", groupId.ToString())
                        End If
                    End If
                    Call cs.Close()
                    '
                    If groupId > 0 Then
                        If cs.Open("people", "email=" & CP.Db.EncodeSQLText(email)) Then
                            userId = cs.GetInteger("id")
                        End If
                        Call cs.Close()
                        '
                        If userId = 0 Then
                            '
                            ' this email address was not found in users, set it to the current user, authenticated or not
                            '   (recognized case is not possible b/c check at top of routine)
                            '
                            userId = CP.User.Id
                            If cs.Open("people", "id=" & CP.User.Id) Then
                                Call cs.SetField("email", email)
                            End If
                            Call cs.Close()
                        End If
                        Call CP.Group.AddUser(groupId.ToString())
                    End If
                End If
                '
                ' return OK to display thank you message
                '
                returnHtml = "OK"
            Catch ex As Exception
                errorReport(CP, ex, "execute")
                returnHtml = ""
            End Try
            Return returnHtml
        End Function
        '
        '=====================================================================================
        ' common report for this class
        '=====================================================================================
        '
        Private Sub errorReport(ByVal cp As CPBaseClass, ByVal ex As Exception, ByVal method As String)
            Try
                cp.Site.ErrorReport(ex, "Unexpected error in sampleClass." & method)
            Catch exLost As Exception
                '
                ' stop anything thrown from cp errorReport
                '
            End Try
        End Sub
    End Class
End Namespace
