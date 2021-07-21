
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.Addons.Blog.Models.View
Imports Contensive.BaseClasses

Namespace Views
    '
    Public Class SidebarView
        '
        Public Shared Function getSidebarView(cp As CPBaseClass, app As ApplicationEnvironmentModel, request As RequestModel, legacyBlogBody As String) As String
            Try
                Dim blog As BlogModel = app.blog
                Dim cellList As String = ""
                Using sidebarCell As CPBlockBaseClass = cp.BlockNew()
                    Dim layout As CPBlockBaseClass = cp.BlockNew()
                    layout.OpenLayout(BlogListLayout)
                    sidebarCell.Load(layout.GetOuter(".blogSidebarCellWrap"))
                    Dim cellTemplate As String = sidebarCell.GetHtml()
                    Dim adminSuggestions As String = ""
                    Dim sidebarCnt As Integer = 0
                    '
                    ' -- wrap the blog body in the wrapper (sidebar, footer, etc)
                    Dim isArticleView As Boolean = (request.blogEntryId > 0)
                    Dim allowListSidebar As Boolean = Blog.allowEmailSubscribe Or Blog.allowFacebookLink Or Blog.allowGooglePlusLink Or Blog.allowGooglePlusLink Or Blog.allowRSSSubscribe Or Blog.allowTwitterLink Or Blog.allowArchiveList Or Blog.allowSearch
                    Dim allowArticleSidebar As Boolean = allowListSidebar Or Blog.allowArticleCTA
                    Dim dstFormId As Integer = cp.Doc.GetInteger(RequestNameFormID)
                    Dim allowSidebar As Boolean = Not (dstFormId = FormBlogEntryEditor) And ((isArticleView And allowArticleSidebar) Or (Not isArticleView And allowListSidebar))
                    '
                    ' Sidebar
                    If allowSidebar Then
                        If Blog.allowArticleCTA And isArticleView Then
                            '
                            ' CTA cells
                            Dim blogEntryCtaRuleList = DbModel.createList(Of BlogEntryCTARuleModel)(cp, "blogEntryid=" & request.blogEntryId)
                            For Each rule In blogEntryCtaRuleList
                                Dim cta = DbModel.create(Of CallsToActionModel)(cp, rule.calltoactionid)
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
                        If Blog.allowEmailSubscribe Then
                            '
                            ' Subscribe by email
                            '
                            Dim subscribed As Boolean = cp.Visit.GetBoolean("EmailSubscribed-Blog" & Blog.id & "-user" & cp.User.Id)
                            If Not subscribed Then
                                subscribed = cp.User.IsInGroup(Blog.emailSubscribeGroupId.ToString())
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
                                Call sidebarCell.SetInner(".blogSidebarCellInput", "<input type=""text"" id=""blogSubscribeEmail"" name=""email"" value=""" & cp.User.Email & """>")
                                Call sidebarCell.SetInner(".blogSidebarCellButton a", "Subscribe")
                                Call sidebarCell.SetInner(".blogSidebarCellButton", "<a href=""#"" id=""blogSidebarEmailSubscribe"">Subscribe</a>")
                            End If
                            cellList &= vbCrLf & vbTab & "<div id=""blogSidebarEmailCell"">" & sidebarCell.GetHtml() & "</div>"
                            sidebarCnt += 1
                        End If
                        '
                        If Blog.allowFacebookLink Or Blog.allowGooglePlusLink Or Blog.allowTwitterLink Then
                            '
                            ' Social Links
                            'Dim sidebarHtml As String = sidebarCell.GetHtml()
                            Dim sidebarHtml As String = ""
                            If Blog.allowFacebookLink And (Blog.facebookLink <> "") Then
                                sidebarHtml &= "<a href=""" & Blog.facebookLink & """ target=""_blank""><img Class=""blogSidebarSocialLogo"" src=""/blogs/facebook.jpg"" width=""32"" height=""32""></a>"
                            ElseIf Blog.allowFacebookLink Then
                                If cp.User.IsAdmin Then
                                    sidebarHtml &= "<div Class=""blogAdminWarning""><h2>Administrator</h2><p>Add a facebook link For this blog, Or disable the Allow Facebook Sidebar checkbox.</p></div>"
                                End If
                            End If
                            If Blog.allowTwitterLink And (Blog.twitterLink <> "") Then
                                sidebarHtml &= "<a href=""" & Blog.twitterLink & """ target=""_blank""><img Class=""blogSidebarSocialLogo"" src=""/blogs/twitter.jpg"" width=""32"" height=""32""></a>"
                            ElseIf Blog.allowTwitterLink Then
                                If cp.User.IsAdmin Then
                                    sidebarHtml &= "<div Class=""blogAdminWarning""><h2>Administrator</h2><p>Add a twitter link For this blog, Or disable the Allow Twitter Sidebar checkbox.</p></div>"
                                End If
                            End If
                            If Blog.allowTwitterLink And (Blog.googlePlusLink <> "") Then
                                sidebarHtml &= "<a href=""" & Blog.googlePlusLink & """ target=""_blank""><img Class=""blogSidebarSocialLogo"" src=""/blogs/GooglePlus.jpg"" width=""32"" height=""32""></a>"
                            ElseIf Blog.allowGooglePlusLink Then
                                If cp.User.IsAdmin Then
                                    sidebarHtml &= "<div Class=""blogAdminWarning""><h2>Administrator</h2><p>Add a GooglePlus link For this blog, Or disable the Allow Google Plus Sidebar checkbox.</p></div>"
                                End If
                            End If
                            If sidebarHtml <> "" Then
                                If Blog.followUsCaption = "" Then
                                    Blog.followUsCaption = "Follow Us"
                                End If
                                sidebarCell.Load(cellTemplate)
                                Call sidebarCell.SetInner(".blogSidebarCellHeadline", Blog.followUsCaption)
                                Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                                Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                                Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                                Call sidebarCell.SetInner(".blogSidebarCellInputCaption", sidebarHtml)
                                cellList &= vbCrLf & vbTab & "<div id=""blogSidebarSocialCell"">" & sidebarCell.GetHtml() & "</div>"
                                sidebarCnt += 1
                            End If
                        End If
                        '
                        If Blog.allowSearch Then
                            '
                            ' Search 
                            Dim formInput As String
                            formInput = cp.Html.InputText("keywordList", cp.Doc.GetText("keywordList"))
                            formInput += cp.Html.Hidden("formid", "120")
                            formInput += cp.Html.Hidden("sourceformid", "120")
                            formInput += cp.Html.Hidden("button", " Search Blogs ")
                            formInput = cp.Html.Form(formInput, "", "blogSidebarSearchForm", "blogSidebarSearchForm")
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
                        If Blog.allowArchiveList Then
                            '
                            ' Archive List
                            Dim sideBar_ArchiveList As String = ""
                            '
                            ' create the article list now - if only the current month, turn if off before setting allowListSidebar
                            ''
                            'Dim blogListQs As String = cp.Doc.RefreshQueryString()
                            'blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, "")
                            'blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameFormID, "")
                            'blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameBlogCategoryID, "")
                            'blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameBlogEntryID, "")
                            Dim blogListLink As String = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, app.blogListLink, "?" & app.blogListLink)
                            sideBar_ArchiveList = SidebarView.GetSidebarArchiveList(cp, app)
                            If (String.IsNullOrEmpty(sideBar_ArchiveList)) Then
                                Blog.allowArchiveList = False
                            Else
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
                        '
                        If Blog.allowRSSSubscribe Then
                            Dim rssFeed = DbModel.create(Of RSSFeedModel)(cp, Blog.RSSFeedID)
                            '
                            If ((rssFeed Is Nothing) OrElse (rssFeed.rssFilename = "")) Then
                                adminSuggestions &= cp.Html.li("This blog includes an RSS Feed, but no feed has been created. It his persists, please contact the site developer. Disable RSS feeds for this blog to hide this message.")
                            Else
                                '
                                sidebarCell.Load(cellTemplate)
                                Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Subscribe By RSS")
                                Call sidebarCell.SetOuter(".blogSidebarCellCopy", "")
                                'http://aoblog/aoblog/files/rssfeeds/RSSFeed8.xml
                                Call sidebarCell.SetInner(".blogSidebarCellInputCaption", "<a href=""http://" & cp.Site.DomainPrimary & cp.Http.CdnFilePathPrefix & rssFeed.rssFilename & """><img id=""blogSidebarRSSLogo"" src=""/blogs/rss.png"" width=""25"" height=""25"">" & blog.name & " Feed" & "</a>")
                                Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                                Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                                cellList &= vbCrLf & vbTab & "<div id=""blogSidebarRSSCell"">" & sidebarCell.GetHtml() & "</div>"
                                sidebarCnt += 1
                            End If
                        End If
                        '
                        If isArticleView Then
                            Dim emtyblogEntryCtaRuleList = DbModel.createList(Of BlogEntryCTARuleModel)(cp, "blogEntryid=" & request.blogEntryId)
                            If emtyblogEntryCtaRuleList.Count > 0 Then

                            Else
                                sidebarCell.Load(cellTemplate)
                                Call sidebarCell.SetInner(".blogSidebarCellHeadline", "")
                                Call sidebarCell.SetInner(".blogSidebarCellCopy", "")
                                Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                                Call sidebarCell.SetOuter(".blogSidebarCellInputCaption", "")
                                If cp.User.IsAdmin Then
                                    cellList &= vbCrLf & vbTab & "<div class=""aoBlogFooterLink"" style=""color:red;"">Blog Post has no CTA selected</a><br></div>"
                                End If
                            End If
                        End If
                        '
                        If Blog.allowArticleCTA Then
                            '
                            ' Call To action List
                            '
                            Dim cta = cp.Content.GetID("Calls To Action")
                            Dim qs As String = ""
                            sidebarCell.Load(cellTemplate)
                            Call sidebarCell.SetInner(".blogSidebarCellHeadline", "Call to Action")
                            Call sidebarCell.SetInner(".blogSidebarCellCopy", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellInputCaption", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellInput", "")
                            Call sidebarCell.SetOuter(".blogSidebarCellButton", "")
                            qs = "cid=" & cp.Content.GetID("Calls To Action")
                            '
                            If cp.User.IsAdmin Then
                                cellList &= vbCrLf & vbTab & "<div class=""aoBlogFooterLink""><a href=""" & cp.Site.GetText("adminUrl") & "?" & qs & """>Add/Edit Site Call-To-Actions</a></div>"
                            End If
                            sidebarCnt += 1
                        End If
                    End If
                    '
                    layout.SetInner(".blogSidebar", cellList)
                    layout.Append(cp.Html.Hidden("blogId", Blog.id.ToString(), "", "blogId"))
                    If sidebarCnt = 0 Then
                        layout.SetInner(".blogWrapper", layout.GetInner(".blogColumn1"))
                    End If
                    Dim resultHtml As String = layout.GetHtml()
                    '
                    ' -- add legacy blog into sidebar wrapper
                    resultHtml = resultHtml.Replace("{{legacyBlog}}", legacyBlogBody)
                    '
                    ' -- add admin hint to the bottom
                    If adminSuggestions <> "" And cp.User.IsAdmin() Then
                        resultHtml &= "<div class=""ccHintWrapper""><div class=""ccHintWrapperContent""><h2>Administrator</h2><ul>" & adminSuggestions & "</ul></div></div>"
                    End If
                    Return resultHtml
                End Using
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        '====================================================================================
        '
        Public Shared Function GetSidebarArchiveList(cp As CPBaseClass, app As ApplicationEnvironmentModel) As String
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
                    & " AND (BlogID=" & app.blog.id & ")" _
                    & " ORDER BY year(dateadded) desc, Month(DateAdded) desc"
                If cs.OpenSQL(SQL) Then
                    qs = app.blogListLink
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
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
        '
    End Class
End Namespace
