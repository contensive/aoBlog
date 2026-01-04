
using Contensive.BaseClasses;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Data;

namespace Contensive.Blog.Models {
    public class BlogImageModel : Contensive.Models.Db.DbBaseModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Images", "BlogImages", "default", false);
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public string altSizeList { get; set; }
        public string description { get; set; }
        public string Filename { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        /// <summary>
        /// for images attached directly to the post. blank for images shared across posts
        /// </summary>
        public int blogEntryId { get; set; }

        public string getUploadPath(string fieldName) {
            return tableMetadata.tableNameLower + "/" + fieldName.ToLower() + "/" + id.ToString().PadLeft(12, '0') + "/";
        }
        // 
        /// <summary>
        /// Return a list of blog entry images for the blog entry
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="entryId">The id of the blog entry</param>
        /// <returns></returns>
        public static List<BlogImageModel> createListFromBlogEntry(CPBaseClass cp, int entryId) {
            var result = new List<BlogImageModel>();
            try {
                string sql = $@"
                    select distinct
                        i.*
                    from
	                    BlogImages i
	                    left join BlogImageRules r on r.blogimageid=i.id
                    where
	                    i.blogentryid={entryId}
	                    or (r.blogentryid={entryId})
                    order by
                        i.sortOrder, i.id
                    ";
                // create a list of blogimagemodel records for all images referenced directly and indirectly by the blog image rules
                using (DataTable dt = cp.Db.ExecuteQuery(sql.Replace("{entryId}", entryId.ToString()))) {
                    foreach (DataRow dr in dt.Rows) {
                        var blogimage = new BlogImageModel();
                        blogimage.load<BlogImageModel>(cp, dr);
                        result.Add(blogimage);
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}