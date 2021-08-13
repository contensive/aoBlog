
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Controllers
    '
    Public Class PaginationController
        '
        '====================================================================================
        ''' <summary>
        ''' return the html required for pagination
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordsPerPage"></param>
        ''' <param name="pageNumberCurrent"></param>
        ''' <param name="recordsOnThisPage"></param>
        ''' <param name="recordCount"></param>
        ''' <returns></returns>
        Public Shared Function getRecordPagination(cp As CPBaseClass, recordsPerPage As Integer, pageNumberCurrent As Integer, recordsOnThisPage As Integer, recordCount As Integer) As String
            Try
                Dim result As New StringBuilder()
                Dim basePageUrl As String = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", "")
                If pageNumberCurrent > 1 Then result.Append("<li class=""page-item""><a class=""page-link"" href=""" & getPageUrl(basePageUrl, pageNumberCurrent - 1) & """>Previous</a></li>")
                Dim recordTop As Integer = (pageNumberCurrent - 1) * recordsPerPage
                Dim pageCount As Integer = CInt(Math.Truncate((recordCount / recordsPerPage) + 0.999))
                Dim pageFirst = pageNumberCurrent - 3
                If pageFirst < 1 Then pageFirst = 1
                Dim pageLast = pageFirst + 6
                If pageLast > pageCount Then pageLast = pageCount
                If (pageCount > 1) Then
                    For pageNumber As Integer = pageFirst To pageLast
                        result.Append("<li class=""page-item""><a class=""page-link"" href=""" & getPageUrl(basePageUrl, pageNumber) & """>" & pageNumber & "</a></li>")
                    Next
                End If
                If (pageCount > pageNumberCurrent) Then
                    result.Append("<li class=""page-item""><a class=""page-link"" href=""" & getPageUrl(basePageUrl, pageCount) & """>Next</a></li>")
                End If
                '
                Return "<nav><ul class=""pagination"">" & result.ToString() & "</ul></nav>"
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        Public Shared Function getPageUrl(basePageUrl As String, pageNumber As Integer) As String
            If pageNumber < 2 Then
                Return basePageUrl
            Else
                Return basePageUrl & "?page=" & pageNumber
            End If
        End Function
    End Class
End Namespace
