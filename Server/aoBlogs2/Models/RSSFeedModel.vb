

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
                rssFeed.save(Of RSSFeedModel)(cp)
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
                Call cp.Utils.ExecuteAddon(RSSProcessAddonGuid)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Sub

        '
    End Class
End Namespace
