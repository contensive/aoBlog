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
            string result = blog?.metaTitle ?? string.Empty;
            if (string.IsNullOrWhiteSpace(result)) {
                //
                // -- try page's meta title
                result = blog.name;
            }
            result = result.Replace(Constants.vbCrLf, " ").Replace(Constants.vbCr, " ").Replace(Constants.vbLf, " ").Trim();
            return result;
        }
        //
        //=====================================================================================================
        /// <summary>
        /// geneate a meta title from 50 to 60 characters
        /// </summary>
        /// <param name="app"></param>
        /// <param name="blog"></param>
        /// <param name="blogEntry"></param>
        /// <param name="cp"></param>
        /// <returns></returns>
        public static string getEntryMetaTitle(ApplicationEnvironmentModel app, BlogModel blog, BlogEntryModel blogEntry) {
            string result = blogEntry?.metaTitle;
            if (string.IsNullOrWhiteSpace(result)) {
                //
                // -- try page's meta title
                result = blogEntry?.name;
            }
            if (string.IsNullOrWhiteSpace(result)) {
                //
                // -- try blog name
                result = getBlogMetaTitle(app, blog);
            }
            result = result.Replace(Constants.vbCrLf, " ").Replace(Constants.vbCr, " ").Replace(Constants.vbLf, " ").Trim();
            return result;
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
            if (string.IsNullOrWhiteSpace(result)) {
                //
                // -- try page's meta description
                result = app.page.metaDescription;
            }
            result = result.Replace(Constants.vbCrLf, " ").Replace(Constants.vbCr, " ").Replace(Constants.vbLf, " ").Trim();
            //if (result.Length > 160) {
            //    //
            //    // trim to 160 characters
            //    int ptr = result.LastIndexOf(" ", 160);
            //    if (ptr < 0)
            //        ptr = 160;
            //    result = result.Substring(0, ptr) + "...";
            //}
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
            if (string.IsNullOrEmpty(result)) {
                //
                // -- try rss description
                result = blogEntry.rssDescription;
            }
            if (string.IsNullOrEmpty(result)) {
                //
                // -- try blog copy
                result = cp.Utils.ConvertHTML2Text(blogEntry.copy);
            }
            result = result.Replace(Constants.vbCrLf, " ").Replace(Constants.vbCr, " ").Replace(Constants.vbLf, " ").Trim();
            //if (result.Length > 160) {
            //    //
            //    // trim to 160 characters
            //    int ptr = result.LastIndexOf(" ", 160);
            //    if (ptr < 0)
            //        ptr = 160;
            //    result = result.Substring(0, ptr - 1) + "...";
            //}
            return result;
        }
        // 
        // ====================================================================================================
        public static void setBlogMetadata(ApplicationEnvironmentModel app, BlogModel blog, int pageCount, int pageNumber, string pageOneOfTenMsg) {

            CPBaseClass cp = app.cp;
            string metaDescription = getBlogMetaDescription(app, blog);
            string metaTitle = getBlogMetaTitle(app, blog);
            if (pageNumber > 1) {
                // 
                // -- set the page title if it is page 2 or more 
                metaTitle = $"{pageOneOfTenMsg}, {metaTitle}";
                metaDescription = $"{pageOneOfTenMsg}, {metaDescription}";
            }
            // 
            // -- set article meta data
            cp.Doc.AddTitle(metaTitle);
            cp.Doc.AddMetaDescription(metaDescription);
            cp.Doc.AddMetaKeywordList((blog.metaKeywordList).Replace(Constants.vbCrLf, ",").Replace(Constants.vbCr, ",").Replace(Constants.vbLf, ",").Replace(",,", ","));
            // 
            // -- set open graph properties modified by the Blog
            //cp.Doc.SetProperty("Open Graph URL", cp.Content.GetPageLink(cp.Doc.PageId));
            cp.Doc.SetProperty("Open Graph Title", metaTitle);
            cp.Doc.SetProperty("Open Graph Description", metaDescription);
            if (!string.IsNullOrWhiteSpace(blog.defaultImageFilename.filename)) {
                string blogImageUrl =  $"{cp.Http.CdnFilePathPrefixAbsolute}{blog.defaultImageFilename.filename}";
                string encodedBlogImageUrl = _GenericController.encodeURLForHrefSrc(blogImageUrl);
                cp.Doc.SetProperty("Open Graph Image", encodedBlogImageUrl);
            }
        }
        // 
        // ====================================================================================================
        public static void setEntryMetadata(ApplicationEnvironmentModel app, BlogModel blog, BlogEntryModel blogEntry, List<BlogImageModel> blogImageList) {
            CPBaseClass cp = app.cp;
            // 
            cp.Utils.AppendLog("Blog.setEntryMetadata, blogEntry.id [" + blogEntry.id + "], set Open Graph Title = blogEntry.name [" + blogEntry.name + "]");
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
            MetadataController.addTitle(cp, MetadataController.getEntryMetaTitle(app, blog, blogEntry));
            cp.Doc.AddMetaDescription(MetadataController.getEntryMetaDescription(cp, blogEntry));
            cp.Doc.AddMetaKeywordList((blogEntry.metaKeywordList + "," + blogEntry.tagList).Replace(Constants.vbCrLf, ",").Replace(Constants.vbCr, ",").Replace(Constants.vbLf, ",").Replace(",,", ","));
            // 
            // -- set open graph properties modified by the Blog
            cp.Doc.SetProperty("Open Graph URL", cp.Content.GetPageLink(cp.Doc.PageId, "BlogEntryID=" + blogEntry.id + "&FormID=300"));
            cp.Doc.SetProperty("Open Graph Title", blogEntry.name);
            cp.Doc.SetProperty("Open Graph Description", blogEntryBrief);
            if (blogImageList.Count > 0) {
                BlogImageModel blogImage = blogImageList.First();
                string blogImageUrl = $"{cp.Http.CdnFilePathPrefixAbsolute}{blogImage.Filename}";
                string encodedBlogImageUrl = _GenericController.encodeURLForHrefSrc(blogImageUrl);
                cp.Log.Debug($"setEntryMetadata, blogImageUrl [{blogImageUrl}], encodedBlogImageUrl [{encodedBlogImageUrl}]");
                cp.Doc.SetProperty("Open Graph Image", encodedBlogImageUrl);
            }
        }
    }
}