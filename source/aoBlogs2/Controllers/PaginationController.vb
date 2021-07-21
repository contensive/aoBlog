
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
        ''' <param name="pageNumber"></param>
        ''' <param name="recordsOnThisPage"></param>
        ''' <param name="recordCount"></param>
        ''' <returns></returns>
        Public Shared Function getRecordPagination(cp As CPBaseClass, recordsPerPage As Integer, pageNumber As Integer, recordsOnThisPage As Integer, recordCount As Integer) As String
            Try
                Dim result As New StringBuilder()
                If pageNumber > 1 Then result.Append("<li class=""page-item""><a class=""page-link"" href=""?page=1"">Previous</a></li>")
                Dim recordTop As Integer = (pageNumber - 1) * recordsPerPage
                Dim pageCount As Integer = CInt(Math.Truncate((recordCount / recordsPerPage) + 0.999))
                If (pageCount > 1) Then
                    For page As Integer = 1 To pageCount
                        result.Append("<li class=""page-item""><a class=""page-link"" href=""?page=" & page & """>" & page & "</a></li>")
                    Next
                End If
                If (pageCount > pageNumber) Then
                    result.Append("<li class=""page-item""><a class=""page-link"" href=""?page=" & (pageNumber + 1) & """>Next</a></li>")
                End If
                '
                Return "<nav><ul class=""pagination"">" & result.ToString() & "</ul></nav>"
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
