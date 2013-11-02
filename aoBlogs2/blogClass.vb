
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
                Dim entryId As Integer = CP.Doc.GetInteger(RequestNameBlogEntryID)
                Dim allowSidebar As Boolean = False
                Dim sidebarCnt As Integer = 0
                Dim cellList As String = ""
                Dim cellTemplate As String = ""
                Dim copy As String
                Dim link As String
                Dim js As String = ""
                Dim emailSubscribeGroupId As Integer
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
                End If
                Call cs.Close()
                '
                Call CP.Doc.SetProperty("blogId", blogId.ToString())
                legacyBlog = CP.Utils.ExecuteAddon(LegacyBlogAddon)
                '

                isArticlePage = (entryId <> 0)
                allowListSidebar = allowEmailSubscribe Or allowFacebookLink Or allowGooglePlusLink Or allowGooglePlusLink Or allowRSSSubscribe Or allowTwitterLink
                allowArticleSidebar = allowListSidebar Or allowArticleCTA
                allowSidebar = Not (dstFormId = FormBlogEntryEditor) And ((isArticlePage And allowArticleSidebar) Or (Not isArticlePage And allowListSidebar))
                '
                cellList = ""
                layout.OpenLayout(BlogListLayout)
                If allowSidebar Then
                    sidebarCell.Load(layout.GetOuter(".blogSidebarCellWrap"))
                    cellTemplate = sidebarCell.GetHtml()
                    '
                    If allowArticleCTA And isArticlePage Then
                        '
                        ' CTA cells
                        '
                        If cs.Open("blog entry cta rules", "blogEntryid=" & entryId) Then
                            Do While cs.OK()
                                If cs2.OpenRecord(cnCTA, cs.GetInteger("CallToActionId")) Then
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
                                        Call sidebarCell.SetInner(".blogSidebarCellButton", "<a href=""" & link & """>" & copy & "</a>")
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
                        sidebarCell.Load(cellTemplate)
                        Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Subscribe By Email")
                        Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                        If CP.User.IsInGroup(emailSubscribeGroupId) Then
                            Call sidebarCell.SetInner(".blogSidebarCellInputCaption", "You are subscribed to this blog.")
                            Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellButton", "")

                        Else
                            Call sidebarCell.SetInner(".blogSidebarCellInputCaption", "Email*")
                            Call sidebarCell.SetInner(".blogSidebarCellInput", "<input type=""text"" id=""blogSubscribeEmail"" name=""email"" value=""" & CP.User.Email & """>")
                            Call sidebarCell.SetInner(".blogSidebarCellButton a", "Subscribe")
                            Call sidebarCell.SetInner(".blogSidebarCellButton", "<a href=""#"" id=""blogSidebarEmailSubscribe"">Subscribe</a>")
                        End If
                        cellList &= vbCrLf & vbTab & sidebarCell.GetHtml()
                        sidebarCnt += 1
                    End If
                End If
                layout.SetInner(".blogSidebar", cellList)
                layout.Append(CP.Html.Hidden("blogId", blogId.ToString(), "", "blogId"))
                If sidebarCnt = 0 Then
                    layout.SetInner(".blogList", layout.GetInner(".blogColumn1"))
                End If
                returnHtml = layout.GetHtml()
                returnHtml = returnHtml.Replace("{{legacyBlog}}", legacyBlog)
                If js <> "" Then
                    CP.Doc.AddHeadJavascript(js)
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
    End Class
End Namespace
