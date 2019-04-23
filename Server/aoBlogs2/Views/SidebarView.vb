
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    '
    Public Class SidebarView
        '
        '====================================================================================
        '
        Public Shared Function GetSidebarArchiveList(cp As CPBaseClass, BlogID As Integer, blogListQs As String) As String
            Dim returnHtml As String = ""
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                Dim ArchiveMonth As Integer
                Dim ArchiveYear As Integer
                Dim NameOfMonth As String
                Dim qs As String
                Dim SQL As String
                '
                SQL = "SELECT distinct Month(DateAdded) as ArchiveMonth, year(dateadded) as ArchiveYear " _
                    & " From ccBlogCopy" _
                    & " Where (ContentControlID = " & cp.Content.GetID(cnBlogEntries) & ") And (Active <> 0)" _
                    & " AND (BlogID=" & BlogID & ")" _
                    & " ORDER BY year(dateadded) desc, Month(DateAdded) desc"
                If cs.OpenSQL(SQL) Then
                    qs = blogListQs
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchivedBlogs.ToString())
                    Do While cs.OK
                        ArchiveMonth = cs.GetInteger("ArchiveMonth")
                        ArchiveYear = cs.GetInteger("ArchiveYear")
                        NameOfMonth = MonthName(ArchiveMonth)
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveMonth, CStr(ArchiveMonth))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveYear, CStr(ArchiveYear))
                        returnHtml = returnHtml & vbCrLf & vbTab & vbTab & "<li class=""aoBlogArchiveLink""><a href=""?" & qs & """>" & NameOfMonth & "&nbsp;" & ArchiveYear & "</a></li>"
                        Call cs.GoNext()
                    Loop
                End If
                Call cs.Close()
                If returnHtml <> "" Then
                    returnHtml = "" _
                        & vbCrLf & vbTab & "<ul class=""aoBlogArchiveLinkList"">" _
                        & returnHtml _
                        & vbCrLf & vbTab & "</ul>"
                End If
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
        '
    End Class
End Namespace
