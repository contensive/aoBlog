
using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models;
using Contensive.Blog.Models.Db;
using Contensive.Blog.Views;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        // -- edit wrapper (opening and closing tags split for insertion around card content)
        public string editWrapperStart { get; set; }
        public string editWrapperEnd { get; set; }
        //
        // -- image
        public bool hasImage { get; set; }
        public string imageUrl { get; set; }
        public string imageName { get; set; }
        public string imageClass { get; set; }
        public string imageWidth { get; set; }
        //
        // -- responsive image properties
        public string imageSrc { get; set; }
        public string imageSrcSet { get; set; }
        public string imageSizes { get; set; }
        public int imageHeight { get; set; }
        //
        // -- aspect ratio styling
        public bool manageAspectRatio { get; set; }
        public string styleAspectRatio { get; set; }
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
        // -- remaining images (article view, images not embedded inline)
        public bool hasRemainingImages { get; set; }
        public string remainingImagesHtml { get; set; }
        //
        // -- add image link (article view, blog editor only)
        public bool hasAddImageLink { get; set; }
        public string addImageLinkHtml { get; set; }
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
                    // Get first image (note: this may be a virtual BlogImageModel created from primaryImage fields)
                    var blogImage = blogImageList.First();

                    // Determine aspect ratio (post-specific or blog default)
                    int aspectRatioId = blogPost.primaryImageAspectRatioId > 0
                        ? blogPost.primaryImageAspectRatioId
                        : app.blog.defaultImageAspectRatioId;

                    // Default widths based on view type
                    int effectiveWidth = isArticleView ? 800 : 400;

                    // Calculate height from aspect ratio
                    int effectiveHeight = ImageController.getImageHeight(effectiveWidth, aspectRatioId);

                    // Get srcset/sizes from platform
                    string altSizeList = blogImage.altSizeList ?? "";
                    var imgResult = cp.Image.GetImgSrcSet(blogImage.Filename, effectiveWidth, effectiveHeight, ref altSizeList);

                    // Update altSizeList back to blogPost if this is the primary image
                    // (BlogImageModel.getPostImageList creates virtual model from primaryImage fields)
                    if (altSizeList != blogPost.primaryImageAltSizeList && blogImage.id == 0) {
                        blogPost.primaryImageAltSizeList = altSizeList;
                        blogPost.save(cp);
                    } else if (altSizeList != blogImage.altSizeList && blogImage.id > 0) {
                        // Update actual BlogImageModel record
                        blogImage.altSizeList = altSizeList;
                        blogImage.save(cp);
                    }

                    // Populate view model
                    result.hasImage = true;
                    result.imageName = blogImage.name;
                    result.imageSrc = imgResult.src;
                    result.imageSrcSet = imgResult.srcset;
                    result.imageSizes = imgResult.sizes;
                    result.imageWidth = isArticleView ? "40%" : "25%";
                    result.imageHeight = imgResult.imageHeight;

                    // Backwards compatibility
                    result.imageUrl = imgResult.src;

                    // Position class (keep existing logic)
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

                    // Aspect ratio styling (matches DesignBlocks pattern)
                    result.styleAspectRatio = ImageController.getAspectRatioStyle(aspectRatioId);
                    result.manageAspectRatio = !string.IsNullOrEmpty(result.styleAspectRatio);
                }
                //
                // -- copy
                if (isArticleView) {
                    //
                    // -- determine aspect ratio for all images (same as primary)
                    int imageAspectRatioId = blogPost.primaryImageAspectRatioId > 0
                        ? blogPost.primaryImageAspectRatioId
                        : app.blog.defaultImageAspectRatioId;
                    //
                    // -- process [imageNNN] tags in article copy, replacing with rendered images
                    bool isEditing = app.userIsEditing;
                    var inlineImageIds = new HashSet<int>();
                    string articleCopy = blogPost.copy ?? "";
                    articleCopy = Regex.Replace(articleCopy, @"\[image(\d+)\]", (match) => {
                        int imageId = int.Parse(match.Groups[1].Value);
                        var image = blogImageList.FirstOrDefault(i => i.id == imageId);
                        if (image == null || string.IsNullOrEmpty(image.Filename)) { return ""; }
                        inlineImageIds.Add(imageId);
                        return renderImageHtml(cp, image, imageAspectRatioId, isEditing);
                    }, RegexOptions.IgnoreCase);
                    //
                    // -- in edit mode, inject drop zones between block-level elements
                    if (isEditing) {
                        articleCopy = injectDropZones(articleCopy, blogPost.id);
                    }
                    result.copy = articleCopy;
                    //
                    // -- remaining images: secondary images (id > 0) not embedded inline, with a filename
                    var remainingImages = blogImageList
                        .Where(i => i.id > 0 && !inlineImageIds.Contains(i.id) && !string.IsNullOrEmpty(i.Filename))
                        .ToList();
                    if (remainingImages.Count > 0) {
                        string html = "";
                        foreach (var image in remainingImages) {
                            html += renderImageHtml(cp, image, imageAspectRatioId, isEditing);
                        }
                        result.hasRemainingImages = true;
                        result.remainingImagesHtml = html;
                    }
                    //
                    // -- add image link (blog editor only, article view only)
                    if (app.user != null && app.user.isBlogEditor(cp, app.blog)) {
                        result.hasAddImageLink = true;
                        result.addImageLinkHtml = cp.Content.GetAddLink(BlogImageModel.tableMetadata.contentName, $"blogEntryId={blogPost.id}", false, app.userIsEditing, false);
                    }
                } else {
                    string summaryCopy = Regex.Replace(blogPost.copy, @"\[image\d+\]", "");
                    //
                    // -- downgrade heading tags by one level (h1->h2, h2->h3, etc.)
                    // -- process from h5->h6 down to h1->h2 to avoid double-downgrading
                    for (int h = 5; h >= 1; h--) {
                        summaryCopy = Regex.Replace(summaryCopy, $@"<(/?)\s*h{h}(\s|>|/>)", $"<$1h{h + 1}$2", RegexOptions.IgnoreCase);
                    }
                    result.copy = summaryCopy;
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
                        // -- list view: no tool line (keeps cards uniform)
                    } else {
                        //
                        // -- article view: show all comments
                        string criteria = $"(EntryID={blogPost.id})";
                        if (app.user == null || !app.user.isBlogEditor(cp, app.blog)) {
                            criteria += $"and((Approved<>0)or(createdby={cp.User.Id}))";
                        }
                        var commentList = DbBaseModel.createList<BlogCommentModel>(cp, criteria, "dateAdded");
                        if (commentList.Count > 0) {
                            string divider = "<div class=\"aoBlogCommentDivider\">&nbsp;</div>";
                            string commentsHtml = "<div class=\"aoBlogCommentHeader\">Comments</div>";
                            commentsHtml += "\r\n" + divider;
                            foreach (var blogComment in commentList) {
                                string fieldSuffix = $"{entryIndex}.{commentPtr}";
                                commentsHtml += Views.BlogCommentCellView.getBlogCommentCell(cp, app.blog, blogPost, blogComment, app.user, false, fieldSuffix);
                                commentsHtml += "\r\n" + divider;
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
        //
        //====================================================================================================
        /// <summary>
        /// Render a single blog image as an HTML img tag with responsive srcset/sizes.
        /// Uses the image's own aspect ratio if set, otherwise falls back to defaultAspectRatioId.
        /// When isEditing is true and the image has a record id, wraps with the platform edit wrapper.
        /// </summary>
        private static string renderImageHtml(CPBaseClass cp, BlogImageModel image, int defaultAspectRatioId, bool isEditing) {
            int aspectRatioId = (image.imageAspectRatioId > 0) ? image.imageAspectRatioId : defaultAspectRatioId;
            int imageWidth = 800;
            int imageHeight = ImageController.getImageHeight(imageWidth, aspectRatioId);
            string altSizeList = image.altSizeList ?? "";
            var imgResult = cp.Image.GetImgSrcSet(image.Filename, imageWidth, imageHeight, ref altSizeList);
            if (altSizeList != image.altSizeList) {
                image.altSizeList = altSizeList;
                image.save(cp);
            }
            string aspectClass = ImageController.getAspectRatioStyle(aspectRatioId);
            string altText = cp.Utils.EncodeHTML(image.name ?? image.description ?? "");
            string containerClass = "blogImageContainer" + (string.IsNullOrEmpty(aspectClass) ? "" : $" {aspectClass}");
            bool manageAspect = !string.IsNullOrEmpty(aspectClass);
            string imgTag;
            if (manageAspect) {
                imgTag = $"<img alt=\"{altText}\" title=\"{altText}\" class=\"blogImage\" src=\"{imgResult.src}\" srcset=\"{imgResult.srcset}\" sizes=\"{imgResult.sizes}\" width=\"{imageWidth}\" height=\"{imgResult.imageHeight}\" loading=\"lazy\">";
            } else {
                imgTag = $"<img alt=\"{altText}\" title=\"{altText}\" class=\"w-100 mx-auto d-block\" src=\"{imgResult.src}\" srcset=\"{imgResult.srcset}\" sizes=\"{imgResult.sizes}\" width=\"{imageWidth}\" height=\"{imgResult.imageHeight}\" loading=\"lazy\" style=\"height:auto\">";
            }
            string dragAttrs = (isEditing && image.id > 0)
                ? $" draggable=\"true\" data-blog-image-id=\"{image.id}\""
                : "";
            string html = $"<div class=\"{containerClass} my-3\"{dragAttrs}>{imgTag}</div>";
            //
            // -- wrap secondary images (id > 0) with edit wrapper when in edit mode
            if (isEditing && image.id > 0) {
                html = _GenericController.addEditWrapper(cp, html, image.id, image.name ?? "", BlogImageModel.tableMetadata.contentName);
            }
            //
            // -- wrap in blogInlineImage so injectDropZones skips this element
            // -- uses a non-block tag name so the drop zone regex does not match boundaries around images
            html = $"<blog-image class=\"blogInlineImage\">{html}</blog-image>";
            return html;
        }
        //
        //====================================================================================================
        /// <summary>
        /// In edit mode, inject drop zone HTML between block-level elements in the rendered article copy.
        /// Each drop zone has a data-drop-position attribute (0=before all, N=between blocks, last=after all)
        /// and a data-post-id attribute for the remote method call.
        /// </summary>
        private static string injectDropZones(string copy, int postId) {
            string blockTags = "p|h[1-6]|div|blockquote|ul|ol|table|figure|section|hr|pre";
            string pattern = $@"(<\/(?:{blockTags})\s*>)(\s*)(<(?:{blockTags})[\s>])";
            int positionCounter = 0;
            //
            string dropZoneHtml(int pos) {
                return $"<div class=\"blogImageDropZone\" data-drop-position=\"{pos}\" data-post-id=\"{postId}\"><span>+ New Image or Drag Image here</span></div>";
            }
            //
            // -- protect inline images from drop zone injection by replacing with placeholders
            var imagePlaceholders = new Dictionary<string, string>();
            int placeholderIndex = 0;
            copy = Regex.Replace(copy, @"<blog-image\b[^>]*>[\s\S]*?</blog-image>", (match) => {
                string key = $"__BLOGIMG{placeholderIndex++}__";
                imagePlaceholders[key] = match.Value;
                return key;
            }, RegexOptions.IgnoreCase);
            //
            // -- insert drop zones between block elements (not before the first)
            string result = Regex.Replace(copy, pattern, (match) => {
                int pos = positionCounter++;
                return $"{match.Groups[1].Value}{dropZoneHtml(pos)}{match.Groups[3].Value}";
            }, RegexOptions.IgnoreCase);
            //
            // -- insert drop zone at the very end
            result += dropZoneHtml(positionCounter);
            //
            // -- restore inline images
            foreach (var kvp in imagePlaceholders) {
                result = result.Replace(kvp.Key, kvp.Value);
            }
            return result;
        }
    }
}
