
using Contensive.Models.Db;

namespace Contensive.Blog.Models {
    public class BlogImageRuleModel : Contensive.Models.Db.DbBaseModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Image Rules", "BlogImageRules", "default", false);
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public int BlogEntryID { get; set; }
        public int BlogImageID { get; set; }


    }
}