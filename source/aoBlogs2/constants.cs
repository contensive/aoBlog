using Microsoft.VisualBasic;

namespace Contensive.Blog {

    public static class constants {
        // 
        public const string AnonymousMemberName = "Anonymous";
        //
        public const string defaultImageUrl = "blogs/placeholder.jpg";
        //
        // -- layouts
        //
        public const string layoutGuidBlogArchiveDateListView = "{716E91F8-0C48-4028-9D7B-F48E339FA8A9}";
        public const string layoutNameBlogArchiveDateListView = "Blog Archive Date List View";
        public const string layoutPathFilenameBlogArchiveDateListView = @"BlogArchiveDateListView.html";
        //
        public const string layoutGuidBlogArchivedPostsView = "{8288F2E2-BFE3-4F94-980E-FC9E076F477B}";
        public const string layoutNameBlogArchivedPostsView = "Blog Archived Posts View";
        public const string layoutPathFilenameBlogArchivedPostsView = @"BlogArchivedPostsView.html";
        //
        public const string layoutGuidBlogArticleView = "{4FAA7369-2C91-43AC-82FC-B3B66A5439A0}";
        public const string layoutNameBlogArticleView = "Blog Article View";
        public const string layoutPathFilenameBlogArticleView = @"BlogArticleView.html";
        //
        public const string layoutGuidBlogEditView = "{F30FD369-31F0-46E8-BF61-910C119CDBCB}";
        public const string layoutNameBlogEditView = "Blog Edit View";
        public const string layoutPathFilenameBlogEditView = @"BlogEditView.html";
        //
        public const string layoutGuidBlog = "{1E579F72-64D5-4413-89EA-A61E8EDA426D}";
        public const string layoutNameBlog = "Blog Layout";
        public const string layoutPathFilenameBlog = @"BlogLayout.html";
        //
        public const string layoutGuidBlogListView = "{387E2746-E5E1-43CF-9413-C86C4EFF2B28}";
        public const string layoutNameBlogListView = "Blog List View";
        public const string layoutPathFilenameBlogListView = @"BlogListView.html";
        //
        public const string layoutGuidBlogPostCell = "{FC6652FD-EA4D-4668-8745-8161A6D34291}";
        public const string layoutNameBlogPostCell = "Blog Post Cell";
        public const string layoutPathFilenameBlogPostCell = @"BlogPostCell.html";
        //
        public const string layoutGuidBlogSearchView = "{075F6A58-7546-4638-987E-F88B766C5DC9}";
        public const string layoutNameBlogSearchView = "Blog Search View";
        public const string layoutPathFilenameBlogSearchView = @"BlogSearchView.html";
        //
        public const string layoutGuidLastestPosts = "{987cb36b-22f8-4896-a54e-aa7dbab98f93}";
        public const string layoutNameLastestPosts = "Latest Posts Layout";
        public const string layoutPathFilenameLastestPosts = @"LatestPostLayout.html";
        //
        // -- Guids
        // 
        public const string guidGroupBlogAuthors = "{082E5ADC-9477-4DB6-A3D7-39B6EAB3A519}";
        public const string nameGroupBlogAuthors = "Blog Authors";
        //
        // -- portal
        public const string guidPortalContentManagement = "{3fdd7c5c-68a5-435f-ba62-a3e4cb0ee61e}";
        public const string guidPortalFeatureBlogList = "{BEF4ADF2-A6FA-4C59-B413-69D369F8B6CE}";
        public const string guidPortalFeatureBlogDetails = "{08FBDABE-CB0E-4CC2-8BE0-57198A80102D}";
        public const string guidAddonBlogList = "{9C74771D-B95B-4ABF-86F1-00F4D5219EFC}";
        public const string guidAddonBlogDetails = "{DF86F90B-C13A-4E27-B88E-BBE7206637E4}";
        public const string guidPortalFeatureBlogPostList = "{ABF702AC-1A8E-4C52-A4CC-2EDF3B787A2B}";
        public const string guidAddonBlogPostList = "{9D2F6B87-BB3A-43BE-AE38-3922381254DA}";
        public const string guidPortalFeatureBlogPostInfo = "{209E7D9C-5A49-409F-B949-9D49F341735E}";
        public const string guidAddonBlogPostInfo = "{064D20C0-3615-420C-931A-92689EE50F28}";
        public const string guidPortalFeatureBlogPostDetails = "{33EA4B39-544B-4E1A-A8B3-80E694DD9842}";
        public const string guidAddonBlogPostDetails = "{F7665186-E496-4855-AB20-CC4CAF2094FA}";
        public const string guidPortalFeatureBlogPostRss = "{A1B12154-07AB-408F-8FA0-BAF27C137F4B}";
        public const string guidAddonBlogPostRss = "{F252F188-061A-46D3-B993-0B815003CF8F}";
        public const string guidPortalFeatureBlogPostSeo = "{B3B1F551-9AA4-4230-9A34-A59BC20F5F79}";
        public const string guidAddonBlogPostSeo = "{E73111BB-1DFB-4FFF-AC7D-D08292946087}";
        //
        public const string rnBlogId = "blogId";
        public const string rnBlogPostId = "blogPostId";
        public const string rnDstFeatureGuid = "dstFeatureGuid";
        public const string rnSrcFormId = "srcFormId";
        //
        public const int formIdBlogList = 700;
        public const int formIdBlogDetails = 710;
        public const int formIdBlogPostList = 720;
        public const int formIdBlogPostInfo = 730;
        public const int formIdBlogPostDetails = 740;
        public const int formIdBlogPostRss = 750;
        public const int formIdBlogPostSeo = 760;
        //
        public const string buttonAdd = " Add ";
        public const string buttonSave = " Save ";
        public const string buttonCancel = " Cancel ";
        public const string buttonOK = " OK ";
        public const string buttonDelete = " Delete ";
        //
        public const string reCaptchaDisplayGuid = "{E9E51C6E-9152-4284-A44F-D3ABC423AB90}";
        public const string reCaptchaProcessGuid = "{030AC5B0-F796-4EA4-B94C-986B1C29C16C}";
        public const string RSSProcessAddonGuid = "{2119C2DA-1D57-4C32-B13C-28CD2D85EDF5}";
        public const string addonGuidWebcast = "{F6037DEE-023C-4A14-A972-ADAFA5538240}";
        public const string guidDefaultCallToAction = "{3ab72599-bfec-4f8c-9801-a21fd7b6a084}";
        public const string BlogListLayout = "{58788483-D050-4464-9261-627278A57B35}";
        public const string LegacyBlogAddon = "{656E95EA-2799-45CD-9712-D4CEDF0E2D02}";
        //public const string facebookLikeAddonGuid = "{17919A35-06B3-4F32-9607-4DB3228A15DF}";
        public const string reCaptchaWorkingGuid = "{500A1F57-86A2-4D47-B747-4EF4D30A83E2}";
        // 
        // -- images
        // 
        public const string iconRSS_16x16 = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-rss-fill\" viewBox=\"0 0 16 16\"><path d=\"M2 0a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V2a2 2 0 0 0-2-2H2zm1.5 2.5c5.523 0 10 4.477 10 10a1 1 0 1 1-2 0 8 8 0 0 0-8-8 1 1 0 0 1 0-2zm0 4a6 6 0 0 1 6 6 1 1 0 1 1-2 0 4 4 0 0 0-4-4 1 1 0 0 1 0-2zm.5 7a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3z\"/></svg>";
        // 
        // -- messages
        // 
        public const string BackToRecentPostsMsg = "« Back to Recent Articles";
        public const string NextArticlePrefix = "» Next Article, ";
        /// <summary>
    /// copy that will be used as the automatic first post if the virtual file blogs/DefaultPostCopy.txt is not found
    /// </summary>
        public const string DefaultPostCopy = "This post has been created automatically for you by the system. Verify the blog is set up properly by viewing the blog settings available after turning on Advanced Edit in the toolbar.";
        // 
        // -- content
        // 
        public const string cnPeople = "people";
        public const string cnBlogs = "Blogs";
        public const string cnBlogCopy = "Blog Copy";
        public const string cnBlogEntries = "Blog Entries";
        public const string cnBlogComments = "Blog Comments";
        public const string cnBlogTypes = "Blog Types";
        public const string cnBlogCategories = "Blog Categories";
        public const string cnBlogCategoryRules = "Blog Category Group Rules";
        public const string cnRSSFeeds = "RSS Feeds";
        public const string cnRSSFeedBlogRules = "RSS Feed Blog Rules";
        public const string cnBlogImages = "Blog Images";
        public const string cnBlogImageRules = "Blog Image Rules";
        public const string cnCTA = "Calls to Action";
        // 
        // -- no idea what this is
        // 
        public const string SNBlogEntryName = "Blog Entry Serial Number";
        public const string SNBlogCommentName = "Blog Comment Serial Number";
        // 
        // -- request name
        // 
        public const string rnPageNumber = "page";
        public const string rnQueryTag = "qTag";
        public const string rnFormID = "FormID";
        public const string RequestNameSourceFormID = "SourceFormID";
        public const string RequestNameBlogTitle = "BlogTitle";
        public const string RequestNameBlogCopy = "BlogCopy";
        public const string RequestNameBlogTagList = "BlogTagList";
        public const string RequestNamedateAdded = "dateAdded";
        public const string RequestNameBlogCategoryID = "BlogCategoryID";
        public const string RequestNameBlogCategoryIDSet = "SetBlogCategoryID";
        public const string RequestNameBlogPodcastMediaLink = "PodcastMediaLink";
        public const string RequestNameAuthorName = "AuthorName";
        public const string RequestNameAuthorEmail = "AuthorEmail";
        public const string RequestNameCommentCopy = "CommentCopy";
        public const string RequestNameCommentTitle = "CommentTitle";
        public const string RequestNameCommentDate = "CommentDate";
        public const string RequestNameApproved = "Approved";
        public const string RequestNameCommentFormKey = "formkey";
        public const string RequestNameKeywordList = "keywordlist";
        public const string RequestNameDateSearch = "DateSearch";
        public const string RequestNameArchiveMonth = "ArchiveMonth";
        public const string RequestNameArchiveYear = "ArchiveYear";
        public const string rnButtonValue = "buttonvalue";
        public const string rnButton = "button";
        public const string rnBlogUploadPrefix = "LibraryUpload";
        public const string rnBlogImageName = "LibraryName";
        public const string rnBlogImageDescription = "LibraryDescription";
        public const string rnBlogImageOrder = "LibraryOrder";
        public const string rnBlogImageDelete = "LibraryUploadDelete";
        public const string rnBlogImageID = "LibraryID";
        public const string RequestNameBlogEntryID = "BlogEntryID";
        public const string RequestNameBlogEntryName = "blogEntryTitle";
        public const string RequestNameBlogEntryCopy = "blogEntryCopy";
        public const string RequestNameBlogEntryTagList = "blogEntryTagList";
        public const string RequestNameBlogEntryCategoryID = "blogEntryCategoryId";
        // 
        // -- system email
        // 
        public const string SystemEmailBlogNotification = "New Blog Notification";
        public const string SystemEmailCommentNotification = "New Blog Comment Notification";
        // 
        // -- buttons
        // 
        public const string FormButtonDelete = " Delete ";
        public const string FormButtonCreate = " Create ";
        public const string FormButtonPost = " Post ";
        public const string FormButtonSearch = " Search Blogs ";
        public const string FormButtonPostComment = " Post Comment ";
        public const string FormButtonCancel = "  Cancel  ";
        public const string FormButtonApply = "  Apply  ";
        public const string FormButtonApplyCommentChanges = "  Apply Comment Changes  ";
        // 
        // -- views
        // 
        public const int FormBlogPostList = 100;
        public const int FormBlogEntryEditor = 110;
        public const int FormBlogSearch = 120;
        public const int FormBlogPostDetails = 300;
        public const int FormBlogArchiveDateList = 400;
        public const int FormBlogArchivedBlogs = 600;
        // 
        public const string cr = Constants.vbCrLf;
        // 
        // -- errors for resultErrList
        public enum resultErrorEnum {
            errPermission = 50,
            errDuplicate = 100,
            errVerification = 110,
            errRestriction = 120,
            errInput = 200,
            errAuthentication = 300,
            errAdd = 400,
            errSave = 500,
            errDelete = 600,
            errLookup = 700,
            errLoad = 710,
            errContent = 800,
            errMiscellaneous = 900
        }
        // 
        // -- http errors
        public enum httpErrorEnum {
            badRequest = 400,
            unauthorized = 401,
            paymentRequired = 402,
            forbidden = 403,
            notFound = 404,
            methodNotAllowed = 405,
            notAcceptable = 406,
            proxyAuthenticationRequired = 407,
            requestTimeout = 408,
            conflict = 409,
            gone = 410,
            lengthRequired = 411,
            preconditionFailed = 412,
            payloadTooLarge = 413,
            urlTooLong = 414,
            unsupportedMediaType = 415,
            rangeNotSatisfiable = 416,
            expectationFailed = 417,
            teapot = 418,
            methodFailure = 420,
            enhanceYourCalm = 420,
            misdirectedRequest = 421,
            unprocessableEntity = 422,
            locked = 423,
            failedDependency = 424,
            upgradeRequired = 426,
            preconditionRequired = 428,
            tooManyRequests = 429,
            requestHeaderFieldsTooLarge = 431,
            loginTimeout = 440,
            noResponse = 444,
            retryWith = 449,
            redirect = 451,
            unavailableForLegalReasons = 451,
            sslCertificateError = 495,
            sslCertificateRequired = 496,
            httpRequestSentToSecurePort = 497,
            invalidToken = 498,
            clientClosedRequest = 499,
            tokenRequired = 499,
            internalServerError = 500
        }
    }
}