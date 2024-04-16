

namespace Contensive.Addons.Blog.Models {
    public class BlogCategoriesModel : DbModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Blog Categories";      // <------ set content name
        public const string contentTableName = "BlogCategories";   // <------ set to tablename for the primary content (used for cache names)
        private new const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        public bool UserBlocking { get; set; }

    }
}