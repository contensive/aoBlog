
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
        'Public Property AuthoringGroupID As Integer
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
        Public Property ImageWidthMax As Integer
        Public Property OverviewLength As Integer
        Public Property OwnerMemberID As Integer
        Public Property postsToDisplay As Integer
        Public Property recaptcha As Boolean
        Public Property RSSFeedID As Integer
        Public Property ThumbnailImageWidth As Integer
        Public Property twitterLink As String
        'Public Property Id As Integer
        '
        '====================================================================================================
        ''' <summary>
        ''' Create a new default blog, ready to use. Must be an administrator. If not, returns null
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Shared Function verifyBlog(cp As CPBaseClass, instanceGuid As String) As BlogModel
            Try
                Dim Blog = DbModel.create(Of BlogModel)(cp, instanceGuid)
                If (Blog IsNot Nothing) Then Return Blog
                If (Not cp.User.IsAdmin) Then Return Nothing

                Blog = DbModel.add(Of BlogModel)(cp)
                Blog.name = "Default Blog"
                Blog.Caption = "The New Blog"
                Blog.Copy = "<p>This is the description of your new blog. It always appears at the top of your list of blog posts. Edit or remove this description by editing the blog features.</p>"
                Blog.OwnerMemberID = cp.User.Id
                'Blog.AuthoringGroupID = cp.Group.GetId("Site Managers")
                Blog.AllowAnonymous = True
                Blog.autoApproveComments = False
                Blog.AllowCategories = True
                Blog.postsToDisplay = 5
                Blog.OverviewLength = 500
                Blog.ThumbnailImageWidth = 200
                Blog.ImageWidthMax = 400
                Blog.allowArticleCTA = True
                Blog.ccguid = instanceGuid
                Blog.save(Of BlogModel)(cp)
                Dim rssFeed = RSSFeedModel.verifyFeed(cp, Blog)
                Blog.RSSFeedID = If(rssFeed IsNot Nothing, rssFeed.id, 0)
                Blog.save(Of BlogModel)(cp)

                Dim blogEntry As BlogPostModel = DbModel.add(Of BlogPostModel)(cp)
                If (blogEntry IsNot Nothing) Then
                    blogEntry.blogID = Blog.id
                    blogEntry.name = "Welcome to the New Blog!"
                    blogEntry.RSSTitle = ""
                    blogEntry.copy = cp.WwwFiles.Read("blogs\DefaultPostCopy.txt")

                    'Dim qs As String = cp.Utils.ModifyQueryString(cp.Doc.RefreshQueryString, RequestNameBlogEntryID, CStr(blogEntry.id))
                    Dim qs As String = cp.Utils.ModifyQueryString("", RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, rnFormID, FormBlogPostDetails.ToString())
                    Call cp.Site.AddLinkAlias(Blog.Caption, cp.Doc.PageId, qs)
                    Dim LinkAlias As List(Of LinkAliasesModel) = DbModel.createList(Of LinkAliasesModel)(cp, "(pageid=" & cp.Doc.PageId & ")and(QueryStringSuffix=" & cp.Db.EncodeSQLText(qs) & ")")
                    If (LinkAlias.Count > 0) Then
                        Dim EntryLink As String = LinkAlias.First().name
                    End If
                    blogEntry.RSSDescription = genericController.getBriefCopy(cp, blogEntry.copy, 150)
                    blogEntry.save(Of BlogPostModel)(cp)
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
                '
                'Add this new Call to Action
                '
                Dim callToAction = Models.CallsToActionModel.create(Of CallsToActionModel)(cp, guidDefaultCallToAction)
                If (callToAction Is Nothing) Then
                    callToAction = DbModel.add(Of CallsToActionModel)(cp)
                    callToAction.name = "Find Out More"
                    callToAction.link = "http://www.MemberBoss.com"
                    callToAction.headline = " Manage Your Membership Community"
                    callToAction.brief = "<p>The best all-in-one-place solution to build and manage your membership community.</p>"
                    callToAction.ccguid = guidDefaultCallToAction
                    callToAction.save(Of BlogModel)(cp)
                End If
                '
                Call RSSFeedModel.UpdateBlogFeed(cp)
                '
                Return Blog
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw New ApplicationException("Exception creating default blog")
            End Try
        End Function
    End Class
End Namespace
