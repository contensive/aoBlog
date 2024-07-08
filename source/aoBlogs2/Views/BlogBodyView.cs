
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using System;
using System.Text;

namespace Contensive.Blog.Views {
    // 
    public class BlogBodyView {
        // 
        // ========================================================================
        /// <summary>
        /// Inner Blog - the list of posts without sidebar. Blog object must be valid
        /// </summary>
        /// <returns></returns>
        public static string getBlogBody(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel legacyRequest, BlogBodyRequestModel blogBodyRequest) {
            string result;
            try {
                // 
                // Process Input
                bool RetryCommentPost = false;
                int dstFormId = legacyRequest.FormID;
                if (!string.IsNullOrEmpty(legacyRequest.ButtonValue)) {
                    // 
                    // Process the source form into form if there was a button - else keep formid
                    dstFormId = Controllers.BlogBodyController.ProcessForm(cp, app, legacyRequest, ref RetryCommentPost);
                }
                // 
                // -- Get Next Form
                result = GetForm(cp, app, legacyRequest, dstFormId, RetryCommentPost);
                // 
                return result;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
        // 
        // ====================================================================================
        // 
        private static string GetForm(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request, int dstFormID, bool RetryCommentPost) {
            var result = new StringBuilder();
            try {
                switch (dstFormID) {
                    case constants.FormBlogPostDetails: {
                            result.Append(ArticleView.getArticleView(cp, app, RetryCommentPost));
                            break;
                        }
                    case constants.FormBlogArchiveDateList: {
                            result.Append(ArchiveView.getFormBlogArchiveDateList(cp, app, request));
                            break;
                        }
                    case constants.FormBlogArchivedBlogs: {
                            result.Append(ArchiveView.getFormBlogArchivedBlogs(cp, app, request));
                            break;
                        }
                    case constants.FormBlogEntryEditor: {
                            result.Append(EditView.getFormBlogEdit(cp, app, request));
                            break;
                        }
                    case constants.FormBlogSearch: {
                            result.Append(SearchView.GetFormBlogSearch(cp, app, request));
                            break;
                        }

                    default: {
                            if (app.blogPost is not null) {
                                // 
                                // Go to details page
                                // 
                                result.Append(ArticleView.getArticleView(cp, app, RetryCommentPost));
                            }
                            else {
                                // 
                                // list all the entries
                                // 
                                result.Append(ListView.getListView(cp, app, request));
                            }

                            break;
                        }
                }
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result.ToString();
        }
        // 
        // 
    }
}