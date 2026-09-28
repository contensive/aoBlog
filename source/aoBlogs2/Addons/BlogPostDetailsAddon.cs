
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;
using System.Data;

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
                    cp.Log.Warn($"BlogPostDetailsAddon, endpoint does not contain portal, redirecting to BlogList");
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
                int postId = cp.Doc.GetInteger("id");
                if (postId == 0) { postId = cp.Doc.GetInteger(constants.rnBlogPostId); }
                int blogId = cp.Doc.GetInteger(constants.rnBlogId);
                if (blogId == 0 && postId > 0) {
                    var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                    if (post != null) { blogId = post.blogId; }
                }
                //
                if (button == constants.buttonSave || button == constants.buttonOK) {
                    //
                    // -- save the post copy (cs.Open includes inactive records)
                    using (var cs = cp.CSNew()) {
                        cs.Open(constants.cnBlogEntries, $"id={postId}");
                        if (cs.OK()) {
                            cs.SetFormInput("name", "rnPostTitle");
                            cs.SetFormInput("copy", "rnPostCopy");
                        }
                        cs.Close();
                    }
                }
                if (button == constants.buttonEmailVersion) {
                    //
                    // -- open or create a group email from this blog post and redirect to its admin edit form
                    // -- load post including inactive records (admin context)
                    string postName = "";
                    string postCopy = "";
                    string postGuid = "";
                    int authorMemberId = 0;
                    int ownerMemberId = 0;
                    bool postFound = false;
                    using (DataTable dtPost = cp.Db.ExecuteQuery($"select name,copy,authorMemberId,ccguid from ccBlogCopy where id={postId}")) {
                        if (dtPost?.Rows != null && dtPost.Rows.Count > 0) {
                            postFound = true;
                            postName = cp.Utils.EncodeText(dtPost.Rows[0]["name"]);
                            postCopy = cp.Utils.EncodeText(dtPost.Rows[0]["copy"]);
                            authorMemberId = cp.Utils.EncodeInteger(dtPost.Rows[0]["authorMemberId"]);
                            postGuid = cp.Utils.EncodeText(dtPost.Rows[0]["ccguid"]);
                        }
                    }
                    if (postFound) {
                        //
                        // -- check if a Group Email already exists with this blog post's guid
                        int groupEmailId = 0;
                        if (!string.IsNullOrEmpty(postGuid)) {
                            using (var csEmail = cp.CSNew()) {
                                if (csEmail.Open(constants.cnGroupEmail, $"ccguid={cp.Db.EncodeSQLText(postGuid)}")) {
                                    groupEmailId = csEmail.GetInteger("id");
                                }
                            }
                        }
                        if (groupEmailId == 0) {
                            //
                            // -- no existing email found, create a new one
                            // -- load blog including inactive records (admin context)
                            using (DataTable dtBlog = cp.Db.ExecuteQuery($"select ownerMemberId from ccBlogs where id={blogId}")) {
                                if (dtBlog?.Rows != null && dtBlog.Rows.Count > 0) {
                                    ownerMemberId = cp.Utils.EncodeInteger(dtBlog.Rows[0]["ownerMemberId"]);
                                }
                            }
                            //
                            // -- resolve from-address: post author > blog owner > current user > site default
                            string fromAddress = "";
                            if (authorMemberId > 0) {
                                using (DataTable dtAuthor = cp.Db.ExecuteQuery($"select email from ccMembers where id={authorMemberId}")) {
                                    if (dtAuthor?.Rows != null && dtAuthor.Rows.Count > 0) {
                                        fromAddress = cp.Utils.EncodeText(dtAuthor.Rows[0]["email"]);
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(fromAddress) && ownerMemberId > 0) {
                                using (DataTable dtOwner = cp.Db.ExecuteQuery($"select email from ccMembers where id={ownerMemberId}")) {
                                    if (dtOwner?.Rows != null && dtOwner.Rows.Count > 0) {
                                        fromAddress = cp.Utils.EncodeText(dtOwner.Rows[0]["email"]);
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(fromAddress)) {
                                fromAddress = cp.User.Email;
                            }
                            if (string.IsNullOrEmpty(fromAddress)) {
                                fromAddress = cp.Site.GetText("EmailFromAddress", $"info@{cp.Site.DomainPrimary}");
                            }
                            //
                            // -- create group email record with the same guid as the blog post
                            groupEmailId = cp.Content.AddRecord(constants.cnGroupEmail);
                            if (groupEmailId > 0) {
                                using (var cs = cp.CSNew()) {
                                    if (cs.OpenRecord(constants.cnGroupEmail, groupEmailId)) {
                                        cs.SetField("name", postName);
                                        cs.SetField("subject", postName);
                                        cs.SetField("fromAddress", fromAddress);
                                        cs.SetField("copyFilename", postCopy);
                                        if (!string.IsNullOrEmpty(postGuid)) {
                                            cs.SetField("ccguid", postGuid);
                                        }
                                        cs.Save();
                                    }
                                }
                            }
                        }
                        if (groupEmailId > 0) {
                            //
                            // -- redirect to admin edit form for the Group Email record
                            string adminEditUrl = $"{cp.Site.GetText("adminUrl", "/admin")}?cid={cp.Content.GetID(constants.cnGroupEmail)}&id={groupEmailId}&af=4";
                            cp.Log.Warn($"BlogPostDetailsAddon, Email Version button clicked, redirecting to Group Email admin edit, emailId [{groupEmailId}]");
                            cp.Response.Redirect(adminEditUrl);
                            return;
                        }
                    }
                }
                if (button == constants.buttonCancel || button == constants.buttonOK) {
                    //
                    // -- return to post list
                    cp.Log.Warn($"BlogPostDetailsAddon, {button} button clicked, blogId [{blogId}], redirecting to BlogPostList");
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
                int postId = cp.Doc.GetInteger("id");
                if (postId == 0) { postId = cp.Doc.GetInteger(constants.rnBlogPostId); }
                int blogId = cp.Doc.GetInteger(constants.rnBlogId);
                //
                // -- load post by id including inactive records (admin context)
                string postName = "";
                string postCopy = "";
                using (DataTable dtPost = cp.Db.ExecuteQuery($"select id,name,copy,blogId from ccBlogCopy where id={postId}")) {
                    if (dtPost?.Rows == null || dtPost.Rows.Count == 0) {
                        cp.Log.Warn($"BlogPostDetailsAddon, post not found, blogId [{blogId}], postId [{postId}], redirecting to BlogPostList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList);
                    }
                    postName = cp.Utils.EncodeText(dtPost.Rows[0]["name"]);
                    postCopy = cp.Utils.EncodeText(dtPost.Rows[0]["copy"]);
                    if (blogId == 0) { blogId = cp.Utils.EncodeInteger(dtPost.Rows[0]["blogId"]); }
                }
                //
                // -- load blog by id including inactive records (admin context)
                string blogName = "";
                using (DataTable dtBlog = cp.Db.ExecuteQuery($"select id,name from ccBlogs where id={blogId}")) {
                    if (dtBlog?.Rows == null || dtBlog.Rows.Count == 0) {
                        cp.Log.Warn($"BlogPostDetailsAddon, blog not found, blogId [{blogId}], redirecting to BlogList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogList);
                    }
                    blogName = cp.Utils.EncodeText(dtBlog.Rows[0]["name"]);
                }
                //
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderNameValue();
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostDetails;
                //
                layoutBuilder.title = $"Edit Post: {postName}";
                layoutBuilder.description = "";
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                //
                // -- form fields
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Title";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostTitle", 255, postName, "form-control");
                layoutBuilder.rowHelp = "The title of the blog post.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Content";
                layoutBuilder.rowValue = cp.Html.InputWysiwyg("rnPostCopy", postCopy, CPHtmlBaseClass.EditorUserScope.Administrator);
                layoutBuilder.rowHelp = "The full content of the blog post.";
                //
                // -- feature subnav
                cp.Doc.AddRefreshQueryString(constants.rnBlogId, blogId);
                cp.Doc.AddRefreshQueryString(constants.rnBlogPostId, postId);
                layoutBuilder.portalSubNavTitleList.Add($"{blogName}, #{blogId}");
                layoutBuilder.portalSubNavTitleList.Add(postName);
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
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
