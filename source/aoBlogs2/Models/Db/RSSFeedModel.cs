
using System;
using Contensive.BaseClasses;
using Contensive.Models.Db;

namespace Contensive.Blog.Models {
    public class RSSFeedModel : Contensive.Models.Db.DbBaseModel {
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("RSS Feeds", "ccRSSFeeds", "default", false);
        // -- const
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
                if (blog.rssFeedId > 0) {
                    rssFeed = create<RSSFeedModel>(cp, blog.rssFeedId);
                    if (rssFeed is not null)
                        return rssFeed;
                }
                rssFeed = DbBaseModel.addDefault<RSSFeedModel>(cp);
                rssFeed.copyright = "Copyright " + DateTime.Now.Year;
                rssFeed.description = "";
                rssFeed.link = "";
                rssFeed.logoFilename = "";
                rssFeed.name = "RSS Feed for Blog #" + blog.id + ", " + blog.caption;
                rssFeed.rssDateUpdated = DateTime.MinValue;
                rssFeed.rssFilename = "RSS" + rssFeed.id + ".xml";
                rssFeed.save(cp);
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