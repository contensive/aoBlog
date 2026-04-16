
using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Views;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contensive.Blog.Models.View {
    public class BlogSearchViewModel {
        //
        // -- header
        public string blogName { get; set; }
        //
        // -- search results
        public bool hasSearchResults { get; set; }
        public string subcaption { get; set; }
        public bool hasResults { get; set; }
        public string searchResultsHtml { get; set; }
        public string noResultsMessage { get; set; }
        //
        // -- search form (built in C#, contains form controls)
        public string searchFormHtml { get; set; }
        //
        // -- hidden fields
        public string hiddenFieldsHtml { get; set; }
        //
        // -- back link
        public string backToRecentLink { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Create the BlogSearchViewModel. Extracts rendering logic from SearchView.GetFormBlogSearch().
        /// The caller must wrap the rendered result in cp.Html.Form().
        /// </summary>
        public static BlogSearchViewModel create(CPBaseClass cp, ApplicationEnvironmentModel app, RequestModel request) {
            try {
                var result = new BlogSearchViewModel();
                var blog = app.blog;
                result.blogName = blog.name;
                result.backToRecentLink = app.blogPageBaseLink;
                //
                cp.Doc.AddRefreshQueryString(constants.RequestNameBlogEntryID, "");
                //
                // -- search results
                string queryTag = cp.Doc.GetText(constants.rnQueryTag);
                string button = cp.Doc.GetText("button");
                if ((button ?? "") == constants.FormButtonSearch || !string.IsNullOrEmpty(queryTag)) {
                    result.hasSearchResults = true;
                    string pageTitle = "";
                    string subcaption = "";
                    var sqlCriteria = new StringBuilder($"(blogid={blog.id})");
                    //
                    // -- keyword list
                    var subCriteria = new StringBuilder();
                    if (!string.IsNullOrEmpty(request.KeywordList)) {
                        string[] keyWordsArray = Strings.Split("," + request.KeywordList + ",", ",", Compare: Constants.vbTextCompare);
                        foreach (var keyword in keyWordsArray) {
                            if (!string.IsNullOrWhiteSpace(keyword)) {
                                pageTitle += (string.IsNullOrEmpty(pageTitle) ? "" : " or ") + cp.Utils.EncodeHTML(keyword);
                                subcaption += (string.IsNullOrEmpty(subcaption) ? "" : " or ") + $"'<i>{cp.Utils.EncodeHTML(keyword)}</i>'";
                                subCriteria.Append($"or(Copy like {cp.Db.EncodeSQLTextLike(keyword)})or(name like {cp.Db.EncodeSQLTextLike(keyword)})");
                            }
                        }
                        if (!string.IsNullOrEmpty(subcaption)) { subcaption = $" containing {subcaption}"; }
                        if (subCriteria.Length > 0) { sqlCriteria.Append($"and({subCriteria.ToString().Substring(2)})"); }
                    }
                    if (!string.IsNullOrEmpty(queryTag)) {
                        pageTitle += (string.IsNullOrEmpty(pageTitle) ? "" : " and ") + cp.Utils.EncodeHTML(queryTag);
                        subcaption += $" tagged with '<i>{cp.Utils.EncodeHTML(queryTag)}</i>'";
                        sqlCriteria.Append($"and(tagList like {cp.Db.EncodeSQLTextLike(queryTag)})");
                    }
                    if (!string.IsNullOrEmpty(subcaption)) { subcaption = $"Search for posts {subcaption}"; }
                    if (string.IsNullOrEmpty(pageTitle)) {
                        pageTitle = "Article Search";
                    } else {
                        pageTitle = $"Articles about {pageTitle}";
                    }
                    //
                    MetadataController.addTitle(cp, pageTitle);
                    cp.Doc.AddMetaDescription(pageTitle);
                    //
                    if (!cp.UserError.OK()) {
                        subcaption += cp.Html.ul(cp.UserError.GetList());
                    }
                    result.subcaption = subcaption;
                    //
                    var blogEntryList = DbBaseModel.createList<BlogEntryModel>(cp, sqlCriteria.ToString());
                    if (blogEntryList.Count == 0) {
                        result.noResultsMessage = "There were no matches to your search";
                    } else {
                        result.hasResults = true;
                        string searchResultsHtml = "<hr>";
                        foreach (var blogEntry in blogEntryList) {
                            if (blogEntry.authorMemberId == 0) {
                                blogEntry.authorMemberId = cp.Utils.EncodeInteger(blogEntry.createdBy);
                            }
                            List<BlogImageModel> blogImageList = BlogImageModel.getPostImageList(cp, blogEntry);
                            var postCellVm = BlogPostCellViewModel.create(cp, app, blogEntry, blogImageList, false, true, "");
                            searchResultsHtml += cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogPostCell, constants.layoutNameBlogPostCell, constants.layoutPathFilenameBlogPostCell), postCellVm);
                            searchResultsHtml += "<hr>";
                        }
                        result.searchResultsHtml = searchResultsHtml;
                    }
                }
                //
                // -- search form
                result.searchFormHtml = $"<div class=\"aoBlogSearchFormCon\"><table width=100% border=0 cellspacing=0 cellpadding=5 class=\"aoBlogSearchTable\">{_GenericController.getFormTableRow(cp, "Keyword(s):", _GenericController.getField(cp, constants.RequestNameKeywordList, 1, 10, 30, cp.Doc.GetText(constants.RequestNameKeywordList)))}{_GenericController.getFormTableRow(cp, "", cp.Html.Button(constants.rnButton, constants.FormButtonSearch))}{_GenericController.getFormTableRow2(cp, $"<div class=\"aoBlogFooterLink\"><a href=\"{app.blogPageBaseLink}\">{constants.BackToRecentPostsMsg}</a></div>")}</table></div>";
                //
                // -- hidden fields
                result.hiddenFieldsHtml = $"<input type=\"hidden\" name=\"{constants.RequestNameSourceFormID}\" value=\"{constants.FormBlogSearch}\">"
                    + $"<input type=\"hidden\" name=\"{constants.rnFormID}\" value=\"{constants.FormBlogSearch}\">";
                //
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "BlogSearchViewModel.create");
                return new BlogSearchViewModel();
            }
        }
    }
}
