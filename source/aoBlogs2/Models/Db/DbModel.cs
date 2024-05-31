
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using Contensive.BaseClasses;

//namespace Contensive.Addons.Blog.Models {
//    public abstract class DbModel {
//        // 
//        // ====================================================================================================
//        // -- const must be set in derived clases
//        // 
//        // Public Const contentName As String = "" '<------ set content name
//        // Public Const contentTableName As String = "" '<------ set to tablename for the primary content (used for cache names)
//        // Public Const contentDataSource As String = "" '<----- set to datasource if not default
//        // 
//        // ====================================================================================================
//        // -- instance properties
//        public int id { get; set; }
//        public string name { get; set; }
//        public string ccguid { get; set; }
//        public bool Active { get; set; }
//        public int ContentControlID { get; set; }
//        public int CreatedBy { get; set; }
//        public int CreateKey { get; set; }
//        public DateTime dateAdded { get; set; }
//        public int ModifiedBy { get; set; }
//        public DateTime ModifiedDate { get; set; }
//        public string SortOrder { get; set; }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// Get the name of the content meta data for the model instance
//        /// </summary>
//        /// <param name="derivedType"></param>
//        /// <returns></returns>
//        private static string derivedContentName(Type derivedType) {
//            var fieldInfo = derivedType.GetField("contentName");
//            if (fieldInfo is null) {
//                throw new ApplicationException("Class [" + derivedType.Name + "] must declare constant [contentName].");
//            }
//            else {
//                return fieldInfo.GetRawConstantValue().ToString();
//            }
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// Get the name of the database table for this meta data
//        /// </summary>
//        /// <param name="derivedType"></param>
//        /// <returns></returns>
//        private static string derivedContentTableName(Type derivedType) {
//            var fieldInfo = derivedType.GetField("contentTableName");
//            if (fieldInfo is null) {
//                throw new ApplicationException("Class [" + derivedType.Name + "] must declare constant [contentTableName].");
//            }
//            else {
//                return fieldInfo.GetRawConstantValue().ToString();
//            }
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// Get the name of the datasource for this metadata
//        /// </summary>
//        /// <param name="derivedType"></param>
//        /// <returns></returns>
//        private static string contentDataSource(Type derivedType) {
//            var fieldInfo = derivedType.GetField("contentTableName");
//            if (fieldInfo is null) {
//                throw new ApplicationException("Class [" + derivedType.Name + "] must declare constant [contentTableName].");
//            }
//            else {
//                return fieldInfo.GetRawConstantValue().ToString();
//            }
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// Create an empty object. needed for deserialization
//        /// </summary>
//        public DbModel() {
//            // 
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// Add a new recod to the db and open it. Starting a new model with this method will use the default values in Contensive metadata (active, contentcontrolid, etc).
//        /// include callersCacheNameList to get a list of cacheNames used to assemble this response
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <returns></returns>
//        internal static T DbBaseModel.addDefault<T>(CPBaseClass cp) where T : Contensive.Models.Db.DbBaseModel {
//            T result = null;
//            try {
//                var instanceType = typeof(T);
//                string contentName = derivedContentName(instanceType);
//                result = create<T>(cp, cp.Content.AddRecord(contentName));
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//                throw;
//            }
//            return result;
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="recordId">The id of the record to be read into the new object</param>
//        internal static T create<T>(CPBaseClass cp, int recordId) where T : Contensive.Models.Db.DbBaseModel {
//            T result = null;
//            try {
//                if (recordId > 0) {
//                    var instanceType = typeof(T);
//                    string contentName = derivedContentName(instanceType);
//                    var cs = cp.CSNew();
//                    if (cs.Open(contentName, "(id=" + recordId.ToString() + ")")) {
//                        result = loadRecord<T>(cp, cs);
//                    }
//                    cs.Close();
//                }
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//                throw;
//            }
//            return result;
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// open an existing object
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="recordGuid"></param>
//        internal static T create<T>(CPBaseClass cp, string recordGuid) where T : Contensive.Models.Db.DbBaseModel {
//            T result = null;
//            try {
//                var instanceType = typeof(T);
//                string contentName = derivedContentName(instanceType);
//                var cs = cp.CSNew();
//                if (cs.Open(contentName, "(ccGuid=" + cp.Db.EncodeSQLText(recordGuid) + ")")) {
//                    result = loadRecord<T>(cp, cs);
//                }
//                cs.Close();
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//                throw;
//            }
//            return result;
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// open an existing object
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="recordName"></param>
//        internal static T createByName<T>(CPBaseClass cp, string recordName) where T : Contensive.Models.Db.DbBaseModel {
//            T result = null;
//            try {
//                if (!string.IsNullOrEmpty(recordName)) {
//                    var instanceType = typeof(T);
//                    string contentName = derivedContentName(instanceType);
//                    var cs = cp.CSNew();
//                    if (cs.Open(contentName, "(name=" + cp.Db.EncodeSQLText(recordName) + ")", "id")) {
//                        result = loadRecord<T>(cp, cs);
//                    }
//                    cs.Close();
//                }
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return result;
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// open an existing object
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="cs"></param>
//        private static T loadRecord<T>(CPBaseClass cp, CPCSBaseClass cs, List<string> listOfLowerCaseFields = null) where T : Contensive.Models.Db.DbBaseModel {
//            T modelInstance = null;
//            try {
//                if (cs.OK()) {
//                    var instanceType = typeof(T);
//                    string tableName = derivedContentTableName(instanceType);
//                    modelInstance = (T)Activator.CreateInstance(instanceType);
//                    // 
//                    foreach (PropertyInfo modelProperty in modelInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
//                        bool includeField = true;
//                        if (listOfLowerCaseFields is not null) {
//                            includeField = listOfLowerCaseFields.Contains(modelProperty.Name.ToLower());
//                        }
//                        if (includeField) {
//                            switch (modelProperty.Name.ToLower() ?? "") {
//                                case "specialcasefield": {
//                                        break;
//                                    }
//                                case "sortorder": {
//                                        // 
//                                        // -- customization for pc, could have been in default property, db default, etc.
//                                        string sortOrder = cs.GetText(modelProperty.Name);
//                                        if (string.IsNullOrEmpty(sortOrder)) {
//                                            sortOrder = "9999";
//                                        }
//                                        modelProperty.SetValue(modelInstance, sortOrder, null);
//                                        break;
//                                    }

