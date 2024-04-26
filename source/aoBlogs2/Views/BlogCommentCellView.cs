using System;
using Contensive.Addons.Blog.Models;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Contensive.Addons.Blog.Views {
    // 
    public sealed class BlogCommentCellView {
        // 
        // ====================================================================================
        // 
        public static string getBlogCommentCell(CPBaseClass cp, BlogModel blog, BlogPostModel blogEntry, BlogCommentModel blogComment, PersonModel user, bool IsSearchListing) {
            try {
                string result = "";
                // 
                if (IsSearchListing) {
                    string qs = cp.Doc.RefreshQueryString;
                    qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameBlogEntryID, blogEntry.id.ToString());
                    qs = cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogPostList.ToString());
                    result += "<div class=\"aoBlogEntryName\">Comment to Blog Post " + blogEntry.name + ", <a href=\"?" + qs + "\">View this post</a></div>";
                    result += "<div class=\"aoBlogCommentDivider\">&nbsp;</div>";
                }
                result += "<div class=\"aoBlogCommentName\">" + cp.Utils.EncodeHTML(blogComment.name) + "</div>";
                string Copy = blogComment.CopyText;
                Copy = cp.Utils.EncodeHTML(Copy);
                Copy = Strings.Replace(Copy, Constants.vbCrLf, "<BR />");
                result += "<div class=\"aoBlogCommentCopy\">" + Copy + "</div>";
                string rowCopy = "";
                if (true) {
                    var author = DbModel.create<PersonModel>(cp, blogEntry.AuthorMemberID);
                    if (author is not null && !string.IsNullOrEmpty(author.name)) {
                        rowCopy += "by " + cp.Utils.EncodeHTML(author.name);
                        if (blogComment.DateAdded != DateTime.MinValue) {
                            rowCopy += " | " + Conversions.ToString(blogComment.DateAdded);
                        }
                    }
                    else if (blogComment.DateAdded != DateTime.MinValue) {
                        rowCopy += Conversions.ToString(blogComment.DateAdded);
                    }
                }
                // 
                if (user.isBlogEditor(cp, blog)) {
                    // 
                    // Blog owner Approval checkbox
                    // 
                    if (!string.IsNullOrEmpty(rowCopy)) {
                        rowCopy += " | ";
                    }
                    rowCopy = rowCopy + cp.Html.Hidden("CommentID", blogComment.id.ToString()) + cp.Html.CheckBox("Approve", blogComment.Approved) + cp.Html.Hidden("Approved", blogComment.Approved.ToString()) + "&nbsp;Approved&nbsp;" + " | " + cp.Html.CheckBox("Delete", false) + "&nbsp;Delete" + "";







                }
                if (!string.IsNullOrEmpty(rowCopy)) {
                    result += "<div class=\"aoBlogCommentByLine\">Posted " + rowCopy + "</div>";
                }
                // 
                if (!blogComment.Approved & !user.isBlogEditor(cp, blog) & blogComment.AuthorMemberID == cp.User.Id) {
                    result = "<div style=\"border:1px solid red;padding-top:10px;padding-bottom:10px;\"><span class=\"aoBlogCommentName\" style=\"color:red;\">Your comment pending approval</span><br />" + result + "</div>";
                }
                // 
                return result;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}