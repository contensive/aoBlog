
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    Public Class BlogClass
        Inherits AddonBaseClass
        '
        '=====================================================================================
        ''' <summary>
        ''' Blog Addon
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <returns></returns>
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                Dim instanceId As String = CP.Doc.GetText("instanceId")
                Dim blog As blogModel = blogModel.verifyBlog(CP, Controllers.InstanceIdController.getInstanceId(CP))
                If (blog Is Nothing) Then Return "<!-- Could not find or create blog from instanceId [" & instanceid & "] -->"
                '
                ' -- get the post list (blog list of posts without sidebar)
                Dim postListController As New BlogPostListClass()
                Dim postList As String = postListController.GetContent(CP, blog)
                Dim blogEntryId As Integer = CP.Doc.GetInteger(RequestNameBlogEntryID)
                Dim isArticlePage As Boolean = (blogEntryId <> 0)
                Dim archiveList As String = ""
                If blog.allowArchiveList Then
                    '
                    ' create the article list now - if only the current month, turn if off before setting allowListSidebar
                    '
                    Dim blogListQs As String = CP.Doc.RefreshQueryString()
                    blogListQs = CP.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, "")
                    blogListQs = CP.Utils.ModifyQueryString(blogListQs, RequestNameFormID, "")
                    blogListQs = CP.Utils.ModifyQueryString(blogListQs, RequestNameBlogCategoryID, "")
                    blogListQs = CP.Utils.ModifyQueryString(blogListQs, RequestNameBlogEntryID, "")
                    Dim blogListLink As String = CP.Content.GetLinkAliasByPageID(CP.Doc.PageId, blogListQs, "?" & blogListQs)
                    archiveList = GetFormBlogArchiveDateList(CP, blog.id, blogListQs)
                    If archiveList = "" Then
                        blog.allowArchiveList = False
                    End If
                End If
                Dim rssFeed = DbModel.create(Of RSSFeedModel)(CP, blog.RSSFeedID)
                Dim allowListSidebar As Boolean = blog.allowEmailSubscribe Or blog.allowFacebookLink Or blog.allowGooglePlusLink Or blog.allowGooglePlusLink Or blog.allowRSSSubscribe Or blog.allowTwitterLink Or blog.allowArchiveList Or blog.allowSearch
                Dim allowArticleSidebar As Boolean = allowListSidebar Or blog.allowArticleCTA
                Dim dstFormId As Integer = CP.Doc.GetInteger(RequestNameFormID)
                Dim allowSidebar As Boolean = Not (dstFormId = FormBlogEntryEditor) And ((isArticlePage And allowArticleSidebar) Or (Not isArticlePage And allowListSidebar))
                '
                Dim cellList As String = ""
                Dim layout As CPBlockBaseClass = CP.BlockNew()
                layout.OpenLayout(BlogListLayout)
                Dim sidebarCell As CPBlockBaseClass = CP.BlockNew()
                sidebarCell.Load(layout.GetOuter(".blogSidebarCellWrap"))
                Dim cellTemplate As String = sidebarCell.GetHtml()
                Dim adminSuggestions As String = ""
                '
                If isArticlePage Then
                    '
                    ' -- article page
                    Dim blogImageList As List(Of BlogImageModel) = BlogImageModel.createListFromBlogEntry(CP, blogEntryId)
                    Dim blogEntry = DbModel.create(Of BlogEntryModel)(CP, blogEntryId)
                    Dim blogEntryBrief As String = blogEntry.RSSDescription
                    If blogEntryBrief = "" Then
                        blogEntryBrief = CP.Utils.DecodeHTML(blogEntry.Copy)
                        If blogEntryBrief.Length > 300 Then
                            Dim ptr As Integer = blogEntryBrief.IndexOf(" ", 290)
                            If ptr < 0 Then ptr = 300
                            blogEntryBrief = blogEntryBrief.Substring(1, ptr - 1) & "..."
                        End If
                    End If
                    '
                    ' -- Set Open Graph
                    If blogEntry.name <> "" Then
                        Dim siteName As String = CP.Site.GetProperty("facebook site_name")
                        If siteName = "" Then
                            Call CP.Site.LogWarning("Facebook site name is not set", "", "Facebook site name missing", "")
                            siteName = CP.Site.Name
                        End If
                        If (blogImageList.Count > 0) Then
                            Call CP.Doc.SetProperty("Open Graph Image", "http://" & CP.Site.Domain & CP.Site.FilePath & blogImageList.First().Filename)
                        Else
                            adminSuggestions &= CP.Html.li("This blog entry has no image. Adding an image will improve your social media appeal.")
                        End If
                        Call CP.Doc.SetProperty("Open Graph Site Name", CP.Utils.EncodeHTML(siteName))
                        Call CP.Doc.SetProperty("Open Graph Content Type", "website")
                        Call CP.Doc.SetProperty("Open Graph URL", CP.Content.GetPageLink(CP.Doc.PageId, "BlogEntryID=" & blogEntryId & "&FormID=300"))
                        Call CP.Doc.SetProperty("Open Graph Title", blogEntry.name)
                        Call CP.Doc.SetProperty("Open Graph Description", blogEntryBrief)
                    End If
                    '
                    ' -- set article meta data
                    Call CP.Doc.AddTitle(blogEntry.metaTitle)
                    Call CP.Doc.AddMetaDescription(blogEntry.metaDescription)
                    Call CP.Doc.AddMetaKeywordList((blogEntry.metaDescription & "," & blogEntry.TagList).Replace(vbCrLf, ",").Replace(vbCr, ",").Replace(vbLf, ",").Replace(",,", ","))
                End If
                '
                ' -- social media likes
                Call sidebarCell.SetOuter(".blogSidebarCellHeadline", "")
                Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                Call sidebarCell.SetInner(".blogSidebarCellInputCaption", CP.Utils.ExecuteAddon(facebookLikeAddonGuid))
                Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                Dim copy As String = sidebarCell.GetHtml()
                If isArticlePage Then
                    postList = Replace(postList, "<div class=""aoBlogEntryCopy"">", copy & "<div class=""aoBlogEntryCopy"">")
                End If
                Dim sidebarCnt As Integer = 0
                '
                ' Sidebar
                '
                If allowSidebar Then
                    '
                    If blog.allowArticleCTA And isArticlePage Then
                        '
                        ' CTA cells
                        '
                        Dim blogEntryCtaRuleList = DbModel.createList(Of BlogEntryCTARuleModel)(CP, "blogEntryid=" & blogEntryId)
                        For Each rule In blogEntryCtaRuleList
                            Dim cta = DbModel.create(Of CallsToActionModel)(CP, 1)
                            If (cta IsNot Nothing) Then
                                sidebarCell.Load(cellTemplate)
                                Call sidebarCell.SetInner(".blogSidebarCellHeadline", cta.headline)
                                Call sidebarCell.SetInner(".blogSidebarCellCopy", cta.brief)
                                Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                                Call sidebarCell.SetOuter(".blogSidebarCellInputCaption", "")
                                If (cta.name = "") Or (cta.link = "") Then
                                    Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                                Else
                                    Call sidebarCell.SetInner(".blogSidebarCellButton", "<a target=""_blank"" href=""" & cta.link & """>" & cta.name & "</a>")
                                End If
                                cellList &= vbCrLf & vbTab & sidebarCell.GetHtml()
                                sidebarCnt += 1
                            End If
                        Next
                    End If
                    '
                    If blog.allowEmailSubscribe Then
                        '
                        ' Subscribe by email
                        '
                        Dim subscribed As Boolean = CP.Visit.GetBoolean("EmailSubscribed-Blog" & blog.id & "-user" & CP.User.Id)
                        If Not subscribed Then
                            subscribed = CP.User.IsInGroup(blog.emailSubscribeGroupId)
                        End If
                        sidebarCell.Load(cellTemplate)
                        Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Subscribe By Email")
                        Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                        If subscribed Then
                            Call sidebarCell.SetInner(".blogSidebarCellInputCaption", "You are subscribed to this blog.")
                            Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellButton", "")

                        Else
                            Call sidebarCell.SetInner(".blogSidebarCellInputCaption", "Email*")
                            Call sidebarCell.SetInner(".blogSidebarCellInput", "<input type=""text"" id=""blogSubscribeEmail"" name=""email"" value=""" & CP.User.Email & """>")
                            Call sidebarCell.SetInner(".blogSidebarCellButton a", "Subscribe")
                            Call sidebarCell.SetInner(".blogSidebarCellButton", "<a href=""#"" id=""blogSidebarEmailSubscribe"">Subscribe</a>")
                        End If
                        cellList &= vbCrLf & vbTab & "<div id=""blogSidebarEmailCell"">" & sidebarCell.GetHtml() & "</div>"
                        sidebarCnt += 1
                    End If
                    '
                    If blog.allowRSSSubscribe Then
                        '
                        If ((rssFeed Is Nothing) Or (rssFeed.RSSFilename = "")) Then
                            adminSuggestions &= CP.Html.li("This blog includes an RSS Feed, but no feed has been created. It his persists, please contact the site developer. Disable RSS feeds for this blog to hide this message.")
                        Else
                            '
                            sidebarCell.Load(cellTemplate)
                            Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Subscribe By RSS")
                            Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                            'Call sidebarCell.SetInner(".blogSidebarCellCopy", "You are subscribed to this Feed.")
                            Call sidebarCell.SetInner(".blogSidebarCellInputCaption", "<a href=""http://" & CP.Site.DomainPrimary & "/rss/" & rssFeed.RSSFilename & """><img id=""blogSidebarRSSLogo"" src=""/blogs/rss.png"" width=""25"" height=""25"">" & blog.name & " Feed" & "</a>")
                            Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                            cellList &= vbCrLf & vbTab & "<div id=""blogSidebarRSSCell"">" & sidebarCell.GetHtml() & "</div>"
                            sidebarCnt += 1
                        End If
                    End If
                    '
                    If blog.allowFacebookLink Or blog.allowGooglePlusLink Or blog.allowTwitterLink Then
                        '
                        ' Social Links
                        copy = ""
                        If blog.allowFacebookLink And (blog.facebookLink <> "") Then
                            copy &= "<a href=""" & blog.facebookLink & """ target=""_blank""><img class=""blogSidebarSocialLogo"" src=""/blogs/facebook.jpg"" width=""32"" height=""32""></a>"
                        ElseIf blog.allowFacebookLink Then
                            If CP.User.IsAdmin Then
                                copy &= "<div class=""blogAdminWarning""><h2>Administrator</h2><p>Add a facebook link for this blog, or disable the Allow Facebook Sidebar checkbox.</p></div>"
                            End If
                        End If
                        If blog.allowTwitterLink And (blog.twitterLink <> "") Then
                            copy &= "<a href=""" & blog.twitterLink & """ target=""_blank""><img class=""blogSidebarSocialLogo"" src=""/blogs/twitter.jpg"" width=""32"" height=""32""></a>"
                        ElseIf blog.allowTwitterLink Then
                            If CP.User.IsAdmin Then
                                copy &= "<div class=""blogAdminWarning""><h2>Administrator</h2><p>Add a twitter link for this blog, or disable the Allow Twitter Sidebar checkbox.</p></div>"
                            End If
                        End If
                        If blog.allowTwitterLink And (blog.googlePlusLink <> "") Then
                            copy &= "<a href=""" & blog.googlePlusLink & """ target=""_blank""><img class=""blogSidebarSocialLogo"" src=""/blogs/GooglePlus.jpg"" width=""32"" height=""32""></a>"
                        ElseIf blog.allowGooglePlusLink Then
                            If CP.User.IsAdmin Then
                                copy &= "<div class=""blogAdminWarning""><h2>Administrator</h2><p>Add a GooglePlus link for this blog, or disable the Allow Google Plus Sidebar checkbox.</p></div>"
                            End If
                        End If
                        If copy <> "" Then
                            If blog.followUsCaption = "" Then
                                blog.followUsCaption = "Follow Us"
                            End If
                            sidebarCell.Load(cellTemplate)
                            Call sidebarCell.SetInner(".blogSidebarCellHeadline", blog.followUsCaption)
                            Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                            Call sidebarCell.SetInner(".blogSidebarCellInputCaption", copy)
                            cellList &= vbCrLf & vbTab & "<div id=""blogSidebarSocialCell"">" & sidebarCell.GetHtml() & "</div>"
                            sidebarCnt += 1
                        End If
                    End If
                    '
                    If blog.allowSearch Then
                        '
                        ' Search 
                        '
                        Dim formInput As String
                        formInput = CP.Html.InputText("keywordList", CP.Doc.GetText("keywordList"))
                        formInput += CP.Html.Hidden("formid", "120")
                        formInput += CP.Html.Hidden("sourceformid", "120")
                        formInput += CP.Html.Hidden("button", " Search Blogs ")
                        formInput = CP.Html.Form(formInput, , , "blogSidebarSearchForm")
                        sidebarCell.Load(cellTemplate)
                        Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Search")
                        Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                        Call sidebarCell.SetOuter(".blogSidebarCellInputCaption", "")
                        Call sidebarCell.SetInner(".blogSidebarCellInput", formInput)
                        Call sidebarCell.SetInner(".blogSidebarCellButton", "<a href=""#"" id=""blogSidebarSearch"" onclick=""jQuery('#blogSidebarSearchForm').submit();return false;"">Search</a>")
                        cellList &= vbCrLf & vbTab & "<div id=""blogSidebarSearchCell"">" & sidebarCell.GetHtml() & "</div>"
                        sidebarCnt += 1
                    End If
                    '
                    If blog.allowArchiveList Then
                        '
                        ' Archive List
                        '
                        sidebarCell.Load(cellTemplate)
                        Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Archives")
                        Call sidebarCell.SetInner(".blogSidebarCellCopy", archiveList)
                        Call sidebarCell.SetOuter(".blogSidebarCellInputCaption", "")
                        Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                        Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                        cellList &= vbCrLf & vbTab & "<div id=""blogSidebarArchiveCell"">" & sidebarCell.GetHtml() & "</div>"
                        sidebarCnt += 1
                    End If
                End If
                layout.SetInner(".blogSidebar", cellList)
                layout.Append(CP.Html.Hidden("blogId", blog.id, "", "blogId"))
                If sidebarCnt = 0 Then
                    layout.SetInner(".blogWrapper", layout.GetInner(".blogColumn1"))
                End If
                returnHtml = layout.GetHtml()
                returnHtml = returnHtml.Replace("{{legacyBlog}}", postList)
                Dim js As String = ""
                If js <> "" Then
                    CP.Doc.AddHeadJavascript(js)
                End If
                If adminSuggestions <> "" And CP.User.IsAdmin() Then
                    returnHtml = "<div class=""ccHintWrapper""><div class=""ccHintWrapperContent""><h2>Administrator</h2><ul>" & adminSuggestions & "</ul></div></div>" & returnHtml
                End If
                '
            Catch ex As Exception
                errorReport(CP, ex, "execute")
            End Try
            Return returnHtml
        End Function
        '
        '=====================================================================================
        ' common report for this class
        '=====================================================================================
        '
        Private Sub errorReport(ByVal cp As CPBaseClass, ByVal ex As Exception, ByVal method As String)
            Try
                cp.Site.ErrorReport(ex, "Unexpected error in sampleClass." & method)
            Catch exLost As Exception
                '
                ' stop anything thrown from cp errorReport
                '
            End Try
        End Sub
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogArchiveDateList(cp As CPBaseClass, BlogID As Integer, blogListQs As String) As String
            Dim returnHtml As String = ""
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                Dim ArchiveMonth As Integer
                Dim ArchiveYear As Integer
                Dim NameOfMonth As String
                Dim qs As String
                Dim SQL As String
                '
                SQL = "SELECT distinct Month(DateAdded) as ArchiveMonth, year(dateadded) as ArchiveYear " _
                    & " From ccBlogCopy" _
                    & " Where (ContentControlID = " & cp.Content.GetID(cnBlogEntries) & ") And (Active <> 0)" _
                    & " AND (BlogID=" & BlogID & ")" _
                    & " ORDER BY year(dateadded) desc, Month(DateAdded) desc"
                If cs.OpenSQL(SQL) Then
                    qs = blogListQs
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchivedBlogs)
                    Do While cs.OK
                        ArchiveMonth = cs.GetInteger("ArchiveMonth")
                        ArchiveYear = cs.GetInteger("ArchiveYear")
                        NameOfMonth = MonthName(ArchiveMonth)
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveMonth, CStr(ArchiveMonth))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveYear, CStr(ArchiveYear))
                        returnHtml = returnHtml & vbCrLf & vbTab & vbTab & "<li class=""aoBlogArchiveLink""><a href=""?" & qs & """>" & NameOfMonth & "&nbsp;" & ArchiveYear & "</a></li>"
                        Call cs.GoNext()
                    Loop
                End If
                Call cs.Close()
                If returnHtml <> "" Then
                    returnHtml = "" _
                        & vbCrLf & vbTab & "<ul class=""aoBlogArchiveLinkList"">" _
                        & returnHtml _
                        & vbCrLf & vbTab & "</ul>"
                End If
                '
            Catch ex As Exception
                errorReport(cp, ex, "GetFormBlogArchiveDateList")
            End Try
            Return returnHtml
        End Function
        '
        Private Const AnonymousMemberName = "Anonymous"
        Private Const reCaptchaDisplayGuid = "{E9E51C6E-9152-4284-A44F-D3ABC423AB90}"
        Private Const reCaptchaProcessGuid = "{030AC5B0-F796-4EA4-B94C-986B1C29C16C}"
        '
        Private Const BackToRecentPostsMsg = "« Back to recent posts"
        '
        Private Const RSSProcessAddonGuid = "{2119C2DA-1D57-4C32-B13C-28CD2D85EDF5}"
        '
        ' copy that will be used as the automatic first post if the virtual file blogs/DefaultPostCopy.txt is not found
        '
        Private Const DefaultPostCopy = "This post has been created automatically for you by the system. Verify the blog is set up properly by viewing the blog settings available after turning on Advanced Edit in the toolbar."
        '
        Private Const cnPeople As String = "people"
        Private Const cnBlogs As String = "Blogs"
        Private Const cnBlogCopy As String = "Blog Copy"
        Private Const cnBlogEntries As String = "Blog Entries"
        Private Const cnBlogComments As String = "Blog Comments"
        Private Const cnBlogTypes As String = "Blog Types"
        Private Const cnBlogCategories As String = "Blog Categories"
        Private Const cnBlogCategoryRules As String = "Blog Category Group Rules"
        Private Const cnRSSFeeds As String = "RSS Feeds"
        Private Const cnRSSFeedBlogRules As String = "RSS Feed Blog Rules"
        Private Const cnBlogImages As String = "Blog Images"
        Private Const cnBlogImageRules As String = "Blog Image Rules"
        Private Const cnCTA As String = "Calls to Action"

        '
        Private Const TableNameBlogCategoryRules As String = "ccBlogCategoryGroupRules"
        '
        Private Const SNBlogEntryName As String = "Blog Entry Serial Number"
        Private Const SNBlogCommentName As String = "Blog Comment Serial Number"
        '
        Private Const RequestNameQueryTag As String = "qTag"
        Private Const RequestNameFormID As String = "FormID"
        Private Const RequestNameSourceFormID As String = "SourceFormID"
        Private Const RequestNameBlogTitle As String = "BlogTitle"
        Private Const RequestNameBlogCopy As String = "BlogCopy"
        Private Const RequestNameBlogTagList As String = "BlogTagList"
        Private Const RequestNameDateAdded As String = "DateAdded"
        Private Const RequestNameBlogCategoryID As String = "BlogCategoryID"
        Private Const RequestNameBlogCategoryIDSet As String = "SetBlogCategoryID"
        Private Const RequestNameBlogPodcastMediaLink As String = "PodcastMediaLink"
        Private Const RequestNameAuthorName As String = "AuthorName"
        Private Const RequestNameAuthorEmail As String = "AuthorEmail"
        Private Const RequestNameCommentCopy As String = "CommentCopy"
        Private Const RequestNameCommentTitle As String = "CommentTitle"
        Private Const RequestNameCommentDate As String = "CommentDate"
        Private Const RequestNameApproved As String = "Approved"
        Private Const RequestNameBlogEntryID As String = "BlogEntryID"
        Private Const RequestNameCommentFormKey As String = "formkey"
        Private Const RequestNameKeywordList As String = "keywordlist"
        Private Const RequestNameDateSearch As String = "DateSearch"
        Private Const RequestNameArchiveMonth As String = "ArchiveMonth"
        Private Const RequestNameArchiveYear As String = "ArchiveYear"
        Private Const rnButtonValue As String = "buttonvalue"
        Private Const rnButton As String = "button"
        Private Const rnBlogUploadPrefix As String = "LibraryUpload"
        Private Const rnBlogImageName As String = "LibraryName"
        Private Const rnBlogImageDescription As String = "LibraryDescription"
        Private Const rnBlogImageOrder As String = "LibraryOrder"
        Private Const rnBlogImageDelete As String = "LibraryUploadDelete"
        Private Const rnBlogImageID As String = "LibraryID"
        '
        Private Const SystemEmailBlogNotification As String = "New Blog Notification"
        Private Const SystemEmailCommentNotification As String = "New Blog Comment Notification"
        '
        Private Const VersionSiteProperty As String = "BlogsVersion"
        '
        Private Const FormButtonDelete As String = " Delete "
        Private Const FormButtonCreate As String = " Create "
        Private Const FormButtonPost As String = " Post "
        Private Const FormButtonSearch As String = " Search Blogs "
        Private Const FormButtonPostComment As String = " Post Comment "
        Private Const FormButtonCancel As String = "  Cancel  "
        Private Const FormButtonApply As String = "  Apply  "
        Private Const FormButtonApplyCommentChanges As String = "  Apply Comment Changes  "
        '
        Private Const FormBlogPostList As Integer = 100
        Private Const FormBlogEntryEditor As Integer = 110
        Private Const FormBlogSearch As Integer = 120
        Private Const FormBlogPostDetails As Integer = 300
        Private Const FormBlogArchiveDateList As Integer = 400
        Private Const FormBlogArchivedBlogs As Integer = 600
        '
        Private Const BlogListLayout As String = "{58788483-D050-4464-9261-627278A57B35}"
        Private Const LegacyBlogAddon As String = "{656E95EA-2799-45CD-9712-D4CEDF0E2D02}"
        Private Const facebookLikeAddonGuid As String = "{17919A35-06B3-4F32-9607-4DB3228A15DF}"
    End Class
End Namespace