//                                default: {
//                                        // 
//                                        // -- get the underlying type if this is nullable
//                                        bool targetNullable = IsNullable(modelProperty.PropertyType);
//                                        string prpertyValueText = cs.GetText(modelProperty.Name);
//                                        if (targetNullable & string.IsNullOrEmpty(prpertyValueText)) {
//                                            // 
//                                            // -- load a blank value to a nullable property as null
//                                            modelProperty.SetValue(modelInstance, null, null);
//                                        }
//                                        else {
//                                            // 
//                                            // -- not nullable or value is not null
//                                            var targetType = targetNullable ? Nullable.GetUnderlyingType(modelProperty.PropertyType) : modelProperty.PropertyType;
//                                            switch (targetType.Name ?? "") {
//                                                case "Int32": {
//                                                        modelProperty.SetValue(modelInstance, cs.GetInteger(modelProperty.Name), null);
//                                                        break;
//                                                    }
//                                                case "Boolean": {
//                                                        modelProperty.SetValue(modelInstance, cs.GetBoolean(modelProperty.Name), null);
//                                                        break;
//                                                    }
//                                                case "DateTime": {
//                                                        modelProperty.SetValue(modelInstance, cs.GetDate(modelProperty.Name), null);
//                                                        break;
//                                                    }
//                                                case "Double": {
//                                                        modelProperty.SetValue(modelInstance, cs.GetNumber(modelProperty.Name), null);
//                                                        break;
//                                                    }
//                                                case "String": {
//                                                        modelProperty.SetValue(modelInstance, cs.GetText(modelProperty.Name), null);
//                                                        break;
//                                                    }
//                                                case "fieldTypeTextFile": {
//                                                        // 
//                                                        // -- cdn files
//                                                        var instanceFileType = new fieldTypeTextFile() { filename = cs.GetFilename(modelProperty.Name) };
//                                                        modelProperty.SetValue(modelInstance, instanceFileType, null);
//                                                        break;
//                                                    }
//                                                case "fieldTypeJavascriptFile": {
//                                                        // 
//                                                        // -- cdn files
//                                                        var instanceFileType = new fieldTypeJavascriptFile();
//                                                        instanceFileType.filename = cs.GetFilename(modelProperty.Name);
//                                                        modelProperty.SetValue(modelInstance, instanceFileType, null);
//                                                        break;
//                                                    }
//                                                case "fieldTypeCSSFile": {
//                                                        // 
//                                                        // -- cdn files
//                                                        var instanceFileType = new fieldTypeCSSFile();
//                                                        instanceFileType.filename = cs.GetFilename(modelProperty.Name);
//                                                        modelProperty.SetValue(modelInstance, instanceFileType, null);
//                                                        break;
//                                                    }
//                                                case "fieldTypeHTMLFile": {
//                                                        // 
//                                                        // -- private files
//                                                        var instanceFileType = new fieldTypeHTMLFile();
//                                                        instanceFileType.filename = cs.GetFilename(modelProperty.Name);
//                                                        modelProperty.SetValue(modelInstance, instanceFileType, null);
//                                                        break;
//                                                    }

