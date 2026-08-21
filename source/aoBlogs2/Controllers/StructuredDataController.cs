
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Blog.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contensive.Blog.Controllers {
    public sealed class StructuredDataController {
        private StructuredDataController() {
        }
        //
        //=====================================================================================================
        /// <summary>
        /// Add BlogPosting JSON-LD structured data to the page head.
        /// Called from BlogArticleViewModel.create() alongside setEntryMetadata().
        /// </summary>
        public static void addBlogPostingJsonLd(CPBaseClass cp, ApplicationEnvironmentModel app, BlogModel blog, BlogEntryModel blogEntry, List<BlogImageModel> blogImageList) {
            try {
                //
                // -- headline (required by schema.org BlogPosting)
                string headline = blogEntry.name ?? "";
                if (string.IsNullOrWhiteSpace(headline)) { return; }
                //
                // -- description
                string description = blogEntry.metaDescription ?? "";
                if (string.IsNullOrEmpty(description)) {
                    description = blogEntry.rssDescription ?? "";
                }
                if (string.IsNullOrEmpty(description)) {
                    description = cp.Utils.ConvertHTML2Text(blogEntry.copy ?? "");
                    if (description.Length > 300) {
                        int ptr = description.IndexOf(" ", 290);
                        if (ptr < 0) { ptr = 300; }
                        description = description.Substring(0, ptr) + "...";
                    }
                }
                //
                // -- dates (ISO 8601)
                DateTime datePublished = (blogEntry.datePublished ?? blogEntry.dateAdded) ?? DateTime.Now;
                DateTime dateModified = (blogEntry.modifiedDate ?? datePublished);
                //
                // -- article URL (must be absolute for JSON-LD)
                string qs = $"BlogEntryID={blogEntry.id}&FormID=300";
                string articleUrl = cp.Content.GetPageLink(cp.Doc.PageId, qs);
                if (!articleUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                    articleUrl = $"https://{cp.Site.DomainPrimary}{articleUrl}";
                }
                //
                // -- image
                string imageUrl = "";
                if (blogImageList != null && blogImageList.Count > 0) {
                    BlogImageModel blogImage = blogImageList.First();
                    if (!string.IsNullOrEmpty(blogImage.Filename)) {
                        imageUrl = $"{cp.Http.CdnFilePathPrefixAbsolute}{blogImage.Filename}";
                    }
                }
                if (string.IsNullOrEmpty(imageUrl) && blog.defaultImageFilename != null && !string.IsNullOrWhiteSpace(blog.defaultImageFilename.filename)) {
                    imageUrl = $"{cp.Http.CdnFilePathPrefixAbsolute}{blog.defaultImageFilename.filename}";
                }
                //
                // -- author
                string authorName = "";
                if (blogEntry.authorMemberId > 0) {
                    var author = Contensive.Models.Db.DbBaseModel.create<Models.PersonModel>(cp, blogEntry.authorMemberId);
                    if (author != null) {
                        authorName = author.name ?? "";
                    }
                }
                //
                // -- publisher (site-wide properties with fallbacks)
                string publisherName = cp.Site.GetText("Structured Data Publisher Name", "");
                if (string.IsNullOrEmpty(publisherName)) {
                    publisherName = blog.name ?? cp.Site.DomainPrimary;
                }
                string publisherLogoUrl = cp.Site.GetText("Structured Data Publisher Logo", "");
                //
                // -- keywords
                string keywords = (blogEntry.tagList ?? "").Trim();
                //
                // -- build JSON-LD
                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append($"\"@context\":\"https://schema.org\"");
                sb.Append($",\"@type\":\"BlogPosting\"");
                sb.Append($",\"headline\":{jsonEncode(headline)}");
                if (!string.IsNullOrEmpty(description)) {
                    sb.Append($",\"description\":{jsonEncode(description)}");
                }
                sb.Append($",\"datePublished\":\"{datePublished:yyyy-MM-ddTHH:mm:ssZ}\"");
                sb.Append($",\"dateModified\":\"{dateModified:yyyy-MM-ddTHH:mm:ssZ}\"");
                sb.Append($",\"url\":{jsonEncode(articleUrl)}");
                sb.Append(",\"mainEntityOfPage\":{\"@type\":\"WebPage\",\"@id\":");
                sb.Append(jsonEncode(articleUrl));
                sb.Append("}");
                if (!string.IsNullOrEmpty(imageUrl)) {
                    sb.Append($",\"image\":{jsonEncode(imageUrl)}");
                }
                if (!string.IsNullOrEmpty(authorName)) {
                    sb.Append(",\"author\":{\"@type\":\"Person\",\"name\":");
                    sb.Append(jsonEncode(authorName));
                    sb.Append("}");
                }
                sb.Append(",\"publisher\":{\"@type\":\"Organization\",\"name\":");
                sb.Append(jsonEncode(publisherName));
                if (!string.IsNullOrEmpty(publisherLogoUrl)) {
                    sb.Append(",\"logo\":{\"@type\":\"ImageObject\",\"url\":");
                    sb.Append(jsonEncode(publisherLogoUrl));
                    sb.Append("}");
                }
                sb.Append("}");
                if (!string.IsNullOrEmpty(keywords)) {
                    sb.Append($",\"keywords\":{jsonEncode(keywords)}");
                }
                sb.Append("}");
                //
                cp.Doc.AddHeadTag($"<script type=\"application/ld+json\">{sb}</script>");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
        }
        //
        //=====================================================================================================
        /// <summary>
        /// Encode a string as a JSON string value (with surrounding quotes).
        /// Escapes backslash, double-quote, and control characters per RFC 8259.
        /// </summary>
        private static string jsonEncode(string value) {
            if (value == null) { return "\"\""; }
            var sb = new StringBuilder(value.Length + 2);
            sb.Append('"');
            foreach (char c in value) {
                switch (c) {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ') {
                            sb.Append($"\\u{(int)c:X4}");
                        } else {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
