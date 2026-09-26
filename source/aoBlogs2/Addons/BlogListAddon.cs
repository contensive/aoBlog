
using Contensive.BaseClasses;
using System;
using System.Data;

namespace Contensive.Blog {
    public class BlogListAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogList;
        public const string guidAddon = constants.guidAddonBlogList;
        //
        public override object Execute(CPBaseClass cp) {
            try {
                if (!cp.User.IsAdmin) { return "<p>You are not authorized to access this feature.</p>"; }
                if (!cp.AdminUI.EndpointContainsPortal()) {
                    cp.Log.Warn($"BlogListAddon, endpoint does not contain portal, redirecting to BlogList");
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
                if (!cp.Doc.IsProperty(constants.rnButton)) { return; }
                string button = cp.Doc.GetText(constants.rnButton);
                if ((button ?? "") == constants.buttonAdd) {
                    //
                    // -- create a new blog and redirect to details
                    using (var cs = cp.CSNew()) {
                        cs.Insert(constants.cnBlogs);
                        if (cs.OK()) {
                            int newBlogId = cs.GetInteger("id");
                            cs.SetField("name", $"Blog {newBlogId}");
                            cs.SetField("active", "1");
                            cs.SetField("postsToDisplay", "5");
                            cs.SetField("overviewLength", "500");
                            cs.Close();
                            string detailLink = cp.AdminUI.GetPortalFeatureLink(constants.guidPortalShare, constants.guidPortalFeatureBlogDetails) + $"&{constants.rnBlogId}={newBlogId}";
                            cp.Log.Warn($"BlogListAddon, Add button clicked, new blogId [{newBlogId}], redirecting to BlogDetails");
                            cp.Response.Redirect(detailLink);
                        }
                    }
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
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderList(constants.guidAddonBlogList);
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogList;
                //
                // -- columns
                layoutBuilder.columnCaption = "Row";
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "";
                layoutBuilder.columnDownloadable = false;
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "b.id";
                layoutBuilder.columnCaption = "ID";
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "";
                layoutBuilder.columnDownloadable = true;
                layoutBuilder.columnSortable = true;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "b.name";
                layoutBuilder.columnCaption = "Name";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnSortable = true;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "";
                layoutBuilder.columnCaption = "Caption";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "";
                layoutBuilder.columnCaption = "Posts";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnName = "b.active";
                layoutBuilder.columnCaption = "Active";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                layoutBuilder.columnSortable = true;
                //
                // -- sql where clause
                string sqlWhere = "(1=1)";
                if (!string.IsNullOrEmpty(layoutBuilder.sqlSearchTerm)) {
                    sqlWhere += $" and(b.name like {cp.Db.EncodeSQLTextLike(layoutBuilder.sqlSearchTerm)})";
                }
                //
                // -- count
                string sqlCount = $"select count(*) from ccBlogs b where {sqlWhere}";
                using (DataTable dt = cp.Db.ExecuteQuery(sqlCount)) {
                    if (dt?.Rows != null && dt.Rows.Count == 1) {
                        layoutBuilder.recordCount = cp.Utils.EncodeInteger(dt.Rows[0][0]);
                    }
                }
                //
                // -- data query
                string sql = $"select b.id, b.name, b.caption, b.active, (select count(*) from ccBlogCopy e where e.blogId=b.id) as postCount from ccBlogs b where {sqlWhere}";
                string orderBy = "b.name";
                if (!string.IsNullOrEmpty(layoutBuilder.sortField)) {
                    orderBy = layoutBuilder.sortField;
                    if (layoutBuilder.sortDirection == "desc") { orderBy += " desc"; }
                }
                sql += $" order by {orderBy}";
                sql += $" OFFSET {(layoutBuilder.paginationPageNumber - 1) * layoutBuilder.paginationPageSize} ROWS FETCH NEXT {layoutBuilder.paginationPageSize} ROWS ONLY";
                //
                string detailLink = cp.AdminUI.GetPortalFeatureLink(constants.guidPortalShare, constants.guidPortalFeatureBlogDetails) + $"&{constants.rnBlogId}=";
                string postListLink = cp.AdminUI.GetPortalFeatureLink(constants.guidPortalShare, constants.guidPortalFeatureBlogPostList) + $"&{constants.rnBlogId}=";
                //
                int rowPtr = 0;
                int rowPtrStart = layoutBuilder.paginationPageSize * (layoutBuilder.paginationPageNumber - 1);
                using (DataTable dt = cp.Db.ExecuteQuery(sql)) {
                    if (dt?.Rows != null) {
                        foreach (DataRow dr in dt.Rows) {
                            int blogId = cp.Utils.EncodeInteger(dr["id"]);
                            string blogName = cp.Utils.EncodeText(dr["name"]);
                            if (string.IsNullOrWhiteSpace(blogName)) { blogName = "(no name)"; }
                            string blogNameLink = $"<a href=\"{detailLink}{blogId}\">{blogName}</a>";
                            string blogCaption = cp.Utils.EncodeText(dr["caption"]);
                            int postCount = cp.Utils.EncodeInteger(dr["postCount"]);
                            bool isActive = cp.Utils.EncodeBoolean(dr["active"]);
                            //
                            layoutBuilder.addRow();
                            layoutBuilder.setCell((rowPtrStart + rowPtr + 1).ToString());
                            layoutBuilder.setCell(blogId.ToString());
                            layoutBuilder.setCell(blogNameLink, blogName);
                            layoutBuilder.setCell(blogCaption);
                            layoutBuilder.setCell($"<a href=\"{postListLink}{blogId}\">{postCount}</a>", postCount);
                            layoutBuilder.setCell(isActive ? "Yes" : "No");
                            //
                            rowPtr += 1;
                        }
                    }
                }
                //
                // -- layout settings
                layoutBuilder.title = "Blogs";
                layoutBuilder.description = "Click a blog to see its details.";
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.includeForm = true;
                layoutBuilder.isOuterContainer = false;
                layoutBuilder.paginationPageSizeDefault = 50;
                layoutBuilder.allowDownloadButton = true;
                //
                // -- buttons
                layoutBuilder.addFormButton(constants.buttonAdd, constants.rnButton);
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogList);
                //
                // -- refresh query string
                cp.Doc.AddRefreshQueryString(constants.rnDstFeatureGuid, constants.guidPortalFeatureBlogList);
                //
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
