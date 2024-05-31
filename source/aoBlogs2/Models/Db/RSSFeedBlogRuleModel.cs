
using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.Models.Db;

namespace Contensive.Addons.Blog.Models {
    public class RSSFeedBlogRuleModel : Contensive.Models.Db.DbBaseModel, ICloneable {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("RSS Feed Blog Rules", "ccRSSFeedRules", "default", false);
        // -- const
        //public const string contentName = "RSS Feed Blog Rules";      // <------ set content name
        //public const string contentTableName = "ccRSSFeedRules";   // <------ set to tablename for the primary content (used for cache names)
        //private  const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public int BlogPostID { get; set; }
        public int RSSFeedID { get; set; }
        // 
        // ====================================================================================================
        public static RSSFeedBlogRuleModel @add(CPBaseClass cp) {
            return DbBaseModel.addDefault<RSSFeedBlogRuleModel>(cp);
        }
        // 
        // ====================================================================================================
        public static RSSFeedBlogRuleModel create(CPBaseClass cp, int recordId) {
            return create<RSSFeedBlogRuleModel>(cp, recordId);
        }
        // 
        // ====================================================================================================
        public static RSSFeedBlogRuleModel create(CPBaseClass cp, string recordGuid) {
            return create<RSSFeedBlogRuleModel>(cp, recordGuid);
        }
        // 
        // ====================================================================================================
        public static RSSFeedBlogRuleModel createByName(CPBaseClass cp, string recordName) {
            return DbBaseModel.createByUniqueName<RSSFeedBlogRuleModel>(cp, recordName);
        }
        //// 
        //// ====================================================================================================
        //public void save(CPBaseClass cp) {
        //    save<RSSFeedBlogRuleModel>(cp);
        //}
        // 
        // ====================================================================================================
        public static void delete(CPBaseClass cp, int recordId) {
            delete<RSSFeedBlogRuleModel>(cp, recordId);
        }
        // 
        // ====================================================================================================
        public static void delete(CPBaseClass cp, string ccGuid) {
            delete<RSSFeedBlogRuleModel>(cp, ccGuid);
        }
        // 
        // ====================================================================================================
        public static List<RSSFeedBlogRuleModel> createList(CPBaseClass cp, string sqlCriteria, string sqlOrderBy = "id") {
            return createList<RSSFeedBlogRuleModel>(cp, sqlCriteria, sqlOrderBy);
        }
        // 
        // ====================================================================================================
        public static string getRecordName(CPBaseClass cp, int recordId) {
            return getRecordName<RSSFeedBlogRuleModel>(cp, recordId);
        }
        // 
        // ====================================================================================================
        public static string getRecordName(CPBaseClass cp, string ccGuid) {
            return getRecordName<RSSFeedBlogRuleModel>(cp, ccGuid);
        }
        // 
        // ====================================================================================================
        public static int getRecordId(CPBaseClass cp, string ccGuid) {
            return getRecordId<RSSFeedBlogRuleModel>(cp, ccGuid);
        }
        // 
        // ====================================================================================================
        public static int getCount(CPBaseClass cp, string sqlCriteria) {
            return getCount<RSSFeedBlogRuleModel>(cp, sqlCriteria);
        }
        // 
        // ====================================================================================================
        //public string getUploadPath(string fieldName) {
        //    return DbBaseModel.getUploadPath<RSSFeedBlogRuleModel>(fieldName);
        //}

        public string getUploadPath(string fieldName) {
            return tableMetadata.tableNameLower + "/" + fieldName.ToLower() + "/" + id.ToString().PadLeft(12, '0') + "/";
        }


        // 
        // ====================================================================================================
        // 
        public RSSFeedBlogRuleModel Clone(CPBaseClass cp) {
            RSSFeedBlogRuleModel result = (RSSFeedBlogRuleModel)Clone();
            result.id = cp.Content.AddRecord(tableMetadata.contentName);
            result.ccguid = cp.Utils.CreateGuid();
            result.save(cp);
            return result;
        }
        // 
        // ====================================================================================================
        // 
        public object Clone() {
            return MemberwiseClone();
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// Save a list of this model to the database, guid required, using the guid as a key for update/import, and ignoring the id.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="modelList">A dictionary with guid as key, and this model as object</param>
        public static void migrationImport(CPBaseClass cp, Dictionary<string, RSSFeedBlogRuleModel> modelList) {
            int ContentControlID = cp.Content.GetID(tableMetadata.contentName);
            foreach (var kvp in modelList) {
                if (!string.IsNullOrEmpty(kvp.Value.ccguid)) {
                    kvp.Value.id = 0;
                    var dbData = create(cp, kvp.Value.ccguid);
                    if (dbData is not null) {
                        kvp.Value.id = dbData.id;
                    }
                    else {
                        kvp.Value.dateAdded = DateTime.Now;
                        kvp.Value.createdBy = 0;
                    }
                    kvp.Value.contentControlId = ContentControlID;
                    kvp.Value.modifiedDate = DateTime.Now;
                    kvp.Value.modifiedBy = 0;
                    kvp.Value.save(cp);
                }
            }
        }


    }
}