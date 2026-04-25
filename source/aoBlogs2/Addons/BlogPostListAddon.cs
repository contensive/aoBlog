
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;
using System.Data;

namespace Contensive.Blog {
    public class BlogPostListAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogPostList;
        public const string guidAddon = constants.guidAddonBlogPostList;
        //
        public override object Execute(CPBaseClass cp) {
            try {
                if (!cp.User.IsAdmin) { return "<p>You are not authorized to access this feature.</p>"; }
                if (!cp.AdminUI.EndpointContainsPortal()) {
                    return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogList, "");
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
                switch (button) {
                    case constants.buttonAdd: {
                            //
                            // -- redirect to post info with postId=0 for new
                            cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogPostInfo, $"&{constants.rnBlogId}={blogId}&{constants.rnBlogPostId}=0");
                            return;
                        }
                    case constants.buttonCancel: {
                            cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogDetails, $"&{constants.rnBlogId}={blogId}");
                            return;
                        }
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
                var blog = DbBaseModel.create<BlogModel>(cp, blogId);
                if (blog == null) { return "The blog is not valid."; }
                //
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderList();
                //
                // -- columns
                layoutBuilder.columnCaption = "Row";
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "ID";
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Name";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Date Added";
                layoutBuilder.columnCaptionClass = "afwWidth200px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Views";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Active";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                //
                // -- sql where clause
                string sqlWhere = $"(blogId={blogId})";
                if (!string.IsNullOrEmpty(layoutBuilder.sqlSearchTerm)) {
                    sqlWhere += $" and(name like {cp.Db.EncodeSQLTextLike(layoutBuilder.sqlSearchTerm)})";
                }
                //
                // -- count
                string sqlCount = $"select count(*) from ccBlogCopy where {sqlWhere}";
                using (DataTable dt = cp.Db.ExecuteQuery(sqlCount)) {
                    if (dt?.Rows != null && dt.Rows.Count == 1) {
                        layoutBuilder.recordCount = cp.Utils.EncodeInteger(dt.Rows[0][0]);
                    }
                }
                //
                // -- data query
                string sql = $"select id, name, dateAdded, viewings, active from ccBlogCopy where {sqlWhere}";
                sql += string.IsNullOrEmpty(layoutBuilder.sqlOrderBy) ? " order by dateAdded desc" : $" order by {layoutBuilder.sqlOrderBy}";
                sql += $" OFFSET {(layoutBuilder.paginationPageNumber - 1) * layoutBuilder.paginationPageSize} ROWS FETCH NEXT {layoutBuilder.paginationPageSize} ROWS ONLY";
                //
                string postDetailBaseUrl = cp.AdminUI.GetPortalFeatureLink(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogPostDetails);
                postDetailBaseUrl = cp.Utils.ModifyLinkQueryString(postDetailBaseUrl, constants.rnBlogId, blogId);
                //
                int rowPtr = 0;
                using (DataTable dt = cp.Db.ExecuteQuery(sql)) {
                    foreach (DataRow dr in dt.Rows) {
                        int postId = cp.Utils.EncodeInteger(dr["id"]);
                        string postName = cp.Utils.EncodeText(dr["name"]);
                        if (string.IsNullOrWhiteSpace(postName)) { postName = "(no name)"; }
                        DateTime dateAdded = cp.Utils.EncodeDate(dr["dateAdded"]);
                        int viewings = cp.Utils.EncodeInteger(dr["viewings"]);
                        bool isActive = cp.Utils.EncodeBoolean(dr["active"]);
                        string postLink = $"{postDetailBaseUrl}&{constants.rnBlogPostId}={postId}";
                        //
                        layoutBuilder.addRow();
                        layoutBuilder.setCell((rowPtr + 1).ToString());
                        layoutBuilder.setCell($"<a href=\"{postLink}\">{postId}</a>");
                        layoutBuilder.setCell($"<a href=\"{postLink}\">{postName}</a>");
                        layoutBuilder.setCell(dateAdded == DateTime.MinValue ? "" : dateAdded.ToShortDateString());
                        layoutBuilder.setCell(viewings.ToString());
                        layoutBuilder.setCell(isActive ? "Yes" : "No");
                        //
                        rowPtr += 1;
                    }
                }
                //
                // -- layout settings
                layoutBuilder.title = "Posts";
                layoutBuilder.description = "Blog posts for this blog.";
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostList;
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                layoutBuilder.paginationPageSizeDefault = 50;
                //
                // -- feature subnav
                cp.Doc.AddRefreshQueryString(constants.rnBlogId, blogId);
                layoutBuilder.portalSubNavTitle = $"{blog.name}, #{blog.id}";
                //
                // -- buttons
                layoutBuilder.addFormButton(constants.buttonAdd);
                layoutBuilder.addFormButton(constants.buttonCancel);
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogPostList);
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
