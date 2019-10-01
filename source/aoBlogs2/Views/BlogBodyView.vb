
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
    Public Class BlogBodyView
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
                If (blog.Id = 0) Then Throw New ApplicationException("BlogBody requires valid Blog object and blog.id is 0.")
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
                    dstFormId = BlogBodyController.ProcessForm(cp, blog, RSSFeed, blogEntry, request, user, blogPageBaseLink, RetryCommentPost)
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
                        result.Append(ArchiveView.GetFormBlogArchiveDateList(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs))
                    Case FormBlogArchivedBlogs
                        result.Append(ArchiveView.GetFormBlogArchivedBlogs(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs))
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
        '
    End Class
End Namespace
