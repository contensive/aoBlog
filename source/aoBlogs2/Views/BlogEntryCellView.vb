
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
    Public Class BlogEntryCellView
        '
        '====================================================================================
        '
        Public Shared Function getBlogEntryCell(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, user As PersonModel, isArticleView As Boolean, IsSearchListing As Boolean, Return_CommentCnt As Integer, entryEditLink As String, blogListQs As String) As String
            Dim result As String = ""
            Try
                If (blogEntry Is Nothing) Then Throw New ApplicationException("BlogEntryCell called without valid BlogEntry")
                '
                Dim qs As String = cp.Utils.ModifyQueryString("", RequestNameBlogEntryID, CStr(blogEntry.id))
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
                Call cp.Site.addLinkAlias(blogEntry.name, cp.Doc.PageId, qs)
                Dim entryLink As String = cp.Content.GetPageLink(cp.Doc.PageId, qs)
                Dim TagListRow As String = ""
                Dim blogImageList = BlogImageModel.createListFromBlogEntry(cp, blogEntry.id)
                If isArticleView Then
                    '
                    ' -- article view
                    result = result & vbCrLf & entryEditLink & "<h2 class=""aoBlogEntryName"">" & blogEntry.name & "</h2>"
                    result &= "<div class=""aoBlogEntryLikeLine"">" & cp.Utils.ExecuteAddon(facebookLikeAddonGuid) & "</div>"
                    result = result & cr & "<div class=""aoBlogEntryCopy"">"
                    If (blogImageList.Count > 0) Then
                        Dim ThumbnailFilename As String = ""
                        Dim imageFilename As String = ""
                        Dim imageName As String = ""
                        Dim imageDescription As String = ""
                        BlogImageView.getBlogImage(cp, blog, rssFeed, blogEntry, blogImageList.First, ThumbnailFilename, imageFilename, imageDescription, imageName)
                        Select Case blogEntry.primaryImagePositionId
                            Case 2
                                '
                                ' align right
                                result = result & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:40%;"">"
                            Case 3
                                '
                                ' align left
                                result = result & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:40%;"">"
                            Case 4
                                '
                                ' hide
                            Case Else
                                '
                                ' 1 and none align per stylesheet
                                result = result & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:40%;"">"
                        End Select
                    End If
                    result = result & blogEntry.copy & "</div>"
                    qs = blogListQs
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor.ToString())
                    Dim c As String = ""
                    'If (blogEntry.imageDisplayTypeId = imageDisplayTypeList) And (blogImageList.Count > 0) Then
                    '    '
                    '    ' Get ImageID List
                    '    For Each blogImage In blogImageList
                    '        Dim ThumbnailFilename As String = ""
                    '        Dim imageFilename As String = ""
                    '        Dim imageName As String = ""
                    '        Dim imageDescription As String = ""
                    '        GetBlogImage(cp, blog, rssFeed, blogEntry, blogImageList.First, ThumbnailFilename, imageFilename, imageDescription, imageName)
                    '        If imageFilename <> "" Then
                    '            c = c _
                    '            & cr & "<div class=""aoBlogEntryImageContainer"">" _
                    '            & cr & "<img alt=""" & imageName & """ title=""" & imageName & """  src=""" & cp.Site.FilePath & imageFilename & """>"
                    '            If imageName <> "" Then
                    '                c = c & cr & "<h2>" & imageName & "</h2>"
                    '            End If
                    '            If imageDescription <> "" Then
                    '                c = c & cr & "<div>" & imageDescription & "</div>"
                    '            End If
                    '            c = c & cr & "</div>"
                    '        End If
                    '    Next
                    '    If c <> "" Then
                    '        result = result _
                    '        & cr & "<div class=""aoBlogEntryImageSection"">" _
                    '        & cp.Html.Indent(c) _
                    '        & cr & "</div>"
                    '    End If
                    'End If
                    If blogEntry.tagList <> "" Then
                        '
                        ' -- make a clickable section
                        Dim clickableLinkList As String = Replace(blogEntry.tagList, ",", vbCrLf)
                        Dim Tags() As String = Split(clickableLinkList, vbCrLf)
                        clickableLinkList = ""
                        Dim SQS As String
                        SQS = cp.Utils.ModifyQueryString(blogListQs, RequestNameFormID, FormBlogSearch.ToString(), True)
                        Dim Ptr As Integer
                        For Ptr = 0 To UBound(Tags)
                            'QS = cp.Utils.ModifyQueryString(SQS, RequestNameFormID, FormBlogSearch, True)
                            qs = cp.Utils.ModifyQueryString(SQS, RequestNameQueryTag, Tags(Ptr), True)
                            Dim Link As String = "?" & qs
                            clickableLinkList = clickableLinkList & ", " & "<a href=""" & Link & """>" & Tags(Ptr) & "</a>"
                        Next
                        clickableLinkList = Mid(clickableLinkList, 3)
                        c = "" _
                        & cr & "<div class=""aoBlogTagListHeader"">" _
                        & cr & vbTab & "Tags" _
                        & cr & "</div>" _
                        & cr & "<div class=""aoBlogTagList"">" _
                        & cr & vbTab & clickableLinkList _
                        & cr & "</div>"
                        TagListRow = "" _
                        & cr & "<div class=""aoBlogTagListSection"">" _
                        & cp.Html.Indent(c) _
                        & cr & "</div>"
                    End If
                Else
                    '
                    ' -- list view
                    result = result & vbCrLf & entryEditLink & "<h4 class=""aoBlogEntryName""><a href=""" & entryLink & """>" & blogEntry.name & "</a></h4>"
                    result = result & cr & "<div class=""aoBlogEntryCopy"">"
                    If (blogImageList.Count > 0) Then
                        Dim ThumbnailFilename As String = ""
                        Dim imageFilename As String = ""
                        Dim imageName As String = ""
                        Dim imageDescription As String = ""
                        BlogImageView.getBlogImage(cp, blog, rssFeed, blogEntry, blogImageList.First, ThumbnailFilename, imageFilename, imageDescription, imageName)
                        If ThumbnailFilename <> "" Then
                            Select Case blogEntry.primaryImagePositionId
                                Case 2
                                    '
                                    ' align right
                                    '
                                    'result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result = result & "<a href=""" & entryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:25%;""></a>"
                                Case 3
                                    '
                                    ' align left
                                    '
                                    ' result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result = result & "<a href=""" & entryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:25%;""></a>"
                                Case 4
                                    '
                                    ' hide
                                    '
                                Case Else
                                    '
                                    ' 1 and none align per stylesheet
                                    '
                                    'result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result = result & "<a href=""" & entryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:25%;""></a>"

                            End Select
                        End If
                    End If
                    result = result & "<p>" & genericController.getBriefCopy(cp, blogEntry.copy, blog.OverviewLength) & "</p></div>"
                    result = result & cr & "<div class=""aoBlogEntryReadMore""><a href=""" & entryLink & """>Read More</a></div>"
                End If
                '
                ' Podcast link
                '
                If blogEntry.PodcastMediaLink <> "" Then
                    cp.Doc.SetProperty("Media Link", blogEntry.PodcastMediaLink)
                    cp.Doc.SetProperty("Media Link", blogEntry.PodcastSize.ToString())
                    cp.Doc.SetProperty("Hide Player", "True")
                    cp.Doc.SetProperty("Auto Start", "False")
                    '
                    result = result & cp.Utils.ExecuteAddon(addonGuidWebcast)
                End If
                '
                ' Author Row
                '
                Dim RowCopy As String = ""
                Dim AuthorMemberID As Integer = blogEntry.AuthorMemberID
                If AuthorMemberID = 0 Then
                    AuthorMemberID = blogEntry.CreatedBy
                End If
                Dim author = DbModel.create(Of PersonModel)(cp, AuthorMemberID)
                If (author IsNot Nothing) Then
                    RowCopy = RowCopy & "By " & author.name
                    If blogEntry.DateAdded <> Date.MinValue Then
                        RowCopy = RowCopy & " | " & blogEntry.DateAdded
                    End If
                Else
                    If blogEntry.DateAdded <> Date.MinValue Then
                        RowCopy = RowCopy & blogEntry.DateAdded
                    End If
                End If
                Dim CommentCount As Integer
                Dim Criteria As String
                Dim visit As VisitModel = VisitModel.create(cp, cp.Visit.Id)
                If blogEntry.AllowComments And (cp.Visit.CookieSupport) And (Not visit.Bot()) Then
                    '
                    ' Show comment count
                    Criteria = "(Approved<>0)and(EntryID=" & blogEntry.id & ")"
                    Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(Approved<>0)and(EntryID=" & blogEntry.id & ")")
                    CommentCount = BlogCommentModelList.Count
                    If isArticleView Then
                        If CommentCount = 1 Then
                            RowCopy = RowCopy & " | 1 Comment"
                        ElseIf CommentCount > 1 Then
                            RowCopy = RowCopy & " | " & CommentCount & " Comments&nbsp;(" & CommentCount & ")"
                        End If
                    Else
                        qs = blogListQs
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
                        If CommentCount = 0 Then
                            RowCopy = RowCopy & " | " & "<a href=""" & entryLink & """>Comment</a>"
                        Else
                            RowCopy = RowCopy & " | " & "<a href=""" & entryLink & """>Comments</a>&nbsp;(" & CommentCount & ")"
                        End If
                    End If
                End If
                If RowCopy <> "" Then
                    result = result & cr & "<div class=""aoBlogEntryByLine"">Posted " & RowCopy & "</div>"
                End If
                '
                ' Tag List Row
                '
                If TagListRow <> "" Then
                    result = result & TagListRow
                End If
                Dim ToolLine As String = ""
                Dim CommentPtr As Integer
                If blogEntry.AllowComments And (cp.Visit.CookieSupport) And (Not visit.Bot) Then


                    'If blogEntry.AllowComments Then
                    If Not isArticleView Then
                        ''
                        '' Show comment count
                        ''
                        Criteria = "(Approved<>0)and(EntryID=" & blogEntry.id & ")"
                        Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(Approved<>0)and(EntryID=" & blogEntry.id & ")")
                        CommentCount = BlogCommentModelList.Count
                        '
                        qs = blogListQs
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
                        Dim CommentLine As String = ""
                        If CommentCount = 0 Then
                            CommentLine = CommentLine & "<a href=""?" & qs & """>Comment</a>"
                        Else
                            CommentLine = CommentLine & "<a href=""?" & qs & """>Comments</a>&nbsp;(" & CommentCount & ")"
                        End If

                        'get the unapproved comments
                        If user.isBlogEditor(cp, blog) Then
                            Criteria = "(EntryID=" & blogEntry.id & ")"
                            Dim BlogUnapprovedCommentModelList = DbModel.createList(Of BlogCommentModel)(cp, "(Approved=0)and(EntryID=" & blogEntry.id & ")")
                            Dim unapprovedCommentCount = BlogUnapprovedCommentModelList.Count
                            If ToolLine <> "" Then
                                ToolLine = ToolLine & "&nbsp;|&nbsp;"
                            End If
                            ToolLine = ToolLine & "Unapproved Comments (" & unapprovedCommentCount & ")"
                            qs = blogListQs
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor.ToString())
                            If ToolLine <> "" Then
                                ToolLine = ToolLine & "&nbsp;|&nbsp;"
                            End If
                            ToolLine = ToolLine & "<a href=""?" & qs & """>Edit</a>"
                        End If

                        '
                    Else
                        '
                        ' Show all comments
                        '
                        'hint =  hint & ",11"
                        If user.isBlogEditor(cp, blog) Then
                            '
                            ' Owner - Show all comments
                            '
                            Criteria = "(EntryID=" & blogEntry.id & ")"
                        Else
                            '
                            ' non-owner - just approved comments plus your own comments
                            '
                            Criteria = "((Approved<>0)or(AuthorMemberID=" & cp.User.Id & "))and(EntryID=" & blogEntry.id & ")"
                        End If
                        Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, Criteria, "DateAdded")
                        If (BlogCommentModelList.Count > 0) Then
                            Dim Divider As String = "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                            result = result & cr & "<div class=""aoBlogCommentHeader"">Comments</div>"
                            result = result & vbCrLf & Divider
                            CommentPtr = 0
                            For Each blogComment In DbModel.createList(Of BlogCommentModel)(cp, Criteria)

                                result = result & BlogCommentCellView.getBlogCommentCell(cp, blog, rssFeed, blogEntry, blogComment, user, False)
                                result = result & vbCrLf & Divider
                                CommentPtr = CommentPtr + 1
                            Next

                        End If

                    End If
                End If
                '
                'hint =  hint & ",12"
                If ToolLine <> "" Then
                    result = result & cr & "<div class=""aoBlogToolLink"">" & ToolLine & "</div>"
                End If
                result = result & vbCrLf & cp.Html.Hidden("CommentCnt" & blogEntry.id, CommentPtr.ToString())
                '
                Return_CommentCnt = CommentPtr
                getBlogEntryCell = result
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
