
using Contensive.Models.Db;
using System;
using Contensive.Addons.Blog.Models;
using Contensive.Addons.Blog.Models.View;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;

namespace Contensive.Addons.Blog.Views {
    // 
    public class SidebarView {
        // 
        public static string getSidebarView(CPBaseClass cp, ApplicationEnvironmentModel app, RequestModel request, string legacyBlogBody) {
            try {
                var blog = app.blog;
                string cellList = "";
                using (var sidebarCell = cp.BlockNew()) {
                    var layout = cp.BlockNew();
                    layout.OpenLayout(constants.BlogListLayout);
                    sidebarCell.Load(layout.GetOuter(".blogSidebarCellWrap"));
                    string cellTemplate = sidebarCell.GetHtml();
                    string adminSuggestions = "";
                    int sidebarCnt = 0;
                    // 
                    // -- wrap the blog body in the wrapper (sidebar, footer, etc)
                    bool isArticleView = request.blogEntryId > 0;
                    bool allowListSidebar = blog.allowEmailSubscribe | blog.allowFacebookLink | blog.allowGooglePlusLink | blog.allowGooglePlusLink | blog.allowRSSSubscribe | blog.allowTwitterLink | blog.allowArchiveList | blog.allowSearch;
                    bool allowArticleSidebar = allowListSidebar | blog.allowArticleCTA;
                    int dstFormId = cp.Doc.GetInteger(constants.rnFormID);
                    bool allowSidebar = !(dstFormId == constants.FormBlogEntryEditor) & (isArticleView & allowArticleSidebar | !isArticleView & allowListSidebar);
                    // 
                    // Sidebar
                    if (allowSidebar) {
                        if (blog.allowArticleCTA & isArticleView) {
                            // 
                            // CTA cells
                            var blogEntryCtaRuleList = DbBaseModel.createList<BlogEntryCTARuleModel>(cp, "blogEntryid=" + request.blogEntryId);
                            foreach (var rule in blogEntryCtaRuleList) {
                                var cta = DbBaseModel.create<CallsToActionModel>(cp, rule.calltoactionid);
                                if (cta is not null) {
                                    sidebarCell.Load(cellTemplate);
                                    sidebarCell.SetInner(".blogSidebarCellHeadline", cta.headline + "<br>");
                                    sidebarCell.SetInner(".blogSidebarCellCopy", cta.brief);
                                    sidebarCell.SetOuter(".blogSidebarCellInput", "");
                                    sidebarCell.SetOuter(".blogSidebarCellInputCaption", "");
                                    if (string.IsNullOrEmpty(cta.name) | string.IsNullOrEmpty(cta.link)) {
                                        sidebarCell.SetOuter(".blogSidebarCellButton", "");
                                    }
                                    else {
                                        sidebarCell.SetInner(".blogSidebarCellButton", "<a target=\"_blank\" href=\"" + cta.link + "\">" + cta.name + "</a>");
                                    }
                                    cellList += Constants.vbCrLf + Constants.vbTab + sidebarCell.GetHtml();
                                    sidebarCnt += 1;
                                }
                            }
                        }
                        // 
                        if (blog.allowEmailSubscribe) {
                            // 
                            // Subscribe by email
                            // 
                            bool subscribed = cp.Visit.GetBoolean("EmailSubscribed-Blog" + blog.id + "-user" + cp.User.Id);
                            if (!subscribed) {
                                subscribed = cp.User.IsInGroup(blog.emailSubscribeGroupId.ToString());
                            }
                            sidebarCell.Load(cellTemplate);
                            sidebarCell.SetInner(".blogSidebarCellHeadline", "Subscribe By Email");
                            sidebarCell.SetOuter(".blogSidebarCellCopy", "");
                            if (subscribed) {
                                sidebarCell.SetInner(".blogSidebarCellInputCaption", "You are subscribed to this blog.");
                                sidebarCell.SetOuter(".blogSidebarCellInput", "");
                                sidebarCell.SetOuter(".blogSidebarCellButton", "");
                            }

                            else {
                                sidebarCell.SetInner(".blogSidebarCellInputCaption", "Email*");
                                sidebarCell.SetInner(".blogSidebarCellInput", "<input type=\"text\" id=\"blogSubscribeEmail\" name=\"email\" value=\"" + cp.User.Email + "\">");
                                sidebarCell.SetInner(".blogSidebarCellButton a", "Subscribe");
                                sidebarCell.SetInner(".blogSidebarCellButton", "<a href=\"#\" id=\"blogSidebarEmailSubscribe\">Subscribe</a>");
                            }
                            cellList += Constants.vbCrLf + Constants.vbTab + "<div id=\"blogSidebarEmailCell\">" + sidebarCell.GetHtml() + "</div>";
                            sidebarCnt += 1;
                        }
                        // 
                        if (blog.allowFacebookLink | blog.allowGooglePlusLink | blog.allowTwitterLink) {
                            // 
                            // Social Links
                            // Dim sidebarHtml As String = sidebarCell.GetHtml()
                            string sidebarHtml = "";
                            if (blog.allowFacebookLink & !string.IsNullOrEmpty(blog.facebookLink)) {
                                sidebarHtml += "<a href=\"" + blog.facebookLink + "\" target=\"_blank\"><img Class=\"blogSidebarSocialLogo\" src=\"/blogs/facebook.jpg\" width=\"32\" height=\"32\"></a>";
                            }
                            else if (blog.allowFacebookLink) {
                                if (cp.User.IsAdmin) {
                                    sidebarHtml += "<div Class=\"blogAdminWarning\"><h2>Administrator</h2><p>Add a facebook link For this blog, Or disable the Allow Facebook Sidebar checkbox.</p></div>";
                                }
                            }
                            if (blog.allowTwitterLink & !string.IsNullOrEmpty(blog.twitterLink)) {
                                sidebarHtml += "<a href=\"" + blog.twitterLink + "\" target=\"_blank\"><img Class=\"blogSidebarSocialLogo\" src=\"/blogs/twitter.jpg\" width=\"32\" height=\"32\"></a>";
                            }
                            else if (blog.allowTwitterLink) {
                                if (cp.User.IsAdmin) {
                                    sidebarHtml += "<div Class=\"blogAdminWarning\"><h2>Administrator</h2><p>Add a twitter link For this blog, Or disable the Allow Twitter Sidebar checkbox.</p></div>";
                                }
                            }
                            if (blog.allowTwitterLink & !string.IsNullOrEmpty(blog.googlePlusLink)) {
                                sidebarHtml += "<a href=\"" + blog.googlePlusLink + "\" target=\"_blank\"><img Class=\"blogSidebarSocialLogo\" src=\"/blogs/GooglePlus.jpg\" width=\"32\" height=\"32\"></a>";
                            }
                            else if (blog.allowGooglePlusLink) {
                                if (cp.User.IsAdmin) {
                                    sidebarHtml += "<div Class=\"blogAdminWarning\"><h2>Administrator</h2><p>Add a GooglePlus link For this blog, Or disable the Allow Google Plus Sidebar checkbox.</p></div>";
                                }
                            }
                            if (!string.IsNullOrEmpty(sidebarHtml)) {
                                if (string.IsNullOrEmpty(blog.followUsCaption)) {
                                    blog.followUsCaption = "Follow Us";
                                }
                                sidebarCell.Load(cellTemplate);
                                sidebarCell.SetInner(".blogSidebarCellHeadline", blog.followUsCaption);
                                sidebarCell.SetOuter(".blogSidebarCellCopy", "");
                                sidebarCell.SetOuter(".blogSidebarCellInput", "");
                                sidebarCell.SetOuter(".blogSidebarCellButton", "");
                                sidebarCell.SetInner(".blogSidebarCellInputCaption", sidebarHtml);
                                cellList += Constants.vbCrLf + Constants.vbTab + "<div id=\"blogSidebarSocialCell\">" + sidebarCell.GetHtml() + "</div>";
                                sidebarCnt += 1;
                            }
                        }
                        // 
                        if (blog.allowSearch) {
                            // 
                            // Search 
                            string formInput;
                            formInput = cp.Html.InputText("keywordList", cp.Doc.GetText("keywordList"));
                            formInput += cp.Html.Hidden("formid", "120");
                            formInput += cp.Html.Hidden("sourceformid", "120");
                            formInput += cp.Html.Hidden("button", " Search Blogs ");
                            formInput = cp.Html.Form(formInput, "", "blogSidebarSearchForm", "blogSidebarSearchForm");
                            sidebarCell.Load(cellTemplate);
                            sidebarCell.SetInner(".blogSidebarCellHeadline", "Search");
                            sidebarCell.SetOuter(".blogSidebarCellCopy", "");
                            sidebarCell.SetOuter(".blogSidebarCellInputCaption", "");
                            sidebarCell.SetInner(".blogSidebarCellInput", formInput);
                            sidebarCell.SetInner(".blogSidebarCellButton", "<a href=\"#\" id=\"blogSidebarSearch\" onclick=\"jQuery('#blogSidebarSearchForm').submit();return false;\">Search</a>");
                            cellList += Constants.vbCrLf + Constants.vbTab + "<div id=\"blogSidebarSearchCell\">" + sidebarCell.GetHtml() + "</div>";
                            sidebarCnt += 1;
                        }
                        // 
                        if (blog.allowArchiveList) {
                            // 
                            // Archive List
                            string sideBar_ArchiveList = "";
                            // 
                            // create the article list now - if only the current month, turn if off before setting allowListSidebar
                            // '
                            // Dim blogListQs As String = cp.Doc.RefreshQueryString()
                            // blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, "")
                            // blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameFormID, "")
                            // blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameBlogCategoryID, "")
                            // blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameBlogEntryID, "")
                            string blogListLink = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, app.blogBaseLink, "?" + app.blogBaseLink);
                            sideBar_ArchiveList = GetSidebarArchiveList(cp, app);
                            if (string.IsNullOrEmpty(sideBar_ArchiveList)) {
                                blog.allowArchiveList = false;
                            }
                            else {
                                sidebarCell.Load(cellTemplate);
                                sidebarCell.SetInner(".blogSidebarCellHeadline", "Archives");
                                sidebarCell.SetInner(".blogSidebarCellCopy", sideBar_ArchiveList);
                                sidebarCell.SetOuter(".blogSidebarCellInputCaption", "");
                                sidebarCell.SetOuter(".blogSidebarCellInput", "");
                                sidebarCell.SetOuter(".blogSidebarCellButton", "");
                                cellList += Constants.vbCrLf + Constants.vbTab + "<div id=\"blogSidebarArchiveCell\">" + sidebarCell.GetHtml() + "</div>";
                                sidebarCnt += 1;
                            }
                        }
                        // 
                        if (blog.allowRSSSubscribe) {
                            var rssFeed = DbBaseModel.create<RSSFeedModel>(cp, blog.RSSFeedID);
                            // 
                            if (rssFeed is null || string.IsNullOrEmpty(rssFeed.rssFilename)) {
                                adminSuggestions += cp.Html.li("This blog includes an RSS Feed, but no feed has been created. It his persists, please contact the site developer. Disable RSS feeds for this blog to hide this message.");
                            }
                            else {
                                // 
                                sidebarCell.Load(cellTemplate);
                                sidebarCell.SetInner(".blogSidebarCellHeadline", "Subscribe By RSS");
                                sidebarCell.SetOuter(".blogSidebarCellCopy", "");
                                // http://aoblog/aoblog/files/rssfeeds/RSSFeed8.xml
                                sidebarCell.SetInner(".blogSidebarCellInputCaption", "<a href=\"http://" + cp.Site.DomainPrimary + cp.Http.CdnFilePathPrefix + rssFeed.rssFilename + "\"><img id=\"blogSidebarRSSLogo\" src=\"/blogs/rss.png\" width=\"25\" height=\"25\">" + blog.name + " Feed" + "</a>");
                                sidebarCell.SetOuter(".blogSidebarCellInput", "");
                                sidebarCell.SetOuter(".blogSidebarCellButton", "");
                                cellList += Constants.vbCrLf + Constants.vbTab + "<div id=\"blogSidebarRSSCell\">" + sidebarCell.GetHtml() + "</div>";
                                sidebarCnt += 1;
                            }
                        }
                        // 
                        if (isArticleView) {
                            var emtyblogEntryCtaRuleList = DbBaseModel.createList<BlogEntryCTARuleModel>(cp, "blogEntryid=" + request.blogEntryId);
                            if (emtyblogEntryCtaRuleList.Count > 0) {
                            }

                            else {
                                sidebarCell.Load(cellTemplate);
                                sidebarCell.SetInner(".blogSidebarCellHeadline", "");
                                sidebarCell.SetInner(".blogSidebarCellCopy", "");
                                sidebarCell.SetOuter(".blogSidebarCellInput", "");
                                sidebarCell.SetOuter(".blogSidebarCellInputCaption", "");
                                if (cp.User.IsAdmin) {
                                    cellList += Constants.vbCrLf + Constants.vbTab + "<div class=\"aoBlogFooterLink\" style=\"color:red;\">Blog Post has no CTA selected</a><br></div>";
                                }
                            }
                        }
                        // 
                        if (blog.allowArticleCTA) {
                            // 
                            // Call To action List
                            // 
                            int cta = cp.Content.GetID("Calls To Action");
                            string qs = "";
                            sidebarCell.Load(cellTemplate);
                            sidebarCell.SetInner(".blogSidebarCellHeadline", "Call to Action");
                            sidebarCell.SetInner(".blogSidebarCellCopy", "");
                            sidebarCell.SetOuter(".blogSidebarCellInputCaption", "");
                            sidebarCell.SetOuter(".blogSidebarCellInput", "");
                            sidebarCell.SetOuter(".blogSidebarCellButton", "");
                            qs = "cid=" + cp.Content.GetID("Calls To Action");
                            // 
                            if (cp.User.IsAdmin) {
                                cellList += Constants.vbCrLf + Constants.vbTab + "<div class=\"aoBlogFooterLink\"><a href=\"" + cp.Site.GetText("adminUrl") + "?" + qs + "\">Add/Edit Site Call-To-Actions</a></div>";
                            }
                            sidebarCnt += 1;
                        }
                    }
                    // 
                    layout.SetInner(".blogSidebar", cellList);
                    layout.Append(cp.Html.Hidden("blogId", blog.id.ToString(), "", "blogId"));
                    if (sidebarCnt == 0) {
                        layout.SetInner(".blogWrapper", layout.GetInner(".blogColumn1"));
                    }
                    string resultHtml = layout.GetHtml();
                    // 
                    // -- add legacy blog into sidebar wrapper
                    resultHtml = resultHtml.Replace("{{legacyBlog}}", legacyBlogBody);
                    // 
                    // -- add admin hint to the bottom
                    if (!string.IsNullOrEmpty(adminSuggestions) & cp.User.IsAdmin) {
                        resultHtml += "<div class=\"ccHintWrapper\"><div class=\"ccHintWrapperContent\"><h2>Administrator</h2><ul>" + adminSuggestions + "</ul></div></div>";
                    }
                    return resultHtml;
                }
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        // ====================================================================================
        // 
        public static string GetSidebarArchiveList(CPBaseClass cp, ApplicationEnvironmentModel app) {
            string returnHtml = "";
            try {
                var cs = cp.CSNew();
                int ArchiveMonth;
                int ArchiveYear;
                string NameOfMonth;
                string SQL;
                // 
                SQL = "SELECT distinct Month(dateAdded) as ArchiveMonth, year(dateAdded) as ArchiveYear " + " From ccBlogCopy" + " Where (ContentControlID = " + cp.Content.GetID(constants.cnBlogEntries) + ") And (Active <> 0)" + " AND (BlogID=" + app.blog.id + ")" + " ORDER BY year(dateAdded) desc, Month(dateAdded) desc";



                if (cs.OpenSQL(SQL)) {
                    //qs = app.blogBaseLink;
                    string qs = cp.Utils.ModifyQueryString("", constants.rnFormID, constants.FormBlogArchivedBlogs.ToString());
                    while (cs.OK()) {
                        ArchiveMonth = cs.GetInteger("ArchiveMonth");
                        ArchiveYear = cs.GetInteger("ArchiveYear");
                        NameOfMonth = DateAndTime.MonthName(ArchiveMonth);
                        qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameArchiveMonth, ArchiveMonth.ToString());
                        qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameArchiveYear, ArchiveYear.ToString());
                        returnHtml = returnHtml + Constants.vbCrLf + Constants.vbTab + Constants.vbTab + "<li class=\"aoBlogArchiveLink\"><a href=\"" + app.blogBaseLink + "?" + qs + "\">" + NameOfMonth + "&nbsp;" + ArchiveYear + "</a></li>";
                        cs.GoNext();
                    }
                }
                cs.Close();
                if (!string.IsNullOrEmpty(returnHtml)) {
                    returnHtml = "" + Constants.vbCrLf + Constants.vbTab + "<ul class=\"aoBlogArchiveLinkList\">" + returnHtml + Constants.vbCrLf + Constants.vbTab + "</ul>";


                }
            }
            // 
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return returnHtml;
        }
        // 
    }
}