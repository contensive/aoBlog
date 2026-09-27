
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Blog.Models.Db;
using Contensive.Models.Db;

namespace Contensive.Blog {
    public class BlogImagePlaceRemote : AddonBaseClass {
        //
        // ====================================================================================================
        //
        private const string blockTags = "p|h[1-6]|div|blockquote|ul|ol|table|figure|section|hr|pre";
        //
        public override object Execute(CPBaseClass cp) {
            try {
                int postId = cp.Doc.GetInteger("postId");
                int imageId = cp.Doc.GetInteger("imageId");
                int insertPosition = cp.Doc.GetInteger("insertPosition");
                //
                if (postId == 0 || imageId == 0) { return "ERR:invalid parameters"; }
                //
                // -- verify caller is a blog editor
                var blogPost = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (blogPost is null) { return "ERR:post not found"; }
                var blog = DbBaseModel.create<BlogModel>(cp, blogPost.blogId);
                if (blog is null) { return "ERR:blog not found"; }
                var person = DbBaseModel.create<Models.PersonModel>(cp, cp.User.Id);
                if (person is null || !person.isBlogEditor(cp, blog)) { return "ERR:not authorized"; }
                //
                // -- verify the image exists and belongs to this post
                var image = DbBaseModel.create<BlogImageModel>(cp, imageId);
                if (image is null || image.blogEntryId != postId) { return "ERR:image not found"; }
                //
                // -- get the copy, replace all [imageNNN] tags with non-block placeholders.
                // -- The dragged image gets a sentinel placeholder so its position does not
                // -- create a new block boundary (matching the frontend drop zone numbering).
                // -- Other images also get non-block placeholders for the same reason.
                string copy = blogPost.copy ?? "";
                string draggedPlaceholder = "__DRAGGEDIMG__";
                var placeholders = new Dictionary<string, string>();
                int placeholderIdx = 0;
                string expandedCopy = Regex.Replace(copy, @"\[image(\d+)\]", (m) => {
                    int tagImageId = int.Parse(m.Groups[1].Value);
                    if (tagImageId == imageId) {
                        return draggedPlaceholder;
                    }
                    string key = $"__IMGPH{placeholderIdx++}__";
                    placeholders[key] = m.Value;
                    return key;
                }, RegexOptions.IgnoreCase);
                //
                // -- insert the image tag at the requested position.
                // -- The dragged image sentinel is still in place, preventing a false
                // -- block boundary where the image was. This matches the frontend
                // -- drop zone numbering which also has no drop zone at the image location.
                string imageTag = $"[image{imageId}]";
                string resultCopy = insertImageTagAtPosition(expandedCopy, imageTag, insertPosition);
                //
                // -- remove the dragged image sentinel now that positions are resolved
                resultCopy = resultCopy.Replace(draggedPlaceholder, "");
                //
                // -- restore other image placeholders back to original [imageNNN] tags
                foreach (var kvp in placeholders) {
                    resultCopy = resultCopy.Replace(kvp.Key, kvp.Value);
                }
                //
                // -- save
                blogPost.copy = resultCopy;
                blogPost.save(cp);
                //
                return "OK";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "BlogImagePlaceRemote.Execute");
                return "ERR:server error";
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Insert an image tag at the specified block-element boundary position.
        /// Position 0 = after the first block element, position N = after the (N+1)th, last = after all content.
        /// </summary>
        private static string insertImageTagAtPosition(string copy, string imageTag, int position) {
            string pattern = $@"(<\/(?:{blockTags})\s*>)(\s*)(<(?:{blockTags})[\s>])";
            //
            var boundaries = new List<int>();
            var matches = Regex.Matches(copy, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches) {
                int insertPoint = match.Groups[1].Index + match.Groups[1].Length;
                boundaries.Add(insertPoint);
            }
            boundaries.Add(copy.Length);
            //
            int clampedPosition = Math.Max(0, Math.Min(position, boundaries.Count - 1));
            int insertIndex = boundaries[clampedPosition];
            //
            return copy.Insert(insertIndex, imageTag);
        }
    }
}
