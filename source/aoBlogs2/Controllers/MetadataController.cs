
using System.Linq;
using Contensive.Addons.Blog.Models;
using Contensive.BaseClasses;
using Microsoft.VisualBasic;

namespace Contensive.Addons.Blog.Controllers {
    public sealed class MetadataController {
        private MetadataController() {
        }
        // 
        // ====================================================================================================
        public static void setMetadata(CPBaseClass cp, BlogPostModel blogEntry) {
            // 
            cp.Utils.AppendLog("Blog.setMetadata, blogEntry.id [" + blogEntry.id + "], set Open Graph Title = blogEntry.name [" + blogEntry.name + "]");
            // 
            var blogImageList = BlogImageModel.createListFromBlogEntry(cp, blogEntry.id);
            string blogEntryBrief = blogEntry.RSSDescription;
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
            cp.Doc.AddTitle(!string.IsNullOrEmpty(blogEntry.metaTitle) ? blogEntry.metaTitle : blogEntry.name);
            cp.Doc.AddMetaDescription(!string.IsNullOrEmpty(blogEntry.metaDescription) ? blogEntry.metaDescription : blogEntry.name);
            cp.Doc.AddMetaKeywordList((blogEntry.metaKeywordList + "," + blogEntry.tagList).Replace(Constants.vbCrLf, ",").Replace(Constants.vbCr, ",").Replace(Constants.vbLf, ",").Replace(",,", ","));
            // 
            // -- set open graph properties modified by the Blog
            cp.Doc.SetProperty("Open Graph URL", cp.Content.GetPageLink(cp.Doc.PageId, "BlogEntryID=" + blogEntry.id + "&FormID=300"));
            cp.Doc.SetProperty("Open Graph Title", blogEntry.name);
            cp.Doc.SetProperty("Open Graph Description", blogEntryBrief);
            if (blogImageList.Count > 0) {
                if (cp.Request.Secure) {
                    cp.Doc.SetProperty("Open Graph Image", "https://" + cp.Site.Domain + cp.Http.CdnFilePathPrefix + blogImageList.First().Filename);
                }
                else {
                    cp.Doc.SetProperty("Open Graph Image", "http://" + cp.Site.Domain + cp.Http.CdnFilePathPrefix + blogImageList.First().Filename);
                }
            }
        }
    }
}