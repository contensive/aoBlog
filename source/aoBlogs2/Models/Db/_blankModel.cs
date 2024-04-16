

namespace Contensive.Addons.Blog.Models {
    public class xxxxxmodelNameGoesHerexxxxx : DbModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "xxxxxcontentNameGoesHerexxxxx";      // <------ set content name
        public const string contentTableName = "xxxxxtableNameGoesHerexxxxx";   // <------ set to tablename for the primary content (used for cache names)
        private new const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        // sample instance property -- Public Property DataSourceID As Integer


    }
}