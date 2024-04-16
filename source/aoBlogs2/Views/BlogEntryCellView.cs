using System;
using System.Linq;
using Contensive.Addons.Blog.Controllers;
using Contensive.Addons.Blog.Models;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Contensive.Addons.Blog.Views {
    // 
    public class BlogEntryCellView {
        // 
        // ====================================================================================
        // 
        public static string getBlogPostCell(CPBaseClass cp, ApplicationEnvironmentModel app, BlogPostModel blogPost, bool isArticleView, bool IsSearchListing, int Return_CommentCnt, string entryEditLink) {
            int hint = 0;
            try {
                string result = "";
                hint = 10;
                // 
                // -- argument test
                if (blogPost is null)
                    throw new ApplicationException("BlogEntryCell called without valid BlogEntry");
                // 
                // -- add link alias for this page
                LinkAliasController.addLinkAlias(cp, blogPost.name, cp.Doc.PageId, blogPost.id);
                // Dim qs As String = cp.Utils.ModifyQueryString("", RequestNameBlogEntryID, CStr(blogPost.id))
                // qs = cp.Utils.ModifyQueryString(qs, rnFormID, FormBlogPostDetails.ToString())
                // Call cp.Site.AddLinkAlias(blogPost.name, cp.Doc.PageId, qs)
                // 
                string qs = LinkAliasController.getLinkAliasQueryString(cp, cp.Doc.PageId, blogPost.id);
                string entryLink = cp.Content.GetPageLink(cp.Doc.PageId, qs);
                var blogImageList = BlogImageModel.createListFromBlogEntry(cp, blogPost.id);
                hint = 20;
                string TagListRow = "";
                if (isArticleView) {
                    hint = 30;
                    // 
                    // -- article view
                    result += "<h1 class=\"aoBlogEntryName\">" + blogPost.name + "</h1>";
                    result += "<div class=\"aoBlogEntryLikeLine\">" + cp.Addon.Execute(constants.facebookLikeAddonGuid) + "</div>";
                    result += "<div class=\"aoBlogEntryCopy\">";
                    if (blogImageList.Count > 0) {
                        hint = 40;
                        string ThumbnailFilename = "";
                        string imageFilename = "";
                        string imageName = "";
                        string imageDescription = "";
                        BlogImageView.getBlogImage(cp, app, blogImageList.First(), ref ThumbnailFilename, ref imageFilename, ref imageDescription, ref imageName);
                        switch (blogPost.primaryImagePositionId) {
                            case 2: {
                                    // 
                                    // align right
                                    result += "<img alt=\"" + imageName + "\" title=\"" + imageName + "\" class=\"aoBlogEntryThumbnailRight\" src=\"" + cp.Http.CdnFilePathPrefix + ThumbnailFilename + "\" style=\"width:40%;\">";
                                    break;
                                }
                            case 3: {
                                    // 
                                    // align left
                                    result += "<img alt=\"" + imageName + "\" title=\"" + imageName + "\" class=\"aoBlogEntryThumbnailLeft\" src=\"" + cp.Http.CdnFilePathPrefix + ThumbnailFilename + "\" style=\"width:40%;\">";
                                    break;
                                }
                            // 
                            // hide
                            case 4: {
                                    break;
                                }

                            default: {
                                    // 
                                    // 1 and none align per stylesheet
                                    result += "<img alt=\"" + imageName + "\" title=\"" + imageName + "\" class=\"aoBlogEntryThumbnail\" src=\"" + cp.Http.CdnFilePathPrefix + ThumbnailFilename + "\" style=\"width:40%;\">";
                                    break;
                                }
                        }
                    }
                    hint = 50;
                    result += blogPost.copy + "</div>";
                    if (app.sitePropertyAllowTags & !string.IsNullOrEmpty(blogPost.tagList)) {
                        hint = 60;
                        // 
                        // -- make a clickable section
                        string clickableLinkList = "";
                        string[] tags = Strings.Split(Strings.Replace(blogPost.tagList, ",", Constants.vbCrLf), Constants.vbCrLf);
                        foreach (var tag in tags) {
                            string Link = app.blogBaseLink + "?" + constants.rnFormID + "=" + constants.FormBlogSearch + "&" + constants.rnQueryTag + "=" + cp.Utils.EncodeHTML(tag);
                            clickableLinkList += ", <a href=\"" + Link + "\">" + tag + "</a>";
                        }
                        TagListRow = "" + "<div class=\"aoBlogTagListSection\">" + "<div class=\"aoBlogTagListHeader\">Tags</div>" + "<div class=\"aoBlogTagList\">" + Strings.Mid(clickableLinkList, 3) + "</div>" + "</div>";



                    }
                }
                else {
                    hint = 70;
                    // 
                    // -- list view
                    result += Constants.vbCrLf + entryEditLink + "<h2 class=\"aoBlogEntryName\"><a href=\"" + entryLink + "\">" + blogPost.name + "</a></h2>";
                    result += "<div class=\"aoBlogEntryCopy\">";
                    if (blogImageList.Count > 0) {
                        hint = 80;
                        string ThumbnailFilename = "";
                        string imageFilename = "";
                        string imageName = "";
                        string imageDescription = "";
                        BlogImageView.getBlogImage(cp, app, blogImageList.First(), ref ThumbnailFilename, ref imageFilename, ref imageDescription, ref imageName);
                        if (!string.IsNullOrEmpty(ThumbnailFilename)) {
                            switch (blogPost.primaryImagePositionId) {
                                case 2: {
                                        // 
                                        // align right
                                        // 
                                        // result &=  "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                        result += "<a href=\"" + entryLink + "\"><img alt=\"" + imageName + "\" title=\"" + imageName + "\" class=\"aoBlogEntryThumbnailRight\" src=\"" + cp.Http.CdnFilePathPrefix + ThumbnailFilename + "\" style=\"width:25%;\"></a>";
                                        break;
                                    }
                                case 3: {
                                        // 
                                        // align left
                                        // 
                                        // result &=  "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                        result += "<a href=\"" + entryLink + "\"><img alt=\"" + imageName + "\" title=\"" + imageName + "\" class=\"aoBlogEntryThumbnailLeft\" src=\"" + cp.Http.CdnFilePathPrefix + ThumbnailFilename + "\" style=\"width:25%;\"></a>";
                                        break;
                                    }
                                // 
                                // hide
                                // 
                                case 4: {
                                        break;
                                    }

                                default: {
                                        // 
                                        // 1 and none align per stylesheet
                                        // 
                                        // result &=  "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Http.CdnFilePathPrefix & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                        result += "<a href=\"" + entryLink + "\"><img alt=\"" + imageName + "\" title=\"" + imageName + "\" class=\"aoBlogEntryThumbnail\" src=\"" + cp.Http.CdnFilePathPrefix + ThumbnailFilename + "\" style=\"width:25%;\"></a>";
                                        break;
                                    }

                            }
                        }
                    }
                    result += "<p>" + genericController.getBriefCopy(cp, blogPost.copy, app.blog.OverviewLength) + "</p></div>";
                    result += "<div class=\"aoBlogEntryReadMore\"><a href=\"" + entryLink + "\">Read More</a></div>";
                }
                hint = 90;
                // 
                // Podcast link
                // 
                if (!string.IsNullOrEmpty(blogPost.PodcastMediaLink)) {
                    cp.Doc.SetProperty("Media Link", blogPost.PodcastMediaLink);
                    cp.Doc.SetProperty("Media Link", blogPost.PodcastSize.ToString());
                    cp.Doc.SetProperty("Hide Player", "True");
                    cp.Doc.SetProperty("Auto Start", "False");
                    // 
                    result += cp.Addon.Execute(constants.addonGuidWebcast);
                }
                hint = 100;
                // 
                // Author Row
                // 
                string RowCopy = "";
                var datePublished = blogPost.datePublished;
                if (datePublished is null)
                    datePublished = blogPost.DateAdded;
                // 
                if (blogPost.AuthorMemberID == 0 & blogPost.CreatedBy > 0) {
                    blogPost.AuthorMemberID = blogPost.CreatedBy;
                    blogPost.save<BlogPostModel>(cp);
                }
                var author = DbModel.create<PersonModel>(cp, blogPost.AuthorMemberID);
                if (author is not null) {
                    RowCopy += "By " + author.name;
                    if (datePublished.HasValue && datePublished.Value != DateTime.MinValue) {
                        RowCopy += " | " + datePublished;
                    }
                }
                else if (datePublished.HasValue && datePublished.Value != DateTime.MinValue) {
                    RowCopy = Conversions.ToString((RowCopy + datePublished));
                }
                var visit = VisitModel.create(cp, cp.Visit.Id);
                if (blogPost.AllowComments & (visit is not null && cp.Visit.CookieSupport & !visit.Bot)) {
                    hint = 110;
                    // 
                    // Show comment count
                    var BlogCommentModelList = DbModel.createList<BlogCommentModel>(cp, "(Approved<>0)and(EntryID=" + blogPost.id + ")");
                    if (isArticleView) {
                        if (BlogCommentModelList.Count == 1) {
                            RowCopy += " | 1 Comment";
                        }
                        else if (BlogCommentModelList.Count > 1) {
                            RowCopy += " | " + BlogCommentModelList.Count + " Comments&nbsp;(" + BlogCommentModelList.Count + ")";
                        }
                    }
                    else {
                        qs = app.blogBaseLink;
                        qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameBlogEntryID, blogPost.id.ToString());
                        qs = cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogPostDetails.ToString());
                        if (BlogCommentModelList.Count == 0) {
                            RowCopy += " | " + "<a href=\"" + entryLink + "\">Comment</a>";
                        }
                        else {
                            RowCopy += " | " + "<a href=\"" + entryLink + "\">Comments</a>&nbsp;(" + BlogCommentModelList.Count + ")";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(RowCopy)) {
                    result += "<div class=\"aoBlogEntryByLine\">Posted " + RowCopy + "</div>";
                }
                hint = 120;
                // 
                // Tag List Row
                // 
                if (!string.IsNullOrEmpty(TagListRow)) {
                    result += TagListRow;
                }
                string toolLine = "";
                var CommentPtr = default(int);
                if (blogPost.AllowComments & cp.Visit.CookieSupport & (visit is not null && !visit.Bot)) {
                    hint = 130;
                    // 
                    // --
                    if (!isArticleView) {
                        hint = 140;
                        // 
                        // Show comment count
                        // 
                        var BlogCommentModelList = DbModel.createList<BlogCommentModel>(cp, "(Approved<>0)and(EntryID=" + blogPost.id + ")");
                        // 
                        qs = app.blogBaseLink;
                        qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameBlogEntryID, blogPost.id.ToString());
                        qs = cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogPostDetails.ToString());
                        string CommentLine = "";
                        if (BlogCommentModelList.Count == 0) {
                            CommentLine = CommentLine + "<a href=\"?" + qs + "\">Comment</a>";
                        }
                        else {
                            CommentLine = CommentLine + "<a href=\"?" + qs + "\">Comments</a>&nbsp;(" + BlogCommentModelList.Count + ")";
                        }

                        // get the unapproved comments
                        if (app.user.isBlogEditor(cp, app.blog)) {
                            hint = 150;
                            var BlogUnapprovedCommentModelList = DbModel.createList<BlogCommentModel>(cp, "(Approved=0)and(EntryID=" + blogPost.id + ")");
                            int unapprovedCommentCount = BlogUnapprovedCommentModelList.Count;
                            if (!string.IsNullOrEmpty(toolLine)) {
                                toolLine = toolLine + "&nbsp;|&nbsp;";
                            }
                            toolLine = toolLine + "Unapproved Comments (" + unapprovedCommentCount + ")";
                            qs = app.blogBaseLink;
                            qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameBlogEntryID, blogPost.id.ToString());
                            qs = cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogEntryEditor.ToString());
                            if (!string.IsNullOrEmpty(toolLine)) {
                                toolLine = toolLine + "&nbsp;|&nbsp;";
                            }
                            toolLine = toolLine + "<a href=\"?" + qs + "\">Edit</a>";
                        }
                    }
                    else {
                        hint = 160;
                        // 
                        // Show all comments
                        // 
                        string Criteria = "(EntryID=" + blogPost.id + ")";
                        if (!app.user.isBlogEditor(cp, app.blog)) {
                            // 
                            // non-owner - just approved comments plus your own comments
                            // 
                            Criteria += "and((Approved<>0)or(AuthorMemberID=" + cp.User.Id + "))";
                        }
                        var BlogCommentModelList = DbModel.createList<BlogCommentModel>(cp, Criteria, "DateAdded");
                        if (BlogCommentModelList.Count > 0) {
                            string Divider = "<div class=\"aoBlogCommentDivider\">&nbsp;</div>";
                            result += "<div class=\"aoBlogCommentHeader\">Comments</div>";
                            result += Constants.vbCrLf + Divider;
                            CommentPtr = 0;
                            foreach (var blogComment in DbModel.createList<BlogCommentModel>(cp, Criteria)) {

                                result += BlogCommentCellView.getBlogCommentCell(cp, app.blog, blogPost, blogComment, app.user, false);
                                result += Constants.vbCrLf + Divider;
                                CommentPtr += 1;
                            }
                        }
                    }
                }
                hint = 170;
                // 
                if (!string.IsNullOrEmpty(toolLine)) {
                    result += "<div class=\"aoBlogToolLink\">" + toolLine + "</div>";
                }
                result += Constants.vbCrLf + cp.Html.Hidden("CommentCnt" + blogPost.id, CommentPtr.ToString());
                // 
                Return_CommentCnt = CommentPtr;
                return result;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex, "hint " + hint);
                throw;
            }
        }
    }
}