//                                                default: {
//                                                        modelProperty.SetValue(modelInstance, cs.GetText(modelProperty.Name), null);
//                                                        break;
//                                                    }
//                                            }
//                                        }

//                                        break;
//                                    }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//                throw;
//            }
//            return modelInstance;
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// save the instance properties to a record with matching id. If id is not provided, a new record is created.
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <returns></returns>
//        internal int save<T>(CPBaseClass cp) where T : Contensive.Models.Db.DbBaseModel {
//            var cs = cp.CSNew();
//            try {
//                var instanceType = GetType();
//                string contentName = derivedContentName(instanceType);
//                string tableName = derivedContentTableName(instanceType);
//                if (id > 0) {
//                    if (!cs.Open(contentName, "id=" + id)) {
//                        string message = "Unable to open record in content [" + contentName + "], with id [" + id + "]";
//                        cs.Close();
//                        id = 0;
//                        throw new ApplicationException(message);
//                    }
//                }
//                else if (!cs.Insert(contentName)) {
//                    cs.Close();
//                    id = 0;
//                    throw new ApplicationException("Unable to insert record in content [" + contentName + "]");
//                }
//                // Dim instanceType As Type = Me.GetType()
//                foreach (PropertyInfo instanceProperty in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
//                    switch (instanceProperty.Name.ToLower() ?? "") {
//                        case "id": {
//                                id = cs.GetInteger("id");
//                                break;
//                            }
//                        case "ccguid": {
//                                if (string.IsNullOrEmpty(ccguid)) {
//                                    ccguid = "{" + Guid.NewGuid().ToString() + "}";
//                                }
//                                string value;
//                                value = instanceProperty.GetValue(this, null).ToString();
//                                cs.SetField(instanceProperty.Name, value);
//                                break;
//                            }

//                        default: {

