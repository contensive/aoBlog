

namespace Contensive.Addons.Blog.Models {
    public class GroupRuleModel : DbModel {
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Group Rules";      // <------ set content name
        public const string contentTableName = "xxxxxtableNameGoesHerexxxxx";   // <------ set to tablename for the primary content (used for cache names)
        private new const string contentDataSource = "ccGroupRules";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties

        public bool AllowAdd { get; set; }
        public bool AllowDelete { get; set; }
        public int ContentID { get; set; }
        public int GroupID { get; set; }

    }
}