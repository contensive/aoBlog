using Contensive.Blog.Models;
using Contensive.BaseClasses;
using Contensive.DesignBlockBase.Controllers;
using System;

namespace Contensive.Blog {
    public class BlogWidget : AddonBaseClass {
        //
        // =====================================================================================
        /// <summary>
        /// Blog Addon - uses the renderWidget pattern with mustache layout
        /// </summary>
        public override object Execute(CPBaseClass cp) {
            try {
                string widgetName = "Blog";
                return DesignBlockController.renderWidget<BlogModel, BlogWidgetViewModel>(cp,
                    widgetName: widgetName,
                    layoutGuid: constants.layoutGuidBlog,
                    layoutName: constants.layoutNameBlog,
                    layoutPathFilename: constants.layoutPathFilenameBlog,
                    layoutBS5PathFilename: constants.layoutPathFilenameBlog);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "execute");
                return "<p>The blog is not currently available.</p>";
            }
        }
    }
    //
    public class BlogBodyRequestModel {
        public string instanceGuid { get; set; }
        public int srcViewId { get; set; }
        public int dstViewId { get; set; }
        public int entryId { get; set; }
        //
        public BlogBodyRequestModel(CPBaseClass cp) {
            srcViewId = cp.Doc.GetInteger(constants.rnFormID);
            instanceGuid = cp.Doc.GetText("instanceId");
            if (string.IsNullOrWhiteSpace(instanceGuid)) {
                instanceGuid = $"BlogWithoutinstanceId-PageId-{cp.Doc.PageId}";
            }
            entryId = cp.Doc.GetInteger(constants.RequestNameBlogEntryID);
        }
    }
}
