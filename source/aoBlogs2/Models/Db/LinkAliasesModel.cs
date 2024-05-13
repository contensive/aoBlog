

namespace Contensive.Addons.Blog.Models {
    public class LinkAliasesModel : DbModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Link Aliases";      // <------ set content name
        public const string contentTableName = "ccLinkAliases";   // <------ set to tablename for the primary content (used for cache names)
        private  const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public string Link { get; set; }
        public int PageID { get; set; }
        public string QueryStringSuffix { get; set; }
        // 

    }
}