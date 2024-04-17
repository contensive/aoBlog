using System;
using Contensive.Addons.Blog.Models;
using Contensive.Addons.Blog.Models.Db;
using Contensive.BaseClasses;
using Contensive.DesignBlockBase.Controllers;

namespace Contensive.Addons.Blog {

    public class LatestPostWidget : AddonBaseClass {

        public override object Execute(CPBaseClass cp) {
            try {
                string widgetName = "Latest Posts Widget";
                return DesignBlockController.renderWidget<DbLatestPostsWidgetsModel, LatestPostWidgetViewModel>(cp,
                    widgetName: widgetName, 
                    layoutGuid: "{987cb36b-22f8-4896-a54e-aa7dbab98f93}", 
                    layoutName: "Latest Posts Layout", 
                    layoutPathFilename: @"aoBlog\LatestPostLayout.html", 
                    layoutBS5PathFilename: @"aoBlog\LatestPostLayout.html");
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return "";
            }
        }
    }
}