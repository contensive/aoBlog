
using System;

namespace Contensive.Addons.Blog.Models {
    public class BlogPostModel : DbModel {        // <------ set set model Name and everywhere that matches this string
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Blog Entries";      // <------ set content name
        public const string contentTableName = "ccBlogCopy";   // <------ set to tablename for the primary content (used for cache names)
        private new const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        /// <summary>
        /// if true, comments appear on this article
        /// </summary>
        /// <returns></returns>
        public bool AllowComments { get; set; } = false;
        /// <summary>
        /// Determines how the primary image is displayed 1 = per stylesheet, 2 = right, 3 = left, 4 = hide
        /// </summary>
        public int primaryImagePositionId { get; set; }
        /// <summary>
        /// The user who last published this article.
        /// </summary>
        /// <returns></returns>
        public int AuthorMemberID { get; set; }
        /// <summary>
        /// Use for categorization. Like Tags but restricted
        /// </summary>
        /// <returns></returns>
        public int blogCategoryID { get; set; }
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
        public int Viewings { get; set; }
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
        public DateTime RSSDateExpire { get; set; }
        public DateTime RSSDatePublish { get; set; }
        public string RSSDescription { get; set; }
        public string RSSLink { get; set; }
        public string RSSTitle { get; set; }
        /// <summary>
        /// Add a video or audio file to the article. The RSS feed will create it as a podcast enclosure.
        /// </summary>
        /// <returns></returns>
        public string PodcastMediaLink { get; set; }
        public int PodcastSize { get; set; }
        // 
        public int blogID { get; set; }
        /// <summary>
        /// if provided, this is the public date displayed. Else, the dateAdded is used
        /// </summary>
        /// <returns></returns>
        public DateTime? datePublished { get; set; }
        public string header { get; set; }
        public int blogpostpageid { get; set; }
    }
}