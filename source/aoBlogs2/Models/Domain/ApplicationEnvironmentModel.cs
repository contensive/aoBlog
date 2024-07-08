
using System.Linq;
using Contensive.Blog.Controllers;
using Contensive.BaseClasses;
using Contensive.Models.Db;

namespace Contensive.Blog.Models {
    public class ApplicationEnvironmentModel {
        // 
        public CPBaseClass cp { get; private set; }
        // 
        // ====================================================================================================
        /// <summary>
        /// The user at the keyboard is editing
        /// </summary>
        /// <returns></returns>
        public bool userIsEditing {
            get {
                local_userIsEditing ??= cp.User.IsEditing();
                return (bool)local_userIsEditing;
            }
        }
        private bool? local_userIsEditing { get; set; } = default;
        // 
        // ====================================================================================================
        /// <summary>
        /// blog set during construction
        /// </summary>
        /// <returns></returns>
        public BlogModel blog {
            get {
                // 
                // -- blog set during object constructor
                return local_blog;
            }
        }
        private readonly BlogModel local_blog = null;
        // 
        // ====================================================================================================
        /// <summary>
        /// Current Blog Entry. Returns null if no valid entry
        /// </summary>
        /// <returns></returns>
        public BlogEntryModel blogPost {
            get {
                // 
                // -- return if set
                if (_blogPost is not null)
                    return _blogPost;
                // 
                // -- return null if blogentryid is not valid
                if (_blogPostId is null || (_blogPostId.HasValue ? _blogPostId.Value == 0 : (bool?)null).GetValueOrDefault())
                    return null;
                // 
                // -- blogEntryId is valid, return blog entry
                _blogPost = DbBaseModel.create<BlogEntryModel>(cp, cp.Utils.EncodeInteger(_blogPostId));
                if (_blogPost is not null) {
                    BlogEntryModel.verifyPost(cp, _blogPost);
                    return _blogPost;
                }
                // 
                // -- if blogentryid is not 0, and blog is null, remove link alias that might have caused this
                LinkAliasController.deleteLinkAlias(cp, (int)_blogPostId);
                return null;
            }
        }
        private readonly int? _blogPostId = default;
        private BlogEntryModel _blogPost = null;
        // 
        // ====================================================================================================
        /// <summary>
        /// The current user at the keyboard
        /// </summary>
        /// <returns></returns>
        public Contensive.Blog.Models.PersonModel user {
            get {
                local_user ??= PersonModel.create<PersonModel>(cp, cp.User.Id);
                return local_user;
            }
        }
        private PersonModel local_user = null;
        // 
        // ====================================================================================================
        /// <summary>
        /// link to the blogs list page
        /// </summary>
        /// <returns></returns>
        public string blogBaseLink {
            get {
                if (local_blogBaseLink is not null)
                    return local_blogBaseLink;
                local_blogBaseLink = cp.Content.GetPageLink(cp.Doc.PageId);
                return local_blogBaseLink;
            }
        }
        private string local_blogBaseLink = null;
        // 
        // ====================================================================================================
        /// <summary>
        /// RSS feed for this blog?
        /// </summary>
        /// <returns></returns>
        public RSSFeedModel rssFeed {
            get {
                if (local_RSSFeed is null) {
                    // 
                    // Get the Feed Args
                    local_RSSFeed = DbBaseModel.create<RSSFeedModel>(cp, blog.rssFeedId);
                    if (local_RSSFeed is null) {
                        local_RSSFeed = DbBaseModel.addDefault<RSSFeedModel>(cp);
                        local_RSSFeed.name = blog.caption;
                        local_RSSFeed.description = "This is your First RssFeed";
                        local_RSSFeed.save(cp);
                        blog.rssFeedId = local_RSSFeed.id;
                        blog.save(cp);
                    }
                }
                return local_RSSFeed;
            }
        }
        private RSSFeedModel local_RSSFeed = null;
        // 
        // ====================================================================================================
        // 
        public string blogPageBaseLink {
            get {
                if (local_blogPageBaseLink is not null)
                    return local_blogPageBaseLink;
                local_blogPageBaseLink = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", "");
                return local_blogPageBaseLink;
            }
        }
        private string local_blogPageBaseLink = null;
        // 
        // 
        public BlogEntryModel nextArticle {
            get {
                if (nextArticle_local is not null)
                    return nextArticle_local;
                var articleList = DbBaseModel.createList<BlogEntryModel>(cp, "(blogID=" + blog.id + ")and(dateAdded<" + cp.Db.EncodeSQLDate(cp.Utils.EncodeDate(blogPost.dateAdded)) + ")", "dateAdded desc", 1, 1);
                if (articleList.Count == 0) {
                    // 
                    // -- this may be the last article in the list, the next article should be the first to loop around
                    articleList = DbBaseModel.createList<BlogEntryModel>(cp, "(blogID=" + blog.id + ")", "dateAdded desc", 1, 1);
                    if (articleList.Count == 0)
                        return null;
                }
                nextArticle_local = articleList.First();
                return nextArticle_local;
            }
        }
        private BlogEntryModel nextArticle_local = null;
        // 
        // 
        public string nextArticleLink {
            get {
                if (nextArticle is null)
                    return null;
                // 
                string qs = LinkAliasController.getLinkAliasQueryString(cp, nextArticle.id);
                nextArticleLink_local = cp.Content.GetPageLink(cp.Doc.PageId, qs);
                return nextArticleLink_local;
            }
        }
        private string nextArticleLink_local = null;
        // 
        // 
        public string nextArticleLinkCaption {
            get {
                if (nextArticle is null)
                    return null;
                return nextArticle.name;
            }
        }

        // 
        // ====================================================================================================
        // 
        public ApplicationEnvironmentModel(CPBaseClass cp, BlogModel blog, int blogEntryId) {
            // 
            this.cp = cp;
            // 
            local_blog = blog;
            _blogPostId = blogEntryId;
        }
        // 
        // 
        /// <summary>
        /// if false, do now show tags. This blocks all the additional pages that hurt SEO
        /// </summary>
        /// <returns></returns>
        public bool sitePropertyAllowTags {
            get {
                return cp.Site.GetBoolean("blog allow tags", false);
            }
        }
    }
}