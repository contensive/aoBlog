
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
    Public Class BlogBodyController
        '
        '========================================================================
        ''' <summary>
        ''' Inner Blog - the list of posts without sidebar. Blog object must be valid
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="blog"></param>
        ''' <returns></returns>
        Public Shared Function getBlogBody(cp As CPBaseClass, blog As BlogModel, request As View.RequestModel, blogEntry As BlogEntryModel) As String
            Dim result As String = ""
            Try
                If (blog Is Nothing) Then Throw New ApplicationException("BlogBody requires valid Blog object and blog object is null.")
                If (blog.id = 0) Then Throw New ApplicationException("BlogBody requires valid Blog object and blog.id is 0.")
                '
                Dim user = PersonModel.create(cp, cp.User.Id)
                Dim qsBlogList As String = cp.Doc.RefreshQueryString()
                qsBlogList = cp.Utils.ModifyQueryString(qsBlogList, RequestNameSourceFormID, "")
                qsBlogList = cp.Utils.ModifyQueryString(qsBlogList, RequestNameFormID, "")
                qsBlogList = cp.Utils.ModifyQueryString(qsBlogList, RequestNameBlogCategoryID, "")
                qsBlogList = cp.Utils.ModifyQueryString(qsBlogList, RequestNameBlogEntryID, "")
                Dim blogPageBaseLink As String = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", "")
                '
                ' Get the Feed Args
                Dim RSSFeed As RSSFeedModel = DbModel.create(Of RSSFeedModel)(cp, blog.RSSFeedID)
                If (RSSFeed Is Nothing) Then
                    RSSFeed = DbModel.add(Of RSSFeedModel)(cp)
                    RSSFeed.name = blog.Caption
                    RSSFeed.description = "This is your First RssFeed"
                    RSSFeed.save(Of BlogModel)(cp)
                    blog.RSSFeedID = RSSFeed.id
                    blog.save(Of BlogModel)(cp)
                End If
                '
                ' Process Input
                Dim RetryCommentPost As Boolean = False
                Dim dstFormId As Integer = request.FormID
                If request.ButtonValue <> "" Then
                    '
                    ' Process the source form into form if there was a button - else keep formid
                    dstFormId = ProcessForm(cp, blog, RSSFeed, blogEntry, request, user, blogPageBaseLink, RetryCommentPost)
                End If
                '
                ' -- Get Next Form
                result = GetForm(cp, blog, RSSFeed, blogEntry, request, user, dstFormId, blogPageBaseLink, qsBlogList, RetryCommentPost)
                '
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Return String.Empty
            End Try
        End Function
        '
        '====================================================================================
        '
        Private Shared Function GetForm(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, FormID As Integer, blogListLink As String, blogListQs As String, RetryCommentPost As Boolean) As String
            Dim result As New StringBuilder()
            Try
                Select Case FormID
                    Case FormBlogPostDetails
                        result.Append(ArticleView.getArticleView(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs, RetryCommentPost))
                    Case FormBlogArchiveDateList
                        result.Append(GetFormBlogArchiveDateList(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs))
                    Case FormBlogArchivedBlogs
                        result.Append(GetFormBlogArchivedBlogs(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs))
                    Case FormBlogEntryEditor
                        result.Append(EditView.GetFormBlogEdit(cp, blog, rssFeed, blogEntry, request, user, blogListLink))
                    Case FormBlogSearch
                        result.Append(SearchView.GetFormBlogSearch(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs))
                    Case Else
                        If (blogEntry IsNot Nothing) Then
                            '
                            ' Go to details page
                            '
                            result.Append(ArticleView.getArticleView(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs, RetryCommentPost))
                        Else
                            '
                            ' list all the entries
                            '
                            result.Append(ListView.GetListView(cp, blog, rssFeed, request, user, blogListLink, blogListQs))
                        End If
                End Select
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result.ToString()
        End Function
        '
        '====================================================================================
        '
        Private Shared Function ProcessForm(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, ByRef RetryCommentPost As Boolean) As Integer
            '
            Try
                If request.ButtonValue <> "" Then
                    Select Case request.SourceFormID
                        Case FormBlogPostList
                            ProcessForm = FormBlogPostList
                        Case FormBlogEntryEditor
                            ProcessForm = EditView.ProcessFormBlogEdit(cp, blog, rssFeed, blogEntry, request, blogListLink)
                        Case FormBlogPostDetails
                            ProcessForm = ArticleView.processArticleView(cp, blog, rssFeed, request, user, RetryCommentPost)
                        Case FormBlogArchiveDateList
                            ProcessForm = FormBlogArchiveDateList
                        Case FormBlogSearch
                            ProcessForm = FormBlogSearch
                        Case FormBlogArchivedBlogs
                            ProcessForm = FormBlogArchivedBlogs
                    End Select
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function
        '
        '====================================================================================
        '
        Private Shared Function GetFormBlogArchiveDateList(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, blogListQs As String) As String
            '
            Dim result As String = ""
            Try
                Dim OpenSQL As String = ""
                Dim contentControlId As String = cp.Content.GetID(cnBlogEntries).ToString
                '
                result = vbCrLf & cp.Content.GetCopy("Blogs Archives Header for " & blog.name, "<h2>" & blog.name & " Blog Archive</h2>")
                ' 
                Dim archiveDateList As List(Of BlogCopyModel.ArchiveDateModel) = BlogCopyModel.createArchiveListFromBlogCopy(cp, blog.id)
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
        Private Shared Function GetFormBlogArchivedBlogs(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, blogListQs As String) As String
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
                Dim BlogEntryModelList As List(Of BlogEntryModel) = DbModel.createList(Of BlogEntryModel)(cp, "(Month(DateAdded) = " & request.ArchiveMonth & ")And(year(DateAdded)=" & request.ArchiveYear & ")And(BlogID=" & blog.id & ")", "DateAdded Desc")
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
                        Dim imageDisplayTypeId As Integer = blogEntry.imageDisplayTypeId
                        Dim primaryImagePositionId As Integer = blogEntry.primaryImagePositionId
                        Dim articlePrimaryImagePositionId As Integer = blogEntry.articlePrimaryImagePositionId
                        Dim Return_CommentCnt As Integer
                        result = result & genericController.getBlogEntryCell(cp, blog, rssFeed, blogEntry, user, False, False, Return_CommentCnt, BlogTagList, blogListQs)
                    Next
                    result = result & cr & "<div Class=""aoBlogEntryDivider"">&nbsp;</div>"
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
    End Class
End Namespace
