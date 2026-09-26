
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;
using System.Data;

namespace Contensive.Blog {
    public class BlogPostRssAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogPostRss;
        public const string guidAddon = constants.guidAddonBlogPostRss;
        //
        public override object Execute(CPBaseClass cp) {
            try {
                if (!cp.User.IsAdmin) { return "<p>You are not authorized to access this feature.</p>"; }
                if (!cp.AdminUI.EndpointContainsPortal()) {
                    cp.Log.Warn($"BlogPostRssAddon, endpoint does not contain portal, redirecting to BlogList");
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
                        post.rssTitle = cp.Doc.GetText("rnPostRssTitle");
                        post.rssDescription = cp.Doc.GetText("rnPostRssDescription");
                        post.podcastMediaLink = cp.Doc.GetText("rnPostPodcastMediaLink");
                        post.save(cp);
                    }
                }
                if (button == constants.buttonCancel || button == constants.buttonOK) {
                    //
                    // -- return to post list
                    cp.Log.Warn($"BlogPostRssAddon, {button} button clicked, blogId [{blogId}], redirecting to BlogPostList");
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
                        cp.Log.Warn($"BlogPostRssAddon, blog not found, blogId [{blogId}], redirecting to BlogList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogList);
                    }
                    blogName = cp.Utils.EncodeText(dtBlog.Rows[0]["name"]);
                }
                //
                // -- load post by id including inactive records (admin context)
                string postName = "";
                string postRssTitle = "";
                string postRssDescription = "";
                string postPodcastMediaLink = "";
                using (DataTable dtPost = cp.Db.ExecuteQuery($"select id,name,rssTitle,rssDescription,podcastMediaLink from ccBlogCopy where id={postId}")) {
                    if (dtPost?.Rows == null || dtPost.Rows.Count == 0) {
                        cp.Log.Warn($"BlogPostRssAddon, post not found, blogId [{blogId}], postId [{postId}], redirecting to BlogPostList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList);
                    }
                    postName = cp.Utils.EncodeText(dtPost.Rows[0]["name"]);
                    postRssTitle = cp.Utils.EncodeText(dtPost.Rows[0]["rssTitle"]);
                    postRssDescription = cp.Utils.EncodeText(dtPost.Rows[0]["rssDescription"]);
                    postPodcastMediaLink = cp.Utils.EncodeText(dtPost.Rows[0]["podcastMediaLink"]);
                }
                //
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderNameValue();
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostRss;
                //
                // -- form fields
                layoutBuilder.addRow();
                layoutBuilder.rowName = "RSS Title";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostRssTitle", 255, postRssTitle, "form-control");
                layoutBuilder.rowHelp = "The title used in the RSS feed for this post.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "RSS Description";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostRssDescription", 255, postRssDescription, "form-control");
                layoutBuilder.rowHelp = "The description used in the RSS feed for this post.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Podcast Media Link";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostPodcastMediaLink", 255, postPodcastMediaLink, "form-control");
                layoutBuilder.rowHelp = "A link to a video or audio file. The RSS feed will create it as a podcast enclosure.";
                //
                // -- layout settings
                layoutBuilder.title = "Post RSS";
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
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogPostRss);
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
