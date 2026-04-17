using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Blog.Models.View;
using Contensive.Blog.Views;
using System;

namespace Contensive.Blog {
    public class OnInstall : AddonBaseClass {
        // 
        // =====================================================================================
        /// <summary>
        /// Blog Addon
        /// </summary>
        /// <param name="CP"></param>
        /// <returns></returns>
        private const int codeVersion = 1;
        //
        public override object Execute(CPBaseClass CP) {
            try {
                //
                CP.Log.Info("Blog OnInstall");
                //
                // -- versioned upgrades
                int siteVersion = CP.Site.GetInteger("Blog Version");
                if (siteVersion < 1) {
                    // -- v1: set blogupdatealarmdays to 30 for all existing blogs
                    CP.Db.ExecuteNonQuery("update ccBlogs set blogupdatealarmdays=30 where (blogupdatealarmdays is null or blogupdatealarmdays=0)");
                    siteVersion = 1;
                }
                CP.Site.SetProperty("Blog Version", siteVersion.ToString());
                //
                // -- non-versioned updates
                //
                // -- update layouts
                CP.Layout.updateLayout(constants.layoutGuidBlogArchiveDateListView, constants.layoutNameBlogArchiveDateListView, constants.layoutPathFilenameBlogArchiveDateListView);
                CP.Layout.updateLayout(constants.layoutGuidBlogArchivedPostsView, constants.layoutNameBlogArchivedPostsView, constants.layoutPathFilenameBlogArchivedPostsView);
                CP.Layout.updateLayout(constants.layoutGuidBlogArticleView, constants.layoutNameBlogArticleView, constants.layoutPathFilenameBlogArticleView);
                CP.Layout.updateLayout(constants.layoutGuidBlogEditView, constants.layoutNameBlogEditView, constants.layoutPathFilenameBlogEditView);
                CP.Layout.updateLayout(constants.layoutGuidBlog, constants.layoutNameBlog, constants.layoutPathFilenameBlog);
                CP.Layout.updateLayout(constants.layoutGuidBlogListView, constants.layoutNameBlogListView, constants.layoutPathFilenameBlogListView);
                CP.Layout.updateLayout(constants.layoutGuidBlogPostCell, constants.layoutNameBlogPostCell, constants.layoutPathFilenameBlogPostCell);
                CP.Layout.updateLayout(constants.layoutGuidBlogSearchView, constants.layoutNameBlogSearchView, constants.layoutPathFilenameBlogSearchView);
                CP.Layout.updateLayout(constants.layoutGuidLastestPosts, constants.layoutNameLastestPosts, constants.layoutPathFilenameLastestPosts);
                //
                CP.Db.ExecuteNonQuery("delete from ccaggregatefunctions where ccguid='{656E95EA-2799-45CD-9712-D4CEDF0E2D02}'");
                CP.Db.ExecuteNonQuery("delete from ccaggregatefunctions where ccguid='{534B4D50-B894-42BC-AB42-7AF6EC6265DE}'");
                //
                int contentId = CP.Content.GetID("blogs");
                CP.Db.ExecuteNonQuery($"update ccfields set MemberSelectGroupID={CP.Content.GetRecordID("groups", "Site Managers")} where contentid={contentId} and name='authormemberid'");
                //
                CP.Db.ExecuteNonQuery("delete from ccfields where name='boaddedupuserid'");
                //
                contentId = CP.Content.GetID("blog comments");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='authormemberid'");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='formkey'");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='commentsreference'");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='blogcategoryid'");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='anonymous'");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='allowcomments'");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='viewings'");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='primaryimagepositionid'");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='articleprimaryimagepositionid'");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='imagedisplaytypeid'");
                //
                contentId = CP.Content.GetID("blog entries");
                CP.Db.ExecuteNonQuery($"delete from ccfields where contentid={contentId} and name='anonymous'");
                //
                return "";
            }
            // 
            catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                throw;
            }
        }

    }
}