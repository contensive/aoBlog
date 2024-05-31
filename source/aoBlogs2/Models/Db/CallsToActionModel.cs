

using Contensive.Models.Db;

namespace Contensive.Addons.Blog.Models {
    public class CallsToActionModel : Contensive.Models.Db.DbBaseModel {
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Calls To Action", "callsToAction", "default", false);
        // -- const
        //public const string contentName = "Calls To Action";
        //public const string contentTableName = "callsToAction";
        //private new const string contentDataSource = "default";
        // 
        // ====================================================================================================
        // -- instance properties
        public string link { get; set; }
        public string headline { get; set; }
        public string brief { get; set; }
    }
}