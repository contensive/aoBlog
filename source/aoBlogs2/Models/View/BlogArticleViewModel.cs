
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
        // -- comment form section (built in C#, complex auth/form logic)
        public string commentFormHtml { get; set; }
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
                    result.applyButtonHtml = cp.Html.Button(constants.FormButtonApplyCommentChanges);
                }
                //
                // -- comment form section
                result.commentFormHtml = buildCommentFormHtml(cp, app, visit, retryCommentPost);
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
        //
        //====================================================================================================
        /// <summary>
        /// Build the comment form HTML. This includes auth states, form fields, recaptcha, and buttons.
        /// Too complex for pure mustache — built in C# and injected via triple-brace.
        /// </summary>
        private static string buildCommentFormHtml(CPBaseClass cp, ApplicationEnvironmentModel app, VisitModel visit, bool retryCommentPost) {
            if (app?.blogPost == null || !app.blogPost.allowComments || cp?.Visit == null || !cp.Visit.CookieSupport || (visit != null && visit.bot)) {
                return "";
            }
            //
            string result = "<div class=\"aoBlogCommentHeader\">Post a Comment</div>";
            //
            if (cp?.UserError != null && !cp.UserError.OK()) {
                result += $"<div class=\"aoBlogCommentError\">{cp.UserError.OK()}</div>";
            }
            //
            if (!app.blog.allowAnonymous && !cp.User.IsAuthenticated) {
                //
                // -- not authenticated, show login/join options
                bool allowPasswordEmail = cp.Site.GetBoolean("AllowPasswordEmail", false);
                bool allowMemberJoin = cp.Site.GetBoolean("AllowMemberJoin", false);
                int auth = cp.Doc.GetInteger("auth");
                if (auth == 1 && !allowPasswordEmail) { auth = 3; }
                else if (auth == 2 && !allowMemberJoin) { auth = 3; }
                //
                cp.Doc.AddRefreshQueryString(constants.rnFormID, constants.FormBlogPostDetails.ToString());
                cp.Doc.AddRefreshQueryString(constants.RequestNameBlogEntryID, app.blogPost.id.ToString());
                cp.Doc.AddRefreshQueryString("auth", "0");
                string qs = cp.Doc.RefreshQueryString;
                //
                switch (auth) {
                    case 1: {
                            string copy = "To retrieve your username and password, submit your email. ";
                            qs = cp.Utils.ModifyQueryString(qs, "auth", "0");
                            copy += $" <a href=\"?{qs}\"> Login?</a>";
                            if (allowMemberJoin) {
                                qs = cp.Utils.ModifyQueryString(qs, "auth", "2");
                                copy += $" <a href=\"?{qs}\"> Join?</a>";
                            }
                            result += $"<div class=\"aoBlogLoginBox\">{Constants.vbCrLf}{Constants.vbTab}<div class=\"aoBlogCommentCopy\">{copy}</div>{Constants.vbCrLf}{Constants.vbTab}<div class=\"aoBlogCommentCopy\">send password form removed</div></div>";
                            break;
                        }
                    case 2: {
                            string copy = "To post a comment to this blog, complete this form. ";
                            qs = cp.Utils.ModifyQueryString(qs, "auth", "0");
                            copy += $" <a href=\"?{qs}\"> Login?</a>";
                            if (allowPasswordEmail) {
                                qs = cp.Utils.ModifyQueryString(qs, "auth", "1");
                                copy += $" <a href=\"?{qs}\"> Forget your username or password?</a>";
                            }
                            result += $"<div class=\"aoBlogLoginBox\"><div class=\"aoBlogCommentCopy\">{copy}</div><div class=\"aoBlogCommentCopy\">Send join form removed</div></div>";
                            break;
                        }
                    default: {
                            string copy = "To post a comment to this Blog, please login.";
                            if (allowMemberJoin) {
                                qs = cp.Utils.ModifyQueryString(qs, "auth", "2");
                                copy += $"<div class=\"aoBlogRegisterLink\"><a href=\"?{qs}\">Need to Register?</a></div>";
                            }
                            result += $"<div class=\"aoBlogCommentCopy\">{copy}</div></div>";
                            break;
                        }
                }
            } else {
                //
                // -- authenticated or anonymous allowed, show comment form
                result += "<div>&nbsp;</div>";
                result += "<div class=\"aoBlogCommentCopy\">Title</div>";
                result += $"<div class=\"aoBlogCommentCopy\">{_GenericController.getField(cp, constants.RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(constants.RequestNameCommentTitle.ToString()))}</div>";
                result += "<div>&nbsp;</div>";
                result += "<div class=\"aoBlogCommentCopy\">Comment</div>";
                result += $"<div class=\"aoBlogCommentCopy\">{cp.Html5.InputTextArea(constants.RequestNameCommentCopy, 500, cp.Doc.GetText(constants.RequestNameCommentCopy))}</div>";
                //
                if (app.blog.recaptcha) {
                    result += "<div class=\"aoBlogCommentCopy\">Verify Text</div>";
                    result += $"<div class=\"aoBlogCommentCopy\">{cp.Addon.Execute(constants.reCaptchaDisplayGuid)}</div>";
                }
                //
                result += $"<div class=\"aoBlogCommentCopy\">{cp.Html.Button(constants.rnButton, constants.FormButtonPostComment)}&nbsp;{cp.Html.Button(constants.rnButton, constants.FormButtonCancel)}</div>";
            }
            return result;
        }
    }
}
