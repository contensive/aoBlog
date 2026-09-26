
using Contensive.BaseClasses;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Data;

namespace Contensive.Blog.Models {
    public class BlogImageModel : Contensive.Models.Db.DbBaseModel { 
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
        /// Controls the aspect ratio for this image. 0=use post/blog default.
        /// </summary>
        public int imageAspectRatioId { get; set; }
        /// <summary>
        /// the post this image belongs to
        /// </summary>
        public int blogEntryId { get; set; }

        public string getUploadPath(string fieldName) {
            return tableMetadata.tableNameLower + "/" + fieldName.ToLower() + "/" + id.ToString().PadLeft(12, '0') + "/";
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Return a list of blog entry images for the blog entry
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blogEntry"></param>
        /// <returns></returns>
        public static List<BlogImageModel> getPostImageList(CPBaseClass cp, BlogEntryModel blogEntry) {
            var result = new List<BlogImageModel>();
            try {
                if (!string.IsNullOrEmpty(blogEntry.primaryImage)) {
                    var primaryImage = new BlogImageModel {
                        blogEntryId = blogEntry.id,
                        Filename = blogEntry.primaryImage,
                        description = blogEntry.primaryImageDescription,
                        altSizeList = blogEntry.primaryImageAltSizeList
                    };
                    result.Add(primaryImage);
                }
                string sql = $@"
                    select
                        i.*
                    from
                        BlogImages i
                    where
                        i.blogentryid={blogEntry.id}
                    order by
                        i.sortOrder, i.id
                    ";
                using (DataTable dt = cp.Db.ExecuteQuery(sql)) {
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