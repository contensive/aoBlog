Option Explicit On
Option Strict On

Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Controllers
    Public NotInheritable Class InstanceIdController
        '
        '====================================================================================================
        '
        Public Shared Function getInstanceId(cp As CPBaseClass) As String
            Dim instanceId As String = cp.Doc.GetText("instanceId")
            If (String.IsNullOrWhiteSpace(instanceId)) Then instanceId = "BlogWithoutinstanceId-PageId-" & cp.Doc.PageId
            Return instanceId
        End Function

    End Class
End Namespace

