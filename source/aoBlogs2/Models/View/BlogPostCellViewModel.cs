
using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models;
using Contensive.Blog.Models.Db;
using Contensive.Blog.Views;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Contensive.Blog.Models.View {
    public class BlogPostCellViewModel {
        //
        // -- post headline
        public string headline { get; set; }
        public string entryLink { get; set; }
        public bool isArticleView { get; set; }
        //
        // -- edit link (injected HTML from framework)
        public string editLinkHtml { get; set; }
        //
        // -- image
        public bool hasImage { get; set; }
        public string imageUrl { get; set; }
        public string imageName { get; set; }
        public string imageClass { get; set; }
        public string imageWidth { get; set; }
        //
        // -- copy content (full for article, brief for list)
        public string copy { get; set; }
        //
        // -- read more link (list view only)
        public bool showReadMore { get; set; }
        //
        // -- podcast
        public bool hasPodcast { get; set; }
        public string podcastHtml { get; set; }
        //
        // -- byline (author, date, comment info)
        public bool hasByLine { get; set; }
        public string byLine { get; set; }
        //
        // -- tags section (article view only)
        public bool hasTags { get; set; }
        public List<TagItemViewModel> tagList { get; set; }
        //
        // -- comments section (article view only)
        public string commentsHtml { get; set; }
        //
        // -- tool line (list view, blog editor only)
        public bool hasToolLine { get; set; }
        public int unapprovedCommentCount { get; set; }
        public string editUrl { get; set; }
        //
        // -- hidden fields data
        public string commentCntName { get; set; }
        public string commentCntValue { get; set; }
        //
        // -- comment count (used by callers)
        public int commentCount { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Create the BlogPostCellViewModel from a blog entry.
        /// Extracts rendering logic from BlogEntryCellView.getBlogPostCell().
        /// </summary>
        public static BlogPostCellViewModel create(CPBaseClass cp, ApplicationEnvironmentModel app, BlogEntryModel blogPost, List<BlogImageModel> blogImageList, bool isArticleView, bool isSearchListing, string entryEditLink, int entryIndex = 0) {
            try {
                if (blogPost is null) { throw new ApplicationException("BlogPostCellViewModel.create called without valid BlogEntry"); }
                //
                var result = new BlogPostCellViewModel();
                result.isArticleView = isArticleView;
                result.headline = blogPost.name;
                //
                // -- verify link alias for this page
                LinkAliasController.addLinkAlias(cp, blogPost.name, blogPost.id, "BlogPostCellViewModel.create()");
                blogPost.blogpostpageid = cp.Doc.PageId;
                blogPost.save(cp);
                //
                string qs = LinkAliasController.getLinkAliasQueryString(cp, blogPost.id);
                result.entryLink = cp.Content.GetPageLink(cp.Doc.PageId, qs);
                result.editLinkHtml = entryEditLink ?? "";
                //
                // -- image
                if (blogImageList.Count > 0 && blogPost.primaryImagePositionId != 4) {
                    string thumbnailFilename = "";
                    string imageFilename = "";
                    string imageName = "";
                    string imageDescription = "";
                    BlogImageView.getBlogImage(cp, app, blogImageList.First(), ref thumbnailFilename, ref imageFilename, ref imageDescription, ref imageName);
                    if (isArticleView || !string.IsNullOrEmpty(thumbnailFilename)) {
                        result.hasImage = true;
                        result.imageName = imageName;
                        result.imageUrl = _GenericController.encodeURLForHrefSrc(cp.Http.CdnFilePathPrefix + thumbnailFilename);
                        result.imageWidth = isArticleView ? "40%" : "25%";
                        switch (blogPost.primaryImagePositionId) {
                            case 2:
                                result.imageClass = "aoBlogEntryThumbnailRight";
                                break;
                            case 3:
                                result.imageClass = "aoBlogEntryThumbnailLeft";
                                break;
                            default:
                                result.imageClass = "aoBlogEntryThumbnail";
                                break;
                        }
                    }
                }
                //
                // -- copy
                if (isArticleView) {
                    result.copy = blogPost.copy;
                } else {
                    result.copy = _GenericController.getBriefCopy(cp, blogPost.copy, app.blog.overviewLength);
                    result.showReadMore = true;
                }
                //
                // -- tags (article view only)
                if (isArticleView && app.sitePropertyAllowTags && !string.IsNullOrEmpty(blogPost.tagList)) {
                    string[] tags = blogPost.tagList.Split(',');
                    var tagItems = new List<TagItemViewModel>();
                    foreach (var tag in tags) {
                        string trimmedTag = tag.Trim();
                        if (!string.IsNullOrEmpty(trimmedTag)) {
                            tagItems.Add(new TagItemViewModel {
                                separator = tagItems.Count > 0 ? ", " : "",
                                name = trimmedTag,
                                url = $"{app.blogBaseLink}?{constants.rnFormID}={constants.FormBlogSearch}&{constants.rnQueryTag}={cp.Utils.EncodeHTML(trimmedTag)}"
                            });
                        }
                    }
                    if (tagItems.Count > 0) {
                        result.hasTags = true;
                        result.tagList = tagItems;
                    }
                }
                //
                // -- podcast
                if (!string.IsNullOrEmpty(blogPost.podcastMediaLink)) {
                    cp.Doc.SetProperty("Media Link", blogPost.podcastMediaLink);
                    cp.Doc.SetProperty("Media Link", blogPost.podcastSize.ToString());
                    cp.Doc.SetProperty("Hide Player", "True");
                    cp.Doc.SetProperty("Auto Start", "False");
                    result.hasPodcast = true;
                    result.podcastHtml = cp.Addon.Execute(constants.addonGuidWebcast);
                }
                //
                // -- byline (author + date + comment info)
                string rowCopy = "";
                var datePublished = blogPost.datePublished ?? blogPost.dateAdded;
                //
                if (blogPost.authorMemberId == 0 && blogPost.createdBy > 0) {
                    blogPost.authorMemberId = cp.Utils.EncodeInteger(blogPost.createdBy);
                    blogPost.save(cp);
                }
                var author = DbBaseModel.create<PersonModel>(cp, blogPost.authorMemberId);
                if (author is not null) {
                    rowCopy += $"By {author.name}";
                    if (datePublished.HasValue && datePublished.Value != DateTime.MinValue) {
                        rowCopy += $" | {cp.Utils.EncodeDate(datePublished):MMMM dd, yyyy}";
                    }
                } else if (datePublished.HasValue && datePublished.Value != DateTime.MinValue) {
                    rowCopy += $"{cp.Utils.EncodeDate(datePublished):MMMM dd, yyyy}";
                }
                var visit = DbBaseModel.create<VisitModel>(cp, cp.Visit.Id);
                bool showComments = blogPost.allowComments && visit is not null && cp.Visit.CookieSupport && !visit.bot;
                if (showComments) {
                    var approvedComments = DbBaseModel.createList<BlogCommentModel>(cp, $"(Approved<>0)and(EntryID={blogPost.id})");
                    if (isArticleView) {
                        if (approvedComments.Count == 1) {
                            rowCopy += " | 1 Comment";
                        } else if (approvedComments.Count > 1) {
                            rowCopy += $" | {approvedComments.Count} Comments&nbsp;({approvedComments.Count})";
                        }
                    } else {
                        if (approvedComments.Count == 0) {
                            rowCopy += $" | <a href=\"{result.entryLink}\">Comment</a>";
                        } else {
                            rowCopy += $" | <a href=\"{result.entryLink}\">Comments</a>&nbsp;({approvedComments.Count})";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(rowCopy)) {
                    result.hasByLine = true;
                    result.byLine = rowCopy;
                }
                //
                // -- comments and tool line
                int commentPtr = 0;
                if (showComments) {
                    if (!isArticleView) {
                        //
                        // -- list view tool line (for blog editors)
                        if (app.user.isBlogEditor(cp, app.blog)) {
                            var unapprovedComments = DbBaseModel.createList<BlogCommentModel>(cp, $"(Approved=0)and(EntryID={blogPost.id})");
                            result.hasToolLine = true;
                            result.unapprovedCommentCount = unapprovedComments.Count;
                            string editQs = app.blogBaseLink;
                            editQs = cp.Utils.ModifyQueryString(editQs, constants.RequestNameBlogEntryID, blogPost.id.ToString());
                            editQs = cp.Utils.ModifyQueryString(editQs, constants.rnFormID, constants.FormBlogEntryEditor.ToString());
                            result.editUrl = $"?{editQs}";
                        }
                    } else {
                        //
                        // -- article view: show all comments
                        string criteria = $"(EntryID={blogPost.id})";
                        if (!app.user.isBlogEditor(cp, app.blog)) {
                            criteria += $"and((Approved<>0)or(createdby={cp.User.Id}))";
                        }
                        var commentList = DbBaseModel.createList<BlogCommentModel>(cp, criteria, "dateAdded");
                        if (commentList.Count > 0) {
                            string divider = "<div class=\"aoBlogCommentDivider\">&nbsp;</div>";
                            string commentsHtml = "<div class=\"aoBlogCommentHeader\">Comments</div>";
                            commentsHtml += Constants.vbCrLf + divider;
                            foreach (var blogComment in commentList) {
                                string fieldSuffix = $"{entryIndex}.{commentPtr}";
                                commentsHtml += Views.BlogCommentCellView.getBlogCommentCell(cp, app.blog, blogPost, blogComment, app.user, false, fieldSuffix);
                                commentsHtml += Constants.vbCrLf + divider;
                                commentPtr++;
                            }
                            result.commentsHtml = commentsHtml;
                        }
                    }
                }
                //
                // -- hidden fields data
                result.commentCntName = $"CommentCnt{entryIndex}";
                result.commentCntValue = commentPtr.ToString();
                result.commentCount = commentPtr;
                //
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "BlogPostCellViewModel.create");
                throw;
            }
        }
    }
}
