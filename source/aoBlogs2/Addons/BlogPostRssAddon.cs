
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;

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
                        post.rssTitle = cp.Doc.GetText("rnPostRssTitle");
                        post.rssDescription = cp.Doc.GetText("rnPostRssDescription");
                        post.podcastMediaLink = cp.Doc.GetText("rnPostPodcastMediaLink");
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
                layoutBuilder.rowName = "RSS Title";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostRssTitle", 255, post.rssTitle ?? "", "form-control");
                layoutBuilder.rowHelp = "The title used in the RSS feed for this post.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "RSS Description";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostRssDescription", 255, post.rssDescription ?? "", "form-control");
                layoutBuilder.rowHelp = "The description used in the RSS feed for this post.";
                //
                layoutBuilder.addRow();
                layoutBuilder.rowName = "Podcast Media Link";
                layoutBuilder.rowValue = cp.Html5.InputText("rnPostPodcastMediaLink", 255, post.podcastMediaLink ?? "", "form-control");
                layoutBuilder.rowHelp = "A link to a video or audio file. The RSS feed will create it as a podcast enclosure.";
                //
                // -- layout settings
                layoutBuilder.title = "Post RSS";
                layoutBuilder.description = "";
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostRss;
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
