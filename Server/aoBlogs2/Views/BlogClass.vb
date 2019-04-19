
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Contensive.Addons.Blog.Controllers
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
                '
                Dim request = New View.RequestModel(CP)
                Dim blog As BlogModel = BlogModel.verifyBlog(CP, request)
                If (blog Is Nothing) Then Return "<!-- Could not find or create blog from instanceId [" & request.instanceId & "] -->"
                '
                ' -- get Blog Body -- the body is the area down the middle that includes the Blog View (Article View, List View, Edit View)
                Dim blogEntry = DbModel.create(Of BlogEntryModel)(CP, request.EntryID)
                Dim blogBody As String = BlogBodyController.getBlogBody(CP, blog, request, blogEntry)
                '
                ' -- wrap the blog body in the wrapper (sidebar, footer, etc)
                Dim isArticleView As Boolean = (request.blogEntryId > 0)
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
                Dim allowSidebar As Boolean = Not (dstFormId = FormBlogEntryEditor) And ((isArticleView And allowArticleSidebar) Or (Not isArticleView And allowListSidebar))
                '
                ' todo convert sidebar to mustache
                Dim cellList As String = ""
                Dim layout As CPBlockBaseClass = CP.BlockNew()
                layout.OpenLayout(BlogListLayout)
                Dim sidebarCell As CPBlockBaseClass = CP.BlockNew()
                sidebarCell.Load(layout.GetOuter(".blogSidebarCellWrap"))
                Dim cellTemplate As String = sidebarCell.GetHtml()
                Dim adminSuggestions As String = ""
                ''
                '' -- social media likes
                'Call sidebarCell.SetOuter(".blogSidebarCellHeadline", "")
                'Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                ''Call sidebarCell.SetInner(".blogSidebarCellInputCaption", CP.Utils.ExecuteAddon(facebookLikeAddonGuid))
                'Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                'Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                'If isArticleView Then
                '    blogBody = Replace(blogBody, "<div class=""aoBlogEntryCopy"">", copy & "<div class=""aoBlogEntryCopy"">")
                'End If
                Dim sidebarCnt As Integer = 0
                '
                ' Sidebar
                If allowSidebar Then
                    If blog.allowArticleCTA And isArticleView Then
                        '
                        ' CTA cells
                        Dim blogEntryCtaRuleList = DbModel.createList(Of BlogEntryCTARuleModel)(CP, "blogEntryid=" & request.blogEntryId)
                        For Each rule In blogEntryCtaRuleList
                            Dim cta = DbModel.create(Of CallsToActionModel)(CP, rule.calltoactionid)
                            If (cta IsNot Nothing) Then
                                sidebarCell.Load(cellTemplate)
                                Call sidebarCell.SetInner(".blogSidebarCellHeadline", cta.headline & "<br>")
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
                    If blog.allowFacebookLink Or blog.allowGooglePlusLink Or blog.allowTwitterLink Then
                        '
                        ' Social Links
                        'Dim sidebarHtml As String = sidebarCell.GetHtml()
                        Dim sidebarHtml As String = ""
                        If blog.allowFacebookLink And (blog.facebookLink <> "") Then
                            sidebarHtml &= "<a href=""" & blog.facebookLink & """ target=""_blank""><img Class=""blogSidebarSocialLogo"" src=""/blogs/facebook.jpg"" width=""32"" height=""32""></a>"
                        ElseIf blog.allowFacebookLink Then
                            If CP.User.IsAdmin Then
                                sidebarHtml &= "<div Class=""blogAdminWarning""><h2>Administrator</h2><p>Add a facebook link For this blog, Or disable the Allow Facebook Sidebar checkbox.</p></div>"
                            End If
                        End If
                        If blog.allowTwitterLink And (blog.twitterLink <> "") Then
                            sidebarHtml &= "<a href=""" & blog.twitterLink & """ target=""_blank""><img Class=""blogSidebarSocialLogo"" src=""/blogs/twitter.jpg"" width=""32"" height=""32""></a>"
                        ElseIf blog.allowTwitterLink Then
                            If CP.User.IsAdmin Then
                                sidebarHtml &= "<div Class=""blogAdminWarning""><h2>Administrator</h2><p>Add a twitter link For this blog, Or disable the Allow Twitter Sidebar checkbox.</p></div>"
                            End If
                        End If
                        If blog.allowTwitterLink And (blog.googlePlusLink <> "") Then
                            sidebarHtml &= "<a href=""" & blog.googlePlusLink & """ target=""_blank""><img Class=""blogSidebarSocialLogo"" src=""/blogs/GooglePlus.jpg"" width=""32"" height=""32""></a>"
                        ElseIf blog.allowGooglePlusLink Then
                            If CP.User.IsAdmin Then
                                sidebarHtml &= "<div Class=""blogAdminWarning""><h2>Administrator</h2><p>Add a GooglePlus link For this blog, Or disable the Allow Google Plus Sidebar checkbox.</p></div>"
                            End If
                        End If
                        If sidebarHtml <> "" Then
                            If blog.followUsCaption = "" Then
                                blog.followUsCaption = "Follow Us"
                            End If
                            sidebarCell.Load(cellTemplate)
                            Call sidebarCell.SetInner(".blogSidebarCellHeadline", blog.followUsCaption)
                            Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                            Call sidebarCell.SetInner(".blogSidebarCellInputCaption", sidebarHtml)
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
                            'http://aoblog/aoblog/files/rssfeeds/RSSFeed8.xml
                            Call sidebarCell.SetInner(".blogSidebarCellInputCaption", "<a href=""http://" & CP.Site.DomainPrimary & CP.Site.FilePath & rssFeed.rssFilename & """><img id=""blogSidebarRSSLogo"" src=""/blogs/rss.png"" width=""25"" height=""25"">" & blog.name & " Feed" & "</a>")
                            Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                            cellList &= vbCrLf & vbTab & "<div id=""blogSidebarRSSCell"">" & sidebarCell.GetHtml() & "</div>"
                            sidebarCnt += 1
                        End If
                    End If
                    '
                    If isArticleView Then
                        Dim emtyblogEntryCtaRuleList = DbModel.createList(Of BlogEntryCTARuleModel)(CP, "blogEntryid=" & request.blogEntryId)
                        If emtyblogEntryCtaRuleList.Count > 0 Then

                        Else
                            sidebarCell.Load(cellTemplate)
                            Call sidebarCell.SetInner(".blogSidebarCellHeadline", "")
                            Call sidebarCell.SetInner(".blogSidebarCellCopy", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellInputCaption", "")
                            cellList &= vbCrLf & vbTab & "<div class=""aoBlogFooterLink"" style=""color:red;"">Blog Post has no CTA selected</a><br></div>"
                        End If
                    End If
                    '
                    If blog.allowArticleCTA Then
                        '
                        ' Call To action List
                        '
                        Dim cta = CP.Content.GetID("Calls To Action")
                        Dim qs As String = ""
                        sidebarCell.Load(cellTemplate)
                        Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Call to Action")
                        Call sidebarCell.SetInner(".blogSidebarCellCopy", "")
                        Call sidebarCell.SetOuter(".blogSidebarCellInputCaption", "")
                        Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                        Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                        qs = "cid=" & CP.Content.GetID("Calls To Action")
                        '
                        cellList &= vbCrLf & vbTab & "<div class=""aoBlogFooterLink""><a href=""" & CP.Site.GetProperty("adminUrl") & "?" & qs & """>Add/Edit Site Call-To-Actions</a></div>"
                        sidebarCnt += 1
                    End If
                End If
                '
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
                    returnHtml &= "<div class=""ccHintWrapper""><div class=""ccHintWrapperContent""><h2>Administrator</h2><ul>" & adminSuggestions & "</ul></div></div>"
                End If
                '
                ' -- if editing enabled, add the link and wrapperwrapper
                returnHtml = genericController.addEditWrapper(CP, returnHtml, blog.id, blog.name, Models.BlogModel.contentName, "Blog Settings")
                '
            Catch ex As Exception
                errorReport(CP, ex, "execute")
            End Try
            '
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
