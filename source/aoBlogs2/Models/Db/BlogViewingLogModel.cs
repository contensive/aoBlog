﻿

namespace Contensive.Addons.Blog.Models {
    public class BlogViewingLogModel : DbModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Blog Viewing Log";      // <------ set content name
        public const string contentTableName = "BlogViewingLog";   // <------ set to tablename for the primary content (used for cache names)
        private new const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public int BlogEntryID { get; set; }
        public int MemberID { get; set; }
        public int VisitID { get; set; }
        // 

    }
}