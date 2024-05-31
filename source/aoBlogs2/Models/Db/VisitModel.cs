
//using System;
//using System.Collections.Generic;
//using Contensive.BaseClasses;
//using Contensive.Models.Db;

//namespace Contensive.Addons.Blog.Models {
//    public class VisitModel : Contensive.Models.Db.DbBaseModel, ICloneable {        // <------ set set model Name and everywhere that matches this string
//        // 
//        // ====================================================================================================
//        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("xxxxx", "xxxxx", "default", false);
//        // -- const
//        public const string contentName = "Visits";      // <------ set content name
//        public const string contentTableName = "ccVisits";   // <------ set to tablename for the primary content (used for cache names)
//        private  const string contentDataSource = "default";             // <------ set to datasource if not default
//        // 
//        // ====================================================================================================
//        // -- instance properties
//        // instancePropertiesGoHere
//        public bool Bot { get; set; }
//        // Public Property Browser As String
//        // Public Property CookieSupport As Boolean
//        public bool ExcludeFromAnalytics { get; set; }
//        // Public Property HTTP_REFERER As String
//        // Public Property LastVisitTime As Date
//        // Public Property LoginAttempts As Integer
//        // Public Property MemberID As Integer
//        // Public Property MemberNew As Boolean
//        // Public Property Mobile As Boolean
//        // Public Property PageVisits As Integer
//        // Public Property RefererPathPage As String
//        // Public Property REMOTE_ADDR As String
//        // Public Property StartDateValue As Integer
//        // Public Property StartTime As Date
//        // Public Property StopTime As Date
//        // Public Property TimeToLastHit As Integer
//        // Public Property VerboseReporting As Boolean
//        // Public Property VisitAuthenticated As Boolean
//        // Public Property VisitorID As Integer
//        // Public Property VisitorNew As Boolean
//        // 
//        // ====================================================================================================
//        public static VisitModel @add(CPBaseClass cp) {
//            return DbBaseModel.addDefault<VisitModel>(cp);
//        }
//        // 
//        // ====================================================================================================
//        public static VisitModel create(CPBaseClass cp, int recordId) {
//            return create<VisitModel>(cp, recordId);
//        }
//        // 
//        // ====================================================================================================
//        public static VisitModel create(CPBaseClass cp, string recordGuid) {
//            return create<VisitModel>(cp, recordGuid);
//        }
//        // 
//        // ====================================================================================================
//        public static VisitModel createByName(CPBaseClass cp, string recordName) {
//            return createByName<VisitModel>(cp, recordName);
//        }
//        // 
//        // ====================================================================================================
//        public void save(CPBaseClass cp) {
//            save<VisitModel>(cp);
//        }
//        // 
//        // ====================================================================================================
//        public static void delete(CPBaseClass cp, int recordId) {
//            delete<VisitModel>(cp, recordId);
//        }
//        // 
//        // ====================================================================================================
//        public static void delete(CPBaseClass cp, string ccGuid) {
//            delete<VisitModel>(cp, ccGuid);
//        }
//        // 
//        // ====================================================================================================
//        public static List<VisitModel> createList(CPBaseClass cp, string sqlCriteria, string sqlOrderBy = "id") {
//            return createList<VisitModel>(cp, sqlCriteria, sqlOrderBy);
//        }
//        // 
//        // ====================================================================================================
//        public static string getRecordName(CPBaseClass cp, int recordId) {
//            return getRecordName<VisitModel>(cp, recordId);
//        }
//        // 
//        // ====================================================================================================
//        public static string getRecordName(CPBaseClass cp, string ccGuid) {
//            return getRecordName<VisitModel>(cp, ccGuid);
//        }
//        // 
//        // ====================================================================================================
//        public static int getRecordId(CPBaseClass cp, string ccGuid) {
//            return getRecordId<VisitModel>(cp, ccGuid);
//        }
//        // 
//        // ====================================================================================================
//        public static int getCount(CPBaseClass cp, string sqlCriteria) {
//            return getCount<VisitModel>(cp, sqlCriteria);
//        }
//        // 
//        // ====================================================================================================
//        public string getUploadPath(string fieldName) {
//            return getUploadPath<VisitModel>(fieldName);
//        }
//        // 
//        // ====================================================================================================
//        // 
//        public VisitModel Clone(CPBaseClass cp) {
//            VisitModel result = (VisitModel)Clone();
//            result.id = cp.Content.AddRecord(contentName);
//            result.ccguid = cp.Utils.CreateGuid();
//            result.save(cp);
//            return result;
//        }
//        // 
//        // ====================================================================================================
//        // 
//        public object Clone() {
//            return MemberwiseClone();
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// Save a list of this model to the database, guid required, using the guid as a key for update/import, and ignoring the id.
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="modelList">A dictionary with guid as key, and this model as object</param>
//        public static void migrationImport(CPBaseClass cp, Dictionary<string, VisitModel> modelList) {
//            int ContentControlID = cp.Content.GetID(contentName);
//            foreach (var kvp in modelList) {
//                if (!string.IsNullOrEmpty(kvp.Value.ccguid)) {
//                    kvp.Value.id = 0;
//                    var dbData = create(cp, kvp.Value.ccguid);
//                    if (dbData is not null) {
//                        kvp.Value.id = dbData.id;
//                    }
//                    else {
//                        kvp.Value.dateAdded = DateTime.Now;
//                        kvp.Value.createdBy = 0;
//                    }
//                    kvp.Value.contentControlId = ContentControlID;
//                    kvp.Value.modifiedDate = DateTime.Now;
//                    kvp.Value.modifiedBy = 0;
//                    kvp.Value.save(cp);
//                }
//            }
//        }


//    }
//}