
using System;
using System.Linq;
using Contensive.Addons.Blog.Models;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;

namespace Contensive.Addons.Blog.Views {
    public class ArchiveView {
        // 
        // 
        // ====================================================================================
        // 
        public static string GetFormBlogArchiveDateList(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request) {
            // 
            string result = "";
            try {

                var blog = app.blog;
                var blogEntry = app.blogEntry;
                string OpenSQL = "";
                string contentControlId = cp.Content.GetID(constants.cnBlogEntries).ToString();
                // 
                result = Constants.vbCrLf + cp.Content.GetCopy("Blogs Archives Header for " + blog.name, "<h1>" + blog.name + " Archives</h1>");
                // 
                var archiveDateList = BlogCopyModel.createArchiveListFromBlogCopy(cp, blog.id);
                if (archiveDateList.Count == 0) {
                    // 
                    // No archives, give them an error
                    result += "<div class=\"aoBlogProblem\">There are no current blog entries</div>";
                    result += "<div class=\"aoBlogFooterLink\"><a href=\"" + app.blogPageBaseLink + "\">" + constants.BackToRecentPostsMsg + "</a></div>";
                }
                else {
                    int ArchiveMonth;
                    int ArchiveYear;
                    if (archiveDateList.Count == 1) {
                        // 
                        // one archive - just display it
                        ArchiveMonth = archiveDateList.First().Month;
                        ArchiveYear = archiveDateList.First().Year;
                        result += GetFormBlogArchivedBlogs(cp, app, request);
                    }
                    else {
                        // 
                        // Display List of archive
                        string qs = cp.Utils.ModifyQueryString(app.blogBaseLink, constants.RequestNameSourceFormID, constants.FormBlogArchiveDateList.ToString());
                        qs = cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogArchivedBlogs.ToString());
                        foreach (var archiveDate in archiveDateList) {
                            ArchiveMonth = archiveDate.Month;
                            ArchiveYear = archiveDate.Year;
                            string NameOfMonth = DateAndTime.MonthName(ArchiveMonth);
                            qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameArchiveMonth, ArchiveMonth.ToString());
                            qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameArchiveYear, ArchiveYear.ToString());
                            result += Constants.vbCrLf + Constants.vbTab + Constants.vbTab + "<div class=\"aoBlogArchiveLink\"><a href=\"" + qs + "\">" + NameOfMonth + " " + ArchiveYear + "</a></div>";
                        }
                        result += Constants.vbCrLf + Constants.vbTab + "<div class=\"aoBlogFooterLink\"><a href=\"" + app.blogPageBaseLink + "\">" + constants.BackToRecentPostsMsg + "</a></div>";
                    }
                }
            }
            // 

            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
        // 
        // ====================================================================================
        // 
        public static string GetFormBlogArchivedBlogs(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request) {
            string GetFormBlogArchivedBlogsRet = default;
            // 
            string result = "";
            try {
                var blog = app.blog;
                int PageNumber;
                // 
                // If it is the current month, start at entry 6
                // 
                PageNumber = 1;
                // 
                // List Blog Entries
                // 
                var BlogEntryModelList = DbModel.createList<BlogPostModel>(cp, "(Month(DateAdded) = " + request.ArchiveMonth + ")And(year(DateAdded)=" + request.ArchiveYear + ")And(BlogID=" + blog.id + ")", "DateAdded Desc");
                if (BlogEntryModelList.Count == 0) {
                    result = "<div Class=\"aoBlogProblem\">There are no blog archives For " + request.ArchiveMonth + "/" + request.ArchiveYear + "</div>";
                }
                else {
                    int EntryPtr = 0;
                    string entryEditLink = "";
                    var Return_CommentCnt = default(int);
                    foreach (var blogEntry in BlogEntryModelList) {
                        int EntryID = blogEntry.id;
                        int AuthorMemberID = blogEntry.AuthorMemberID;
                        if (AuthorMemberID == 0) {
                            AuthorMemberID = blogEntry.CreatedBy;
                        }
                        var DateAdded = blogEntry.DateAdded;
                        string EntryName = blogEntry.name;
                        if (app.userIsEditing) {
                            entryEditLink = cp.Content.GetEditLink(EntryName, EntryID.ToString(), true, EntryName, true);
                        }
                        string EntryCopy = blogEntry.copy;
                        bool allowComments = blogEntry.AllowComments;
                        string BlogTagList = blogEntry.tagList;
                        int primaryImagePositionId = blogEntry.primaryImagePositionId;
                        result += BlogEntryCellView.getBlogPostCell(cp, app, blogEntry, false, false, Return_CommentCnt, BlogTagList);
                    }
                    result += "<hr>";
                    EntryPtr = EntryPtr + 1;

                }
                // 
                // 
                string qs = app.blogBaseLink;
                // qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch.ToString())
                result += "<div>&nbsp;</div>";
                // result &= "<div Class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                qs = cp.Doc.RefreshQueryString;
                qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameBlogEntryID, "", true);
                qs = cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogPostList.ToString());
                result += "<div Class=\"aoBlogFooterLink\"><a href=\"" + app.blogPageBaseLink + "\">" + constants.BackToRecentPostsMsg + "</a></div>";
                cp.CSNew().Close();
                result += cp.Html.Hidden(constants.RequestNameSourceFormID, constants.FormBlogArchivedBlogs.ToString());
                result = cp.Html.Form(result);
                // 
                GetFormBlogArchivedBlogsRet = result;
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