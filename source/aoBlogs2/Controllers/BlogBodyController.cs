
using System;
using Contensive.Blog.Models;
using Contensive.BaseClasses;
using Contensive.Blog.Views;

namespace Contensive.Blog.Controllers {
    // 
    public class BlogBodyController {
        // 
        // ====================================================================================
        // 
        public static int ProcessForm(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request, ref bool RetryCommentPost) {
            int ProcessFormRet = default;
            // 
            try {
                if (!string.IsNullOrEmpty(request.ButtonValue)) {
                    switch (request.SourceFormID) {
                        case constants.FormBlogPostList: {
                                ProcessFormRet = constants.FormBlogPostList;
                                break;
                            }
                        case constants.FormBlogEntryEditor: {
                                ProcessFormRet = EditView.ProcessFormBlogEdit(cp, app, request);
                                break;
                            }
                        case constants.FormBlogPostDetails: {
                                ProcessFormRet = ArticleView.processArticleView(cp, app, request, ref RetryCommentPost);
                                break;
                            }
                        case constants.FormBlogArchiveDateList: {
                                ProcessFormRet = constants.FormBlogArchiveDateList;
                                break;
                            }
                        case constants.FormBlogSearch: {
                                ProcessFormRet = constants.FormBlogSearch;
                                break;
                            }
                        case constants.FormBlogArchivedBlogs: {
                                ProcessFormRet = constants.FormBlogArchivedBlogs;
                                break;
                            }
                    }
                }
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }

            return ProcessFormRet;
        }
    }
}