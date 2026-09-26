
using Contensive.BaseClasses;
using System;
using System.Data;

namespace Contensive.Blog {
    public class BlogPostImagesAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogPostImages;
        public const string guidAddon = constants.guidAddonBlogPostImages;
        //
        // -- thumbnail width for the image list
        internal const int thumbnailWidth = 100;
        //
        public override object Execute(CPBaseClass cp) {
            try {
                if (!cp.User.IsAdmin) { return "<p>You are not authorized to access this feature.</p>"; }
                if (!cp.AdminUI.EndpointContainsPortal()) {
                    cp.Log.Warn($"BlogPostImagesAddon, endpoint does not contain portal, redirecting to BlogList");
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
                //
                int postId = cp.Doc.GetInteger(constants.rnBlogPostId);
                //
                if (button == constants.buttonAdd) {
                    //
                    // -- redirect to image details with imageId=0 for new
                    cp.Log.Warn($"BlogPostImagesAddon, Add button clicked, blogId [{blogId}], postId [{postId}], redirecting to BlogPostImageDetails");
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostImageDetails, $"&{constants.rnBlogImageId}=0");
                    return;
                }
                if (button == constants.buttonCancel || button == constants.buttonOK) {
                    //
                    // -- return to post list
                    cp.Log.Warn($"BlogPostImagesAddon, {button} button clicked, blogId [{blogId}], redirecting to BlogPostList");
                    cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList, "");
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
                //
                // -- load blog by id including inactive records (admin context)
                string blogName = "";
                using (DataTable dtBlog = cp.Db.ExecuteQuery($"select id,name from ccBlogs where id={blogId}")) {
                    if (dtBlog?.Rows == null || dtBlog.Rows.Count == 0) {
                        cp.Log.Warn($"BlogPostImagesAddon, blog not found, blogId [{blogId}], redirecting to BlogList");
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
                        cp.Log.Warn($"BlogPostImagesAddon, post not found, blogId [{blogId}], postId [{postId}], redirecting to BlogPostList");
                        return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList);
                    }
                    postName = cp.Utils.EncodeText(dtPost.Rows[0]["name"]);
                    postPrimaryImage = cp.Utils.EncodeText(dtPost.Rows[0]["primaryImage"]);
                    postPrimaryImageDescription = cp.Utils.EncodeText(dtPost.Rows[0]["primaryImageDescription"]);
                }
                //
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderList(constants.guidAddonBlogPostImages);
                //
                // -- columns
                layoutBuilder.columnCaption = "Row";
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                layoutBuilder.columnDownloadable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "image";
                layoutBuilder.columnCaption = "Image";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "name";
                layoutBuilder.columnCaption = "Name";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "filename";
                layoutBuilder.columnCaption = "Filename";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "alt";
                layoutBuilder.columnCaption = "Alt";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "title";
                layoutBuilder.columnCaption = "Title";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnSortable = false;
                //
                // -- base url for image detail links
                string imageDetailBaseUrl = cp.AdminUI.GetPortalFeatureLink(constants.guidPortalShare, constants.guidPortalFeatureBlogPostImageDetails);
                imageDetailBaseUrl = cp.Utils.ModifyLinkQueryString(imageDetailBaseUrl, constants.rnBlogId, blogId);
                imageDetailBaseUrl = cp.Utils.ModifyLinkQueryString(imageDetailBaseUrl, constants.rnBlogPostId, postId);
                //
                // -- build image list: primary image first, then secondary images by sort order
                int rowPtr = 0;
                //
                // -- primary image
                if (!string.IsNullOrEmpty(postPrimaryImage)) {
                    string imageLink = $"{imageDetailBaseUrl}&{constants.rnBlogImageIsPrimary}=1";
                    string thumbnailSrc = cp.Http.CdnFilePathPrefix + cp.Image.ResizeAndCrop(postPrimaryImage, thumbnailWidth, 0);
                    string thumbnailHtml = $"<a href=\"{imageLink}\"><img src=\"{thumbnailSrc}\" alt=\"{cp.Utils.EncodeHTML(postPrimaryImageDescription)}\" style=\"max-width:{thumbnailWidth}px;\"></a>";
                    // -- extract just the filename from the path
                    string filename = postPrimaryImage;
                    int lastSlash = filename.LastIndexOf('/');
                    string displayFilename = lastSlash >= 0 ? filename.Substring(lastSlash + 1) : filename;
                    //
                    layoutBuilder.addRow();
                    layoutBuilder.setCell((rowPtr + 1).ToString());
                    layoutBuilder.setCell(thumbnailHtml);
                    layoutBuilder.setCell($"<a href=\"{imageLink}\">(Primary Image)</a>");
                    layoutBuilder.setCell(displayFilename);
                    layoutBuilder.setCell(postPrimaryImageDescription);
                    layoutBuilder.setCell(postPrimaryImageDescription);
                    rowPtr++;
                }
                //
                // -- secondary images ordered by sort order
                string sql = $@"
                    select
                        i.id, i.name, i.filename, i.description, i.sortorder
                    from
                        BlogImages i
                    where
                        i.blogentryid={postId}
                    order by
                        i.sortOrder, i.id";
                using (DataTable dt = cp.Db.ExecuteQuery(sql)) {
                    foreach (DataRow dr in dt.Rows) {
                        int imageId = cp.Utils.EncodeInteger(dr["id"]);
                        string imageName = cp.Utils.EncodeText(dr["name"]);
                        string imageFilename = cp.Utils.EncodeText(dr["filename"]);
                        string imageDescription = cp.Utils.EncodeText(dr["description"]);
                        string imageLink = $"{imageDetailBaseUrl}&{constants.rnBlogImageId}={imageId}";
                        //
                        // -- thumbnail
                        string thumbnailHtml = "";
                        if (!string.IsNullOrEmpty(imageFilename)) {
                            string thumbnailSrc = cp.Http.CdnFilePathPrefix + cp.Image.ResizeAndCrop(imageFilename, thumbnailWidth, 0);
                            thumbnailHtml = $"<a href=\"{imageLink}\"><img src=\"{thumbnailSrc}\" alt=\"{cp.Utils.EncodeHTML(imageName)}\" style=\"max-width:{thumbnailWidth}px;\"></a>";
                        }
                        // -- extract just the filename from the path
                        int lastSlash = imageFilename.LastIndexOf('/');
                        string displayFilename = lastSlash >= 0 ? imageFilename.Substring(lastSlash + 1) : imageFilename;
                        //
                        layoutBuilder.addRow();
                        layoutBuilder.setCell((rowPtr + 1).ToString());
                        layoutBuilder.setCell(thumbnailHtml);
                        layoutBuilder.setCell($"<a href=\"{imageLink}\">{(string.IsNullOrEmpty(imageName) ? "(no name)" : imageName)}</a>", imageName);
                        layoutBuilder.setCell(displayFilename);
                        layoutBuilder.setCell(imageName);
                        layoutBuilder.setCell(imageDescription);
                        rowPtr++;
                    }
                }
                //
                layoutBuilder.recordCount = rowPtr;
                //
                // -- layout settings
                layoutBuilder.title = "Post Images";
                layoutBuilder.description = "";
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                //
                // -- feature subnav
                cp.Doc.AddRefreshQueryString(constants.rnBlogId, blogId);
                cp.Doc.AddRefreshQueryString(constants.rnBlogPostId, postId);
                layoutBuilder.portalSubNavTitleList.Add($"{blogName}, #{blogId}");
                layoutBuilder.portalSubNavTitleList.Add(postName);
                //
                // -- buttons
                layoutBuilder.addFormButton(constants.buttonAdd);
                layoutBuilder.addFormButton(constants.buttonOK);
                layoutBuilder.addFormButton(constants.buttonCancel);
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogPostImages);
                layoutBuilder.addFormHidden(constants.rnBlogPostId, postId);
                layoutBuilder.addFormHidden(constants.rnBlogId, blogId);
                //
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