//                                // 
//                                // -- get the underlying type if this is nullable
//                                bool targetNullable = IsNullable(instanceProperty.PropertyType);
//                                string propertyValueText = cs.GetText(instanceProperty.Name);
//                                if (targetNullable & string.IsNullOrEmpty(propertyValueText)) {
//                                    // 
//                                    // -- null value in a nullable property - save a blank value to a Db field
//                                    cs.SetField(instanceProperty.Name, "");
//                                }
//                                else {
//                                    // 
//                                    // -- not nullable or value is not null
//                                    var targetType = targetNullable ? Nullable.GetUnderlyingType(instanceProperty.PropertyType) : instanceProperty.PropertyType;
//                                    switch (targetType.Name ?? "") {
//                                        case "Int32": {
//                                                int value;
//                                                int.TryParse(instanceProperty.GetValue(this, null).ToString(), out value);
//                                                cs.SetField(instanceProperty.Name, value.ToString());
//                                                break;
//                                            }
//                                        case "Boolean": {
//                                                bool value;
//                                                bool.TryParse(instanceProperty.GetValue(this, null).ToString(), out value);
//                                                cs.SetField(instanceProperty.Name, value.ToString());
//                                                break;
//                                            }
//                                        case "DateTime": {
//                                                DateTime value;
//                                                DateTime.TryParse(instanceProperty.GetValue(this, null).ToString(), out value);
//                                                cs.SetField(instanceProperty.Name, value.ToString());
//                                                break;
//                                            }
//                                        case "Double": {
//                                                double value;
//                                                double.TryParse(instanceProperty.GetValue(this, null).ToString(), out value);
//                                                cs.SetField(instanceProperty.Name, value.ToString());
//                                                break;
//                                            }
//                                        case "fieldTypeTextFile":
//                                        case "fieldTypeJavascriptFile":
//                                        case "fieldTypeCSSFile":
//                                        case "fieldTypeHTMLFile": {
//                                                int fieldTypeId = 0;
//                                                PropertyInfo contentProperty = null;
//                                                PropertyInfo contentUpdatedProperty;
//                                                bool contentUpdated;
//                                                string content = "";
//                                                switch (instanceProperty.PropertyType.Name ?? "") {
//                                                    case "fieldTypeJavascriptFile": {
//                                                            fieldTypeId = FieldTypeIdFileJavascript;
//                                                            fieldTypeJavascriptFile fileProperty = (fieldTypeJavascriptFile)instanceProperty.GetValue(this, null);
//                                                            fileProperty.internalCp = cp;
//                                                            contentProperty = instanceProperty.PropertyType.GetProperty("content");
//                                                            contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
//                                                            contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty, null);
//                                                            content = (string)contentProperty.GetValue(fileProperty, null);
//                                                            break;
//                                                        }
//                                                    case "fieldTypeCSSFile": {
//                                                            fieldTypeId = FieldTypeIdFileCSS;
//                                                            fieldTypeCSSFile fileProperty = (fieldTypeCSSFile)instanceProperty.GetValue(this, null);
//                                                            fileProperty.internalCp = cp;
//                                                            contentProperty = instanceProperty.PropertyType.GetProperty("content");
//                                                            contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
//                                                            contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty, null);
//                                                            content = (string)contentProperty.GetValue(fileProperty, null);
//                                                            break;
//                                                        }
//                                                    case "fieldTypeHTMLFile": {
//                                                            fieldTypeId = FieldTypeIdFileHTML;
//                                                            fieldTypeHTMLFile fileProperty = (fieldTypeHTMLFile)instanceProperty.GetValue(this, null);
//                                                            fileProperty.internalCp = cp;
//                                                            contentProperty = instanceProperty.PropertyType.GetProperty("content");
//                                                            contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
//                                                            contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty, null);
//                                                            content = (string)contentProperty.GetValue(fileProperty, null);
//                                                            break;
//                                                        }

//                                                    default: {
//                                                            fieldTypeId = FieldTypeIdFileText;
//                                                            fieldTypeTextFile fileProperty = (fieldTypeTextFile)instanceProperty.GetValue(this, null);
//                                                            fileProperty.internalCp = cp;
//                                                            contentProperty = instanceProperty.PropertyType.GetProperty("content");
//                                                            contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
//                                                            contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty, null);
//                                                            content = (string)contentProperty.GetValue(fileProperty, null);
//                                                            break;
//                                                        }
//                                                }
//                                                if (contentUpdated) {
//                                                    string filename = cs.GetFilename(instanceProperty.Name);
//                                                    if (string.IsNullOrEmpty(content)) {
//                                                        // 
//                                                        // -- empty content
//                                                        if (!string.IsNullOrEmpty(filename)) {
//                                                            cs.SetField(instanceProperty.Name, "");
//                                                            cp.CdnFiles.DeleteFile(filename);
//                                                        }
//                                                    }
//                                                    else {
//                                                        // 
//                                                        // -- save content
//                                                        if (string.IsNullOrEmpty(filename)) {
//                                                            filename = getUploadPath<T>(instanceProperty.Name.ToLower());
//                                                        }
//                                                        cs.SetField(instanceProperty.Name, content);
//                                                    }
//                                                }

