
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
    Public Class BlogCommentCellView
        '
        '====================================================================================
        '
        Public Shared Function getBlogCommentCell(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, blogComment As BlogCommentModel, user As PersonModel, IsSearchListing As Boolean) As String
            Dim result As String = ""
            Try
                Dim Copy As String
                Dim RowCopy As String
                Dim qs As String
                '
                If IsSearchListing Then
                    qs = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList.ToString())
                    result = result & cr & "<div class=""aoBlogEntryName"">Comment to Blog Post " & blogEntry.name & ", <a href=""?" & qs & """>View this post</a></div>"
                    result = result & cr & "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                End If
                result = result & cr & "<div class=""aoBlogCommentName"">" & cp.Utils.EncodeHTML(blogComment.name) & "</div>"
                Copy = blogComment.CopyText
                Copy = cp.Utils.EncodeHTML(Copy)
                Copy = Replace(Copy, vbCrLf, "<BR />")
                result = result & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>"
                RowCopy = ""
                If (True) Then
                    Dim author = DbModel.create(Of PersonModel)(cp, blogEntry.AuthorMemberID)
                    If (author IsNot Nothing) AndAlso (author.name <> "") Then
                        RowCopy = RowCopy & "by " & cp.Utils.EncodeHTML(author.name)
                        If blogComment.DateAdded <> Date.MinValue Then
                            RowCopy = RowCopy & " | " & blogComment.DateAdded
                        End If
                    Else
                        If blogComment.DateAdded <> Date.MinValue Then
                            RowCopy = RowCopy & blogComment.DateAdded
                        End If
                    End If
                End If
                '
                If user.isBlogEditor(cp, blog) Then
                    '
                    ' Blog owner Approval checkbox
                    '
                    If RowCopy <> "" Then
                        RowCopy = RowCopy & " | "
                    End If
                    RowCopy = RowCopy _
                        & cp.Html.Hidden("CommentID", blogComment.id.ToString()) _
                        & cp.Html.CheckBox("Approve", blogComment.Approved) _
                        & cp.Html.Hidden("Approved", blogComment.Approved.ToString()) _
                        & "&nbsp;Approved&nbsp;" _
                        & " | " _
                        & cp.Html.CheckBox("Delete", False) _
                        & "&nbsp;Delete" _
                        & ""
                End If
                If RowCopy <> "" Then
                    result = result & cr & "<div class=""aoBlogCommentByLine"">Posted " & RowCopy & "</div>"
                End If
                '
                If (Not blogComment.Approved) And (Not user.isBlogEditor(cp, blog)) And (blogComment.AuthorMemberID = cp.User.Id) Then
                    result = "<div style=""border:1px solid red;padding-top:10px;padding-bottom:10px;""><span class=""aoBlogCommentName"" style=""color:red;"">Your comment pending approval</span><br />" & result & "</div>"
                End If
                '
                getBlogCommentCell = result
                'GetBlogCommentCell = cr & "<div class=""aoBlogComment"">" & s & cr & "</div>"
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '
    End Class
End Namespace
