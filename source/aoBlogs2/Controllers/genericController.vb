Option Explicit On
Option Strict On

Imports System.Linq
Imports Contensive.Addons.Blog.Models
Imports Contensive.Addons.Blog.Views
Imports Contensive.BaseClasses

Namespace Controllers
    Public NotInheritable Class genericController
        Private Sub New()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' if date is invalid, set to minValue
        ''' </summary>
        ''' <param name="srcDate"></param>
        ''' <returns></returns>
        Public Shared Function encodeMinDate(srcDate As DateTime) As DateTime
            Dim returnDate As DateTime = srcDate
            If srcDate < New DateTime(1900, 1, 1) Then
                returnDate = DateTime.MinValue
            End If
            Return returnDate
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' if valid date, return the short date, else return blank string 
        ''' </summary>
        ''' <param name="srcDate"></param>
        ''' <returns></returns>
        Public Shared Function getShortDateString(srcDate As DateTime) As String
            Dim returnString As String = ""
            Dim workingDate As DateTime = encodeMinDate(srcDate)
            If Not isDateEmpty(srcDate) Then
                returnString = workingDate.ToShortDateString()
            End If
            Return returnString
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function isDateEmpty(srcDate As DateTime) As Boolean
            Return (srcDate < New DateTime(1900, 1, 1))
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function getSortOrderFromInteger(id As Integer) As String
            Return id.ToString().PadLeft(7, "0"c)
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function getDateForHtmlInput(source As DateTime) As String
            If isDateEmpty(source) Then
                Return ""
            Else
                Return source.Year.ToString() + "-" + source.Month.ToString().PadLeft(2, "0"c) + "-" + source.Day.ToString().PadLeft(2, "0"c)
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function convertToDosPath(sourcePath As String) As String
            Return sourcePath.Replace("/", "\")
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function convertToUnixPath(sourcePath As String) As String
            Return sourcePath.Replace("\", "/")
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' is a member of a group in the group model list
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="groupList"></param>
        ''' <returns></returns>
        Public Shared Function isGroupListMember(cp As CPBaseClass, groupList As List(Of GroupModel)) As Boolean
            For Each Group In groupList
                If cp.User.IsInGroup(Group.name) Then Return True
            Next
            Return False
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Return a shortened version of the copy
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="rawCopy"></param>
        ''' <param name="MaxLength"></param>
        ''' <returns></returns>
        Friend Shared Function getBriefCopy(cp As CPBaseClass, rawCopy As String, MaxLength As Integer) As String
            Try
                Dim Copy As String = cp.Utils.ConvertHTML2Text(rawCopy)
                If Len(Copy) > MaxLength Then
                    Copy = Left(Copy, MaxLength)
                    Copy = Copy & "..."
                End If
                Return Copy
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Return ""
            End Try
        End Function
        '
        '====================================================================================
        '
        Public Shared Function getFormTableRow2(cp As CPBaseClass, Innards As String) As String
            Return "<tr>" _
                & "<td colspan=2 width=""100%"">" & Innards & "</td>" _
                & "</tr>"
        End Function
        '
        '====================================================================================
        '
        Public Shared Function getFormTableRow(cp As CPBaseClass, FieldCaption As String, Innards As String, Optional AlignLeft As Boolean = True) As String
            '
            Dim Stream As String = ""
            Try
                Dim AlignmentString As String
                '
                If Not AlignLeft Then
                    AlignmentString = " align=right"
                Else
                    AlignmentString = " align=left"
                End If
                '
                Stream = Stream & "<tr>"
                Stream = Stream & "<td Class=""aoBlogTableRowCellLeft"" " & AlignmentString & ">" & FieldCaption & "</td>"
                Stream = Stream & "<td Class=""aoBlogTableRowCellRight"">" & Innards & "</td>"
                Stream = Stream & "</tr>"
                '
                ' GetFormTableRow = Stream
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return Stream
        End Function
        '
        '====================================================================================
        '
        Public Shared Function getField(cp As CPBaseClass, RequestName As String, Height As Integer, Width As Integer, MaxLenghth As Integer, DefaultValue As String) As String
            Dim result As String = ""
            Try
                If Height = 0 Then
                    Height = 1
                End If
                If Width = 0 Then
                    Width = 25
                End If
                '           
                result = cp.Html.InputText(RequestName, DefaultValue, Height.ToString(), Width.ToString())
                result = Replace(result, "<INPUT ", "<INPUT maxlength=""" & MaxLenghth & """ ", 1, 99, CompareMethod.Text)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '
        Public Shared Function getLinkAlias(cp As CPBaseClass, sourceLink As String) As String
            '
            Dim result As String = ""
            Try
                result = sourceLink
                If cp.Utils.EncodeBoolean(cp.Site.GetProperty("allowLinkAlias", "1")) Then
                    Dim Link As String = sourceLink
                    '
                    Dim pageQs() As String = Split(LCase(Link), "?")
                    If UBound(pageQs) > 0 Then
                        Dim nameValues() As String = Split(pageQs(1), "&")
                        Dim cnt As Integer = UBound(nameValues) + 1
                        If UBound(nameValues) < 0 Then
                        Else
                            Dim qs As String = ""
                            Dim Ptr As Integer
                            Dim pageId As Integer
                            For Ptr = 0 To cnt - 1
                                Dim NameValue As String = nameValues(Ptr)
                                If pageId = 0 Then
                                    If Mid(NameValue, 1, 4) = "bid=" Then
                                        pageId = cp.Utils.EncodeInteger(Mid(NameValue, 5))
                                        NameValue = ""
                                    End If
                                End If
                                If NameValue <> "" Then
                                    qs = qs & "&" & NameValue
                                End If
                            Next
                            If pageId <> 0 Then
                                If Len(qs) > 1 Then
                                    qs = Mid(qs, 2)
                                End If
                                result = cp.Content.GetLinkAliasByPageID(pageId, qs, sourceLink)
                            End If
                        End If
                    End If
                End If
                getLinkAlias = result
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '
        'Public Shared Function addEditWrapper(ByVal cp As CPBaseClass, ByVal innerHtml As String, ByVal instanceId As Integer, ByVal instanceName As String, ByVal contentName As String, ByVal designBlockCaption As String) As String
        '    If (Not cp.User.IsEditingAnything) Then Return innerHtml
        '    Dim editLink As String = getEditLink(cp, contentName, instanceId, designBlockCaption)
        '    Dim settingContent As String = cp.Html.div(innerHtml, "", "dbSettingWrapper")
        '    Dim settingHeader As String = cp.Html.div(editLink, "", "dbSettingHeader")
        '    Return cp.Html.div(settingHeader + settingContent)
        'End Function
        ''
        ''========================================================================
        ''
        'Public Shared Function getEditLink(ByVal cp As CPBaseClass, ByVal contentName As String, ByVal recordId As Integer, Caption As String) As String
        '    Dim contentId As Integer = cp.Content.GetID(contentName)
        '    If contentId = 0 Then Return String.Empty
        '    Return "<a href=""/admin?af=4&aa=2&ad=1&cid=" & contentId & "&id=" & recordId & """ class=""ccRecordEditLink""><span style=""color:#0c0""><i title=""edit"" class=""fas fa-cog""></i></span>&nbsp;" & Caption & "</a>"
        'End Function
        '
        Public Shared Function addEditWrapper(ByVal cp As CPBaseClass, ByVal innerHtml As String, ByVal recordId As Integer, ByVal recordName As String, ByVal contentName As String) As String
            If (Not cp.User.IsEditingAnything) Then Return innerHtml
            Dim header As String = cp.Content.GetEditLink(contentName, recordId.ToString(), False, recordName, True)
            Dim content As String = cp.Html.div(innerHtml, "", "")
            Return cp.Html.div(header + content, "", "ccEditWrapper")
        End Function

    End Class
End Namespace

