﻿using Contensive.Blog.Models;
using Contensive.Blog.Views;
using Contensive.BaseClasses;
using System;

namespace Contensive.Blog {
    public class BlogWidget : AddonBaseClass {
        // 
        // =====================================================================================
        /// <summary>
        /// Blog Addon
        /// </summary>
        /// <param name="CP"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass CP) {
            try {
                // 
                // -- requests model - todo the controller for each view should handle its own requests
                var blogBodyRequest = new BlogBodyRequestModel(CP);
                var request = new Contensive.Blog.Models.View.RequestModel(CP);
                // 
                // -- get blog settings
                var blog = BlogModel.verifyBlog(CP, blogBodyRequest.instanceGuid);
                if (blog is null) { return "<!-- Could not find or create blog from instanceId [" + blogBodyRequest.instanceGuid + "] -->"; }
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
                var app = new ApplicationEnvironmentModel(CP, blog, blogBodyRequest.entryId);
                // 
                // -- get legacy Blog Body -- the body is the area down the middle that includes the Blog View (Article View, List View, Edit View)
                string blogBody = BlogBodyView.getBlogBody(CP, app, request, blogBodyRequest);
                // 
                // -- sidebar wrapper
                string blogSidebarHtml = SidebarView.getSidebarView(CP, app, request, blogBody);
                // 
                // -- if editing enabled, add the link and wrapperwrapper
                return DesignBlockBase.Controllers.DesignBlockController.addDesignBlockEditWrapper(CP, blogSidebarHtml, blog, Models.BlogModel.tableMetadata.contentName);
                //return genericController.addEditWrapper(CP, blogSidebarHtml, blog.id, blog.name, BlogModel.tableMetadata.contentName);
            }
            // 
            catch (Exception ex) {
                CP.Site.ErrorReport(ex, "execute");
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