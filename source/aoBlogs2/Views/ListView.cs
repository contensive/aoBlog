﻿using System;
using System.Collections.Generic;
using System.Text;
using Contensive.Addons.Blog.Controllers;
using Contensive.Addons.Blog.Models;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;

namespace Contensive.Addons.Blog.Views {
    // 
    public class ListView {
        // 
        // ====================================================================================
        // 
        public static string getListView(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request) {
            var result = new StringBuilder();
            try {
                var blog = app.blog;
                var user = app.user;
                var rssFeed = app.rssFeed;

                string NoneMsg = "There are no articles available";
                string criteria = "(BlogID=" + blog.id + ")";
                var blogCategory = request.categoryId.Equals(0) ? null : DbModel.create<BlogCategoriesModel>(cp, request.categoryId);
                if (blogCategory is not null) {
                    criteria += "and(BlogCategoryID=" + request.categoryId + ")";
                    NoneMsg = "There are no articles available in the category " + blogCategory.name;
                }

                int recordCount = DbModel.getCount<BlogPostModel>(cp, criteria);
                int pageNumber = request.page;
                if (pageNumber > recordCount)
                    pageNumber = recordCount;
                if (pageNumber < 1)
                    pageNumber = 1;
                int pageCount = (int)Math.Round(Math.Truncate(recordCount / (double)blog.postsToDisplay + 0.999d));
                string paginationSuffix = ", " + "page " + pageNumber + " of " + pageCount + "";
                // 
                if (!string.IsNullOrEmpty(blog.Caption)) {
                    result.Append(Constants.vbCrLf + "<h1 Class=\"aoBlogCaption\">" + blog.Caption + (pageNumber == 1 ? "" : paginationSuffix) + "</h1>");
                }
                if (!string.IsNullOrEmpty(blog.Copy)) {
                    result.Append(Constants.vbCrLf + "<div Class=\"aoBlogDescription\">" + blog.Copy + "</div>");
                }

                if (blogCategory is not null) {
                    result.Append("<div Class=\"aoBlogCategoryCaption\">Category " + blogCategory.name + "</div>");
                }
                // 
                // Display the most recent entries
                var postList = DbModel.createList<BlogPostModel>(cp, criteria, "DateAdded Desc", blog.postsToDisplay, pageNumber);
                var isBlogCategoryBlockedDict = new Dictionary<int, bool>();
                int recordsOnThisPage = 0;
                if (postList.Count.Equals(0)) {
                    // 
                    result.Append("<div Class=\"aoBlogProblem\">" + NoneMsg + "</div>");
                }
                else {
                    var blogCategoryList = DbModel.createList<BlogCategoriesModel>(cp, "");
                    var Return_CommentCnt = default(int);
                    foreach (BlogPostModel blogEntry in postList) {
                        if (recordsOnThisPage >= blog.postsToDisplay)
                            break;
                        bool IsBlocked = false;
                        if (isBlogCategoryBlockedDict.ContainsKey(blogEntry.blogCategoryID)) {
                            IsBlocked = isBlogCategoryBlockedDict[blogEntry.blogCategoryID];
                        }
                        else {
                            var blogEntryCategory = DbModel.create<BlogCategoriesModel>(cp, blogEntry.blogCategoryID);
                            if (blogEntryCategory is not null) {
                                if (blogEntryCategory.UserBlocking)
                                    IsBlocked = !genericController.isGroupListMember(cp, GroupModel.GetBlockingGroups(cp, blogEntryCategory.id));
                                isBlogCategoryBlockedDict.Add(blogEntry.blogCategoryID, IsBlocked);
                            }
                        }
                        if (!IsBlocked) {
                            string blogArticleCell = BlogEntryCellView.getBlogPostCell(cp, app, blogEntry, false, true, Return_CommentCnt, "");
                            // 
                            // -- if editing enabled, add the link and wrapperwrapper
                            blogArticleCell = genericController.addEditWrapper(cp, blogArticleCell, blogEntry.id, blogEntry.name, BlogPostModel.contentName);

                            result.Append(blogArticleCell);
                            result.Append("<hr>");
                        }
                        recordsOnThisPage += 1;
                    }
                }
                // 
                // -- pagination
                result.Append(PaginationController.getRecordPagination(cp, blog.postsToDisplay, pageNumber, recordsOnThisPage, recordCount));
                string CategoryFooter = "";
                string qs;
                // 
                // Build Footers
                if (cp.User.IsAdmin & blog.AllowCategories) {
                    qs = "cid=" + cp.Content.GetID("Blog Categories") + "&af=4";
                    CategoryFooter = CategoryFooter + "<div Class=\"aoBlogFooterLink\"><a href=\"" + cp.Site.GetText("ADMINURL") + "?" + qs + "\">Add a New category</a></div>";
                }
                string ReturnFooter = "";
                if (blog.AllowCategories) {
                    qs = cp.Doc.RefreshQueryString;
                    qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameBlogCategoryIDSet, "0", true);
                    CategoryFooter = CategoryFooter + "<div class=\"aoBlogFooterLink\"><a href=\"" + app.blogPageBaseLink + "\">See Posts in All Categories</a></div>";
                    // 
                    // select a category
                    qs = cp.Doc.RefreshQueryString;
                    var BlogCategoryList = DbModel.createList<BlogCategoriesModel>(cp, "");
                    if (BlogCategoryList.Count > 0) {
                        foreach (var blogCategaory in BlogCategoryList) {
                            bool IsBlocked = false;
                            if (isBlogCategoryBlockedDict.ContainsKey(blogCategaory.id)) {
                                IsBlocked = isBlogCategoryBlockedDict[blogCategaory.id];
                            }
                            else {
                                var blogEntryCategory = DbModel.create<BlogCategoriesModel>(cp, blogCategaory.id);
                                if (blogEntryCategory is not null) {
                                    if (blogEntryCategory.UserBlocking)
                                        IsBlocked = !genericController.isGroupListMember(cp, GroupModel.GetBlockingGroups(cp, blogEntryCategory.id));
                                    isBlogCategoryBlockedDict.Add(blogCategaory.id, IsBlocked);
                                }
                            }
                            if (!IsBlocked) {
                                string categoryLink = cp.Utils.ModifyQueryString(qs, constants.RequestNameBlogCategoryIDSet, blogCategaory.id.ToString(), true);
                                CategoryFooter = CategoryFooter + "<div class=\"aoBlogFooterLink\"><a href=\"?" + categoryLink + "\"> See Posts in All Category " + blogCategaory.name + "</a></div>";
                            }
                        }
                    }
                }
                // 
                // Footer
                result.Append("<div>&nbsp;</div>");
                if (user.isBlogEditor(cp, blog)) {
                    // 
                    // Create a new entry if this is the Blog Owner
                    // 
                    qs = cp.Doc.RefreshQueryString;
                    qs = cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogEntryEditor.ToString(), true);
                    result.Append("<div class=\"aoBlogFooterLink\"><a href=\"?" + qs + "\">Create New Blog Post</a></div>");
                    // 
                    // Create a link to edit the blog record
                    // 
                    qs = "cid=" + cp.Content.GetID("Blogs") + "&af=4&id=" + blog.id;
                    result.Append("<div class=\"aoBlogFooterLink\"><a href=\"" + cp.Site.GetText("adminUrl") + "?" + qs + "\">Blog Settings</a></div>");
                    // 
                    // Create a link to edit the rss record
                    // 
                    if (rssFeed.id == 0) {
                    }

                    else {
                        qs = "cid=" + cp.Content.GetID("RSS Feeds") + "&af=4&id=" + rssFeed.id;
                        result.Append("<div class=\"aoBlogFooterLink\"><a href=\"" + cp.Site.GetText("adminUrl") + "?" + qs + "\">Edit RSS Features</a></div>");
                    }
                }
                result.Append(ReturnFooter);
                result.Append(CategoryFooter);
                // '
                // ' Search
                // '
                // qs = cp.Doc.RefreshQueryString
                // qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch.ToString(), True)
                // result.Append("<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>")
                // 
                // Link to RSS Feed
                // 
                string FeedFooter = "";
                if (!string.IsNullOrEmpty(rssFeed.rssFilename)) {
                    FeedFooter = "RSS: " + "<a href=\"" + cp.Http.CdnFilePathPrefix + rssFeed.rssFilename + "\">" + rssFeed.name + "</a>" + "&nbsp;" + FeedFooter + constants.iconRSS_16x16 + "";




                    result.Append("<div class=\"aoBlogFooterLink\">" + FeedFooter + "</div>");
                }
                result.Append(cp.Html.Hidden(constants.RequestNameSourceFormID, constants.FormBlogPostList.ToString()));
                // 
                // -- meta data
                if (pageNumber > 1) {
                    // 
                    // -- set the page title if it is page 2 or more 
                    cp.Doc.AddTitle(blog.metaTitle + paginationSuffix);
                    cp.Doc.AddMetaDescription(blog.metaDescription + paginationSuffix);
                }
                else {
                    cp.Doc.AddMetaDescription(blog.metaDescription);
                    cp.Doc.AddTitle(blog.metaTitle);
                }
                cp.Doc.AddMetaKeywordList(blog.metaKeywordList);
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            // 
            return result.ToString();
        }
    }
}