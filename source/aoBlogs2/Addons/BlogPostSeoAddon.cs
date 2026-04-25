
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;

namespace Contensive.Blog {
    public class BlogPostSeoAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogPostSeo;
        public const string guidAddon = constants.guidAddonBlogPostSeo;
        //
        public override object Execute(CPBaseClass cp) {
            try {
                if (!cp.User.IsAdmin) { return "<p>You are not authorized to access this feature.</p>"; }
                if (!cp.AdminUI.EndpointContainsPortal()) {
                    return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogList, "");
                }
                processForm(cp);
                return getForm(cp);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        internal static void processForm(CPBaseClass cp) {
            try {
                string button = cp.Doc.GetText(constants.rnButton);
                if (string.IsNullOrEmpty(button)) { return; }
                int blogId = cp.Doc.GetInteger(constants.rnBlogId);
                int postId = cp.Doc.GetInteger(constants.rnBlogPostId);
                //
                if (button == constants.buttonSave || button == constants.buttonOK) {
                    //
                    // -- save
                    var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                    if (post != null) {
                        post.metaTitle = cp.Doc.GetText("rnPostMetaTitle");
                        post.metaDescription = cp.Doc.GetText("rnPostMetaDescription");
                        post.metaKeywordList = cp.Doc.GetText("rnPostMetaKeywordList");
                        post.save(cp);
                    }
                }
                if (button == constants.buttonCancel || button == constants.buttonOK) {
                    //
                    // -- return to post list
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogPostList, $"&{constants.rnBlogId}={blogId}");
                    return;
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        internal static string getForm(CPBaseClass cp) {
            try {
                if (!cp.Response.isOpen) { return ""; }
                //
                int blogId = cp.Doc.GetInteger(constants.rnBlogId);
                int postId = cp.Doc.GetInteger(constants.rnBlogPostId);
                //
                var blog = DbBaseModel.create<BlogModel>(cp, blogId);
                if (blog == null) {
                    return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogList);
                }
                //
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (post == null) {
                    return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogPostList, $"&{constants.rnBlogId}={blogId}");
                }
                //
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderNameValue();
                //
                // -- form fields
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Meta Title";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostMetaTitle", 255, post.metaTitle ?? "", "form-control");
                layoutBuilder.rowHelp = "The meta title for this post's page.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Meta Description";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostMetaDescription", 255, post.metaDescription ?? "", "form-control");
                layoutBuilder.rowHelp = "The meta description for this post's page.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Meta Keywords";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostMetaKeywordList", 255, post.metaKeywordList ?? "", "form-control");
                layoutBuilder.rowHelp = "The meta keywords for this post's page.";
                //
                // -- layout settings
                layoutBuilder.title = "Post SEO";
                layoutBuilder.description = "";
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostSeo;
                //
                // -- feature subnav
                cp.Doc.AddRefreshQueryString(constants.rnBlogId, blogId);
                cp.Doc.AddRefreshQueryString(constants.rnBlogPostId, postId);
                layoutBuilder.portalSubNavTitle = $"{blog.name}, #{blog.id}";
                //
                // -- buttons
                layoutBuilder.addFormButton(constants.buttonOK);
                layoutBuilder.addFormButton(constants.buttonSave);
                layoutBuilder.addFormButton(constants.buttonCancel);
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogPostSeo);
                layoutBuilder.addFormHidden(constants.rnBlogPostId, postId);
                layoutBuilder.addFormHidden(constants.rnBlogId, blogId);
                //
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
