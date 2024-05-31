
//using Contensive.Models.Db;
//using System;

//namespace Contensive.Addons.Blog.Models {
//    public class MemberRuleModel : Contensive.Models.Db.DbBaseModel {
//        // 
//        // ====================================================================================================
//        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("xxxxx", "xxxxx", "default", false);
//        // -- const
//        public const string contentName = "Member Rules";      // <------ set content name
//        public const string contentTableName = "ccMemberRules";   // <------ set to tablename for the primary content (used for cache names)
//        private  const string contentDataSource = "default";             // <------ set to datasource if not default
//        // 
//        // ====================================================================================================
//        // -- instance properties

//        public DateTime DateExpires { get; set; }
//        public int GroupID { get; set; }
//        public int MemberID { get; set; }
//        // 
//    }
//}