//                                                break;
//                                            }

//                                        default: {
//                                                string value;
//                                                value = instanceProperty.GetValue(this, null).ToString();
//                                                cs.SetField(instanceProperty.Name, value);
//                                                break;
//                                            }
//                                    }


//                                }

//                                break;
//                            }
//                    }
//                }
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//                throw;
//            }
//            finally {
//                cs.Close();
//            }
//            return id;
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// delete an existing database record by id
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="recordId"></param>
//        internal static void delete<T>(CPBaseClass cp, int recordId) where T : Contensive.Models.Db.DbBaseModel {
//            try {
//                if (recordId > 0) {
//                    var instanceType = typeof(T);
//                    string contentName = derivedContentName(instanceType);
//                    string tableName = derivedContentTableName(instanceType);
//                    cp.Content.Delete(contentName, "id=" + recordId.ToString());
//                }
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//                throw;
//            }
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// delete an existing database record by guid
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="ccguid"></param>
//        internal static void delete<T>(CPBaseClass cp, string ccguid) where T : Contensive.Models.Db.DbBaseModel {
//            try {
//                if (!string.IsNullOrEmpty(ccguid)) {
//                    var instanceType = typeof(T);
//                    string contentName = derivedContentName(instanceType);
//                    var instance = create<DbModel>(cp, ccguid);
//                    if (instance is not null) {
//                        cp.Content.Delete(contentName, "(ccguid=" + cp.Db.EncodeSQLText(ccguid) + ")");
//                    }
//                }
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//                throw;
//            }
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// pattern get a list of objects from this model
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="sqlCriteria"></param>
//        /// <returns></returns>
//        internal static List<T> createList<T>(CPBaseClass cp, string sqlCriteria, string sqlOrderBy, int pageSize = 9999, int pageNumber = 1) where T : Contensive.Models.Db.DbBaseModel {
//            var result = new List<T>();
//            try {
//                var cs = cp.CSNew();
//                var ignoreCacheNames = new List<string>();
//                var instanceType = typeof(T);
//                string contentName = derivedContentName(instanceType);
//                if (cs.Open(contentName, sqlCriteria, sqlOrderBy, true, "", pageSize, pageNumber)) {
//                    T instance;
//                    do {
//                        instance = loadRecord<T>(cp, cs);
//                        if (instance is not null) {
//                            result.Add(instance);
//                        }
//                        cs.GoNext();
//                    }
//                    while (cs.OK());
//                }
//                cs.Close();
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return result;
//        }
//        // 
//        internal static List<T> createList<T>(CPBaseClass cp, string sqlCriteria) where T : Contensive.Models.Db.DbBaseModel {
//            return createList<T>(cp, sqlCriteria, "");
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// pattern get a list of objects from this model
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="sqlCriteria"></param>
//        /// <returns></returns>
//        internal static List<T> createList<T>(CPBaseClass cp, string sqlCriteria, string sqlOrderBy, int pageSize, int pageNumber, List<string> listOfLowerCaseFields = null) where T : Contensive.Models.Db.DbBaseModel {
//            var result = new List<T>();
//            try {
//                var cs = cp.CSNew();
//                var ignoreCacheNames = new List<string>();
//                var instanceType = typeof(T);
//                string contentName = derivedContentName(instanceType);
//                if (cs.Open(contentName, sqlCriteria, sqlOrderBy, true, "", pageSize, pageNumber)) {
//                    T instance;
//                    do {
//                        instance = loadRecord<T>(cp, cs, listOfLowerCaseFields);
//                        if (instance is not null) {
//                            result.Add(instance);
//                        }
//                        cs.GoNext();
//                    }
//                    while (cs.OK());
//                }
//                cs.Close();
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return result;
//        }

