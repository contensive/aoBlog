VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "PatchClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False

'
'========================================================================
' ----- Returns true if the visitor is a member, and in the group named
'========================================================================
'
Public Function IsGroupListMember(Main As Object, GroupIDList As String, Optional CheckMemberID As Variant) As Boolean
    On Error GoTo ErrorTrap
    '
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
    Exit Function
    '
    ' ----- Error Trap
    '
ErrorTrap:
    Call handleClassError("", "isGroupListMember", Err.Number, Err.Source, Err.Description, True, False)
End Function
'
'
'
Public Function GetBlockingGroups(Main As Object, BlogCategoryID As Long) As String
    On Error GoTo ErrorTrap
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
    Exit Function
    '
    ' ----- Error Trap
    '
ErrorTrap:
    Call handleClassError("", "GetBlockingGroups", Err.Number, Err.Source, Err.Description, True, False)
End Function
'
'
'
Private Function handleClassError(ignore As String, MethodName As String, ErrNumber As Long, ErrSource As String, ErrDescription As String, ErrorTrap As Boolean, ResumeNext As Boolean, Optional URL As String) As String
    If Main.version < "4.2.000" Then
        Call Main.ReportError3(Main.applicationName, "patchClass", MethodName, "", ErrNumber, ErrSource, ErrDescription, True)
        Err.Clear
    Else
        Call Main.ReportError3(Main.applicationName, "patchClass", MethodName, "", ErrNumber, ErrSource, ErrDescription, ResumeNext)
    End If
End Function

