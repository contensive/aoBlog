
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Controllers
    '
    Public Class Vb6PatchClass
        '
        '
        '========================================================================
        ' ----- Returns true if the visitor is a member, and in the group named
        '========================================================================
        '
        Public Function IsGroupListMember(cp As CPBaseClass, GroupIDList As String, Optional CheckMemberID As Object = 0) As Boolean
            Dim result As Boolean = False

            Dim iCheckMemberID As Long
            Dim CSPointer As Long
            Dim GroupID As Long
            Dim MethodName As String
            Dim iGroupName As String
            Dim InList As String
            Dim SQLPageStartTime As String
            '
            SQLPageStartTime = cp.Db.EncodeSQLDate(Now)
            result = False
            iCheckMemberID = If(CheckMemberID = 0, cp.User.Id, CheckMemberID)
            InList = "," & GroupIDList & ","
            Dim memberRuleList As List(Of Models.MemberRuleModel) = Models.MemberRuleModel.createList(cp, "((DateExpires is null)or(DateExpires>" & SQLPageStartTime & "))and(MemberID=" & KmaEncodeSQLNumber(iCheckMemberID) & ")")
            For Each memberRule In memberRuleList
                If InStr(1, InList, "," & memberRule.GroupID & ",") Then
                    result = True
                    Exit For
                End If
            Next

            Return result

        End Function
        '
        '
        '
        Public Function GetBlockingGroups(Main As Object, BlogCategoryID As Long) As String
            '
            Dim SQL As String
            Dim CS As Long
            '
            SQL = "select GroupID from ccBlogCategoryGroupRules where BlogCategoryID=" & BlogCategoryID
            CS = Main.OpenCSSQL("default", SQL)
            Do While Main.IsCSOK(CS)
                GetBlockingGroups = GetBlockingGroups & "," & Main.GetCS(CS, "GroupID")
                Call Main.NextCSRecord(CS)
            Loop
            Call Main.CloseCS(CS)
        End Function

    End Class
End Namespace
