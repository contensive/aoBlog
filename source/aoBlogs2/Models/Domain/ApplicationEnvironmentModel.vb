
Imports System.Linq
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Models
    Public Class ApplicationEnvironmentModel
        '
        Public ReadOnly Property cp As CPBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' The user at the keyboard is editing
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property userIsEditing As Boolean
            Get
                If local_userIsEditing Is Nothing Then
                    local_userIsEditing = cp.User.IsEditingAnything
                End If
                Return CBool(local_userIsEditing)
            End Get
        End Property
        Private Property local_userIsEditing As Boolean? = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' blog set during construction
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property blog As BlogModel
            Get
                '
                ' -- blog set during object constructor
                Return local_blog
            End Get
        End Property
        Private local_blog As BlogModel = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' Current Blog Entry. Returns null if no valid entry
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property blogEntry As BlogPostModel
            Get
                '
                ' -- return if set
                If (local_blogEntry IsNot Nothing) Then Return local_blogEntry
                '
                ' -- return null if blogentryid is not valid
                If (local_blogEntryId Is Nothing) OrElse (local_blogEntryId = 0) Then Return Nothing
                '
                ' -- blogEntryId is valid, return blog entry
                local_blogEntry = DbModel.create(Of BlogPostModel)(cp, cp.Utils.EncodeInteger(local_blogEntryId))
                If (local_blogEntry IsNot Nothing) Then Return local_blogEntry
                '
                ' -- if blogentryid is not 0, and blog is null, remove link alias that might have caused this
                LinkAliasController.deleteLinkAlias(cp, cp.Doc.PageId, CInt(local_blogEntryId))
                Return Nothing
            End Get
        End Property
        Private ReadOnly local_blogEntryId As Integer? = Nothing
        Private local_blogEntry As BlogPostModel = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' The current user at the keyboard
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property user As PersonModel
            Get
                If (local_user Is Nothing) Then
                    local_user = PersonModel.create(cp, cp.User.Id)
                End If
                Return local_user
            End Get
        End Property
        Private local_user As PersonModel = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' link to the blogs list page
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property blogBaseLink As String
            Get
                If (local_blogBaseLink IsNot Nothing) Then Return local_blogBaseLink
                local_blogBaseLink = cp.Content.GetPageLink(cp.Doc.PageId)
                Return local_blogBaseLink
            End Get
        End Property
        Private local_blogBaseLink As String = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' RSS feed for this blog?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property rssFeed As RSSFeedModel
            Get
                If (local_RSSFeed Is Nothing) Then
                    '
                    ' Get the Feed Args
                    local_RSSFeed = DbModel.create(Of RSSFeedModel)(cp, blog.RSSFeedID)
                    If (local_RSSFeed Is Nothing) Then
                        local_RSSFeed = DbModel.add(Of RSSFeedModel)(cp)
                        local_RSSFeed.name = blog.Caption
                        local_RSSFeed.description = "This is your First RssFeed"
                        local_RSSFeed.save(Of BlogModel)(cp)
                        blog.RSSFeedID = local_RSSFeed.id
                        blog.save(Of BlogModel)(cp)
                    End If
                End If
                Return local_RSSFeed
            End Get
        End Property
        Private local_RSSFeed As RSSFeedModel = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property blogPageBaseLink As String
            Get
                If local_blogPageBaseLink IsNot Nothing Then Return local_blogPageBaseLink
                local_blogPageBaseLink = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", "")
                Return local_blogPageBaseLink
            End Get
        End Property
        Private local_blogPageBaseLink As String = Nothing
        '
        '
        Public ReadOnly Property nextArticle As BlogPostModel
            Get
                If nextArticle_local IsNot Nothing Then Return nextArticle_local
                Dim postList As List(Of BlogPostModel) = DbModel.createList(Of BlogPostModel)(cp, "(blogID=" & blog.id & ")and(DateAdded>" & cp.Db.EncodeSQLDate(blogEntry.DateAdded) & ")", "DateAdded", 1, 1)
                If postList.Count = 0 Then Return Nothing
                nextArticle_local = postList.First()
                Return nextArticle_local
            End Get
        End Property
        Private nextArticle_local As BlogPostModel = Nothing
        '
        '
        Public ReadOnly Property nextArticleLink As String
            Get
                If nextArticle Is Nothing Then Return Nothing
                '
                Dim qs As String = LinkAliasController.getLinkAliasQueryString(cp, cp.Doc.PageId, nextArticle.id)
                nextArticleLink_local = cp.Content.GetPageLink(cp.Doc.PageId, qs)
                Return nextArticleLink_local
            End Get
        End Property
        Private nextArticleLink_local As String = Nothing
        '
        '
        Public ReadOnly Property nextArticleLinkCaption As String
            Get
                If nextArticle Is Nothing Then Return Nothing
                Return nextArticle.name
            End Get
        End Property

        '
        '====================================================================================================
        '
        Public Sub New(cp As CPBaseClass, blog As BlogModel, blogEntryId As Integer)
            '
            Me.cp = cp
            '
            local_blog = blog
            local_blogEntryId = blogEntryId
        End Sub
        '
        '
        ''' <summary>
        ''' if false, do now show tags. This blocks all the additional pages that hurt SEO
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property sitePropertyAllowTags As Boolean
            Get
                Return cp.Site.GetBoolean("blog allow tags", False)
            End Get
        End Property
    End Class
End Namespace