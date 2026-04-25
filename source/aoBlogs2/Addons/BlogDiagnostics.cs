using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Data;

namespace Contensive.Blog {
    public class BlogDiagnostics : AddonBaseClass {
        public override object Execute(CPBaseClass cp) {
            try {
                var errors = new List<string>();
                var blogs = DbBaseModel.createList<BlogModel>(cp);
                foreach (var blog in blogs) {
                    if (blog.blogUpdateAlarmDays <= 0) { continue; }
                    DateTime? lastDate = null;
                    using (DataTable dt = cp.Db.ExecuteQuery($"SELECT TOP 1 COALESCE(datePublished, dateAdded) as lastDate FROM ccBlogCopy WHERE blogId={blog.id} AND active<>0 ORDER BY COALESCE(datePublished, dateAdded) DESC")) {
                        if (dt?.Rows != null && dt.Rows.Count > 0) {
                            object value = dt.Rows[0]["lastDate"];
                            if (value != DBNull.Value) {
                                lastDate = Convert.ToDateTime(value);
                            }
                        }
                    }
                    if (!lastDate.HasValue) { continue; }
                    if (lastDate.Value.AddDays(blog.blogUpdateAlarmDays) < DateTime.Now) {
                        errors.Add($"ERROR, {blog.name} does not have a published post within the page {blog.blogUpdateAlarmDays} day, the Blog Update Alarm Days value set in the blog.");
                    }
                }
                return errors.Count == 0 ? "ok" : string.Join("\r\n", errors);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
