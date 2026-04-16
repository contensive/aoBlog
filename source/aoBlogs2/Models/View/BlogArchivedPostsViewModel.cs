
using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Views;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Contensive.Blog.Models.View {
    public class BlogArchivedPostsViewModel {
        //
        // -- title
        public string title { get; set; }
        //
        // -- posts
        public bool hasPosts { get; set; }
        public string postCellsHtml { get; set; }
        public string noPostsMessage { get; set; }
        //
        // -- back link
        public string backToRecentLink { get; set; }
        //
        // -- hidden fields
        public string hiddenFieldsHtml { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Create the BlogArchivedPostsViewModel. Extracts rendering logic from ArchiveView.getFormBlogArchivedBlogs().
        /// The caller must wrap the rendered result in cp.Html.Form().
        /// </summary>
        public static BlogArchivedPostsViewModel create(CPBaseClass cp, ApplicationEnvironmentModel app, RequestModel request) {
            try {
                var result = new BlogArchivedPostsViewModel();
                var blog = app.blog;
                result.backToRecentLink = app.blogPageBaseLink;
                //
                // -- metadata
                cp.Doc.AddMetaDescription($"Archives {DateTimeFormatInfo.CurrentInfo.GetMonthName(request.ArchiveMonth)} {request.ArchiveYear}, {MetadataController.getBlogMetaDescription(app, blog)}");
                string title = $"Archives {DateTimeFormatInfo.CurrentInfo.GetMonthName(request.ArchiveMonth)} {request.ArchiveYear}, {MetadataController.getBlogMetaTitle(app, blog)} ";
                MetadataController.addTitle(cp, title);
                result.title = title;
                //
                var postList = DbBaseModel.createList<BlogEntryModel>(cp, $"(Month(COALESCE(datePublished, dateAdded)) = {request.ArchiveMonth})And(year(COALESCE(datePublished, dateAdded))={request.ArchiveYear})And(BlogID={blog.id})", "COALESCE(datePublished, dateAdded) Desc");
                if (postList.Count == 0) {
                    result.noPostsMessage = $"There are no blog archives For {request.ArchiveMonth}/{request.ArchiveYear}";
                    //
                    // -- remove stale link alias
                    string linkAliasQs = $"FormID=600&ArchiveMonth={request.ArchiveMonth}&ArchiveYear={request.ArchiveYear}";
                    DbBaseModel.deleteRows<LinkAliasModel>(cp, $"(pageid={cp.Doc.PageId}) and (queryStringSuffix={cp.Db.EncodeSQLText(linkAliasQs)})");
                } else {
                    result.hasPosts = true;
                    string postCellsHtml = "";
                    foreach (var blogPost in postList) {
                        if (blogPost.authorMemberId == 0) {
                            blogPost.authorMemberId = cp.Utils.EncodeInteger(blogPost.createdBy);
                        }
                        string entryEditLink = app.userIsEditing ? cp.Content.GetEditLink(blogPost.name, blogPost.id.ToString(), true, blogPost.name, true) : "";
                        List<BlogImageModel> blogImageList = BlogImageModel.getPostImageList(cp, blogPost);
                        var postCellVm = BlogPostCellViewModel.create(cp, app, blogPost, blogImageList, false, false, entryEditLink);
                        postCellsHtml += cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogPostCell, constants.layoutNameBlogPostCell, constants.layoutPathFilenameBlogPostCell), postCellVm);
                        postCellsHtml += "<hr>";
                    }
                    result.postCellsHtml = postCellsHtml;
                }
                //
                result.hiddenFieldsHtml = cp.Html.Hidden(constants.RequestNameSourceFormID, constants.FormBlogArchivedBlogs.ToString());
                //
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "BlogArchivedPostsViewModel.create");
                return new BlogArchivedPostsViewModel();
            }
        }
    }
}
