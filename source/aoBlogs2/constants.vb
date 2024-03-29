﻿
Option Explicit On
Option Strict On

Public Module constants
    '
    Public Const AnonymousMemberName = "Anonymous"
    '
    ' -- Guids
    '
    Public Const reCaptchaDisplayGuid = "{E9E51C6E-9152-4284-A44F-D3ABC423AB90}"
    Public Const reCaptchaProcessGuid = "{030AC5B0-F796-4EA4-B94C-986B1C29C16C}"
    Public Const RSSProcessAddonGuid = "{2119C2DA-1D57-4C32-B13C-28CD2D85EDF5}"
    Public Const addonGuidWebcast As String = "{F6037DEE-023C-4A14-A972-ADAFA5538240}"
    Public Const guidDefaultCallToAction As String = "{3ab72599-bfec-4f8c-9801-a21fd7b6a084}"
    Public Const BlogListLayout As String = "{58788483-D050-4464-9261-627278A57B35}"
    Public Const LegacyBlogAddon As String = "{656E95EA-2799-45CD-9712-D4CEDF0E2D02}"
    Public Const facebookLikeAddonGuid As String = "{17919A35-06B3-4F32-9607-4DB3228A15DF}"
    Public Const reCaptchaWorkingGuid As String = "{500A1F57-86A2-4D47-B747-4EF4D30A83E2}"
    '
    ' -- images
    '
    Public Const iconRSS_16x16 As String = "<svg xmlns=""http://www.w3.org/2000/svg"" width=""16"" height=""16"" fill=""currentColor"" class=""bi bi-rss-fill"" viewBox=""0 0 16 16""><path d=""M2 0a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V2a2 2 0 0 0-2-2H2zm1.5 2.5c5.523 0 10 4.477 10 10a1 1 0 1 1-2 0 8 8 0 0 0-8-8 1 1 0 0 1 0-2zm0 4a6 6 0 0 1 6 6 1 1 0 1 1-2 0 4 4 0 0 0-4-4 1 1 0 0 1 0-2zm.5 7a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3z""/></svg>"
    '
    ' -- messages
    '
    Public Const BackToRecentPostsMsg = "« Back to Recent Articles"
    Public Const NextArticlePrefix = "» Next Article, "
    ''' <summary>
    ''' copy that will be used as the automatic first post if the virtual file blogs/DefaultPostCopy.txt is not found
    ''' </summary>
    Public Const DefaultPostCopy = "This post has been created automatically for you by the system. Verify the blog is set up properly by viewing the blog settings available after turning on Advanced Edit in the toolbar."
    '
    ' -- content
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
    ' -- no idea what this is
    ' 
    Public Const SNBlogEntryName As String = "Blog Entry Serial Number"
    Public Const SNBlogCommentName As String = "Blog Comment Serial Number"
    '
    ' -- request name
    ' 
    Public Const rnPageNumber As String = "page"
    Public Const rnQueryTag As String = "qTag"
    Public Const rnFormID As String = "FormID"
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
    Public Const RequestNameBlogEntryID As String = "BlogEntryID"
    Public Const RequestNameBlogEntryName As String = "blogEntryTitle"
    Public Const RequestNameBlogEntryCopy As String = "blogEntryCopy"
    Public Const RequestNameBlogEntryTagList As String = "blogEntryTagList"
    Public Const RequestNameBlogEntryCategoryID As String = "blogEntryCategoryId"
    '
    ' -- system email
    '
    Public Const SystemEmailBlogNotification As String = "New Blog Notification"
    Public Const SystemEmailCommentNotification As String = "New Blog Comment Notification"
    '
    '-- buttons
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
    ' -- views
    '
    Public Const FormBlogPostList As Integer = 100
    Public Const FormBlogEntryEditor As Integer = 110
    Public Const FormBlogSearch As Integer = 120
    Public Const FormBlogPostDetails As Integer = 300
    Public Const FormBlogArchiveDateList As Integer = 400
    Public Const FormBlogArchivedBlogs As Integer = 600
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
