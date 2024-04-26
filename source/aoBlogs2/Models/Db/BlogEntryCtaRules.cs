
namespace Contensive.Addons.Blog.Models {
    public class BlogEntryCTARuleModel : DbModel {
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Blog Entry CTA Rules";
        public const string contentTableName = "ccBlogEntryCTARules";
        private new const string contentDataSource = "default";
        // 
        // ====================================================================================================
        // -- instance properties
        public int blogentryid { get; set; }
        public int calltoactionid { get; set; }
    }
}