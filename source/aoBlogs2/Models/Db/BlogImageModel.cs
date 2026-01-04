
using Contensive.BaseClasses;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Data;

namespace Contensive.Blog.Models {
    public class BlogImageModel : Contensive.Models.Db.DbBaseModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Images", "BlogImages", "default", false);
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public string altSizeList { get; set; }
        public string description { get; set; }
        public string Filename { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        /// <summary>
        /// for images attached directly to the post. blank for images shared across posts
        /// </summary>
        public int blogEntryId { get; set; }

        public string getUploadPath(string fieldName) {
            return tableMetadata.tableNameLower + "/" + fieldName.ToLower() + "/" + id.ToString().PadLeft(12, '0') + "/";
        }
    }
}