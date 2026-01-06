using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Contensive.Blog.Controllers {
    public sealed class MetadataController {
        private MetadataController() {
        }
        //
        //=====================================================================================================
        //
        public static void addTitle(CPBaseClass cp, string title) {
            if (string.IsNullOrEmpty(title)) { return; }
            //
            cp.Doc.AddTitle(title);
        }
        //
        //=====================================================================================================
        /// <summary>
        /// geneate a meta title from 50 to 60 characters
        /// </summary>
        /// <param name="app"></param>
        /// <param name="blog"></param>
        /// <param name="cp"></param>
        /// <returns></returns>
        public static string getBlogMetaTitle(ApplicationEnvironmentModel app, BlogModel blog) {
            CPBaseClass cp = app.cp;
            string processedTitle = blog?.metaTitle ?? string.Empty;
            if(string.IsNullOrWhiteSpace(processedTitle)) {
                //
                // -- try page's meta title
                processedTitle += " " + blog.name;
            }
            if (processedTitle.Length > 60) {
                int truncatePosition = processedTitle.LastIndexOf(" ", 60);
                if (truncatePosition > 0) {
                    processedTitle = processedTitle.Substring(0, truncatePosition) + "...";
                } else {
                    // No space found before position 60, truncate at 60
                    processedTitle = processedTitle.Substring(0, 60) + "...";
                }
            }
            return processedTitle;
        }
        //
        //=====================================================================================================
        /// <summary>
        /// geneate a meta description from 120 to 160 characters
        /// </summary>
        /// <param name="app"></param>
        /// <param name="blog"></param>
        /// <param name="cp"></param>
        /// <returns></returns>
        public static string getBlogMetaDescription(ApplicationEnvironmentModel app, BlogModel blog) {
            CPBaseClass cp = app.cp;
            //
            // -- entry meta description
            string result = blog?.metaDescription ?? "";
            if (string.IsNullOrWhiteSpace(result) ) {
                //
                // -- try page's meta description
                result += " " + app.page.metaDescription;
            }
            if (result.Length > 160) {
                //
                // trim to 160 characters
                int ptr = result.LastIndexOf(" ", 160);
                if (ptr < 0)
                    ptr = 160;
                result = result.Substring(0, ptr - 1) + "...";
            }
            return result;
        }
        //
        //=====================================================================================================
        /// <summary>
        /// geneate a meta description from 120 to 160 characters
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blogEntry"></param>
        /// <returns></returns>
        public static string getEntryMetaDescription(CPBaseClass cp, BlogEntryModel blogEntry) {
            //
            // -- entry meta description
            string result = blogEntry.metaDescription ?? "";
            if (result.Length < 120) {
                //
                // -- try rss description
                result += " " + blogEntry.rssDescription;
            }
            if (result.Length < 120) {
                //
                // -- try blog copy
                result += " " + cp.Utils.ConvertHTML2Text(blogEntry.copy);
            }
            if (result.Length > 160) {
                //
                // trim to 160 characters
                int ptr = result.LastIndexOf(" ", 160);
                if (ptr < 0)
                    ptr = 160;
                result = result.Substring(0, ptr - 1) + "...";
            }
            return result;
        }
        // 
        // ====================================================================================================
        public static void setMetadata( ApplicationEnvironmentModel app, BlogModel blog,  BlogEntryModel blogEntry, List<BlogImageModel> blogImageList) {
            CPBaseClass cp = app.cp;
            // 
            cp.Utils.AppendLog("Blog.setMetadata, blogEntry.id [" + blogEntry.id + "], set Open Graph Title = blogEntry.name [" + blogEntry.name + "]");
            // 
            string blogEntryBrief = blogEntry.rssDescription;
            if (string.IsNullOrEmpty(blogEntryBrief)) {
                blogEntryBrief = cp.Utils.ConvertHTML2Text(blogEntry.copy);
                if (blogEntryBrief.Length > 300) {
                    int ptr = blogEntryBrief.IndexOf(" ", 290);
                    if (ptr < 0)
                        ptr = 300;
                    blogEntryBrief = blogEntryBrief.Substring(0, ptr - 1) + "...";
                }
            }
            // 
            // -- set article meta data
            MetadataController.addTitle(cp, MetadataController.getBlogMetaTitle(app, blog));
            cp.Doc.AddMetaDescription(MetadataController.getEntryMetaDescription(cp, blogEntry));
            cp.Doc.AddMetaKeywordList((blogEntry.metaKeywordList + "," + blogEntry.tagList).Replace(Constants.vbCrLf, ",").Replace(Constants.vbCr, ",").Replace(Constants.vbLf, ",").Replace(",,", ","));
            // 
            // -- set open graph properties modified by the Blog
            cp.Doc.SetProperty("Open Graph URL", cp.Content.GetPageLink(cp.Doc.PageId, "BlogEntryID=" + blogEntry.id + "&FormID=300"));
            cp.Doc.SetProperty("Open Graph Title", blogEntry.name);
            cp.Doc.SetProperty("Open Graph Description", blogEntryBrief);
            if (blogImageList.Count > 0) {
                if (cp.Request.Secure) {
                    cp.Doc.SetProperty("Open Graph Image", "https://" + cp.Site.Domain + cp.Http.CdnFilePathPrefix + blogImageList.First().Filename);
                } else {
                    cp.Doc.SetProperty("Open Graph Image", "http://" + cp.Site.Domain + cp.Http.CdnFilePathPrefix + blogImageList.First().Filename);
                }
            }
        }
    }
}