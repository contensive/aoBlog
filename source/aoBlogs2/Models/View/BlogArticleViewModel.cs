
using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Views;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;

namespace Contensive.Blog.Models.View {
    public class BlogArticleViewModel {
        //
        // -- back link
        public string backToRecentLink { get; set; }
        //
        // -- blog post cell (rendered sub-template)
        public string postCellHtml { get; set; }
        //
        // -- apply comment changes button (editor only, when comments exist)
        public bool hasApplyButton { get; set; }
        public string applyButtonHtml { get; set; }
        //
        // -- comment form section (nested view model, rendered inline in article layout)
        public CommentFormViewModel commentForm { get; set; }
        //
        // -- hidden fields
        public string hiddenFieldsHtml { get; set; }
        //
        // -- form key
        public string formKey { get; set; }
        //
        // -- article not found
        public bool articleNotFound { get; set; }
        public string articleNotFoundMessage { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Create the BlogArticleViewModel. Extracts rendering logic from ArticleView.getArticleView().
        /// The caller must wrap the rendered result in cp.Html.Form() and addEditWrapper().
        /// </summary>
        public static BlogArticleViewModel create(CPBaseClass cp, ApplicationEnvironmentModel app, bool retryCommentPost) {
            try {
                cp.Log.Debug("BlogArticleViewModel.create, entry");
                //
                var result = new BlogArticleViewModel();
                result.formKey = cp.Utils.CreateGuid();
                result.backToRecentLink = app.blogPageBaseLink;
                //
                if (app.blogPost is null) {
                    result.articleNotFound = true;
                    result.articleNotFoundMessage = "Sorry, the blog post you selected is not currently available";
                    return result;
                }
                //
                // -- count the viewing
                cp.Db.ExecuteNonQuery($"update {BlogEntryModel.tableMetadata.tableNameLower} set viewings={app.blogPost.viewings + 1}");
                //
                // -- entry edit link
                string entryEditLink = app.userIsEditing ? cp.Content.GetEditLink(BlogModel.tableMetadata.contentName, app.blogPost.id.ToString(), false, app.blogPost.name, true) : "";
                //
                // -- render blog post cell via sub-template
                List<BlogImageModel> blogImageList = BlogImageModel.getPostImageList(cp, app.blogPost);
                var postCellVm = BlogPostCellViewModel.create(cp, app, app.blogPost, blogImageList, true, false, entryEditLink);
                result.postCellHtml = cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogPostCell, constants.layoutNameBlogPostCell, constants.layoutPathFilenameBlogPostCell), postCellVm);
                //
                // -- viewing log
                var visit = DbBaseModel.create<VisitModel>(cp, cp.Visit.Id);
                if (app?.user != null && visit is not null) {
                    if (!visit.excludeFromAnalytics) {
                        int blogEntryId = app.blogPost is not null ? app.blogPost.id : 0;
                        var blogViewingLog = DbBaseModel.addDefault<BlogViewingLogModel>(cp);
                        if (blogViewingLog is not null) {
                            blogViewingLog.name = $"{cp.User.Name}, post {blogEntryId}, {Conversions.ToString(DateTime.Now)}";
                            blogViewingLog.BlogEntryID = blogEntryId;
                            blogViewingLog.MemberID = cp.User.Id;
                            blogViewingLog.VisitID = cp.Visit.Id;
                            blogViewingLog.save(cp);
                        }
                    }
                }
                //
                // -- apply comment changes button (editor only)
                if (app?.user != null && app.user.isBlogEditor(cp, app.blog) && postCellVm.commentCount > 0) {
                    result.hasApplyButton = true;
                    result.applyButtonHtml = cp.Html.Button(constants.rnButton,constants.FormButtonApplyCommentChanges);
                }
                //
                // -- comment form section (rendered sub-template)
                result.commentForm = CommentFormViewModel.create(cp, app, visit, retryCommentPost);
                //
                // -- hidden fields
                result.hiddenFieldsHtml = Constants.vbCrLf + cp.Html5.Hidden(constants.RequestNameSourceFormID, constants.FormBlogPostDetails)
                    + Constants.vbCrLf + cp.Html5.Hidden(constants.RequestNameBlogEntryID, app.blogPost.id)
                    + Constants.vbCrLf + cp.Html5.Hidden("EntryCnt", 1);
                //
                cp.Visit.SetProperty(constants.SNBlogCommentName, cp.Utils.GetRandomInteger().ToString());
                //
                // -- set metadata
                cp.Log.Debug("BlogArticleViewModel.create, call setEntryMetadata");
                MetadataController.setEntryMetadata(app, app.blog, app.blogPost, blogImageList);
                //
                cp.Log.Debug("BlogArticleViewModel.create, exit");
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "BlogArticleViewModel.create");
                throw;
            }
        }
    }
}
