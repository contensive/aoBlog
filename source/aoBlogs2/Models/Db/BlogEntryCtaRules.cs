
using Contensive.Models.Db;

namespace Contensive.Addons.Blog.Models {
    public class BlogEntryCTARuleModel : Contensive.Models.Db.DbBaseModel {
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Entry CTA Rules", "ccBlogEntryCTARules", "default", false);
        // -- const
        //public const string contentName = "Blog Entry CTA Rules";
        //public const string contentTableName = "ccBlogEntryCTARules";
        //private  const string contentDataSource = "default";
        // 
        // ====================================================================================================
        // -- instance properties
        public int blogentryid { get; set; }
        public int calltoactionid { get; set; }
    }
}