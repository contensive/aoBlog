
using Contensive.Models.Db;

namespace Contensive.Blog.Models {
    public class BlogCommentModel : Contensive.Models.Db.DbBaseModel { 
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Comments", "ccBlogComments", "default", false);
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        //
        /// <summary>
        /// displayed only if approved. if blog.autoApproveComments then it is automatic, else must be manually approved
        /// </summary>
        public bool approved { get; set; }
        /// <summary>
        /// The blog for this comment
        /// </summary>
        public int blogId { get; set; }
        /// <summary>
        /// the blog entry for this comment
        /// </summary>
        public int entryId { get; set; }
        /// <summary>
        /// no idea what this does. Saved from comment proces
        /// </summary>
        public string formKey { get; set; }
        /// <summary>
        /// The text comment 
        /// </summary>
        public string copyText { get; set; }

    }
}