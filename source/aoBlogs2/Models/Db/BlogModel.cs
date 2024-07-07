using Contensive.Blog.Controllers;
using Contensive.BaseClasses;
using Contensive.DesignBlockBase.Models.Db;
using Contensive.Models.Db;
using System;

namespace Contensive.Blog.Models {
    public class BlogModel : SettingsBaseModel {
        // 
        // ====================================================================================================
        public static DbBaseTableMetadataModel tableMetadata { get; private set; } = new DbBaseTableMetadataModel("Blogs", "ccBlogs", "default", false);
        // -- const
        // 
        // ====================================================================================================
        // -- instance properties
        public bool allowAnonymous { get; set; }
        public bool allowArchiveList { get; set; }
        public bool allowArticleCTA { get; set; }
        public bool allowCategories { get; set; }
        public bool allowEmailSubscribe { get; set; }
        public bool allowFacebookLink { get; set; }
        public bool allowGooglePlusLink { get; set; }
        public bool allowRSSSubscribe { get; set; }
        public bool allowSearch { get; set; }
        public bool allowTwitterLink { get; set; }
        // Public Property AuthoringGroupID As Integer
        public bool autoApproveComments { get; set; }
        public string briefFilename { get; set; }
        public string caption { get; set; }
        public string copy { get; set; }
        public bool emailComment { get; set; }
        public int emailSubscribeGroupId { get; set; }
        public string facebookLink { get; set; }
        public string followUsCaption { get; set; }
        public string googlePlusLink { get; set; }
        public bool hideContributer { get; set; }
        public int imageWidthMax { get; set; }
        public int overviewLength { get; set; }
        public int ownerMemberId { get; set; }
        public int postsToDisplay { get; set; }
        public bool recaptcha { get; set; }
        public int rssFeedId { get; set; }
        public int thumbnailImageWidth { get; set; }
        public string twitterLink { get; set; }
        /// <summary>
        /// Meta data for the landing page of the blog
        /// </summary>
        /// <returns></returns>
        public string metaTitle { get; set; }
        public string metaDescription { get; set; }
        public string metaKeywordList { get; set; }
        // 
        // ====================================================================================================
        /// <summary>
        /// Create a new default blog, ready to use. Must be an administrator. If not, returns null
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="instanceGuid"></param>
        /// <returns></returns>
        public static BlogModel verifyBlog(CPBaseClass cp, string instanceGuid) {
            try {
                var Blog = create<BlogModel>(cp, instanceGuid);
                if (Blog is not null)
                    return Blog;
                if (!cp.User.IsAdmin)
                    return null;

                Blog =  DbBaseModel.addDefault<BlogModel>(cp);
                Blog.name = "Default Blog";
                Blog.caption = "The New Blog";
                Blog.copy = "<p>This is the description of your new blog. It always appears at the top of your list of blog posts. Edit or remove this description by editing the blog features.</p>";
                Blog.ownerMemberId = cp.User.Id;
                // Blog.AuthoringGroupID = cp.Group.GetId("Site Managers")
                Blog.allowAnonymous = true;
                Blog.autoApproveComments = false;
                Blog.allowCategories = true;
                Blog.postsToDisplay = 5;
                Blog.overviewLength = 500;
                Blog.thumbnailImageWidth = 200;
                Blog.imageWidthMax = 400;
                Blog.allowArticleCTA = true;
                Blog.ccguid = instanceGuid;
                Blog.save(cp);
                var rssFeed = RSSFeedModel.verifyFeed(cp, Blog);
                Blog.rssFeedId = rssFeed is not null ? rssFeed.id : 0;
                Blog.save(cp);

                var blogEntry = DbBaseModel.addDefault<BlogEntryModel>(cp);
                if (blogEntry is not null) {
                    blogEntry.blogId = Blog.id;
                    blogEntry.name = "Welcome to the New Blog!";
                    blogEntry.rssTitle = "";
                    blogEntry.copy = cp.WwwFiles.Read(@"blogs\DefaultPostCopy.txt");
                    // 
                    LinkAliasController.addLinkAlias(cp, blogEntry.name, blogEntry.id);

                    // Dim qs As String = cp.Utils.ModifyQueryString("", RequestNameBlogEntryID, CStr(blogEntry.id))
                    // qs = cp.Utils.ModifyQueryString(qs, rnFormID, FormBlogPostDetails.ToString())
                    // Call cp.Site.AddLinkAlias(Blog.Caption, cp.Doc.PageId, qs)
                    // Dim LinkAlias As List(Of LinkAliasModel) = DbBaseModel.createList(Of LinkAliasModel)(cp, "(pageid=" & cp.Doc.PageId & ")and(QueryStringSuffix=" & cp.Db.EncodeSQLText(qs) & ")")
                    // If (LinkAlias.Count > 0) Then
                    // Dim EntryLink As String = LinkAlias.First().name
                    // End If
                    blogEntry.rssDescription = _GenericController.getBriefCopy(cp, blogEntry.copy, 150);
                    blogEntry.save(cp);
                }
                // 
                // Add this new default post to the new feed
                // 
                var RSSFeedBlogRules = RSSFeedBlogRuleModel.@add(cp);
                if (RSSFeedBlogRules is not null) {
                    RSSFeedBlogRules.RSSFeedID = rssFeed.id;
                    RSSFeedBlogRules.BlogPostID = blogEntry.id;
                    RSSFeedBlogRules.name = "RSS Feed [" + rssFeed.name + "], Blog Post [" + blogEntry.name + "]";
                    RSSFeedBlogRules.save(cp);
                }
                // 
                // Add this new Call to Action
                // 
                var callToAction = create<CallsToActionModel>(cp, constants.guidDefaultCallToAction);
                if (callToAction is null) {
                    callToAction = DbBaseModel.addDefault<CallsToActionModel>(cp);
                    callToAction.name = "Find Out More";
                    callToAction.link = "http://www.MemberBoss.com";
                    callToAction.headline = " Manage Your Membership Community";
                    callToAction.brief = "<p>The best all-in-one-place solution to build and manage your membership community.</p>";
                    callToAction.ccguid = constants.guidDefaultCallToAction;
                    callToAction.save(cp);
                }
                // 
                RSSFeedModel.UpdateBlogFeed(cp);
                // 
                return Blog;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw new ApplicationException("Exception creating default blog");
            }
        }
    }
}