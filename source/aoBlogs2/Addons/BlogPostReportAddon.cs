
using Contensive.BaseClasses;
using Contensive.BaseClasses.LayoutBuilder;
using System;
using System.Collections.Generic;
using System.Data;

namespace Contensive.Blog {
    public class BlogPostReportAddon : AddonBaseClass {
        //
        public const string guidPortalFeature = constants.guidPortalFeatureBlogPostReport;
        public const string guidAddon = constants.guidAddonBlogPostReport;
        //
        private const string filterViewName = "BlogPostReport";
        //
        public override object Execute(CPBaseClass cp) {
            try {
                if (!cp.User.IsAdmin) { return "<p>You are not authorized to access this feature.</p>"; }
                if (!cp.AdminUI.EndpointContainsPortal()) {
                    return cp.AdminUI.RedirectToPortalFeature(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogPostReport, "");
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
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderList();
                //
                // -- get filter values
                int blogFilterId = layoutBuilder.getFilterInteger(constants.rnReportBlogFilter, filterViewName);
                int periodFilter = layoutBuilder.getFilterInteger(constants.rnReportPeriodFilter, filterViewName);
                if (periodFilter < 1 || periodFilter > 3) { periodFilter = 1; }
                //
                // -- blog filter (select from content)
                layoutBuilder.addFilterGroup("Blog");
                layoutBuilder.addFilterSelectContent("Blog", constants.rnReportBlogFilter, blogFilterId, constants.cnBlogs, "", "All Blogs");
                if (blogFilterId > 0) {
                    layoutBuilder.addActiveFilter("Blog", constants.rnReportBlogFilter, constants.rnReportBlogFilter);
                }
                //
                // -- period filter (select from list)
                layoutBuilder.addFilterGroup("Period");
                var periodOptions = new List<NameValueSelected> {
                    new NameValueSelected("All Time", "1", periodFilter == 1),
                    new NameValueSelected("Last 30 Days", "2", periodFilter == 2),
                    new NameValueSelected("Last 7 Days", "3", periodFilter == 3)
                };
                layoutBuilder.addFilterSelect("Period", constants.rnReportPeriodFilter, periodOptions);
                if (periodFilter > 1) {
                    layoutBuilder.addActiveFilter("Period", constants.rnReportPeriodFilter, constants.rnReportPeriodFilter);
                }
                //
                // -- date filter clause for the viewing log
                string dateFilterClause = "";
                switch (periodFilter) {
                    case 2:
                        dateFilterClause = " and v.dateAdded > dateadd(day, -30, getdate())";
                        break;
                    case 3:
                        dateFilterClause = " and v.dateAdded > dateadd(day, -7, getdate())";
                        break;
                }
                //
                // -- columns: Row, Blog, Post Name, Views, UTM Views, Active
                layoutBuilder.columnCaption = "Row";
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Blog";
                layoutBuilder.columnCaptionClass = "afwWidth200px afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Post Name";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                layoutBuilder.columnSortable = false;
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Views";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "UTM Views";
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
                if (blogFilterId > 0) {
                    sqlWhere += $" and (p.blogId={blogFilterId})";
                }
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
                // -- data query with view counts from BlogViewingLog
                string sql = $@"
                    select
                        p.id as postId,
                        p.name as postName,
                        p.active as postActive,
                        p.blogId,
                        b.name as blogName,
                        isnull(vc.viewCount, 0) as viewCount,
                        isnull(uc.utmViewCount, 0) as utmViewCount
                    from ccBlogCopy p
                        left join ccBlogs b on b.id = p.blogId
                        left join (
                            select
                                v.blogEntryId,
                                count(v.id) as viewCount
                            from BlogViewingLog v
                                inner join ccVisits vis on vis.id = v.visitId
                            where vis.bot = 0
                                and vis.excludeFromAnalytics = 0
                                {dateFilterClause}
                            group by v.blogEntryId
                        ) vc on vc.blogEntryId = p.id
                        left join (
                            select
                                v.blogEntryId,
                                count(v.id) as utmViewCount
                            from BlogViewingLog v
                                inner join ccVisits vis on vis.id = v.visitId
                            where vis.bot = 0
                                and vis.excludeFromAnalytics = 0
                                and (vis.refererPathPage like '%utm_%' or vis.http_referer like '%utm_%')
                                {dateFilterClause}
                            group by v.blogEntryId
                        ) uc on uc.blogEntryId = p.id
                    where {sqlWhere}";
                sql += string.IsNullOrEmpty(layoutBuilder.sqlOrderBy) ? " order by isnull(vc.viewCount, 0) desc" : $" order by {layoutBuilder.sqlOrderBy}";
                sql += $" OFFSET {(layoutBuilder.paginationPageNumber - 1) * layoutBuilder.paginationPageSize} ROWS FETCH NEXT {layoutBuilder.paginationPageSize} ROWS ONLY";
                //
                // -- build the post detail link base
                string postDetailBaseUrl = cp.AdminUI.GetPortalFeatureLink(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogPostDetails);
                string blogDetailBaseUrl = cp.AdminUI.GetPortalFeatureLink(constants.guidPortalContentManagement, constants.guidPortalFeatureBlogDetails);
                //
                int rowPtr = 0;
                using (DataTable dt = cp.Db.ExecuteQuery(sql)) {
                    if (dt?.Rows != null) {
                        foreach (DataRow dr in dt.Rows) {
                            int postId = cp.Utils.EncodeInteger(dr["postId"]);
                            string postName = cp.Utils.EncodeText(dr["postName"]);
                            if (string.IsNullOrWhiteSpace(postName)) { postName = "(no name)"; }
                            bool postActive = cp.Utils.EncodeBoolean(dr["postActive"]);
                            int blogId = cp.Utils.EncodeInteger(dr["blogId"]);
                            string blogName = cp.Utils.EncodeText(dr["blogName"]);
                            if (string.IsNullOrWhiteSpace(blogName)) { blogName = "(no blog)"; }
                            int viewCount = cp.Utils.EncodeInteger(dr["viewCount"]);
                            int utmViewCount = cp.Utils.EncodeInteger(dr["utmViewCount"]);
                            //
                            string postLink = cp.Utils.ModifyLinkQueryString(postDetailBaseUrl, constants.rnBlogId, blogId);
                            postLink = cp.Utils.ModifyLinkQueryString(postLink, constants.rnBlogPostId, postId);
                            //
                            string blogLink = cp.Utils.ModifyLinkQueryString(blogDetailBaseUrl, constants.rnBlogId, blogId);
                            //
                            layoutBuilder.addRow();
                            layoutBuilder.setCell((rowPtr + 1).ToString());
                            layoutBuilder.setCell($"<a href=\"{blogLink}\">{cp.Utils.EncodeHTML(blogName)}</a>");
                            layoutBuilder.setCell($"<a href=\"{postLink}\">{cp.Utils.EncodeHTML(postName)}</a>");
                            layoutBuilder.setCell(viewCount.ToString());
                            layoutBuilder.setCell(utmViewCount.ToString());
                            layoutBuilder.setCell(postActive ? "Yes" : "No");
                            //
                            rowPtr += 1;
                        }
                    }
                }
                //
                // -- layout settings
                layoutBuilder.title = "Blog Post Report";
                layoutBuilder.description = "Blog post views report with period and blog filters.";
                layoutBuilder.callbackAddonGuid = constants.guidAddonBlogPostReport;
                layoutBuilder.includeForm = true;
                layoutBuilder.includeBodyColor = true;
                layoutBuilder.includeBodyPadding = true;
                layoutBuilder.isOuterContainer = false;
                layoutBuilder.paginationPageSizeDefault = 50;
                //
                // -- hiddens
                layoutBuilder.addFormHidden(constants.rnSrcFormId, constants.formIdBlogPostReport);
                //
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
