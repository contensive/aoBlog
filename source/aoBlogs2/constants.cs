using Microsoft.VisualBasic;

namespace Contensive.Blog {

    public static class constants {
        // 
        public const string AnonymousMemberName = "Anonymous";
        //
        public const string defaultImageUrl = "blogs/placeholder.jpg";
        //
        // -- layouts
        public const string layoutGuidLastestPosts = "{987cb36b-22f8-4896-a54e-aa7dbab98f93}";
        public const string layoutNameLastestPosts = "Latest Posts Layout";
        public const string layoutPathFilenameLastestPosts = @"aoBlog\LatestPostLayout.html";
        // 
        // -- Guids
        // 
        public const string guidGroupBlogAuthors = "{082E5ADC-9477-4DB6-A3D7-39B6EAB3A519}";
        public const string nameGroupBlogAuthors = "Blog Authors";
        //
        public const string reCaptchaDisplayGuid = "{E9E51C6E-9152-4284-A44F-D3ABC423AB90}";
        public const string reCaptchaProcessGuid = "{030AC5B0-F796-4EA4-B94C-986B1C29C16C}";
        public const string RSSProcessAddonGuid = "{2119C2DA-1D57-4C32-B13C-28CD2D85EDF5}";
        public const string addonGuidWebcast = "{F6037DEE-023C-4A14-A972-ADAFA5538240}";
        public const string guidDefaultCallToAction = "{3ab72599-bfec-4f8c-9801-a21fd7b6a084}";
        public const string BlogListLayout = "{58788483-D050-4464-9261-627278A57B35}";
        public const string LegacyBlogAddon = "{656E95EA-2799-45CD-9712-D4CEDF0E2D02}";
        public const string facebookLikeAddonGuid = "{17919A35-06B3-4F32-9607-4DB3228A15DF}";
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