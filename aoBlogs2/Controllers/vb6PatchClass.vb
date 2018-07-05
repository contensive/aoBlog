
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
        Public Function IsGroupListMember(Main As Object, GroupIDList As String, Optional CheckMemberID As Variant) As Boolean
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
            iCheckMemberID = KmaEncodeMissingInteger(CheckMemberID, Main.MemberID)
            InList = "," & GroupIDList & ","
            CSPointer = Main.OpenCSContent_Internal("Member Rules", "((DateExpires is null)or(DateExpires>" & SQLPageStartTime & "))and(MemberID=" & KmaEncodeSQLNumber(iCheckMemberID) & ")", , , , , "GroupID")
            Do While Main.IsCSOK(CSPointer)
                If InStr(1, InList, "," & Main.GetCSText(CSPointer, "GroupID") & ",") Then
                    IsGroupListMember = True
                    Exit Do
                End If
                Call Main.NextCSRecord(CSPointer)
            Loop
            Call Main.CloseCS(CSPointer)
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
