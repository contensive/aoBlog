
Option Explicit On
Option Strict On

Imports System.Linq
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.BaseClasses

Namespace Models
    Public Class BlogModel
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Blogs"
        Public Const contentTableName As String = "ccBlogs"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property AllowAnonymous As Boolean
        Public Property allowArchiveList As Boolean
        Public Property allowArticleCTA As Boolean
        Public Property AllowCategories As Boolean
        Public Property allowEmailSubscribe As Boolean
        Public Property allowFacebookLink As Boolean
        Public Property allowGooglePlusLink As Boolean
        Public Property allowRSSSubscribe As Boolean
        Public Property allowSearch As Boolean
        Public Property allowTwitterLink As Boolean
        Public Property AuthoringGroupID As Integer
        Public Property autoApproveComments As Boolean
        Public Property BriefFilename As String
        Public Property Caption As String
        Public Property Copy As String
        Public Property emailComment As Boolean
        Public Property emailSubscribeGroupId As Integer
        Public Property facebookLink As String
        Public Property followUsCaption As String
        Public Property googlePlusLink As String
        Public Property HideContributer As Boolean
        Public Property ignoreLegacyInstanceOptions As Boolean
        Public Property ImageWidthMax As Integer
        Public Property OverviewLength As Integer
        Public Property OwnerMemberID As Integer
        Public Property PostsToDisplay As Integer
        Public Property recaptcha As Boolean
        Public Property RSSFeedID As Integer
        Public Property ThumbnailImageWidth As Integer
        Public Property twitterLink As String
        '
        '====================================================================================================
        ''' <summary>
        ''' Create a new default blog, ready to use. Must be an administrator. If not, returns null
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Shared Function verifyBlog(cp As CPBaseClass, instanceId As String) As BlogModel
            Try
                If (Not cp.User.IsAdmin) Then Return Nothing

                Dim Blog = DbModel.add(Of BlogModel)(cp)
                Blog.name = "Default Blog"
                Blog.Caption = "The New Blog"
                Blog.OwnerMemberID = cp.User.Id
                Blog.AuthoringGroupID = cp.Group.GetId("Site Managers")
                Blog.ignoreLegacyInstanceOptions = True
                Blog.AllowAnonymous = True
                Blog.autoApproveComments = False
                Blog.AllowCategories = True
                Blog.PostsToDisplay = 5
                Blog.OverviewLength = 500
                Blog.ThumbnailImageWidth = 200
                Blog.ImageWidthMax = 400
                Blog.ccguid = instanceId
                Blog.save(Of BlogModel)(cp)
                Dim rssFeed = RSSFeedModel.verifyFeed(cp, Blog)
                Blog.RSSFeedID = If(rssFeed IsNot Nothing, rssFeed.id, 0)
                Blog.save(Of BlogModel)(cp)

                Dim blogEntry As BlogEntryModel = DbModel.add(Of BlogEntryModel)(cp)
                If (blogEntry IsNot Nothing) Then
                    blogEntry.BlogID = Blog.id
                    blogEntry.name = "Welcome to the New Blog!"
                    blogEntry.RSSTitle = ""
                    blogEntry.Copy = cp.WwwFiles.Read("blogs\DefaultPostCopy.txt")

                    Dim qs As String = cp.Utils.ModifyQueryString(cp.Doc.RefreshQueryString, RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
                    Call cp.Site.addLinkAlias(Blog.Caption, cp.Doc.PageId, qs)
                    Dim LinkAlias As List(Of LinkAliasesModel) = DbModel.createList(Of LinkAliasesModel)(cp, "(pageid=" & cp.Doc.PageId & ")and(QueryStringSuffix=" & cp.Db.EncodeSQLText(qs) & ")")
                    If (LinkAlias.Count > 0) Then
                        Dim EntryLink As String = LinkAlias.First().name
                    End If
                    blogEntry.RSSDescription = genericController.filterCopy(cp, blogEntry.Copy, 150)
                    blogEntry.save(Of BlogEntryModel)(cp)
                End If
                '
                ' Add this new default post to the new feed
                '
                Dim RSSFeedBlogRules As RSSFeedBlogRuleModel = RSSFeedBlogRuleModel.add(cp)
                If (RSSFeedBlogRules IsNot Nothing) Then
                    RSSFeedBlogRules.RSSFeedID = rssFeed.id
                    RSSFeedBlogRules.BlogPostID = blogEntry.id
                    RSSFeedBlogRules.name = "RSS Feed [" & rssFeed.name & "], Blog Post [" & blogEntry.name & "]"
                    RSSFeedBlogRules.save(Of BlogModel)(cp)
                End If
                Return Blog
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw New ApplicationException("Exception creating default blog")
            End Try
        End Function
    End Class
End Namespace
