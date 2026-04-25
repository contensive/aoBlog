
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
                if (!cp.AdminUI.EndpointContainsPortal()) { return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogList, ""); }
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
                            string detailLink = cp.AdminUI.GetPortalFeatureLink(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogDetails) + $"&{constants.rnBlogId}={newBlogId}";
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
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderList();
                //
                // -- columns
                layoutBuilder.columnCaption = "Row";
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "ID";
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Name";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Caption";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Posts";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Active";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
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
                sql += string.IsNullOrEmpty(layoutBuilder.sqlOrderBy) ? " order by b.name" : $" order by {layoutBuilder.sqlOrderBy}";
                sql += $" OFFSET {(layoutBuilder.paginationPageNumber - 1) * layoutBuilder.paginationPageSize} ROWS FETCH NEXT {layoutBuilder.paginationPageSize} ROWS ONLY";
                //
                string detailLink = cp.AdminUI.GetPortalFeatureLink(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogDetails) + $"&{constants.rnBlogId}=";
                //
                int rowPtr = 0;
                using (var csList = cp.CSNew()) {
                    if (csList.OpenSQL(sql)) {
                        int rowPtrStart = layoutBuilder.paginationPageSize * (layoutBuilder.paginationPageNumber - 1);
                        do {
                            int blogId = csList.GetInteger("id");
                            string blogName = csList.GetText("name");
                            if (string.IsNullOrWhiteSpace(blogName)) { blogName = "(no name)"; }
                            blogName = $"<a href=\"{detailLink}{blogId}\">{blogName}</a>";
                            string blogCaption = csList.GetText("caption");
                            int postCount = csList.GetInteger("postCount");
                            bool isActive = csList.GetBoolean("active");
                            //
                            layoutBuilder.addRow();
                            layoutBuilder.setCell((rowPtrStart + rowPtr + 1).ToString());
                            layoutBuilder.setCell(blogId.ToString());
                            layoutBuilder.setCell(blogName);
                            layoutBuilder.setCell(blogCaption);
                            layoutBuilder.setCell(postCount.ToString());
                            layoutBuilder.setCell(isActive ? "Yes" : "No");
                            //
                            rowPtr += 1;
                            csList.GoNext();
                        } while (csList.OK());
                        csList.Close();
                    }
                }
                //
                // -- layout settings
                layoutBuilder.title = "Blogs";
                layoutBuilder.description = "Click a blog to see its details.";
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogList;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.includeForm = true;
                layoutBuilder.isOuterContainer = false;
                layoutBuilder.paginationPageSizeDefault = 50;
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
