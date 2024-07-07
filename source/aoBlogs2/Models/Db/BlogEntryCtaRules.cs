
using Contensive.Models.Db;

namespace Contensive.Blog.Models {
    public class BlogEntryCTARuleModel : Contensive.Models.Db.DbBaseModel {
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Entry CTA Rules", "BlogEntryCTARules", "default", false);
        // 
        // ====================================================================================================
        // -- instance properties
        public int blogentryid { get; set; }
        public int calltoactionid { get; set; }
    }
}