//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// get the name of the record by it's id
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="recordId"></param>record
//        /// <returns></returns>
//        internal static string getRecordName<T>(CPBaseClass cp, int recordId) where T : Contensive.Models.Db.DbBaseModel {
//            try {
//                if (recordId > 0) {
//                    var instanceType = typeof(T);
//                    string tableName = derivedContentTableName(instanceType);
//                    var cs = cp.CSNew();
//                    if (cs.OpenSQL("select name from " + tableName + " where id=" + recordId.ToString())) {
//                        return cs.GetText("name");
//                    }
//                    cs.Close();
//                }
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return "";
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// get the name of the record by it's guid 
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="ccGuid"></param>record
//        /// <returns></returns>
//        internal static string getRecordName<T>(CPBaseClass cp, string ccGuid) where T : Contensive.Models.Db.DbBaseModel {
//            try {
//                if (!string.IsNullOrEmpty(ccGuid)) {
//                    var instanceType = typeof(T);
//                    string tableName = derivedContentTableName(instanceType);
//                    var cs = cp.CSNew();
//                    if (cs.OpenSQL("select name from " + tableName + " where ccguid=" + cp.Db.EncodeSQLText(ccGuid))) {
//                        return cs.GetText("name");
//                    }
//                    cs.Close();
//                }
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return "";
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// get the id of the record by it's guid 
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="ccGuid"></param>record
//        /// <returns></returns>
//        internal static int getRecordId<T>(CPBaseClass cp, string ccGuid) where T : Contensive.Models.Db.DbBaseModel {
//            try {
//                if (!string.IsNullOrEmpty(ccGuid)) {
//                    var instanceType = typeof(T);
//                    string tableName = derivedContentTableName(instanceType);
//                    var cs = cp.CSNew();
//                    if (cs.OpenSQL("select id from " + tableName + " where ccguid=" + cp.Db.EncodeSQLText(ccGuid))) {
//                        return cs.GetInteger("id");
//                    }
//                    cs.Close();
//                }
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return 0;
//        }
//        // 
//        // ====================================================================================================
//        internal static int getCount<T>(CPBaseClass cp, string sqlCriteria) where T : Contensive.Models.Db.DbBaseModel {
//            int result = 0;
//            try {
//                var instanceType = typeof(T);
//                string tableName = derivedContentTableName(instanceType);
//                var cs = cp.CSNew();
//                string sql = "select count(id) as cnt from " + tableName;
//                if (!string.IsNullOrEmpty(sqlCriteria)) {
//                    sql += " where " + sqlCriteria;
//                }
//                if (cs.OpenSQL(sql)) {
//                    result = cs.GetInteger("cnt");
//                }
//                cs.Close();
//            }
//            catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return result;
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// Temporary method to create a path for an uploaded. First, try the texrt value in the field. If it is empty, use this method to create the path,
//        /// append the filename to the end and save it to the field, and save the file there. This path starts with the tablename and ends with a slash.
//        /// </summary>
//        /// <param name="fieldName"></param>
//        /// <returns></returns>
//        internal string getUploadPath<T>(string fieldName) where T : Contensive.Models.Db.DbBaseModel {
//            var instanceType = typeof(T);
//            string tableName = derivedContentTableName(instanceType);
//            return tableName.ToLower() + "/" + fieldName.ToLower() + "/" + id.ToString().PadLeft(12, '0') + "/";
//        }
//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// return true if the type is nullable
//        /// </summary>
//        /// <param name="type"></param>
//        /// <returns></returns>
//        public static bool IsNullable(Type @type) {
//            return Nullable.GetUnderlyingType(type) is not null;
//        }

