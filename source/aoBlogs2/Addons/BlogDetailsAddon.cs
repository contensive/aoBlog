
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;

namespace Contensive.Blog {
    public class BlogDetailsAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogDetails;
        public const string guidAddon = constants.guidAddonBlogDetails;
        //
        public override object Execute(CPBaseClass cp) {
            try {
                if (!cp.User.IsAdmin) { return "<p>You are not authorized to access this feature.</p>"; }
                if (!cp.AdminUI.EndpointContainsPortal()) { return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogList, ""); }
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
                if (!cp.Doc.IsProperty(constants.rnButton)) { return; }
                string button = cp.Doc.GetText(constants.rnButton);
                if ((button ?? "") == constants.buttonSave || (button ?? "") == constants.buttonOK) {
                    //
                    // -- save changes
                    int blogId = cp.Doc.GetInteger(constants.rnBlogId);
                    using (var cs = cp.CSNew()) {
                        cs.Open(constants.cnBlogs, $"id={blogId}");
                        if (cs.OK()) {
                            cs.SetFormInput("name", "rnBlogName");
                            cs.SetFormInput("caption", "rnBlogCaption");
                            cs.SetFormInput("postsToDisplay", "rnBlogPostsToDisplay");
                            cs.SetFormInput("overviewLength", "rnBlogOverviewLength");
                            cs.SetFormInput("allowCategories", "rnBlogAllowCategories");
                            cs.SetFormInput("autoApproveComments", "rnBlogAutoApproveComments");
                            cs.SetFormInput("allowRSSSubscribe", "rnBlogAllowRSS");
                            cs.SetFormInput("allowEmailSubscribe", "rnBlogAllowEmailSubscribe");
                            cs.SetFormInput("allowSearch", "rnBlogAllowSearch");
                            cs.SetFormInput("allowAnonymous", "rnBlogAllowAnonymous");
                            cs.SetFormInput("allowArticleCTA", "rnBlogAllowArticleCTA");
                        }
                        cs.Close();
                    }
                }
                if ((button ?? "") == constants.buttonCancel || (button ?? "") == constants.buttonOK) {
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogList, "");
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
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderNameValue();
                layoutBuilder.title = "Blog Details";
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                //
                int blogId = cp.Doc.GetInteger(constants.rnBlogId);
                var blog = DbBaseModel.create<BlogModel>(cp, blogId);
                if (blog is null) {
                    layoutBuilder.warningMessage = "This blog is not valid.";
                    return layoutBuilder.getHtml();
                }
                //
                // -- refresh query string
                cp.Doc.AddRefreshQueryString(constants.rnDstFeatureGuid, constants.guidPortalFeatureBlogDetails);
                cp.Doc.AddRefreshQueryString(constants.rnBlogId, blogId);
                //
                using (var cs = cp.CSNew()) {
                    cs.Open(constants.cnBlogs, $"id={blogId}");
                    if (!cs.OK()) {
                        layoutBuilder.portalSubNavTitle = "Unknown Blog";
                        layoutBuilder.addRow();
                        layoutBuilder.rowName = "&nbsp;";
                        layoutBuilder.rowValue = $"Blog [{blogId}] was not found.";
                        return layoutBuilder.getHtml();
                    }
                    //
                    layoutBuilder.portalSubNavTitle = $"{cs.GetText("name")}, #{cs.GetInteger("id")}";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Name";
                    layoutBuilder.rowValue = cp.Html5.InputText("rnBlogName", 255, cs.GetText("name"), "form-control");
                    layoutBuilder.rowHelp = "The name of this blog.";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Caption";
                    layoutBuilder.rowValue = cp.Html5.InputText("rnBlogCaption", 255, cs.GetText("caption"), "form-control");
                    layoutBuilder.rowHelp = "The caption displayed at the top of the blog.";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Posts To Display";
                    layoutBuilder.rowValue = cp.Html5.InputText("rnBlogPostsToDisplay", 10, cs.GetInteger("postsToDisplay").ToString(), "form-control");
                    layoutBuilder.rowHelp = "The number of posts displayed per page.";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Overview Length";
                    layoutBuilder.rowValue = cp.Html5.InputText("rnBlogOverviewLength", 10, cs.GetInteger("overviewLength").ToString(), "form-control");
                    layoutBuilder.rowHelp = "The number of characters shown in the post overview.";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Allow Categories";
                    layoutBuilder.rowValue = cp.Html5.CheckBox("rnBlogAllowCategories", cs.GetBoolean("allowCategories"), "form-check-input");
                    layoutBuilder.rowHelp = "When checked, blog posts can be organized by category.";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Auto Approve Comments";
                    layoutBuilder.rowValue = cp.Html5.CheckBox("rnBlogAutoApproveComments", cs.GetBoolean("autoApproveComments"), "form-check-input");
                    layoutBuilder.rowHelp = "When checked, comments are automatically approved without moderation.";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Allow RSS";
                    layoutBuilder.rowValue = cp.Html5.CheckBox("rnBlogAllowRSS", cs.GetBoolean("allowRSSSubscribe"), "form-check-input");
                    layoutBuilder.rowHelp = "When checked, an RSS feed is available for this blog.";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Allow Email Subscribe";
                    layoutBuilder.rowValue = cp.Html5.CheckBox("rnBlogAllowEmailSubscribe", cs.GetBoolean("allowEmailSubscribe"), "form-check-input");
                    layoutBuilder.rowHelp = "When checked, visitors can subscribe to new posts by email.";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Allow Search";
                    layoutBuilder.rowValue = cp.Html5.CheckBox("rnBlogAllowSearch", cs.GetBoolean("allowSearch"), "form-check-input");
                    layoutBuilder.rowHelp = "When checked, a search feature is available on the blog.";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Allow Anonymous";
                    layoutBuilder.rowValue = cp.Html5.CheckBox("rnBlogAllowAnonymous", cs.GetBoolean("allowAnonymous"), "form-check-input");
                    layoutBuilder.rowHelp = "When checked, anonymous users can post comments.";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Allow Article CTA";
                    layoutBuilder.rowValue = cp.Html5.CheckBox("rnBlogAllowArticleCTA", cs.GetBoolean("allowArticleCTA"), "form-check-input");
                    layoutBuilder.rowHelp = "When checked, calls-to-action can be displayed on articles.";
                    //
                    cs.Close();
                }
                //
                layoutBuilder.portalSubNavTitle = $"{blog.name}, #{blog.id}";
                //
                // -- buttons
                layoutBuilder.addFormButton(constants.buttonCancel);
                layoutBuilder.addFormButton(constants.buttonSave);
                layoutBuilder.addFormButton(constants.buttonOK);
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogDetails);
                layoutBuilder.addFormHidden(constants.rnBlogId, blog.id);
                //
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
