using Contensive.Blog.Models;
using Contensive.Blog.Views;
using Contensive.BaseClasses;
using System;
using System.Data;

namespace Contensive.Blog {
    public class BlogWidget : AddonBaseClass {
        // 
        // =====================================================================================
        /// <summary>
        /// Blog Addon
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass cp) {
            try {
                // 
                // -- requests model - todo the controller for each view should handle its own requests
                var blogBodyRequest = new BlogBodyRequestModel(cp);
                var request = new Contensive.Blog.Models.View.RequestModel(cp);
                // 
                // -- get blog settings
                var blog = BlogModel.verifyBlog(cp, blogBodyRequest.instanceGuid);
                if (blog is null) {
                    if (!string.IsNullOrEmpty(blogBodyRequest.instanceGuid) && cp.User.IsAdmin && cp.User.IsEditing()) {
                        //
                        // -- admin editing, tell them if it is inactive
                        using (DataTable dt = cp.Db.ExecuteQuery($"select id from ccBlogs where (active=0)and(ccguid={cp.Db.EncodeSQLText(blogBodyRequest.instanceGuid)})")) {
                            if (dt.Rows.Count > 0) {
                                //
                                // -- admin user, inactive blog, return null
                                string result = @"<div styles=""width:40rem;padding:10px;margin:auto"">This blog is currently inactive. If it's permanently inactive, you can remove the widget. To make it active again, go to the advanced settings and find the 'active' checkbox in the control-info section.</div>";
                                //
                                // -- real hack solution until we update the designblockBase method to allow just the id number
                                //
                                cp.Db.ExecuteNonQuery($"update ccblogs set active=1 where ccguid={cp.Db.EncodeSQLText(blogBodyRequest.instanceGuid)}");
                                blog = BlogModel.verifyBlog(cp, blogBodyRequest.instanceGuid);
                                if (blog != null) {
                                    result = DesignBlockBase.Controllers.DesignBlockController.addDesignBlockEditWrapper(cp, result, blog, Models.BlogModel.tableMetadata.contentName);

                                };
                                cp.Db.ExecuteNonQuery($"update ccblogs set active=0 where ccguid={cp.Db.EncodeSQLText(blogBodyRequest.instanceGuid)}");
                                return result;
                            };
                        }
                    }
                    return "<!-- Could not find or create blog from instanceId [" + blogBodyRequest.instanceGuid + "] -->";
                }
                // 
                // -- process view requests
                switch (blogBodyRequest.srcViewId) {
                    default: {
                            break;
                        }
                        // 
                        // -- default process nothing
                }
                // 
                // -- create next view
                switch (blogBodyRequest.dstViewId) {
                    default: {
                            break;
                        }
                        // 
                        // -- default view
                }
                // 
                var app = new ApplicationEnvironmentModel(cp, blog, blogBodyRequest.entryId);
                // 
                // -- get legacy Blog Body -- the body is the area down the middle that includes the Blog View (Article View, List View, Edit View)
                string blogBody = BlogBodyView.getBlogBody(cp, app, request, blogBodyRequest);
                // 
                // -- sidebar wrapper
                string blogSidebarHtml = SidebarView.getSidebarView(cp, app, request, blogBody);
                // 
                // -- if editing enabled, add the link and wrapperwrapper
                return DesignBlockBase.Controllers.DesignBlockController.addDesignBlockEditWrapper(cp, blogSidebarHtml, blog, Models.BlogModel.tableMetadata.contentName);
                //return genericController.addEditWrapper(CP, blogSidebarHtml, blog.id, blog.name, BlogModel.tableMetadata.contentName);
            }
            // 
            catch (Exception ex) {
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
            if (string.IsNullOrWhiteSpace(instanceGuid))
                instanceGuid = "BlogWithoutinstanceId-PageId-" + cp.Doc.PageId;
            entryId = cp.Doc.GetInteger(constants.RequestNameBlogEntryID);
        }

    }
}