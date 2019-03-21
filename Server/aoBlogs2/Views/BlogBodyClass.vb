
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
    Public Class BlogBodyClass
        '
        '========================================================================
        ''' <summary>
        ''' Inner Blog - the list of posts without sidebar. Blog object must be valid
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="blog"></param>
        ''' <returns></returns>
        Public Function getBlogBody(cp As CPBaseClass, blog As BlogModel, request As View.RequestModel) As String
            Dim result As String = ""
            Try
                If (blog Is Nothing) Then Throw New ApplicationException("BlogBody requires valid Blog object and blog object is null.")
                If (blog.id = 0) Then Throw New ApplicationException("BlogBody requires valid Blog object and blog.id is 0.")
                '
                Dim user = PersonModel.create(cp, cp.User.Id)
                '
                Dim qsBlogList As String = cp.Doc.RefreshQueryString()
                qsBlogList = cp.Utils.ModifyQueryString(qsBlogList, RequestNameSourceFormID, "")
                qsBlogList = cp.Utils.ModifyQueryString(qsBlogList, RequestNameFormID, "")
                qsBlogList = cp.Utils.ModifyQueryString(qsBlogList, RequestNameBlogCategoryID, "")
                qsBlogList = cp.Utils.ModifyQueryString(qsBlogList, RequestNameBlogEntryID, "")
                Dim blogPageBaseLink As String = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", "")
                '
                '
                ' Get the Feed Args
                '
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
                Dim blogEntry = DbModel.create(Of BlogEntryModel)(cp, request.EntryID)
                Dim RetryCommentPost As Boolean = False
                Dim dstFormId As Integer = request.FormID
                If request.ButtonValue <> "" Then
                    '
                    ' Process the source form into form if there was a button - else keep formid
                    dstFormId = ProcessForm(cp, blog, RSSFeed, blogEntry, request, user, blogPageBaseLink, RetryCommentPost)
                End If
                '
                ' -- Get Next Form
                Return GetForm(cp, blog, RSSFeed, blogEntry, request, user, dstFormId, blogPageBaseLink, qsBlogList, RetryCommentPost)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Return String.Empty
            End Try
        End Function
        '
        '====================================================================================
        '
        Private Function GetForm(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, FormID As Integer, blogListLink As String, blogListQs As String, RetryCommentPost As Boolean) As String
            Dim result As New StringBuilder()
            Try
                Select Case FormID
                    Case FormBlogPostDetails
                        result.Append(BlogBodyDetailsClass.GetBlogBodyDetails(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs, RetryCommentPost))
                    Case FormBlogArchiveDateList
                        result.Append(GetFormBlogArchiveDateList(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs))
                    Case FormBlogArchivedBlogs
                        result.Append(GetFormBlogArchivedBlogs(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs))
                    Case FormBlogEntryEditor
                        result.Append(BlogBodyEditClass.GetFormBlogEdit(cp, blog, rssFeed, blogEntry, request, user, blogListLink))
                    Case FormBlogSearch
                        result.Append(BlogBodySearchClass.GetFormBlogSearch(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs))
                    Case Else
                        If (blogEntry IsNot Nothing) Then
                            '
                            ' Go to details page
                            '
                            result.Append(BlogBodyDetailsClass.GetBlogBodyDetails(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs, RetryCommentPost))
                        Else
                            '
                            ' list all the entries
                            '
                            result.Append(GetFormBlogPostList(cp, blog, rssFeed, request, user, blogListLink, blogListQs))
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
        Private Function ProcessForm(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, ByRef RetryCommentPost As Boolean) As Integer
            '
            Try
                If request.ButtonValue <> "" Then
                    Select Case request.SourceFormID
                        Case FormBlogPostList
                            ProcessForm = FormBlogPostList
                        Case FormBlogEntryEditor
                            ProcessForm = BlogBodyEditClass.ProcessFormBlogEdit(cp, blog, rssFeed, blogEntry, request, blogListLink)
                        Case FormBlogPostDetails
                            ProcessForm = BlogBodyDetailsClass.ProcessBlogBodyDetails(cp, blog, rssFeed, request, user, RetryCommentPost)
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
        Private Function GetFormBlogArchiveDateList(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, blogListQs As String) As String
            '
            Dim result As String = ""
            Try
                Dim OpenSQL As String = ""
                Dim contentControlId As String = cp.Content.GetID(cnBlogEntries).ToString
                '
                cp.Utils.AppendLog("entering GetFormBlogArchiveDateList")
                result = vbCrLf & cp.Content.GetCopy("Blogs Archives Header for " & blog.name, "<h2>" & blog.name & " Blog Archive</h2>")
                ' 
                'Dim BlogCopyModelList As List(Of BlogCopyModel) = BlogCopyModel.createList(cp, "(blogid=" & blog.id & ")", "year(dateadded) desc, Month(DateAdded) desc")
                Dim archiveDateList As List(Of BlogCopyModel.ArchiveDateModel) = BlogCopyModel.createArchiveListFromBlogCopy(cp, blog.id)
                cp.Utils.AppendLog("open archive date list")
                If (archiveDateList.Count = 0) Then
                    '
                    ' No archives, give them an error
                    '
                    result = result & cr & "<div class=""aoBlogProblem"">There are no current blog entries</div>"
                    result = result & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                Else
                    'Dim RowCnt As Integer = BlogCopyModelList.Count
                    Dim ArchiveMonth As Integer
                    Dim ArchiveYear As Integer
                    If archiveDateList.Count = 1 Then
                        '
                        ' one archive - just display it
                        cp.Utils.AppendLog("display one archive count 1")
                        '
                        ArchiveMonth = archiveDateList.First.Month
                        ArchiveYear = archiveDateList.First.Year
                        result = result & GetFormBlogArchivedBlogs(cp, blog, rssFeed, blogEntry, request, user, blogListLink, blogListQs)
                        cp.Utils.AppendLog("after display one archive count 1")
                    Else
                        '
                        cp.Utils.AppendLog("enter display list of archive")
                        ' Display List of archive
                        '
                        Dim qs As String = cp.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, FormBlogArchiveDateList.ToString())
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchivedBlogs.ToString())
                        For Each archiveDate In archiveDateList
                            ArchiveMonth = archiveDate.Month
                            ArchiveYear = archiveDate.Year
                            Dim NameOfMonth As String = MonthName(ArchiveMonth)
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveMonth, CStr(ArchiveMonth))
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveYear, CStr(ArchiveYear))
                            '
                            result = result & vbCrLf & vbTab & vbTab & "<div class=""aoBlogArchiveLink""><a href=""?" & qs & """>" & NameOfMonth & " " & ArchiveYear & "</a></div>"
                            's = s & vbCrLf & vbTab & vbTab & "<div class=""aoBlogArchiveLink""><a href=""?" & qs & RequestNameFormID & "=" & FormBlogArchivedBlogs & "&" & RequestNameArchiveMonth & "=" & ArchiveMonth & "&" & RequestNameArchiveYear & "=" & ArchiveYear & "&" & RequestNameSourceFormID & "=" & FormBlogArchiveDateList & """>" & NameOfMonth & " " & ArchiveYear & "</a></div>"

                        Next
                        cp.Utils.AppendLog("exit display list of archive")
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
        Private Function GetFormBlogArchivedBlogs(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String, blogListQs As String) As String
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
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogPostList(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, request As View.RequestModel, user As PersonModel, blogListLink As String, blogListQs As String) As String
            Dim result As New StringBuilder()
            Try
                If blog.Caption <> "" Then
                    result.Append(vbCrLf & "<h2 Class=""aoBlogCaption"">" & blog.Caption & "</h2>")
                End If
                If blog.Copy <> "" Then
                    result.Append(vbCrLf & "<div Class=""aoBlogDescription"">" & blog.Copy & "</div>")
                End If
                '
                ' Display the most recent entries
                cp.Utils.AppendLog(" GetFormBlogPostList 300")
                Dim BlogEntryList As List(Of BlogEntryModel)
                Dim NoneMsg As String
                Dim blogCategory = DbModel.create(Of Models.BlogCategorieModel)(cp, request.categoryId)
                If (blogCategory IsNot Nothing) Then
                    BlogEntryList = DbModel.createList(Of BlogEntryModel)(cp, "(BlogID=" & blog.id & ")And(BlogCategoryID=" & request.categoryId & ")", "DateAdded Desc")
                    result.Append(cr & "<div Class=""aoBlogCategoryCaption"">Category " & blogCategory.name & "</div>")
                    NoneMsg = "There are no blog posts available in the category " & blogCategory.name
                Else
                    BlogEntryList = DbModel.createList(Of BlogEntryModel)(cp, "(BlogID=" & blog.id & ")", "DateAdded Desc")
                    NoneMsg = "There are no blog posts available"
                End If

                Dim isBlogCategoryBlockedDict As New Dictionary(Of Integer, Boolean)
                If (BlogEntryList.Count.Equals(0)) Then
                    '
                    result.Append(cr & "<div Class=""aoBlogProblem"">" & NoneMsg & "</div>")
                Else
                    Dim blogCategoryList = DbModel.createList(Of BlogCategorieModel)(cp, "")
                    Dim EntryPtr As Integer = 0
                    For Each blogEntry In BlogEntryList
                        If (EntryPtr >= blog.PostsToDisplay) Then Exit For
                        Dim IsBlocked As Boolean = False
                        If (isBlogCategoryBlockedDict.ContainsKey(blogEntry.blogCategoryID)) Then
                            IsBlocked = isBlogCategoryBlockedDict(blogEntry.blogCategoryID)
                        Else
                            Dim blogEntryCategory = DbModel.create(Of Models.BlogCategorieModel)(cp, blogEntry.blogCategoryID)
                            If (blogEntryCategory IsNot Nothing) Then
                                If (blogEntryCategory.UserBlocking) Then IsBlocked = Not genericController.IsGroupListMember(cp, GroupModel.GetBlockingGroups(cp, blogEntryCategory.id))
                                isBlogCategoryBlockedDict.Add(blogEntry.blogCategoryID, IsBlocked)
                            End If
                        End If
                        If Not IsBlocked Then
                            Dim entryEditLink As String

                            If (cp.User.IsAuthoring("Blogs")) Then
                                entryEditLink = cp.Content.GetEditLink("Blog Entries", blogEntry.id.ToString(), True, blogEntry.name, True)
                            End If
                            Dim Return_CommentCnt As Integer
                            result.Append(genericController.getBlogEntryCell(cp, blog, rssFeed, blogEntry, user, False, True, Return_CommentCnt, "", blogListQs))
                            result.Append(cr & "<div Class=""aoBlogEntryDivider"">&nbsp;</div>")
                        End If
                        EntryPtr += 1
                    Next
                End If
                Dim CategoryFooter As String = ""
                Dim qs As String
                '
                ' Build Footers
                If cp.User.IsAdmin() And blog.AllowCategories Then
                    qs = "cid=" & cp.Content.GetID("Blog Categories") & "&af=4"
                    CategoryFooter = CategoryFooter & cr & "<div Class=""aoBlogFooterLink""><a href=""" & cp.Site.GetProperty("ADMINURL") & "?" & qs & """>Add a New category</a></div>"
                End If
                Dim ReturnFooter As String = ""
                If blog.AllowCategories Then
                    qs = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogCategoryIDSet, "0", True)
                    CategoryFooter = CategoryFooter & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>See posts in all categories</a></div>"
                    '
                    ' select a category
                    qs = cp.Doc.RefreshQueryString
                    Dim BlogCategoryList As List(Of BlogCategorieModel) = DbModel.createList(Of BlogCategorieModel)(cp, "")
                    If (BlogCategoryList.Count > 0) Then
                        For Each blogCategaory In BlogCategoryList

                            Dim IsBlocked As Boolean = False
                            If (isBlogCategoryBlockedDict.ContainsKey(blogCategaory.id)) Then
                                IsBlocked = isBlogCategoryBlockedDict(blogCategaory.id)
                            Else
                                Dim blogEntryCategory = DbModel.create(Of Models.BlogCategorieModel)(cp, blogCategaory.id)
                                If (blogEntryCategory IsNot Nothing) Then
                                    If (blogEntryCategory.UserBlocking) Then IsBlocked = Not genericController.IsGroupListMember(cp, GroupModel.GetBlockingGroups(cp, blogEntryCategory.id))
                                    isBlogCategoryBlockedDict.Add(blogCategaory.id, IsBlocked)
                                End If
                            End If
                            If Not IsBlocked Then
                                Dim categoryLink As String = cp.Utils.ModifyQueryString(qs, RequestNameBlogCategoryIDSet, CStr(blogCategaory.id), True)
                                CategoryFooter = CategoryFooter & cr & "<div class=""aoBlogFooterLink""><a href=""?" & categoryLink & """> See posts in the category " & blogCategaory.name & "</a></div>"
                            End If
                        Next
                    End If
                End If
                '
                ' Footer
                result.Append(cr & "<div>&nbsp;</div>")
                If user.isBlogEditor(cp, blog) Then
                    '
                    ' Create a new entry if this is the Blog Owner
                    '
                    qs = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor.ToString(), True)
                    result.Append(cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Create new Blog Post</a></div>")
                    '
                    ' Create a link to edit the blog record
                    '
                    qs = "cid=" & cp.Content.GetID("Blogs") & "&af=4&id=" & blog.id
                    result.Append(cr & "<div class=""aoBlogFooterLink""><a href=""" & cp.Site.GetProperty("adminUrl") & "?" & qs & """>Blog Settings</a></div>")
                    '
                    ' Create a link to edit the rss record
                    '
                    If rssFeed.id = 0 Then

                    Else
                        qs = "cid=" & cp.Content.GetID("RSS Feeds") & "&af=4&id=" & rssFeed.id
                        result.Append(cr & "<div class=""aoBlogFooterLink""><a href=""" & cp.Site.GetProperty("adminUrl") & "?" & qs & """>Edit rss feed features</a></div>")
                    End If
                End If
                result.Append(ReturnFooter)
                result.Append(CategoryFooter)
                '
                ' Search
                '
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch.ToString(), True)
                result.Append(cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>")
                '
                ' Link to archives
                '
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchiveDateList.ToString(), True)
                result.Append(cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>See archives</a></div>")
                '
                ' Link to RSS Feed
                '
                Dim FeedFooter As String = ""
                If rssFeed.rssFilename <> "" Then
                    FeedFooter = "<a href=""http://" & cp.Site.Domain & "/RSS/" & rssFeed.rssFilename & """>"
                    FeedFooter = "rss feed " _
                        & FeedFooter & rssFeed.name & "</a>" _
                        & "&nbsp;" _
                        & FeedFooter & "<img src=""/cclib/images/IconXML-25x13.gif"" width=25 height=13 class=""aoBlogRSSFeedImage""></a>" _
                        & ""
                    result.Append(cr & "<div class=""aoBlogFooterLink"">" & FeedFooter & "</div>")
                End If
                result.Append(cp.Html.Hidden(RequestNameSourceFormID, FormBlogPostList.ToString()))
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result.ToString()
        End Function
    End Class
End Namespace
