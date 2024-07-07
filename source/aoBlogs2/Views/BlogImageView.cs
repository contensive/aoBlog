
using System;
using System.Linq;
using Contensive.Blog.Models;
using Contensive.BaseClasses;

namespace Contensive.Blog.Views {
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
                    string altSizeList = blogImage.altSizeList;
                    Return_ImageDescription = blogImage.description;
                    Return_Imagename = blogImage.name;
                    Return_ThumbnailFilename = cp.Image.ResizeAndCrop(blogImage.Filename, app.blog.imageWidthMax, (int)Math.Round(app.blog.imageWidthMax * 0.75d), ref altSizeList, out bool isNewSize);
                    Return_ImageFilename = cp.Image.ResizeAndCrop(blogImage.Filename, app.blog.thumbnailImageWidth, 0, ref altSizeList, out bool isNewSize2);
                    if (isNewSize2 || isNewSize) {
                        blogImage.altSizeList = altSizeList;
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