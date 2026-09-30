
using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.Models.Db;

namespace Contensive.Blog.Models {
    public class BlogEntryTopicRuleModel : Contensive.Models.Db.DbBaseModel, ICloneable {
        //
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Entry Topic Rules", "ccBlogEntryTopicRules", "default", false);
        //
        // ====================================================================================================
        // -- instance properties
        public int blogEntryId { get; set; }
        public int topicId { get; set; }
        //
        // ====================================================================================================
        public static BlogEntryTopicRuleModel @add(CPBaseClass cp) {
            return DbBaseModel.addDefault<BlogEntryTopicRuleModel>(cp);
        }
        //
        // ====================================================================================================
        public static BlogEntryTopicRuleModel create(CPBaseClass cp, int recordId) {
            return create<BlogEntryTopicRuleModel>(cp, recordId);
        }
        //
        // ====================================================================================================
        public static BlogEntryTopicRuleModel create(CPBaseClass cp, string recordGuid) {
            return create<BlogEntryTopicRuleModel>(cp, recordGuid);
        }
        //
        // ====================================================================================================
        public static BlogEntryTopicRuleModel createByName(CPBaseClass cp, string recordName) {
            return DbBaseModel.createByUniqueName<BlogEntryTopicRuleModel>(cp, recordName);
        }
        //
        // ====================================================================================================
        public static void delete(CPBaseClass cp, int recordId) {
            delete<BlogEntryTopicRuleModel>(cp, recordId);
        }
        //
        // ====================================================================================================
        public static void delete(CPBaseClass cp, string ccGuid) {
            delete<BlogEntryTopicRuleModel>(cp, ccGuid);
        }
        //
        // ====================================================================================================
        public static List<BlogEntryTopicRuleModel> createList(CPBaseClass cp, string sqlCriteria, string sqlOrderBy = "id") {
            return createList<BlogEntryTopicRuleModel>(cp, sqlCriteria, sqlOrderBy);
        }
        //
        // ====================================================================================================
        public static string getRecordName(CPBaseClass cp, int recordId) {
            return getRecordName<BlogEntryTopicRuleModel>(cp, recordId);
        }
        //
        // ====================================================================================================
        public static string getRecordName(CPBaseClass cp, string ccGuid) {
            return getRecordName<BlogEntryTopicRuleModel>(cp, ccGuid);
        }
        //
        // ====================================================================================================
        public static int getRecordId(CPBaseClass cp, string ccGuid) {
            return getRecordId<BlogEntryTopicRuleModel>(cp, ccGuid);
        }
        //
        // ====================================================================================================
        public static int getCount(CPBaseClass cp, string sqlCriteria) {
            return getCount<BlogEntryTopicRuleModel>(cp, sqlCriteria);
        }
        //
        // ====================================================================================================
        public object Clone() {
            return MemberwiseClone();
        }
    }
}
