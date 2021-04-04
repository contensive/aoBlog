Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    '
    Public NotInheritable Class BlogCommentCellView
        '
        '====================================================================================
        '
        Public Shared Function getBlogCommentCell(cp As CPBaseClass, blog As BlogModel, blogEntry As BlogPostModel, blogComment As BlogCommentModel, user As PersonModel, IsSearchListing As Boolean) As String
            Try
                Dim result As String = ""
                '
                If IsSearchListing Then
                    Dim qs As String = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList.ToString())
                    result &= "<div class=""aoBlogEntryName"">Comment to Blog Post " & blogEntry.name & ", <a href=""?" & qs & """>View this post</a></div>"
                    result &= "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                End If
                result &= "<div class=""aoBlogCommentName"">" & cp.Utils.EncodeHTML(blogComment.name) & "</div>"
                Dim Copy As String = blogComment.CopyText
                Copy = cp.Utils.EncodeHTML(Copy)
                Copy = Replace(Copy, vbCrLf, "<BR />")
                result &= "<div class=""aoBlogCommentCopy"">" & Copy & "</div>"
                Dim rowCopy As String = ""
                If (True) Then
                    Dim author = DbModel.create(Of PersonModel)(cp, blogEntry.AuthorMemberID)
                    If (author IsNot Nothing) AndAlso (author.name <> "") Then
                        rowCopy &= "by " & cp.Utils.EncodeHTML(author.name)
                        If blogComment.DateAdded <> Date.MinValue Then
                            rowCopy &= " | " & blogComment.DateAdded
                        End If
                    Else
                        If blogComment.DateAdded <> Date.MinValue Then
                            rowCopy &= blogComment.DateAdded
                        End If
                    End If
                End If
                '
                If user.isBlogEditor(cp, blog) Then
                    '
                    ' Blog owner Approval checkbox
                    '
                    If Not String.IsNullOrEmpty(rowCopy) Then
                        rowCopy &= " | "
                    End If
                    rowCopy = rowCopy _
                        & cp.Html.Hidden("CommentID", blogComment.id.ToString()) _
                        & cp.Html.CheckBox("Approve", blogComment.Approved) _
                        & cp.Html.Hidden("Approved", blogComment.Approved.ToString()) _
                        & "&nbsp;Approved&nbsp;" _
                        & " | " _
                        & cp.Html.CheckBox("Delete", False) _
                        & "&nbsp;Delete" _
                        & ""
                End If
                If Not String.IsNullOrEmpty(rowCopy) Then
                    result &= "<div class=""aoBlogCommentByLine"">Posted " & rowCopy & "</div>"
                End If
                '
                If (Not blogComment.Approved) And (Not user.isBlogEditor(cp, blog)) And (blogComment.AuthorMemberID = cp.User.Id) Then
                    result = "<div style=""border:1px solid red;padding-top:10px;padding-bottom:10px;""><span class=""aoBlogCommentName"" style=""color:red;"">Your comment pending approval</span><br />" & result & "</div>"
                End If
                '
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
