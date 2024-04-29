using System;
using System.Collections.Generic;
using System.Linq;
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
        public Boolean showCellTwo { get; set; }
        public Boolean showCellThree { get; set; }
        public Boolean showCellFour { get; set; }
        public string latestPostAddTag { get; set; }

        public static LatestPostWidgetViewModel create(CPBaseClass cp, DbLatestPostsWidgetsModel settings) {
            int hint = 0;

            try {
                var result = create<LatestPostWidgetViewModel>(cp, settings);
                List<BlogPostModel> latestPost = BlogPostModel.createList<BlogPostModel>(cp, "", "dateadded desc", 4, 1);
                result.mainCell = new LatestPostItemViewModel();
                result.secondCell = new LatestPostItemViewModel();
                result.thirdCell = new LatestPostItemViewModel();
                result.lastCell = new LatestPostItemViewModel();
                
                if (latestPost.Count >= 1) {
                    result.mainCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + "/" + latestPost[0].name.Trim().Replace(" ", "-"), latestPost[0].blogpostpageid);
                    result.mainCell.description = LatestPostWidgetController.limitString(cp, cp.Utils.ConvertHTML2Text(latestPost[0].copy), 175);
                    result.mainCell.postDate = latestPost[0].DateAdded.ToString("MMMM dd, yyyy");
                    result.mainCell.header = latestPost[0].name;
                    result.mainCell.LatestPostElementEditTag = cp.Content.GetEditLink("Blog Entries", latestPost[0].id, "Edit Latest Post element");

                    List<BlogImageRuleModel> imageRule = BlogImageRuleModel.createList<BlogImageRuleModel>(cp, "blogentryid = " + latestPost[0].id, "sortorder asc");
                    if(imageRule.Count > 0) {
                        BlogImageModel postImage = BlogImageModel.create<BlogImageModel>(cp, imageRule.First().BlogImageID);
                        if(postImage != null) {
                            result.mainCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(postImage.Filename, 550, 452);
                        }
                    }
                    else {
                        //there is no image rule so use the default image from the widget
                        DbLatestPostsWidgetsModel latestPostWidget = DbLatestPostsWidgetsModel.create<DbLatestPostsWidgetsModel>(cp, settings.id);
                        if (latestPostWidget != null) {
                            result.mainCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestPostWidget.defaultpostimage, 550, 452);
                        }
                    }
                }

                if (latestPost.Count >= 2) {
                    result.secondCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + "/" + latestPost[1].name.Trim().Replace(" ", "-"), latestPost[1].blogpostpageid);
                    result.secondCell.description = LatestPostWidgetController.limitString(cp, cp.Utils.ConvertHTML2Text(latestPost[1].copy), 175);
                    result.secondCell.postDate = latestPost[1].DateAdded.ToString("MMMM dd, yyyy");
                    result.secondCell.header = latestPost[1].name;
                    result.secondCell.LatestPostElementEditTag = cp.Content.GetEditLink("Blog Entries", latestPost[1].id, "Edit Latest Post element");
                    result.showCellTwo = true;
                    List<BlogImageRuleModel> imageRule = BlogImageRuleModel.createList<BlogImageRuleModel>(cp, "blogentryid = " + latestPost[1].id, "sortorder asc");
                    if (imageRule.Count > 0) {
                        BlogImageModel postImage = BlogImageModel.create<BlogImageModel>(cp, imageRule.First().BlogImageID);
                        if (postImage != null) {
                            result.secondCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(postImage.Filename, 550, 452);
                        }
                    }
                    else {
                        //there is no image rule so use the default image from the widget
                        DbLatestPostsWidgetsModel latestPostWidget = DbLatestPostsWidgetsModel.create<DbLatestPostsWidgetsModel>(cp, settings.id);
                        if (latestPostWidget != null) {
                            result.secondCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestPostWidget.defaultpostimage, 550, 452);
                        }
                    }                    
                }
                else {
                    result.showCellTwo = false;
                }

                if (latestPost.Count >= 3) {
                    result.thirdCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + "/" + latestPost[2].name.Trim().Replace(" ", "-"), latestPost[2].blogpostpageid);
                    result.thirdCell.description = LatestPostWidgetController.limitString(cp, cp.Utils.ConvertHTML2Text(latestPost[2].copy), 175);
                    result.thirdCell.postDate = latestPost[2].DateAdded.ToString("MMMM dd, yyyy");
                    result.thirdCell.header = latestPost[2].name;
                    result.thirdCell.LatestPostElementEditTag = cp.Content.GetEditLink("Blog Entries", latestPost[2].id, "Edit Latest Post element");
                    result.showCellThree = true;
                    List<BlogImageRuleModel> imageRule = BlogImageRuleModel.createList<BlogImageRuleModel>(cp, "blogentryid = " + latestPost[2].id, "sortorder asc");
                    if (imageRule.Count > 0) {
                        BlogImageModel postImage = BlogImageModel.create<BlogImageModel>(cp, imageRule.First().BlogImageID);
                        if (postImage != null) {
                            result.thirdCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(postImage.Filename, 550, 452);
                        }
                        if (string.IsNullOrEmpty(postImage.Filename)) {
                            //there is no image rule so use the default image from the widget
                            DbLatestPostsWidgetsModel latestPostWidget = DbLatestPostsWidgetsModel.create<DbLatestPostsWidgetsModel>(cp, settings.id);
                            if (latestPostWidget != null) {
                                result.thirdCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestPostWidget.defaultpostimage, 550, 452);
                            }
                        }
                    }
                    else {
                        //there is no image rule so use the default image from the widget
                        DbLatestPostsWidgetsModel latestPostWidget = DbLatestPostsWidgetsModel.create<DbLatestPostsWidgetsModel>(cp, settings.id);
                        if (latestPostWidget != null) {
                            result.thirdCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestPostWidget.defaultpostimage, 550, 452);
                        }
                    }
                }
                else {
                    result.showCellThree = false;
                }

                if (latestPost.Count >= 4) {
                    result.lastCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + "/" + latestPost[3].name.Trim().Replace(" ", "-"), latestPost[3].blogpostpageid);
                    result.lastCell.description = LatestPostWidgetController.limitString(cp, cp.Utils.ConvertHTML2Text(latestPost[3].copy), 175);
                    result.lastCell.postDate = latestPost[3].DateAdded.ToString("MMMM dd, yyyy");
                    result.lastCell.header = latestPost[3].name;
                    result.lastCell.LatestPostElementEditTag = cp.Content.GetEditLink("Blog Entries", latestPost[3].id, "Edit Latest Post element");
                    result.showCellFour = true;
                    List<BlogImageRuleModel> imageRule = BlogImageRuleModel.createList<BlogImageRuleModel>(cp, "blogentryid = " + latestPost[3].id, "sortorder asc");
                    if (imageRule.Count > 0) {
                        BlogImageModel postImage = BlogImageModel.create<BlogImageModel>(cp, imageRule.First().BlogImageID);
                        if (postImage != null) {
                            result.lastCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(postImage.Filename, 550, 452);
                        }
                        if(string.IsNullOrEmpty(postImage.Filename)) {
                            //there is no image rule so use the default image from the widget
                            DbLatestPostsWidgetsModel latestPostWidget = DbLatestPostsWidgetsModel.create<DbLatestPostsWidgetsModel>(cp, settings.id);
                            if (latestPostWidget != null) {
                                result.lastCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestPostWidget.defaultpostimage, 550, 452);
                            }
                        }
                    }
                    else {
                        //there is no image rule so use the default image from the widget
                        DbLatestPostsWidgetsModel latestPostWidget = DbLatestPostsWidgetsModel.create<DbLatestPostsWidgetsModel>(cp, settings.id);
                        if (latestPostWidget != null) {
                            result.lastCell.postImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestPostWidget.defaultpostimage, 550, 452);
                        }
                    }
                }
                else {
                    result.showCellFour = false;
                }

                //result.latestPostAddTag = cp.User.IsEditing("") ? cp.Content.GetAddLink("Blog Entries") : "";
                return result;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport("Error in LatestNewsWidgetViewModel create hint = : " + hint + " " + ex);
                return new LatestPostWidgetViewModel();
            }
        }
    }
}
