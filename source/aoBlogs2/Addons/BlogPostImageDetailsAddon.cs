
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;
using System.Data;

namespace Contensive.Blog {
    public class BlogPostImageDetailsAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogPostImageDetails;
        public const string guidAddon = constants.guidAddonBlogPostImageDetails;
        //
        // -- request names for form inputs
        private const string rnImageName = "rnImageName";
        private const string rnImageDescription = "rnImageDescription";
        private const string rnImageUpload = "rnImageUpload";
        private const string rnImageDelete = "rnImageDeleteFile";
        //
        public override object Execute(CPBaseClass cp) {
            try {
                if (!cp.User.IsAdmin) { return "<p>You are not authorized to access this feature.</p>"; }
                if (!cp.AdminUI.EndpointContainsPortal()) {
                    cp.Log.Warn($"BlogPostImageDetailsAddon, endpoint does not contain portal, redirecting to BlogList");
                    return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogList, "");
                }
                processForm(cp);
                return getForm(cp);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        internal static void processForm(CPBaseClass cp) {
            try {
                string button = cp.Doc.GetText(constants.rnButton);
                if (string.IsNullOrEmpty(button)) { return; }
                int blogId = cp.Doc.GetInteger(constants.rnBlogId);
                int postId = cp.Doc.GetInteger(constants.rnBlogPostId);
                int imageId = cp.Doc.GetInteger(constants.rnBlogImageId);
                bool isPrimary = cp.Doc.GetBoolean(constants.rnBlogImageIsPrimary);
                //
                if (button == constants.buttonSave || button == constants.buttonOK) {
                    if (isPrimary) {
                        //
                        // -- save primary image fields on the post record
                        var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                        if (post != null) {
                            post.primaryImageDescription = cp.Doc.GetText(rnImageName);
                            //
                            // -- handle file upload
                            string uploadedFilename = cp.Doc.GetText(rnImageUpload);
                            if (!string.IsNullOrEmpty(uploadedFilename)) {
                                string virtualFilePath = $"ccBlogCopy/primaryimage/{post.id.ToString().PadLeft(12, '0')}/";
                                cp.Html.ProcessInputFile(rnImageUpload, virtualFilePath);
                                post.primaryImage = virtualFilePath + uploadedFilename;
                                post.primaryImageAltSizeList = "";
                            }
                            //
                            // -- handle delete
                            if (cp.Doc.GetBoolean(rnImageDelete)) {
                                post.primaryImage = "";
                                post.primaryImageAltSizeList = "";
                            }
                            post.save(cp);
                        }
                    } else if (imageId == 0) {
                        //
                        // -- create a new secondary image record
                        var image = DbBaseModel.addDefault<BlogImageModel>(cp);
                        if (image != null) {
                            image.blogEntryId = postId;
                            image.name = cp.Doc.GetText(rnImageName);
                            image.description = cp.Doc.GetText(rnImageDescription);
                            image.save(cp);
                            //
                            // -- handle file upload
                            string uploadedFilename = cp.Doc.GetText(rnImageUpload);
                            if (!string.IsNullOrEmpty(uploadedFilename)) {
                                string virtualFilePath = image.getUploadPath("filename");
                                cp.Html.ProcessInputFile(rnImageUpload, virtualFilePath);
                                image.Filename = virtualFilePath + uploadedFilename;
                                image.save(cp);
                            }
                            //
                            // -- set the new image id so getForm displays the saved record
                            cp.Doc.SetProperty(constants.rnBlogImageId, image.id.ToString());
                        }
                    } else {
                        //
                        // -- save existing secondary image (BlogImages record)
                        var image = DbBaseModel.create<BlogImageModel>(cp, imageId);
                        if (image != null) {
                            image.name = cp.Doc.GetText(rnImageName);
                            image.description = cp.Doc.GetText(rnImageDescription);
                            //
                            // -- handle file upload
                            string uploadedFilename = cp.Doc.GetText(rnImageUpload);
                            if (!string.IsNullOrEmpty(uploadedFilename)) {
                                string virtualFilePath = image.getUploadPath("filename");
                                cp.Html.ProcessInputFile(rnImageUpload, virtualFilePath);
                                image.Filename = virtualFilePath + uploadedFilename;
                                image.altSizeList = "";
                            }
                            //
                            // -- handle delete
                            if (cp.Doc.GetBoolean(rnImageDelete)) {
                                image.Filename = "";
                                image.altSizeList = "";
                            }
                            image.save(cp);
                        }
                    }
                }
                if (button == constants.buttonCancel || button == constants.buttonOK) {
                    //
                    // -- return to image list
                    cp.Log.Warn($"BlogPostImageDetailsAddon, {button} button clicked, blogId [{blogId}], postId [{postId}], redirecting to BlogPostImages");
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostImages, "");
                    return;
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        internal static string getForm(CPBaseClass cp) {
            try {
                if (!cp.Response.isOpen) { return ""; }
                //
                int blogId = cp.Doc.GetInteger(constants.rnBlogId);
                int postId = cp.Doc.GetInteger(constants.rnBlogPostId);
                int imageId = cp.Doc.GetInteger(constants.rnBlogImageId);
                bool isPrimary = cp.Doc.GetBoolean(constants.rnBlogImageIsPrimary);
                bool isNew = (imageId == 0 && !isPrimary);
                //
                // -- load blog by id including inactive records (admin context)
                string blogName = "";
                using (DataTable dtBlog = cp.Db.ExecuteQuery($"select id,name from ccBlogs where id={blogId}")) {
                    if (dtBlog?.Rows == null || dtBlog.Rows.Count == 0) {
                        cp.Log.Warn($"BlogPostImageDetailsAddon, blog not found, blogId [{blogId}], redirecting to BlogList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogList);
                    }
                    blogName = cp.Utils.EncodeText(dtBlog.Rows[0]["name"]);
                }
                //
                // -- load post by id including inactive records (admin context)
                string postName = "";
                string postPrimaryImage = "";
                string postPrimaryImageDescription = "";
                using (DataTable dtPost = cp.Db.ExecuteQuery($"select id,name,primaryImage,primaryImageDescription from ccBlogCopy where id={postId}")) {
                    if (dtPost?.Rows == null || dtPost.Rows.Count == 0) {
                        cp.Log.Warn($"BlogPostImageDetailsAddon, post not found, blogId [{blogId}], postId [{postId}], redirecting to BlogPostList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList);
                    }
                    postName = cp.Utils.EncodeText(dtPost.Rows[0]["name"]);
                    postPrimaryImage = cp.Utils.EncodeText(dtPost.Rows[0]["primaryImage"]);
                    postPrimaryImageDescription = cp.Utils.EncodeText(dtPost.Rows[0]["primaryImageDescription"]);
                }
                //
                // -- load image data depending on primary vs secondary vs new
                string imageName = "";
                string imageDescription = "";
                string imageFilename = "";
                string imageTitle = "Image Details";
                if (isPrimary) {
                    //
                    // -- primary image
                    imageName = postPrimaryImageDescription;
                    imageDescription = postPrimaryImageDescription;
                    imageFilename = postPrimaryImage;
                    imageTitle = "Primary Image Details";
                } else if (isNew) {
                    //
                    // -- new image
                    imageTitle = "Add Image";
                } else {
                    //
                    // -- existing secondary image
                    var image = DbBaseModel.create<BlogImageModel>(cp, imageId);
                    if (image == null) {
                        cp.Log.Warn($"BlogPostImageDetailsAddon, image not found, blogId [{blogId}], postId [{postId}], imageId [{imageId}], redirecting to BlogPostImages");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostImages);
                    }
                    imageName = image.name ?? "";
                    imageDescription = image.description ?? "";
                    imageFilename = image.Filename ?? "";
                }
                //
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderNameValue();
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostImageDetails;
                //
                layoutBuilder.title = imageTitle;
                layoutBuilder.description = "";
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                //
                // -- current image preview
                if (!string.IsNullOrEmpty(imageFilename)) {
                    string previewSrc = cp.Http.CdnFilePathPrefix + cp.Image.ResizeAndCrop(imageFilename, 400, 0);
                    int lastSlash = imageFilename.LastIndexOf('/');
                    string displayFilename = lastSlash >= 0 ? imageFilename.Substring(lastSlash + 1) : imageFilename;
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Current Image";
                    layoutBuilder.rowValue = $"<div><img src=\"{previewSrc}\" alt=\"{cp.Utils.EncodeHTML(imageName)}\" style=\"max-width:400px;\"></div><div class=\"mt-1\">{displayFilename}</div>";
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Delete Image";
                    layoutBuilder.rowValue = cp.Html5.CheckBox(rnImageDelete, false, "form-check-input");
                    layoutBuilder.rowHelp = "Check to remove the current image file.";
                }
                //
                // -- upload new image
                layoutBuilder.addRow();
                layoutBuilder.rowName = !string.IsNullOrEmpty(imageFilename) ? "Replace Image" : "Upload Image";
                layoutBuilder.rowValue = cp.Html.InputFile(rnImageUpload, "", "");
                layoutBuilder.rowHelp = !string.IsNullOrEmpty(imageFilename) ? "Upload a new image to replace the current one." : "Upload an image file.";
                //
                // -- name field
                layoutBuilder.addRow();
                layoutBuilder.rowName = isPrimary ? "Description" : "Name";
                layoutBuilder.rowValue = cp.Html5.InputText(rnImageName, 255, imageName, "form-control");
                layoutBuilder.rowHelp = isPrimary ? "The description and alt text for the primary image." : "The name used as the ALT text for the image.";
                //
                // -- description field (secondary and new images only)
                if (!isPrimary) {
                    layoutBuilder.addRow();
                    layoutBuilder.rowName = "Description";
                    layoutBuilder.rowValue = cp.Html5.InputText(rnImageDescription, 255, imageDescription, "form-control");
                    layoutBuilder.rowHelp = "A description of the image.";
                }
                //
                // -- feature subnav
                cp.Doc.AddRefreshQueryString(constants.rnBlogId, blogId);
                cp.Doc.AddRefreshQueryString(constants.rnBlogPostId, postId);
                cp.Doc.AddRefreshQueryString(constants.rnBlogImageId, imageId);
                if (isPrimary) { cp.Doc.AddRefreshQueryString(constants.rnBlogImageIsPrimary, true); }
                layoutBuilder.portalSubNavTitleList.Add($"{blogName}, #{blogId}");
                layoutBuilder.portalSubNavTitleList.Add($"{postName}");
                layoutBuilder.portalSubNavTitleList.Add(isNew ? "Add Image" : isPrimary ? "Primary Image" : imageName);
                //
                // -- buttons
                layoutBuilder.addFormButton(constants.buttonOK);
                layoutBuilder.addFormButton(constants.buttonSave);
                layoutBuilder.addFormButton(constants.buttonCancel);
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogPostImageDetails);
                layoutBuilder.addFormHidden(constants.rnBlogPostId, postId);
                layoutBuilder.addFormHidden(constants.rnBlogId, blogId);
                layoutBuilder.addFormHidden(constants.rnBlogImageId, imageId);
                if (isPrimary) { layoutBuilder.addFormHidden(constants.rnBlogImageIsPrimary, true); }
                //
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
