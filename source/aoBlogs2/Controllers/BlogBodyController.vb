
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
        '====================================================================================
        '
        Public Shared Function ProcessForm(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, ByRef RetryCommentPost As Boolean) As Integer
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
    End Class
End Namespace
