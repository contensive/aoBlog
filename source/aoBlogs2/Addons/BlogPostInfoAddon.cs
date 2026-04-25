
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;

namespace Contensive.Blog {
    public class BlogPostInfoAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogPostInfo;
        public const string guidAddon = constants.guidAddonBlogPostInfo;
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
                    using (var cs = cp.CSNew()) {
                        if (postId == 0) {
                            cs.Insert(constants.cnBlogEntries);
                            if (cs.OK()) {
                                postId = cs.GetInteger("id");
                                cs.SetField("blogId", blogId.ToString());
                            }
                        } else {
                            cs.Open(constants.cnBlogEntries, $"id={postId}");
                        }
                        if (cs.OK()) {
                            cs.SetFormInput("active", "rnPostActive");
                            cs.SetFormInput("allowComments", "rnPostAllowComments");
                            cs.SetFormInput("tagList", "rnPostTagList");
                        }
                        cs.Close();
                    }
                }
                if (button == constants.buttonDelete) {
                    //
                    // -- delete
                    if (postId > 0) {
                        cp.Content.Delete(constants.cnBlogEntries, $"id={postId}");
                    }
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogPostList, $"&{constants.rnBlogId}={blogId}");
                    return;
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
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderNameValue();
                //
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                bool isNew = (post == null);
                //
                layoutBuilder.title = isNew ? "Add Post" : "Post Info";
                layoutBuilder.description = "";
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostInfo;
                //
                // -- form fields
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Active";
                layoutBuilder.rowValue = cp.Html5.CheckBox("rnPostActive", post?.active ?? true, "form-check-input");
                layoutBuilder.rowHelp = "When unchecked, this post will not be displayed.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Date Added";
                layoutBuilder.rowValue = post?.dateAdded == null ? "" : ((DateTime)post.dateAdded).ToShortDateString();
                layoutBuilder.rowHelp = "The date this post was created.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Views";
                layoutBuilder.rowValue = (post?.viewings ?? 0).ToString();
                layoutBuilder.rowHelp = "The number of times this post has been viewed.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Allow Comments";
                layoutBuilder.rowValue = cp.Html5.CheckBox("rnPostAllowComments", post?.allowComments ?? false, "form-check-input");
                layoutBuilder.rowHelp = "When checked, comments can be posted on this article.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Tags";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostTagList", 255, post?.tagList ?? "", "form-control");
                layoutBuilder.rowHelp = "Comma-delimited list of tags for this post.";
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
                if (!isNew) {
                    layoutBuilder.addFormButton(constants.buttonDelete);
                }
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogPostInfo);
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
