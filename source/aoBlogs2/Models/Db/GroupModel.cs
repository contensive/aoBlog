
using System.Collections.Generic;
using Contensive.BaseClasses;
//using Contensive.Models.Db;

namespace Contensive.Addons.Blog.Models {
    public class GroupModel : Contensive.Models.Db.DbBaseModel {
        //        // 
        //        // ====================================================================================================
        //        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Groups", "ccGroups", "default", false);
        //        // -- const
        //        public const string contentName = "Groups";      // <------ set content name
        //        public const string contentTableName = "ccGroups";   // <------ set to tablename for the primary content (used for cache names)
        //        private  const string contentDataSource = "default";             // <------ set to datasource if not default
        //        // 
        //        // ====================================================================================================
        //        // -- instance properties

        //        public bool AllowBulkEmail { get; set; }
        //        public string Caption { get; set; }
        //        public string CopyFilename { get; set; }
        //        public bool PublicJoin { get; set; }
        //        // 
        public static List<GroupModel> GetBlockingGroups(CPBaseClass cp, int BlogCategoryID) {
            // 
            var result = new List<GroupModel>();
            // 
            foreach (var BlogCategoryGroupRule in createList<BlogCategoryGroupRulesModel>(cp, "BlogCategoryID=" + BlogCategoryID))
                result.Add(create<GroupModel>(cp, BlogCategoryGroupRule.GroupID));
            return result;
        }

    }
}