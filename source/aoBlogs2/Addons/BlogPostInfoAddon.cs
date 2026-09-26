
using Contensive.BaseClasses;
using System;
using System.Data;

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
                    cp.Log.Warn($"BlogPostInfoAddon, endpoint does not contain portal, redirecting to BlogList");
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
                            cs.SetFormInput("datePublished", "rnPostDatePublished");
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
                    cp.Log.Warn($"BlogPostInfoAddon, Delete button clicked, blogId [{blogId}], postId [{postId}], redirecting to BlogPostList");
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList, "");
                    return;
                }
                if (button == constants.buttonCancel || button == constants.buttonOK) {
                    //
                    // -- return to post list
                    cp.Log.Warn($"BlogPostInfoAddon, {button} button clicked, blogId [{blogId}], redirecting to BlogPostList");
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
                        cp.Log.Warn($"BlogPostInfoAddon, blog not found, blogId [{blogId}], redirecting to BlogList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogList);
                    }
                    blogName = cp.Utils.EncodeText(dtBlog.Rows[0]["name"]);
                }
                //
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderNameValue();
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostInfo;
                //
                // -- load post by id including inactive records (admin context)
                bool isNew = true;
                bool postActive = true;
                DateTime postDateAdded = DateTime.MinValue;
                DateTime postDatePublished = DateTime.MinValue;
                int postViewings = 0;
                bool postAllowComments = false;
                string postTagList = "";
                string postName = "";
                if (postId > 0) {
                    using (DataTable dtPost = cp.Db.ExecuteQuery($"select name,active,dateAdded,datePublished,viewings,allowComments,tagList from ccBlogCopy where id={postId}")) {
                        if (dtPost?.Rows != null && dtPost.Rows.Count > 0) {
                            isNew = false;
                            postName = cp.Utils.EncodeText(dtPost.Rows[0]["name"]);
                            postActive = cp.Utils.EncodeBoolean(dtPost.Rows[0]["active"]);
                            postDateAdded = cp.Utils.EncodeDate(dtPost.Rows[0]["dateAdded"]);
                            postDatePublished = cp.Utils.EncodeDate(dtPost.Rows[0]["datePublished"]);
                            postViewings = cp.Utils.EncodeInteger(dtPost.Rows[0]["viewings"]);
                            postAllowComments = cp.Utils.EncodeBoolean(dtPost.Rows[0]["allowComments"]);
                            postTagList = cp.Utils.EncodeText(dtPost.Rows[0]["tagList"]);
                        }
                    }
                }
                //
                layoutBuilder.title = isNew ? "Add Post" : "Post Info";
                layoutBuilder.description = "";
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                //
                // -- form fields
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Active";
                layoutBuilder.rowValue = cp.Html5.CheckBox("rnPostActive", postActive, "form-check-input");
                layoutBuilder.rowHelp = "When unchecked, this post will not be displayed.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Date Added";
                layoutBuilder.rowValue = postDateAdded == DateTime.MinValue ? "" : postDateAdded.ToShortDateString();
                layoutBuilder.rowHelp = "The date this post was created.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Publish Date";
                layoutBuilder.rowValue = "<div style=\"display:inline-block;width:400px\">"
                    + cp.Html5.InputDate("rnPostDatePublished", postDatePublished == DateTime.MinValue ? DateTime.MinValue : postDatePublished.Date, "form-control")
                    + "</div>";
                layoutBuilder.rowHelp = "Posts are ordered by this date and it displays on the blog page. If this date is missing, the date added is used.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Views";
                layoutBuilder.rowValue = postViewings.ToString();
                layoutBuilder.rowHelp = "The number of times this post has been viewed.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Allow Comments";
                layoutBuilder.rowValue = cp.Html5.CheckBox("rnPostAllowComments", postAllowComments, "form-check-input");
                layoutBuilder.rowHelp = "When checked, comments can be posted on this article.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Tags";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostTagList", 255, postTagList, "form-control");
                layoutBuilder.rowHelp = "Comma-delimited list of tags for this post.";
                //
                // -- feature subnav
                cp.Doc.AddRefreshQueryString(constants.rnBlogId, blogId);
                cp.Doc.AddRefreshQueryString(constants.rnBlogPostId, postId);
                layoutBuilder.portalSubNavTitleList.Add($"{blogName}, #{blogId}");
                if (!isNew) {
                    layoutBuilder.portalSubNavTitleList.Add(postName);
                }
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
