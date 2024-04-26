using System;
using Contensive.Addons.Blog.Controllers;
using Contensive.BaseClasses;

namespace Contensive.Addons.Blog.Models {
    public class BlogModel : DbModel {
        // 
        // ====================================================================================================
        // -- const
        public const string contentName = "Blogs";
        public const string contentTableName = "ccBlogs";
        private new const string contentDataSource = "default";
        // 
        // ====================================================================================================
        // -- instance properties
        public bool AllowAnonymous { get; set; }
        public bool allowArchiveList { get; set; }
        public bool allowArticleCTA { get; set; }
        public bool AllowCategories { get; set; }
        public bool allowEmailSubscribe { get; set; }
        public bool allowFacebookLink { get; set; }
        public bool allowGooglePlusLink { get; set; }
        public bool allowRSSSubscribe { get; set; }
        public bool allowSearch { get; set; }
        public bool allowTwitterLink { get; set; }
        // Public Property AuthoringGroupID As Integer
        public bool autoApproveComments { get; set; }
        public string BriefFilename { get; set; }
        public string Caption { get; set; }
        public string Copy { get; set; }
        public bool emailComment { get; set; }
        public int emailSubscribeGroupId { get; set; }
        public string facebookLink { get; set; }
        public string followUsCaption { get; set; }
        public string googlePlusLink { get; set; }
        public bool HideContributer { get; set; }
        public int ImageWidthMax { get; set; }
        public int OverviewLength { get; set; }
        public int OwnerMemberID { get; set; }
        public int postsToDisplay { get; set; }
        public bool recaptcha { get; set; }
        public int RSSFeedID { get; set; }
        public int ThumbnailImageWidth { get; set; }
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
        /// <returns></returns>
        public static BlogModel verifyBlog(CPBaseClass cp, string instanceGuid) {
            try {
                var Blog = create<BlogModel>(cp, instanceGuid);
                if (Blog is not null)
                    return Blog;
                if (!cp.User.IsAdmin)
                    return null;

                Blog = @add<BlogModel>(cp);
                Blog.name = "Default Blog";
                Blog.Caption = "The New Blog";
                Blog.Copy = "<p>This is the description of your new blog. It always appears at the top of your list of blog posts. Edit or remove this description by editing the blog features.</p>";
                Blog.OwnerMemberID = cp.User.Id;
                // Blog.AuthoringGroupID = cp.Group.GetId("Site Managers")
                Blog.AllowAnonymous = true;
                Blog.autoApproveComments = false;
                Blog.AllowCategories = true;
                Blog.postsToDisplay = 5;
                Blog.OverviewLength = 500;
                Blog.ThumbnailImageWidth = 200;
                Blog.ImageWidthMax = 400;
                Blog.allowArticleCTA = true;
                Blog.ccguid = instanceGuid;
                Blog.save<BlogModel>(cp);
                var rssFeed = RSSFeedModel.verifyFeed(cp, Blog);
                Blog.RSSFeedID = rssFeed is not null ? rssFeed.id : 0;
                Blog.save<BlogModel>(cp);

                var blogEntry = @add<BlogPostModel>(cp);
                if (blogEntry is not null) {
                    blogEntry.blogID = Blog.id;
                    blogEntry.name = "Welcome to the New Blog!";
                    blogEntry.RSSTitle = "";
                    blogEntry.copy = cp.WwwFiles.Read(@"blogs\DefaultPostCopy.txt");
                    // 
                    LinkAliasController.addLinkAlias(cp, blogEntry.name, cp.Doc.PageId, blogEntry.id);

                    // Dim qs As String = cp.Utils.ModifyQueryString("", RequestNameBlogEntryID, CStr(blogEntry.id))
                    // qs = cp.Utils.ModifyQueryString(qs, rnFormID, FormBlogPostDetails.ToString())
                    // Call cp.Site.AddLinkAlias(Blog.Caption, cp.Doc.PageId, qs)
                    // Dim LinkAlias As List(Of LinkAliasesModel) = DbModel.createList(Of LinkAliasesModel)(cp, "(pageid=" & cp.Doc.PageId & ")and(QueryStringSuffix=" & cp.Db.EncodeSQLText(qs) & ")")
                    // If (LinkAlias.Count > 0) Then
                    // Dim EntryLink As String = LinkAlias.First().name
                    // End If
                    blogEntry.RSSDescription = genericController.getBriefCopy(cp, blogEntry.copy, 150);
                    blogEntry.save<BlogPostModel>(cp);
                }
                // 
                // Add this new default post to the new feed
                // 
                var RSSFeedBlogRules = RSSFeedBlogRuleModel.@add(cp);
                if (RSSFeedBlogRules is not null) {
                    RSSFeedBlogRules.RSSFeedID = rssFeed.id;
                    RSSFeedBlogRules.BlogPostID = blogEntry.id;
                    RSSFeedBlogRules.name = "RSS Feed [" + rssFeed.name + "], Blog Post [" + blogEntry.name + "]";
                    RSSFeedBlogRules.save<BlogModel>(cp);
                }
                // 
                // Add this new Call to Action
                // 
                var callToAction = create<CallsToActionModel>(cp, constants.guidDefaultCallToAction);
                if (callToAction is null) {
                    callToAction = @add<CallsToActionModel>(cp);
                    callToAction.name = "Find Out More";
                    callToAction.link = "http://www.MemberBoss.com";
                    callToAction.headline = " Manage Your Membership Community";
                    callToAction.brief = "<p>The best all-in-one-place solution to build and manage your membership community.</p>";
                    callToAction.ccguid = constants.guidDefaultCallToAction;
                    callToAction.save<BlogModel>(cp);
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