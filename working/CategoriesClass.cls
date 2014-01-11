VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CategoriesClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
Option Explicit
'
'========================================================================
'
'========================================================================
'
'Private Main As ccWeb3.MainClass
Private Csv As Object
Private Main As Object
'
'=================================================================================
'   Execute Method, v3.4 Interface
'=================================================================================
'
Public Function Execute(CsvObject As Object, MainObject As Object, OptionString As String, FilterInput As String) As String
    On Error GoTo ErrorTrap
    '
    Dim BlogLink As String
    Dim CS As Long
    Dim s As String
    Dim Link As String
    Dim BlogURL As String
    Dim cnt As Long
    Dim AClass As String
    Dim RecordID As Long
    Dim RecordName As String
    Dim Patch As New PatchClass
    Dim GroupList As String
    Dim IsBlocked As Boolean
    '
    Set Main = MainObject
    Set Csv = CsvObject
    '
    BlogURL = Csv.GetAddonOption("Blog URL", OptionString)
    '
    If BlogURL = "" Then
        Link = "?"
    Else
        Link = BlogURL
        If InStr(1, Link, "?") = 0 Then
            Link = Link & "?"
        Else
            Link = Link & "&"
        End If
    End If
    s = ""
    RecordID = 0
    CS = CsvObject.OpenCSContent("Blog Categories")
    Do While Csv.IsCSOK(CS)
        IsBlocked = Csv.GetCSBoolean(CS, "UserBlocking")
        If IsBlocked Then
            IsBlocked = Not Main.IsAdmin()
        End If
        If IsBlocked Then
            If Main.ContentServerVersion < "4.1.098" Then
                GroupList = Patch.GetBlockingGroups(Main, Csv.GetCSInteger(CS, "id"))
                IsBlocked = Not Patch.IsGroupListMember(Main, GroupList)
            Else
                GroupList = Main.GetCS(CS, "BlockingGroups")
                IsBlocked = Not Main.IsGroupListMember(GroupList)
            End If
        End If
        If Not IsBlocked Then
            If RecordID <> 0 Then
                '
                ' add the previous (which is not the first or last)
                '
                s = s & vbCrLf & vbTab & "<li><a href=""" & Link & "BlogCategoryID=" & RecordID & """>" & RecordName & "</a></li>"
            End If
            RecordID = Csv.GetCSInteger(CS, "id")
            RecordName = Csv.GetCSText(CS, "name")
        End If
        Call Csv.NextCSRecord(CS)
    Loop
    Call Csv.CloseCS(CS)
    '
    ' add the last entry
    '
    If RecordID <> 0 Then
        s = s & vbCrLf & vbTab & "<li><a class=""last"" href=""" & Link & "BlogCategoryID=" & RecordID & """>" & RecordName & "</a></li>"
    End If
    '
    ' Add the first and last entries
    '
    If s <> "" Then
        s = "" _
            & vbCrLf & vbTab & "<li><a class=""first"" href=""" & Link & "BlogCategoryID=0"">Any</a></li>" _
            & s _
            & ""
    Else
        s = vbCrLf & vbTab & "<li><a class=""first last"" href=""" & Link & "BlogCategoryID=0"">Any</a></li>"
    End If
    '
    ' done
    '
    Execute = s
    If s <> "" Then
        Execute = "" _
            & vbCrLf & vbTab & "<ul>" _
            & kmaIndent(Execute) _
            & vbCrLf & vbTab & "</ul>" _
            & ""
        Execute = "" _
            & vbCrLf & vbTab & "<div class=""blogCatList"">" _
            & kmaIndent(Execute) _
            & vbCrLf & vbTab & "</div>" _
            & ""
    End If
    
    '
    Exit Function
ErrorTrap:
    Call handleClassError("", "execute", Err.Number, Err.Source, Err.Description, True, False)
    'HandleError
End Function
'
'
'
Private Function handleClassError(ignore As String, MethodName As String, ErrNumber As Long, ErrSource As String, ErrDescription As String, ErrorTrap As Boolean, ResumeNext As Boolean, Optional URL As String) As String
    Call Main.ReportError3(Main.applicationName, "categoriesClass", MethodName, "", ErrNumber, ErrSource, ErrDescription, ResumeNext)
End Function

