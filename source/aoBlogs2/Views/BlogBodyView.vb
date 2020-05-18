
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
        ''' <returns></returns>
        Public Shared Function getBlogBody(cp As CPBaseClass, app As ApplicationController, legacyRequest As View.RequestModel, blogBodyRequest As BlogBodyRequestModel) As String
            Dim result As String = ""
            Try
                '
                ' Process Input
                Dim RetryCommentPost As Boolean = False
                Dim dstFormId As Integer = legacyRequest.FormID
                If legacyRequest.ButtonValue <> "" Then
                    '
                    ' Process the source form into form if there was a button - else keep formid
                    dstFormId = BlogBodyController.ProcessForm(cp, app, legacyRequest, RetryCommentPost)
                End If
                '
                ' -- Get Next Form
                result = GetForm(cp, app, legacyRequest, dstFormId, RetryCommentPost)
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
        Private Shared Function GetForm(cp As CPBaseClass, app As ApplicationController, request As View.RequestModel, dstFormID As Integer, RetryCommentPost As Boolean) As String
            Dim result As New StringBuilder()
            Try
                Select Case dstFormID
                    Case FormBlogPostDetails
                        result.Append(ArticleView.getArticleView(cp, app, request, RetryCommentPost))
                    Case FormBlogArchiveDateList
                        result.Append(ArchiveView.GetFormBlogArchiveDateList(cp, app, request))
                    Case FormBlogArchivedBlogs
                        result.Append(ArchiveView.GetFormBlogArchivedBlogs(cp, app, request))
                    Case FormBlogEntryEditor
                        result.Append(EditView.GetFormBlogEdit(cp, app, request))
                    Case FormBlogSearch
                        result.Append(SearchView.GetFormBlogSearch(cp, app, request))
                    Case Else
                        If (app.blogEntry IsNot Nothing) Then
                            '
                            ' Go to details page
                            '
                            result.Append(ArticleView.getArticleView(cp, app, request, RetryCommentPost))
                        Else
                            '
                            ' list all the entries
                            '
                            result.Append(ListView.GetListView(cp, app, request))
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
