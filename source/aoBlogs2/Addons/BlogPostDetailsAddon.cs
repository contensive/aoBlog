
using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
                //
                // -- check for email dialog action first (dialog submits without a button value)
                string emailAction = cp.Doc.GetText(constants.rnEmailAction);
                if (!string.IsNullOrEmpty(emailAction)) {
                    int postId = cp.Doc.GetInteger(constants.rnBlogPostId);
                    if (postId == 0) { postId = cp.Doc.GetInteger("id"); }
                    int blogId = cp.Doc.GetInteger(constants.rnBlogId);
                    //
                    var blogPost = DbBaseModel.create<BlogEntryModel>(cp, postId);
                    if (blogPost == null) { return; }
                    if (blogId == 0) { blogId = blogPost.blogId; }
                    string postGuid = blogPost.ccguid ?? "";
                    //
                    // -- check if a Group Email already exists with this blog post's guid
                    int existingEmailId = 0;
                    if (!string.IsNullOrEmpty(postGuid)) {
                        using (var csEmail = cp.CSNew()) {
                            if (csEmail.Open(constants.cnGroupEmail, $"ccguid={cp.Db.EncodeSQLText(postGuid)}")) {
                                existingEmailId = csEmail.GetInteger("id");
                            }
                        }
                    }
                    //
                    if (emailAction == "goto" && existingEmailId > 0) {
                        string adminEditUrl = $"{cp.Site.GetText("adminUrl", "/admin")}?cid={cp.Content.GetID(constants.cnGroupEmail)}&id={existingEmailId}&af=4";
                        cp.Response.Redirect(adminEditUrl);
                        return;
                    }
                    if (emailAction == "create") {
                        //
                        // -- delete existing email if present
                        if (existingEmailId > 0) {
                            cp.Content.Delete(constants.cnGroupEmail, $"id={existingEmailId}");
                        }
                        //
                        // -- get dialog inputs
                        int paragraphCount = cp.Doc.GetInteger(constants.rnEmailParagraphCount);
                        if (paragraphCount < 1) { paragraphCount = 3; }
                        int emailTemplateId = cp.Doc.GetInteger(constants.rnEmailTemplateId);
                        //
                        // -- save paragraph count preference for this user
                        cp.User.SetProperty(constants.userPropertyEmailParagraphCount, paragraphCount);
                        //
                        // -- truncate copy to requested number of blocks
                        string postCopy = blogPost.copy ?? "";
                        string truncatedCopy = truncateToBlocks(postCopy, paragraphCount);
                        //
                        // -- build "Read More..." link with UTM parameters
                        string readMoreHtml = buildReadMoreLink(cp, blogPost);
                        truncatedCopy += readMoreHtml;
                        //
                        // -- resolve from-address: post author > blog owner > current user > site default
                        string fromAddress = resolveFromAddress(cp, blogPost.authorMemberId, blogId);
                        //
                        // -- create group email record
                        int groupEmailId = cp.Content.AddRecord(constants.cnGroupEmail);
                        if (groupEmailId > 0) {
                            using (var cs = cp.CSNew()) {
                                if (cs.OpenRecord(constants.cnGroupEmail, groupEmailId)) {
                                    cs.SetField("name", blogPost.name);
                                    cs.SetField("subject", blogPost.name);
                                    cs.SetField("fromAddress", fromAddress);
                                    cs.SetField("copyFilename", truncatedCopy);
                                    if (emailTemplateId > 0) {
                                        cs.SetField("emailTemplateId", emailTemplateId.ToString());
                                    }
                                    if (!string.IsNullOrEmpty(postGuid)) {
                                        cs.SetField("ccguid", postGuid);
                                    }
                                    cs.Save();
                                }
                            }
                            //
                            // -- redirect to admin edit form for the Group Email record
                            string adminEditUrl = $"{cp.Site.GetText("adminUrl", "/admin")}?cid={cp.Content.GetID(constants.cnGroupEmail)}&id={groupEmailId}&af=4";
                            cp.Log.Warn($"BlogPostDetailsAddon, Email Version created, redirecting to Group Email admin edit, emailId [{groupEmailId}]");
                            cp.Response.Redirect(adminEditUrl);
                            return;
                        }
                    }
                    return;
                }
                //
                string button = cp.Doc.GetText(constants.rnButton);
                if (string.IsNullOrEmpty(button)) { return; }
                int postId2 = cp.Doc.GetInteger("id");
                if (postId2 == 0) { postId2 = cp.Doc.GetInteger(constants.rnBlogPostId); }
                int blogId2 = cp.Doc.GetInteger(constants.rnBlogId);
                if (blogId2 == 0 && postId2 > 0) {
                    var post = DbBaseModel.create<BlogEntryModel>(cp, postId2);
                    if (post != null) { blogId2 = post.blogId; }
                }
                //
                if (button == constants.buttonSave || button == constants.buttonOK) {
                    //
                    // -- save the post copy (cs.Open includes inactive records)
                    using (var cs = cp.CSNew()) {
                        cs.Open(constants.cnBlogEntries, $"id={postId2}");
                        if (cs.OK()) {
                            cs.SetFormInput("name", "rnPostTitle");
                            cs.SetFormInput("copy", "rnPostCopy");
                        }
                        cs.Close();
                    }
                }
                if (button == constants.buttonEmailVersion) {
                    //
                    // -- Email Version button clicked, getForm will show the dialog
                    return;
                }
                if (button == constants.buttonCancel || button == constants.buttonOK) {
                    //
                    // -- return to post list
                    cp.Log.Warn($"BlogPostDetailsAddon, {button} button clicked, blogId [{blogId2}], redirecting to BlogPostList");
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
                string postGuid = "";
                using (DataTable dtPost = cp.Db.ExecuteQuery($"select id,name,copy,blogId,ccguid from ccBlogCopy where id={postId}")) {
                    if (dtPost?.Rows == null || dtPost.Rows.Count == 0) {
                        cp.Log.Warn($"BlogPostDetailsAddon, post not found, blogId [{blogId}], postId [{postId}], redirecting to BlogPostList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList);
                    }
                    postName = cp.Utils.EncodeText(dtPost.Rows[0]["name"]);
                    postCopy = cp.Utils.EncodeText(dtPost.Rows[0]["copy"]);
                    postGuid = cp.Utils.EncodeText(dtPost.Rows[0]["ccguid"]);
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
                // -- check if Email Version button was just clicked (no action yet means show dialog)
                string button = cp.Doc.GetText(constants.rnButton);
                string emailAction = cp.Doc.GetText(constants.rnEmailAction);
                bool showEmailDialog = (button == constants.buttonEmailVersion && string.IsNullOrEmpty(emailAction));
                cp.Log.Warn($"BlogPostDetailsAddon.getForm, button=[{button}], emailAction=[{emailAction}], showEmailDialog=[{showEmailDialog}], buttonEmailVersion=[{constants.buttonEmailVersion}], match=[{button == constants.buttonEmailVersion}]");
                if (showEmailDialog) {
                    string dialogHtml = buildEmailVersionDialog(cp, postGuid);
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
        /// Build the Email Version dialog as a Bootstrap 5 modal that auto-opens.
        /// </summary>
        private static string buildEmailVersionDialog(CPBaseClass cp, string postGuid) {
            //
            // -- get user's saved paragraph count preference (default 3)
            int defaultParagraphCount = cp.User.GetInteger(constants.userPropertyEmailParagraphCount, 3);
            if (defaultParagraphCount < 1) { defaultParagraphCount = 3; }
            //
            // -- check if email already exists for this post
            bool emailExists = false;
            if (!string.IsNullOrEmpty(postGuid)) {
                using (var csEmail = cp.CSNew()) {
                    emailExists = csEmail.Open(constants.cnGroupEmail, $"ccguid={cp.Db.EncodeSQLText(postGuid)}");
                }
            }
            //
            // -- load email templates for dropdown using content name to filter by content control ID
            var templates = new List<KeyValuePair<int, string>>();
            using (var csTemplate = cp.CSNew()) {
                if (csTemplate.Open("Email Templates", "(active<>0)", "name")) {
                    while (csTemplate.OK()) {
                        templates.Add(new KeyValuePair<int, string>(csTemplate.GetInteger("id"), csTemplate.GetText("name")));
                        csTemplate.GoNext();
                    }
                }
            }
            //
            var sb = new StringBuilder();
            sb.Append("<div class=\"modal fade\" id=\"emailVersionModal\" tabindex=\"-1\" aria-labelledby=\"emailVersionModalLabel\" aria-hidden=\"true\">");
            sb.Append("<div class=\"modal-dialog\">");
            sb.Append("<div class=\"modal-content\">");
            //
            // -- header
            sb.Append("<div class=\"modal-header\">");
            sb.Append("<h5 class=\"modal-title\" id=\"emailVersionModalLabel\">Email Version</h5>");
            sb.Append("<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\" aria-label=\"Close\"></button>");
            sb.Append("</div>");
            //
            // -- body
            sb.Append("<div class=\"modal-body\">");
            //
            // -- paragraph count input
            sb.Append("<div class=\"mb-3\">");
            sb.Append("<label for=\"emailParagraphCount\" class=\"form-label\">Include how many paragraphs</label>");
            sb.Append($"<input type=\"number\" class=\"form-control\" id=\"emailParagraphCount\" name=\"{constants.rnEmailParagraphCount}\" value=\"{defaultParagraphCount}\" min=\"1\">");
            sb.Append("</div>");
            //
            // -- email template dropdown
            sb.Append("<div class=\"mb-3\">");
            sb.Append("<label for=\"emailTemplateId\" class=\"form-label\">Email Template</label>");
            sb.Append($"<select class=\"form-select\" id=\"emailTemplateId\" name=\"{constants.rnEmailTemplateId}\">");
            sb.Append("<option value=\"0\">-- Select Template --</option>");
            foreach (var template in templates) {
                sb.Append($"<option value=\"{template.Key}\">{cp.Utils.EncodeHTML(template.Value)}</option>");
            }
            sb.Append("</select>");
            sb.Append("</div>");
            //
            sb.Append("</div>");
            //
            // -- footer with action buttons
            sb.Append("<div class=\"modal-footer\">");
            sb.Append("<button type=\"button\" class=\"btn btn-secondary\" data-bs-dismiss=\"modal\">Cancel</button>");
            if (emailExists) {
                sb.Append($"<button type=\"button\" class=\"btn btn-info\" onclick=\"document.querySelector('input[name={constants.rnEmailAction}]').value='goto';this.closest('form').submit();\">Go To Existing Email</button>");
            }
            sb.Append($"<button type=\"button\" class=\"btn btn-primary\" onclick=\"document.querySelector('input[name={constants.rnEmailAction}]').value='create';this.closest('form').submit();\">Create New Email</button>");
            sb.Append("</div>");
            //
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</div>");
            //
            // -- hidden field for action
            sb.Append($"<input type=\"hidden\" name=\"{constants.rnEmailAction}\" value=\"\">");
            //
            // -- script to auto-open modal on page load
            sb.Append("<script>document.addEventListener('DOMContentLoaded',function(){var m=new bootstrap.Modal(document.getElementById('emailVersionModal'));m.show();});</script>");
            //
            return sb.ToString();
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Truncate HTML to a specified number of block-level elements.
        /// [imageNNN] placeholders are not counted as blocks but are kept attached to the following block.
        /// The primary image is handled separately and is not part of the copy.
        /// </summary>
        internal static string truncateToBlocks(string htmlCopy, int blockCount) {
            if (string.IsNullOrEmpty(htmlCopy) || blockCount < 1) { return htmlCopy; }
            //
            // -- regex to match block-level opening tags
            string blockTags = "p|h[1-6]|div|blockquote|ul|ol|table|figure|section|pre";
            string pattern = $@"<(?:{blockTags})[\s>]";
            //
            var matches = Regex.Matches(htmlCopy, pattern, RegexOptions.IgnoreCase);
            if (matches.Count == 0) { return htmlCopy; }
            //
            int blocksFound = 0;
            int cutPosition = htmlCopy.Length;
            //
            for (int i = 0; i < matches.Count; i++) {
                int matchStart = matches[i].Index;
                //
                // -- check if this block is preceded by an [imageNNN] placeholder
                // -- look backward from this match for [imageNNN] that is not separated by another block
                string beforeMatch = htmlCopy.Substring(0, matchStart);
                var imagePlaceholder = Regex.Match(beforeMatch, @"\[image\d+\]\s*$", RegexOptions.IgnoreCase);
                //
                blocksFound++;
                if (blocksFound > blockCount) {
                    //
                    // -- we've found one block beyond our limit; cut before this block
                    // -- if there's an [imageNNN] placeholder right before it, cut before that too
                    if (imagePlaceholder.Success) {
                        cutPosition = imagePlaceholder.Index;
                    } else {
                        cutPosition = matchStart;
                    }
                    break;
                }
            }
            //
            string result = htmlCopy.Substring(0, cutPosition);
            //
            // -- close any open tags
            result = closeOpenTags(result);
            //
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Close any HTML tags that are left open after truncation.
        /// </summary>
        private static string closeOpenTags(string html) {
            var openTags = new Stack<string>();
            var tagPattern = new Regex(@"<(/?)(\w+)[^>]*?(/?)>", RegexOptions.IgnoreCase);
            var selfClosingTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "br", "img", "hr", "input", "meta", "link", "area", "base", "col", "embed", "source", "track", "wbr" };
            foreach (Match match in tagPattern.Matches(html)) {
                string isClosing = match.Groups[1].Value;
                string tagName = match.Groups[2].Value.ToLowerInvariant();
                string isSelfClosing = match.Groups[3].Value;
                if (selfClosingTags.Contains(tagName) || !string.IsNullOrEmpty(isSelfClosing)) {
                    continue;
                }
                if (string.IsNullOrEmpty(isClosing)) {
                    openTags.Push(tagName);
                } else {
                    // -- pop until we find a match
                    var temp = new Stack<string>();
                    while (openTags.Count > 0) {
                        string top = openTags.Pop();
                        if (top == tagName) { break; }
                        temp.Push(top);
                    }
                    while (temp.Count > 0) {
                        openTags.Push(temp.Pop());
                    }
                }
            }
            var sb = new StringBuilder(html);
            while (openTags.Count > 0) {
                sb.Append($"</{openTags.Pop()}>");
            }
            return sb.ToString();
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Build the "Read More..." paragraph with a full URL and UTM parameters.
        /// </summary>
        private static string buildReadMoreLink(CPBaseClass cp, BlogEntryModel blogPost) {
            //
            // -- build article URL using same pattern as StructuredDataController
            string qs = LinkAliasController.getLinkAliasQueryString(cp, blogPost.id);
            int pageId = blogPost.blogpostpageid;
            if (pageId == 0) {
                //
                // -- fallback: try to find blog's page
                var blog = DbBaseModel.create<BlogModel>(cp, blogPost.blogId);
                if (blog != null) {
                    // -- use current doc page as last resort
                    pageId = cp.Doc.PageId;
                }
            }
            string articleUrl = cp.Content.GetPageLink(pageId, qs);
            if (!articleUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                articleUrl = $"https://{cp.Site.DomainPrimary}{articleUrl}";
            }
            //
            // -- build UTM parameters
            string utmSource = Uri.EscapeDataString($"blog post - {blogPost.name}");
            string utmMedium = "email";
            string utmCampaign = "";
            //
            // -- build utm_message from topics associated with this blog entry
            string utmMessage = buildUtmMessage(cp, blogPost.id);
            //
            string separator = articleUrl.Contains("?") ? "&" : "?";
            string fullUrl = $"{articleUrl}{separator}utm_source={utmSource}&utm_medium={utmMedium}&utm_campaign={Uri.EscapeDataString(utmCampaign)}&utm_message={Uri.EscapeDataString(utmMessage)}";
            //
            return $"<p><a href=\"{fullUrl}\">Read More...</a></p>";
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Build the utm_message value from topics associated with a blog entry.
        /// Returns a comma-delimited list of double-quoted topic names.
        /// </summary>
        private static string buildUtmMessage(CPBaseClass cp, int blogEntryId) {
            var topicRules = BlogEntryTopicRuleModel.createList(cp, $"blogEntryId={blogEntryId}");
            if (topicRules == null || topicRules.Count == 0) { return ""; }
            //
            var topicNames = new List<string>();
            foreach (var rule in topicRules) {
                if (rule.topicId > 0) {
                    var topic = DbBaseModel.create<TopicModel>(cp, rule.topicId);
                    if (topic != null && !string.IsNullOrEmpty(topic.name)) {
                        topicNames.Add($"\"{topic.name}\"");
                    }
                }
            }
            return string.Join(",", topicNames);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Resolve the from-address for the email: post author > blog owner > current user > site default.
        /// </summary>
        private static string resolveFromAddress(CPBaseClass cp, int authorMemberId, int blogId) {
            string fromAddress = "";
            if (authorMemberId > 0) {
                using (DataTable dtAuthor = cp.Db.ExecuteQuery($"select email from ccMembers where id={authorMemberId}")) {
                    if (dtAuthor?.Rows != null && dtAuthor.Rows.Count > 0) {
                        fromAddress = cp.Utils.EncodeText(dtAuthor.Rows[0]["email"]);
                    }
                }
            }
            if (string.IsNullOrEmpty(fromAddress)) {
                int ownerMemberId = 0;
                using (DataTable dtBlog = cp.Db.ExecuteQuery($"select ownerMemberId from ccBlogs where id={blogId}")) {
                    if (dtBlog?.Rows != null && dtBlog.Rows.Count > 0) {
                        ownerMemberId = cp.Utils.EncodeInteger(dtBlog.Rows[0]["ownerMemberId"]);
                    }
                }
                if (ownerMemberId > 0) {
                    using (DataTable dtOwner = cp.Db.ExecuteQuery($"select email from ccMembers where id={ownerMemberId}")) {
                        if (dtOwner?.Rows != null && dtOwner.Rows.Count > 0) {
                            fromAddress = cp.Utils.EncodeText(dtOwner.Rows[0]["email"]);
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(fromAddress)) {
                fromAddress = cp.User.Email;
            }
            if (string.IsNullOrEmpty(fromAddress)) {
                fromAddress = cp.Site.GetText("EmailFromAddress", $"info@{cp.Site.DomainPrimary}");
            }
            return fromAddress;
        }
    }
}
