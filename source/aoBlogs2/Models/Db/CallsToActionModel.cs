

using Contensive.Models.Db;

namespace Contensive.Blog.Models {
    public class CallsToActionModel : Contensive.Models.Db.DbBaseModel {
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Calls To Action", "callsToAction", "default", false);
        // -- const
        // 
        // ====================================================================================================
        // -- instance properties
        public string link { get; set; }
        public string headline { get; set; }
        public string brief { get; set; }
    }
}