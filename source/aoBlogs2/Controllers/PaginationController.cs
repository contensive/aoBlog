
using System;
using System.Text;
using Contensive.BaseClasses;

namespace Contensive.Blog.Controllers {
    // 
    public class PaginationController {
        // 
        // ====================================================================================
        /// <summary>
        /// return the html required for pagination
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordsPerPage"></param>
        /// <param name="pageNumberCurrent"></param>
        /// <param name="recordsOnThisPage"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public static string getRecordPagination(CPBaseClass cp, int recordsPerPage, int pageNumberCurrent, int recordsOnThisPage, int recordCount) {
            try {
                var result = new StringBuilder();
                string basePageUrl = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", "");
                if (pageNumberCurrent > 1)
                    result.Append("<li class=\"page-item\"><a class=\"page-link\" href=\"" + getPageUrl(cp, basePageUrl, pageNumberCurrent - 1) + "\">Previous</a></li>");
                int recordTop = (pageNumberCurrent - 1) * recordsPerPage;
                int pageCount = (int)Math.Round(Math.Truncate(recordCount / (double)recordsPerPage + 0.999d));
                int pageFirst = pageNumberCurrent - 3;
                if (pageFirst < 1)
                    pageFirst = 1;
                int pageLast = pageFirst + 6;
                if (pageLast > pageCount)
                    pageLast = pageCount;
                if (pageCount > 1) {
                    for (int pageNumber = pageFirst, loopTo = pageLast; pageNumber <= loopTo; pageNumber++) {
                        var pageUrl = getPageUrl(cp, basePageUrl, pageNumber);
                        var htmlClass = (pageUrl==cp.Request.PathPage) ? "page-item active" : "page-item";
                        result.Append($"<li class=\"{htmlClass}\"><a class=\"page-link\" href=\"{pageUrl}\">{pageNumber}</a></li>");
                    }
                }
                if (pageCount > pageNumberCurrent) {
                    result.Append($"<li class=\"page-item\"><a class=\"page-link\" href=\"{getPageUrl(cp, basePageUrl, pageNumberCurrent + 1)}\">Next</a></li>");
                }
                // 
                return $"<nav><ul class=\"pagination\">{result}</ul></nav>";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        // 
        public static string getPageUrl(CPBaseClass cp, string basePageUrl, int pageNumber) {
            if (pageNumber < 2) {
                return basePageUrl;
            }
            string alias = $"{basePageUrl}/page/{pageNumber}";
            string qs = $"page={pageNumber}";
            cp.Site.AddLinkAlias(alias, cp.Doc.PageId, qs);
            return cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, qs, alias);
        }
    }
}