
using Contensive.BaseClasses;
using System;
using System.Data;

namespace Contensive.Blog {
    public class PostListAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeaturePostList;
        public const string guidAddon = constants.guidAddonPostList;
        //
        public override object Execute(CPBaseClass cp) {
            try {
                if (!cp.User.IsAdmin) { return "<p>You are not authorized to access this feature.</p>"; }
                if (!cp.AdminUI.EndpointContainsPortal()) {
                    cp.Log.Warn($"PostListAddon, endpoint does not contain portal, redirecting to PostList");
                    return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalShare, constants.guidPortalFeaturePostList, "");
                }
                return getForm(cp);
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
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderList(constants.guidAddonPostList);
                layoutBuilder.callbackAddonGuid = constants.guidAddonPostList;
                //
                // -- columns
                layoutBuilder.columnCaption = "Row";
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                layoutBuilder.columnDownloadable = false;
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "p.id";
                layoutBuilder.columnCaption = "ID";
                layoutBuilder.columnCaptionClass = "afwWidth50px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                layoutBuilder.columnDownloadable = true;
                layoutBuilder.columnSortable = true;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "b.name";
                layoutBuilder.columnCaption = "Blog";
                layoutBuilder.columnCaptionClass = "afwWidth200px afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnDownloadable = true;
                layoutBuilder.columnSortable = true;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "p.name";
                layoutBuilder.columnCaption = "Post Name";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnDownloadable = true;
                layoutBuilder.columnSortable = true;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "p.dateAdded";
                layoutBuilder.columnCaption = "Date Added";
                layoutBuilder.columnCaptionClass = "afwWidth200px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                layoutBuilder.columnDownloadable = true;
                layoutBuilder.columnSortable = true;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "p.viewings";
                layoutBuilder.columnCaption = "Views";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                layoutBuilder.columnDownloadable = true;
                layoutBuilder.columnSortable = true;
                //
                // -- sql where clause
                string sqlWhere = "(1=1)";
                if (!string.IsNullOrEmpty(layoutBuilder.sqlSearchTerm)) {
                    string likeTerm = cp.Db.EncodeSQLTextLike(layoutBuilder.sqlSearchTerm);
                    sqlWhere += $" and (p.name like {likeTerm} or b.name like {likeTerm})";
                }
                //
                // -- count
                string sqlCount = $"select count(*) from ccBlogCopy p left join ccBlogs b on b.id = p.blogId where {sqlWhere}";
                using (DataTable dt = cp.Db.ExecuteQuery(sqlCount)) {
                    if (dt?.Rows != null && dt.Rows.Count == 1) {
                        layoutBuilder.recordCount = cp.Utils.EncodeInteger(dt.Rows[0][0]);
                    }
                }
                //
                // -- data query
                string sql = $"select p.id, p.name, p.dateAdded, p.viewings, p.blogId, b.name as blogName from ccBlogCopy p left join ccBlogs b on b.id = p.blogId where {sqlWhere}";
                string orderBy = "p.dateAdded desc";
                if (!string.IsNullOrEmpty(layoutBuilder.sortField)) {
                    orderBy = layoutBuilder.sortField;
                    if (layoutBuilder.sortDirection == "desc") { orderBy += " desc"; }
                }
                sql += $" order by {orderBy}";
                sql += $" OFFSET {(layoutBuilder.paginationPageNumber - 1) * layoutBuilder.paginationPageSize} ROWS FETCH NEXT {layoutBuilder.paginationPageSize} ROWS ONLY";
                //
                // -- link to post edit view (BlogPostDetails under BlogList)
                string postDetailBaseUrl = cp.AdminUI.GetPortalFeatureLink(constants.guidPortalShare, constants.guidPortalFeatureBlogPostDetails);
                //
                int rowPtr = 0;
                int rowPtrStart = layoutBuilder.paginationPageSize * (layoutBuilder.paginationPageNumber - 1);
                using (DataTable dt = cp.Db.ExecuteQuery(sql)) {
                    if (dt?.Rows != null) {
                        foreach (DataRow dr in dt.Rows) {
                            int postId = cp.Utils.EncodeInteger(dr["id"]);
                            string postName = cp.Utils.EncodeText(dr["name"]);
                            if (string.IsNullOrWhiteSpace(postName)) { postName = "(no name)"; }
                            DateTime dateAdded = cp.Utils.EncodeDate(dr["dateAdded"]);
                            int viewings = cp.Utils.EncodeInteger(dr["viewings"]);
                            int blogId = cp.Utils.EncodeInteger(dr["blogId"]);
                            string blogName = cp.Utils.EncodeText(dr["blogName"]);
                            if (string.IsNullOrWhiteSpace(blogName)) { blogName = "(no blog)"; }
                            //
                            string postLink = $"{postDetailBaseUrl}&{constants.rnBlogId}={blogId}&{constants.rnBlogPostId}={postId}";
                            //
                            layoutBuilder.addRow();
                            layoutBuilder.setCell((rowPtrStart + rowPtr + 1).ToString());
                            layoutBuilder.setCell($"<a href=\"{postLink}\">{postId}</a>", postId);
                            layoutBuilder.setCell(cp.Utils.EncodeHTML(blogName), blogName);
                            layoutBuilder.setCell($"<a href=\"{postLink}\">{cp.Utils.EncodeHTML(postName)}</a>", postName);
                            layoutBuilder.setCell(dateAdded == DateTime.MinValue ? "" : dateAdded.ToShortDateString());
                            layoutBuilder.setCell(viewings.ToString());
                            //
                            rowPtr += 1;
                        }
                    }
                }
                //
                // -- layout settings
                layoutBuilder.title = "Posts";
                layoutBuilder.description = "All blog posts across all blogs. Click a post to edit. postlist-0104";
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.includeForm = true;
                layoutBuilder.isOuterContainer = false;
                layoutBuilder.paginationPageSizeDefault = 50;
                layoutBuilder.allowDownloadButton = true;
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdPostList);
                //
                // -- refresh query string
                cp.Doc.AddRefreshQueryString(constants.rnDstFeatureGuid, constants.guidPortalFeaturePostList);
                //
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
