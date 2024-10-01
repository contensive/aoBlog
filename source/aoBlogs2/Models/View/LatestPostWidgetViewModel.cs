using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Contensive.Blog.Models.Db;
using Contensive.DesignBlockBase.Models.View;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Policy;

namespace Contensive.Blog.Models {

    public class LatestPostWidgetViewModel : DesignBlockViewBaseModel {
        /// <summary>
        /// The newest post, displays on one side of the view
        /// </summary>
        public LatestPostWidgetViewModel_Item mainPost { get; set; }
        /// <summary>
        /// list of the other posts, list down the other side of the vew
        /// </summary>
        public List<LatestPostWidgetViewModel_Item> postList { get; set; }
        /// <summary>
        /// in editing mode. enable edit tags and wrappers
        /// </summary>
        public bool isEditing { get; set; }
        public bool hasPostList { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// create the data model to populate the layout
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static LatestPostWidgetViewModel create(CPBaseClass cp, LatestPostsWidgetModel settings) {
            int hint = 0;

            try {
                var result = create<LatestPostWidgetViewModel>(cp, settings);
                List<BlogEntryModel> latestPosts = BlogEntryModel.createList<BlogEntryModel>(cp, $"COALESCE(datePublished,dateAdded)<{cp.Db.EncodeSQLDate(DateTime.Now)}", "COALESCE(datePublished,dateAdded) desc", 4, 1);
                if (latestPosts.Count > 0) {
                    BlogEntryModel.verifyPost(cp, latestPosts[0]);
                    result.hasPostList = true;
                    result.isEditing = cp.User.IsEditing();
                    result.mainPost = createCell(cp, settings, latestPosts.First());
                    result.postList = [];
                    foreach (var post in latestPosts.Skip(1)) {
                        BlogEntryModel.verifyPost(cp, post);
                        result.postList.Add(createCell(cp, settings, post));
                    }
                }
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport("Error in LatestNewsWidgetViewModel create hint = : " + hint + " " + ex);
                return new LatestPostWidgetViewModel();
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the cell properties
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="settings"></param>
        /// <param name="post"></param>

        private static LatestPostWidgetViewModel_Item createCell(CPBaseClass cp, LatestPostsWidgetModel settings, BlogEntryModel post) {
            string qs = $"blogentryid={post.id}&formid=300";
            string continueURL = cp.Content.GetLinkAliasByPageID(post.blogpostpageid, qs, "");
            continueURL = (continueURL == "/") ? "" : continueURL;
            LatestPostWidgetViewModel_Item cell = new() {
                continueURL = continueURL,
                description = LatestPostWidgetController.limitString(cp, cp.Utils.ConvertHTML2Text(post.copy), 175),
                postDate = (post.datePublished is null) ? cp.Utils.EncodeDate(post.dateAdded).ToString("MMMM d, yyyy") : cp.Utils.EncodeDate(post.datePublished).ToString("MMMM d, yyyy"),
                headline = post.name,
                editTag = cp.Content.GetEditLink("Blog Entries", post.id, "Edit Latest Post")
            };

            List<BlogImageRuleModel> imageRules = BlogImageRuleModel.createList<BlogImageRuleModel>(cp, "blogentryid = " + post.id, "sortorder asc");
            if (imageRules.Count > 0 && imageRules.First().BlogImageID > 0) {
                BlogImageModel postImage = BlogImageModel.create<BlogImageModel>(cp, imageRules.First().BlogImageID);
                if (postImage?.Filename != null) {
                    cell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.ResizeAndCrop(postImage.Filename, 550, 452);
                    return cell;
                }
                else {
                    var blog = DbBaseModel.create<BlogModel>(cp, post.blogId);
                    if (blog != null) {
                        cell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.ResizeAndCrop(blog.defaultImageFilename.filename, 550, 452);
                        return cell;
                    }                    
                }
            }
            if (string.IsNullOrEmpty(cell.postImage) && !string.IsNullOrEmpty(settings.defaultpostimage)) {
                //there is no image rule so use the default image from the widget
                cell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.ResizeAndCrop(settings.defaultpostimage, 550, 452);
                return cell;
            }
            if (string.IsNullOrEmpty(cell.postImage)) {
                //there is no image rule so use the default image from the widget
                cell.postImage = cp.Http.CdnFilePathPrefix + constants.defaultImageUrl;
            }
            return cell;
        }
    }
}
