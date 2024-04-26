

namespace Contensive.Addons.Blog.Models {
    public class CallsToActionModel : DbModel {
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Calls To Action";
        public const string contentTableName = "callsToAction";
        private new const string contentDataSource = "default";
        // 
        // ====================================================================================================
        // -- instance properties
        public string link { get; set; }
        public string headline { get; set; }
        public string brief { get; set; }
    }
}