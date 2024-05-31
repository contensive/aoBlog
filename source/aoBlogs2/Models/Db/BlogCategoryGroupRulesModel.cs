
using Contensive.Models.Db;

namespace Contensive.Addons.Blog.Models {
    public class BlogCategoryGroupRulesModel : Contensive.Models.Db.DbBaseModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Category Group Rules", "ccBlogCategoryGroupRules", "default", false);

        // -- const
        //public const string contentName = "Blog Category Group Rules";      // <------ set content name
        //public const string contentTableName = "ccBlogCategoryGroupRules";   // <------ set to tablename for the primary content (used for cache names)
        //private  const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public int BlogCategoryID { get; set; }
        public int GroupID { get; set; }
        // 
    }
}