

Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class RSSFeedModel
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "RSS Feeds"      '<------ set content name
        Public Const contentTableName As String = "ccRSSFeeds"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        Public Property copyright As String
        Public Property description As String
        Public Property link As String
        Public Property logoFilename As String
        Public Property rssDateUpdated As Date
        Public Property rssFilename As String
        '
        '========================================================================
        '   Verify the RSSFeed and return the ID and the Link
        '       run when a new blog is created
        '       run on every post
        '========================================================================
        '
        Friend Shared Function verifyFeed(cp As CPBaseClass, blog As BlogModel) As RSSFeedModel
            Try
                Dim rssFeed As RSSFeedModel
                If (blog.RSSFeedID > 0) Then
                    rssFeed = DbModel.create(Of RSSFeedModel)(cp, blog.RSSFeedID)
                    If (rssFeed IsNot Nothing) Then Return rssFeed
                End If
                rssFeed = DbModel.add(Of RSSFeedModel)(cp)
                rssFeed.copyright = "Copyright " & Now.Year
                rssFeed.description = ""
                rssFeed.link = ""
                rssFeed.logoFilename = ""
                rssFeed.name = "RSS Feed for Blog #" & blog.id & ", " & blog.Caption
                rssFeed.rssDateUpdated = Date.MinValue
                rssFeed.rssFilename = "RSS" & rssFeed.id & ".xml"
                Return rssFeed
                'If Not (blog Is Nothing) Then
                '    ' Dim blog As blogModel = blogList.First
                '    BlogName = blog.name 'Csv.GetCSText(CSBlog, "name")
                '    blogDescription = Trim(blog.Copy)
                '    Return_RSSFeedID = blog.RSSFeedID 'Csv.GetCSInteger(CSBlog, "RSSFeedID")
                '    If Trim(BlogName) = "" Then
                '        Return_RSSFeedName = "Feed for Blog " & blog.id
                '    Else
                '        Return_RSSFeedName = BlogName
                '    End If
                '    blog.save(Of blogModel)(cp)
                '    Dim RssFeed As RSSFeedModel = RSSFeedModel.create(cp, Return_RSSFeedID)
                '    If Return_RSSFeedID <> 0 Then
                '        '
                '        ' Make sure the record exists
                '        '
                '        If Not cnRSSFeeds Then
                '            Return_RSSFeedID = 0
                '            'CSFeed = Main.OpenCSContentRecord("RSS Feeds", Return_RSSFeedID)
                '            'If Not Main.IsCSOK(CSFeed) Then
                '            '    Return_RSSFeedID = 0
                '            'End If
                '        End If
                '        '
                '        RssFeed = RSSFeedModel.add(cp)
                '        Return_RSSFeedID = cp.Doc.GetInteger(CSFeed, "ID")
                '        RssFeed.id = Return_RSSFeedID
                '        RssFeed.name = Return_RSSFeedName
                '        If blogDescription = "" Then
                '            blogDescription = Trim(Return_RSSFeedName)
                '        End If
                '        RssFeed.Description = blogDescription

                '        Return_RSSFeedFilename = cp.Db.EncodeSQLText(Return_RSSFeedName) & ".xml"
                '        RssFeed.RSSFilename = Return_RSSFeedFilename
                '        RssFeed.save(Of blogModel)(cp)
                '    End If
                '    RssFeed = RSSFeedModel.create(cp, Return_RSSFeedID)
                '    RssFeed = RSSFeedModel.add(cp)
                '    Return_RSSFeedName = RssFeed.name
                '    Return_RSSFeedID = RssFeed.id 'cp.Doc.GetInteger("id")
                '    Return_RSSFeedFilename = Trim(RssFeed.RSSFilename)
                '    If Trim(RssFeed.Link) = "" Then
                '        rssLink = cp.Request.Protocol & cp.Request.Host & blogListLink
                '        RssFeed.Link = rssLink

                '    End If
                '    'If Main.IsCSOK(CSFeed) Then
                '    '    '
                '    '    ' Manage the Feed name, title and description
                '    '    '   because it is associated to this blog'
                '    '    '   only reset the link if it is blank (see desc at top of class)
                '    '    '   only manage the RSSFeedFilename if it is blank
                '    '    '
                '    '    Return_RSSFeedName = rssfeed.  'cp.Doc.GetText(CSFeed, "name")
                '    '    Return_RSSFeedID = cp.Doc.GetInteger(CSFeed, "id")
                '    '    Return_RSSFeedFilename = Trim(Main.GetCS(CSFeed, "rssfilename"))
                '    '    If Trim(Main.GetCS(CSFeed, "link")) = "" Then
                '    '        rssLink = Main.serverProtocol & Main.ServerHost & blogListLink
                '    '        Call Main.SetCS(CSFeed, "link", rssLink)
                '    '    End If
                '    '    'Return_BlogRootLink = Trim(Main.GetCS(CSFeed, "link"))
                '    '    'If Return_BlogRootLink = "" Then
                '    '    '    '
                '    '    '    ' set blog link to current link without forms/categories
                '    '    '    '   exclude admin
                '    '    '    '   exclude a post
                '    '    '    '
                '    '    '    Return_BlogRootLink = Main.ServerLink
                '    '    '    If (InStr(1, Return_BlogRootLink, "admin", vbTextCompare) = 0) And (Main.ServerForm = "") Then
                '    '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameFormID, "", False)
                '    '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameSourceFormID, "", False)
                '    '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryID, "", False)
                '    '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryIDSet, "", False)
                '    '    '        Call Main.SetCS(CSFeed, "link", Return_BlogRootLink)
                '    '    '    End If
                '    '    'End If
                '    'End If
                '    ' Call Main.CloseCS(CSFeed)
                'End If
                '' Call Csv.CloseCS(CSBlog)
                ''CSBlog = Csv.OpenCSContent(cnBlogs, "ID=" & blogId)
                ''If Csv.IsCSOK(CSBlog) Then
                ''    BlogName = Csv.GetCSText(CSBlog, "name")
                ''    blogDescription = Trim(Csv.GetCS(CSBlog, "copy"))
                ''    Return_RSSFeedID = Csv.GetCSInteger(CSBlog, "RSSFeedID")
                ''    If Trim(BlogName) = "" Then
                ''        Return_RSSFeedName = "Feed for Blog " & blogId
                ''    Else
                ''        Return_RSSFeedName = BlogName
                ''    End If
                ''    If Return_RSSFeedID <> 0 Then
                ''        '
                ''        ' Make sure the record exists
                ''        '
                ''        CSFeed = Main.OpenCSContentRecord("RSS Feeds", Return_RSSFeedID)
                ''        If Not Main.IsCSOK(CSFeed) Then
                ''            Return_RSSFeedID = 0
                ''        End If
                ''    End If
                ''    If Return_RSSFeedID = 0 Then
                ''
                '' new blog was created, now create new feed
                ''   set name and description from the blog
                ''
                ''        CSFeed = Main.InsertCSContent(cnRSSFeeds)
                ''    If Main.IsCSOK(CSFeed) Then
                ''        Return_RSSFeedID = cp.Doc.GetInteger(CSFeed, "ID")
                ''        Call Main.SetCS(CSBlog, "RSSFeedID", Return_RSSFeedID)
                ''        Call Main.SetCS(CSFeed, "Name", Return_RSSFeedName)
                ''        If blogDescription = "" Then
                ''            blogDescription = Trim(Return_RSSFeedName)
                ''        End If
                ''        Call Main.SetCS(CSFeed, "description", blogDescription)
                ''        Return_RSSFeedFilename = encodeFilename(Return_RSSFeedName) & ".xml"
                ''        Call Main.SetCS(CSFeed, "rssfilename", Return_RSSFeedFilename)
                ''    End If
                ''End If
                ''If Main.IsCSOK(CSFeed) Then
                ''    '
                ''    ' Manage the Feed name, title and description
                ''    '   because it is associated to this blog'
                ''    '   only reset the link if it is blank (see desc at top of class)
                ''    '   only manage the RSSFeedFilename if it is blank
                ''    '
                ''    Return_RSSFeedName = cp.Doc.GetText(CSFeed, "name")
                ''    Return_RSSFeedID = cp.Doc.GetInteger(CSFeed, "id")
                ''    Return_RSSFeedFilename = Trim(Main.GetCS(CSFeed, "rssfilename"))
                ''    If Trim(Main.GetCS(CSFeed, "link")) = "" Then
                ''        rssLink = Main.serverProtocol & Main.ServerHost & blogListLink
                ''        Call Main.SetCS(CSFeed, "link", rssLink)
                ''    End If
                ''    'Return_BlogRootLink = Trim(Main.GetCS(CSFeed, "link"))
                ''    'If Return_BlogRootLink = "" Then
                ''    '    '
                ''    '    ' set blog link to current link without forms/categories
                ''    '    '   exclude admin
                ''    '    '   exclude a post
                ''    '    '
                ''    '    Return_BlogRootLink = Main.ServerLink
                ''    '    If (InStr(1, Return_BlogRootLink, "admin", vbTextCompare) = 0) And (Main.ServerForm = "") Then
                ''    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameFormID, "", False)
                ''    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameSourceFormID, "", False)
                ''    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryID, "", False)
                ''    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryIDSet, "", False)
                ''    '        Call Main.SetCS(CSFeed, "link", Return_BlogRootLink)
                ''    '    End If
                ''    'End If
                ''End If
                ''Call Main.CloseCS(CSFeed)
                ''End If
                ''Call Csv.CloseCS(CSBlog)
                ''
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Return Nothing
            End Try
        End Function


        '
        Public Shared Sub UpdateBlogFeed(cp As CPBaseClass)
            '
            Try
                Call cp.Utils.ExecuteAddon(RSSProcessAddonGuid)
                'Dim RSSTitle As String
                'Dim EntryCopy As String
                'Dim EntryLink As String
                'Dim qs As String
                'Dim RuleIDs() As Integer
                'Dim RuleBlogPostIDs() As Integer
                'Dim RuleCnt As Integer
                'Dim RulePtr As Integer
                'Dim RuleSize As Integer
                'Dim BlogPostID As Integer
                'Dim AdminURL As String
                ''
                'AdminURL = cp.Site.GetProperty("adminUrl")
                'If rssFeed.id <> 0 Then
                '    '
                '    ' Gather all the current rules
                '    '
                '    Dim RSSFeedBlogRuleList As List(Of RSSFeedBlogRuleModel) = RSSFeedBlogRuleModel.createList(cp, "RSSFeedID=" & rssFeed.id, "id,BlogPostID")
                '    RuleCnt = 0
                '    For Each RSSFeedBlogRule In RSSFeedBlogRuleList
                '        If RuleCnt >= RuleSize Then
                '            RuleSize = RuleSize + 10
                '            ReDim Preserve RuleIDs(RuleSize)
                '            ReDim Preserve RuleBlogPostIDs(RuleSize)
                '        End If
                '        RuleIDs(RuleCnt) = RSSFeedBlogRule.id 'Csv.GetCSInteger(CS, "ID")
                '        RuleBlogPostIDs(RuleCnt) = RSSFeedBlogRule.BlogPostID 'Csv.GetCSInteger(CS, "BlogPostID")
                '        RuleCnt = RuleCnt + 1
                '    Next

                '    Dim BlogCopyModelList As List(Of BlogCopyModel) = BlogCopyModel.createListFromBlogCopy(cp, blog.id)
                '    For Each BlogCopy In BlogCopyModelList
                '        BlogPostID = BlogCopy.id 'Csv.GetCSInteger(CS, "id")
                '        For RulePtr = 0 To RuleCnt - 1
                '            If BlogPostID = RuleBlogPostIDs(RulePtr) Then
                '                RuleIDs(RulePtr) = -1
                '                Exit For
                '            End If
                '        Next
                '        If RulePtr >= RuleCnt Then
                '            '
                '            ' Rule not found, add it
                '            '
                '            Dim RSSFeedBlogRule As RSSFeedBlogRuleModel = RSSFeedBlogRuleModel.add(cp)
                '            If (RSSFeedBlogRule IsNot Nothing) Then
                '                RSSFeedBlogRule.RSSFeedID = rssFeed.id
                '                RSSFeedBlogRule.BlogPostID = BlogPostID
                '                RSSFeedBlogRule.save(Of BlogModel)(cp)
                '            End If
                '            'CSRule = Csv.InsertCSRecord(cnRSSFeedBlogRules, 0)
                '            'If Csv.IsCSOK(CSRule) Then

                '            'Call Csv.SetCS(CSRule, "RSSFeedID", RSSFeed.id)
                '            'Call Csv.SetCS(CSRule, "BlogPostID", BlogPostID)
                '            'End If
                '            'Call Csv.CloseCS(CSRule)
                '            Dim BlogEntry As BlogEntryModel = DbModel.add(Of BlogEntryModel)(cp)
                '            If (BlogEntry IsNot Nothing) Then
                '                RSSTitle = Trim(BlogEntry.name)
                '                If RSSTitle = "" Then
                '                    RSSTitle = "Blog Post " & EntryID
                '                End If
                '                BlogEntry.RSSTitle = RSSTitle
                '                'Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
                '                '
                '                qs = ""
                '                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                '                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                '                EntryLink = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, qs, blogListLink & "?" & qs)
                '                If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                '                    'Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                '                    BlogEntry.RSSLink = EntryLink
                '                End If
                '                BlogEntry.RSSDescription = genericController.filterCopy(cp, EntryCopy, 150)
                '                ' Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
                '            End If
                '            ' CSPost = Csv.OpenCSContentRecord(cnBlogEntries, BlogPostID)
                '            'If Csv.IsCSOK(CSPost) Then
                '            '            blogEntry.Name = Csv.GetCSText(CSPost, "name")
                '            '            EntryID = Csv.GetCSInteger(CSPost, "id")
                '            '            EntryCopy = Csv.GetCSText(CSPost, "copy")
                '            '            '
                '            '            RSSTitle = Trim(blogEntry.Name)
                '            '            If RSSTitle = "" Then
                '            '                RSSTitle = "Blog Post " & EntryID
                '            '            End If
                '            '            Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
                '            '            '
                '            '            qs = ""
                '            '            qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                '            '            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                '            '            EntryLink = Main.GetLinkAliasByPageID(cp.Doc.PageId, qs, blogListLink & "?" & qs)
                '            '            If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                '            '                Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                '            '            End If
                '            '            Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
                '        End If

                '        ' Call Csv.CloseCS(CSPost)
                '        '
                '    Next
                '    For RulePtr = 0 To RuleCnt - 1
                '        If RuleIDs(RulePtr) <> -1 Then
                '            'Call Csv.DeleteContentRecord(cnRSSFeedBlogRules, RuleIDs(RulePtr))
                '            Call cp.Content.Delete(cnRSSFeedBlogRules, RuleIDs(RulePtr))
                '        End If
                '    Next
                'End If
                ''
                'Dim BuildVersion As String = cp.Site.GetProperty("BuildVersion")
                'If BuildVersion >= "4.1.098" Then
                '    Call cp.Utils.ExecuteAddon(RSSProcessAddonGuid)
                'End If
                ''Do While Csv.IsCSOK(CS)
                ''    BlogPostID = Csv.GetCSInteger(CS, "id")
                ''    For RulePtr = 0 To RuleCnt - 1
                ''        If BlogPostID = RuleBlogPostIDs(RulePtr) Then
                ''            RuleIDs(RulePtr) = -1
                ''            Exit For
                ''        End If
                ''    Next
                ''If RulePtr >= RuleCnt Then
                ''        '
                ''        ' Rule not found, add it
                ''        '
                ''        CSRule = Csv.InsertCSRecord(cnRSSFeedBlogRules, 0)
                ''        If Csv.IsCSOK(CSRule) Then
                ''            Call Csv.SetCS(CSRule, "RSSFeedID", RSSFeed.id)
                ''            Call Csv.SetCS(CSRule, "BlogPostID", BlogPostID)
                ''        End If
                ''        Call Csv.CloseCS(CSRule)
                ''    End If
                ''
                '' Now update the Blog Post RSS fields, RSSLink, RSSTitle, RSSDescription, RSSPublish, RSSExpire
                '' Should do this here because if RSS was installed after Blog, there is no link until a post is edited
                ''
                ''CSPost = Csv.OpenCSContentRecord(cnBlogEntries, BlogPostID)
                ''    If Csv.IsCSOK(CSPost) Then
                ''        blogEntry.Name = Csv.GetCSText(CSPost, "name")
                ''        EntryID = Csv.GetCSInteger(CSPost, "id")
                ''        EntryCopy = Csv.GetCSText(CSPost, "copy")
                ''        '
                ''        RSSTitle = Trim(blogEntry.Name)
                ''        If RSSTitle = "" Then
                ''            RSSTitle = "Blog Post " & EntryID
                ''        End If
                ''        Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
                ''        '
                ''        qs = ""
                ''        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                ''        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                ''        EntryLink = Main.GetLinkAliasByPageID(cp.Doc.PageId, qs, blogListLink & "?" & qs)
                ''        If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                ''            Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                ''        End If
                ''        Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
                ''    End If
                ''    Call Csv.CloseCS(CSPost)
                ''    '
                ''    Call Csv.NextCSRecord(CS)
                ''Loop
                ''Call Csv.CloseCS(CS)
                ''
                '' Now delete all the rules that were not found in the blog
                ''
                ''For RulePtr = 0 To RuleCnt - 1
                ''        If RuleIDs(RulePtr) <> -1 Then
                ''            Call Csv.DeleteContentRecord(cnRSSFeedBlogRules, RuleIDs(RulePtr))
                ''        End If
                ''    Next
                ''End If
                ''
                ''If Main.ContentServerVersion >= "4.1.098" Then
                ''    Call cp.Utils.ExecuteAddon(RSSProcessAddonGuid)
                ''End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Sub

        '
    End Class
End Namespace
