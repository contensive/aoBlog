
Option Explicit On
Option Strict On

Public Module constants
    '
    Public Const AnonymousMemberName = "Anonymous"
    Public Const reCaptchaDisplayGuid = "{E9E51C6E-9152-4284-A44F-D3ABC423AB90}"
    Public Const reCaptchaProcessGuid = "{030AC5B0-F796-4EA4-B94C-986B1C29C16C}"
    '
    Public Const BackToRecentPostsMsg = "« Back to recent posts"
    '
    Public Const RSSProcessAddonGuid = "{2119C2DA-1D57-4C32-B13C-28CD2D85EDF5}"
    '
    ' copy that will be used as the automatic first post if the virtual file blogs/DefaultPostCopy.txt is not found
    '
    Public Const DefaultPostCopy = "This post has been created automatically for you by the system. Verify the blog is set up properly by viewing the blog settings available after turning on Advanced Edit in the toolbar."
    '
    Public Const cnPeople As String = "people"
    Public Const cnBlogs As String = "Blogs"
    Public Const cnBlogCopy As String = "Blog Copy"
    Public Const cnBlogEntries As String = "Blog Entries"
    Public Const cnBlogComments As String = "Blog Comments"
    Public Const cnBlogTypes As String = "Blog Types"
    Public Const cnBlogCategories As String = "Blog Categories"
    Public Const cnBlogCategoryRules As String = "Blog Category Group Rules"
    Public Const cnRSSFeeds As String = "RSS Feeds"
    Public Const cnRSSFeedBlogRules As String = "RSS Feed Blog Rules"
    Public Const cnBlogImages As String = "Blog Images"
    Public Const cnBlogImageRules As String = "Blog Image Rules"
    Public Const cnCTA As String = "Calls to Action"

    '
    Public Const TableNameBlogCategoryRules As String = "ccBlogCategoryGroupRules"
    '
    Public Const SNBlogEntryName As String = "Blog Entry Serial Number"
    Public Const SNBlogCommentName As String = "Blog Comment Serial Number"
    '
    Public Const RequestNameQueryTag As String = "qTag"
    Public Const RequestNameFormID As String = "FormID"
    Public Const RequestNameSourceFormID As String = "SourceFormID"
    Public Const RequestNameBlogTitle As String = "BlogTitle"
    Public Const RequestNameBlogCopy As String = "BlogCopy"
    Public Const RequestNameBlogTagList As String = "BlogTagList"
    Public Const RequestNameDateAdded As String = "DateAdded"
    Public Const RequestNameBlogCategoryID As String = "BlogCategoryID"
    Public Const RequestNameBlogCategoryIDSet As String = "SetBlogCategoryID"
    Public Const RequestNameBlogPodcastMediaLink As String = "PodcastMediaLink"
    Public Const RequestNameAuthorName As String = "AuthorName"
    Public Const RequestNameAuthorEmail As String = "AuthorEmail"
    Public Const RequestNameCommentCopy As String = "CommentCopy"
    Public Const RequestNameCommentTitle As String = "CommentTitle"
    Public Const RequestNameCommentDate As String = "CommentDate"
    Public Const RequestNameApproved As String = "Approved"
    Public Const RequestNameBlogEntryID As String = "BlogEntryID"
    Public Const RequestNameCommentFormKey As String = "formkey"
    Public Const RequestNameKeywordList As String = "keywordlist"
    Public Const RequestNameDateSearch As String = "DateSearch"
    Public Const RequestNameArchiveMonth As String = "ArchiveMonth"
    Public Const RequestNameArchiveYear As String = "ArchiveYear"
    Public Const rnButtonValue As String = "buttonvalue"
    Public Const rnButton As String = "button"
    Public Const rnBlogUploadPrefix As String = "LibraryUpload"
    Public Const rnBlogImageName As String = "LibraryName"
    Public Const rnBlogImageDescription As String = "LibraryDescription"
    Public Const rnBlogImageOrder As String = "LibraryOrder"
    Public Const rnBlogImageDelete As String = "LibraryUploadDelete"
    Public Const rnBlogImageID As String = "LibraryID"
    '
    Public Const SystemEmailBlogNotification As String = "New Blog Notification"
    Public Const SystemEmailCommentNotification As String = "New Blog Comment Notification"
    '
    Public Const VersionSiteProperty As String = "BlogsVersion"
    '
    Public Const FormButtonDelete As String = " Delete "
    Public Const FormButtonCreate As String = " Create "
    Public Const FormButtonPost As String = " Post "
    Public Const FormButtonSearch As String = " Search Blogs "
    Public Const FormButtonPostComment As String = " Post Comment "
    Public Const FormButtonCancel As String = "  Cancel  "
    Public Const FormButtonApply As String = "  Apply  "
    Public Const FormButtonApplyCommentChanges As String = "  Apply Comment Changes  "
    '
    Public Const FormBlogPostList As Integer = 100
    Public Const FormBlogEntryEditor As Integer = 110
    Public Const FormBlogSearch As Integer = 120
    Public Const FormBlogPostDetails As Integer = 300
    Public Const FormBlogArchiveDateList As Integer = 400
    Public Const FormBlogArchivedBlogs As Integer = 600
    '
    Public Const BlogListLayout As String = "{58788483-D050-4464-9261-627278A57B35}"
    Public Const LegacyBlogAddon As String = "{656E95EA-2799-45CD-9712-D4CEDF0E2D02}"
    Public Const facebookLikeAddonGuid As String = "{17919A35-06B3-4F32-9607-4DB3228A15DF}"
    '
    Public Const reCaptchaWorkingGuid As String = "{500A1F57-86A2-4D47-B747-4EF4D30A83E2}"
    '
    Public Const imageDisplayTypeNone As Integer = 1
    Public Const imageDisplayTypeList As Integer = 2
    Public Const imageDisplayTypeFader As Integer = 3
    '
    Public Const cr As String = vbCrLf
    '
    ' -- errors for resultErrList
    Public Enum resultErrorEnum
        errPermission = 50
        errDuplicate = 100
        errVerification = 110
        errRestriction = 120
        errInput = 200
        errAuthentication = 300
        errAdd = 400
        errSave = 500
        errDelete = 600
        errLookup = 700
        errLoad = 710
        errContent = 800
        errMiscellaneous = 900
    End Enum
    '
    ' -- http errors
    Public Enum httpErrorEnum
        badRequest = 400
        unauthorized = 401
        paymentRequired = 402
        forbidden = 403
        notFound = 404
        methodNotAllowed = 405
        notAcceptable = 406
        proxyAuthenticationRequired = 407
        requestTimeout = 408
        conflict = 409
        gone = 410
        lengthRequired = 411
        preconditionFailed = 412
        payloadTooLarge = 413
        urlTooLong = 414
        unsupportedMediaType = 415
        rangeNotSatisfiable = 416
        expectationFailed = 417
        teapot = 418
        methodFailure = 420
        enhanceYourCalm = 420
        misdirectedRequest = 421
        unprocessableEntity = 422
        locked = 423
        failedDependency = 424
        upgradeRequired = 426
        preconditionRequired = 428
        tooManyRequests = 429
        requestHeaderFieldsTooLarge = 431
        loginTimeout = 440
        noResponse = 444
        retryWith = 449
        redirect = 451
        unavailableForLegalReasons = 451
        sslCertificateError = 495
        sslCertificateRequired = 496
        httpRequestSentToSecurePort = 497
        invalidToken = 498
        clientClosedRequest = 499
        tokenRequired = 499
        internalServerError = 500
    End Enum
End Module
