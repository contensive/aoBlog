
using Contensive.BaseClasses;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;

namespace Contensive.Blog.Models {
    public class BlogEntryModel : Contensive.Models.Db.DbBaseModel { 
        //
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blog Entries", "ccBlogCopy", "default", false);
        // -- const 
        // ====================================================================================================
        // -- instance properties
        /// <summary>
        /// if true, comments appear on this article
        /// </summary>
        /// <returns></returns>
        public bool allowComments { get; set; } = false;
        /// <summary>
        /// Determines how the primary image is displayed 1 = per stylesheet, 2 = right, 3 = left, 4 = hide
        /// </summary>
        public int primaryImagePositionId { get; set; }
        /// <summary>
        /// The user who last published this article.
        /// </summary>
        /// <returns></returns>
        public int authorMemberId { get; set; }
        /// <summary>
        /// Use for categorization. Like Tags but restricted
        /// </summary>
        /// <returns></returns>
        public int blogCategoryId { get; set; }
        /// <summary>
        /// The article text
        /// </summary>
        /// <returns></returns>
        public string copy { get; set; }
        /// <summary>
        /// comma delimited tag list
        /// </summary>
        /// <returns></returns>
        public string tagList { get; set; }
        /// <summary>
        /// number of times this article page is viewed
        /// </summary>
        /// <returns></returns>
        public int viewings { get; set; }
        /// <summary>
        /// Meta data for the page
        /// </summary>
        /// <returns></returns>
        public string metaTitle { get; set; }
        public string metaDescription { get; set; }
        public string metaKeywordList { get; set; }
        /// <summary>
        /// RSS data for the RSS feed
        /// </summary>
        /// <returns></returns>
        public DateTime rssDateExpire { get; set; }
        public DateTime rssDatePublish { get; set; }
        public string rssDescription { get; set; }
        public string rssLink { get; set; }
        public string rssTitle { get; set; }
        /// <summary>
        /// Add a video or audio file to the article. The RSS feed will create it as a podcast enclosure.
        /// </summary>
        /// <returns></returns>
        public string podcastMediaLink { get; set; }
        public int podcastSize { get; set; }
        // 
        public int blogId { get; set; }
        /// <summary>
        /// if provided, this is the public date displayed. Else, the dateAdded is used
        /// </summary>
        /// <returns></returns>
        public DateTime? datePublished { get; set; }
        public int blogpostpageid { get; set; }
        //
        // ==================================================================================================== 
        //
        public static void verifyPost(CPBaseClass cp, BlogEntryModel post) {
            if (post is null) { return; }
            if (post.dateAdded == null) {
                post.dateAdded = DateTime.Now;
                post.save(cp);
            }
            if (post.datePublished == null) {
                post.datePublished = post.dateAdded;
                post.save(cp);
            }
        }
        //
        // ==================================================================================================== 
        /// <summary>
        /// Return a list of Archive Blog Copy
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blogId">The id of the Blog Copy</param>
        /// <returns></returns>
        public static List<ArchiveDateModel> createArchiveListFromBlogCopy(CPBaseClass cp, int blogId) {
            var result = new List<ArchiveDateModel>();
            try {
                // result = createList(cp, "(BlogID=" & blogId & ")", "year(dateAdded) desc, Month(dateAdded) desc")
                string SQL = "SELECT DISTINCT month(dateAdded) as archiveMonth, year(dateAdded) as archiveYear" + " From ccBlogCopy" + " Where (ContentControlID = " + cp.Content.GetID(constants.cnBlogEntries) + ") And (Active <> 0)" + " AND (BlogID=" + blogId + ")" + " ORDER BY year(dateAdded) desc, month(dateAdded) desc ";



                var cs = cp.CSNew();
                if (cs.OpenSQL(SQL)) {
                    do {
                        var archiveDate = new ArchiveDateModel();
                        archiveDate.Month = cs.GetInteger("archiveMonth");
                        archiveDate.Year = cs.GetInteger("archiveYear");
                        result.Add(archiveDate);
                        cs.GoNext();
                    }
                    while (cs.OK());
                }

                cs.Close();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}