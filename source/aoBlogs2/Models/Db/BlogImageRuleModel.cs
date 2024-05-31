
using Contensive.Models.Db;

namespace Contensive.Addons.Blog.Models {
    public class BlogImageRuleModel : Contensive.Models.Db.DbBaseModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Image Rules", "BlogImageRules", "default", false);
        // -- const
        //public const string contentName = "Blog Image Rules";      // <------ set content name
        //public const string contentTableName = "BlogImageRules";   // <------ set to tablename for the primary content (used for cache names)
        //private new const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public int BlogEntryID { get; set; }
        public int BlogImageID { get; set; }


    }
}