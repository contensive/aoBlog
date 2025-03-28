using System;
using System.Text;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;
using Contensive.Models.Db;

namespace Contensive.Blog.Views {
    // 
    public class SearchView {
        // 
        // ====================================================================================
        // 
        public static string GetFormBlogSearch(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request) {
            var result = new StringBuilder();
            try {
                var blog = app.blog;
                cp.Doc.AddRefreshQueryString(constants.RequestNameBlogEntryID, "");
                // 
                result.Append("<h1>" + blog.name + " Search</h1>");
                // 
                // Search results
                // 
                string QueryTag = cp.Doc.GetText(constants.rnQueryTag);
                string Button = cp.Doc.GetText("button");
                if ((Button ?? "") == constants.FormButtonSearch | !string.IsNullOrEmpty(QueryTag)) {
                    string pageTitle = "";
                    string subcaption = "";
                    var sqlCriteria = new StringBuilder("(blogid=" + blog.id + ")");
                    // 
                    // -- Keyword list
                    var subCriteria = new StringBuilder();
                    if (!string.IsNullOrEmpty(request.KeywordList)) {
                        string[] KeyWordsArray = Strings.Split("," + request.KeywordList + ",", ",", Compare: Constants.vbTextCompare);
                        foreach (var keyword in KeyWordsArray) {
                            if (!string.IsNullOrWhiteSpace(keyword)) {
                                pageTitle = (string.IsNullOrEmpty(pageTitle) ? "" : " or ") + cp.Utils.EncodeHTML(keyword);
                                subcaption = (string.IsNullOrEmpty(subcaption) ? "" : " or ") + "'<i>" + cp.Utils.EncodeHTML(keyword) + "</i>'";
                                subCriteria.Append($"or(Copy like {cp.Db.EncodeSQLTextLike(keyword)})or(name like {cp.Db.EncodeSQLTextLike(keyword)})");
                            }
                        }
                        if (!string.IsNullOrEmpty(subcaption))
                            subcaption = $" containing {subcaption}";
                        if (subCriteria.Length > 0)
                            sqlCriteria.Append("and(" + subCriteria.ToString().Substring(2) + ")");
                    }
                    if (!string.IsNullOrEmpty(QueryTag)) {
                        pageTitle = (string.IsNullOrEmpty(pageTitle) ? "" : " and ") + cp.Utils.EncodeHTML(QueryTag);
                        subcaption += " tagged with '<i>" + cp.Utils.EncodeHTML(QueryTag) + "</i>'";
                        sqlCriteria.Append($"and(tagList like {cp.Db.EncodeSQLTextLike(QueryTag)})");
                    }
                    if (!string.IsNullOrEmpty(subcaption)) {
                        subcaption = "Search for posts " + subcaption;
                    }
                    if (string.IsNullOrEmpty(pageTitle)) {
                        // 
                        // -- empty search page
                        pageTitle = "Article Search";
                    } else {
                        // 
                        // -- search results
                        pageTitle = "Articles about " + pageTitle;
                    }
                    // 
                    // -- add title and meta description to make this search page unique
                    cp.Doc.AddTitle(pageTitle);
                    cp.Doc.AddMetaDescription(pageTitle);
                    // 
                    // Display the results
                    if (!cp.UserError.OK()) {
                        subcaption += cp.Html.ul(cp.UserError.GetList());
                    }
                    result.Append("<h4 class=\"aoBlogEntryCopy\">" + subcaption + "</h4>");
                    // 
                    var BlogEntryList = DbBaseModel.createList<BlogEntryModel>(cp, sqlCriteria.ToString());
                    if (BlogEntryList.Count == 0) {
                        result.Append("</br>" + "<div class=\"aoBlogProblem\">There were no matches to your search</div>");
                    } else {
                        result.Append("</br>" + "<hr>");
                        var Return_CommentCnt = default(int);
                        foreach (var blogEntry in BlogEntryList) {
                            int AuthorMemberID = blogEntry.authorMemberId;
                            if (AuthorMemberID == 0)
                                AuthorMemberID = cp.Utils.EncodeInteger(blogEntry.createdBy);
                            result.Append(BlogEntryCellView.getBlogPostCell(cp, app, blogEntry, false, true, Return_CommentCnt, ""));
                            result.Append("<hr>");
                        }
                    }
                    result = new StringBuilder(cp.Html.div(result.ToString(), "", "aoBlogSearchResultsCon"));
                }
                // 
                result.Append("" + "<div  class=\"aoBlogSearchFormCon\">" + "<table width=100% border=0 cellspacing=0 cellpadding=5 class=\"aoBlogSearchTable\">" + _GenericController.getFormTableRow(cp, "Keyword(s):", _GenericController.getField(cp, constants.RequestNameKeywordList, 1, 10, 30, cp.Doc.GetText(constants.RequestNameKeywordList))) + _GenericController.getFormTableRow(cp, "", cp.Html.Button(constants.rnButton, constants.FormButtonSearch)) + _GenericController.getFormTableRow2(cp, "<div class=\"aoBlogFooterLink\"><a href=\"" + app.blogPageBaseLink + "\">" + constants.BackToRecentPostsMsg + "</a></div>") + "</table>" + "</div>");






                result.Append("<input type=\"hidden\" name=\"" + constants.RequestNameSourceFormID + "\" value=\"" + constants.FormBlogSearch.ToString() + "\">");
                result.Append("<input type=\"hidden\" name=\"" + constants.rnFormID + "\" value=\"" + constants.FormBlogSearch.ToString() + "\">");
                return cp.Html.Form(result.ToString());
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
    }
}