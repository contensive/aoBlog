
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Public Class ApplicationController
    '
    Public ReadOnly Property cp As CPBaseClass
    '
    '====================================================================================================
    '
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
    '
    Public Property blog As BlogModel
    '
    '====================================================================================================
    '
    Public Property blogEntry As BlogEntryModel
    '
    '====================================================================================================
    '
    Public Property user As PersonModel
    '
    '====================================================================================================
    '
    Public Property blogListLink As String
    '
    '====================================================================================================
    '
    Public Property RSSFeed As RSSFeedModel
    '
    '====================================================================================================
    '
    Public Property blogPageBaseLink As String
    '
    '====================================================================================================
    '
    Public Sub New(cp As CPBaseClass, blog As BlogModel, blogEntryId As Integer)
        '
        Me.cp = cp
        '
        Me.blog = blog
        user = PersonModel.create(cp, cp.User.Id)
        blogListLink = cp.Doc.RefreshQueryString()
        blogListLink = cp.Utils.ModifyQueryString(blogListLink, RequestNameSourceFormID, "")
        blogListLink = cp.Utils.ModifyQueryString(blogListLink, RequestNameFormID, "")
        blogListLink = cp.Utils.ModifyQueryString(blogListLink, RequestNameBlogCategoryID, "")
        blogListLink = cp.Utils.ModifyQueryString(blogListLink, RequestNameBlogEntryID, "")
        blogPageBaseLink = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", "")
        blogEntry = DbModel.create(Of BlogEntryModel)(cp, blogEntryId)
        '
        ' Get the Feed Args
        RSSFeed = DbModel.create(Of RSSFeedModel)(cp, blog.RSSFeedID)
        If (RSSFeed Is Nothing) Then
            RSSFeed = DbModel.add(Of RSSFeedModel)(cp)
            RSSFeed.name = blog.Caption
            RSSFeed.description = "This is your First RssFeed"
            RSSFeed.save(Of BlogModel)(cp)
            blog.RSSFeedID = RSSFeed.id
            blog.save(Of BlogModel)(cp)
        End If
    End Sub
End Class
