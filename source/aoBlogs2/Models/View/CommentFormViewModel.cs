
using Contensive.BaseClasses;
using Contensive.Models.Db;
using System;

namespace Contensive.Blog.Models.View {
    public class CommentFormViewModel {
        //
        // -- master toggle
        public bool showCommentForm { get; set; }
        //
        // -- error display
        public bool hasUserError { get; set; }
        public string userErrorMessage { get; set; }
        //
        // -- login required (not authenticated, anonymous not allowed)
        public bool showLoginRequired { get; set; }
        //
        // -- auth mode panels (mutually exclusive)
        public bool showPasswordEmail { get; set; }
        public bool showJoin { get; set; }
        public bool showLogin { get; set; }
        //
        // -- auth links
        public string loginLink { get; set; }
        public string joinLink { get; set; }
        public string passwordEmailLink { get; set; }
        //
        // -- conditional link visibility within auth panels
        public bool allowMemberJoin { get; set; }
        public bool allowPasswordEmail { get; set; }
        //
        // -- post form (authenticated or anonymous allowed)
        public bool showPostForm { get; set; }
        public string titleInputValue { get; set; }
        public string commentTextAreaValue { get; set; }
        //
        // -- recaptcha
        public bool hasRecaptcha { get; set; }
        public string recaptchaHtml { get; set; }
        //
        // -- buttons
        public string postButtonValue { get; set; }
        public string cancelButtonValue { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Build the comment form view model from application state.
        /// </summary>
        public static CommentFormViewModel create(CPBaseClass cp, ApplicationEnvironmentModel app, VisitModel visit, bool retryCommentPost) {
            var result = new CommentFormViewModel();
            try {
                if (app?.blogPost == null || !app.blogPost.allowComments || cp?.Visit == null || !cp.Visit.CookieSupport || (visit != null && visit.bot)) {
                    return result;
                }
                result.showCommentForm = true;
                //
                // -- user error
                if (cp?.UserError != null && !cp.UserError.OK()) {
                    result.hasUserError = true;
                    result.userErrorMessage = cp.UserError.OK().ToString();
                }
                //
                if (!app.blog.allowAnonymous && !cp.User.IsAuthenticated) {
                    //
                    // -- not authenticated, show login/join options
                    result.showLoginRequired = true;
                    result.allowPasswordEmail = cp.Site.GetBoolean("AllowPasswordEmail", false);
                    result.allowMemberJoin = cp.Site.GetBoolean("AllowMemberJoin", false);
                    int auth = cp.Doc.GetInteger("auth");
                    if (auth == 1 && !result.allowPasswordEmail) { auth = 3; }
                    else if (auth == 2 && !result.allowMemberJoin) { auth = 3; }
                    //
                    cp.Doc.AddRefreshQueryString(constants.rnFormID, constants.FormBlogPostDetails.ToString());
                    cp.Doc.AddRefreshQueryString(constants.RequestNameBlogEntryID, app.blogPost.id.ToString());
                    cp.Doc.AddRefreshQueryString("auth", "0");
                    string qs = cp.Doc.RefreshQueryString;
                    //
                    result.loginLink = $"?{cp.Utils.ModifyQueryString(qs, "auth", "0")}";
                    result.joinLink = $"?{cp.Utils.ModifyQueryString(qs, "auth", "2")}";
                    result.passwordEmailLink = $"?{cp.Utils.ModifyQueryString(qs, "auth", "1")}";
                    //
                    switch (auth) {
                        case 1:
                            result.showPasswordEmail = true;
                            break;
                        case 2:
                            result.showJoin = true;
                            break;
                        default:
                            result.showLogin = true;
                            break;
                    }
                } else {
                    //
                    // -- authenticated or anonymous allowed, show comment form
                    result.showPostForm = true;
                    result.titleInputValue = cp.Doc.GetText(constants.RequestNameCommentTitle);
                    result.commentTextAreaValue = cp.Doc.GetText(constants.RequestNameCommentCopy);
                    //
                    if (app.blog.recaptcha) {
                        result.hasRecaptcha = true;
                        result.recaptchaHtml = cp.Addon.Execute(constants.reCaptchaDisplayGuid);
                    }
                    //
                    result.postButtonValue = constants.FormButtonPostComment;
                    result.cancelButtonValue = constants.FormButtonCancel;
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "CommentFormViewModel.create");
                throw;
            }
            return result;
        }
    }
}
