
using Contensive.Addons.Mcp.Shared;
using Contensive.BaseClasses;
using Contensive.Blog.Models;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
//
namespace Contensive.Addons.Blog {
    //
    /// <summary>
    /// MCP extension addon that provides AI-callable tools for managing blog content.
    /// Called by the MCP server with mcpToolName and mcpArguments set as doc properties.
    /// </summary>
    public class BlogMcpTools : AddonBaseClass {
        //
        // -- tool name constants
        private const string ToolBlogList = "blog_list";
        private const string ToolBlogUpdate = "blog_update";
        private const string ToolBlogPostList = "blog_post_list";
        private const string ToolBlogPostGet = "blog_post_get";
        private const string ToolBlogPostCreate = "blog_post_create";
        private const string ToolBlogPostUpdate = "blog_post_update";
        private const string ToolBlogPostDelete = "blog_post_delete";
        private const string ToolBlogPostImageList = "blog_post_image_list";
        private const string ToolBlogPostImageAdd = "blog_post_image_add";
        private const string ToolBlogPostImageUpdate = "blog_post_image_update";
        private const string ToolBlogPostImageDelete = "blog_post_image_delete";
        private const string ToolBlogPostImagePlace = "blog_post_image_place";
        //
        // ====================================================================================================
        //
        public override object Execute(CPBaseClass cp) {
            try {
                string toolName = cp.Doc.GetText("mcpToolName");
                if (string.IsNullOrEmpty(toolName)) {
                    return McpResponseHelper.Error(cp, "This addon is intended for MCP tool calls only.");
                }
                if (!cp.User.IsAdmin) {
                    return McpResponseHelper.Error(cp, "Blog MCP tools require admin access.");
                }
                string argsJson = cp.Doc.GetText("mcpArguments");
                var args = string.IsNullOrEmpty(argsJson)
                    ? new Dictionary<string, object>()
                    : cp.JSON.Deserialize<Dictionary<string, object>>(argsJson)
                      ?? new Dictionary<string, object>();
                //
                switch (toolName) {
                    case ToolBlogList:
                        return blogList(cp, args);
                    case ToolBlogUpdate:
                        return blogUpdate(cp, args);
                    case ToolBlogPostList:
                        return blogPostList(cp, args);
                    case ToolBlogPostGet:
                        return blogPostGet(cp, args);
                    case ToolBlogPostCreate:
                        return blogPostCreate(cp, args);
                    case ToolBlogPostUpdate:
                        return blogPostUpdate(cp, args);
                    case ToolBlogPostDelete:
                        return blogPostDelete(cp, args);
                    case ToolBlogPostImageList:
                        return blogPostImageList(cp, args);
                    case ToolBlogPostImageAdd:
                        return blogPostImageAdd(cp, args);
                    case ToolBlogPostImageUpdate:
                        return blogPostImageUpdate(cp, args);
                    case ToolBlogPostImageDelete:
                        return blogPostImageDelete(cp, args);
                    case ToolBlogPostImagePlace:
                        return blogPostImagePlace(cp, args);
                    default:
                        return McpResponseHelper.Error(cp, $"Unknown tool: {toolName}");
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Internal error processing blog MCP tool.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_list
        // ====================================================================================================
        //
        private string blogList(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int pageSize = McpResponseHelper.GetIntArg(args, "pageSize", 50);
                int pageNumber = McpResponseHelper.GetIntArg(args, "pageNumber", 1);
                //
                var blogs = DbBaseModel.createList<BlogModel>(cp, "(active<>0)", "name", pageSize, pageNumber);
                var result = blogs.Select(b => new {
                    blogId = b.id,
                    name = b.name,
                    caption = b.caption,
                    postsToDisplay = b.postsToDisplay,
                    allowCategories = b.allowCategories,
                    allowSearch = b.allowSearch,
                    allowEmailSubscribe = b.allowEmailSubscribe
                }).ToList();
                return McpResponseHelper.Success(cp, new { blogs = result, pageSize, pageNumber }, $"Found {result.Count} blog(s)");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error listing blogs.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_update
        // ====================================================================================================
        //
        private string blogUpdate(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int blogId = McpResponseHelper.GetIntArg(args, "blogId");
                if (blogId == 0) {
                    return McpResponseHelper.Error(cp, "blogId is required.");
                }
                var blog = DbBaseModel.create<BlogModel>(cp, blogId);
                if (blog == null) {
                    return McpResponseHelper.Error(cp, $"Blog #{blogId} not found.");
                }
                //
                // -- capture undo before changes
                var fieldsBefore = new Dictionary<string, string> {
                    ["caption"] = blog.caption ?? "",
                    ["copy"] = blog.copy ?? "",
                    ["allowAnonymous"] = blog.allowAnonymous ? "1" : "0",
                    ["allowCategories"] = blog.allowCategories ? "1" : "0",
                    ["allowEmailSubscribe"] = blog.allowEmailSubscribe ? "1" : "0",
                    ["allowSearch"] = blog.allowSearch ? "1" : "0",
                    ["allowArchiveList"] = blog.allowArchiveList ? "1" : "0",
                    ["autoApproveComments"] = blog.autoApproveComments ? "1" : "0",
                    ["postsToDisplay"] = blog.postsToDisplay.ToString(),
                    ["overviewLength"] = blog.overviewLength.ToString(),
                    ["metaTitle"] = blog.metaTitle ?? "",
                    ["metaDescription"] = blog.metaDescription ?? "",
                    ["metaKeywordList"] = blog.metaKeywordList ?? "",
                    ["imageWidthMax"] = blog.imageWidthMax.ToString()
                };
                McpUndoHelper.Capture(cp, Contensive.Blog.constants.cnBlogs, blog.id, blog.ccguid, ToolBlogUpdate, fieldsBefore);
                //
                // -- apply updates for any provided fields
                if (args.ContainsKey("caption")) { blog.caption = McpResponseHelper.GetStringArg(args, "caption"); }
                if (args.ContainsKey("copy")) { blog.copy = McpResponseHelper.GetStringArg(args, "copy"); }
                if (args.ContainsKey("allowAnonymous")) { blog.allowAnonymous = McpResponseHelper.GetBoolArg(args, "allowAnonymous"); }
                if (args.ContainsKey("allowCategories")) { blog.allowCategories = McpResponseHelper.GetBoolArg(args, "allowCategories"); }
                if (args.ContainsKey("allowEmailSubscribe")) { blog.allowEmailSubscribe = McpResponseHelper.GetBoolArg(args, "allowEmailSubscribe"); }
                if (args.ContainsKey("allowSearch")) { blog.allowSearch = McpResponseHelper.GetBoolArg(args, "allowSearch"); }
                if (args.ContainsKey("allowArchiveList")) { blog.allowArchiveList = McpResponseHelper.GetBoolArg(args, "allowArchiveList"); }
                if (args.ContainsKey("autoApproveComments")) { blog.autoApproveComments = McpResponseHelper.GetBoolArg(args, "autoApproveComments"); }
                if (args.ContainsKey("postsToDisplay")) { blog.postsToDisplay = McpResponseHelper.GetIntArg(args, "postsToDisplay"); }
                if (args.ContainsKey("overviewLength")) { blog.overviewLength = McpResponseHelper.GetIntArg(args, "overviewLength"); }
                if (args.ContainsKey("metaTitle")) { blog.metaTitle = McpResponseHelper.GetStringArg(args, "metaTitle"); }
                if (args.ContainsKey("metaDescription")) { blog.metaDescription = McpResponseHelper.GetStringArg(args, "metaDescription"); }
                if (args.ContainsKey("metaKeywordList")) { blog.metaKeywordList = McpResponseHelper.GetStringArg(args, "metaKeywordList"); }
                if (args.ContainsKey("imageWidthMax")) { blog.imageWidthMax = McpResponseHelper.GetIntArg(args, "imageWidthMax"); }
                //
                blog.save(cp);
                return McpResponseHelper.Success(cp, new { blogId = blog.id, name = blog.name }, "Blog updated successfully.");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error updating blog.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_post_list
        // ====================================================================================================
        //
        private string blogPostList(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int pageSize = McpResponseHelper.GetIntArg(args, "pageSize", 50);
                int pageNumber = McpResponseHelper.GetIntArg(args, "pageNumber", 1);
                int blogId = McpResponseHelper.GetIntArg(args, "blogId");
                int categoryId = McpResponseHelper.GetIntArg(args, "categoryId");
                int authorId = McpResponseHelper.GetIntArg(args, "authorId");
                string searchKeyword = McpResponseHelper.GetStringArg(args, "searchKeyword");
                //
                var criteria = new List<string> { "(active<>0)" };
                if (blogId > 0) {
                    criteria.Add($"(blogId={blogId})");
                }
                if (categoryId > 0) {
                    criteria.Add($"(blogCategoryId={categoryId})");
                }
                if (authorId > 0) {
                    criteria.Add($"(authorMemberId={authorId})");
                }
                if (!string.IsNullOrEmpty(searchKeyword)) {
                    string encoded = cp.Db.EncodeSQLText(searchKeyword);
                    criteria.Add($"(name like '%' + {encoded} + '%' or copy like '%' + {encoded} + '%')");
                }
                string criteriaStr = string.Join(" AND ", criteria);
                //
                var posts = DbBaseModel.createList<BlogEntryModel>(cp, criteriaStr, "dateAdded DESC", pageSize, pageNumber);
                var result = posts.Select(p => new {
                    postId = p.id,
                    name = p.name,
                    blogId = p.blogId,
                    authorMemberId = p.authorMemberId,
                    datePublished = p.datePublished.HasValue ? p.datePublished.Value.ToString("yyyy-MM-dd") : "",
                    dateAdded = p.dateAdded.HasValue ? p.dateAdded.Value.ToString("yyyy-MM-dd") : "",
                    blogCategoryId = p.blogCategoryId,
                    tagList = p.tagList ?? "",
                    allowComments = p.allowComments,
                    viewings = p.viewings
                }).ToList();
                return McpResponseHelper.Success(cp, new { posts = result, pageSize, pageNumber }, $"Found {result.Count} post(s)");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error listing blog posts.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_post_get
        // ====================================================================================================
        //
        private string blogPostGet(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int postId = McpResponseHelper.GetIntArg(args, "postId");
                if (postId == 0) {
                    return McpResponseHelper.Error(cp, "postId is required.");
                }
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (post == null) {
                    return McpResponseHelper.Error(cp, $"Blog post #{postId} not found.");
                }
                //
                // -- build unified images array
                var images = buildImageList(cp, post);
                //
                var result = new {
                    postId = post.id,
                    name = post.name,
                    copy = post.copy ?? "",
                    blogId = post.blogId,
                    authorMemberId = post.authorMemberId,
                    blogCategoryId = post.blogCategoryId,
                    tagList = post.tagList ?? "",
                    datePublished = post.datePublished.HasValue ? post.datePublished.Value.ToString("yyyy-MM-dd") : "",
                    dateAdded = post.dateAdded.HasValue ? post.dateAdded.Value.ToString("yyyy-MM-dd") : "",
                    allowComments = post.allowComments,
                    viewings = post.viewings,
                    metaTitle = post.metaTitle ?? "",
                    metaDescription = post.metaDescription ?? "",
                    metaKeywordList = post.metaKeywordList ?? "",
                    primaryImage = post.primaryImage ?? "",
                    primaryImageDescription = post.primaryImageDescription ?? "",
                    primaryImageAspectRatioId = post.primaryImageAspectRatioId,
                    active = post.active,
                    images
                };
                return McpResponseHelper.Success(cp, result, "OK");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error retrieving blog post.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_post_create
        // ====================================================================================================
        //
        private string blogPostCreate(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int blogId = McpResponseHelper.GetIntArg(args, "blogId");
                string name = McpResponseHelper.GetStringArg(args, "name");
                if (blogId == 0) {
                    return McpResponseHelper.Error(cp, "blogId is required.");
                }
                if (string.IsNullOrEmpty(name)) {
                    return McpResponseHelper.Error(cp, "name is required.");
                }
                //
                // -- verify the blog exists
                var blog = DbBaseModel.create<BlogModel>(cp, blogId);
                if (blog == null) {
                    return McpResponseHelper.Error(cp, $"Blog #{blogId} not found.");
                }
                //
                var post = DbBaseModel.addDefault<BlogEntryModel>(cp);
                if (post == null) {
                    return McpResponseHelper.Error(cp, "Error creating blog post record.");
                }
                post.name = name;
                post.blogId = blogId;
                post.copy = McpResponseHelper.GetStringArg(args, "copy");
                post.authorMemberId = McpResponseHelper.GetIntArg(args, "authorMemberId", cp.User.Id);
                post.blogCategoryId = McpResponseHelper.GetIntArg(args, "categoryId");
                post.allowComments = McpResponseHelper.GetBoolArg(args, "allowComments");
                post.tagList = McpResponseHelper.GetStringArg(args, "tagList");
                //
                string dateStr = McpResponseHelper.GetStringArg(args, "datePublished");
                if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime parsedDate)) {
                    post.datePublished = parsedDate;
                } else {
                    post.datePublished = DateTime.Now;
                }
                //
                post.save(cp);
                //
                // -- capture undo (recordDeleted=true means undo will delete this newly created record)
                McpUndoHelper.Capture(cp, Contensive.Blog.constants.cnBlogEntries, post.id, post.ccguid, ToolBlogPostCreate, new Dictionary<string, string>(), recordDeleted: true);
                //
                return McpResponseHelper.Success(cp, new { postId = post.id, name = post.name, blogId = post.blogId }, "Blog post created successfully.");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error creating blog post.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_post_update
        // ====================================================================================================
        //
        private string blogPostUpdate(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int postId = McpResponseHelper.GetIntArg(args, "postId");
                if (postId == 0) {
                    return McpResponseHelper.Error(cp, "postId is required.");
                }
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (post == null) {
                    return McpResponseHelper.Error(cp, $"Blog post #{postId} not found.");
                }
                //
                // -- capture undo before changes
                var fieldsBefore = new Dictionary<string, string> {
                    ["name"] = post.name ?? "",
                    ["copy"] = post.copy ?? "",
                    ["blogCategoryId"] = post.blogCategoryId.ToString(),
                    ["authorMemberId"] = post.authorMemberId.ToString(),
                    ["datePublished"] = post.datePublished?.ToString("o") ?? "",
                    ["allowComments"] = post.allowComments ? "1" : "0",
                    ["tagList"] = post.tagList ?? "",
                    ["metaTitle"] = post.metaTitle ?? "",
                    ["metaDescription"] = post.metaDescription ?? "",
                    ["metaKeywordList"] = post.metaKeywordList ?? "",
                    ["primaryImage"] = post.primaryImage ?? "",
                    ["primaryImageDescription"] = post.primaryImageDescription ?? "",
                    ["primaryImageAspectRatioId"] = post.primaryImageAspectRatioId.ToString()
                };
                McpUndoHelper.Capture(cp, Contensive.Blog.constants.cnBlogEntries, post.id, post.ccguid, ToolBlogPostUpdate, fieldsBefore);
                //
                // -- apply updates for any provided fields
                if (args.ContainsKey("name")) { post.name = McpResponseHelper.GetStringArg(args, "name"); }
                if (args.ContainsKey("copy")) { post.copy = McpResponseHelper.GetStringArg(args, "copy"); }
                if (args.ContainsKey("categoryId")) { post.blogCategoryId = McpResponseHelper.GetIntArg(args, "categoryId"); }
                if (args.ContainsKey("authorMemberId")) { post.authorMemberId = McpResponseHelper.GetIntArg(args, "authorMemberId"); }
                if (args.ContainsKey("allowComments")) { post.allowComments = McpResponseHelper.GetBoolArg(args, "allowComments"); }
                if (args.ContainsKey("tagList")) { post.tagList = McpResponseHelper.GetStringArg(args, "tagList"); }
                if (args.ContainsKey("metaTitle")) { post.metaTitle = McpResponseHelper.GetStringArg(args, "metaTitle"); }
                if (args.ContainsKey("metaDescription")) { post.metaDescription = McpResponseHelper.GetStringArg(args, "metaDescription"); }
                if (args.ContainsKey("metaKeywordList")) { post.metaKeywordList = McpResponseHelper.GetStringArg(args, "metaKeywordList"); }
                if (args.ContainsKey("primaryImage")) { post.primaryImage = McpResponseHelper.GetStringArg(args, "primaryImage"); }
                if (args.ContainsKey("primaryImageDescription")) { post.primaryImageDescription = McpResponseHelper.GetStringArg(args, "primaryImageDescription"); }
                if (args.ContainsKey("primaryImageAspectRatioId")) { post.primaryImageAspectRatioId = McpResponseHelper.GetIntArg(args, "primaryImageAspectRatioId"); }
                if (args.ContainsKey("datePublished")) {
                    string dateStr = McpResponseHelper.GetStringArg(args, "datePublished");
                    if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime parsedDate)) {
                        post.datePublished = parsedDate;
                    }
                }
                //
                post.save(cp);
                return McpResponseHelper.Success(cp, new { postId = post.id, name = post.name }, "Blog post updated successfully.");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error updating blog post.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_post_delete
        // ====================================================================================================
        //
        private string blogPostDelete(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int postId = McpResponseHelper.GetIntArg(args, "postId");
                if (postId == 0) {
                    return McpResponseHelper.Error(cp, "postId is required.");
                }
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (post == null) {
                    return McpResponseHelper.Error(cp, $"Blog post #{postId} not found.");
                }
                //
                // -- capture undo before delete (soft delete restores active flag)
                var fieldsBefore = new Dictionary<string, string> {
                    ["active"] = post.active.ToString()
                };
                McpUndoHelper.Capture(cp, Contensive.Blog.constants.cnBlogEntries, post.id, post.ccguid, ToolBlogPostDelete, fieldsBefore);
                //
                // -- soft delete by setting active=0 via CS to avoid model limitations
                using (CPCSBaseClass cs = cp.CSNew()) {
                    if (cs.OpenRecord(Contensive.Blog.constants.cnBlogEntries, postId)) {
                        cs.SetField("active", "0");
                        cs.Save();
                    }
                }
                return McpResponseHelper.Success(cp, new { postId = post.id, name = post.name }, "Blog post deleted successfully.");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error deleting blog post.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_post_image_list
        // ====================================================================================================
        //
        private string blogPostImageList(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int postId = McpResponseHelper.GetIntArg(args, "postId");
                if (postId == 0) {
                    return McpResponseHelper.Error(cp, "postId is required.");
                }
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (post == null) {
                    return McpResponseHelper.Error(cp, $"Blog post #{postId} not found.");
                }
                //
                var images = buildImageList(cp, post);
                return McpResponseHelper.Success(cp, new { postId = post.id, images }, $"Found {images.Count} image(s) for post #{postId}");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error listing blog post images.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_post_image_add
        // ====================================================================================================
        //
        private string blogPostImageAdd(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int postId = McpResponseHelper.GetIntArg(args, "postId");
                string resourceName = McpResponseHelper.GetStringArg(args, "resourceName");
                if (postId == 0) {
                    return McpResponseHelper.Error(cp, "postId is required.");
                }
                if (string.IsNullOrEmpty(resourceName)) {
                    return McpResponseHelper.Error(cp, "resourceName is required.");
                }
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (post == null) {
                    return McpResponseHelper.Error(cp, $"Blog post #{postId} not found.");
                }
                //
                var image = DbBaseModel.addDefault<BlogImageModel>(cp);
                if (image == null) {
                    return McpResponseHelper.Error(cp, "Error creating image record.");
                }
                image.blogEntryId = postId;
                image.name = McpResponseHelper.GetStringArg(args, "altText");
                image.description = McpResponseHelper.GetStringArg(args, "description");
                image.imageAspectRatioId = McpResponseHelper.GetIntArg(args, "aspectRatioId");
                image.sortOrder = getNextSortOrder(cp, postId);
                image.save(cp);
                //
                // -- copy the uploaded resource file to the image's upload path
                string uploadPath = image.getUploadPath("filename");
                string fileName = System.IO.Path.GetFileName(resourceName);
                string destPath = $"{uploadPath}{fileName}";
                cp.CdnFiles.Copy(resourceName, destPath);
                image.Filename = destPath;
                image.save(cp);
                //
                // -- capture undo (recordDeleted=true means undo will delete this newly created record)
                McpUndoHelper.Capture(cp, Contensive.Blog.constants.cnBlogImages, image.id, image.ccguid, ToolBlogPostImageAdd, new Dictionary<string, string>(), recordDeleted: true);
                //
                // -- determine position in the unified list
                var imageList = BlogImageModel.getPostImageList(cp, post);
                int position = imageList.FindIndex(i => i.id == image.id) + 1;
                //
                return McpResponseHelper.Success(cp, new { imageId = image.id, postId, filename = image.Filename, position }, "Image added successfully.");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error adding blog post image.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_post_image_update
        // ====================================================================================================
        //
        private string blogPostImageUpdate(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int postId = McpResponseHelper.GetIntArg(args, "postId");
                int imageId = McpResponseHelper.GetIntArg(args, "imageId");
                if (postId == 0) {
                    return McpResponseHelper.Error(cp, "postId is required.");
                }
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (post == null) {
                    return McpResponseHelper.Error(cp, $"Blog post #{postId} not found.");
                }
                //
                if (imageId == 0) {
                    //
                    // -- update primary image
                    var fieldsBefore = new Dictionary<string, string> {
                        ["primaryImage"] = post.primaryImage ?? "",
                        ["primaryImageDescription"] = post.primaryImageDescription ?? "",
                        ["primaryImageAspectRatioId"] = post.primaryImageAspectRatioId.ToString(),
                        ["primaryImageAltSizeList"] = post.primaryImageAltSizeList ?? ""
                    };
                    McpUndoHelper.Capture(cp, Contensive.Blog.constants.cnBlogEntries, post.id, post.ccguid, ToolBlogPostImageUpdate, fieldsBefore);
                    //
                    if (args.ContainsKey("altText")) { post.primaryImageDescription = McpResponseHelper.GetStringArg(args, "altText"); }
                    if (args.ContainsKey("aspectRatioId")) { post.primaryImageAspectRatioId = McpResponseHelper.GetIntArg(args, "aspectRatioId"); }
                    if (args.ContainsKey("resourceName")) {
                        string resourceName = McpResponseHelper.GetStringArg(args, "resourceName");
                        string uploadPath = $"ccblogcopy/primaryimage/{postId.ToString().PadLeft(12, '0')}/";
                        string fileName = System.IO.Path.GetFileName(resourceName);
                        string destPath = $"{uploadPath}{fileName}";
                        cp.CdnFiles.Copy(resourceName, destPath);
                        post.primaryImage = destPath;
                        post.primaryImageAltSizeList = "";
                    }
                    post.save(cp);
                    return McpResponseHelper.Success(cp, new { imageId = 0, postId }, "Primary image updated successfully.");
                } else {
                    //
                    // -- update secondary image
                    var image = DbBaseModel.create<BlogImageModel>(cp, imageId);
                    if (image == null || image.blogEntryId != postId) {
                        return McpResponseHelper.Error(cp, $"Image #{imageId} not found on post #{postId}.");
                    }
                    var fieldsBefore = new Dictionary<string, string> {
                        ["name"] = image.name ?? "",
                        ["description"] = image.description ?? "",
                        ["Filename"] = image.Filename ?? "",
                        ["imageAspectRatioId"] = image.imageAspectRatioId.ToString(),
                        ["altSizeList"] = image.altSizeList ?? ""
                    };
                    McpUndoHelper.Capture(cp, Contensive.Blog.constants.cnBlogImages, image.id, image.ccguid, ToolBlogPostImageUpdate, fieldsBefore);
                    //
                    if (args.ContainsKey("altText")) { image.name = McpResponseHelper.GetStringArg(args, "altText"); }
                    if (args.ContainsKey("description")) { image.description = McpResponseHelper.GetStringArg(args, "description"); }
                    if (args.ContainsKey("aspectRatioId")) { image.imageAspectRatioId = McpResponseHelper.GetIntArg(args, "aspectRatioId"); }
                    if (args.ContainsKey("resourceName")) {
                        string resourceName = McpResponseHelper.GetStringArg(args, "resourceName");
                        string uploadPath = image.getUploadPath("filename");
                        string fileName = System.IO.Path.GetFileName(resourceName);
                        string destPath = $"{uploadPath}{fileName}";
                        cp.CdnFiles.Copy(resourceName, destPath);
                        image.Filename = destPath;
                        image.altSizeList = "";
                    }
                    image.save(cp);
                    return McpResponseHelper.Success(cp, new { imageId = image.id, postId }, "Image updated successfully.");
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error updating blog post image.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_post_image_delete
        // ====================================================================================================
        //
        private string blogPostImageDelete(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int postId = McpResponseHelper.GetIntArg(args, "postId");
                int imageId = McpResponseHelper.GetIntArg(args, "imageId");
                if (postId == 0) {
                    return McpResponseHelper.Error(cp, "postId is required.");
                }
                if (imageId == 0) {
                    return McpResponseHelper.Error(cp, "Cannot delete the primary image this way. Use blog_post_update to clear primaryImage instead.");
                }
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (post == null) {
                    return McpResponseHelper.Error(cp, $"Blog post #{postId} not found.");
                }
                var image = DbBaseModel.create<BlogImageModel>(cp, imageId);
                if (image == null || image.blogEntryId != postId) {
                    return McpResponseHelper.Error(cp, $"Image #{imageId} not found on post #{postId}.");
                }
                //
                // -- capture undo for the image record (soft delete)
                var imageFieldsBefore = new Dictionary<string, string> {
                    ["active"] = "1"
                };
                McpUndoHelper.Capture(cp, Contensive.Blog.constants.cnBlogImages, image.id, image.ccguid, ToolBlogPostImageDelete, imageFieldsBefore);
                //
                // -- remove any [imageNNN] tag from post copy
                string copyBefore = post.copy ?? "";
                string pattern = $@"\[image{imageId}\]";
                string copyAfter = Regex.Replace(copyBefore, pattern, "", RegexOptions.IgnoreCase);
                if (copyAfter != copyBefore) {
                    var postFieldsBefore = new Dictionary<string, string> {
                        ["copy"] = copyBefore
                    };
                    McpUndoHelper.Capture(cp, Contensive.Blog.constants.cnBlogEntries, post.id, post.ccguid, ToolBlogPostImageDelete, postFieldsBefore);
                    post.copy = copyAfter;
                    post.save(cp);
                }
                //
                // -- soft delete the image
                using (CPCSBaseClass cs = cp.CSNew()) {
                    if (cs.OpenRecord(Contensive.Blog.constants.cnBlogImages, imageId)) {
                        cs.SetField("active", "0");
                        cs.Save();
                    }
                }
                return McpResponseHelper.Success(cp, new { imageId, postId }, "Image deleted successfully.");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error deleting blog post image.");
            }
        }
        //
        // ====================================================================================================
        // -- blog_post_image_place
        // ====================================================================================================
        //
        private string blogPostImagePlace(CPBaseClass cp, Dictionary<string, object> args) {
            try {
                int postId = McpResponseHelper.GetIntArg(args, "postId");
                int imageId = McpResponseHelper.GetIntArg(args, "imageId");
                int insertPosition = McpResponseHelper.GetIntArg(args, "insertPosition");
                if (postId == 0) {
                    return McpResponseHelper.Error(cp, "postId is required.");
                }
                if (imageId == 0) {
                    return McpResponseHelper.Error(cp, "imageId is required and must be a secondary image (> 0). The primary image cannot be placed inline.");
                }
                var post = DbBaseModel.create<BlogEntryModel>(cp, postId);
                if (post == null) {
                    return McpResponseHelper.Error(cp, $"Blog post #{postId} not found.");
                }
                var image = DbBaseModel.create<BlogImageModel>(cp, imageId);
                if (image == null || image.blogEntryId != postId) {
                    return McpResponseHelper.Error(cp, $"Image #{imageId} not found on post #{postId}.");
                }
                //
                // -- capture undo for the post copy
                var fieldsBefore = new Dictionary<string, string> {
                    ["copy"] = post.copy ?? ""
                };
                McpUndoHelper.Capture(cp, Contensive.Blog.constants.cnBlogEntries, post.id, post.ccguid, ToolBlogPostImagePlace, fieldsBefore);
                //
                // -- place the image using the shared logic from BlogImagePlaceRemote
                post.copy = Contensive.Blog.BlogImagePlaceRemote.placeImageInCopy(post.copy ?? "", imageId, insertPosition);
                post.save(cp);
                //
                return McpResponseHelper.Success(cp, new { postId, imageId, inlineTag = $"[image{imageId}]", insertPosition }, "Image placed inline successfully.");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return McpResponseHelper.Error(cp, "Error placing blog post image.");
            }
        }
        //
        // ====================================================================================================
        // -- helper: build unified image list for a post
        // ====================================================================================================
        //
        private static List<object> buildImageList(CPBaseClass cp, BlogEntryModel post) {
            var blogImageList = BlogImageModel.getPostImageList(cp, post);
            //
            // -- scan copy for [imageNNN] tags to determine inline placement
            var inlineTagLookup = new Dictionary<int, string>();
            if (!string.IsNullOrEmpty(post.copy)) {
                foreach (Match match in Regex.Matches(post.copy, @"\[image(\d+)\]", RegexOptions.IgnoreCase)) {
                    int tagImageId = int.Parse(match.Groups[1].Value);
                    inlineTagLookup[tagImageId] = match.Value;
                }
            }
            //
            int position = 1;
            return blogImageList.Select(img => {
                bool isPrimary = (img.id == 0);
                string inlineTag = (!isPrimary && inlineTagLookup.ContainsKey(img.id)) ? inlineTagLookup[img.id] : "";
                return (object)new {
                    position = position++,
                    imageId = img.id,
                    isPrimary,
                    filename = img.Filename ?? "",
                    altText = isPrimary ? (post.primaryImageDescription ?? "") : (img.name ?? ""),
                    description = img.description ?? "",
                    aspectRatioId = isPrimary ? post.primaryImageAspectRatioId : img.imageAspectRatioId,
                    inlineTag
                };
            }).ToList();
        }
        //
        // ====================================================================================================
        // -- helper: get next sort order for a post's images
        // ====================================================================================================
        //
        private static string getNextSortOrder(CPBaseClass cp, int postId) {
            int count = 0;
            string sql = $"SELECT COUNT(*) as cnt FROM BlogImages WHERE blogentryid={postId} AND (active<>0)";
            using (var dt = cp.Db.ExecuteQuery(sql)) {
                if (dt.Rows.Count > 0) {
                    count = cp.Utils.EncodeInteger(dt.Rows[0]["cnt"]);
                }
            }
            return (count + 1).ToString().PadLeft(12, '0');
        }
    }
}
