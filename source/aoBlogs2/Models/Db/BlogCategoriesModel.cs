

namespace Contensive.Addons.Blog.Models {
    public class BlogCategoriesModel : DbModel {
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Blog Categories";      
        public const string contentTableName = "BlogCategories";   
        private  const string contentDataSource = "default";            
        // 
        // ====================================================================================================
        // -- instance properties
        public bool UserBlocking { get; set; }

    }
}