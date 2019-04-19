
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
    Public Class ListView
        '
        '====================================================================================
        '
        Public Shared Function GetListView(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, request As View.RequestModel, user As PersonModel, blogListLink As String, blogListQs As String) As String
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
                Dim BlogEntryList As List(Of BlogEntryModel)
                Dim NoneMsg As String
                Dim blogCategory = DbModel.create(Of Models.BlogCategorieModel)(cp, request.categoryId)
                '
                If (blogCategory IsNot Nothing) Then
                    BlogEntryList = DbModel.createList(Of BlogEntryModel)(cp, "(BlogID=" & blog.id & ")And(BlogCategoryID=" & request.categoryId & ")", "DateAdded Desc", blog.PostsToDisplay, 1)
                    result.Append(cr & "<div Class=""aoBlogCategoryCaption"">Category " & blogCategory.name & "</div>")
                    NoneMsg = "There are no blog posts available in the category " & blogCategory.name
                Else
                    BlogEntryList = DbModel.createList(Of BlogEntryModel)(cp, "(BlogID=" & blog.id & ")", "DateAdded Desc", blog.PostsToDisplay, 1)
                    NoneMsg = "There are no blog posts available"
                End If
                '
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
                            Dim Return_CommentCnt As Integer
                            Dim blogArticleCell = genericController.getBlogEntryCell(cp, blog, rssFeed, blogEntry, user, False, True, Return_CommentCnt, "", blogListQs)
                            '
                            ' -- if editing enabled, add the link and wrapperwrapper
                            blogArticleCell = genericController.addEditWrapper(cp, blogArticleCell, blogEntry.id, blogEntry.name, Models.BlogEntryModel.contentName, "Blog Article Settings")

                            result.Append(blogArticleCell)
                            result.Append(cr & "<hr>")
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
                ' Link to RSS Feed
                '
                Dim FeedFooter As String = ""
                If rssFeed.rssFilename <> "" Then
                    FeedFooter = "<a href=""" & cp.Site.FilePath & rssFeed.rssFilename & """>"
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
            '
            Return result.ToString()
        End Function
    End Class
End Namespace