//        // 
//        // ====================================================================================================
//        /// <summary>
//        /// field type to store file related fields. Copied from cpCore
//        /// </summary>
//        public abstract class fieldCdnFile {
//            // 
//            // -- 
//            // during load
//            // -- The filename is loaded into the model (blank or not). No content Is read from the file during load.
//            // -- the internalCpCore must be set
//            // 
//            // during a cache load, the internalCpCore must be set
//            // 
//            // content property read:
//            // -- If the filename Is blank, a blank Is returned
//            // -- if the filename exists, the content is read into the model and returned to the consumer
//            // 
//            // content property written:
//            // -- content is stored in the model until save(). contentUpdated is set.
//            // 
//            // filename property read: nothing special
//            // 
//            // filename property written:
//            // -- contentUpdated set true if it was previously set (content was written), or if the content is not empty
//            // 
//            // contentLoaded property means the content in the model is valid
//            // contentUpdated property means the content needs to be saved on the next save
//            // 
//            public string filename {
//                set {
//                    _filename = value;
//                    // 
//                    // -- mark content updated if the content was updated, or if the content is not blank (so old content is written to the new updated filename)
//                    contentUpdated = contentUpdated | !string.IsNullOrEmpty(_content);
//                }
//                get {
//                    return _filename;
//                }
//            }
//            private string _filename = "";
//            // 
//            // -- content in the file. loaded as needed, not during model create. 
//            public string content {
//                set {
//                    _content = value;
//                    contentUpdated = true;
//                }
//                get {
//                    if (!contentLoaded) {
//                        if (!string.IsNullOrEmpty(filename) & internalCp is not null) {
//                            contentLoaded = true;
//                            _content = internalCp.CdnFiles.Read(filename);
//                        }
//                    }
//                    return _content;
//                }
//            }
//            // 
//            // -- internal storage for content
//            private string _content { get; set; } = "";
//            // 
//            // -- When field is deserialized from cache, contentLoaded flag is used to deferentiate between unloaded content and blank conent.
//            public bool contentLoaded { get; set; } = false;
//            // 
//            // -- When content is updated, the model.save() writes the file
//            public bool contentUpdated { get; set; } = false;
//            // 
//            // -- set by load(). Used by field to read content from filename when needed
//            public CPBaseClass internalCp { get; set; } = null;
//        }

