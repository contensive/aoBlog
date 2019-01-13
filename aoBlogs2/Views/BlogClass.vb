
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
                Dim blog As BlogModel = BlogModel.verifyBlog(CP, Controllers.InstanceIdController.getInstanceId(CP))
                If (blog Is Nothing) Then Return "<!-- Could not find or create blog from instanceId [" & instanceId & "] -->"
                '
                Dim blogEntry As BlogEntryModel = DbModel.create(Of BlogEntryModel)(CP, CP.Doc.GetInteger(RequestNameBlogEntryID))
                '
                ' -- get the post list (blog list of posts without sidebar)
                Dim BlogBodyController As New BlogBodyClass()
                Dim blogBody As String = BlogBodyController.getBlogBody(CP, blog)
                Dim isBlogBodyDetailForm As Boolean = (blogEntry IsNot Nothing) AndAlso (blogEntry.id <> 0)
                Dim sideBar_ArchiveList As String = ""
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
                    sideBar_ArchiveList = GetFormBlogArchiveDateList(CP, blog.id, blogListQs)
                    If sideBar_ArchiveList = "" Then
                        blog.allowArchiveList = False
                    End If
                End If
                Dim rssFeed = DbModel.create(Of RSSFeedModel)(CP, blog.RSSFeedID)
                Dim allowListSidebar As Boolean = blog.allowEmailSubscribe Or blog.allowFacebookLink Or blog.allowGooglePlusLink Or blog.allowGooglePlusLink Or blog.allowRSSSubscribe Or blog.allowTwitterLink Or blog.allowArchiveList Or blog.allowSearch
                Dim allowArticleSidebar As Boolean = allowListSidebar Or blog.allowArticleCTA
                Dim dstFormId As Integer = CP.Doc.GetInteger(RequestNameFormID)
                Dim allowSidebar As Boolean = Not (dstFormId = FormBlogEntryEditor) And ((isBlogBodyDetailForm And allowArticleSidebar) Or (Not isBlogBodyDetailForm And allowListSidebar))
                '
                ' todo convert sidebar to mustache
                Dim cellList As String = ""
                Dim layout As CPBlockBaseClass = CP.BlockNew()
                layout.OpenLayout(BlogListLayout)
                Dim sidebarCell As CPBlockBaseClass = CP.BlockNew()
                sidebarCell.Load(layout.GetOuter(".blogSidebarCellWrap"))
                Dim cellTemplate As String = sidebarCell.GetHtml()
                Dim adminSuggestions As String = ""
                '
                If isBlogBodyDetailForm Then
                    '
                    ' -- article page
                    Dim blogImageList As List(Of BlogImageModel) = BlogImageModel.createListFromBlogEntry(CP, blogEntry.id)
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
                        Call CP.Doc.SetProperty("Open Graph URL", CP.Content.GetPageLink(CP.Doc.PageId, "BlogEntryID=" & blogEntry.id & "&FormID=300"))
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
                If isBlogBodyDetailForm Then
                    blogBody = Replace(blogBody, "<div class=""aoBlogEntryCopy"">", copy & "<div class=""aoBlogEntryCopy"">")
                End If
                Dim sidebarCnt As Integer = 0
                '
                ' Sidebar
                '
                If allowSidebar Then
                    '
                    If blog.allowArticleCTA And isBlogBodyDetailForm Then
                        '
                        ' CTA cells
                        '
                        Dim blogEntryCtaRuleList = DbModel.createList(Of BlogEntryCTARuleModel)(CP, "blogEntryid=" & blogEntry.id)
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
                            subscribed = CP.User.IsInGroup(blog.emailSubscribeGroupId.ToString())
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
                        If ((rssFeed Is Nothing) OrElse (rssFeed.rssFilename = "")) Then
                            adminSuggestions &= CP.Html.li("This blog includes an RSS Feed, but no feed has been created. It his persists, please contact the site developer. Disable RSS feeds for this blog to hide this message.")
                        Else
                            '
                            sidebarCell.Load(cellTemplate)
                            Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Subscribe By RSS")
                            Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                            'Call sidebarCell.SetInner(".blogSidebarCellCopy", "You are subscribed to this Feed.")
                            Call sidebarCell.SetInner(".blogSidebarCellInputCaption", "<a href=""http://" & CP.Site.DomainPrimary & "/rss/" & rssFeed.rssFilename & """><img id=""blogSidebarRSSLogo"" src=""/blogs/rss.png"" width=""25"" height=""25"">" & blog.name & " Feed" & "</a>")
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
                        Call sidebarCell.SetInner(".blogSidebarCellCopy", sideBar_ArchiveList)
                        Call sidebarCell.SetOuter(".blogSidebarCellInputCaption", "")
                        Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                        Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                        cellList &= vbCrLf & vbTab & "<div id=""blogSidebarArchiveCell"">" & sidebarCell.GetHtml() & "</div>"
                        sidebarCnt += 1
                    End If
                End If
                layout.SetInner(".blogSidebar", cellList)
                layout.Append(CP.Html.Hidden("blogId", blog.id.ToString(), "", "blogId"))
                If sidebarCnt = 0 Then
                    layout.SetInner(".blogWrapper", layout.GetInner(".blogColumn1"))
                End If
                returnHtml = layout.GetHtml()
                returnHtml = returnHtml.Replace("{{legacyBlog}}", blogBody)
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
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchivedBlogs.ToString())
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
    End Class
End Namespace
