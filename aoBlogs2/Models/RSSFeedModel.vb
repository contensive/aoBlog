

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
    End Class
End Namespace
