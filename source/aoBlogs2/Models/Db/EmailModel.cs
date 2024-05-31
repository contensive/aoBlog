
//using Contensive.Models.Db;
//using System;

//namespace Contensive.Addons.Blog.Models {
//    public class EmailModel : Contensive.Models.Db.DbBaseModel {
//        // 
//        // ====================================================================================================
//        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Email", "ccEmail", "default", false);
//        // -- const
//        public const string contentName = "Email";      // <------ set content name
//        public const string contentTableName = "ccEmail";   // <------ set to tablename for the primary content (used for cache names)
//        private new const string contentDataSource = "default";             // <------ set to datasource if not default
//        // 
//        // ====================================================================================================
//        // -- instance properties

//        public bool AddLinkEID { get; set; }
//        public bool AllowSpamFooter { get; set; }
//        public bool BlockSiteStyles { get; set; }
//        public DateTime ConditionExpireDate { get; set; }
//        public int ConditionID { get; set; }
//        public int ConditionPeriod { get; set; }
//        public string CopyFilename { get; set; }
//        public int EmailTemplateID { get; set; }
//        public int EmailWizardID { get; set; }
//        public string FromAddress { get; set; }
//        public string InlineStyles { get; set; }
//        public DateTime LastSendTestDate { get; set; }
//        public DateTime ScheduleDate { get; set; }
//        public bool Sent { get; set; }
//        public string StylesFilename { get; set; }
//        public string Subject { get; set; }
//        public bool Submitted { get; set; }
//        public int TestMemberID { get; set; }
//        public bool ToAll { get; set; }


//    }
//}