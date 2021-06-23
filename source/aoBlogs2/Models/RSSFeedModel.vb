

Imports Contensive.BaseClasses
Imports Contensive.Models.Db

Namespace Models
    Public Class RSSFeedModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("RSS Feeds", "ccRSSFeeds", "default", False)
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
                    rssFeed = create(Of RSSFeedModel)(cp, blog.RSSFeedID)
                    If (rssFeed IsNot Nothing) Then Return rssFeed
                End If
                rssFeed = addDefault(Of RSSFeedModel)(cp)
                rssFeed.copyright = "Copyright " & Now.Year
                rssFeed.description = ""
                rssFeed.link = ""
                rssFeed.logoFilename = ""
                rssFeed.name = "RSS Feed for Blog #" & blog.id & ", " & blog.Caption
                rssFeed.rssDateUpdated = Date.MinValue
                rssFeed.rssFilename = "RSS" & rssFeed.id & ".xml"
                rssFeed.save(cp)
                Return rssFeed

                ''
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Return Nothing
            End Try
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Update Blog
        ''' </summary>
        ''' <param name="cp"></param>
        Public Shared Sub UpdateBlogFeed(cp As CPBaseClass)
            '
            Try
                Call cp.Addon.Execute(RSSProcessAddonGuid)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Sub

        '
    End Class
End Namespace
