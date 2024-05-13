
using System;
using System.Collections.Generic;
using Contensive.BaseClasses;

namespace Contensive.Addons.Blog.Models {
    /// <summary>
    /// Blog copy is the root content for the ccBlogCopy table. 
    /// Blog Entries (posts) and Blog Comments (comments to posts) are both stored in Blog Copy
    /// </summary>
    public class BlogCopyModel : DbModel {
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Blog Copy";      // <------ set content name
        public const string contentTableName = "ccBlogCopy";   // <------ set to tablename for the primary content (used for cache names)
        private  const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // 
        public bool allowComments { get; set; }
        public bool anonymous { get; set; }
        public bool approved { get; set; }
        public int articlePrimaryImagePositionId { get; set; }
        public int authorMemberID { get; set; }
        public int blogCategoryID { get; set; }
        public int blogID { get; set; }
        public int entryID { get; set; }
        public int imageDisplayTypeId { get; set; }
        public int primaryImagePositionId { get; set; }
        public int viewings { get; set; }
        public string podcastMediaLink { get; set; }
        public int podcastSize { get; set; }
        public string tagList { get; set; }
        public string copy { get; set; }
        // 
        /// <summary>
        /// Return a list of Blog Copy
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blogId">The id of the Blog Copy</param>
        /// <returns></returns>
        public static List<BlogCopyModel> createListFromBlogCopy(CPBaseClass cp, int blogId) {
            var result = new List<BlogCopyModel>();
            try {
                string Sql = "select p.ID" + " from (ccBlogCopy p" + " left join BlogCategories c on c.id=p.blogCategoryID)" + " where (p.blogid=" + blogId + ")" + " and((c.id is null)or(c.UserBlocking=0)or(c.UserBlocking is null))";



                result = createList<BlogCopyModel>(cp, "(id in (" + Sql + "))");
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
        // 
        /// <summary>
        /// Return a list of Archive Blog Copy
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blogId">The id of the Blog Copy</param>
        /// <returns></returns>
        public static List<ArchiveDateModel> createArchiveListFromBlogCopy(CPBaseClass cp, int blogId) {
            var result = new List<ArchiveDateModel>();
            try {
                // result = createList(cp, "(BlogID=" & blogId & ")", "year(dateadded) desc, Month(DateAdded) desc")
                string SQL = "SELECT DISTINCT month(dateadded) as archiveMonth, year(dateadded) as archiveYear" + " From ccBlogCopy" + " Where (ContentControlID = " + cp.Content.GetID(constants.cnBlogEntries) + ") And (Active <> 0)" + " AND (BlogID=" + blogId + ")" + " ORDER BY year(dateadded) desc, month(dateadded) desc ";



                var cs = cp.CSNew();
                if (cs.OpenSQL(SQL)) {
                    do {
                        var archiveDate = new ArchiveDateModel();
                        archiveDate.Month = cs.GetInteger("archiveMonth");
                        archiveDate.Year = cs.GetInteger("archiveYear");
                        result.Add(archiveDate);
                        cs.GoNext();
                    }
                    while (cs.OK());
                }

                cs.Close();
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }

        public class ArchiveDateModel {
            public int Year;
            public int Month;
        }

    }
}