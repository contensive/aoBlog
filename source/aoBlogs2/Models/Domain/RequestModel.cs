using System;
using Contensive.BaseClasses;

namespace Contensive.Addons.Blog.Models.View {
    public class RequestModel {
        // 
        // ====================================================================================================
        // 
        private readonly CPBaseClass cp;
        // 
        // 
        public RequestModel(CPBaseClass cp) {
            this.cp = cp;

        }
        // 
        // 
        public int page {
            get {
                return cp.Doc.GetInteger(constants.rnPageNumber);
            }
        }
        // 
        // 
        public string instanceGuid {
            get {
                string instanceGuidRet = default;
                instanceGuidRet = cp.Doc.GetText("instanceId");
                if (string.IsNullOrWhiteSpace(instanceGuidRet))
                    instanceGuidRet = "BlogWithoutinstanceId-PageId-" + cp.Doc.PageId;
                return instanceGuidRet;
            }
        }
        // 
        // 
        public int srcViewId {
            get {
                return cp.Doc.GetInteger(constants.rnFormID);
            }
        }
        // 
        // 
        public int dstViewId { get; set; }
        // 
        // ====================================================================================================
        /// <summary>
        /// category Id
        /// </summary>
        /// <returns></returns>
        public int categoryId {
            get {
                if (_categoryId is null) {
                    if (!string.IsNullOrEmpty(cp.Doc.GetText(constants.RequestNameBlogCategoryIDSet))) {
                        _categoryId = cp.Doc.GetInteger(constants.RequestNameBlogCategoryIDSet);
                    }
                    else {
                        _categoryId = cp.Doc.GetInteger(constants.RequestNameBlogCategoryID);
                    }
                }
                return Convert.ToInt32(_categoryId);
            }
        }
        private int? _categoryId = default;
        // 
        // ====================================================================================================
        /// <summary>
        /// The Blog Entry Id requested
        /// </summary>
        /// <returns></returns>
        public int blogEntryId {
            get {
                if (_blogEntryId is null) {
                    _blogEntryId = cp.Doc.GetInteger(constants.RequestNameBlogEntryID);
                }
                return Convert.ToInt32(_blogEntryId);
            }
        }
        private int? _blogEntryId = default;
        // 
        // ====================================================================================================
        // 
        public string ButtonValue {
            get {
                if (_ButtonValue is null) {
                    _ButtonValue = cp.Doc.GetText("button");
                }
                return Convert.ToString(_ButtonValue);
            }
        }
        private string _ButtonValue = null;
        // 
        // ====================================================================================================
        // 
        public int FormID {
            get {
                if (_FormID is null) {
                    _FormID = cp.Doc.GetInteger(constants.rnFormID);
                }
                return Convert.ToInt32(_FormID);
            }
        }
        private int? _FormID = default;

        // 
        // ====================================================================================================
        // 
        public int SourceFormID {
            get {
                if (_SourceFormID is null) {
                    _SourceFormID = cp.Doc.GetInteger(constants.RequestNameSourceFormID);
                }
                return Convert.ToInt32(_SourceFormID);
            }
        }
        private int? _SourceFormID = default;
        // 
        // ====================================================================================================
        // 
        public string KeywordList {
            get {
                if (_KeywordList is null) {
                    _KeywordList = cp.Doc.GetText(constants.RequestNameKeywordList);
                }
                return Convert.ToString(_KeywordList);
            }
        }
        private string _KeywordList = null;
        // 
        // ====================================================================================================
        // 
        public string DateSearchText {
            get {
                if (_DateSearchText is null) {
                    _DateSearchText = cp.Doc.GetText(constants.RequestNameDateSearch);
                }
                return Convert.ToString(_DateSearchText);
            }
        }
        private string _DateSearchText = null;
        // 
        // ====================================================================================================
        // 
        public int ArchiveMonth {
            get {
                if (_ArchiveMonth is null) {
                    _ArchiveMonth = cp.Doc.GetInteger(constants.RequestNameArchiveMonth);
                }
                return Convert.ToInt32(_ArchiveMonth);
            }
        }
        private int? _ArchiveMonth = default;
        // 
        // ====================================================================================================
        // 
        public int ArchiveYear {
            get {
                if (_ArchiveYear is null) {
                    _ArchiveYear = cp.Doc.GetInteger(constants.RequestNameArchiveYear);
                }
                return Convert.ToInt32(_ArchiveYear);
            }
        }
        private int? _ArchiveYear = default;
        // 
        // ====================================================================================================
        // 
        public int EntryID {
            get {
                if (_EntryID is null) {
                    _EntryID = cp.Doc.GetInteger(constants.RequestNameBlogEntryID);
                }
                return Convert.ToInt32(_EntryID);
            }
        }
        private int? _EntryID = default;
        // 
        // ====================================================================================================
        // 
        public string BlogEntryName {
            get {
                if (_BlogEntryName is null) {
                    _BlogEntryName = cp.Doc.GetText(constants.RequestNameBlogEntryName);
                }
                return Convert.ToString(_BlogEntryName);
            }
        }
        private string _BlogEntryName = null;
        // 
        // ====================================================================================================
        // 
        public string BlogEntryCopy {
            get {
                if (_BlogEntryCopy is null) {
                    _BlogEntryCopy = cp.Doc.GetText(constants.RequestNameBlogEntryCopy);
                }
                return Convert.ToString(_BlogEntryCopy);
            }
        }
        private string _BlogEntryCopy = null;
        // 
        // ====================================================================================================
        // 
        public string BlogEntryTagList {
            get {
                if (_BlogEntryTagList is null) {
                    _BlogEntryTagList = cp.Doc.GetText(constants.RequestNameBlogEntryTagList);
                }
                return Convert.ToString(_BlogEntryTagList);
            }
        }
        private string _BlogEntryTagList = null;
        // 
        // ====================================================================================================
        // 
        public int BlogEntryCategoryId {
            get {
                if (_BlogEntryCategoryId is null) {
                    _BlogEntryCategoryId = cp.Doc.GetInteger(constants.RequestNameBlogEntryCategoryID);
                }
                return Convert.ToInt32(_BlogEntryCategoryId);
            }
        }
        private int? _BlogEntryCategoryId = default;
    }
}