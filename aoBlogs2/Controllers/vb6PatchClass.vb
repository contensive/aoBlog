
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

            Dim iCheckMemberID As Integer
            Dim CSPointer As Integer
            Dim GroupID As Integer
            Dim MethodName As String
            Dim iGroupName As String
            Dim InList As String
            Dim SQLPageStartTime As String
            '
            SQLPageStartTime = cp.Db.EncodeSQLDate(Now)
            result = False
            iCheckMemberID = If(CheckMemberID = 0, cp.User.Id, CheckMemberID)
            InList = "," & GroupIDList & ","
            Dim memberRuleList As List(Of Models.MemberRuleModel) = Models.MemberRuleModel.createList(cp, "((DateExpires is null)or(DateExpires>" & SQLPageStartTime & "))and(MemberID=" & cp.Db.EncodeSQLNumber(iCheckMemberID) & ")")
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
        Public Function GetBlockingGroups(cp As CPBaseClass, BlogCategoryID As Integer) As String
            '

            Dim result As String = ""
            '           
            For Each BlogCategoryGroupRule In Models.BlogCategoryGroupRulesModel.createList(cp, "BlogCategoryID=" & BlogCategoryID)
                result = result & "," & BlogCategoryGroupRule.GroupID
            Next

            Return result
        End Function

    End Class
End Namespace
