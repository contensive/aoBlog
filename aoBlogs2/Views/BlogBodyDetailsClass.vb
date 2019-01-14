
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
    Public Class BlogBodyDetailsClass
        '
        '====================================================================================
        '
        Public Shared Function GetBlogBodyDetails(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, blogListQs As String, RetryCommentPost As Boolean) As String
            '
            Dim result As String = ""
            Try
                'Dim EntryCopyOverview As String
                Dim BlogTagList As String
                Dim entryEditLink As String
                Dim formKey As String
                Dim Return_CommentCnt As Integer
                Dim Copy As String
                Dim DateAdded As Date
                Dim AuthorMemberID As Integer
                Dim EntryName As String
                Dim EntryCopy As String
                Dim allowComments As Boolean
                Dim EntryPtr As Integer
                Dim qs As String
                Dim CommentCnt As Integer
                Dim imageDisplayTypeId As Integer
                Dim primaryImagePositionId As Integer
                Dim articlePrimaryImagePositionId As Integer
                '
                Call cp.Site.TestPoint("blog -- getFormBlogPostDetails, enter ")
                Call cp.Site.TestPoint("blog -- getFormBlogPostDetails, blogEntry.recaptcha=[" & blog.recaptcha & "]")
                '
                ' setup form key
                '
                formKey = "{" & Guid.NewGuid().ToString() & "}" ' cp.Utils.enc  Main.EncodeKeyNumber(Main.VisitID, Now())
                result = vbCrLf & cp.Html.Hidden("FormKey", formKey)
                result = result & cr & "<div class=""aoBlogHeaderLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                '
                ' Print the Blog Entry
                CommentCnt = 0
                If (blogEntry IsNot Nothing) Then

                    'Dim BlogEntry As BlogEntryModel = blogEntryList.First
                    If Not (blogEntry IsNot Nothing) Then
                        result = result & cr & "<div class=""aoBlogProblem"">Sorry, the blog post you selected is not currently available</div>"
                    Else
                        AuthorMemberID = blogEntry.AuthorMemberID
                        If AuthorMemberID = 0 Then
                            AuthorMemberID = blogEntry.CreatedBy
                        End If
                        DateAdded = blogEntry.DateAdded
                        EntryName = blogEntry.name
                        If cp.User.IsAuthoring("Blogs") Then
                            entryEditLink = cp.Content.GetEditLink(EntryName, blogEntry.id.ToString(), True, EntryName, True)
                        End If
                        EntryCopy = blogEntry.copy

                        allowComments = blogEntry.AllowComments
                        BlogTagList = blogEntry.tagList
                        qs = ""
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
                        blogEntry.Viewings = (1 + cp.Doc.GetInteger("viewings"))
                        blogEntry.imageDisplayTypeId = cp.Doc.GetInteger("imageDisplayTypeId")
                        blogEntry.primaryImagePositionId = cp.Doc.GetInteger("primaryImagePositionId")
                        ' blogEntry.articlePrimaryImagePositionId = cp.Doc.GetInteger("articlePrimaryImagePositionId")
                        result = result & genericController.GetBlogEntryCell(cp, blog, rssFeed, blogEntry, user, True, False, Return_CommentCnt, "", blogListQs)
                        EntryPtr = EntryPtr + 1
                        '
                        blog.id = cp.Doc.GetInteger("BlogID")
                    End If

                    '
                End If
                '
                Dim criteria As String = ""
                Dim VisitModel As New VisitModel
                Dim excludeFromAnalytics As Boolean = VisitModel.ExcludeFromAnalytics
                If (Not excludeFromAnalytics) Then
                    Dim BlogViewingLog As BlogViewingLogModel = DbModel.add(Of BlogViewingLogModel)(cp)
                    If (BlogViewingLog IsNot Nothing) Then
                        BlogViewingLog.name = cp.User.Name & ", post " & CStr(blogEntry.id) & ", " & Now()
                        BlogViewingLog.BlogEntryID = blogEntry.id
                        BlogViewingLog.MemberID = cp.User.Id
                        BlogViewingLog.VisitID = cp.Visit.Id
                        BlogViewingLog.save(Of BlogModel)(cp)
                    End If
                End If
                '
                If user.isBlogEditor(cp, blog) And (Return_CommentCnt > 0) Then
                    result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.Button(FormButtonApplyCommentChanges) & "</div>"
                End If
                '
                Dim Auth As Integer
                Dim AllowPasswordEmail As Boolean
                Dim AllowMemberJoin As Boolean
                '
                If allowComments And (cp.Visit.CookieSupport) And (Not VisitModel.Bot()) Then
                    result = result & cr & "<div class=""aoBlogCommentHeader"">Post a Comment</div>"
                    '
                    If Not (cp.UserError.OK()) Then
                        result = result & "<div class=""aoBlogCommentError"">" & (cp.UserError.OK()) & "</div>"
                    End If
                    '

                    If (Not blog.AllowAnonymous) And (Not cp.User.IsAuthenticated) Then
                        AllowPasswordEmail = cp.Utils.EncodeBoolean(cp.Site.GetProperty("AllowPasswordEmail", "0"))
                        AllowMemberJoin = cp.Utils.EncodeBoolean(cp.Site.GetProperty("AllowMemberJoin", "0"))
                        Auth = cp.Doc.GetInteger("auth")
                        If (Auth = 1) And (Not AllowPasswordEmail) Then
                            Auth = 3
                        ElseIf (Auth = 2) And (Not AllowMemberJoin) Then
                            Auth = 3
                        End If
                        Call cp.Doc.AddRefreshQueryString(RequestNameFormID, FormBlogPostDetails.ToString())
                        Call cp.Doc.AddRefreshQueryString(RequestNameBlogEntryID, blogEntry.id.ToString())
                        Call cp.Doc.AddRefreshQueryString("auth", "0")
                        qs = cp.Doc.RefreshQueryString()
                        Select Case Auth
                            Case 1
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
                                        & cr & "</div>"
                            Case 2
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
                                        & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                                        & cr & "<div class=""aoBlogCommentCopy"">" & "Send join form removed" & "</div>" _
                                        & cr & "</div>"
                            Case Else
                                '
                                ' login
                                '
                                Copy = "To post a comment to this Blog, please login."
                                'If AllowPasswordEmail Then
                                '    qs = cp.Utils.ModifyQueryString(qs, "auth", "1")
                                '    Copy = Copy & " <a href=""?" & qs & """> Forget your username or password?</a>"
                                'End If
                                If AllowMemberJoin Then
                                    qs = cp.Utils.ModifyQueryString(qs, "auth", "2")
                                    Copy = Copy & "<div class=""aoBlogRegisterLink""><a href=""?" & qs & """>Need to Register?</a></div>"
                                End If
                                'loginForm = "Get login Form removed"
                                'loginForm = Replace(loginForm, "LoginUsernameInput", "LoginUsernameInput-BlockFocus")
                                result = result _
                                        & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                                         & cr & "</div>"
                                '& cr & "<div class=""aoBlogLoginBox"">" _
                                '& cr & "<div class=""aoBlogCommentCopy"">" & loginForm & "</div>" _
                                '& cr & "</div>"
                        End Select

                    Else
                        result = result & cr & "<div>&nbsp;</div>"
                        result = result & cr & "<div class=""aoBlogCommentCopy"">Title</div>"
                        If RetryCommentPost Then
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & genericController.GetField(cp, RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(RequestNameCommentTitle.ToString)) & "</div>"
                            result = result & cr & "<div>&nbsp;</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">Comment</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.InputText(RequestNameCommentCopy, cp.Doc.GetText(RequestNameCommentCopy), "15", "70",) & "</div>"
                        Else
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & genericController.GetField(cp, RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(RequestNameCommentTitle.ToString)) & "</div>"
                            result = result & cr & "<div>&nbsp;</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">Comment</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.InputText(RequestNameCommentCopy, "", "15", "70") & "</div>"
                        End If
                        's = s & cr & "<div class=""aoBlogCommentCopy"">Verify Text</div>"
                        '
                        If blog.recaptcha Then
                            result = result & cr & "<div class=""aoBlogCommentCopy"">Verify Text</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Utils.ExecuteAddon(reCaptchaDisplayGuid) & "</div>"
                            Call cp.Site.TestPoint("output - reCaptchaDisplayGuid")
                        End If
                        '
                        result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.Button(rnButton, FormButtonPostComment) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel) & "</div>"
                    End If

                End If

                result = result & cr & "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                '
                ' edit link
                '
                If user.isBlogEditor(cp, blog) Then
                    qs = cp.Doc.RefreshQueryString()
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor.ToString())
                    result = result & cr & "<div class=""aoBlogToolLink""><a href=""?" & qs & """>Edit</a></div>"
                End If
                '
                ' Search
                '
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch.ToString(), True)
                result = result & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                '
                ' back to recent posts
                '
                'QSBack = cp.Doc.RefreshQueryString()
                'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameBlogEntryID, "", True)
                'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameFormID, FormBlogPostList)
                result = result & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                '
                result = result & vbCrLf & cp.Html.Hidden(RequestNameSourceFormID, FormBlogPostDetails.ToString())
                result = result & vbCrLf & cp.Html.Hidden(RequestNameBlogEntryID, blogEntry.id.ToString())
                result = result & vbCrLf & cp.Html.Hidden("EntryCnt", EntryPtr.ToString())
                'result = result & "</div>"
                GetBlogBodyDetails = result
                result = cp.Html.Form(GetBlogBodyDetails)
                '
                '
                Try
                    Call cp.Visit.SetProperty(SNBlogCommentName, CStr(cp.Utils.GetRandomInteger()))
                Catch ex As Exception

                End Try

                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================
        '
        Public Shared Function ProcessBlogBodyDetails(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, request As View.RequestModel, user As PersonModel, ByRef RetryCommentPost As Boolean) As Integer
            Dim result As Integer
            Try
                Dim MemberID As Integer
                Dim MemberName As String
                Dim EntryLink As String
                Dim EmailBody As String
                Dim EmailFromAddress As String
                Dim EntryCnt As Integer
                Dim EntryPtr As Integer
                Dim CommentCnt As Integer
                Dim CommentPtr As Integer
                Dim Suffix As String
                Dim Copy As String
                Dim CommentID As Integer
                Dim SN As String
                'Dim EntryID As Integer
                Dim formKey As String
                Dim optionStr As String
                Dim captchaResponse As String
                '
                result = request.SourceFormID
                SN = cp.Visit.GetProperty(SNBlogCommentName)
                '
                If SN = "" Then
                    '
                    ' Process out of order, go to main
                    '
                    result = FormBlogPostList
                Else
                    If request.ButtonValue = FormButtonCancel Then
                        '
                        ' Cancel button, go to main
                        '
                        result = FormBlogPostList
                    ElseIf request.ButtonValue = FormButtonPostComment Then
                        If blog.recaptcha Then
                            '
                            ' Process recaptcha
                            '
                            optionStr = "Challenge=" + cp.Doc.GetText("recaptcha_challenge_field")
                            optionStr = optionStr & "&Response=" + cp.Doc.GetText("recaptcha_response_field")
                            Dim WrapperId As Integer = Nothing
                            captchaResponse = cp.Utils.ExecuteAddon(reCaptchaProcessGuid)
                            Call cp.Site.TestPoint("output - reCaptchaProcessGuid, result=" & captchaResponse)
                            If captchaResponse <> "" Then
                                Call cp.UserError.Add("The verify text you entered did not match correctly. Please try again.")
                                Call cp.Utils.AppendLog("testpoint1")
                            End If
                        End If
                        '
                        ' Process comment post
                        '
                        RetryCommentPost = True
                        formKey = cp.Doc.GetText("formkey")
                        'formKey = Main.DecodeKeyNumber(formKey)
                        Copy = cp.Doc.GetText(RequestNameCommentCopy)
                        If Copy <> "" Then
                            Call cp.Site.TestPoint("blog -- adding comment [" & Copy & "]")
                            'If (Main.VisitID <> kmaEncodeInteger(Main.DecodeKeyNumber(formKey))) Then
                            '    Call Main.AddUserError("<p>This comment has already been accepted.</p>")
                            'End If
                            Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(formkey=" & cp.Db.EncodeSQLText(formKey) & ")", "ID")
                            If (BlogCommentModelList.Count <> 0) Then
                                Call cp.UserError.Add("<p>This comment has already been accepted.</p>")
                                RetryCommentPost = False
                                Call cp.Utils.AppendLog("testpoint2")
                            Else
                                Call cp.Site.TestPoint("blog -- adding comment, no user error")
                                Dim EntryID = cp.Doc.GetInteger(RequestNameBlogEntryID)
                                Dim BlogEntry As BlogEntryModel = DbModel.create(Of BlogEntryModel)(cp, EntryID)
                                Dim BlogComment As BlogCommentModel = DbModel.add(Of BlogCommentModel)(cp)
                                'CSP = Main.InsertCSRecord(cnBlogComments)
                                If blog.AllowAnonymous And (Not cp.User.IsAuthenticated) Then
                                    MemberID = 0
                                    MemberName = AnonymousMemberName
                                Else
                                    MemberID = cp.User.Id
                                    MemberName = cp.User.Name
                                End If
                                cp.Utils.AppendLog("test1")
                                BlogComment.BlogID = blog.id
                                BlogComment.Active = True
                                BlogComment.name = cp.Doc.GetText(RequestNameCommentTitle)
                                BlogComment.CopyText = Copy
                                BlogComment.EntryID = EntryID
                                BlogComment.Approved = user.isBlogEditor(cp, blog) Or blog.autoApproveComments
                                BlogComment.FormKey = formKey
                                BlogComment.save(Of BlogCommentModel)(cp)
                                CommentID = BlogComment.id
                                RetryCommentPost = False
                                '
                                cp.Utils.AppendLog("test2")
                                If (blog.emailComment) Then
                                    '
                                    ' Send Comment Notification
                                    EntryLink = BlogEntry.RSSLink
                                    If InStr(1, EntryLink, "?") = 0 Then
                                        EntryLink = EntryLink & "?"
                                    Else
                                        EntryLink = EntryLink & "&"
                                    End If
                                    EntryLink = EntryLink & "blogentryid=" & EntryID
                                    EmailBody = "" _
                                                    & cr & "The following blog comment was posted " & Now() _
                                                    & cr & "To approve this comment, go to " & EntryLink _
                                                    & vbCrLf _
                                                    & cr & "Blog '" & blog.name & "'" _
                                                    & cr & "Post '" & BlogEntry.name & "'" _
                                                    & cr & "By " & cp.User.Name _
                                                    & vbCrLf _
                                                    & vbCrLf & cp.Utils.EncodeHTML(Copy) _
                                                    & vbCrLf
                                    EmailFromAddress = cp.Site.GetProperty("EmailFromAddress", "info@" & cp.Site.Domain)
                                    'Call Main.SendMemberEmail(BlogOwnerID, EmailFromAddress, "Blog comment notification for [" & blog.name & "]", EmailBody, False, False)
                                    Call cp.Email.sendUser(BlogEntry.AuthorMemberID.ToString(), EmailFromAddress, "Blog comment notification for [" & blog.name & "]", EmailBody, False, False)
                                    If blog.AuthoringGroupID <> 0 Then
                                        Dim MemberRuleList As List(Of MemberRuleModel) = DbModel.createList(Of MemberRuleModel)(cp, "GroupId=" & blog.AuthoringGroupID)
                                        For Each MemberRule In MemberRuleList
                                            Call cp.Email.sendUser(MemberRule.MemberID.ToString(), EmailFromAddress, "Blog comment on " & blog.name, EmailBody, False, False)
                                        Next
                                    End If
                                End If

                            End If
                        End If
                        result = FormBlogPostDetails
                        'result = FormBlogPostList
                    ElseIf request.ButtonValue = FormButtonApplyCommentChanges Then
                        '
                        ' Post approval changes if the person is the owner
                        '
                        If user.isBlogEditor(cp, blog) Then
                            EntryCnt = cp.Doc.GetInteger("EntryCnt")
                            If EntryCnt > 0 Then
                                For EntryPtr = 0 To EntryCnt - 1
                                    CommentCnt = cp.Doc.GetInteger("CommentCnt" & EntryPtr)
                                    If CommentCnt > 0 Then
                                        For CommentPtr = 0 To CommentCnt - 1
                                            Suffix = EntryPtr & "." & CommentPtr
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
                                                Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(name=" & cp.Utils.EncodeQueryString(blog.name) & ")", "ID")
                                                If (BlogCommentModelList.Count > 0) Then
                                                    ' CS = Main.OpenCSContent("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blog.id & ")")
                                                    Dim BlogComment As BlogCommentModel = DbModel.add(Of BlogCommentModel)(cp)
                                                    If cp.CSNew.OK() Then
                                                        BlogComment.Approved = True
                                                        ' Call Main.SetCS(CS, "Approved", True)
                                                    End If
                                                    'Call Main.CloseCS(CS)
                                                ElseIf Not cp.Doc.GetBoolean("Approve" & Suffix) And cp.Doc.GetBoolean("Approved" & Suffix) Then
                                                    '
                                                    ' Unapprove comment
                                                    '
                                                    'CS = Main.OpenCSContent("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blog.id & ")")
                                                    Dim BlogComment As BlogCommentModel = DbModel.add(Of BlogCommentModel)(cp)
                                                    If (BlogComment IsNot Nothing) Then
                                                        'Call Main.SetCS(CS, "Approved", True)
                                                        BlogComment.Approved = False
                                                        'Call Main.SetCS(CS, "Approved", False)
                                                    End If
                                                    'Call Main.CloseCS(CS)
                                                End If
                                            End If
                                        Next
                                    End If
                                Next
                            End If
                        End If
                        '            '
                        ' CommentInfo = "Submitted at " & Now() & " by " & AuthorName
                        '  EmailString = "<br>"
                        ' EmailString = EmailString & CommentInfo & "<br>"
                        'EmailString = EmailString & "<a href=""http://" & Main.ServerHost & Main.ServerAppRootPath & "admin/index.asp?cid=" & Main.GetContentID(cnBlogEntries) & "&id=" & CommentID & "&af=4"">click here</a>"
                        ' Call Main.SendSystemEmail(SystemEmailCommentNotification, EmailString)
                        'test good
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
