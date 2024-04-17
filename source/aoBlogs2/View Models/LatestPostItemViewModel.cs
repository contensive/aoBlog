using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contensive.Addons.Blog.View_Models {
    public class LatestPostItemViewModel {
        public string postImage { get; set; }
        public string postDate { get; set; }
        public string header { get; set; }
        public string description { get; set; }
        public string continueURL { get; set; }
        public string LatestPostElementEditTag { get; set; }
    }
}
