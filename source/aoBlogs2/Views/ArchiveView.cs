
using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
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
                cp.Doc.AddMetaDescription($"Archives {DateTimeFormatInfo.CurrentInfo.GetMonthName(request.ArchiveMonth)} {request.ArchiveYear}, {MetadataController.getBlogMetaDescription(app, blog)}");
                string title = $"Archives {DateTimeFormatInfo.CurrentInfo.GetMonthName(request.ArchiveMonth)} {request.ArchiveYear}, {MetadataController.getBlogMetaTitle(app, blog)} ";
                MetadataController.addTitle(cp, title);
                //
                result += $"<h1>{title}</h1>";
                var postList = DbBaseModel.createList<BlogEntryModel>(cp, "(Month(COALESCE(datePublished, dateAdded)) = " + request.ArchiveMonth + ")And(year(COALESCE(datePublished, dateAdded))=" + request.ArchiveYear + ")And(BlogID=" + blog.id + ")", "COALESCE(datePublished, dateAdded) Desc");
                if (postList.Count == 0) {
                    // 
                    // -- no results
                    result += $"<div class=\"aoBlogProblem\">There are no blog archives For {request.ArchiveMonth}/{request.ArchiveYear}</div>";
                    //
                    // -- in this case, there may have been a link alias that led here, remove it
                    string linkAliasQs = $"FormID=600&ArchiveMonth={request.ArchiveMonth}&ArchiveYear={request.ArchiveYear}";
                    DbBaseModel.deleteRows<LinkAliasModel>(cp, $"(pageid={cp.Doc.PageId}) and (queryStringSuffix={cp.Db.EncodeSQLText(linkAliasQs)})");
                } else {
                    // 
                    // -- list of articles
                    int EntryPtr = 0;
                    string entryEditLink = "";
                    var Return_CommentCnt = default(int);
                    foreach (var blogPost in postList) {
                        int EntryID = blogPost.id;
                        int AuthorMemberID = blogPost.authorMemberId;
                        if (AuthorMemberID == 0) {
                            AuthorMemberID = cp.Utils.EncodeInteger(blogPost.createdBy);
                        }
                        var dateAdded = blogPost.dateAdded;
                        string EntryName = blogPost.name;
                        if (app.userIsEditing) {
                            entryEditLink = cp.Content.GetEditLink(EntryName, EntryID.ToString(), true, EntryName, true);
                        }
                        string EntryCopy = blogPost.copy;
                        bool allowComments = blogPost.allowComments;
                        string BlogTagList = blogPost.tagList;
                        int primaryImagePositionId = blogPost.primaryImagePositionId;
                        List<BlogImageModel> blogImageList = BlogImageModel.getPostImageList(cp, blogPost);
                        result += BlogEntryCellView.getBlogPostCell(cp, app, blogPost, blogImageList, false, false, Return_CommentCnt, entryEditLink);
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