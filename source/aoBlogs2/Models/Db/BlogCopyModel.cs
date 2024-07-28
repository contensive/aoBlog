
//using Contensive.BaseClasses;
//using Contensive.Models.Db;
//using System;
//using System.Collections.Generic;

//namespace Contensive.Blog.Models {
//    /// <summary>
//    /// Blog copy is the root content for the ccBlogCopy table. 
//    /// Blog Entries (posts) and Blog Comments (comments to posts) are both stored in Blog Copy
//    /// </summary>
//    public class BlogCopyModel : Contensive.Models.Db.DbBaseModel {
//        // 
//        // ====================================================================================================
//        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Copy", "ccBlogCopy", "default", false);
//        // 
//        // ====================================================================================================
//        // -- instance properties
//        // 
//        public bool allowComments { get; set; }
//        public bool anonymous { get; set; }
//        public bool approved { get; set; }
//        public int articlePrimaryImagePositionId { get; set; }
//        public int authorMemberID { get; set; }
//        public int blogCategoryID { get; set; }
//        public int blogID { get; set; }
//        public int entryID { get; set; }
//        public int imageDisplayTypeId { get; set; }
//        public int primaryImagePositionId { get; set; }
//        public int viewings { get; set; }
//        public string podcastMediaLink { get; set; }
//        public int podcastSize { get; set; }
//        public string tagList { get; set; }
//        public string copy { get; set; }
//        // 
//        /// <summary>
//        /// Return a list of Blog Copy
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="blogId">The id of the Blog Copy</param>
//        /// <returns></returns>
//        public static List<BlogCopyModel> createListFromBlogCopy(CPBaseClass cp, int blogId) {
//            var result = new List<BlogCopyModel>();
//            try {
//                string Sql = "select p.ID" + " from (ccBlogCopy p" + " left join BlogCategories c on c.id=p.blogCategoryID)" + " where (p.blogid=" + blogId + ")" + " and((c.id is null)or(c.UserBlocking=0)or(c.UserBlocking is null))";



//                result = createList<BlogCopyModel>(cp, "(id in (" + Sql + "))");
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return result;
//        }

//    }
//}