
using System;
using System.Collections.Generic;
using Contensive.BaseClasses;

namespace Contensive.Addons.Blog.Models {
    public class RSSFeedBlogRuleModel : DbModel, ICloneable {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "RSS Feed Blog Rules";      // <------ set content name
        public const string contentTableName = "ccRSSFeedRules";   // <------ set to tablename for the primary content (used for cache names)
        private  const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        // instancePropertiesGoHere
        public int BlogPostID { get; set; }
        public int RSSFeedID { get; set; }
        // 
        // ====================================================================================================
        public static RSSFeedBlogRuleModel @add(CPBaseClass cp) {
            return @add<RSSFeedBlogRuleModel>(cp);
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
            return createByName<RSSFeedBlogRuleModel>(cp, recordName);
        }
        // 
        // ====================================================================================================
        public void save(CPBaseClass cp) {
            save<RSSFeedBlogRuleModel>(cp);
        }
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
        public string getUploadPath(string fieldName) {
            return getUploadPath<RSSFeedBlogRuleModel>(fieldName);
        }
        // 
        // ====================================================================================================
        // 
        public RSSFeedBlogRuleModel Clone(CPBaseClass cp) {
            RSSFeedBlogRuleModel result = (RSSFeedBlogRuleModel)Clone();
            result.id = cp.Content.AddRecord(contentName);
            result.ccguid = cp.Utils.CreateGuid();
            result.save<BlogModel>(cp);
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
            int ContentControlID = cp.Content.GetID(contentName);
            foreach (var kvp in modelList) {
                if (!string.IsNullOrEmpty(kvp.Value.ccguid)) {
                    kvp.Value.id = 0;
                    var dbData = create(cp, kvp.Value.ccguid);
                    if (dbData is not null) {
                        kvp.Value.id = dbData.id;
                    }
                    else {
                        kvp.Value.DateAdded = DateTime.Now;
                        kvp.Value.CreatedBy = 0;
                    }
                    kvp.Value.ContentControlID = ContentControlID;
                    kvp.Value.ModifiedDate = DateTime.Now;
                    kvp.Value.ModifiedBy = 0;
                    kvp.Value.save<BlogModel>(cp);
                }
            }
        }


    }
}