using System;
using System.Linq;
using System.Text;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;
using Contensive.Models.Db;

namespace Contensive.Blog.Views {
    // 
    public sealed class EditView {
        // 
        // ====================================================================================
        // 
        public static string getFormBlogEdit(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request) {
            var result = new StringBuilder();
            try {
                // 
                result.Append("<table width=100% border=0 cellspacing=0 cellpadding=5 class=\"aoBlogPostTable\">");
                // 
                // -- create properties that can be either from the model, or the request
                int blogEntry_id = 0;
                string blogEntry_copy = "";
                string blogEntry_tagList = "";
                string blogEntry_name = "";
                int blongEntry_blogCategoryId = 0;
                if (app.blogPost is null) {
                    // 
                    // -- Create a new blog post
                    result.Append("<h2>Create a new blog post</h2>");
                    if ((request.ButtonValue ?? "") == constants.FormButtonPost) {
                        // 
                        // entry posted, but process could not save, display what was submitted
                        blogEntry_name = request.BlogEntryName;
                        blogEntry_copy = request.BlogEntryCopy;
                        blongEntry_blogCategoryId = request.BlogEntryCategoryId;
                        blogEntry_tagList = request.BlogEntryTagList;
                    }
                }
                else {
                    // 
                    // Edit an entry
                    blogEntry_id = app.blogPost.id;
                    blogEntry_name = app.blogPost.name;
                    blongEntry_blogCategoryId = app.blogPost.blogCategoryId;
                    blogEntry_copy = app.blogPost.copy;
                    if (string.IsNullOrEmpty(blogEntry_copy)) {
                        blogEntry_copy = "<!-- cc --><p><br></p><!-- /cc -->";
                    }
                    blogEntry_tagList = app.blogPost.tagList;
                }
                result.Append(_GenericController.getFormTableRow(cp, "<div style=\"padding-top:3px\">Title: </div>", cp.Html.InputText(constants.RequestNameBlogEntryName, blogEntry_name, 255, "form-control")));
                result.Append(_GenericController.getFormTableRow(cp, "<div style=\"padding-top:108px\">Post: </div>", cp.Html.InputWysiwyg(constants.RequestNameBlogEntryCopy, blogEntry_copy, CPHtmlBaseClass.EditorUserScope.ContentManager)));
                if (app.sitePropertyAllowTags) {
                    result.Append(_GenericController.getFormTableRow(cp, "<div style=\"padding-top:3px\">Tag List: </div>", cp.Html.InputText(constants.RequestNameBlogEntryTagList, blogEntry_tagList, 255, "form-control")));
                }
                if (app.blog.allowCategories) {
                    string CategorySelect = cp.Html.SelectContent(constants.RequestNameBlogEntryCategoryID, blongEntry_blogCategoryId.ToString(), "Blog Categories");
                    if (Strings.InStr(1, CategorySelect, "<option value=\"\"></option></select>", Constants.vbTextCompare) != 0) {
                        // 
                        // Select is empty
                        CategorySelect = "<div>This blog has no categories defined</div>";
                    }
                    result.Append(_GenericController.getFormTableRow(cp, "Category: ", CategorySelect));
                }
                // 
                // file upload form taken from Resource Library
                string imageForm = "" + "<TABLE id=\"UploadInsert\" border=\"0\" cellpadding=\"0\" cellspacing=\"1\" width=\"100%\" class=\"aoBlogImageTable\">" + "<tr>" + "";


                var BlogImageModelList = BlogImageModel.createListFromBlogEntry(cp, blogEntry_id);
                int Ptr = 1;
                var hiddenList = new StringBuilder();
                if (BlogImageModelList.Count > 0) {
                    var BlogImage = BlogImageModelList.First();
                    // For Each BlogImage In BlogImageModelList
                    string imageFilename = BlogImage.Filename;
                    string imageDescription = BlogImage.description;
                    string imageName = BlogImage.name;
                    hiddenList.Append("<input type=\"hidden\" name=\"" + constants.rnBlogImageID + "." + Ptr + "\" value=\"" + BlogImage.id + "\">");
                    if (Ptr != 1) {
                        imageForm = imageForm + "<tr>" + "<td><HR></td>" + "</tr>" + "";



                    }
                    // 
                    // -- image
                    // 
                    imageForm = imageForm + "<tr>" + "<td><input type=\"checkbox\" name=\"" + constants.rnBlogImageDelete + "." + Ptr + "\">&nbsp;Delete</td>" + "</tr>" + "<tr>" + "<td align=\"left\" class=\"ccAdminSmall\"><img class=\"aoBlogEditImagePreview\" alt=\"" + imageName + "\" title=\"" + imageName + "\" src=\"" + cp.Http.CdnFilePathPrefix + imageFilename + "\"></td>" + "</tr>" + "";






                    // 
                    // -- name
                    // 
                    imageForm = imageForm + "<tr>" + "<td>Image Alt Text<br><INPUT TYPE=\"Text\" NAME=\"" + constants.rnBlogImageName + "." + Ptr + "\" SIZE=\"25\" value=\"" + cp.Utils.EncodeHTML(imageName) + "\"></td>" + "</tr>" + "";



                    // '
                    // ' -- description
                    // '
                    // imageForm = imageForm _
                    // & "<tr>" _
                    // & "<td>Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50"">" & cp.Utils.EncodeHTML(imageDescription) & "</TEXTAREA></td>" _
                    // & "</tr>" _
                    // & ""
                    Ptr = Ptr + 1;
                }
                // 
                // -- Next
                // 
                imageForm = imageForm + "<TR style=\"padding-bottom:10px; padding-bottom:10px;\">" + "<td><HR></td>" + "</tr>" + "";



                // 
                // -- image
                // 
                if (Ptr == 1) {
                    imageForm = imageForm + "<tr>" + "<td align=\"left\">Image<br><INPUT TYPE=\"file\" name=\"LibraryUpload." + Ptr + "\"></td>" + "</tr>" + "";



                    // 
                    // -- image name
                    // 
                    imageForm = imageForm + "<tr>" + "<td align=\"left\">Name<br><INPUT TYPE=\"Text\" NAME=\"" + constants.rnBlogImageName + "." + Ptr + "\" SIZE=\"25\"></td>" + "</tr>" + "";



                    // 
                    // -- image description
                    // 
                    imageForm = imageForm + "<tr>" + "<td align=\"left\">Description<br><TEXTAREA NAME=\"" + constants.rnBlogImageDescription + "." + Ptr + "\" ROWS=\"5\" COLS=\"50\"></TEXTAREA></td>" + "</tr>" + "";



                }
                // 
                imageForm = imageForm + "</Table>" + "";

                // 
                imageForm = imageForm + cp.Html.Hidden("LibraryUploadCount", Ptr.ToString(), "LibraryUploadCount") + hiddenList.ToString();

                // 
                result.Append(_GenericController.getFormTableRow(cp, "Images: ", imageForm));
                if (blogEntry_id != 0) {
                    result.Append(_GenericController.getFormTableRow(cp, "", cp.Html.Button(constants.rnButton, constants.FormButtonPost) + "&nbsp;" + cp.Html.Button(constants.rnButton, constants.FormButtonCancel) + "&nbsp;" + cp.Html.Button(constants.rnButton, constants.FormButtonDelete)));
                }
                else {
                    result.Append(_GenericController.getFormTableRow(cp, "", cp.Html.Button(constants.rnButton, constants.FormButtonPost) + "&nbsp;" + cp.Html.Button(constants.rnButton, constants.FormButtonCancel)));
                }
                string qs = cp.Doc.RefreshQueryString;
                qs = cp.Utils.ModifyQueryString(qs, constants.RequestNameBlogEntryID, "", true);
                qs = cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogPostList.ToString());
                result.Append(Constants.vbCrLf + _GenericController.getFormTableRow2(cp, "<div class=\"aoBlogFooterLink\"><a href=\"" + app.blogPageBaseLink + "\">" + constants.BackToRecentPostsMsg + "</a></div>"));
                // 
                result.Append(cp.Html.Hidden(constants.RequestNameBlogEntryID, blogEntry_id.ToString()));
                result.Append(cp.Html.Hidden(constants.RequestNameSourceFormID, constants.FormBlogEntryEditor.ToString()));
                result.Append("</table>");
                // 
                // -- *****************************************************
                // -- De-Vince this
                // -- *****************************************************
                // 
                cp.Visit.SetProperty(constants.SNBlogEntryName, cp.Utils.GetRandomInteger().ToString());
                return cp.Html.Form(result.ToString());
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        // ====================================================================================
        /// <summary>
        /// Process a new blog entry or save the edit from an existing blog entry. If BlogEntry object is nothing, this is a new entry.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="app"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static int ProcessFormBlogEdit(CPBaseClass cp, ApplicationEnvironmentModel app, Models.View.RequestModel request) {
            int ProcessFormBlogEditRet = default;
            try {
                var blog = app.blog;
                var post = app.blogPost;
                if (!string.IsNullOrEmpty(cp.Visit.GetText(constants.SNBlogEntryName))) {
                    cp.Visit.GetText(constants.SNBlogEntryName, "");
                    if ((request.ButtonValue ?? "") == constants.FormButtonCancel) {
                        // 
                        // Cancel
                        // 
                        return constants.FormBlogPostList;
                    }
                    else if ((request.ButtonValue ?? "") == constants.FormButtonDelete) {
                        // 
                        // Delete
                        DbBaseModel.delete<BlogEntryModel>(cp, post.id);
                        RSSFeedModel.UpdateBlogFeed(cp);
                        post = null;
                        return constants.FormBlogPostList;
                    }
                    else if ((request.ButtonValue ?? "") == constants.FormButtonPost) {
                        // 
                        // Post
                        // 
                        if (post is null) {
                            post = DbBaseModel.addDefault<BlogEntryModel>(cp);
                            // 
                            // -- by default, the RSS feed should be checked for this new blog entry
                            var rule = RSSFeedBlogRuleModel.@add(cp);
                            rule.BlogPostID = post.id;
                            rule.RSSFeedID = app.rssFeed.id;
                            rule.save(cp);
                        }

                        post.name = request.BlogEntryName;
                        post.copy = request.BlogEntryCopy;
                        post.blogId = blog.id;
                        post.save(cp);
                        if (app.sitePropertyAllowTags) {                        // 
                            post.tagList = request.BlogEntryTagList;
                        }
                        if (app.blog.allowCategories) {
                            post.blogCategoryId = request.BlogEntryCategoryId;
                        }
                        // Upload files
                        // 
                        int UploadCount = cp.Doc.GetInteger("LibraryUploadCount");
                        if (UploadCount > 0) {
                            int UploadPointer;
                            var loopTo = UploadCount;
                            for (UploadPointer = 1; UploadPointer <= loopTo; UploadPointer++) {
                                int ImageID = cp.Doc.GetInteger(constants.rnBlogImageID + "." + UploadPointer);
                                string imageFilename = cp.Doc.GetText(constants.rnBlogUploadPrefix + "." + UploadPointer);
                                int imageOrder = cp.Doc.GetInteger(constants.rnBlogImageOrder + "." + UploadPointer);
                                string imageName = cp.Doc.GetText(constants.rnBlogImageName + "." + UploadPointer);
                                // Dim imageDescription As String = cp.Doc.GetText(rnBlogImageDescription & "." & UploadPointer)
                                int BlogImageID = 0;
                                if (ImageID != 0) {
                                    // 
                                    // edit image
                                    // 
                                    if (cp.Doc.GetBoolean(constants.rnBlogImageDelete + "." + UploadPointer)) {
                                        cp.Content.Delete(constants.cnBlogImages, ImageID.ToString());
                                    }
                                    else {
                                        var BlogImage = DbBaseModel.create<BlogImageModel>(cp, ImageID);
                                        if (BlogImage is not null) {
                                            BlogImage.name = imageName;
                                            // BlogImage.description = imageDescription
                                            BlogImage.sortOrder = new string('0', 12 - imageOrder.ToString().Length) + imageOrder.ToString();
                                            BlogImage.save(cp);
                                        }
                                    }
                                }
                                else if (!string.IsNullOrEmpty(imageFilename)) {
                                    // 
                                    // upload image
                                    // 
                                    // CS = Main.InsertCSRecord(cnBlogImages)
                                    // If Main.IsCSOK(CS) Then
                                    var BlogImage = DbBaseModel.addDefault<BlogImageModel>(cp);
                                    if (BlogImage is not null) {
                                        BlogImageID = BlogImage.id;
                                        BlogImage.name = imageName;
                                        // BlogImage.description = imageDescription
                                        string FileExtension = "";
                                        string FilenameNoExtension = "";
                                        int Pos = Strings.InStrRev(imageFilename, ".");
                                        if (Pos > 0) {
                                            FileExtension = Strings.Mid(imageFilename, Pos + 1);
                                            FilenameNoExtension = Strings.Left(imageFilename, Pos - 1);
                                        }
                                        string VirtualFilePath = BlogImage.getUploadPath("filename");
                                        cp.Html.ProcessInputFile(constants.rnBlogUploadPrefix + "." + UploadPointer, VirtualFilePath);
                                        BlogImage.Filename = VirtualFilePath + imageFilename;
                                        BlogImage.save(cp);
                                        BlogImage.sortOrder = new string('0', 12 - imageOrder.ToString().Length) + imageOrder.ToString();
                                    }
                                    // 
                                    var ImageRule = DbBaseModel.addDefault<BlogImageRuleModel>(cp);
                                    if (ImageRule is not null) {
                                        ImageRule.BlogEntryID = post.id;
                                        ImageRule.BlogImageID = BlogImageID;
                                        ImageRule.save(cp);
                                    }
                                }
                            }
                        }
                        // 
                        RSSFeedModel.UpdateBlogFeed(cp);
                        ProcessFormBlogEditRet = constants.FormBlogPostList;
                    }
                }
            }
            // 
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }

            return ProcessFormBlogEditRet;
        }
    }
}