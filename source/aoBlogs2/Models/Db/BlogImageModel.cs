
using Contensive.BaseClasses;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;

namespace Contensive.Addons.Blog.Models {
    public class BlogImageModel : Contensive.Models.Db.DbBaseModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Images", "BlogImages", "default", false);
        // -- const
        //public const string contentName = "Blog Images";      // <------ set content name
        //public const string contentTableName = "BlogImages";   // <------ set to tablename for the primary content (used for cache names)
        //private new const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public string AltSizeList { get; set; }
        public string description { get; set; }
        public string Filename { get; set; }
        public int height { get; set; }
        public int width { get; set; }

        public string getUploadPath(string fieldName) {
            return tableMetadata.tableNameLower + "/" + fieldName.ToLower() + "/" + id.ToString().PadLeft(12, '0') + "/";
        }


        // 
        /// <summary>
        /// Return a list of blog entry images for the blog entry
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="entryId">The id of the blog entry</param>
        /// <returns></returns>
        public static List<BlogImageModel> createListFromBlogEntry(CPBaseClass cp, int entryId) {
            var result = new List<BlogImageModel>();
            try {
                foreach (var Rule in createList<BlogImageRuleModel>(cp, "(BlogEntryID=" + entryId + ")")) {
                    var blogimage = create<BlogImageModel>(cp, Rule.BlogImageID);
                    if (blogimage is not null) {
                        result.Add(create<BlogImageModel>(cp, Rule.BlogImageID));
                    }
                }
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }


    }
}