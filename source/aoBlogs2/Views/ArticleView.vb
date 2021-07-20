
Imports System.Runtime.CompilerServices
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    '
    Public Class ArticleView
        '
        '====================================================================================
        '
        Public Shared Function getArticleView(cp As CPBaseClass, app As ApplicationController, request As View.RequestModel, RetryCommentPost As Boolean) As String
            Dim hint As Integer = 0
            Try
                If app.blogEntry Is Nothing Then
                    '
                    ' -- This article cannot be found
                    Return "<div class=""aoBlogProblem"">Sorry, the blog post you selected is not currently available</div>"
                End If
                '
                ' -- count the viewing
                cp.Db.ExecuteNonQuery("update " & BlogPostModel.contentTableName & " set viewings=" & (app.blogEntry.Viewings + 1))
                hint = 10
                '
                Dim result As String = ""
                '
                ' setup form key
                Dim formKey As String = cp.Utils.CreateGuid()
                result &= cp.Html.Hidden("FormKey", formKey)
                result &= "<div class=""aoBlogHeaderLink""><a href=""" & app.blogPageBaseLink & """>" & BackToRecentPostsMsg & "</a></div>"
                hint = 20
                '
                Dim entryEditLink As String = If(app.userIsEditing, cp.Content.GetEditLink(BlogModel.contentName, app.blogEntry.id.ToString(), False, app.blogEntry.name, True), "")
                '
                ' Print the Blog Entry
                Dim return_CommentCnt As Integer
                result &= BlogEntryCellView.getBlogPostCell(cp, app, app.blogEntry, True, False, return_CommentCnt, entryEditLink)
                '
                Dim visit As VisitModel = VisitModel.create(cp, cp.Visit.Id)
                If (visit IsNot Nothing) Then
                    If (Not visit.ExcludeFromAnalytics) Then
                        Dim blogEntryId As Integer = If(app.blogEntry IsNot Nothing, app.blogEntry.id, 0)
                        Dim BlogViewingLog As BlogViewingLogModel = DbModel.add(Of BlogViewingLogModel)(cp)
                        If (BlogViewingLog IsNot Nothing) Then
                            BlogViewingLog.name = cp.User.Name & ", post " & CStr(blogEntryId) & ", " & Now()
                            BlogViewingLog.BlogEntryID = blogEntryId
                            BlogViewingLog.MemberID = cp.User.Id
                            BlogViewingLog.VisitID = cp.Visit.Id
                            BlogViewingLog.save(Of BlogModel)(cp)
                        End If
                    End If
                End If
                hint = 50
                '
                If app.user.isBlogEditor(cp, app.blog) And (return_CommentCnt > 0) Then
                    result &= "<div class=""aoBlogCommentCopy"">" & cp.Html.Button(FormButtonApplyCommentChanges) & "</div>"
                End If
                '
                hint = 60
                Dim qs As String
                If app.blogEntry.AllowComments And (cp.Visit.CookieSupport) And (Not visit.Bot()) Then
                    hint = 70
                    result &= "<div class=""aoBlogCommentHeader"">Post a Comment</div>"
                    '
                    If Not (cp.UserError.OK()) Then
                        result &= "<div class=""aoBlogCommentError"">" & (cp.UserError.OK()) & "</div>"
                    End If
                    '
                    If (Not app.blog.AllowAnonymous) And (Not cp.User.IsAuthenticated) Then
                        hint = 80
                        Dim AllowPasswordEmail As Boolean = cp.Site.GetBoolean("AllowPasswordEmail", False)
                        Dim AllowMemberJoin As Boolean = cp.Site.GetBoolean("AllowMemberJoin", False)
                        '
                        Dim Auth As Integer = cp.Doc.GetInteger("auth")
                        If (Auth = 1) And (Not AllowPasswordEmail) Then
                            Auth = 3
                        ElseIf (Auth = 2) And (Not AllowMemberJoin) Then
                            Auth = 3
                        End If
                        Call cp.Doc.AddRefreshQueryString(RequestNameFormID, FormBlogPostDetails.ToString())
                        Call cp.Doc.AddRefreshQueryString(RequestNameBlogEntryID, app.blogEntry.id.ToString())
                        Call cp.Doc.AddRefreshQueryString("auth", "0")
                        qs = cp.Doc.RefreshQueryString()
                        Dim Copy As String
                        hint = 90
                        Select Case Auth
                            Case 1
                                hint = 100
                                '
                                ' password email
                                '
                                Copy = "To retrieve your username and password, submit your email. "
                                qs = cp.Utils.ModifyQueryString(qs, "auth", "0")
                                Copy = Copy & " <a href=""?" & qs & """> Login?</a>"
                                If AllowMemberJoin Then
                                    qs = cp.Utils.ModifyQueryString(qs, "auth", "2")
                                    Copy = Copy & " <a href=""?" & qs & """> Join?</a>"
                                End If
                                result = result _
                                    & "<div class=""aoBlogLoginBox"">" _
                                    & vbCrLf & vbTab & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                                    & vbCrLf & vbTab & "<div class=""aoBlogCommentCopy"">" & "send password form removed" & "</div>" _
                                    & "</div>"
                            Case 2
                                hint = 110
                                '
                                ' join
                                '
                                Copy = "To post a comment to this blog, complete this form. "
                                qs = cp.Utils.ModifyQueryString(qs, "auth", "0")
                                Copy = Copy & " <a href=""?" & qs & """> Login?</a>"
                                If AllowPasswordEmail Then
                                    qs = cp.Utils.ModifyQueryString(qs, "auth", "1")
                                    Copy = Copy & " <a href=""?" & qs & """> Forget your username or password?</a>"
                                End If
                                result = result _
                                    & "<div class=""aoBlogLoginBox"">" _
                                    & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                                    & "<div class=""aoBlogCommentCopy"">" & "Send join form removed" & "</div>" _
                                    & "</div>"
                            Case Else
                                hint = 120
                                '
                                ' login
                                '
                                Copy = "To post a comment to this Blog, please login."
                                If AllowMemberJoin Then
                                    qs = cp.Utils.ModifyQueryString(qs, "auth", "2")
                                    Copy = Copy & "<div class=""aoBlogRegisterLink""><a href=""?" & qs & """>Need to Register?</a></div>"
                                End If
                                result = result _
                                    & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                                    & "</div>"
                        End Select
                        hint = 140
                    Else
                        hint = 150
                        result &= "<div>&nbsp;</div>"
                        result &= "<div class=""aoBlogCommentCopy"">Title</div>"
                        If RetryCommentPost Then
                            hint = 160
                            result &= "<div class=""aoBlogCommentCopy"">" & genericController.getField(cp, RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(RequestNameCommentTitle.ToString)) & "</div>"
                            result &= "<div>&nbsp;</div>"
                            result &= "<div class=""aoBlogCommentCopy"">Comment</div>"
                            result &= "<div class=""aoBlogCommentCopy"">" & cp.Html5.InputTextArea(RequestNameCommentCopy, 500, cp.Doc.GetText(RequestNameCommentCopy)) & "</div>"
                        Else
                            hint = 170
                            result &= "<div class=""aoBlogCommentCopy"">" & genericController.getField(cp, RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(RequestNameCommentTitle.ToString)) & "</div>"
                            result &= "<div>&nbsp;</div>"
                            result &= "<div class=""aoBlogCommentCopy"">Comment</div>"
                            result &= "<div class=""aoBlogCommentCopy"">" & cp.Html5.InputTextArea(RequestNameCommentCopy, 500, cp.Doc.GetText(RequestNameCommentCopy)) & "</div>"
                        End If
                        '
                        ' todo re-enable recaptcha 20190123
                        hint = 180
                        If app.blog.recaptcha Then
                            result &= "<div class=""aoBlogCommentCopy"">Verify Text</div>"
                            result &= "<div class=""aoBlogCommentCopy"">" & cp.Addon.Execute(reCaptchaDisplayGuid) & "</div>"
                        End If
                        '
                        result &= "<div class=""aoBlogCommentCopy"">" & cp.Html.Button(rnButton, FormButtonPostComment) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel) & "</div>"
                    End If
                End If
                hint = 190
                '
                result &= "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                '
                ' edit link
                '
                hint = 200
                If app.user.isBlogEditor(cp, app.blog) Then
                    qs = cp.Doc.RefreshQueryString()
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, app.blogEntry.id)
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor)
                    result &= "<div class=""aoBlogToolLink""><a href=""?" & qs & """>Edit</a></div>"
                End If
                '
                ' Search
                '
                'hint = 210
                'qs = cp.Doc.RefreshQueryString
                'qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch, True)
                'result &= "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                '
                ' back to recent posts
                result &= "<div class=""aoBlogFooterLink""><a href=""" & app.blogPageBaseLink & """>" & BackToRecentPostsMsg & "</a></div>"
                '
                result &= vbCrLf & cp.Html5.Hidden(RequestNameSourceFormID, FormBlogPostDetails)
                result &= vbCrLf & cp.Html5.Hidden(RequestNameBlogEntryID, app.blogEntry.id)
                result &= vbCrLf & cp.Html5.Hidden("EntryCnt", 1)
                getArticleView = result
                result = cp.Html.Form(getArticleView)
                '
                hint = 220
                Call cp.Visit.SetProperty(SNBlogCommentName, CStr(cp.Utils.GetRandomInteger()))
                '
                ' -- set metadata
                hint = 230
                MetadataController.setMetadata(cp, app.blogEntry)
                '
                ' -- if editing enabled, add the link and wrapperwrapper
                hint = 240
                result = genericController.addEditWrapper(cp, result, app.blogEntry.id, app.blogEntry.name, Models.BlogPostModel.contentName)
                '
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Hint [" & hint & "]")
                Throw
            End Try
        End Function
        '
        '====================================================================================
        '
        Public Shared Function processArticleView(cp As CPBaseClass, app As ApplicationController, request As View.RequestModel, ByRef RetryCommentPost As Boolean) As Integer
            Dim result As Integer
            Try
                Dim blog As BlogModel = app.blog
                Dim blogEntry As BlogPostModel = app.blogEntry
                Dim user As PersonModel = app.user
                '
                result = request.SourceFormID
                Dim SN As String = cp.Visit.GetText(SNBlogCommentName)
                '
                If SN = "" Then
                    '
                    ' Process out of order, go to main
                    '
                    result = FormBlogPostList
                Else
                    Dim Copy As String
                    Dim formKey As String
                    Dim CommentID As Integer
                    If request.ButtonValue = FormButtonCancel Then
                        '
                        ' Cancel button, go to main
                        '
                        result = FormBlogPostList
                    ElseIf request.ButtonValue = FormButtonPostComment Then
                        ' todo re-enable recaptcha 20190123
                        If blog.recaptcha Then
                            '
                            ' Process recaptcha
                            '
                            Dim optionStr As String = "Challenge=" + cp.Doc.GetText("recaptcha_challenge_field")
                            optionStr = optionStr & "&Response=" + cp.Doc.GetText("recaptcha_response_field")
                            Dim WrapperId As Integer = Nothing
                            Dim captchaResponse As String = cp.Addon.Execute(reCaptchaProcessGuid)
                            If captchaResponse <> "" Then
                                Call cp.UserError.Add("The verify text you entered did not match correctly. Please try again.")
                            End If
                        End If
                        '
                        ' Process comment post
                        '
                        RetryCommentPost = True
                        formKey = cp.Doc.GetText("formkey")
                        Copy = cp.Doc.GetText(RequestNameCommentCopy)
                        If Copy <> "" Then
                            Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(formkey=" & cp.Db.EncodeSQLText(formKey) & ")", "ID")
                            If (BlogCommentModelList.Count <> 0) Then
                                Call cp.UserError.Add("<p>This comment has already been accepted.</p>")
                                RetryCommentPost = False
                            Else
                                'Dim EntryID = cp.Doc.GetInteger(RequestNameBlogEntryID)
                                'Dim BlogEntry As BlogEntryModel = DbModel.create(Of BlogEntryModel)(cp, EntryID)
                                Dim BlogComment As BlogCommentModel = DbModel.add(Of BlogCommentModel)(cp)
                                BlogComment.BlogID = blog.id
                                BlogComment.Active = True
                                BlogComment.name = cp.Doc.GetText(RequestNameCommentTitle)
                                BlogComment.CopyText = Copy
                                BlogComment.EntryID = blogEntry.id
                                BlogComment.Approved = user.isBlogEditor(cp, blog) Or blog.autoApproveComments
                                BlogComment.FormKey = formKey
                                BlogComment.save(Of BlogCommentModel)(cp)
                                CommentID = BlogComment.id
                                RetryCommentPost = False
                                '
                                If (blog.emailComment) Then
                                    '
                                    ' Send Comment Notification
                                    Dim EntryLink As String = blogEntry.RSSLink
                                    If InStr(1, EntryLink, "?") = 0 Then
                                        EntryLink = EntryLink & "?"
                                    Else
                                        EntryLink = EntryLink & "&"
                                    End If
                                    EntryLink = EntryLink & "blogentryid=" & blogEntry.id
                                    Dim EmailBody As String = "" _
                                        & "The following blog comment was posted " & Now() _
                                        & "To approve this comment, go to " & EntryLink _
                                        & vbCrLf _
                                        & "Blog '" & blog.name & "'" _
                                        & "Post '" & blogEntry.name & "'" _
                                        & "By " & cp.User.Name _
                                        & vbCrLf _
                                        & vbCrLf & cp.Utils.EncodeHTML(Copy) _
                                        & vbCrLf
                                    Dim EmailFromAddress As String = cp.Site.GetText("EmailFromAddress", "info@" & cp.Site.Domain)

                                    If blogEntry.AuthorMemberID <> 0 Then
                                        Call cp.Email.sendUser(blogEntry.AuthorMemberID, EmailFromAddress, "Blog comment notification for [" & blog.name & "]", EmailBody, True, False)
                                        Call cp.Email.sendUser(blogEntry.AuthorMemberID, EmailFromAddress, "Blog comment notification for [" & blog.name & "]", EmailBody, False, False)
                                    End If

                                    'If blog.AuthoringGroupID <> 0 Then
                                    'Dim MemberRuleList As List(Of MemberRuleModel) = DbModel.createList(Of MemberRuleModel)(cp, "GroupId=" & blog.AuthoringGroupID)
                                    'For Each MemberRule In MemberRuleList
                                    'Call cp.Email.sendUser(MemberRule.MemberID.ToString(), EmailFromAddress, "Blog comment on " & blog.name, EmailBody, False, False)
                                    'Next
                                    'End If
                                    Dim blogAuthorsGroupId As Integer = cp.Group.GetId("Blog Authors")
                                    If blogAuthorsGroupId <> 0 Then
                                        Dim MemberRuleList As List(Of MemberRuleModel) = DbModel.createList(Of MemberRuleModel)(cp, "GroupId=" & blogAuthorsGroupId)
                                        For Each MemberRule In MemberRuleList
                                            Call cp.Email.sendUser(MemberRule.MemberID, EmailFromAddress, "Blog comment on " & blog.name, EmailBody, False, False)
                                        Next
                                    End If
                                End If

                            End If
                        End If
                        result = FormBlogPostDetails
                    ElseIf request.ButtonValue = FormButtonApplyCommentChanges Then
                        '
                        ' Post approval changes if the person is the owner
                        '
                        If user.isBlogEditor(cp, blog) Then
                            Dim EntryCnt As Integer = cp.Doc.GetInteger("EntryCnt")
                            If EntryCnt > 0 Then
                                Dim EntryPtr As Integer
                                For EntryPtr = 0 To EntryCnt - 1
                                    Dim CommentCnt As Integer = cp.Doc.GetInteger("CommentCnt" & EntryPtr)
                                    If CommentCnt > 0 Then
                                        Dim CommentPtr As Integer
                                        For CommentPtr = 0 To CommentCnt - 1
                                            Dim Suffix As String = EntryPtr & "." & CommentPtr
                                            CommentID = cp.Doc.GetInteger("CommentID" & Suffix)
                                            If cp.Doc.GetBoolean("Delete" & Suffix) Then
                                                '
                                                ' Delete comment
                                                '
                                                Call cp.Content.Delete("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blog.id & ")")
                                            ElseIf cp.Doc.GetBoolean("Approve" & Suffix) And Not cp.Doc.GetBoolean("Approved" & Suffix) Then
                                                '
                                                ' Approve Comment
                                                '
                                                Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(name=" & cp.Utils.EncodeRequestVariable(blog.name) & ")", "ID")
                                                If (BlogCommentModelList.Count > 0) Then
                                                    Dim BlogComment As BlogCommentModel = DbModel.add(Of BlogCommentModel)(cp)
                                                    If cp.CSNew.OK() Then
                                                        BlogComment.Approved = True
                                                    End If
                                                ElseIf Not cp.Doc.GetBoolean("Approve" & Suffix) And cp.Doc.GetBoolean("Approved" & Suffix) Then
                                                    '
                                                    ' Unapprove comment
                                                    '
                                                    Dim BlogComment As BlogCommentModel = DbModel.add(Of BlogCommentModel)(cp)
                                                    If (BlogComment IsNot Nothing) Then
                                                        BlogComment.Approved = False
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                Next
                            End If
                        End If
                        '            '
                    End If
                    Call cp.Visit.SetProperty(SNBlogCommentName, "")
                End If
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
    End Class
End Namespace
