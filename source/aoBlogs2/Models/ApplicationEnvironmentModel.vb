
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
                ' -- return null if no blog entry for this doc
                If (local_blogEntryId Is Nothing) OrElse (local_blogEntryId = 0) Then Return Nothing
                '
                ' -- lookup and return the blog entry
                local_blogEntry = DbModel.create(Of BlogPostModel)(cp, cp.Utils.EncodeInteger(local_blogEntryId))
                Return local_blogEntry
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
        Public ReadOnly Property blogListLink As String
            Get
                If (local_blogListLink IsNot Nothing) Then Return local_blogListLink
                local_blogListLink = cp.Doc.RefreshQueryString()
                local_blogListLink = cp.Utils.ModifyQueryString(local_blogListLink, RequestNameSourceFormID, "")
                local_blogListLink = cp.Utils.ModifyQueryString(local_blogListLink, RequestNameFormID, "")
                local_blogListLink = cp.Utils.ModifyQueryString(local_blogListLink, RequestNameBlogCategoryID, "")
                local_blogListLink = cp.Utils.ModifyQueryString(local_blogListLink, RequestNameBlogEntryID, "")
                Return local_blogListLink
            End Get
        End Property
        Private local_blogListLink As String = Nothing
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
        '====================================================================================================
        '
        Public Sub New(cp As CPBaseClass, blog As BlogModel, blogEntryId As Integer)
            '
            Me.cp = cp
            '
            local_blog = blog
            local_blogEntryId = blogEntryId
        End Sub
    End Class
End Namespace