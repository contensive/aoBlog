
namespace Contensive.Blog.Models {
    public class LatestPostWidgetViewModel_Item {
        public string postImage { get; set; }
        public string postDate { get; set; }
        public string headline { get; set; }
        public string description { get; set; }
        public string continueURL { get; set; }
        public string editTag { get; set; }
        //
        // -- responsive image properties
        public string postImageSrc { get; set; }
        public string postImageSrcSet { get; set; }
        public string postImageSizes { get; set; }
        public int postImageWidth { get; set; }
        public int postImageHeight { get; set; }
    }
}
