
using Contensive.BaseClasses;
using Contensive.Blog.Controllers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Contensive.Blog.Models.View {
    public class BlogArchiveDateListViewModel {
        //
        // -- header
        public string archiveHeader { get; set; }
        //
        // -- archive date links (multiple archives)
        public bool hasDateList { get; set; }
        public List<ArchiveDateItemViewModel> archiveDateList { get; set; }
        //
        // -- single archive (rendered BlogArchivedPostsView)
        public bool hasSingleArchive { get; set; }
        public string singleArchiveHtml { get; set; }
        //
        // -- no archives
        public bool hasNoArchives { get; set; }
        //
        // -- back link
        public string backToRecentLink { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Create the BlogArchiveDateListViewModel. Extracts rendering logic from ArchiveView.getFormBlogArchiveDateList().
        /// </summary>
        public static BlogArchiveDateListViewModel create(CPBaseClass cp, ApplicationEnvironmentModel app, RequestModel request) {
            try {
                var result = new BlogArchiveDateListViewModel();
                var blog = app.blog;
                result.backToRecentLink = app.blogPageBaseLink;
                //
                result.archiveHeader = cp.Content.GetCopy($"Blogs Archives Header for {blog.name}", $"<h1>{blog.name} Archives</h1>");
                //
                var archiveDateList = BlogEntryModel.createArchiveListFromBlogCopy(cp, blog.id);
                if (archiveDateList.Count == 0) {
                    result.hasNoArchives = true;
                } else if (archiveDateList.Count == 1) {
                    //
                    // -- single archive, render archived posts directly
                    result.hasSingleArchive = true;
                    var archivedPostsVm = BlogArchivedPostsViewModel.create(cp, app, request);
                    result.singleArchiveHtml = cp.Mustache.Render(cp.Layout.GetLayout(constants.layoutGuidBlogArchivedPostsView, constants.layoutNameBlogArchivedPostsView, constants.layoutPathFilenameBlogArchivedPostsView), archivedPostsVm);
                } else {
                    //
                    // -- multiple archives, show date list
                    result.hasDateList = true;
                    result.archiveDateList = new List<ArchiveDateItemViewModel>();
                    string qs = cp.Utils.ModifyQueryString(app.blogBaseLink, constants.RequestNameSourceFormID, constants.FormBlogArchiveDateList.ToString());
                    qs = cp.Utils.ModifyQueryString(qs, constants.rnFormID, constants.FormBlogArchivedBlogs.ToString());
                    foreach (var archiveDate in archiveDateList) {
                        int archiveMonth = archiveDate.Month;
                        int archiveYear = archiveDate.Year;
                        string nameOfMonth = DateAndTime.MonthName(archiveMonth);
                        string dateQs = cp.Utils.ModifyQueryString(qs, constants.RequestNameArchiveMonth, archiveMonth.ToString());
                        dateQs = cp.Utils.ModifyQueryString(dateQs, constants.RequestNameArchiveYear, archiveYear.ToString());
                        result.archiveDateList.Add(new ArchiveDateItemViewModel {
                            monthName = nameOfMonth,
                            year = archiveYear.ToString(),
                            url = dateQs
                        });
                    }
                }
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "BlogArchiveDateListViewModel.create");
                return new BlogArchiveDateListViewModel();
            }
        }
    }
}
