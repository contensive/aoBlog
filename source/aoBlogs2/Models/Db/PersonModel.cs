
using System;
using System.Collections.Generic;
using Contensive.BaseClasses;

namespace Contensive.Addons.Blog.Models {
    public class PersonModel : DbModel, ICloneable {
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "People";      // <------ set content name
        public const string contentTableName = "ccMembers";   // <------ set to tablename for the primary content (used for cache names)
        private new const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties

        public string Address { get; set; }
        public string Address2 { get; set; }
        public bool Admin { get; set; }
        public int AdminMenuModeID { get; set; }
        public bool AllowBulkEmail { get; set; }
        public bool AllowToolsPanel { get; set; }
        // Public Property authorInfoLink As String
        public bool AutoLogin { get; set; }
        public string BillAddress { get; set; }
        public string BillAddress2 { get; set; }
        public string BillCity { get; set; }
        public string BillCompany { get; set; }
        public string BillCountry { get; set; }
        public string BillEmail { get; set; }
        public string BillFax { get; set; }
        public string BillName { get; set; }
        public string BillPhone { get; set; }
        public string BillState { get; set; }
        public string BillZip { get; set; }
        public int BirthdayDay { get; set; }
        public int BirthdayMonth { get; set; }
        public int BirthdayYear { get; set; }
        public string City { get; set; }
        public string Company { get; set; }
        public string Country { get; set; }
        public bool CreatedByVisit { get; set; }
        public DateTime DateExpires { get; set; }
        public bool Developer { get; set; }
        public string Email { get; set; }
        public bool ExcludeFromAnalytics { get; set; }
        public string Fax { get; set; }
        public string FirstName { get; set; }
        public string ImageFilename { get; set; }
        public int LanguageID { get; set; }
        public string LastName { get; set; }
        public DateTime LastVisit { get; set; }
        public string nickName { get; set; }
        public string NotesFilename { get; set; }
        public int OrganizationID { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string ResumeFilename { get; set; }
        public string ShipAddress { get; set; }
        public string ShipAddress2 { get; set; }
        public string ShipCity { get; set; }
        public string ShipCompany { get; set; }
        public string ShipCountry { get; set; }
        public string ShipName { get; set; }
        public string ShipPhone { get; set; }
        public string ShipState { get; set; }
        public string ShipZip { get; set; }
        public string State { get; set; }
        // Public Property StyleFilename As String
        public string ThumbnailFilename { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public int Visits { get; set; }
        public string Zip { get; set; }


        // 
        // ====================================================================================================
        public static PersonModel @add(CPBaseClass cp) {
            return @add<PersonModel>(cp);
        }
        // 
        // ====================================================================================================
        public static PersonModel create(CPBaseClass cp, int recordId) {
            return create<PersonModel>(cp, recordId);
        }
        // 
        // ====================================================================================================
        public static PersonModel create(CPBaseClass cp, string recordGuid) {
            return create<PersonModel>(cp, recordGuid);
        }
        // 
        // ====================================================================================================
        public static PersonModel createByName(CPBaseClass cp, string recordName) {
            return createByName<PersonModel>(cp, recordName);
        }
        // 
        // ====================================================================================================
        public void save(CPBaseClass cp) {
            save<PersonModel>(cp);
        }
        // 
        // ====================================================================================================
        public static void delete(CPBaseClass cp, int recordId) {
            delete<PersonModel>(cp, recordId);
        }
        // 
        // ====================================================================================================
        public static void delete(CPBaseClass cp, string ccGuid) {
            delete<PersonModel>(cp, ccGuid);
        }
        // 
        // ====================================================================================================
        public static List<PersonModel> createList(CPBaseClass cp, string sqlCriteria, string sqlOrderBy = "id") {
            return createList<PersonModel>(cp, sqlCriteria, sqlOrderBy);
        }
        // 
        // ====================================================================================================
        public static string getRecordName(CPBaseClass cp, int recordId) {
            return getRecordName<PersonModel>(cp, recordId);
        }
        // 
        // ====================================================================================================
        public static string getRecordName(CPBaseClass cp, string ccGuid) {
            return getRecordName<PersonModel>(cp, ccGuid);
        }
        // 
        // ====================================================================================================
        public static int getRecordId(CPBaseClass cp, string ccGuid) {
            return getRecordId<PersonModel>(cp, ccGuid);
        }
        // 
        // ====================================================================================================
        public static int getCount(CPBaseClass cp, string sqlCriteria) {
            return getCount<PersonModel>(cp, sqlCriteria);
        }
        // 
        // ====================================================================================================
        public string getUploadPath(string fieldName) {
            return getUploadPath<PersonModel>(fieldName);
        }
        // 
        // ====================================================================================================
        // 
        public PersonModel Clone(CPBaseClass cp) {
            PersonModel result = (PersonModel)Clone();
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
        /// true if the person can edit the blog
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blog"></param>
        /// <returns></returns>
        public bool isBlogEditor(CPBaseClass cp, BlogModel blog) {
            int blogAuthorsGroupId = cp.Group.GetId("Blog Authors");
            if (blogAuthorsGroupId == 0) {
                cp.Group.Add("Blog Authors");
                blogAuthorsGroupId = cp.Group.GetId("Blog Authors");
            }
            return cp.User.IsAuthenticated & (id.Equals(blog.OwnerMemberID) || Admin || cp.User.IsInGroupList(blogAuthorsGroupId.ToString(), id));
        }
    }
}