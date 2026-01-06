

using Contensive.BaseClasses;
using Contensive.Blog.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Contensive.Blog.Controllers {
    public sealed class ImageController {
        private ImageController() {
        }
        // 
        /// <summary>
        /// Return a list of blog entry images for the blog entry
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="blogEntry"></param>
        /// <returns></returns>
        public static List<BlogImageModel> getPostImageList(CPBaseClass cp, BlogEntryModel blogEntry) {
            var result = new List<BlogImageModel>();
            try {
                if (!string.IsNullOrEmpty(blogEntry.primaryImage)) {
                    var primaryImage = new BlogImageModel {
                        blogEntryId = blogEntry.id,
                        Filename = blogEntry.primaryImage,
                        description = blogEntry.primaryImageDescription,
                        altSizeList = blogEntry.primaryImageAltSizeList
                    };
                    result.Add(primaryImage);
                }
                string sql = $@"
                    select distinct
                        i.*
                    from
	                    BlogImages i
	                    left join BlogImageRules r on r.blogimageid=i.id
                    where
	                    i.blogentryid={blogEntry.id}
	                    or (r.blogentryid={blogEntry.id})
                    order by
                        i.sortOrder, i.id
                    ";
                // create a list of blogimagemodel records for all images referenced directly and indirectly by the blog image rules
                using (DataTable dt = cp.Db.ExecuteQuery(sql)) {
                    foreach (DataRow dr in dt.Rows) {
                        var blogimage = new BlogImageModel();
                        blogimage.load<BlogImageModel>(cp, dr);
                        result.Add(blogimage);
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}