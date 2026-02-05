using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Contensive.Blog.Views {
    // 
    public class ListView {
        // 
        // ====================================================================================
        // 
        public static string getListView(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request) {
            var result = new StringBuilder();
            try {
                var blog = app.blog;
                var user = app.user;
                var rssFeed = app.rssFeed;

                string NoneMsg = "There are no articles available";
                //
                // -- if editing, you can see unpublished posts
                string criteria = $"(blogId={blog.id})";
                if (!cp.User.IsEditing()) {
                    criteria += $"and(COALESCE(datePublished,dateAdded)<{cp.Db.EncodeSQLDate(DateTime.Now)})";
                }
                var blogCategory = request.categoryId.Equals(0) ? null : Contensive.Models.Db.DbBaseModel.create<BlogCategoriesModel>(cp, request.categoryId);
                if (blogCategory is not null) {
                    criteria += "and(BlogCategoryID=" + request.categoryId + ")";
                    NoneMsg = "There are no articles available in the category " + blogCategory.name;
                }
                int recordCount = DbBaseModel.getCount<BlogEntryModel>(cp, criteria);
                int pageNumber = request.page;
                if (pageNumber > recordCount)
                    pageNumber = recordCount;
                if (pageNumber < 1)
                    pageNumber = 1;
                int pageCount = (int)Math.Round(Math.Truncate(recordCount / (double)blog.postsToDisplay + 0.999d));
                string pageOneOfTenMsg = "Page " + pageNumber + " of " + pageCount + "";
                // 
                if (!string.IsNullOrEmpty(blog.caption)) {
                    result.Append(Constants.vbCrLf + "<h1 Class=\"aoBlogCaption\">" + blog.caption + (pageNumber == 1 ? "" : ", " + pageOneOfTenMsg) + "</h1>");
                }
                if (!string.IsNullOrEmpty(blog.copy)) {
                    result.Append(Constants.vbCrLf + "<div Class=\"aoBlogDescription\">" + blog.copy + "</div>");
                }

                if (blogCategory is not null) {
                    result.Append("<div Class=\"aoBlogCategoryCaption\">Category " + blogCategory.name + "</div>");
                }
                // 
                // Display the most recent entries
                var postList = DbBaseModel.createList<BlogEntryModel>(cp, criteria, "COALESCE(datePublished,dateAdded) desc", blog.postsToDisplay, pageNumber);
                var isBlogCategoryBlockedDict = new Dictionary<int, bool>();
                int recordsOnThisPage = 0;
                if (postList.Count.Equals(0)) {
                    // 
                    result.Append("<div Class=\"aoBlogProblem\">" + NoneMsg + "</div>");
                }
                else {
                    var blogCategoryList = DbBaseModel.createList<BlogCategoriesModel>(cp, "");
                    var Return_CommentCnt = default(int);
                    foreach (BlogEntryModel post in postList) {
                        BlogEntryModel.verifyPost(cp, post);
                        if (recordsOnThisPage >= blog.postsToDisplay)
                            break;
                        bool IsBlocked = false;
                        if (isBlogCategoryBlockedDict.ContainsKey(post.blogCategoryId)) {
                            IsBlocked = isBlogCategoryBlockedDict[post.blogCategoryId];
                        }
                        else {
                            var blogEntryCategory = DbBaseModel.create<BlogCategoriesModel>(cp, post.blogCategoryId);
                            if (blogEntryCategory is not null) {
                                if (blogEntryCategory.UserBlocking)
                                    IsBlocked = !_GenericController.isGroupListMember(cp, Models.GroupModel.GetBlockingGroups(cp, blogEntryCategory.id));
                                isBlogCategoryBlockedDict.Add(post.blogCategoryId, IsBlocked);
                            }
                        }
                        if (!IsBlocked) {
                            List<BlogImageModel> blogImageList = BlogImageModel.getPostImageList(cp, post);
                            string blogArticleCell = BlogEntryCellView.getBlogPostCell(cp, app, post, blogImageList, false, true, Return_CommentCnt, "");
                            // 
                            // -- if editing enabled, add the link and wrapperwrapper
                            blogArticleCell = _GenericController.addEditWrapper(cp, blogArticleCell, post.id, post.name, BlogEntryModel.tableMetadata.contentName);

                            result.Append(blogArticleCell);
                            result.Append("<hr>");
                        }
                        recordsOnThisPage += 1;
                    }
                }
                // 
                // -- pagination
                result.Append(PaginationController.getRecordPagination(cp, blog.postsToDisplay, pageNumber, recordsOnThisPage, recordCount));
                // 
                // Footer
                result.Append("<div>&nbsp;</div>");
                if (user.isBlogEditor(cp, blog)) {
                    result.Append(cp.Content.GetAddLink(BlogEntryModel.tableMetadata.contentName, $"blogId={app.blog.id}", false, app.userIsEditing, false));
                }
                // 
                // Link to RSS Feed
                // 
                string FeedFooter = "";
                if (!string.IsNullOrEmpty(rssFeed.rssFilename)) {
                    FeedFooter = "RSS: <a href=\"" + cp.Http.CdnFilePathPrefix + rssFeed.rssFilename + "\">" + rssFeed.name + "</a>&nbsp;" + FeedFooter + constants.iconRSS_16x16 + "";
                    result.Append("<div class=\"aoBlogFooterLink\">" + FeedFooter + "</div>");
                }
                result.Append(cp.Html.Hidden(constants.RequestNameSourceFormID, constants.FormBlogPostList.ToString()));
                // 
                // -- meta data
                MetadataController.setBlogMetadata(app, blog, pageCount, pageNumber, pageOneOfTenMsg);
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            // 
            return result.ToString();
        }
    }
}