

using Contensive.Models.Db;

namespace Contensive.Addons.Blog.Models {
    public class BlogCategoriesModel : Contensive.Models.Db.DbBaseModel {
        // 
        // ====================================================================================================
        //
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Categories", "BlogCategories", "default", false);
        //
        // -- const
        //public const string contentName = "";
        //public const string contentTableName = "BlogCategories";
        //private const string contentDataSource = "default";
        // 
        // ====================================================================================================
        // -- instance properties
        public bool UserBlocking { get; set; }

    }
}