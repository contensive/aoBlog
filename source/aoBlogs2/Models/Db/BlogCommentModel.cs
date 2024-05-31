
using Contensive.Models.Db;

namespace Contensive.Addons.Blog.Models {
    public class BlogCommentModel : Contensive.Models.Db.DbBaseModel {       // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Comments", "ccBlogCopy", "default", false);
        // -- const
        //public const string contentName = "Blog Comments";      // <------ set content name
        //public const string contentTableName = "ccBlogCopy";   // <------ set to tablename for the primary content (used for cache names)
        //private new const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public bool AllowComments { get; set; }
        public bool Anonymous { get; set; }
        public bool Approved { get; set; }
        public int articlePrimaryImagePositionId { get; set; }
        public int AuthorMemberID { get; set; }
        public int blogCategoryID { get; set; }
        public int BlogID { get; set; }
        // Public Property Copy As String
        public int EntryID { get; set; }
        public string FormKey { get; set; }
        public int imageDisplayTypeId { get; set; }
        public int primaryImagePositionId { get; set; }
        public int Viewings { get; set; }
        public string CopyText { get; set; }

    }
}