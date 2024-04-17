using System;
using System.Collections.Generic;
using Contensive.Addons.Blog.Controllers;
using Contensive.Addons.Blog.Models;
using Contensive.Addons.Blog.Models.Db;
using Contensive.Addons.Blog.View_Models;
using Contensive.BaseClasses;
using Contensive.DesignBlockBase.Models.View;

namespace Contensive.Addons.Blog {

    public class LatestPostWidgetViewModel : DesignBlockViewBaseModel {

        public LatestPostItemViewModel mainCell { get; set; }
        public LatestPostItemViewModel secondCell { get; set; }
        public LatestPostItemViewModel thirdCell { get; set; }
        public LatestPostItemViewModel lastCell { get; set; }
        public string latestPostAddTag { get; set; }

        public static LatestPostWidgetViewModel create(CPBaseClass cp, DbLatestPostsWidgetsModel settings) {
            int hint = 0;

            try {
                var result = create<LatestPostWidgetViewModel>(cp, settings);
                List<BlogPostModel> latestPost = BlogPostModel.createList<BlogPostModel>(cp, "", "dateadded desc", 4, 1);
                int newsPageId = cp.Site.GetInteger("latest post widget pageid");
                result.mainCell = new LatestPostItemViewModel();
                result.secondCell = new LatestPostItemViewModel();
                result.thirdCell = new LatestPostItemViewModel();
                result.lastCell = new LatestPostItemViewModel();

                if (latestPost.Count >= 1) {
                    LinkAliasController.addLinkAlias(cp, latestPost[0].name, newsPageId, latestPost[0].id);
                    result.mainCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + latestPost[0].name.Trim().Replace(" ", "-"), newsPageId);
                    result.mainCell.description = LatestPostWidgetController.limitString(cp, cp.Utils.ConvertHTML2Text(latestPost[0].copy), 175);
                    result.mainCell.postDate = latestPost[0].DateAdded.ToString("MMMM dd, yyyy");
                    result.mainCell.header = latestPost[0].header;
                    result.mainCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestPost[0].blogImage, 550, 452);
                    result.mainCell.LatestPostElementEditTag = cp.Content.GetEditLink("Blog Entries", latestPost[0].id, "Edit Latest Post element");
                }

                if (latestPost.Count >= 2) {
                    LinkAliasController.addLinkAlias(cp, latestPost[1].name, newsPageId, latestPost[1].id);
                    result.secondCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + latestPost[1].name.Trim().Replace(" ", "-"), newsPageId);
                    result.secondCell.description = LatestPostWidgetController.limitString(cp, cp.Utils.ConvertHTML2Text(latestPost[1].copy), 175);
                    result.secondCell.postDate = latestPost[1].DateAdded.ToString("MMMM dd, yyyy");
                    result.secondCell.header = latestPost[1].header;
                    result.secondCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestPost[1].blogImage, 191, 148);
                    result.secondCell.LatestPostElementEditTag = cp.Content.GetEditLink("Blog Entries", latestPost[1].id, "Edit Latest Post element");
                }

                if (latestPost.Count >= 3) {
                    LinkAliasController.addLinkAlias(cp, latestPost[2].name, newsPageId, latestPost[2].id);
                    result.thirdCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + latestPost[2].name.Trim().Replace(" ", "-"), newsPageId);
                    result.thirdCell.description = LatestPostWidgetController.limitString(cp, cp.Utils.ConvertHTML2Text(latestPost[2].copy), 175);
                    result.thirdCell.postDate = latestPost[2].DateAdded.ToString("MMMM dd, yyyy");
                    result.thirdCell.header = latestPost[2].header;
                    result.thirdCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestPost[2].blogImage, 191, 148);
                    result.thirdCell.LatestPostElementEditTag = cp.Content.GetEditLink("Blog Entries", latestPost[2].id, "Edit Latest Post element");
                }

                if (latestPost.Count >= 4) {
                    LinkAliasController.addLinkAlias(cp, latestPost[3].name, newsPageId, latestPost[3].id);
                    result.lastCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + latestPost[3].name.Trim().Replace(" ", "-"), newsPageId);
                    result.lastCell.description = LatestPostWidgetController.limitString(cp, cp.Utils.ConvertHTML2Text(latestPost[3].copy), 175);
                    result.lastCell.postDate = latestPost[3].DateAdded.ToString("MMMM dd, yyyy");
                    result.lastCell.header = latestPost[3].header;
                    result.lastCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestPost[3].blogImage, 191, 148);
                    result.lastCell.LatestPostElementEditTag = cp.Content.GetEditLink("Blog Entries", latestPost[3].id, "Edit Latest Post element");
                }

                result.latestPostAddTag = cp.User.IsEditing("") ? cp.Content.GetAddLink("Blog Entries") : "";
                return result;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport("Error in LatestNewsWidgetViewModel create hint = : " + hint + " " + ex);
                return new LatestPostWidgetViewModel();
            }
        }
    }
}
