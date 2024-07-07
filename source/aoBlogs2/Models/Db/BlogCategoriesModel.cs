

using Contensive.Models.Db;

namespace Contensive.Blog.Models {
    public class BlogCategoriesModel : Contensive.Models.Db.DbBaseModel {
        // 
        // ====================================================================================================
        //
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Categories", "BlogCategories", "default", false);
        //
        // ====================================================================================================
        // -- instance properties
        public bool UserBlocking { get; set; }

    }
}