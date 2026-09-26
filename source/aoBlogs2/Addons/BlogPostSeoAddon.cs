
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;
using System.Data;

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
                    cp.Log.Warn($"BlogPostSeoAddon, endpoint does not contain portal, redirecting to BlogList");
                    return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogList, "");
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
                    cp.Log.Warn($"BlogPostSeoAddon, {button} button clicked, blogId [{blogId}], redirecting to BlogPostList");
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList, "");
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
                // -- load blog by id including inactive records (admin context)
                string blogName = "";
                using (DataTable dtBlog = cp.Db.ExecuteQuery($"select id,name from ccBlogs where id={blogId}")) {
                    if (dtBlog?.Rows == null || dtBlog.Rows.Count == 0) {
                        cp.Log.Warn($"BlogPostSeoAddon, blog not found, blogId [{blogId}], redirecting to BlogList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogList);
                    }
                    blogName = cp.Utils.EncodeText(dtBlog.Rows[0]["name"]);
                }
                //
                // -- load post by id including inactive records (admin context)
                string postName = "";
                string postMetaTitle = "";
                string postMetaDescription = "";
                string postMetaKeywordList = "";
                using (DataTable dtPost = cp.Db.ExecuteQuery($"select id,name,metaTitle,metaDescription,metaKeywordList from ccBlogCopy where id={postId}")) {
                    if (dtPost?.Rows == null || dtPost.Rows.Count == 0) {
                        cp.Log.Warn($"BlogPostSeoAddon, post not found, blogId [{blogId}], postId [{postId}], redirecting to BlogPostList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList);
                    }
                    postName = cp.Utils.EncodeText(dtPost.Rows[0]["name"]);
                    postMetaTitle = cp.Utils.EncodeText(dtPost.Rows[0]["metaTitle"]);
                    postMetaDescription = cp.Utils.EncodeText(dtPost.Rows[0]["metaDescription"]);
                    postMetaKeywordList = cp.Utils.EncodeText(dtPost.Rows[0]["metaKeywordList"]);
                }
                //
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderNameValue();
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostSeo;
                //
                // -- form fields
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Meta Title";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostMetaTitle", 255, postMetaTitle, "form-control");
                layoutBuilder.rowHelp = "The meta title for this post's page.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Meta Description";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostMetaDescription", 255, postMetaDescription, "form-control");
                layoutBuilder.rowHelp = "The meta description for this post's page.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Meta Keywords";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostMetaKeywordList", 255, postMetaKeywordList, "form-control");
                layoutBuilder.rowHelp = "The meta keywords for this post's page.";
                //
                // -- layout settings
                layoutBuilder.title = "Post SEO";
                layoutBuilder.description = "";
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                //
                // -- feature subnav
                cp.Doc.AddRefreshQueryString(constants.rnBlogId, blogId);
                cp.Doc.AddRefreshQueryString(constants.rnBlogPostId, postId);
                layoutBuilder.portalSubNavTitleList.Add($"{blogName}, #{blogId}");
                layoutBuilder.portalSubNavTitleList.Add($"{postName}");
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
