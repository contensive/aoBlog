

using Contensive.Models.Db;

namespace Contensive.Blog.Models {
    public class BlogViewingLogModel : Contensive.Models.Db.DbBaseModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Viewing Log", "BlogViewingLog", "default", false);
        // -- const
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public int BlogEntryID { get; set; }
        public int MemberID { get; set; }
        public int VisitID { get; set; }
        // 

    }
}