
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    Public Class ArchiveView
        '
        '
        '====================================================================================
        '
        Public Shared Function GetFormBlogArchiveDateList(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, blogListQs As String) As String
            '
            Dim result As String = ""
            Try
                Dim OpenSQL As String = ""
                Dim contentControlId As String = cp.Content.GetID(cnBlogEntries).ToString
                '
                result = vbCrLf & cp.Content.GetCopy("Blogs Archives Header for " & blog.name, "<h2>" & blog.name & " Blog Archive</h2>")
                ' 
                Dim archiveDateList As List(Of BlogCopyModel.ArchiveDateModel) = BlogCopyModel.createArchiveListFromBlogCopy(cp, blog.Id)
                If (archiveDateList.Count = 0) Then
                    '
                    ' No archives, give them an error
                    result = result & cr & "<div class=""aoBlogProblem"">There are no current blog entries</div>"
                    result = result & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                Else
                    Dim ArchiveMonth As Integer
                    Dim ArchiveYear As Integer
                    If archiveDateList.Count = 1 Then
                        '
                        ' one archive - just display it
                        ArchiveMonth = archiveDateList.First.Month
                        ArchiveYear = archiveDateList.First.Year
                        result = result & GetFormBlogArchivedBlogs(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs)
                    Else
                        '
                        ' Display List of archive
                        Dim qs As String = cp.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, FormBlogArchiveDateList.ToString())
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchivedBlogs.ToString())
                        For Each archiveDate In archiveDateList
                            ArchiveMonth = archiveDate.Month
                            ArchiveYear = archiveDate.Year
                            Dim NameOfMonth As String = MonthName(ArchiveMonth)
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveMonth, CStr(ArchiveMonth))
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveYear, CStr(ArchiveYear))
                            result = result & vbCrLf & vbTab & vbTab & "<div class=""aoBlogArchiveLink""><a href=""?" & qs & """>" & NameOfMonth & " " & ArchiveYear & "</a></div>"
                        Next
                        result = result & vbCrLf & vbTab & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                    End If
                End If
                '

            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================
        '
        Public Shared Function GetFormBlogArchivedBlogs(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, blogListQs As String) As String
            '
            Dim result As String = ""
            Try
                Dim PageNumber As Integer
                '
                ' If it is the current month, start at entry 6
                '
                PageNumber = 1
                '
                ' List Blog Entries
                '
                Dim BlogEntryModelList As List(Of BlogEntryModel) = DbModel.createList(Of BlogEntryModel)(cp, "(Month(DateAdded) = " & request.ArchiveMonth & ")And(year(DateAdded)=" & request.ArchiveYear & ")And(BlogID=" & blog.Id & ")", "DateAdded Desc")
                If (BlogEntryModelList.Count = 0) Then
                    result = "<div Class=""aoBlogProblem"">There are no blog archives For " & request.ArchiveMonth & "/" & request.ArchiveYear & "</div>"
                Else
                    Dim EntryPtr As Integer = 0
                    Dim entryEditLink As String = ""
                    For Each blogEntry In BlogEntryModelList
                        Dim EntryID As Integer = blogEntry.id
                        Dim AuthorMemberID As Integer = blogEntry.AuthorMemberID
                        If AuthorMemberID = 0 Then
                            AuthorMemberID = blogEntry.CreatedBy
                        End If
                        Dim DateAdded As Date = blogEntry.DateAdded
                        Dim EntryName As String = blogEntry.name
                        If cp.User.IsAuthoring("Blogs") Then
                            entryEditLink = cp.Content.GetEditLink(EntryName, EntryID.ToString(), True, EntryName, True)
                        End If
                        Dim EntryCopy As String = blogEntry.copy
                        Dim allowComments As Boolean = blogEntry.AllowComments
                        Dim BlogTagList As String = blogEntry.tagList
                        Dim primaryImagePositionId As Integer = blogEntry.primaryImagePositionId
                        Dim Return_CommentCnt As Integer
                        result = result & BlogEntryCellView.getBlogEntryCell(cp, blog, rssFeed, blogEntry, user, False, False, Return_CommentCnt, BlogTagList, blogListQs)
                    Next
                    result = result & cr & "<hr>"
                    EntryPtr = EntryPtr + 1

                End If
                '              
                '
                Dim qs As String = blogListQs
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch.ToString())
                result = result & cr & "<div>&nbsp;</div>"
                result = result & cr & "<div Class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                qs = cp.Doc.RefreshQueryString()
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList.ToString())
                result = result & cr & "<div Class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                Call cp.CSNew.Close()
                result = result & cp.Html.Hidden(RequestNameSourceFormID, FormBlogArchivedBlogs.ToString())
                result = cp.Html.Form(result)
                '
                GetFormBlogArchivedBlogs = result
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
    End Class
End Namespace
