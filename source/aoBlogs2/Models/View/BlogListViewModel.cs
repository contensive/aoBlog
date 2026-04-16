
using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Views;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;

namespace Contensive.Blog.Models.View {
    public class BlogListViewModel {
        //
        // -- blog header
        public bool hasCaption { get; set; }
        public string caption { get; set; }
        public bool hasDescription { get; set; }
        public string description { get; set; }
        public bool hasCategory { get; set; }
        public string categoryName { get; set; }
        //
        // -- posts
        public bool hasPosts { get; set; }
        public string postCellsHtml { get; set; }
        public string noneMessage { get; set; }
        //
        // -- pagination
        public string paginationHtml { get; set; }
        //
        // -- footer
        public string addLinkHtml { get; set; }
        public bool hasRssFeed { get; set; }
        public string rssFeedUrl { get; set; }
        public string rssFeedName { get; set; }
        public string rssIcon { get; set; }
        //
        // -- hidden fields
        public string sourceFormId { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Create the BlogListViewModel. Extracts rendering logic from ListView.getListView().
        /// </summary>
        public static BlogListViewModel create(CPBaseClass cp, ApplicationEnvironmentModel app, RequestModel request) {
            try {
                var result = new BlogListViewModel();
                var blog = app.blog;
                var user = app.user;
                var rssFeed = app.rssFeed;
                //
                string noneMsg = "There are no articles available";
                //
                // -- build criteria
                string criteria = $"(blogId={blog.id})";
                if (!cp.User.IsEditing()) {
                    criteria += $"and(COALESCE(datePublished,dateAdded)<{cp.Db.EncodeSQLDate(DateTime.Now)})";
                }
                var blogCategory = request.categoryId.Equals(0) ? null : DbBaseModel.create<BlogCategoriesModel>(cp, request.categoryId);
                if (blogCategory is not null) {
                    criteria += $"and(BlogCategoryID={request.categoryId})";
                    noneMsg = $"There are no articles available in the category {blogCategory.name}";
                    result.hasCategory = true;
                    result.categoryName = blogCategory.name;
                }
                int recordCount = DbBaseModel.getCount<BlogEntryModel>(cp, criteria);
                int pageNumber = request.page;
                if (pageNumber > recordCount) { pageNumber = recordCount; }
                if (pageNumber < 1) { pageNumber = 1; }
                int pageCount = (int)Math.Round(Math.Truncate(recordCount / (double)blog.postsToDisplay + 0.999d));
                string pageOneOfTenMsg = $"Page {pageNumber} of {pageCount}";
                //
                // -- caption
                if (!string.IsNullOrEmpty(blog.caption)) {
                    result.hasCaption = true;
                    result.caption = blog.caption + (pageNumber == 1 ? "" : $", {pageOneOfTenMsg}");
                }
                //
                // -- description
                if (!string.IsNullOrEmpty(blog.copy)) {
                    result.hasDescription = true;
                    result.description = blog.copy;
                }
                //
                // -- render post cells
                var postList = DbBaseModel.createList<BlogEntryModel>(cp, criteria, "COALESCE(datePublished,dateAdded) desc", blog.postsToDisplay, pageNumber);
                var isBlogCategoryBlockedDict = new Dictionary<int, bool>();
                int recordsOnThisPage = 0;
                if (postList.Count == 0) {
                    result.noneMessage = noneMsg;
                } else {
                    result.hasPosts = true;
                    string postCellsHtml = "";
                    foreach (BlogEntryModel post in postList) {
                        BlogEntryModel.verifyPost(cp, post);
                        if (recordsOnThisPage >= blog.postsToDisplay) { break; }
                        bool isBlocked = false;
                        if (isBlogCategoryBlockedDict.ContainsKey(post.blogCategoryId)) {
                            isBlocked = isBlogCategoryBlockedDict[post.blogCategoryId];
                        } else {
                            var blogEntryCategory = DbBaseModel.create<BlogCategoriesModel>(cp, post.blogCategoryId);
                            if (blogEntryCategory is not null) {
                                if (blogEntryCategory.UserBlocking) {
                                    isBlocked = !_GenericController.isGroupListMember(cp, GroupModel.GetBlockingGroups(cp, blogEntryCategory.id));
                                }
                                isBlogCategoryBlockedDict.Add(post.blogCategoryId, isBlocked);
                            }
                        }
                        if (!isBlocked) {
                            List<BlogImageModel> blogImageList = BlogImageModel.getPostImageList(cp, post);
                            var postCellVm = BlogPostCellViewModel.create(cp, app, post, blogImageList, false, true, "");
                            string blogArticleCell = cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogPostCell, constants.layoutNameBlogPostCell, constants.layoutPathFilenameBlogPostCell), postCellVm);
                            //
                            // -- if editing enabled, add the edit wrapper
                            blogArticleCell = _GenericController.addEditWrapper(cp, blogArticleCell, post.id, post.name, BlogEntryModel.tableMetadata.contentName);
                            postCellsHtml += blogArticleCell + "<hr>";
                        }
                        recordsOnThisPage++;
                    }
                    result.postCellsHtml = postCellsHtml;
                }
                //
                // -- pagination
                result.paginationHtml = PaginationController.getRecordPagination(cp, blog.postsToDisplay, pageNumber, recordsOnThisPage, recordCount);
                //
                // -- footer
                if (user.isBlogEditor(cp, blog)) {
                    result.addLinkHtml = cp.Content.GetAddLink(BlogEntryModel.tableMetadata.contentName, $"blogId={app.blog.id}", false, app.userIsEditing, false);
                }
                //
                // -- RSS feed link
                if (!string.IsNullOrEmpty(rssFeed.rssFilename)) {
                    result.hasRssFeed = true;
                    result.rssFeedUrl = cp.Http.CdnFilePathPrefix + rssFeed.rssFilename;
                    result.rssFeedName = rssFeed.name;
                    result.rssIcon = constants.iconRSS_16x16;
                }
                //
                result.sourceFormId = constants.FormBlogPostList.ToString();
                //
                // -- set metadata
                MetadataController.setBlogMetadata(app, blog, pageCount, pageNumber, pageOneOfTenMsg);
                //
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "BlogListViewModel.create");
                return new BlogListViewModel();
            }
        }
    }
}
