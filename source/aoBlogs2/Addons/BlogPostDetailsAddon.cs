
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
                    // -- save the post copy
                    var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                    if (post != null) {
                        post.name = cp.Doc.GetText("rnPostTitle");
                        post.copy = cp.Doc.GetText("rnPostCopy");
                        post.save(cp);
                    }
                }
                if (button == constants.buttonEmailVersion) {
                    //
                    // -- create a group email from this blog post and redirect to its admin edit form
                    var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                    if (post != null) {
                        var blog = DbBaseModel.create<BlogModel>(cp, blogId);
                        if (blog != null) {
                            //
                            // -- resolve from-address: post author > blog owner > current user > site default
                            string fromAddress = "";
                            if (post.authorMemberId > 0) {
                                var postAuthor = DbBaseModel.create<Models.PersonModel>(cp, post.authorMemberId);
                                if (postAuthor != null && !string.IsNullOrEmpty(postAuthor.email)) {
                                    fromAddress = postAuthor.email;
                                }
                            }
                            if (string.IsNullOrEmpty(fromAddress) && blog.ownerMemberId > 0) {
                                var blogOwner = DbBaseModel.create<Models.PersonModel>(cp, blog.ownerMemberId);
                                if (blogOwner != null && !string.IsNullOrEmpty(blogOwner.email)) {
                                    fromAddress = blogOwner.email;
                                }
                            }
                            if (string.IsNullOrEmpty(fromAddress)) {
                                fromAddress = cp.User.Email;
                            }
                            if (string.IsNullOrEmpty(fromAddress)) {
                                fromAddress = cp.Site.GetText("EmailFromAddress", $"info@{cp.Site.DomainPrimary}");
                            }
                            //
                            // -- create group email record and populate with blog post content
                            int groupEmailId = cp.Content.AddRecord(constants.cnGroupEmail);
                            if (groupEmailId > 0) {
                                using (var cs = cp.CSNew()) {
                                    if (cs.OpenRecord(constants.cnGroupEmail, groupEmailId)) {
                                        cs.SetField("name", post.name);
                                        cs.SetField("subject", post.name);
                                        cs.SetField("fromAddress", fromAddress);
                                        cs.SetField("copyFilename", post.copy);
                                        cs.Save();
                                    }
                                }
                                //
                                // -- redirect to admin edit form for the new Group Email record
                                string adminEditUrl = $"{cp.Site.GetText("adminUrl", "/admin")}?cid={cp.Content.GetID(constants.cnGroupEmail)}&id={groupEmailId}&af=4";
                                cp.Response.Redirect(adminEditUrl);
                                return;
                            }
                        }
                    }
                }
                if (button == constants.buttonCancel || button == constants.buttonOK) {
                    //
                    // -- return to post list
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList, $"&{constants.rnBlogId}={blogId}");
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
                    return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogList);
                }
                //
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (post == null) {
                    return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList, $"&{constants.rnBlogId}={blogId}");
                }
                //
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilder();
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostDetails;
                //
                // -- title field and WYSIWYG editor for blog copy
                string titleInput = cp.Html5.InputText("rnPostTitle", 255, post.name ?? "", "form-control");
                string titleRow = $"<div class=\"mb-3\"><label class=\"form-label\"><b>Title</b></label>{titleInput}</div>";
                layoutBuilder.body = titleRow + cp.Html.InputWysiwyg("rnPostCopy", post.copy ?? "", CPHtmlBaseClass.EditorUserScope.Administrator);
                //
                // -- layout settings
                layoutBuilder.title = $"Edit Post: {post.name}";
                layoutBuilder.portalSubNavTitleList.Add($"{blog.name}, #{blog.id}");
                layoutBuilder.includeForm = true;
                //
                // -- buttons
                layoutBuilder.addFormButton(constants.buttonOK);
                layoutBuilder.addFormButton(constants.buttonSave);
                layoutBuilder.addFormButton(constants.buttonCancel);
                layoutBuilder.addFormButton(constants.buttonEmailVersion);
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
