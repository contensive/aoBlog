
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
            Dim iCheckMemberID As Long
            Dim CSPointer As Long
            Dim GroupID As Long
            Dim MethodName As String
            Dim iGroupName As String
            Dim InList As String
            Dim SQLPageStartTime As String
            '
            SQLPageStartTime = KmaEncodeSQLDate(Now)
            IsGroupListMember = False
            iCheckMemberID = If(CheckMemberID = 0, cp.User.Id, CheckMemberID)
            InList = "," & GroupIDList & ","
            CSPointer = cp.OpenCSContent_Internal("Member Rules", "((DateExpires is null)or(DateExpires>" & SQLPageStartTime & "))and(MemberID=" & KmaEncodeSQLNumber(iCheckMemberID) & ")", , , , , "GroupID")
            Do While cp.IsCSOK(CSPointer)
                If InStr(1, InList, "," & cp.GetCSText(CSPointer, "GroupID") & ",") Then
                    IsGroupListMember = True
                    Exit Do
                End If
                Call cp.NextCSRecord(CSPointer)
            Loop
            Call cp.CloseCS(CSPointer)
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
