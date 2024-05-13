

namespace Contensive.Addons.Blog.Models {
    public class BlogCategoryGroupRulesModel : DbModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Blog Category Group Rules";      // <------ set content name
        public const string contentTableName = "ccBlogCategoryGroupRules";   // <------ set to tablename for the primary content (used for cache names)
        private  const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public int BlogCategoryID { get; set; }
        public int GroupID { get; set; }
        // 
    }
}