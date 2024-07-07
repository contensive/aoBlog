
using Contensive.Models.Db;

namespace Contensive.Blog.Models {
    public class BlogCategoryGroupRulesModel : Contensive.Models.Db.DbBaseModel {
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Category Group Rules", "ccBlogCategoryGroupRules", "default", false);
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public int BlogCategoryID { get; set; }
        public int GroupID { get; set; }
        // 
    }
}