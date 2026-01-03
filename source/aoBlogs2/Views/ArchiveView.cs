
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using System;
using System.Globalization;
using System.Linq;

namespace Contensive.Blog.Views {
    public class ArchiveView {
        // 
        // 
        // ====================================================================================
        // 
        public static string getFormBlogArchiveDateList(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request) {
            // 
            string result = "";
            try {

                var blog = app.blog;
                var blogEntry = app.blogPost;
                string contentControlId = cp.Content.GetID(constants.cnBlogEntries).ToString();
                // 
                result = Constants.vbCrLf + cp.Content.GetCopy("Blogs Archives Header for " + blog.name, "<h1>" + blog.name + " Archives</h1>");
                // 
                var archiveDateList = BlogEntryModel.createArchiveListFromBlogCopy(cp, blog.id);
                if (archiveDateList.Count == 0) {
                    // 
                    // No archives, give them an error
                    result += "<div class=\"aoBlogProblem\">There are no current blog entries</div>";
                    result += "<div class=\"aoBlogFooterLink\"><a href=\"" + app.blogPageBaseLink + "\">" + constants.BackToRecentPostsMsg + "</a></div>";
                } else {
                    int ArchiveMonth;
                    int ArchiveYear;
                    if (archiveDateList.Count == 1) {
                        // 
                        // one archive - just display it
                        ArchiveMonth = archiveDateList.First().Month;
                        ArchiveYear = archiveDateList.First().Year;
                        result += getFormBlogArchivedBlogs(cp, app, request);
                    } else {
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
        public static string getFormBlogArchivedBlogs(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request) {
            string getFormBlogArchivedBlogsRet;
            //
            string result = "";
            try {
                var blog = app.blog;
                // 
                // List Blog Entries
                // 
                var postList = DbBaseModel.createList<BlogEntryModel>(cp, "(Month(COALESCE(datePublished, dateAdded)) = " + request.ArchiveMonth + ")And(year(COALESCE(datePublished, dateAdded))=" + request.ArchiveYear + ")And(BlogID=" + blog.id + ")", "COALESCE(datePublished, dateAdded) Desc");
                if (postList.Count == 0) {
                    // 
                    // -- no results
                    string title = $"{app.blog.name} Archives {DateTimeFormatInfo.CurrentInfo.GetMonthName(request.ArchiveMonth)} {request.ArchiveYear}";
                    cp.Doc.AddTitle(title);
                    result += $"<h1>{title}</h1>";
                    result += $"<div class=\"aoBlogProblem\">There are no blog archives For {request.ArchiveMonth}/{request.ArchiveYear}</div>";
                } else if (postList.Count == 1) {
                    //
                    // -- show 1 resulting article
                    var app2 = new ApplicationEnvironmentModel(cp, blog, postList[0].id);
                    result = ArticleView.getArticleView(cp, app2, false);
                } else {
                    // 
                    // -- list of articles
                    string title = $"{app.blog.name} Archives {DateTimeFormatInfo.CurrentInfo.GetMonthName(request.ArchiveMonth)} {request.ArchiveYear}";
                    cp.Doc.AddTitle(title);
                    result += $"<h1>{title}</h1>";
                    int EntryPtr = 0;
                    string entryEditLink = "";
                    var Return_CommentCnt = default(int);
                    foreach (var post in postList) {
                        int EntryID = post.id;
                        int AuthorMemberID = post.authorMemberId;
                        if (AuthorMemberID == 0) {
                            AuthorMemberID = cp.Utils.EncodeInteger(post.createdBy);
                        }
                        var dateAdded = post.dateAdded;
                        string EntryName = post.name;
                        if (app.userIsEditing) {
                            entryEditLink = cp.Content.GetEditLink(EntryName, EntryID.ToString(), true, EntryName, true);
                        }
                        string EntryCopy = post.copy;
                        bool allowComments = post.allowComments;
                        string BlogTagList = post.tagList;
                        int primaryImagePositionId = post.primaryImagePositionId;
                        result += BlogEntryCellView.getBlogPostCell(cp, app, post, false, false, Return_CommentCnt, BlogTagList);
                        result += "<hr>";
                    }
                    EntryPtr++;

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
                getFormBlogArchivedBlogsRet = result;
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