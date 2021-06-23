
Imports Contensive.Models.Db

Namespace Models
    Public Class BlogPostModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("Blog Entries", "ccBlogCopy", "default", False)
        '
        '====================================================================================================
        ' -- instance properties
        ''' <summary>
        ''' if true, comments appear on this article
        ''' </summary>
        ''' <returns></returns>
        Public Property AllowComments As Boolean = False
        ''' <summary>
        ''' Determines how the primary image is displayed 1 = per stylesheet, 2 = right, 3 = left, 4 = hide
        ''' </summary>
        Public Property primaryImagePositionId As Integer
        ''' <summary>
        ''' The user who last published this article.
        ''' </summary>
        ''' <returns></returns>
        Public Property AuthorMemberID As Integer
        ''' <summary>
        ''' Use for categorization. Like Tags but restricted
        ''' </summary>
        ''' <returns></returns>
        Public Property blogCategoryID As Integer
        ''' <summary>
        ''' The article text
        ''' </summary>
        ''' <returns></returns>
        Public Property copy As String
        ''' <summary>
        ''' comma delimited tag list
        ''' </summary>
        ''' <returns></returns>
        Public Property tagList As String
        ''' <summary>
        ''' number of times this article page is viewed
        ''' </summary>
        ''' <returns></returns>
        Public Property Viewings As Integer
        ''' <summary>
        ''' Meta data for the page
        ''' </summary>
        ''' <returns></returns>
        Public Property metaTitle As String
        Public Property metaDescription As String
        Public Property metaKeywordList As String
        ''' <summary>
        ''' RSS data for the RSS feed
        ''' </summary>
        ''' <returns></returns>
        Public Property RSSDateExpire As Date
        Public Property RSSDatePublish As Date
        Public Property RSSDescription As String
        Public Property RSSLink As String
        Public Property RSSTitle As String
        ''' <summary>
        ''' Add a video or audio file to the article. The RSS feed will create it as a podcast enclosure.
        ''' </summary>
        ''' <returns></returns>
        Public Property PodcastMediaLink As String
        Public Property PodcastSize As Integer
        Public Property blogID As Integer
    End Class
End Namespace
