
using System;
using System.Linq;
using Contensive.Addons.Blog.Models;
using Contensive.BaseClasses;

namespace Contensive.Addons.Blog.Views {
    // 
    public class BlogImageView {
        // 
        // ========================================================================
        // 
        public static string getBlogImage(CPBaseClass cp, ApplicationEnvironmentModel app, BlogImageModel blogImage, ref string Return_ThumbnailFilename, ref string Return_ImageFilename, ref string Return_ImageDescription, ref string Return_Imagename) {
            string results = "";
            try {
                // 
                if (blogImage is not null) {
                    var imageSizeList = blogImage.AltSizeList.Split(',').ToList();
                    Return_ImageDescription = blogImage.description;
                    Return_Imagename = blogImage.name;
                    Return_ThumbnailFilename = cp.Image.GetBestFitWebP(blogImage.Filename, app.blog.ImageWidthMax, (int)Math.Round(app.blog.ImageWidthMax * 0.75d), imageSizeList);
                    Return_ImageFilename = cp.Image.GetBestFitWebP(blogImage.Filename, app.blog.ThumbnailImageWidth, 0, imageSizeList);
                    if ((string.Join(",", imageSizeList) ?? "") != (blogImage.AltSizeList ?? "")) {
                        blogImage.AltSizeList = string.Join(",", imageSizeList);
                        blogImage.save(cp);
                    }
                }
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return results;
        }
        // 
        // ==================================================================================
        // 
        // 
    }
}