Imports System.Linq
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    '
    Public Class BlogEntryCellView
        '
        '====================================================================================
        '
        Public Shared Function getBlogPostCell(cp As CPBaseClass, app As ApplicationEnvironmentModel, blogPost As BlogPostModel, isArticleView As Boolean, IsSearchListing As Boolean, Return_CommentCnt As Integer, entryEditLink As String) As String
            Dim hint As Integer = 0
            Try
                Dim result As String = ""
                hint = 10
                '
                ' -- argument test
                If (blogPost Is Nothing) Then Throw New ApplicationException("BlogEntryCell called without valid BlogEntry")
                '
                ' -- add link alias for this page
                Dim qs As String = cp.Utils.ModifyQueryString("", RequestNameBlogEntryID, CStr(blogPost.id))
                qs = cp.Utils.ModifyQueryString(qs, rnFormID, FormBlogPostDetails.ToString())
                Call cp.Site.AddLinkAlias(blogPost.name, cp.Doc.PageId, qs)
                Dim entryLink As String = cp.Content.GetPageLink(cp.Doc.PageId, qs)
                Dim blogImageList = BlogImageModel.createListFromBlogEntry(cp, blogPost.id)
                hint = 20
                Dim TagListRow As String = ""
                If isArticleView Then
                    hint = 30
                    '
                    ' -- article view
                    result &= "<h1 class=""aoBlogEntryName"">" & blogPost.name & "</h1>"
                    result &= "<div class=""aoBlogEntryLikeLine"">" & cp.Addon.Execute(facebookLikeAddonGuid) & "</div>"
                    result &= "<div class=""aoBlogEntryCopy"">"
                    If (blogImageList.Count > 0) Then
                        hint = 40
                        Dim ThumbnailFilename As String = ""
                        Dim imageFilename As String = ""
                        Dim imageName As String = ""
                        Dim imageDescription As String = ""
                        BlogImageView.getBlogImage(cp, app, blogImageList.First, ThumbnailFilename, imageFilename, imageDescription, imageName)
                        Select Case blogPost.primaryImagePositionId
                            Case 2
                                '
                                ' align right
                                result &= "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:40%;"">"
                            Case 3
                                '
                                ' align left
                                result &= "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:40%;"">"
                            Case 4
                                '
                                ' hide
                            Case Else
                                '
                                ' 1 and none align per stylesheet
                                result &= "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:40%;"">"
                        End Select
                    End If
                    hint = 50
                    result &= blogPost.copy & "</div>"
                    If Not String.IsNullOrEmpty(blogPost.tagList) Then
                        hint = 60
                        '
                        ' -- make a clickable section
                        Dim clickableLinkList As String = ""
                        Dim tags() As String = Split(Replace(blogPost.tagList, ",", vbCrLf), vbCrLf)
                        For Each tag In tags
                            Dim Link As String = app.blogBaseLink & "?" & rnFormID & "=" & FormBlogSearch & "&" & rnQueryTag & "=" & cp.Utils.EncodeHTML(tag)
                            clickableLinkList &= ", <a href=""" & Link & """>" & tag & "</a>"
                        Next
                        TagListRow = "" _
                            & "<div class=""aoBlogTagListSection"">" _
                            & "<div class=""aoBlogTagListHeader"">Tags</div>" _
                            & "<div class=""aoBlogTagList"">" & Mid(clickableLinkList, 3) & "</div>" _
                            & "</div>"
                    End If
                Else
                    hint = 70
                    '
                    ' -- list view
                    result &= vbCrLf & entryEditLink & "<h4 class=""aoBlogEntryName""><a href=""" & entryLink & """>" & blogPost.name & "</a></h4>"
                    result &= "<div class=""aoBlogEntryCopy"">"
                    If (blogImageList.Count > 0) Then
                        hint = 80
                        Dim ThumbnailFilename As String = ""
                        Dim imageFilename As String = ""
                        Dim imageName As String = ""
                        Dim imageDescription As String = ""
                        BlogImageView.getBlogImage(cp, app, blogImageList.First, ThumbnailFilename, imageFilename, imageDescription, imageName)
                        If ThumbnailFilename <> "" Then
                            Select Case blogPost.primaryImagePositionId
                                Case 2
                                    '
                                    ' align right
                                    '
                                    'result &=  "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result &= "<a href=""" & entryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:25%;""></a>"
                                Case 3
                                    '
                                    ' align left
                                    '
                                    ' result &=  "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result &= "<a href=""" & entryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:25%;""></a>"
                                Case 4
                                    '
                                    ' hide
                                    '
                                Case Else
                                    '
                                    ' 1 and none align per stylesheet
                                    '
                                    'result &=  "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result &= "<a href=""" & entryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:25%;""></a>"

                            End Select
                        End If
                    End If
                    result &= "<p>" & genericController.getBriefCopy(cp, blogPost.copy, app.blog.OverviewLength) & "</p></div>"
                    result &= "<div class=""aoBlogEntryReadMore""><a href=""" & entryLink & """>Read More</a></div>"
                End If
                hint = 90
                '
                ' Podcast link
                '
                If blogPost.PodcastMediaLink <> "" Then
                    cp.Doc.SetProperty("Media Link", blogPost.PodcastMediaLink)
                    cp.Doc.SetProperty("Media Link", blogPost.PodcastSize.ToString())
                    cp.Doc.SetProperty("Hide Player", "True")
                    cp.Doc.SetProperty("Auto Start", "False")
                    '
                    result &= cp.Addon.Execute(addonGuidWebcast)
                End If
                hint = 100
                '
                ' Author Row
                '
                Dim RowCopy As String = ""
                If (blogPost.AuthorMemberID = 0) And (blogPost.CreatedBy > 0) Then
                    blogPost.AuthorMemberID = blogPost.CreatedBy
                    blogPost.save(Of BlogPostModel)(cp)
                End If
                Dim author = DbModel.create(Of PersonModel)(cp, blogPost.AuthorMemberID)
                If (author IsNot Nothing) Then
                    RowCopy &= "By " & author.name
                    If blogPost.DateAdded <> Date.MinValue Then
                        RowCopy &= " | " & blogPost.DateAdded
                    End If
                Else
                    If blogPost.DateAdded <> Date.MinValue Then
                        RowCopy &= blogPost.DateAdded
                    End If
                End If
                Dim visit As VisitModel = VisitModel.create(cp, cp.Visit.Id)
                If blogPost.AllowComments And ((visit IsNot Nothing) AndAlso (cp.Visit.CookieSupport And (Not visit.Bot()))) Then
                    hint = 110
                    '
                    ' Show comment count
                    Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(Approved<>0)and(EntryID=" & blogPost.id & ")")
                    If isArticleView Then
                        If BlogCommentModelList.Count = 1 Then
                            RowCopy &= " | 1 Comment"
                        ElseIf BlogCommentModelList.Count > 1 Then
                            RowCopy &= " | " & BlogCommentModelList.Count & " Comments&nbsp;(" & BlogCommentModelList.Count & ")"
                        End If
                    Else
                        qs = app.blogBaseLink
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogPost.id))
                        qs = cp.Utils.ModifyQueryString(qs, rnFormID, FormBlogPostDetails.ToString())
                        If BlogCommentModelList.Count = 0 Then
                            RowCopy &= " | " & "<a href=""" & entryLink & """>Comment</a>"
                        Else
                            RowCopy &= " | " & "<a href=""" & entryLink & """>Comments</a>&nbsp;(" & BlogCommentModelList.Count & ")"
                        End If
                    End If
                End If
                If Not String.IsNullOrEmpty(RowCopy) Then
                    result &= "<div class=""aoBlogEntryByLine"">Posted " & RowCopy & "</div>"
                End If
                hint = 120
                '
                ' Tag List Row
                '
                If TagListRow <> "" Then
                    result &= TagListRow
                End If
                Dim toolLine As String = ""
                Dim CommentPtr As Integer
                If blogPost.AllowComments And (cp.Visit.CookieSupport) And ((visit IsNot Nothing) AndAlso (Not visit.Bot)) Then
                    hint = 130
                    '
                    ' --
                    If Not isArticleView Then
                        hint = 140
                        '
                        ' Show comment count
                        '
                        Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(Approved<>0)and(EntryID=" & blogPost.id & ")")
                        '
                        qs = app.blogBaseLink
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogPost.id))
                        qs = cp.Utils.ModifyQueryString(qs, rnFormID, FormBlogPostDetails.ToString())
                        Dim CommentLine As String = ""
                        If BlogCommentModelList.Count = 0 Then
                            CommentLine = CommentLine & "<a href=""?" & qs & """>Comment</a>"
                        Else
                            CommentLine = CommentLine & "<a href=""?" & qs & """>Comments</a>&nbsp;(" & BlogCommentModelList.Count & ")"
                        End If

                        'get the unapproved comments
                        If app.user.isBlogEditor(cp, app.blog) Then
                            hint = 150
                            Dim BlogUnapprovedCommentModelList = DbModel.createList(Of BlogCommentModel)(cp, "(Approved=0)and(EntryID=" & blogPost.id & ")")
                            Dim unapprovedCommentCount = BlogUnapprovedCommentModelList.Count
                            If toolLine <> "" Then
                                toolLine = toolLine & "&nbsp;|&nbsp;"
                            End If
                            toolLine = toolLine & "Unapproved Comments (" & unapprovedCommentCount & ")"
                            qs = app.blogBaseLink
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogPost.id))
                            qs = cp.Utils.ModifyQueryString(qs, rnFormID, FormBlogEntryEditor.ToString())
                            If toolLine <> "" Then
                                toolLine = toolLine & "&nbsp;|&nbsp;"
                            End If
                            toolLine = toolLine & "<a href=""?" & qs & """>Edit</a>"
                        End If
                    Else
                        hint = 160
                        '
                        ' Show all comments
                        '
                        Dim Criteria As String = "(EntryID=" & blogPost.id & ")"
                        If Not app.user.isBlogEditor(cp, app.blog) Then
                            '
                            ' non-owner - just approved comments plus your own comments
                            '
                            Criteria &= "and((Approved<>0)or(AuthorMemberID=" & cp.User.Id & "))"
                        End If
                        Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, Criteria, "DateAdded")
                        If (BlogCommentModelList.Count > 0) Then
                            Dim Divider As String = "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                            result &= "<div class=""aoBlogCommentHeader"">Comments</div>"
                            result &= vbCrLf & Divider
                            CommentPtr = 0
                            For Each blogComment In DbModel.createList(Of BlogCommentModel)(cp, Criteria)

                                result &= BlogCommentCellView.getBlogCommentCell(cp, app.blog, blogPost, blogComment, app.user, False)
                                result &= vbCrLf & Divider
                                CommentPtr += 1
                            Next
                        End If
                    End If
                End If
                hint = 170
                '
                If toolLine <> "" Then
                    result &= "<div class=""aoBlogToolLink"">" & toolLine & "</div>"
                End If
                result &= vbCrLf & cp.Html.Hidden("CommentCnt" & blogPost.id, CommentPtr.ToString())
                '
                Return_CommentCnt = CommentPtr
                getBlogPostCell = result

                hint = 999
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "hint " & hint)
                Throw
            End Try
        End Function
    End Class
End Namespace
