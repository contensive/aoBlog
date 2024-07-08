using Contensive.Models.Db;
using System;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Contensive.Blog.Views {
    // 
    public class ArticleView {
        // 
        // ====================================================================================
        // 
        public static string getArticleView(CPBaseClass cp, ApplicationEnvironmentModel app, bool RetryCommentPost) {
            string getArticleViewRet;
            int hint = 0;
            try {
                string result = "";
                // 
                // setup form key
                string formKey = cp.Utils.CreateGuid();
                result += cp.Html.Hidden("FormKey", formKey);
                result += "<div class=\"aoBlogHeaderLink\"><a href=\"" + app.blogPageBaseLink + "\">" + constants.BackToRecentPostsMsg + "</a></div>";
                //
                if (app.blogPost is null) {
                    hint = 10;
                    // 
                    // -- This article cannot be found
                    return result + "<div class=\"aoBlogProblem\">Sorry, the blog post you selected is not currently available</div>";
                }
                // 
                // -- count the viewing
                cp.Db.ExecuteNonQuery("update " + BlogEntryModel.tableMetadata.tableNameLower + " set viewings=" + (app.blogPost.viewings + 1));
                hint = 20;
                // 
                string entryEditLink = app.userIsEditing ? cp.Content.GetEditLink(BlogModel.tableMetadata.contentName, app.blogPost.id.ToString(), false, app.blogPost.name, true) : "";
                // 
                // Print the Blog Entry
                var return_CommentCnt = default(int);
                result += BlogEntryCellView.getBlogPostCell(cp, app, app.blogPost, true, false, return_CommentCnt, entryEditLink);
                // 
                var visit = DbBaseModel.create<VisitModel>(cp, cp.Visit.Id);
                if (app?.user != null && visit is not null) {
                    if (!visit.excludeFromAnalytics) {
                        int blogEntryId = app.blogPost is not null ? app.blogPost.id : 0;
                        var BlogViewingLog = DbBaseModel.addDefault<BlogViewingLogModel>(cp);
                        if (BlogViewingLog is not null) {
                            BlogViewingLog.name = cp.User.Name + ", post " + blogEntryId.ToString() + ", " + Conversions.ToString(DateTime.Now);
                            BlogViewingLog.BlogEntryID = blogEntryId;
                            BlogViewingLog.MemberID = cp.User.Id;
                            BlogViewingLog.VisitID = cp.Visit.Id;
                            BlogViewingLog.save(cp);
                        }
                    }
                }
                hint = 50;
                // 
                if (app?.user != null && app.user.isBlogEditor(cp, app.blog) && return_CommentCnt > 0) {
                    result += "<div class=\"aoBlogCommentCopy\">" + cp.Html.Button(constants.FormButtonApplyCommentChanges) + "</div>";
                }
                // 
                hint = 60;
                string qs;
                if (app.blogPost.allowComments & cp.Visit.CookieSupport & !visit.bot) {
                    hint = 70;
                    result += "<div class=\"aoBlogCommentHeader\">Post a Comment</div>";
                    // 
                    if (cp?.UserError != null && !cp.UserError.OK()) {
                        result += "<div class=\"aoBlogCommentError\">" + cp.UserError.OK() + "</div>";
                    }
                    // 
                    if (!app.blog.allowAnonymous & !cp.User.IsAuthenticated) {
                        hint = 80;
                        bool AllowPasswordEmail = cp.Site.GetBoolean("AllowPasswordEmail", false);
                        bool AllowMemberJoin = cp.Site.GetBoolean("AllowMemberJoin", false);
                        // 
                        int Auth = cp.Doc.GetInteger("auth");
                        if (Auth == 1 & !AllowPasswordEmail) {
                            Auth = 3;
                        } else if (Auth == 2 & !AllowMemberJoin) {
                            Auth = 3;
                        }
                        cp.Doc.AddRefreshQueryString(constants.rnFormID, constants.FormBlogPostDetails.ToString());
                        cp.Doc.AddRefreshQueryString(constants.RequestNameBlogEntryID, app.blogPost.id.ToString());
                        cp.Doc.AddRefreshQueryString("auth", "0");
                        qs = cp.Doc.RefreshQueryString;
                        string Copy;
                        hint = 90;
                        switch (Auth) {
                            case 1: {
                                    hint = 100;
                                    // 
                                    // password email
                                    // 
                                    Copy = "To retrieve your username and password, submit your email. ";
                                    qs = cp.Utils.ModifyQueryString(qs, "auth", "0");
                                    Copy = Copy + " <a href=\"?" + qs + "\"> Login?</a>";
                                    if (AllowMemberJoin) {
                                        qs = cp.Utils.ModifyQueryString(qs, "auth", "2");
                                        Copy = Copy + " <a href=\"?" + qs + "\"> Join?</a>";
                                    }
                                    result = result + "<div class=\"aoBlogLoginBox\">" + Constants.vbCrLf + Constants.vbTab + "<div class=\"aoBlogCommentCopy\">" + Copy + "</div>" + Constants.vbCrLf + Constants.vbTab + "<div class=\"aoBlogCommentCopy\">" + "send password form removed" + "</div>" + "</div>";



                                    break;
                                }
                            case 2: {
                                    hint = 110;
                                    // 
                                    // join
                                    // 
                                    Copy = "To post a comment to this blog, complete this form. ";
                                    qs = cp.Utils.ModifyQueryString(qs, "auth", "0");
                                    Copy = Copy + " <a href=\"?" + qs + "\"> Login?</a>";
                                    if (AllowPasswordEmail) {
                                        qs = cp.Utils.ModifyQueryString(qs, "auth", "1");
                                        Copy = Copy + " <a href=\"?" + qs + "\"> Forget your username or password?</a>";
                                    }
                                    result = result + "<div class=\"aoBlogLoginBox\">" + "<div class=\"aoBlogCommentCopy\">" + Copy + "</div>" + "<div class=\"aoBlogCommentCopy\">" + "Send join form removed" + "</div>" + "</div>";



                                    break;
                                }

                            default: {
                                    hint = 120;
                                    // 
                                    // login
                                    // 
                                    Copy = "To post a comment to this Blog, please login.";
                                    if (AllowMemberJoin) {
                                        qs = cp.Utils.ModifyQueryString(qs, "auth", "2");
                                        Copy = Copy + "<div class=\"aoBlogRegisterLink\"><a href=\"?" + qs + "\">Need to Register?</a></div>";
                                    }
                                    result = result + "<div class=\"aoBlogCommentCopy\">" + Copy + "</div>" + "</div>";

                                    break;
                                }
                        }
                        hint = 140;
                    } else {
                        hint = 150;
                        result += "<div>&nbsp;</div>";
                        result += "<div class=\"aoBlogCommentCopy\">Title</div>";
                        if (RetryCommentPost) {
                            hint = 160;
                            result += "<div class=\"aoBlogCommentCopy\">" + _GenericController.getField(cp, constants.RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(constants.RequestNameCommentTitle.ToString())) + "</div>";
                            result += "<div>&nbsp;</div>";
                            result += "<div class=\"aoBlogCommentCopy\">Comment</div>";
                            result += "<div class=\"aoBlogCommentCopy\">" + cp.Html5.InputTextArea(constants.RequestNameCommentCopy, 500, cp.Doc.GetText(constants.RequestNameCommentCopy)) + "</div>";
                        } else {
                            hint = 170;
                            result += "<div class=\"aoBlogCommentCopy\">" + _GenericController.getField(cp, constants.RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(constants.RequestNameCommentTitle.ToString())) + "</div>";
                            result += "<div>&nbsp;</div>";
                            result += "<div class=\"aoBlogCommentCopy\">Comment</div>";
                            result += "<div class=\"aoBlogCommentCopy\">" + cp.Html5.InputTextArea(constants.RequestNameCommentCopy, 500, cp.Doc.GetText(constants.RequestNameCommentCopy)) + "</div>";
                        }
                        // 
                        // todo re-enable recaptcha 20190123
                        hint = 180;
                        if (app.blog.recaptcha) {
                            result += "<div class=\"aoBlogCommentCopy\">Verify Text</div>";
                            result += "<div class=\"aoBlogCommentCopy\">" + cp.Addon.Execute(constants.reCaptchaDisplayGuid) + "</div>";
                        }
                        // 
                        result += "<div class=\"aoBlogCommentCopy\">" + cp.Html.Button(constants.rnButton, constants.FormButtonPostComment) + "&nbsp;" + cp.Html.Button(constants.rnButton, constants.FormButtonCancel) + "</div>";
                    }
                }
                hint = 190;
                // 
                result += "<div class=\"aoBlogCommentDivider\">&nbsp;</div>";
                // 
                // edit link
                // 
                hint = 200;
                if (app?.user != null && app.user.isBlogEditor(cp, app.blog)) {
                    qs = cp.Doc.RefreshQueryString;
                    qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameBlogEntryID, app.blogPost.id);
                    qs = cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogEntryEditor);
                    result += "<div class=\"aoBlogToolLink\"><a href=\"?" + qs + "\">Edit</a></div>";
                }
                // 
                // Search
                // 
                // hint = 210
                // qs = cp.Doc.RefreshQueryString
                // qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch, True)
                // result &= "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                // 
                // back to recent posts
                result += "<div class=\"aoBlogFooterLink\"><a href=\"" + app.blogPageBaseLink + "\">" + constants.BackToRecentPostsMsg + "</a></div>";
                if (!string.IsNullOrEmpty(app.nextArticleLink))
                    result += "<div class=\"aoBlogFooterLink\"><a href=\"" + app.nextArticleLink + "\">" + constants.NextArticlePrefix + app.nextArticleLinkCaption + "</a></div>";
                // 
                result += Constants.vbCrLf + cp.Html5.Hidden(constants.RequestNameSourceFormID, constants.FormBlogPostDetails);
                result += Constants.vbCrLf + cp.Html5.Hidden(constants.RequestNameBlogEntryID, app.blogPost.id);
                result += Constants.vbCrLf + cp.Html5.Hidden("EntryCnt", 1);
                getArticleViewRet = result;
                result = cp.Html.Form(getArticleViewRet);
                // 
                hint = 220;
                cp.Visit.SetProperty(constants.SNBlogCommentName, cp.Utils.GetRandomInteger().ToString());
                // 
                // -- set metadata
                hint = 230;
                MetadataController.setMetadata(cp, app.blogPost);
                // 
                // -- if editing enabled, add the link and wrapperwrapper
                hint = 240;
                result = _GenericController.addEditWrapper(cp, result, app.blogPost.id, app.blogPost.name, BlogEntryModel.tableMetadata.contentName);
                // 
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "Hint [" + hint + "]");
                throw;
            }
        }
        // 
        // ====================================================================================
        // 
        public static int processArticleView(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request, ref bool RetryCommentPost) {
            var result = default(int);
            try {
                var blog = app.blog;
                var blogEntry = app.blogPost;
                var user = app.user;
                // 
                result = request.SourceFormID;
                string SN = cp.Visit.GetText(constants.SNBlogCommentName);
                // 
                if (string.IsNullOrEmpty(SN)) {
                    // 
                    // Process out of order, go to main
                    // 
                    result = constants.FormBlogPostList;
                } else {
                    string Copy;
                    string formKey;
                    int CommentID;
                    if ((request.ButtonValue ?? "") == constants.FormButtonCancel) {
                        // 
                        // Cancel button, go to main
                        // 
                        result = constants.FormBlogPostList;
                    } else if ((request.ButtonValue ?? "") == constants.FormButtonPostComment) {
                        // todo re-enable recaptcha 20190123
                        if (blog.recaptcha) {
                            // 
                            // Process recaptcha
                            // 
                            string optionStr = "Challenge=" + cp.Doc.GetText("recaptcha_challenge_field");
                            optionStr = optionStr + "&Response=" + cp.Doc.GetText("recaptcha_response_field");
                            string captchaResponse = cp.Addon.Execute(constants.reCaptchaProcessGuid);
                            if (!string.IsNullOrEmpty(captchaResponse)) {
                                cp.UserError.Add("The verify text you entered did not match correctly. Please try again.");
                            }
                        }
                        // 
                        // Process comment post
                        // 
                        RetryCommentPost = true;
                        formKey = cp.Doc.GetText("formkey");
                        Copy = cp.Doc.GetText(constants.RequestNameCommentCopy);
                        if (!string.IsNullOrEmpty(Copy)) {
                            var BlogCommentModelList = DbBaseModel.createList<BlogCommentModel>(cp, "(formkey=" + cp.Db.EncodeSQLText(formKey) + ")", "ID");
                            if (BlogCommentModelList.Count != 0) {
                                cp.UserError.Add("<p>This comment has already been accepted.</p>");
                                RetryCommentPost = false;
                            } else {
                                // Dim EntryID = cp.Doc.GetInteger(RequestNameBlogEntryID)
                                // Dim BlogEntry As BlogEntryModel = DbBaseModel.create(Of BlogEntryModel)(cp, EntryID)
                                var BlogComment = DbBaseModel.addDefault<BlogCommentModel>(cp);
                                BlogComment.blogId = blog.id;
                                BlogComment.active = true;
                                BlogComment.name = cp.Doc.GetText(constants.RequestNameCommentTitle);
                                BlogComment.copyText = Copy;
                                BlogComment.entryId = blogEntry.id;
                                BlogComment.approved = user.isBlogEditor(cp, blog) | blog.autoApproveComments;
                                BlogComment.formKey = formKey;
                                BlogComment.save(cp);
                                CommentID = BlogComment.id;
                                RetryCommentPost = false;
                                // 
                                if (blog.emailComment) {
                                    // 
                                    // Send Comment Notification
                                    string EntryLink = blogEntry.rssLink;
                                    if (Strings.InStr(1, EntryLink, "?") == 0) {
                                        EntryLink += "?";
                                    } else {
                                        EntryLink += "&";
                                    }
                                    EntryLink = EntryLink + "blogentryid=" + blogEntry.id;
                                    string EmailBody = "" + "The following blog comment was posted " + Conversions.ToString(DateTime.Now) + "To approve this comment, go to " + EntryLink + Constants.vbCrLf + "Blog '" + blog.name + "'" + "Post '" + blogEntry.name + "'" + "By " + cp.User.Name + Constants.vbCrLf + Constants.vbCrLf + cp.Utils.EncodeHTML(Copy) + Constants.vbCrLf;
                                    string EmailFromAddress = cp.Site.GetText("EmailFromAddress", "info@" + cp.Site.Domain);
                                    if (blogEntry.authorMemberId != 0) {
                                        cp.Email.sendUser(blogEntry.authorMemberId, EmailFromAddress, "Blog comment notification for [" + blog.name + "]", EmailBody, true, false);
                                        cp.Email.sendUser(blogEntry.authorMemberId, EmailFromAddress, "Blog comment notification for [" + blog.name + "]", EmailBody, false, false);
                                    }

                                    // If blog.AuthoringGroupID <> 0 Then
                                    // Dim MemberRuleList As List(Of MemberRuleModel) = DbBaseModel.createList(Of MemberRuleModel)(cp, "GroupId=" & blog.AuthoringGroupID)
                                    // For Each MemberRule In MemberRuleList
                                    // Call cp.Email.sendUser(MemberRule.MemberID.ToString(), EmailFromAddress, "Blog comment on " & blog.name, EmailBody, False, False)
                                    // Next
                                    // End If
                                    int blogAuthorsGroupId = cp.Group.GetId("Blog Authors");
                                    if (blogAuthorsGroupId != 0) {
                                        var MemberRuleList = DbBaseModel.createList<MemberRuleModel>(cp, "GroupId=" + blogAuthorsGroupId);
                                        foreach (var MemberRule in MemberRuleList)
                                            cp.Email.sendUser(MemberRule.memberId, EmailFromAddress, "Blog comment on " + blog.name, EmailBody, false, false);
                                    }
                                }

                            }
                        }
                        result = constants.FormBlogPostDetails;
                    } else if ((request.ButtonValue ?? "") == constants.FormButtonApplyCommentChanges) {
                        // 
                        // Post approval changes if the person is the owner
                        // 
                        if (user.isBlogEditor(cp, blog)) {
                            int EntryCnt = cp.Doc.GetInteger("EntryCnt");
                            if (EntryCnt > 0) {
                                int EntryPtr;
                                var loopTo = EntryCnt - 1;
                                for (EntryPtr = 0; EntryPtr <= loopTo; EntryPtr++) {
                                    int CommentCnt = cp.Doc.GetInteger("CommentCnt" + EntryPtr);
                                    if (CommentCnt > 0) {
                                        int CommentPtr;
                                        var loopTo1 = CommentCnt - 1;
                                        for (CommentPtr = 0; CommentPtr <= loopTo1; CommentPtr++) {
                                            string Suffix = EntryPtr + "." + CommentPtr;
                                            CommentID = cp.Doc.GetInteger("CommentID" + Suffix);
                                            if (cp.Doc.GetBoolean("Delete" + Suffix)) {
                                                // 
                                                // Delete comment
                                                // 
                                                cp.Content.Delete("Blog Comments", "(id=" + CommentID + ")and(BlogID=" + blog.id + ")");
                                            } else if (cp.Doc.GetBoolean("Approve" + Suffix) & !cp.Doc.GetBoolean("Approved" + Suffix)) {
                                                // 
                                                // Approve Comment
                                                // 
                                                var BlogCommentModelList = DbBaseModel.createList<BlogCommentModel>(cp, "(name=" + cp.Utils.EncodeRequestVariable(blog.name) + ")", "ID");
                                                if (BlogCommentModelList.Count > 0) {
                                                    var BlogComment = DbBaseModel.addDefault<BlogCommentModel>(cp);
                                                    if (cp.CSNew().OK()) {
                                                        BlogComment.approved = true;
                                                    }
                                                } else if (!cp.Doc.GetBoolean("Approve" + Suffix) & cp.Doc.GetBoolean("Approved" + Suffix)) {
                                                    // 
                                                    // Unapprove comment
                                                    // 
                                                    var BlogComment = DbBaseModel.addDefault<BlogCommentModel>(cp);
                                                    if (BlogComment is not null) {
                                                        BlogComment.approved = false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // '
                    }
                    cp.Visit.SetProperty(constants.SNBlogCommentName, "");
                }
            }
            // 
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
        // 
    }
}