//        // 
//        public class fieldTypeTextFile : fieldCdnFile {
//        }
//        public class fieldTypeJavascriptFile : fieldCdnFile {
//        }
//        public class fieldTypeCSSFile : fieldCdnFile {
//        }
//        public class fieldTypeHTMLFile : fieldCdnFile {
//        }
//        // 
//        // -----------------------------------------------------------------------
//        // ----- Field type Definitions
//        // Field Types are numeric values that describe how to treat values
//        // stored as ContentFieldDefinitionType (FieldType property of FieldType Type.. ;)
//        // -----------------------------------------------------------------------
//        // 
//        public const int FieldTypeIdInteger = 1;       // An long number
//        public const int FieldTypeIdText = 2;          // A text field (up to 255 characters)
//        public const int FieldTypeIdLongText = 3;      // A memo field (up to 8000 characters)
//        public const int FieldTypeIdBoolean = 4;       // A yes/no field
//        public const int FieldTypeIdDate = 5;          // A date field
//        public const int FieldTypeIdFile = 6;          // A filename of a file in the files directory.
//        public const int FieldTypeIdLookup = 7;        // A lookup is a FieldTypeInteger that indexes into another table
//        public const int FieldTypeIdRedirect = 8;      // creates a link to another section
//        public const int FieldTypeIdCurrency = 9;      // A Float that prints in dollars
//        public const int FieldTypeIdFileText = 10;     // Text saved in a file in the files area.
//        public const int FieldTypeIdFileImage = 11;        // A filename of a file in the files directory.
//        public const int FieldTypeIdFloat = 12;        // A float number
//        public const int FieldTypeIdAutoIdIncrement = 13; // long that automatically increments with the new record
//        public const int FieldTypeIdManyToMany = 14;    // no database field - sets up a relationship through a Rule table to another table
//        public const int FieldTypeIdMemberSelect = 15; // This ID is a ccMembers record in a group defined by the MemberSelectGroupID field
//        public const int FieldTypeIdFileCSS = 16;      // A filename of a CSS compatible file
//        public const int FieldTypeIdFileXML = 17;      // the filename of an XML compatible file
//        public const int FieldTypeIdFileJavascript = 18; // the filename of a javascript compatible file
//        public const int FieldTypeIdLink = 19;           // Links used in href tags -- can go to pages or resources
//        public const int FieldTypeIdResourceLink = 20;   // Links used in resources, link <img or <object. Should not be pages
//        public const int FieldTypeIdHTML = 21;           // LongText field that expects HTML content
//        public const int FieldTypeIdFileHTML = 22;       // TextFile field that expects HTML content
//        public const int FieldTypeIdMax = 22;
//        // 
//        // ----- Field Descriptors for these type
//        // These are what are publicly displayed for each type
//        // See GetFieldTypeNameByType and vise-versa to translater
//        // 
//        public const string FieldTypeNameInteger = "Integer";
//        public const string FieldTypeNameText = "Text";
//        public const string FieldTypeNameLongText = "LongText";
//        public const string FieldTypeNameBoolean = "Boolean";
//        public const string FieldTypeNameDate = "Date";
//        public const string FieldTypeNameFile = "File";
//        public const string FieldTypeNameLookup = "Lookup";
//        public const string FieldTypeNameRedirect = "Redirect";
//        public const string FieldTypeNameCurrency = "Currency";
//        public const string FieldTypeNameImage = "Image";
//        public const string FieldTypeNameFloat = "Float";
//        public const string FieldTypeNameManyToMany = "ManyToMany";
//        public const string FieldTypeNameTextFile = "TextFile";
//        public const string FieldTypeNameCSSFile = "CSSFile";
//        public const string FieldTypeNameXMLFile = "XMLFile";
//        public const string FieldTypeNameJavascriptFile = "JavascriptFile";
//        public const string FieldTypeNameLink = "Link";
//        public const string FieldTypeNameResourceLink = "ResourceLink";
//        public const string FieldTypeNameMemberSelect = "MemberSelect";
//        public const string FieldTypeNameHTML = "HTML";
//        public const string FieldTypeNameHTMLFile = "HTMLFile";
//        // 
//        public const string FieldTypeNameLcaseInteger = "integer";
//        public const string FieldTypeNameLcaseText = "text";
//        public const string FieldTypeNameLcaseLongText = "longtext";
//        public const string FieldTypeNameLcaseBoolean = "boolean";
//        public const string FieldTypeNameLcaseDate = "date";
//        public const string FieldTypeNameLcaseFile = "file";
//        public const string FieldTypeNameLcaseLookup = "lookup";
//        public const string FieldTypeNameLcaseRedirect = "redirect";
//        public const string FieldTypeNameLcaseCurrency = "currency";
//        public const string FieldTypeNameLcaseImage = "image";
//        public const string FieldTypeNameLcaseFloat = "float";
//        public const string FieldTypeNameLcaseManyToMany = "manytomany";
//        public const string FieldTypeNameLcaseTextFile = "textfile";
//        public const string FieldTypeNameLcaseCSSFile = "cssfile";
//        public const string FieldTypeNameLcaseXMLFile = "xmlfile";
//        public const string FieldTypeNameLcaseJavascriptFile = "javascriptfile";
//        public const string FieldTypeNameLcaseLink = "link";
//        public const string FieldTypeNameLcaseResourceLink = "resourcelink";
//        public const string FieldTypeNameLcaseMemberSelect = "memberselect";
//        public const string FieldTypeNameLcaseHTML = "html";
//        public const string FieldTypeNameLcaseHTMLFile = "htmlfile";
//    }
//}