
using Contensive.BaseClasses;
using System;
using System.Data;
using System.Text;

namespace Contensive.Blog {
    public class BlogDetailsAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogDetails;
        public const string guidAddon = constants.guidAddonBlogDetails;
        //
        public override object Execute(CPBaseClass cp) {
            try {
                if (!cp.User.IsAdmin) { return "<p>You are not authorized to access this feature.</p>"; }
                if (!cp.AdminUI.EndpointContainsPortal()) {
                    cp.Log.Warn($"BlogDetailsAddon, endpoint does not contain portal, redirecting to BlogList");
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
                //
                // -- check for delete dialog action first (dialog submits without a button value)
                string deleteAction = cp.Doc.GetText(constants.rnDeleteAction);
                if (!string.IsNullOrEmpty(deleteAction) && deleteAction == "confirm") {
                    int blogId = cp.Doc.GetInteger(constants.rnBlogId);
                    if (blogId == 0) { blogId = cp.Doc.GetInteger("id"); }
                    if (blogId > 0) {
                        //
                        // -- delete all posts for this blog
                        cp.Content.Delete(constants.cnBlogEntries, $"blogId={blogId}");
                        //
                        // -- delete the blog
                        cp.Content.Delete(constants.cnBlogs, $"id={blogId}");
                        //
                        cp.Log.Warn($"BlogDetailsAddon, blog [{blogId}] and all its posts deleted");
                    }
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogList, "");
                    return;
                }
                //
                if (!cp.Doc.IsProperty(constants.rnButton)) { return; }
                string button = cp.Doc.GetText(constants.rnButton);
                //
                if ((button ?? "") == constants.buttonDelete) {
                    //
                    // -- Delete button clicked, getForm will show the confirmation dialog
                    return;
                }
                if ((button ?? "") == constants.buttonSave || (button ?? "") == constants.buttonOK) {
                    //
                    // -- save changes
                    int blogId = cp.Doc.GetInteger("id");
                    if (blogId == 0) { blogId = cp.Doc.GetInteger(constants.rnBlogId); }
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
                            string snoozeInput = cp.Doc.GetText("rnBlogAlarmSnoozeDate");
                            if (string.IsNullOrWhiteSpace(snoozeInput)) {
                                cs.SetField("blogUpdateAlarmSnoozeDate", "");
                            } else {
                                cs.SetField("blogUpdateAlarmSnoozeDate", snoozeInput);
                            }
                        }
                        cs.Close();
                    }
                }
                if ((button ?? "") == constants.buttonCancel || (button ?? "") == constants.buttonOK) {
                    cp.Log.Warn($"BlogDetailsAddon, {button} button clicked, redirecting to BlogList");
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogList, "");
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
                int blogId = cp.Doc.GetInteger("id");
                if (blogId == 0) { blogId = cp.Doc.GetInteger(constants.rnBlogId); }
                //
                // -- verify blog exists including inactive records (admin context)
                using (DataTable dtBlog = cp.Db.ExecuteQuery($"select id from ccBlogs where id={blogId}")) {
                    if (dtBlog?.Rows == null || dtBlog.Rows.Count == 0) {
                        layoutBuilder.warningMessage = "This blog is not valid.";
                        return layoutBuilder.getHtml();
                    }
                }
                //
                // -- refresh query string
                cp.Doc.AddRefreshQueryString(constants.rnDstFeatureGuid, constants.guidPortalFeatureBlogDetails);
                cp.Doc.AddRefreshQueryString(constants.rnBlogId, blogId);
                //
                string blogName = "";
                using (var cs = cp.CSNew()) {
                    cs.Open(constants.cnBlogs, $"id={blogId}");
                    if (!cs.OK()) {
                        layoutBuilder.portalSubNavTitleList.Add("Unknown Blog");
                        layoutBuilder.addRow();
                        layoutBuilder.rowName = "&nbsp;";
                        layoutBuilder.rowValue = $"Blog [{blogId}] was not found.";
                        return layoutBuilder.getHtml();
                    }
                    //
                    blogName = cs.GetText("name");
                    layoutBuilder.portalSubNavTitleList.Add($"{blogName}, #{cs.GetInteger("id")}");
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
                    DateTime snoozeDate = cs.GetDate("blogUpdateAlarmSnoozeDate");
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Alarm Snooze Date";
                    layoutBuilder.rowValue = cp.Html5.InputDate("rnBlogAlarmSnoozeDate", snoozeDate == DateTime.MinValue ? DateTime.MinValue : snoozeDate.Date, "form-control");
                    layoutBuilder.rowHelp = "Set a future date to suppress the blog update alarm until after that date. The alarm will resume after this date passes.";
                    //
                    cs.Close();
                }
                //
                // -- buttons
                layoutBuilder.addFormButton(constants.buttonCancel);
                layoutBuilder.addFormButton(constants.buttonSave);
                layoutBuilder.addFormButton(constants.buttonOK);
                layoutBuilder.addFormButton(constants.buttonDelete, constants.rnButton, "", "btn btn-danger float-end ms-2");
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogDetails);
                layoutBuilder.addFormHidden(constants.rnBlogId, blogId);
                //
                // -- check if Delete button was just clicked (show confirmation dialog)
                string button = cp.Doc.GetText(constants.rnButton);
                if ((button ?? "") == constants.buttonDelete) {
                    string dialogHtml = buildDeleteConfirmDialog(cp, blogName);
                    layoutBuilder.htmlAfterBody += dialogHtml;
                }
                //
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Build the delete confirmation dialog as a Bootstrap 5 modal that auto-opens.
        /// </summary>
        private static string buildDeleteConfirmDialog(CPBaseClass cp, string blogName) {
            var sb = new StringBuilder();
            sb.Append("<div class=\"modal fade\" id=\"deleteBlogModal\" tabindex=\"-1\" aria-labelledby=\"deleteBlogModalLabel\" aria-hidden=\"true\">");
            sb.Append("<div class=\"modal-dialog\">");
            sb.Append("<div class=\"modal-content\">");
            //
            // -- header
            sb.Append("<div class=\"modal-header\">");
            sb.Append("<h5 class=\"modal-title\" id=\"deleteBlogModalLabel\">Delete Blog</h5>");
            sb.Append("<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\" aria-label=\"Close\"></button>");
            sb.Append("</div>");
            //
            // -- body
            sb.Append("<div class=\"modal-body\">");
            sb.Append($"<p>Are you sure you want to permanently delete the blog named \"{cp.Utils.EncodeHTML(blogName)}\"?</p>");
            sb.Append("<p>This will also delete all of its posts. This action cannot be undone.</p>");
            sb.Append("</div>");
            //
            // -- footer
            sb.Append("<div class=\"modal-footer\">");
            sb.Append("<button type=\"button\" class=\"btn btn-secondary\" data-bs-dismiss=\"modal\">No</button>");
            sb.Append($"<button type=\"button\" class=\"btn btn-danger\" onclick=\"document.querySelector('input[name={constants.rnDeleteAction}]').value='confirm';this.closest('form').submit();\">Yes, Delete</button>");
            sb.Append("</div>");
            //
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</div>");
            //
            // -- hidden field for delete action
            sb.Append($"<input type=\"hidden\" name=\"{constants.rnDeleteAction}\" value=\"\">");
            //
            // -- auto-open modal
            sb.Append("<script>document.addEventListener('DOMContentLoaded',function(){var m=new bootstrap.Modal(document.getElementById('deleteBlogModal'));m.show();});</script>");
            //
            return sb.ToString();
        }
    }
}
