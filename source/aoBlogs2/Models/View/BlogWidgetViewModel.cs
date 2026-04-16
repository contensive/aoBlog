using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models.Db;
using Contensive.Blog.Models.View;
using Contensive.DesignBlockBase.Models.View;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;

namespace Contensive.Blog.Models {

    public class BlogWidgetViewModel : DesignBlockViewBaseModel {
        //
        // -- body content (rendered sub-template HTML)
        public string blogBody { get; set; }
        //
        // -- sidebar control
        public bool hasSidebar { get; set; }
        public bool isArticleView { get; set; }
        public string blogId { get; set; }
        //
        // -- email subscribe section
        public bool allowEmailSubscribe { get; set; }
        public bool isSubscribed { get; set; }
        public string userEmail { get; set; }
        //
        // -- social links section
        public bool allowSocialLinks { get; set; }
        public string followUsCaption { get; set; }
        public string facebookLink { get; set; }
        public string twitterLink { get; set; }
        public string googlePlusLink { get; set; }
        public bool hasFacebookLink { get; set; }
        public bool hasTwitterLink { get; set; }
        public bool hasGooglePlusLink { get; set; }
        //
        // -- search section
        public bool allowSearch { get; set; }
        public string searchKeywordValue { get; set; }
        //
        // -- archive list section
        public bool allowArchiveList { get; set; }
        public List<ArchiveItemViewModel> archiveList { get; set; }
        //
        // -- RSS section
        public bool allowRSSSubscribe { get; set; }
        public string rssUrl { get; set; }
        public string rssFeedName { get; set; }
        public string domainPrimary { get; set; }
        //
        // -- CTA section (article view only)
        public bool allowArticleCTA { get; set; }
        public List<CtaItemViewModel> ctaList { get; set; }
        //
        // -- admin
        public bool isAdmin { get; set; }
        public string adminSuggestions { get; set; }
        //
        // -- admin social warnings
        public bool showFacebookWarning { get; set; }
        public bool showTwitterWarning { get; set; }
        public bool showGooglePlusWarning { get; set; }
        //
        // -- no CTA warning (article view, admin, no CTA rules)
        public bool showNoCTAWarning { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Create the ViewModel from the BlogModel settings.
        /// This replaces the logic from BlogWidget.Execute(), BlogBodyView, and SidebarView.
        /// </summary>
        public static BlogWidgetViewModel create(CPBaseClass cp, BlogModel settings) {
            try {
                var result = create<BlogWidgetViewModel>(cp, settings);
                //
                // -- verify blog
                var blogBodyRequest = new BlogBodyRequestModel(cp);
                var blog = BlogModel.verifyBlog(cp, blogBodyRequest.instanceGuid);
                if (blog is null) {
                    if (!string.IsNullOrEmpty(blogBodyRequest.instanceGuid) && cp.User.IsAdmin && cp.User.IsEditing()) {
                        using (DataTable dt = cp.Db.ExecuteQuery($"select id from ccBlogs where (active=0)and(ccguid={cp.Db.EncodeSQLText(blogBodyRequest.instanceGuid)})")) {
                            if (dt.Rows.Count > 0) {
                                result.blogBody = @"<div style=""width:40rem;padding:10px;margin:auto"">This blog is currently inactive. If it's permanently inactive, you can remove the widget. To make it active again, go to the advanced settings and find the 'active' checkbox in the control-info section.</div>";
                                return result;
                            }
                        }
                    }
                    result.blogBody = $"<!-- Could not find or create blog from instanceId [{blogBodyRequest.instanceGuid}] -->";
                    return result;
                }
                //
                // -- build application environment
                var app = new ApplicationEnvironmentModel(cp, blog, blogBodyRequest.entryId);
                var request = new View.RequestModel(cp);
                //
                // -- process form input and get blog body
                bool retryCommentPost = false;
                int dstFormId = request.FormID;
                if (!string.IsNullOrEmpty(request.ButtonValue)) {
                    dstFormId = BlogBodyController.ProcessForm(cp, app, request, ref retryCommentPost);
                }
                result.blogBody = getBodyHtml(cp, app, request, dstFormId, retryCommentPost);
                //
                // -- populate sidebar properties
                result.blogId = blog.id.ToString();
                result.isAdmin = cp.User.IsAdmin;
                result.isArticleView = request.blogEntryId > 0;
                //
                bool allowListSidebar = blog.allowEmailSubscribe | blog.allowFacebookLink | blog.allowGooglePlusLink | blog.allowRSSSubscribe | blog.allowTwitterLink | blog.allowArchiveList | blog.allowSearch;
                bool allowArticleSidebar = allowListSidebar | blog.allowArticleCTA;
                bool isEditForm = dstFormId == constants.FormBlogEntryEditor;
                result.hasSidebar = !isEditForm && (result.isArticleView ? allowArticleSidebar : allowListSidebar);
                //
                if (result.hasSidebar) {
                    //
                    // -- CTA (article view only)
                    result.ctaList = new List<CtaItemViewModel>();
                    if (blog.allowArticleCTA && result.isArticleView) {
                        result.allowArticleCTA = true;
                        var blogEntryCtaRuleList = DbBaseModel.createList<BlogEntryCTARuleModel>(cp, $"blogEntryid={request.blogEntryId}");
                        cp.Log.Debug($"BlogWidgetViewModel.create, CTA check: blog.allowArticleCTA={blog.allowArticleCTA}, isArticleView={result.isArticleView}, blogEntryId={request.blogEntryId}, ctaRuleCount={blogEntryCtaRuleList.Count}");
                        foreach (var rule in blogEntryCtaRuleList) {
                            var cta = DbBaseModel.create<CallsToActionModel>(cp, rule.calltoactionid);
                            if (cta is not null) {
                                cp.Log.Debug($"BlogWidgetViewModel.create, CTA found: id={cta.id}, name={cta.name}, headline={cta.headline}, buttontext={cta.buttontext}, link={cta.link}");
                                result.ctaList.Add(new CtaItemViewModel {
                                    headline = cta.headline,
                                    brief = cta.brief,
                                    buttonText = cta.buttontext,
                                    buttonLink = cta.link,
                                    hasButton = !string.IsNullOrEmpty(cta.buttontext) && !string.IsNullOrEmpty(cta.link)
                                });
                            }
                        }
                        if (blogEntryCtaRuleList.Count == 0 && cp.User.IsAdmin) {
                            result.showNoCTAWarning = true;
                        }
                    } else {
                        cp.Log.Debug($"BlogWidgetViewModel.create, CTA disabled: blog.allowArticleCTA={blog.allowArticleCTA}, isArticleView={result.isArticleView}");
                    }
                    //
                    // -- email subscribe
                    if (blog.allowEmailSubscribe) {
                        result.allowEmailSubscribe = true;
                        result.isSubscribed = cp.Visit.GetBoolean($"EmailSubscribed-Blog{blog.id}-user{cp.User.Id}");
                        if (!result.isSubscribed) {
                            result.isSubscribed = cp.User.IsInGroup(blog.emailSubscribeGroupId.ToString());
                        }
                        result.userEmail = cp.User.Email;
                    }
                    //
                    // -- social links
                    bool hasSocialContent = false;
                    if (blog.allowFacebookLink && !string.IsNullOrEmpty(blog.facebookLink)) {
                        result.hasFacebookLink = true;
                        result.facebookLink = blog.facebookLink;
                        hasSocialContent = true;
                    } else if (blog.allowFacebookLink && cp.User.IsAdmin) {
                        result.showFacebookWarning = true;
                        hasSocialContent = true;
                    }
                    if (blog.allowTwitterLink && !string.IsNullOrEmpty(blog.twitterLink)) {
                        result.hasTwitterLink = true;
                        result.twitterLink = blog.twitterLink;
                        hasSocialContent = true;
                    } else if (blog.allowTwitterLink && cp.User.IsAdmin) {
                        result.showTwitterWarning = true;
                        hasSocialContent = true;
                    }
                    if (blog.allowGooglePlusLink && !string.IsNullOrEmpty(blog.googlePlusLink)) {
                        result.hasGooglePlusLink = true;
                        result.googlePlusLink = blog.googlePlusLink;
                        hasSocialContent = true;
                    } else if (blog.allowGooglePlusLink && cp.User.IsAdmin) {
                        result.showGooglePlusWarning = true;
                        hasSocialContent = true;
                    }
                    if (hasSocialContent) {
                        result.allowSocialLinks = true;
                        result.followUsCaption = string.IsNullOrEmpty(blog.followUsCaption) ? "Follow Us" : blog.followUsCaption;
                    }
                    //
                    // -- search
                    if (blog.allowSearch) {
                        result.allowSearch = true;
                        result.searchKeywordValue = cp.Doc.GetText("keywordList");
                    }
                    //
                    // -- archive list
                    if (blog.allowArchiveList) {
                        result.archiveList = getArchiveList(cp, app);
                        result.allowArchiveList = result.archiveList.Count > 0;
                    }
                    //
                    // -- RSS
                    if (blog.allowRSSSubscribe) {
                        var rssFeed = DbBaseModel.create<RSSFeedModel>(cp, blog.rssFeedId);
                        if (rssFeed is null || string.IsNullOrEmpty(rssFeed.rssFilename)) {
                            result.adminSuggestions += cp.Html.li("This blog includes an RSS Feed, but no feed has been created. If this persists, please contact the site developer. Disable RSS feeds for this blog to hide this message.");
                        } else {
                            result.allowRSSSubscribe = true;
                            result.rssUrl = $"https://{cp.Site.DomainPrimary}{cp.Http.CdnFilePathPrefix}{rssFeed.rssFilename}";
                            result.rssFeedName = $"{blog.name} Feed";
                            result.domainPrimary = cp.Site.DomainPrimary;
                        }
                    }
                }
                //
                // -- wrap admin suggestions
                if (!string.IsNullOrEmpty(result.adminSuggestions) && cp.User.IsAdmin) {
                    result.adminSuggestions = $"<div class=\"ccHintWrapper\"><div class=\"ccHintWrapperContent\"><h2>Administrator</h2><ul>{result.adminSuggestions}</ul></div></div>";
                }
                //
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "BlogWidgetViewModel.create");
                return new BlogWidgetViewModel();
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get the blog body HTML for the current view.
        /// Replaces BlogBodyView.getBlogBody() and BlogBodyView.GetForm().
        /// </summary>
        private static string getBodyHtml(CPBaseClass cp, ApplicationEnvironmentModel app, View.RequestModel request, int dstFormId, bool retryCommentPost) {
            try {
                switch (dstFormId) {
                    case constants.FormBlogPostDetails:
                        return renderArticleView(cp, app, retryCommentPost);
                    case constants.FormBlogArchiveDateList: {
                            var vm = BlogArchiveDateListViewModel.create(cp, app, request);
                            return cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogArchiveDateListView, constants.layoutNameBlogArchiveDateListView, constants.layoutPathFilenameBlogArchiveDateListView), vm);
                        }
                    case constants.FormBlogArchivedBlogs: {
                            var vm = BlogArchivedPostsViewModel.create(cp, app, request);
                            string rendered = cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogArchivedPostsView, constants.layoutNameBlogArchivedPostsView, constants.layoutPathFilenameBlogArchivedPostsView), vm);
                            return cp.Html.Form(rendered);
                        }
                    case constants.FormBlogEntryEditor: {
                            var vm = BlogEditViewModel.create(cp, app, request);
                            string rendered = cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogEditView, constants.layoutNameBlogEditView, constants.layoutPathFilenameBlogEditView), vm);
                            return cp.Html.Form(rendered);
                        }
                    case constants.FormBlogSearch: {
                            var vm = BlogSearchViewModel.create(cp, app, request);
                            string rendered = cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogSearchView, constants.layoutNameBlogSearchView, constants.layoutPathFilenameBlogSearchView), vm);
                            return cp.Html.Form(rendered);
                        }
                    default:
                        if (app.blogPost is not null) {
                            return renderArticleView(cp, app, retryCommentPost);
                        }
                        var listVm = BlogListViewModel.create(cp, app, request);
                        return cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogListView, constants.layoutNameBlogListView, constants.layoutPathFilenameBlogListView), listVm);
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Render article view: create ViewModel, render mustache template, wrap in form, add edit wrapper.
        /// </summary>
        private static string renderArticleView(CPBaseClass cp, ApplicationEnvironmentModel app, bool retryCommentPost) {
            var vm = BlogArticleViewModel.create(cp, app, retryCommentPost);
            string rendered = cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogArticleView, constants.layoutNameBlogArticleView, constants.layoutPathFilenameBlogArticleView), vm);
            string result = cp.Html.Form(rendered);
            //
            // -- if editing enabled, add the edit wrapper
            if (app.blogPost is not null) {
                result = Controllers._GenericController.addEditWrapper(cp, result, app.blogPost.id, app.blogPost.name, BlogEntryModel.tableMetadata.contentName);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get the archive list for the sidebar using DataTable instead of CSNew.
        /// Replaces SidebarView.getSidebarArchiveList().
        /// </summary>
        private static List<ArchiveItemViewModel> getArchiveList(CPBaseClass cp, ApplicationEnvironmentModel app) {
            var archiveList = new List<ArchiveItemViewModel>();
            try {
                string sql = $@"
                    SELECT DISTINCT
                        Month(COALESCE(datePublished,dateAdded)) as ArchiveMonth,
                        Year(COALESCE(datePublished,dateAdded)) as ArchiveYear
                    FROM ccBlogCopy
                    WHERE
                        (ContentControlID = {cp.Content.GetID(constants.cnBlogEntries)})
                        AND (Active <> 0)
                        AND (BlogID={app.blog.id})
                    ORDER BY
                        Year(COALESCE(datePublished,dateAdded)) DESC,
                        Month(COALESCE(datePublished,dateAdded)) DESC";
                using (DataTable dt = cp.Db.ExecuteQuery(sql)) {
                    string blogPageUrl = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", app.blog.name);
                    string qsBase = cp.Utils.ModifyQueryString("", constants.rnFormID, constants.FormBlogArchivedBlogs.ToString());
                    foreach (DataRow row in dt.Rows) {
                        int archiveMonth = cp.Utils.EncodeInteger(row["ArchiveMonth"]);
                        int archiveYear = cp.Utils.EncodeInteger(row["ArchiveYear"]);
                        string archiveMonthName = DateAndTime.MonthName(archiveMonth < 1 ? 1 : archiveMonth > 12 ? 12 : archiveMonth);
                        string linkAlias = $"{blogPageUrl}-archives-{archiveMonthName}-{archiveYear}";
                        string qs = cp.Utils.ModifyQueryString(qsBase, constants.RequestNameArchiveMonth, archiveMonth.ToString());
                        qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameArchiveYear, archiveYear.ToString());
                        cp.Site.AddLinkAlias(linkAlias, cp.Doc.PageId, qs);
                        string archiveUrl = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, qs, linkAlias);
                        archiveList.Add(new ArchiveItemViewModel {
                            monthName = archiveMonthName,
                            year = archiveYear.ToString(),
                            url = archiveUrl
                        });
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return archiveList;
        }
    }
}
