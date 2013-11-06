
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Contensive.Addons.aoBlogs2
    '
    ' Sample Vb addon
    '
    Public Class blogClass
        Inherits AddonBaseClass
        '
        ' - Create a Contensive Addon record, set the dotnet class full name to yourNameSpaceName.yourClassName
        '
        '=====================================================================================
        ' addon api
        '=====================================================================================
        '
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim returnHtml As String
            Try
                Dim layout As CPBlockBaseClass = CP.BlockNew()
                Dim sidebarCell As CPBlockBaseClass = CP.BlockNew()
                Dim legacyBlog As String
                Dim blogName As String = CP.Doc.GetText("blogname")
                Dim blogId As Integer = 0
                Dim cs As CPCSBaseClass = CP.CSNew()
                Dim cs2 As CPCSBaseClass = CP.CSNew()
                Dim allowArticleCTA As Boolean = False
                Dim allowEmailSubscribe As Boolean = False
                Dim allowRSSSubscribe As Boolean = False
                Dim allowFacebookLink As Boolean = False
                Dim facebookLink As String = ""
                Dim allowTwitterLink As Boolean = False
                Dim twitterLink As String = ""
                Dim allowGooglePlusLink As Boolean = False
                Dim googlePlusLink As String = ""
                Dim allowListSidebar As Boolean = False
                Dim allowArticleSidebar As Boolean = False
                Dim srcFormId As Integer = CP.Doc.GetInteger(RequestNameSourceFormID)
                Dim dstFormId As Integer = CP.Doc.GetInteger(RequestNameFormID)
                Dim isArticlePage As Boolean = False
                Dim blogEntryId As Integer = CP.Doc.GetInteger(RequestNameBlogEntryID)
                Dim allowSidebar As Boolean = False
                Dim sidebarCnt As Integer = 0
                Dim cellList As String = ""
                Dim cellTemplate As String = ""
                Dim copy As String = ""
                Dim link As String
                Dim js As String = ""
                Dim emailSubscribeGroupId As Integer
                Dim rssFeedId As Integer
                Dim RSSFilename As String
                Dim followUsCaption As String = ""
                Dim subscribed As Boolean = False
                Dim sql As String
                Dim imageFilename As String
                Dim siteName As String
                Dim blogEntryName As String
                Dim blogEntryBrief As String
                Dim adminSuggestions As String = ""
                '
                If blogName = "" Then
                    blogName = "Default"
                End If
                If cs.Open("blogs", "name=" & CP.Db.EncodeSQLText(blogName)) Then
                    blogId = cs.GetInteger("id")
                    allowArticleCTA = cs.GetBoolean("allowArticleCTA")
                    allowEmailSubscribe = cs.GetBoolean("allowEmailSubscribe")
                    allowFacebookLink = cs.GetBoolean("allowFacebookLink")
                    allowTwitterLink = cs.GetBoolean("allowTwitterLink")
                    allowGooglePlusLink = cs.GetBoolean("allowGooglePlusLink")
                    facebookLink = cs.GetText("facebookLink")
                    twitterLink = cs.GetText("twitterLink")
                    googlePlusLink = cs.GetText("googlePlusLink")
                    emailSubscribeGroupId = cs.GetInteger("emailSubscribeGroupId")
                    rssFeedId = cs.GetInteger("rssFeedId")
                    allowRSSSubscribe = cs.GetBoolean("allowRSSSubscribe")
                    followUsCaption = cs.GetText("followUsCaption")
                End If
                Call cs.Close()
                '
                ' execute legaacy 
                '
                Call CP.Doc.SetProperty("blogId", blogId.ToString())
                legacyBlog = CP.Utils.ExecuteAddon(LegacyBlogAddon)
                isArticlePage = (blogEntryId <> 0)
                allowListSidebar = allowEmailSubscribe Or allowFacebookLink Or allowGooglePlusLink Or allowGooglePlusLink Or allowRSSSubscribe Or allowTwitterLink
                allowArticleSidebar = allowListSidebar Or allowArticleCTA
                allowSidebar = Not (dstFormId = FormBlogEntryEditor) And ((isArticlePage And allowArticleSidebar) Or (Not isArticlePage And allowListSidebar))
                '
                cellList = ""
                layout.OpenLayout(BlogListLayout)
                sidebarCell.Load(layout.GetOuter(".blogSidebarCellWrap"))
                cellTemplate = sidebarCell.GetHtml()
                '
                ' Set Open Graph
                '
                If isArticlePage Then
                    '
                    ' article
                    '
                    imageFilename = ""
                    sql = "select filename from blogImages i inner join blogImageRules r on r.blogImageId=i.id where r.blogentryId=" & blogEntryId & " order by r.sortOrder"
                    If cs.OpenSQL(sql) Then
                        imageFilename = cs.GetText("filename")
                    End If
                    Call cs.Close()
                    '
                    blogEntryName = ""
                    blogEntryBrief = ""
                    If cs.Open("blog entries", "id=" & blogEntryId) Then
                        blogEntryName = cs.GetText("name")
                        blogEntryBrief = cs.GetText("rssDescription")
                        If blogEntryBrief = "" Then
                            blogEntryBrief = CP.Utils.DecodeHTML(cs.GetText("copy"))
                        End If
                    End If
                    Call cs.Close()
                    '
                    If blogEntryName <> "" Then
                        siteName = CP.Site.GetProperty("facebook site_name")
                        If siteName = "" Then
                            Call CP.Site.LogWarning("Facebook site name is not set", "", "Facebook site name missing", "")
                            siteName = CP.Site.Name
                        End If
                        If imageFilename <> "" Then
                            Call CP.Doc.SetProperty("Open Graph Image", "http://" & CP.Site.Domain & CP.Site.FilePath & imageFilename)
                        Else
                            adminSuggestions &= CP.Html.li("This blog entry has no image. Adding an image will improve your social media appeal.")
                        End If
                        Call CP.Doc.SetProperty("Open Graph Site Name", CP.Utils.EncodeHTML(siteName))
                        Call CP.Doc.SetProperty("Open Graph Content Type", "website")
                        Call CP.Doc.SetProperty("Open Graph URL", CP.Content.GetPageLink(CP.Doc.PageId, "BlogEntryID=" & blogEntryId & "&FormID=300"))
                        Call CP.Doc.SetProperty("Open Graph Title", blogEntryName)
                        Call CP.Doc.SetProperty("Open Graph Description", blogEntryBrief)
                    End If
                Else
                    '
                    ' main blog will be handled by the content page
                    '
                End If
                '
                ' social media likes
                '
                Call sidebarCell.SetOuter(".blogSidebarCellHeadline", "")
                Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                Call sidebarCell.SetInner(".blogSidebarCellInputCaption", CP.Utils.ExecuteAddon(facebookLikeAddonGuid))
                Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                copy = sidebarCell.GetHtml()
                If isArticlePage Then
                    legacyBlog = Replace(legacyBlog, "<div class=""aoBlogEntryCopy"">", copy & "<div class=""aoBlogEntryCopy"">")
                End If
                '
                ' Sidebar
                '
                If allowSidebar Then
                    '
                    If allowArticleCTA And isArticlePage Then
                        '
                        ' CTA cells
                        '
                        If cs.Open("blog entry cta rules", "blogEntryid=" & blogEntryId) Then
                            Do While cs.OK()
                                If cs2.Open(cnCTA, "id=" & cs.GetInteger("CallToActionId")) Then
                                    sidebarCell.Load(cellTemplate)
                                    '
                                    copy = cs2.GetText("headline")
                                    If copy = "" Then
                                        Call sidebarCell.SetOuter(".blogSidebarCellHeadline", "")
                                    Else
                                        Call sidebarCell.SetInner(".blogSidebarCellHeadline", copy)
                                    End If
                                    '
                                    copy = cs2.GetText("brief")
                                    If copy = "" Then
                                        Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                                    Else
                                        Call sidebarCell.SetInner(".blogSidebarCellCopy", copy)
                                    End If
                                    '
                                    Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                                    Call sidebarCell.SetOuter(".blogSidebarCellInputCaption", "")
                                    '
                                    copy = cs2.GetText("name")
                                    link = cs2.GetText("link")
                                    If (copy = "") Or (link = "") Then
                                        Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                                    Else
                                        Call sidebarCell.SetInner(".blogSidebarCellButton", "<a target=""_blank"" href=""" & link & """>" & copy & "</a>")
                                    End If

                                    'Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Subscribe By Email")
                                    'Call sidebarCell.SetInner(".blogSidebarCellCopy", "")
                                    'Call sidebarCell.SetInner(".blogSidebarCellInputCaption", "Email*")
                                    'Call sidebarCell.SetInner(".blogSidebarCellButton a", "Subscribe")
                                    cellList &= vbCrLf & vbTab & sidebarCell.GetHtml()
                                    sidebarCnt += 1
                                End If
                                Call cs2.Close()
                                Call cs.GoNext()
                            Loop
                        End If
                        Call cs.Close()
                    End If
                    '
                    If allowEmailSubscribe Then
                        '
                        ' Subscribe by email
                        '
                        subscribed = CP.Visit.GetBoolean("EmailSubscribed-Blog" & blogId & "-user" & CP.User.Id)
                        If Not subscribed Then
                            subscribed = CP.User.IsInGroup(emailSubscribeGroupId)
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
                    If allowRSSSubscribe And (rssFeedId <> 0) Then
                        '
                        ' Subscribe by RSS
                        '
                        If cs.Open("rss feeds", "id=" & rssFeedId) Then
                            RSSFilename = cs.GetText("RSSFilename ")
                        End If
                        Call cs.Close()
                        '
                        sidebarCell.Load(cellTemplate)
                        Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Subscribe By RSS")
                        Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                        'Call sidebarCell.SetInner(".blogSidebarCellCopy", "You are subscribed to this Feed.")
                        Call sidebarCell.SetInner(".blogSidebarCellInputCaption", "<a href=""http://" & CP.Site.DomainPrimary & "/rss/" & RSSFilename & """><img id=""blogSidebarRSSLogo"" src=""/blogs/rss.png"" width=""25"" height=""25"">" & blogName & " Feed" & "</a>")
                        Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                        Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                        cellList &= vbCrLf & vbTab & "<div id=""blogSidebarRSSCell"">" & sidebarCell.GetHtml() & "</div>"
                        sidebarCnt += 1
                    End If
                    '
                    If allowFacebookLink Or allowGooglePlusLink Or allowTwitterLink Then
                        '
                        ' Social Links
                        '
                        '
                        copy = ""
                        If allowFacebookLink And (facebookLink <> "") Then
                            copy &= "<a href=""" & facebookLink & """ target=""_blank""><img class=""blogSidebarSocialLogo"" src=""/blogs/facebook.jpg"" width=""32"" height=""32""></a>"
                        ElseIf allowFacebookLink Then
                            If CP.User.IsAdmin Then
                                copy &= "<div class=""blogAdminWarning""><h2>Administrator</h2><p>Add a facebook link for this blog, or disable the Allow Facebook Sidebar checkbox.</p></div>"
                            End If
                        End If
                        If allowTwitterLink And (twitterLink <> "") Then
                            copy &= "<a href=""" & twitterLink & """ target=""_blank""><img class=""blogSidebarSocialLogo"" src=""/blogs/twitter.jpg"" width=""32"" height=""32""></a>"
                        ElseIf allowTwitterLink Then
                            If CP.User.IsAdmin Then
                                copy &= "<div class=""blogAdminWarning""><h2>Administrator</h2><p>Add a twitter link for this blog, or disable the Allow Twitter Sidebar checkbox.</p></div>"
                            End If
                        End If
                        If allowTwitterLink And (googlePlusLink <> "") Then
                            copy &= "<a href=""" & googlePlusLink & """ target=""_blank""><img class=""blogSidebarSocialLogo"" src=""/blogs/GooglePlus.jpg"" width=""32"" height=""32""></a>"
                        ElseIf allowGooglePlusLink Then
                            If CP.User.IsAdmin Then
                                copy &= "<div class=""blogAdminWarning""><h2>Administrator</h2><p>Add a GooglePlus link for this blog, or disable the Allow Google Plus Sidebar checkbox.</p></div>"
                            End If
                        End If
                        If copy <> "" Then
                            If followUsCaption = "" Then
                                followUsCaption = "Follow Us"
                            End If
                            sidebarCell.Load(cellTemplate)
                            Call sidebarCell.SetInner(".blogSidebarCellHeadline", followUsCaption)
                            Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                            Call sidebarCell.SetInner(".blogSidebarCellInputCaption", copy)
                            cellList &= vbCrLf & vbTab & "<div id=""blogSidebarSocialCell"">" & sidebarCell.GetHtml() & "</div>"
                            sidebarCnt += 1
                        End If
                    End If
                End If
                layout.SetInner(".blogSidebar", cellList)
                layout.Append(CP.Html.Hidden("blogId", blogId.ToString(), "", "blogId"))
                If sidebarCnt = 0 Then
                    layout.SetInner(".blogWrapper", layout.GetInner(".blogColumn1"))
                End If
                returnHtml = layout.GetHtml()
                returnHtml = returnHtml.Replace("{{legacyBlog}}", legacyBlog)
                If js <> "" Then
                    CP.Doc.AddHeadJavascript(js)
                End If
                If adminSuggestions <> "" And CP.User.IsAdmin() Then
                    returnHtml = "<div class=""ccHintWrapper""><div class=""ccHintWrapperContent""><h2>Administrator</h2><ul>" & adminSuggestions & "</ul></div></div>" & returnHtml
                End If
                '
            Catch ex As Exception
                errorReport(CP, ex, "execute")
                returnHtml = "Visual Studio Contensive Addon - Error response"
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
