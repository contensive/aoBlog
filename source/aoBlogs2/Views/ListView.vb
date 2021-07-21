Imports System.Text
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    '
    Public Class ListView
        '
        '====================================================================================
        '
        Public Shared Function getListView(cp As CPBaseClass, app As ApplicationEnvironmentModel, request As View.RequestModel) As String
            Dim result As New StringBuilder()
            Try
                Dim blog As BlogModel = app.blog
                Dim user As PersonModel = app.user
                Dim rssFeed As RSSFeedModel = app.rssFeed
                If blog.Caption <> "" Then
                    result.Append(vbCrLf & "<h2 Class=""aoBlogCaption"">" & blog.Caption & "</h2>")
                End If
                If blog.Copy <> "" Then
                    result.Append(vbCrLf & "<div Class=""aoBlogDescription"">" & blog.Copy & "</div>")
                End If

                Dim NoneMsg As String = "There are no blog posts available"
                Dim criteria As String = "(BlogID=" & blog.id & ")"
                Dim blogCategory = If(request.categoryId.Equals(0), Nothing, DbModel.create(Of Models.BlogCategoriesModel)(cp, request.categoryId))
                If (blogCategory IsNot Nothing) Then
                    criteria &= "and(BlogCategoryID=" & request.categoryId & ")"
                    result.Append("<div Class=""aoBlogCategoryCaption"">Category " & blogCategory.name & "</div>")
                    NoneMsg = "There are no blog posts available in the category " & blogCategory.name
                End If
                Dim recordCount As Integer = DbModel.getCount(Of BlogPostModel)(cp, criteria)
                Dim pageNumber As Integer = request.page
                If (pageNumber > recordCount) Then pageNumber = recordCount
                If (pageNumber < 1) Then pageNumber = 1
                '
                ' Display the most recent entries
                Dim postList As List(Of BlogPostModel) = DbModel.createList(Of BlogPostModel)(cp, criteria, "DateAdded Desc", blog.postsToDisplay, pageNumber)
                Dim isBlogCategoryBlockedDict As New Dictionary(Of Integer, Boolean)
                Dim recordsOnThisPage As Integer = 0
                If (postList.Count.Equals(0)) Then
                    '
                    result.Append("<div Class=""aoBlogProblem"">" & NoneMsg & "</div>")
                Else
                    Dim blogCategoryList = DbModel.createList(Of BlogCategoriesModel)(cp, "")
                    For Each blogEntry As BlogPostModel In postList
                        If (recordsOnThisPage >= blog.postsToDisplay) Then Exit For
                        Dim IsBlocked As Boolean = False
                        If (isBlogCategoryBlockedDict.ContainsKey(blogEntry.blogCategoryID)) Then
                            IsBlocked = isBlogCategoryBlockedDict(blogEntry.blogCategoryID)
                        Else
                            Dim blogEntryCategory = DbModel.create(Of Models.BlogCategoriesModel)(cp, blogEntry.blogCategoryID)
                            If (blogEntryCategory IsNot Nothing) Then
                                If (blogEntryCategory.UserBlocking) Then IsBlocked = Not genericController.isGroupListMember(cp, GroupModel.GetBlockingGroups(cp, blogEntryCategory.id))
                                isBlogCategoryBlockedDict.Add(blogEntry.blogCategoryID, IsBlocked)
                            End If
                        End If
                        If Not IsBlocked Then
                            Dim Return_CommentCnt As Integer
                            Dim blogArticleCell = BlogEntryCellView.getBlogPostCell(cp, app, blogEntry, False, True, Return_CommentCnt, "")
                            '
                            ' -- if editing enabled, add the link and wrapperwrapper
                            blogArticleCell = genericController.addEditWrapper(cp, blogArticleCell, blogEntry.id, blogEntry.name, Models.BlogPostModel.contentName)

                            result.Append(blogArticleCell)
                            result.Append("<hr>")
                        End If
                        recordsOnThisPage += 1
                    Next
                End If
                '
                ' -- pagination
                result.Append(PaginationController.getRecordPagination(cp, blog.postsToDisplay, pageNumber, recordsOnThisPage, recordCount))
                Dim CategoryFooter As String = ""
                Dim qs As String
                '
                ' Build Footers
                If cp.User.IsAdmin() And blog.AllowCategories Then
                    qs = "cid=" & cp.Content.GetID("Blog Categories") & "&af=4"
                    CategoryFooter = CategoryFooter & "<div Class=""aoBlogFooterLink""><a href=""" & cp.Site.GetText("ADMINURL") & "?" & qs & """>Add a New category</a></div>"
                End If
                Dim ReturnFooter As String = ""
                If blog.AllowCategories Then
                    qs = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogCategoryIDSet, "0", True)
                    CategoryFooter = CategoryFooter & "<div class=""aoBlogFooterLink""><a href=""" & app.blogPageBaseLink & """>See Posts in All Categories</a></div>"
                    '
                    ' select a category
                    qs = cp.Doc.RefreshQueryString
                    Dim BlogCategoryList As List(Of BlogCategoriesModel) = DbModel.createList(Of BlogCategoriesModel)(cp, "")
                    If (BlogCategoryList.Count > 0) Then
                        For Each blogCategaory In BlogCategoryList
                            Dim IsBlocked As Boolean = False
                            If (isBlogCategoryBlockedDict.ContainsKey(blogCategaory.id)) Then
                                IsBlocked = isBlogCategoryBlockedDict(blogCategaory.id)
                            Else
                                Dim blogEntryCategory = DbModel.create(Of Models.BlogCategoriesModel)(cp, blogCategaory.id)
                                If (blogEntryCategory IsNot Nothing) Then
                                    If (blogEntryCategory.UserBlocking) Then IsBlocked = Not genericController.isGroupListMember(cp, GroupModel.GetBlockingGroups(cp, blogEntryCategory.id))
                                    isBlogCategoryBlockedDict.Add(blogCategaory.id, IsBlocked)
                                End If
                            End If
                            If Not IsBlocked Then
                                Dim categoryLink As String = cp.Utils.ModifyQueryString(qs, RequestNameBlogCategoryIDSet, CStr(blogCategaory.id), True)
                                CategoryFooter = CategoryFooter & "<div class=""aoBlogFooterLink""><a href=""?" & categoryLink & """> See Posts in All Category " & blogCategaory.name & "</a></div>"
                            End If
                        Next
                    End If
                End If
                '
                ' Footer
                result.Append("<div>&nbsp;</div>")
                If user.isBlogEditor(cp, blog) Then
                    '
                    ' Create a new entry if this is the Blog Owner
                    '
                    qs = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor.ToString(), True)
                    result.Append("<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Create New Blog Post</a></div>")
                    '
                    ' Create a link to edit the blog record
                    '
                    qs = "cid=" & cp.Content.GetID("Blogs") & "&af=4&id=" & blog.id
                    result.Append("<div class=""aoBlogFooterLink""><a href=""" & cp.Site.GetText("adminUrl") & "?" & qs & """>Blog Settings</a></div>")
                    '
                    ' Create a link to edit the rss record
                    '
                    If rssFeed.id = 0 Then

                    Else
                        qs = "cid=" & cp.Content.GetID("RSS Feeds") & "&af=4&id=" & rssFeed.id
                        result.Append("<div class=""aoBlogFooterLink""><a href=""" & cp.Site.GetText("adminUrl") & "?" & qs & """>Edit RSS Features</a></div>")
                    End If
                End If
                result.Append(ReturnFooter)
                result.Append(CategoryFooter)
                ''
                '' Search
                ''
                'qs = cp.Doc.RefreshQueryString
                'qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch.ToString(), True)
                'result.Append("<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>")
                '
                ' Link to RSS Feed
                '
                Dim FeedFooter As String = ""
                If rssFeed.rssFilename <> "" Then
                    FeedFooter = "RSS: " _
                        & "<a href=""" & cp.Http.CdnFilePathPrefix & rssFeed.rssFilename & """>" & rssFeed.name & "</a>" _
                        & "&nbsp;" _
                        & FeedFooter _
                        & iconRSS_16x16 _
                        & ""
                    result.Append("<div class=""aoBlogFooterLink"">" & FeedFooter & "</div>")
                End If
                result.Append(cp.Html.Hidden(RequestNameSourceFormID, FormBlogPostList.ToString()))
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            '
            Return result.ToString()
        End Function
    End Class
End Namespace
