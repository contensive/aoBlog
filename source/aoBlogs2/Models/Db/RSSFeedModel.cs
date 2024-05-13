
using System;
using Contensive.BaseClasses;

namespace Contensive.Addons.Blog.Models {
    public class RSSFeedModel : DbModel {
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "RSS Feeds";      // <------ set content name
        public const string contentTableName = "ccRSSFeeds";   // <------ set to tablename for the primary content (used for cache names)
        private  const string contentDataSource = "default";             // <------ set to datasource if not default
        // 
        // ====================================================================================================
        // -- instance properties
        public string copyright { get; set; }
        public string description { get; set; }
        public string link { get; set; }
        public string logoFilename { get; set; }
        public DateTime rssDateUpdated { get; set; }
        public string rssFilename { get; set; }
        // 
        // ========================================================================
        // Verify the RSSFeed and return the ID and the Link
        // run when a new blog is created
        // run on every post
        // ========================================================================
        // 
        internal static RSSFeedModel verifyFeed(CPBaseClass cp, BlogModel blog) {
            try {
                RSSFeedModel rssFeed;
                if (blog.RSSFeedID > 0) {
                    rssFeed = create<RSSFeedModel>(cp, blog.RSSFeedID);
                    if (rssFeed is not null)
                        return rssFeed;
                }
                rssFeed = @add<RSSFeedModel>(cp);
                rssFeed.copyright = "Copyright " + DateTime.Now.Year;
                rssFeed.description = "";
                rssFeed.link = "";
                rssFeed.logoFilename = "";
                rssFeed.name = "RSS Feed for Blog #" + blog.id + ", " + blog.Caption;
                rssFeed.rssDateUpdated = DateTime.MinValue;
                rssFeed.rssFilename = "RSS" + rssFeed.id + ".xml";
                rssFeed.save<RSSFeedModel>(cp);
                return rssFeed;
            }

            // '
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return null;
            }
        }
        // 
        // ====================================================================================================
        /// <summary>
        /// Update Blog
        /// </summary>
        /// <param name="cp"></param>
        public static void UpdateBlogFeed(CPBaseClass cp) {
            // 
            try {
                cp.Addon.Execute(constants.RSSProcessAddonGuid);
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
        }

        // 
    }
}