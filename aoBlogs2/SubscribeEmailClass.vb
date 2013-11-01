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
                Dim cs As CPCSBaseClass = CP.CSNew()
                Dim groupId As Integer = 0
                '
                If CP.User.IsRecognized() And Not CP.User.IsAuthenticated Then
                    Call CP.User.Logout()
                End If
                '
                If email <> "" And blogId <> 0 Then
                    If cs.OpenRecord("blogs", blogId) Then
                        groupId = cs.GetInteger("emailSubscribeGroupId ")
                    End If
                    Call cs.Close()
                    If groupId > 0 Then
                        If cs.OpenRecord("people", CP.User.Id) Then
                            Call cs.SetField("email", email)
                        End If
                        Call cs.Close()
                        Call CP.Group.AddUser(groupId.ToString())
                    End If
                End If
                returnHtml = ""
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
