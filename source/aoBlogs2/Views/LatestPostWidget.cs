using System;
using Contensive.Addons.Blog.Models;
using Contensive.Addons.Blog.Models.Db;
using Contensive.BaseClasses;
using Contensive.DesignBlockBase.Controllers;

namespace Contensive.Addons.Blog {

    public class LatestPostWidget : AddonBaseClass {

        public override object Execute(CPBaseClass cp) {
            try {
                string widgetName = "Latest News Widget";
                return DesignBlockController.renderWidget<DbLatestPostsWidgetsModel, LatestPostWidgetViewModel>(cp,
                    widgetName: widgetName, 
                    layoutGuid: "{cf55cad2-a7a4-4c74-8ffe-4327e3174372}", 
                    layoutName: "News Home Layout", 
                    layoutPathFilename: @"Watf\index.html", 
                    layoutBS5PathFilename: @"Watf\index.html");
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return "";
            }
        }
    }
}