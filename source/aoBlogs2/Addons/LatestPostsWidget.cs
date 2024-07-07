using Contensive.BaseClasses;
using Contensive.Blog.Models.Db;
using Contensive.DesignBlockBase.Controllers;
using System;

namespace Contensive.Blog {

    public class LatestPostsWidget : AddonBaseClass {

        public override object Execute(CPBaseClass cp) {
            try {
                string widgetName = "Latest Posts Widget";
                return DesignBlockController.renderWidget<DbLatestPostsWidgetsModel, Models.LatestPostWidgetViewModel>(cp,
                    widgetName: widgetName, 
                    layoutGuid: constants.layoutGuidLastestPosts, 
                    layoutName: constants.layoutNameLastestPosts, 
                    layoutPathFilename: constants.layoutPathFilenameLastestPosts, 
                    layoutBS5PathFilename: @"aoBlog\LatestPostLayout.html");
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return "";
            }
        }
    }
}