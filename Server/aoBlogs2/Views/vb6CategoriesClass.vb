
'Imports System
'Imports System.Collections.Generic
'Imports System.Text
'Imports Contensive.BaseClasses

'Namespace Views
'    '
'    Public Class Vb6CategoriesClass
'        Inherits AddonBaseClass
'        '
'        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
'            Dim returnHtml As String = ""
'            Try
'                '
'                Dim BlogLink As String
'                Dim CS As Integer
'                Dim s As String
'                Dim Link As String
'                Dim BlogURL As String
'                Dim cnt As Integer
'                Dim AClass As String
'                Dim RecordID As Integer
'                Dim RecordName As String
'                Dim GroupList As String
'                Dim IsBlocked As Boolean
'                '
'                BlogURL = Csv.GetAddonOption("Blog URL", OptionString)
'                '
'                If BlogURL = "" Then
'                    Link = "?"
'                Else
'                    Link = BlogURL
'                    If InStr(1, Link, "?") = 0 Then
'                        Link = Link & "?"
'                    Else
'                        Link = Link & "&"
'                    End If
'                End If
'                s = ""
'                RecordID = 0
'                CS = CsvObject.OpenCSContent("Blog Categories")
'                Do While Csv.IsCSOK(CS)
'                    IsBlocked = Csv.GetCSBoolean(CS, "UserBlocking")
'                    If IsBlocked Then
'                        IsBlocked = Not Main.IsAdmin()
'                    End If
'                    If IsBlocked Then
'                        If Main.ContentServerVersion < "4.1.098" Then
'                            GroupList = Patch.GetBlockingGroups(Main, Csv.GetCSInteger(CS, "id"))
'                            IsBlocked = Not Patch.IsGroupListMember(Main, GroupList)
'                        Else
'                            GroupList = Main.GetCS(CS, "BlockingGroups")
'                            IsBlocked = Not Main.IsGroupListMember(GroupList)
'                        End If
'                    End If
'                    If Not IsBlocked Then
'                        If RecordID <> 0 Then
'                            '
'                            ' add the previous (which is not the first or last)
'                            '
'                            s = s & vbCrLf & vbTab & "<li><a href=""" & Link & "BlogCategoryID=" & RecordID & """>" & RecordName & "</a></li>"
'                        End If
'                        RecordID = Csv.GetCSInteger(CS, "id")
'                        RecordName = Csv.GetCSText(CS, "name")
'                    End If
'                    Call Csv.NextCSRecord(CS)
'                Loop
'                Call Csv.CloseCS(CS)
'                '
'                ' add the last entry
'                '
'                If RecordID <> 0 Then
'                    s = s & vbCrLf & vbTab & "<li><a class=""last"" href=""" & Link & "BlogCategoryID=" & RecordID & """>" & RecordName & "</a></li>"
'                End If
'                '
'                ' Add the first and last entries
'                '
'                If s <> "" Then
'                    s = "" _
'            & vbCrLf & vbTab & "<li><a class=""first"" href=""" & Link & "BlogCategoryID=0"">Any</a></li>" _
'            & s _
'            & ""
'                Else
'                    s = vbCrLf & vbTab & "<li><a class=""first last"" href=""" & Link & "BlogCategoryID=0"">Any</a></li>"
'                End If
'                '
'                ' done
'                '
'                Execute = s
'                If s <> "" Then
'                    Execute = "" _
'            & vbCrLf & vbTab & "<ul>" _
'            & CP.Html.Indent(Execute) _
'            & vbCrLf & vbTab & "</ul>" _
'            & ""
'                    Execute = "" _
'            & vbCrLf & vbTab & "<div class=""blogCatList"">" _
'            & CP.Html.Indent(Execute) _
'            & vbCrLf & vbTab & "</div>" _
'            & ""
'                End If
'                '
'            Catch ex As Exception
'                CP.Site.ErrorReport(ex)
'            End Try
'            Return returnHtml
'        End Function
'    End Class
'End Namespace
