
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;

namespace Contensive.Blog {
    public class BlogPostDetailsAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogPostDetails;
        public const string guidAddon = constants.guidAddonBlogPostDetails;
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
                    // -- save the post copy
                    var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                    if (post != null) {
                        post.name = cp.Doc.GetText("rnPostTitle");
                        post.copy = cp.Doc.GetText("rnPostCopy");
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
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilder();
                //
                // -- title field and WYSIWYG editor for blog copy
                string titleInput = cp.Html5.InputText("rnPostTitle", 255, post.name ?? "", "form-control");
                string titleRow = $"<div class=\"mb-3\"><label class=\"form-label\"><b>Title</b></label>{titleInput}</div>";
                layoutBuilder.body = titleRow + cp.Html.InputWysiwyg("rnPostCopy", post.copy ?? "", CPHtmlBaseClass.EditorUserScope.Administrator);
                //
                // -- layout settings
                layoutBuilder.title = $"Edit Post: {post.name}";
                layoutBuilder.portalSubNavTitle = $"{blog.name}, #{blog.id}";
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostDetails;
                layoutBuilder.includeForm = true;
                //
                // -- buttons
                layoutBuilder.addFormButton(constants.buttonOK);
                layoutBuilder.addFormButton(constants.buttonSave);
                layoutBuilder.addFormButton(constants.buttonCancel);
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogPostDetails);
                layoutBuilder.addFormHidden(constants.rnBlogPostId, postId);
                layoutBuilder.addFormHidden(constants.rnBlogId, blogId);
                //
                // -- feature subnav
                cp.Doc.AddRefreshQueryString(constants.rnBlogId, blogId);
                cp.Doc.AddRefreshQueryString(constants.rnBlogPostId, postId);
                //
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
