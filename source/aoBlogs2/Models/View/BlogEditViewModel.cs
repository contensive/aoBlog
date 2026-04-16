
using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models.Db;
using Contensive.Models.Db;
using Microsoft.VisualBasic;
using System.Linq;
using System.Text;
using System;

namespace Contensive.Blog.Models.View {
    public class BlogEditViewModel {
        //
        // -- the edit form content (WYSIWYG, file uploads, buttons — all built in C#)
        public string editFormHtml { get; set; }
        //
        // -- back link
        public string backToRecentLink { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Create the BlogEditViewModel. Extracts rendering logic from EditView.getFormBlogEdit().
        /// The caller must wrap the rendered result in cp.Html.Form().
        /// </summary>
        public static BlogEditViewModel create(CPBaseClass cp, ApplicationEnvironmentModel app, RequestModel request) {
            try {
                var result = new BlogEditViewModel();
                result.backToRecentLink = app.blogPageBaseLink;
                //
                var formHtml = new StringBuilder();
                formHtml.Append("<table width=100% border=0 cellspacing=0 cellpadding=5 class=\"aoBlogPostTable\">");
                //
                // -- create properties that can be either from the model, or the request
                int blogEntry_id = 0;
                string blogEntry_copy = "";
                string blogEntry_tagList = "";
                string blogEntry_name = "";
                int blogEntry_blogCategoryId = 0;
                if (app.blogPost is null) {
                    formHtml.Append("<h2>Create a new blog post</h2>");
                    if ((request.ButtonValue ?? "") == constants.FormButtonPost) {
                        blogEntry_name = request.BlogEntryName;
                        blogEntry_copy = request.BlogEntryCopy;
                        blogEntry_blogCategoryId = request.BlogEntryCategoryId;
                        blogEntry_tagList = request.BlogEntryTagList;
                    }
                } else {
                    blogEntry_id = app.blogPost.id;
                    blogEntry_name = app.blogPost.name;
                    blogEntry_blogCategoryId = app.blogPost.blogCategoryId;
                    blogEntry_copy = app.blogPost.copy;
                    if (string.IsNullOrEmpty(blogEntry_copy)) {
                        blogEntry_copy = "<!-- cc --><p><br></p><!-- /cc -->";
                    }
                    blogEntry_tagList = app.blogPost.tagList;
                }
                //
                // -- title
                formHtml.Append(_GenericController.getFormTableRow(cp, "<div style=\"padding-top:3px\">Title: </div>", cp.Html.InputText(constants.RequestNameBlogEntryName, blogEntry_name, 255, "form-control")));
                //
                // -- WYSIWYG editor
                formHtml.Append(_GenericController.getFormTableRow(cp, "<div style=\"padding-top:108px\">Post: </div>", cp.Html.InputWysiwyg(constants.RequestNameBlogEntryCopy, blogEntry_copy, CPHtmlBaseClass.EditorUserScope.ContentManager)));
                //
                // -- tags
                if (app.sitePropertyAllowTags) {
                    formHtml.Append(_GenericController.getFormTableRow(cp, "<div style=\"padding-top:3px\">Tag List: </div>", cp.Html.InputText(constants.RequestNameBlogEntryTagList, blogEntry_tagList, 255, "form-control")));
                }
                //
                // -- categories
                if (app.blog.allowCategories) {
                    string categorySelect = cp.Html.SelectContent(constants.RequestNameBlogEntryCategoryID, blogEntry_blogCategoryId.ToString(), "Blog Categories");
                    if (Strings.InStr(1, categorySelect, "<option value=\"\"></option></select>", Constants.vbTextCompare) != 0) {
                        categorySelect = "<div>This blog has no categories defined</div>";
                    }
                    formHtml.Append(_GenericController.getFormTableRow(cp, "Category: ", categorySelect));
                }
                //
                // -- image upload form
                string imageForm = "<TABLE id=\"UploadInsert\" border=\"0\" cellpadding=\"0\" cellspacing=\"1\" width=\"100%\" class=\"aoBlogImageTable\"><tr>";
                var blogImageList = app.blogPost is null ? [] : BlogImageModel.getPostImageList(cp, app.blogPost);
                int ptr = 1;
                var hiddenList = new StringBuilder();
                if (blogImageList.Count > 0) {
                    var blogImage = blogImageList.First();
                    string imageFilename = blogImage.Filename;
                    string imageName = blogImage.name;
                    hiddenList.Append($"<input type=\"hidden\" name=\"{constants.rnBlogImageID}.{ptr}\" value=\"{blogImage.id}\">");
                    imageForm += $"<tr><td><input type=\"checkbox\" name=\"{constants.rnBlogImageDelete}.{ptr}\">&nbsp;Delete</td></tr><tr><td align=\"left\" class=\"ccAdminSmall\"><img class=\"aoBlogEditImagePreview\" alt=\"{imageName}\" title=\"{imageName}\" src=\"{_GenericController.encodeURLForHrefSrc(cp.Http.CdnFilePathPrefix + imageFilename)}\"></td></tr>";
                    imageForm += $"<tr><td>Image Alt Text<br><INPUT TYPE=\"Text\" NAME=\"{constants.rnBlogImageName}.{ptr}\" SIZE=\"25\" value=\"{cp.Utils.EncodeHTML(imageName)}\"></td></tr>";
                    ptr++;
                }
                imageForm += "<TR style=\"padding-bottom:10px; padding-bottom:10px;\"><td><HR></td></tr>";
                if (ptr == 1) {
                    imageForm += $"<tr><td align=\"left\">Image<br><INPUT TYPE=\"file\" name=\"LibraryUpload.{ptr}\"></td></tr>";
                    imageForm += $"<tr><td align=\"left\">Name<br><INPUT TYPE=\"Text\" NAME=\"{constants.rnBlogImageName}.{ptr}\" SIZE=\"25\"></td></tr>";
                    imageForm += $"<tr><td align=\"left\">Description<br><TEXTAREA NAME=\"{constants.rnBlogImageDescription}.{ptr}\" ROWS=\"5\" COLS=\"50\"></TEXTAREA></td></tr>";
                }
                imageForm += "</Table>";
                imageForm += cp.Html.Hidden("LibraryUploadCount", ptr.ToString(), "LibraryUploadCount") + hiddenList.ToString();
                formHtml.Append(_GenericController.getFormTableRow(cp, "Images: ", imageForm));
                //
                // -- buttons
                if (blogEntry_id != 0) {
                    formHtml.Append(_GenericController.getFormTableRow(cp, "", $"{cp.Html.Button(constants.rnButton, constants.FormButtonPost)}&nbsp;{cp.Html.Button(constants.rnButton, constants.FormButtonCancel)}&nbsp;{cp.Html.Button(constants.rnButton, constants.FormButtonDelete)}"));
                } else {
                    formHtml.Append(_GenericController.getFormTableRow(cp, "", $"{cp.Html.Button(constants.rnButton, constants.FormButtonPost)}&nbsp;{cp.Html.Button(constants.rnButton, constants.FormButtonCancel)}"));
                }
                //
                // -- back link
                formHtml.Append(Constants.vbCrLf + _GenericController.getFormTableRow2(cp, $"<div class=\"aoBlogFooterLink\"><a href=\"{app.blogPageBaseLink}\">{constants.BackToRecentPostsMsg}</a></div>"));
                //
                // -- hidden fields
                formHtml.Append(cp.Html.Hidden(constants.RequestNameBlogEntryID, blogEntry_id.ToString()));
                formHtml.Append(cp.Html.Hidden(constants.RequestNameSourceFormID, constants.FormBlogEntryEditor.ToString()));
                formHtml.Append("</table>");
                //
                cp.Visit.SetProperty(constants.SNBlogEntryName, cp.Utils.GetRandomInteger().ToString());
                //
                result.editFormHtml = formHtml.ToString();
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "BlogEditViewModel.create");
                throw;
            }
        }
    }
}
