VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "Blogs"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True

Option Explicit
'
'========================================================================
'   Blog
'
'   The blog and the RSS feed work together.
'       The feed code identies which tables to search through by looking
'       in the ccfields table for any CDef with a "RSSFeeds" many-to-many.
'       If found, this many-to-many is used to find the records for this feed
'
'   Each blog has
'       RSSFeedID - this is the RSS feed used by the blog code to automatically
'           add a post when it is made. You can manually set a blog entry to any
'           post later. The blog code also manages the Feed's description, link, etc.
'
'   Each blog post (entries) have several fields that the feed uses directly:
'       RSSFeeds - many-to-many field that associates this post to a feed
'       RSSTitle - set from the name of the blog post
'       RSSLink - link to this post, created from the RSSFeed Link each time
'       RSSDescription - created from teh blog post copy
'       RSSDateExpire - not support automatically yet, need Addon Execute Events
'           that can be saved in a table, and run in the future.
'       RSSDatePublic - same as expire
'
'   When a new blog is created, an RSS Feed is created for it with a blank Link.
'       > VerifyFeed
'   When the blog is first viewed publically, the link is picked up and put
'       in the RSSFeed record.
'   Each time a post is saved, the entire feed is rebuilt if the RSS Link is
'       not null. The RSSFeeds Link is used as the base URL to the posts.
'       > UpdateBlogFeed
'   The Post form should have a 'set RSS feed to this URL' button.
'
'========================================================================
'
Const AnonymousMemberName = "Anonymous"
Const reCaptchaDisplayGuid = "{E9E51C6E-9152-4284-A44F-D3ABC423AB90}"
Const reCaptchaProcessGuid = "{030AC5B0-F796-4EA4-B94C-986B1C29C16C}"
'
Const BackToRecentPostsMsg = "« Back to recent posts"
'Const BackToRecentPostsMsg = "See recent posts"
'
Const RSSProcessAddonGuid = "{2119C2DA-1D57-4C32-B13C-28CD2D85EDF5}"
'
' copy that will be used as the automatic first post if the virtual file blogs/DefaultPostCopy.txt is not found
'
Const DefaultPostCopy = "This post has been created automatically for you by the system. Verify the blog is set up properly by viewing the blog settings available after turning on Advanced Edit in the toolbar."
'
Const cnPeople = "people"
Const cnBlogs = "Blogs"
Const cnBlogCopy = "Blog Copy"
Const cnBlogEntries = "Blog Entries"
Const cnBlogComments = "Blog Comments"
Const cnBlogTypes = "Blog Types"
Const cnBlogCategories = "Blog Categories"
Const cnBlogCategoryRules = "Blog Category Group Rules"
Const cnRSSFeeds = "RSS Feeds"
Const cnRSSFeedBlogRules = "RSS Feed Blog Rules"
Const cnBlogImages = "Blog Images"
Const cnBlogImageRules = "Blog Image Rules"
'
Const TableNameBlogCategoryRules = "ccBlogCategoryGroupRules"
'
Const SNBlogEntryName = "Blog Entry Serial Number"
Const SNBlogCommentName = "Blog Comment Serial Number"

'
Const RequestNameQueryTag = "qTag"
Const RequestNameFormID = "FormID"
Const RequestNameSourceFormID = "SourceFormID"
Const RequestNameBlogTitle = "BlogTitle"
Const RequestNameBlogCopy = "BlogCopy"
Const RequestNameBlogTagList = "BlogTagList"
Const RequestNameDateAdded = "DateAdded"
Const RequestNameBlogCategoryID = "BlogCategoryID"
Const RequestNameBlogCategoryIDSet = "SetBlogCategoryID"
Const RequestNameBlogPodcastMediaLink = "PodcastMediaLink"
Const RequestNameAuthorName = "AuthorName"
Const RequestNameAuthorEmail = "AuthorEmail"
Const RequestNameCommentCopy = "CommentCopy"
Const RequestNameCommentTitle = "CommentTitle"
Const RequestNameCommentDate = "CommentDate"
Const RequestNameApproved = "Approved"
Const RequestNameBlogEntryID = "BlogEntryID"
Const RequestNameCommentFormKey = "formkey"
Const RequestNameKeywordList = "keywordlist"
Const RequestNameDateSearch = "DateSearch"
Const RequestNameArchiveMonth = "ArchiveMonth"
Const RequestNameArchiveYear = "ArchiveYear"
Const rnButtonValue = "buttonvalue"
Const rnButton = "button"
Const rnBlogUploadPrefix = "LibraryUpload"
Const rnBlogImageName = "LibraryName"
Const rnBlogImageDescription = "LibraryDescription"
Const rnBlogImageOrder = "LibraryOrder"
Const rnBlogImageDelete = "LibraryUploadDelete"
Const rnBlogImageID = "LibraryID"
'
Const SystemEmailBlogNotification = "New Blog Notification"
Const SystemEmailCommentNotification = "New Blog Comment Notification"
'
Private Const VersionSiteProperty = "BlogsVersion"
'Private Const Version = "2008"
'
Const FormButtonDelete = " Delete "
Const FormButtonCreate = " Create "
Const FormButtonPost = " Post "
'Const FormButtonSave = " Save "
Const FormButtonSearch = " Search Blogs "
Const FormButtonPostComment = " Post Comment "
Const FormButtonCancel = "  Cancel  "
Const FormButtonApply = "  Apply  "
Const FormButtonApplyCommentChanges = "  Apply Comment Changes  "
'
Const FormBlogPostList = 100
Const FormBlogEntryEditor = 110
Const FormBlogSearch = 120
'Const FormBlogSearchResults = 200
Const FormBlogPostDetails = 300
Const FormBlogArchiveDateList = 400
'Const FormBlogThankYou = 500
'Const FormBlogCommentThankYou = 550
Const FormBlogArchivedBlogs = 600
'
Private WorkingQueryString As String
Private ErrorString As String
'Private ButtonValue As String
'
'Private BlogTitle As String
'Private DateSearch As String
'Private KeywordList As String
Private RetryCommentPost As Boolean                 ' when true, the comment post page prepopulates with the previous comment post. set by process comment

Private OverviewLength As Long
Private PostsToDisplay As Long

Private AddOnVersion As String
Const StreamUpgradeTimeout = 1800

Private Main As Object
Private Csv As Object
'
'========================================================================
'   v3.3 Add-on Compatibility
'       To make an Add-on that works the same in v3.3 and v3.4, use this adapter instead of the execute above
'========================================================================
'
Public Function Execute(CsvObject As Object, MainObject As Object, OptionString As String, FilterInput As String) As String
    Set Csv = CsvObject
    Call Init(MainObject)
    Execute = GetContent(OptionString)
End Function

Public Sub Init(MainObject As Object)
        
        Set Main = MainObject
        AddOnVersion = App.Major & "." & App.Minor & "." & Format(App.Revision, "000")
        'AddOnVersion = App.Major & "." & App.Minor & "." & App.Revision
        Call Upgrade
    Exit Sub
ErrorTrap:
    Call HandleError("Blogs", "Init", Err.Number, Err.Source, Err.Description, True, False)
End Sub
'
'====================================================================================
'
'====================================================================================
'
Public Function GetContent(OptionString As String) As String
    On Error GoTo ErrorTrap
    '
    Dim blogCaption As String
    Dim blogDescription As String
    Dim ignoreLegacyInstanceOptions As Boolean
    Dim authoringGroup As String
    Dim authoringGroupId As Long
    Dim NewPostCopy As String
    Dim BlogPostID As Long
    Dim CSRule As Long
    Dim EntryLink As String
    Dim RSSTitle As String
    Dim RSSFeedFilename As String
    Dim CSFeed As Long
    Dim DateSearchText As String
    Dim KeywordList As String
    Dim ButtonValue As String
    Dim IsContentManager As Boolean
    Dim Stream As String
    Dim BlogName As String
    Dim BlogID As Long
    Dim BlogOwnerID As Long
    Dim IsBlogOwner As Boolean
    Dim CS As Long
    Dim FormID As Long
    Dim SourceFormID As Long
    Dim DbVersion As String
    Dim AppVersion As String
    Dim s As String
    Dim ArchiveMonth As Long
    Dim ArchiveYear As Long
    Dim EntryID As Long
    Dim AllowAnonymous As Boolean
    Dim Title As String
    Dim AllowCategories As Boolean
    Dim BlogCategoryID As Long
    Dim RSSFeedId As Long
    Dim RSSFeedName As String
    Dim BlogRootLink As String
    Dim ThumbnailImageWidth As Long
    Dim BuildVersion As String
    Dim ImageWidthMax As Long
    Dim autoApproveComments As Boolean
    Dim emailComment As Boolean
    '
    s = s & vbCrLf & cr & "<!-- Blog " & BlogName & " -->" & vbCrLf
    '
    If Not (Main Is Nothing) Then
        BlogName = Main.GetAddonOption("BlogName", OptionString)
        '
        ' get legacy values that used to be in the instance options
        ' These values will all be empty except when the addon was added to a page
        ' in a previous build. A new Blog field "ignoreLegacyInstanceOptions" will be set
        ' when this is detected and the instance values will be saved to the Blog record.
        '
'        AllowAnonymous = kmaEncodeBoolean(Main.GetAddonOption("Allow Anonymous Comments", OptionString))
'        autoApproveComments = kmaEncodeBoolean(Main.GetAddonOption("Auto-Approve New Comments", OptionString))
'        AllowCategories = kmaEncodeBoolean(Main.GetAddonOption("Allow Categories", OptionString))
'        PostsToDisplay = kmaEncodeInteger(Main.GetAddonOption("Number of entries to display", OptionString))
'        OverviewLength = kmaEncodeInteger(Main.GetAddonOption("Overview Character Length", OptionString))
'        ThumbnailImageWidth = kmaEncodeInteger(Main.GetAddonOption("Thumbnail Image Width", OptionString))
'        If ThumbnailImageWidth = 0 Then
'            ThumbnailImageWidth = 150
'        End If
'        ImageWidthMax = kmaEncodeInteger(Main.GetAddonOption("Image Width Max", OptionString))
'        If ImageWidthMax = 0 Then
'            ImageWidthMax = 400
'        End If
'        If OverviewLength = 0 Then
'            OverviewLength = 150
'        End If
'        If PostsToDisplay = 0 Then
'            PostsToDisplay = 5
'        End If
'        If AllowCategories Then
'            If Main.InStream(RequestNameBlogCategoryIDSet) Then
'                BlogCategoryID = Main.GetStreamInteger(RequestNameBlogCategoryIDSet)
'            Else
'                BlogCategoryID = Main.GetStreamInteger(RequestNameBlogCategoryID)
'            End If
'            Call Main.AddRefreshQueryString(RequestNameBlogCategoryID, BlogCategoryID)
'        End If
'        If BlogName = "" Then
'            BlogName = "Default"
'        End If
        If BlogName = "" Then
            BlogName = "Default"
        End If
        BuildVersion = Main.SiteProperty_BuildVersion
        '
        ' add warning if the blog is being viewed in the admin site
        '
        If (InStr(1, Main.ServerLink, Main.SiteProperty_AdminURL, vbTextCompare) <> 0) Then
            s = s & Main.GetAdminHintWrapper("Some blog features such as the rss feed can not be initialized when viewed from the admin site.")
        End If
        '
        ' Get the blog record
        '
        CS = Main.OpenCSContent(cnBlogs, "(name=" & KmaEncodeSQLText(BlogName) & ")", "ID")
        If Main.IsCSOK(CS) Then
            BlogID = Main.GetCSInteger(CS, "ID")
            RSSFeedId = Main.GetCSInteger(CS, "RSSFeedID")
            BlogOwnerID = Main.GetCSInteger(CS, "OwnerMemberID")
            authoringGroupId = Main.GetCSInteger(CS, "AuthoringGroupID")
            ignoreLegacyInstanceOptions = Main.GetCSBoolean(CS, "ignoreLegacyInstanceOptions")
            AllowAnonymous = Main.GetCSBoolean(CS, "AllowAnonymous")
            autoApproveComments = Main.GetCSBoolean(CS, "autoApproveComments")
            emailComment = Main.GetCSBoolean(CS, "emailComment")
            AllowCategories = Main.GetCSBoolean(CS, "AllowCategories")
            PostsToDisplay = Main.GetCSInteger(CS, "PostsToDisplay")
            OverviewLength = Main.GetCSInteger(CS, "OverviewLength")
            ThumbnailImageWidth = Main.GetCSInteger(CS, "ThumbnailImageWidth")
            ImageWidthMax = Main.GetCSInteger(CS, "ImageWidthMax")
            blogDescription = Main.GetCSText(CS, "copy")
            blogCaption = Main.GetCSText(CS, "caption")
        End If
        Call Main.CloseCS(CS)
        If BlogID = 0 Then
            '
            ' Create New Blog
            '
            IsContentManager = Main.IsContentManager("Page Content")
            If IsContentManager Then
                '
                ' BIG assumption - First hit by a content manager for this page is the author
                '
                CS = Main.InsertCSContent(cnBlogs)
                If Main.IsCSOK(CS) Then
                    BlogID = Main.GetCSInteger(CS, "ID")
                    BlogOwnerID = Main.MemberID
                    IsBlogOwner = True
                    '
                    Call Main.SetCS(CS, "name", BlogName)
                    Call Main.SetCS(CS, "OwnerMemberID", BlogOwnerID)
                    '
                    RSSFeedId = Main.GetCSInteger(CS, "RSSFeedID")
                    authoringGroupId = Main.GetCSInteger(CS, "AuthoringGroupID")
                    ignoreLegacyInstanceOptions = Main.GetCSBoolean(CS, "ignoreLegacyInstanceOptions")
                    AllowAnonymous = Main.GetCSBoolean(CS, "AllowAnonymous")
                    autoApproveComments = Main.GetCSBoolean(CS, "autoApproveComments")
                    AllowCategories = Main.GetCSBoolean(CS, "AllowCategories")
                    PostsToDisplay = Main.GetCSInteger(CS, "PostsToDisplay")
                    OverviewLength = Main.GetCSInteger(CS, "OverviewLength")
                    ThumbnailImageWidth = Main.GetCSInteger(CS, "ThumbnailImageWidth")
                    ImageWidthMax = Main.GetCSInteger(CS, "ImageWidthMax")
                End If
                Call Main.CloseCS(CS)
                '
                ' Create the Feed for the new blog
                '
                Call VerifyFeedReturnArgs(BlogID, RSSFeedId, RSSFeedName, RSSFeedFilename, BlogRootLink)
                '
                Title = "Welcome to the " & BlogName
                If InStr(1, BlogName, " Blog", vbTextCompare) = 0 Then
                    Title = Title & " blog"
                End If
                Title = Title & "!"
                '
                CS = Main.InsertCSRecord(cnBlogEntries)
                If Main.CSOK(CS) Then
                    BlogPostID = Main.GetCS(CS, "ID")
                    Call Main.SetCS(CS, "BlogID", BlogID)
                    Call Main.SetCS(CS, "Name", Title)
                    NewPostCopy = Main.ReadVirtualFile("blogs/DefaultPostCopy.txt")
                    If NewPostCopy = "" Then
                        NewPostCopy = DefaultPostCopy
                    End If
                    Call Main.SetCS(CS, "copy", NewPostCopy)
                    Call Main.SetCS(CS, "AuthorMemberID", Main.MemberID)
                    EntryLink = BlogRootLink
                    EntryLink = ModifyQueryString(EntryLink, RequestNameBlogEntryID, CStr(BlogPostID))
                    EntryLink = ModifyQueryString(EntryLink, RequestNameFormID, FormBlogPostDetails)
                    EntryLink = ModifyQueryString(EntryLink, RequestNameBlogCategoryID, "")
                    Call Main.SetCS(CS, "RSSTitle", Title)
                    If InStr(1, EntryLink, Main.SiteProperty_AdminURL, vbTextCompare) = 0 Then
                        Call Main.SetCS(CS, "RSSLink", EntryLink)
                    End If
                    Call Main.SetCS(CS, "RSSDescription", filterCopy(NewPostCopy, 150))
                    RSSTitle = Trim(Title)
                    If RSSTitle = "" Then
                        RSSTitle = "Blog Post " & EntryID
                    End If
                    Call Main.SetCS(CS, "RSSTitle", RSSTitle)
                End If
                Call Main.CloseCS(CS)
                '
                ' Add this new default post to the new feed
                '
                CSRule = Csv.InsertCSRecord(cnRSSFeedBlogRules, 0)
                If Csv.IsCSOK(CSRule) Then
                    Call Csv.SetCS(CSRule, "name", "RSS Feed [" & RSSFeedName & "], Blog Post [" & Title & "]")
                    Call Csv.SetCS(CSRule, "RSSFeedID", RSSFeedId)
                    Call Csv.SetCS(CSRule, "BlogPostID", BlogPostID)
                End If
                Call Csv.CloseCS(CSRule)
                '
                If Main.ContentServerVersion >= "4.1.098" Then
                    Call Csv.ExecuteAddonAsProcess(RSSProcessAddonGuid)
                End If
            
            End If
        End If
        IsBlogOwner = (Main.IsAuthenticated And (Main.MemberID = BlogOwnerID))
        If Not IsBlogOwner Then
            IsBlogOwner = IsBlogOwner Or Main.IsAdmin()
            If Not IsBlogOwner Then
                If authoringGroupId <> 0 Then
                    authoringGroup = Main.GetRecordName("Groups", authoringGroupId)
                    If authoringGroup <> "" Then
                        IsBlogOwner = IsBlogOwner Or Main.IsGroupMember(authoringGroup, Main.MemberID)
                    End If
                End If
            End If
        End If
        '
        ' handle legacy instance option migration to the blog record
        '
        If Not ignoreLegacyInstanceOptions Then
            AllowAnonymous = kmaEncodeBoolean(Main.GetAddonOption("Allow Anonymous Comments", OptionString))
            autoApproveComments = kmaEncodeBoolean(Main.GetAddonOption("Auto-Approve New Comments", OptionString))
            AllowCategories = kmaEncodeBoolean(Main.GetAddonOption("Allow Categories", OptionString))
            PostsToDisplay = kmaEncodeInteger(Main.GetAddonOption("Number of entries to display", OptionString))
            OverviewLength = kmaEncodeInteger(Main.GetAddonOption("Overview Character Length", OptionString))
            ThumbnailImageWidth = kmaEncodeInteger(Main.GetAddonOption("Thumbnail Image Width", OptionString))
            If ThumbnailImageWidth = 0 Then
                ThumbnailImageWidth = 150
            End If
            ImageWidthMax = kmaEncodeInteger(Main.GetAddonOption("Image Width Max", OptionString))
            If ImageWidthMax = 0 Then
                ImageWidthMax = 400
            End If
            If OverviewLength = 0 Then
                OverviewLength = 150
            End If
            If PostsToDisplay = 0 Then
                PostsToDisplay = 5
            End If
            CS = Main.OpenCSContent(cnBlogs, "(id=" & KmaEncodeSQLNumber(BlogID) & ")", "ID")
            If Main.IsCSOK(CS) Then
                Call Main.SetCS(CS, "ignoreLegacyInstanceOptions", True)
                Call Main.SetCS(CS, "AllowAnonymous", AllowAnonymous)
                Call Main.SetCS(CS, "autoApproveComments", autoApproveComments)
                Call Main.SetCS(CS, "AllowCategories", AllowCategories)
                Call Main.SetCS(CS, "PostsToDisplay", PostsToDisplay)
                Call Main.SetCS(CS, "OverviewLength", OverviewLength)
                Call Main.SetCS(CS, "ThumbnailImageWidth", ThumbnailImageWidth)
                Call Main.SetCS(CS, "ImageWidthMax", ImageWidthMax)
            End If
            Call Main.CloseCS(CS)
        End If
        If AllowCategories Then
            If Main.InStream(RequestNameBlogCategoryIDSet) Then
                BlogCategoryID = Main.GetStreamInteger(RequestNameBlogCategoryIDSet)
            Else
                BlogCategoryID = Main.GetStreamInteger(RequestNameBlogCategoryID)
            End If
            Call Main.AddRefreshQueryString(RequestNameBlogCategoryID, BlogCategoryID)
        End If
        '
        Call Main.TestPoint("Blogs.GetContent, Main.IsAuthenticated=" & Main.IsAuthenticated)
        Call Main.TestPoint("Blogs.GetContent, Main.MemberID=" & Main.MemberID)
        Call Main.TestPoint("Blogs.GetContent, BlogOwnerID=" & BlogOwnerID)
        Call Main.TestPoint("Blogs.GetContent, IsBlogOwner=" & IsBlogOwner)
        If IsBlogOwner And (BlogID = 0) Then
            '
            ' Blog record was not, or can not be created
            '
            s = s & Main.GetAdminHintWrapper("This blog has not been configured. Please edit this page and edit the properties for the blog Add-on")
        Else
            '
            ' Get the Feed Args
            '
            CSFeed = Main.OpenCSContentRecord(cnRSSFeeds, RSSFeedId)
            If Main.IsCSOK(CS) Then
                RSSFeedName = Main.GetCS(CSFeed, "Name")
                RSSFeedFilename = Main.GetCS(CSFeed, "RSSFilename")
                BlogRootLink = Main.GetCS(CSFeed, "Link")
            End If
            Call Main.CloseCS(CSFeed)
            '
            If BlogRootLink = "" Or RSSFeedFilename = "" Then
                '
                ' feed has not been initialized yet, call it now
                '
                Call VerifyFeedReturnArgs(BlogID, RSSFeedId, RSSFeedName, RSSFeedFilename, BlogRootLink)
            End If
            '
            ' Process Input
            '
            ButtonValue = Main.GetStreamText("button")
            FormID = Main.GetStreamInteger(RequestNameFormID)
            SourceFormID = Main.GetStreamInteger(RequestNameSourceFormID)
            KeywordList = Main.GetStreamText(RequestNameKeywordList)
            DateSearchText = Main.GetStreamText(RequestNameDateSearch)
            'BlogTitle = Main.GetStreamText(RequestNameBlogTitle)
            'BlogCopy = Main.GetStreamText(RequestNameBlogCopy)
            ArchiveMonth = Main.GetStreamInteger(RequestNameArchiveMonth)
            ArchiveYear = Main.GetStreamInteger(RequestNameArchiveYear)
            EntryID = Main.GetStreamInteger(RequestNameBlogEntryID)
            '
            WorkingQueryString = Main.RefreshQueryString
            If WorkingQueryString = "" Then
                WorkingQueryString = "?"
            Else
                WorkingQueryString = "?" & WorkingQueryString & "&"
            End If
            '
            If ButtonValue <> "" Then
                '
                ' Process the source form into form if there was a button - else keep formid
                '
                FormID = ProcessForm(SourceFormID, BlogID, IsBlogOwner, EntryID, ButtonValue, BlogName, BlogOwnerID, AllowAnonymous, AllowCategories, BlogCategoryID, RSSFeedId, BlogRootLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, autoApproveComments, authoringGroupId, emailComment, OptionString)
            End If
            '
            ' Get Next Form
            '
            s = s & GetForm(FormID, BlogID, BlogName, IsBlogOwner, ArchiveMonth, ArchiveYear, EntryID, KeywordList, ButtonValue, DateSearchText, AllowAnonymous, AllowCategories, BlogCategoryID, RSSFeedName, RSSFeedFilename, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogDescription, blogCaption, RSSFeedId)
        End If
    End If
    '
    GetContent = s
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetContent", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetForm(FormID As Long, BlogID As Long, BlogName As String, IsBlogOwner As Boolean, ArchiveMonth As Long, ArchiveYear As Long, EntryID As Long, KeywordList As String, ButtonValue As String, DateSearchText As String, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Long, RSSFeedName As String, RSSFeedFilename As String, ThumbnailImageWidth As Long, BuildVersion As String, ImageWidthMax As Long, blogDescription As String, blogCaption As String, RSSFeedId As Long) As String
    On Error GoTo ErrorTrap
    '
    Dim Stream As String
    Dim IsEditing As Boolean
    '
    'Call Main.AddRefreshQueryString(RequestNameSourceFormID, FormID)
    IsEditing = Main.IsAuthoring("Blogs")
    Select Case FormID
        Case FormBlogPostDetails
            Stream = Stream & GetFormBlogPostDetails(BlogID, EntryID, IsBlogOwner, AllowAnonymous, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax)
        Case FormBlogArchiveDateList
            Stream = Stream & GetFormBlogArchiveDateList(BlogID, BlogName, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax)
        Case FormBlogArchivedBlogs
            Stream = Stream & GetFormBlogArchivedBlogs(BlogID, BlogName, ArchiveMonth, ArchiveYear, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax)
        Case FormBlogEntryEditor
            Stream = Stream & GetFormBlogPost(BlogID, BlogName, IsBlogOwner, EntryID, AllowCategories, BlogCategoryID)
        Case FormBlogSearch
            Stream = Stream & GetFormBlogSearch(BlogID, BlogName, IsBlogOwner, KeywordList, ButtonValue, DateSearchText, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax)
        Case Else
            If EntryID <> 0 Then
                '
                ' Go to details page
                '
                FormID = FormBlogPostDetails
                Stream = Stream & GetFormBlogPostDetails(BlogID, EntryID, IsBlogOwner, AllowAnonymous, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax)
            Else
                '
                ' list all the entries
                '
                FormID = FormBlogPostList
                Stream = Stream & GetFormBlogPostList(BlogID, BlogName, IsBlogOwner, IsEditing, EntryID, AllowCategories, BlogCategoryID, RSSFeedName, RSSFeedFilename, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogDescription, blogCaption, RSSFeedId)
            End If
    End Select
    '
    GetForm = Stream
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetForm", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'   Processes the SourceFormID
'       returns the FormID for the next one to display
'====================================================================================
'
Private Function ProcessForm(SourceFormID As Long, BlogID As Long, IsBlogOwner As Boolean, EntryID As Long, ButtonValue As String, BlogName As String, BlogOwnerID As Long, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Long, RSSFeedId As Long, BlogRootLink As String, ThumbnailImageWidth As Long, BuildVersion As String, ImageWidthMax As Long, autoApproveComments As Boolean, authoringGroupId As Long, emailComment As Boolean, OptionString As String) As Long
    On Error GoTo ErrorTrap
    '
    If ButtonValue <> "" Then
        Select Case SourceFormID
            Case FormBlogPostList
                ProcessForm = FormBlogPostList
            Case FormBlogEntryEditor
                ProcessForm = ProcessFormBlogPost(SourceFormID, BlogID, EntryID, ButtonValue, BlogCategoryID, RSSFeedId, BlogRootLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax)
            Case FormBlogPostDetails
                ProcessForm = ProcessFormBlogPostDetails(SourceFormID, BlogID, IsBlogOwner, ButtonValue, BlogName, BlogOwnerID, AllowAnonymous, BlogCategoryID, autoApproveComments, authoringGroupId, emailComment, OptionString)
            Case FormBlogArchiveDateList
                 ProcessForm = FormBlogArchiveDateList
            Case FormBlogSearch
                ProcessForm = FormBlogSearch
            Case FormBlogArchivedBlogs
                ProcessForm = FormBlogArchivedBlogs
        End Select
    End If
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "ProcessForm", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetFormBlogSearch(BlogID As Long, BlogName As String, IsBlogOwner As Boolean, KeywordList As String, ButtonValue As String, DateSearchText As String, AllowCategories As Boolean, BlogCategoryID As Long, ThumbnailImageWidth As Long, BuildVersion As String, ImageWidthMax As Long) As String
    On Error GoTo ErrorTrap
    '
    'Dim EntryCopyOverview As String
    Dim BlogTagList As String
    Dim PodcastMediaLink As String
    Dim PodcastSize As String
    Dim Return_CommentCnt As Long
    Dim DateSearch As Date
    Dim Subcaption As String
    Dim AuthorMemberID As Long
    Dim ParentID As Long
    Dim ResultPtr As Long
    Dim EntryID As Long
    Dim EntryName As String
    Dim EntryCopy As String
    Dim CommentID As Long
    Dim CommentName As String
    Dim CommentCopy As String
    Dim AuthorMemberName As String
    Dim DateAdded As Date
    Dim Approved As Boolean
    Dim s As String
    Dim CS As Long
    Dim SQL As String
    Dim CommentPointer As Long
    Dim CommentCount As Long
    Dim CommentSQL As String
    Dim CommentString As String
    Dim CommentBlogID As Integer
    Dim CSP As String
    Dim Divider As String
    Dim qs As String
    '
    Dim CSPointer As Long
    Dim Criteria As String
    Dim SearchMonth As String
    Dim SearchYear As String
    Dim DateString As String
    Dim KeyWordsArray() As String
    Dim KeyWordsCounter As Long
    Dim KeyWordsArrayCounter As Long
    Dim EnteredKeyWords As String
    Dim CounterKeyWords As Long
    Dim OrClause As String
    Dim Button As String
    Dim OrCnt As Long
    Dim allowComments As Boolean
    Dim QueryTag As String
    '
    Call Main.AddRefreshQueryString(RequestNameSourceFormID, FormBlogSearch)
    Call Main.AddRefreshQueryString(RequestNameFormID, FormBlogSearch)
    Call Main.AddRefreshQueryString(RequestNameBlogEntryID, "")
    '
    s = s & vbCrLf & Main.GetContentCopy2("Blogs Search Header for " & BlogName, , "<h2>" & BlogName & " Blog Search</h2>")
    s = s & vbCrLf & Main.GetFormStart
    '
    ' Search results
    '
    QueryTag = Main.GetStreamText(RequestNameQueryTag)
    Button = Main.GetStreamText("button")
    If (Button = FormButtonSearch) Or (QueryTag <> "") Then
        '
        ' Attempt to figure out the date provided
        '
        If KmaEncodeDate(DateSearchText) <> CDate(0) Then
            DateSearch = KmaEncodeDate(DateSearchText)
            If DateSearch < CDate("1/1/2000") Then
                DateSearch = CDate(0)
                Call Main.AddUserError("The date is not valid")
            End If
        End If
        '
        Subcaption = ""
        Criteria = ""
        If (KeywordList <> "") Then
            KeywordList = "," & KeywordList & ","
            KeyWordsArray = Split(KeywordList, ",", , vbTextCompare)
            KeyWordsArrayCounter = UBound(KeyWordsArray)
            For CounterKeyWords = 0 To KeyWordsArrayCounter
                If KeyWordsArray(CounterKeyWords) <> "" Then
                    If Criteria <> "" Then
                        Criteria = Criteria & "or"
                        Subcaption = Subcaption & " or "
                    End If
                    EnteredKeyWords = KeyWordsArray(CounterKeyWords)
                    Subcaption = Subcaption & "'<i>" & kmaEncodeHTML(EnteredKeyWords) & "</i>'"
                    EnteredKeyWords = KmaEncodeSQLText(EnteredKeyWords)
                    EnteredKeyWords = "'%" & Mid(EnteredKeyWords, 2, Len(EnteredKeyWords) - 2) & "%'"
                    Criteria = Criteria & " (Copy like " & EnteredKeyWords & ")"
                End If
            Next
            If Subcaption <> "" Then
                Subcaption = " containing " & Subcaption
            End If
            If Criteria <> "" Then
                Criteria = "(" & Criteria & ")"
            End If
        End If
        If (DateSearch <> CDate(0)) Then
            SearchMonth = Month(DateSearch)
            SearchYear = Year(DateSearch)
            Subcaption = Subcaption & " in " & SearchMonth & "/" & SearchYear
            If Criteria <> "" Then
                Criteria = Criteria & "AND"
            End If
            Criteria = Criteria & "(DateAdded>=" & KmaEncodeSQLDate(DateSearch) & ")and(DateAdded<" & KmaEncodeSQLDate(AddMonth(DateSearch, 1)) & ")"
        End If
        If (QueryTag <> "") Then
            Subcaption = Subcaption & " tagged with '<i>" & kmaEncodeHTML(QueryTag) & "</i>'"
            If Criteria <> "" Then
                Criteria = Criteria & "AND"
            End If
            QueryTag = KmaEncodeSQLText(QueryTag)
            QueryTag = "'%" & Mid(QueryTag, 2, Len(QueryTag) - 2) & "%'"
            Criteria = Criteria & " (taglist like " & QueryTag & ")"
        End If
        If Subcaption <> "" Then
            Subcaption = "Search for entries and comments " & Subcaption
        End If
        If Main.IsUserError Then
            Subcaption = Subcaption & Main.GetUserError
        End If
        '
        If Criteria = "" Then
            CSPointer = -1
        Else
            CSPointer = Main.OpenCSContent(cnBlogCopy, Criteria)
        End If
        '
        ' Display the results
        '
        s = s & cr & "<div class=""aoBlogEntryCopy"">" & Subcaption & "</div>"
        s = s & Main.GetFormStart
        s = s & "<table width=100% border=0 cellspacing=0 cellpadding=5>"
        '
        If Not Main.CSOK(CSPointer) Then
            s = s & cr & "<div class=""aoBlogProblem"">There were no matches to your search</div>"
        Else
            s = s & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
            Do While Main.CSOK(CSPointer)
                ParentID = Main.GetCSInteger(CSPointer, "EntryID")
                If ParentID = 0 Then
                    '
                    ' Entry
                    '
                    AuthorMemberID = Main.GetCSInteger(CSPointer, "AuthorMemberID")
                    AuthorMemberName = Main.GetRecordName("people", AuthorMemberID)
                    EntryID = Main.GetCSInteger(CSPointer, "ID")
                    EntryName = Main.GetCSText(CSPointer, "name")
                    EntryCopy = Main.GetCSText(CSPointer, "copy")
                    'EntryCopyOverview = Main.GetCSText(CSPointer, "copyOverview")
                    DateAdded = Main.GetCSDate(CSPointer, "DateAdded")
                    allowComments = Main.GetCSBoolean(CSPointer, "allowComments")
                    PodcastMediaLink = Main.GetCS(CSPointer, "PodcastMediaLink")
                    PodcastSize = Main.GetCS(CSPointer, "PodcastSize")
                    BlogTagList = Main.GetCS(CSPointer, "TagList")
                    s = s & GetBlogEntryCell(ResultPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, AuthorMemberName, DateAdded, False, True, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, "", ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList)
                    's = s & GetBlogEntryCell(ResultPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, AuthorMemberName, DateAdded, False, True, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, "", ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, EntryCopyOverview)
                Else
                    '
                    ' Comment
                    '
                    AuthorMemberID = Main.GetCSInteger(CSPointer, "AuthorMemberID")
                    AuthorMemberName = Main.GetRecordName("people", AuthorMemberID)
                    CommentID = Main.GetCSInteger(CSPointer, "ID")
                    CommentName = Main.GetCSText(CSPointer, "name")
                    CommentCopy = Main.GetCSText(CSPointer, "copyText")
                    DateAdded = Main.GetCSDate(CSPointer, "DateAdded")
                    Approved = Main.GetCSBoolean(CSPointer, "Approved")
                    EntryID = Main.GetCSInteger(CSPointer, "EntryID")
                    EntryName = Main.GetRecordName("Blog Copy", EntryID)
                    s = s & GetBlogCommentCell(IsBlogOwner, AuthorMemberName, DateAdded, Approved, CommentName, CommentCopy, CommentID, ResultPtr, 0, AuthorMemberID, EntryID, True, EntryName)
                End If
                '
                s = s & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                ResultPtr = ResultPtr + 1
                Call Main.NextCSRecord(CSPointer)
            Loop
        End If
        Call Main.CloseCS(CSPointer)
        s = s & "</table>"
    End If
    '
    ' Blog Search Form
    '
    s = s & "<table width=100% border=0 cellspacing=0 cellpadding=5>"
    s = s & GetFormRow("Date:", GetField(RequestNameDateSearch, , 10, , Main.GetStreamText(RequestNameDateSearch)) & " " & "&nbsp;(mm/yyyy)")
    s = s & GetFormRow("Keyword(s):", GetField(RequestNameKeywordList, KeywordList, , 30, Main.GetStreamText(RequestNameKeywordList)))
    s = s & GetFormRow("", Main.GetFormButton(FormButtonSearch))
    qs = Main.RefreshQueryString()
    qs = ModifyQueryString(qs, RequestNameSourceFormID, "")
    qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
    s = s & vbCrLf & GetFormRow2("<div class=""aoBlogFooterLink""><a href=""?" & qs & """>" & BackToRecentPostsMsg & "</a></div>")
    s = s & "</table>"
    s = s & Main.GetFormEnd
    '
    GetFormBlogSearch = s
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetFormBlogSearch", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetFormBlogArchiveDateList(BlogID As Long, BlogName As String, IsEditing As Boolean, IsBlogOwner As Boolean, AllowCategories As Boolean, BlogCategoryID As Long, ThumbnailImageWidth As Long, BuildVersion As String, ImageWidthMax As Long) As String
    On Error GoTo ErrorTrap
    '
    Dim s As String
    Dim CSPointer As Long
    Dim ArchiveList As String ' ** link for different month
    Dim TitleBlog As String
    Dim DateBlog As String
    Dim CopyBlog As String
    Dim MonthCounter As Long
    Dim ThisMonth As Long
    Dim ThisYear As Long
    Dim ArchiveMonth As Long
    Dim ArchiveYear As Long
    Dim Counter As Long
    Dim NameOfMonth As String
    Dim CS As Long
    Dim DateAddedSQL As String
    Dim FieldList As String
    Dim qs As String
    Dim RowCnt As Long
    Dim SQL As String
    '
    s = s & vbCrLf & Main.GetContentCopy2("Blogs Archives Header for " & BlogName, , "<h2>" & BlogName & " Blog Archive</h2>")
    '
    'FieldList = "distinct Month(DateAdded) as ArchiveMonth, year(dateadded) as ArchiveYear"
    qs = Main.RefreshQueryString()
    qs = ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
    qs = ModifyQueryString(qs, RequestNameSourceFormID, FormBlogArchiveDateList)
    'CS = Main.OpenCSContent(cnBlogEntries, "", "Year(DateAdded), Month(DateAdded) desc", , , , FieldList)
    
    SQL = "SELECT distinct Month(DateAdded) as ArchiveMonth, year(dateadded) as ArchiveYear "
    SQL = SQL & " From ccBlogCopy"
    SQL = SQL & " Where (ContentControlID = " & Main.GetContentID(cnBlogEntries) & ") And (Active <> 0)"
    SQL = SQL & " AND (BlogID=" & BlogID & ")"
    SQL = SQL & " ORDER BY year(dateadded) desc, Month(DateAdded) desc"
    
    CS = Main.OpenCSSQL("Default", SQL)
    
    If Not Main.IsCSOK(CS) Then
        '
        ' No archives, give them an error
        '
        s = s & cr & "<div class=""aoBlogProblem"">There are no current blog entries</div>"
        s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>" & BackToRecentPostsMsg & "</a></div>"
        
    Else
        RowCnt = Main.GetCSRowCount(CS)
        If RowCnt = 0 Then
            '
            ' weird - just give them the same error
            '
            s = s & cr & "<div class=""aoBlogProblem"">There are no current blog posts</div>"
            s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>" & BackToRecentPostsMsg & "</a></div>"
        ElseIf RowCnt = 1 Then
            '
            ' one archive - just display it
            '
            ArchiveMonth = Main.GetCSInteger(CS, "ArchiveMonth")
            ArchiveYear = Main.GetCSInteger(CS, "ArchiveYear")
            s = s & GetFormBlogArchivedBlogs(BlogID, BlogName, ArchiveMonth, ArchiveYear, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax)
        Else
            '
            ' Display List of archive
            '
            's = s & vbCrLf & vbTab & "<table width=100% border=0 cellspacing=0 cellpadding=5>"
            Do While Main.IsCSOK(CS)
                ArchiveMonth = Main.GetCSInteger(CS, "ArchiveMonth")
                ArchiveYear = Main.GetCSInteger(CS, "ArchiveYear")
                NameOfMonth = MonthName(ArchiveMonth)
                s = s & vbCrLf & vbTab & vbTab & "<div class=""aoBlogArchiveLink""><a href=""" & Main.ServerPage & WorkingQueryString & RequestNameFormID & "=" & FormBlogArchivedBlogs & "&" & RequestNameArchiveMonth & "=" & ArchiveMonth & "&" & RequestNameArchiveYear & "=" & ArchiveYear & "&" & RequestNameSourceFormID & "=" & FormBlogArchiveDateList & """>" & NameOfMonth & " " & ArchiveYear & "</a></div>"
                Main.NextCSRecord (CS)
            Loop
            's = s & vbCrLf & vbTab & "</table>"
            s = s & vbCrLf & vbTab & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>" & BackToRecentPostsMsg & "</a></div>"
        End If
    End If
    '
    GetFormBlogArchiveDateList = s
    
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetFormBlogArchiveDateList", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetFormBlogArchivedBlogs(BlogID As Long, BlogName As String, ArchiveMonth As Long, ArchiveYear As Long, IsEditing As Boolean, IsBlogOwner As Boolean, AllowCategories As Boolean, BlogCategoryID As Long, ThumbnailImageWidth As Long, BuildVersion As String, ImageWidthMax As Long) As String
    On Error GoTo ErrorTrap
    '
    'Dim EntryCopyOverview As String
    Dim PodcastMediaLink As String
    Dim PodcastSize As String
    Dim Return_CommentCnt As Long
    Dim qs As String
    Dim EntryCopy As String
    Dim EntryName As String
    Dim entryEditLink As String
    Dim DateAdded As Date
    Dim AuthorMemberName As String
    Dim AuthorMemberID As Long
    Dim EntryID As Long
    Dim BlogTagList As String
    Dim EntryPtr As Long
    Dim s As String
    Dim ArchivesSQL As Long
    Dim ArchivesPointer As Long
    Dim SelectedDate As String
    Dim BlahDate As Long
    Dim CurrentBLogPointer As Long
    Dim CurrentBlogID As Long
    Dim PageNumber As Long
    Dim CS As Long
    Dim allowComments As Boolean
    '
    ' If it is the current month, start at entry 6
    '
    PageNumber = 1
    If Month(Now) = ArchiveMonth And Year(Now) = ArchiveYear Then
        PageNumber = 2
    End If
    IsEditing = Main.IsAuthoring("Blogs")
    s = s & vbCrLf & Main.GetFormStart
    '
    ' List Blog Entries
    '
    CS = Main.OpenCSContent(cnBlogEntries, "(Month(DateAdded)=" & ArchiveMonth & ")and(year(DateAdded)=" & ArchiveYear & ")and(BlogID=" & BlogID & ")", "DateAdded Desc", , , , , PostsToDisplay, PageNumber)
    If Not Main.CSOK(CS) Then
        s = s & cr & "<div class=""aoBlogProblem"">There are no blog archives for " & ArchiveMonth & "/" & ArchiveYear & "</div>"
    Else
        EntryPtr = 0
        entryEditLink = ""
        Do While Main.CSOK(CS)
            EntryID = Main.GetCSInteger(CS, "ID")
            AuthorMemberID = Main.GetCSInteger(CS, "AuthorMemberID")
            AuthorMemberName = Main.GetRecordName("people", AuthorMemberID)
            DateAdded = Main.GetCSText(CS, "DateAdded")
            EntryName = Main.GetCSText(CS, "Name")
            If IsEditing Then
                entryEditLink = Main.GetCSRecordEditLink(CS)
            End If
            EntryCopy = Main.GetCSText(CS, "Copy")
            'EntryCopyOverview = Main.GetCSText(CS, "copyOverview")
            allowComments = Main.GetCSBoolean(CS, "allowComments")
            PodcastMediaLink = Main.GetCS(CS, "PodcastMediaLink")
            PodcastSize = Main.GetCS(CS, "PodcastSize")
            BlogTagList = Main.GetCS(CS, "TagList")
            s = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, AuthorMemberName, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList)
            's = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, AuthorMemberName, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, EntryCopyOverview)
            Call Main.NextCSRecord(CS)
            'If Main.CSOK(CS) Then
                s = s & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
            'End If
            EntryPtr = EntryPtr + 1
        Loop
    End If
    s = s & cr & "<div>&nbsp;</div>"
    s = s & cr & "<div class=""aoBlogFooterLink""><a href=""" & Main.ServerPage & WorkingQueryString & RequestNameFormID & "=" & FormBlogSearch & """>Search</a></div>"
    qs = Main.RefreshQueryString()
    qs = ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
    qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
    s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>" & BackToRecentPostsMsg & "</a></div>"
    Call Main.CloseCS(CS)
    '
    's = s & Main.GetFormInputHidden(RequestNameBlogEntryID, CommentBlogID)
    s = s & Main.GetFormInputHidden(RequestNameSourceFormID, FormBlogArchivedBlogs)
    s = s & Main.GetFormEnd
    '
    GetFormBlogArchivedBlogs = s
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetFormBlogArchivedBlogs", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetFormBlogThankYou(BlogID As Long, AllowCategories As Boolean) As String
    On Error GoTo ErrorTrap
    '
    Dim Stream As String
    '
    Stream = Stream & Main.GetFormStart
    Stream = Stream & "<table width=100% border=0 cellspacing=0 cellpadding=5>"
    Stream = Stream & GetFormRow("", Main.GetContentCopy("Blog Thank You Page"))
    Stream = Stream & GetFormRow2("<BR><BR><BR>")
    Stream = Stream & GetFormRow2("<a href=""" & Main.ServerPage & "?" & WorkingQueryString & "&" & RequestNameFormID & "=" & FormBlogPostList & """>Main Blog page</a>")
    '
    Stream = Stream & "</table>"
    Stream = Stream & Main.GetFormEnd
    '
    GetFormBlogThankYou = Stream
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetFormBlogThankYou", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetFormBlogCommentThankYou(BlogID As Long) As String
    On Error GoTo ErrorTrap
    '
    Dim Stream As String
    '
    Stream = Stream & Main.GetFormStart
    Stream = Stream & "<table width=100% border=0 cellspacing=0 cellpadding=5>"
    Stream = Stream & GetFormRow("", Main.GetContentCopy("Blog comment post thank you note"))
    Stream = Stream & GetFormRow2("<BR><BR><BR>")
    Stream = Stream & GetFormRow2("<a href=""" & Main.ServerPage & "?" & WorkingQueryString & "&" & RequestNameFormID & "=" & FormBlogPostList & """>Main Blog page</a>")
    '
    Stream = Stream & "</table>"
    Stream = Stream & Main.GetFormEnd
    '
    GetFormBlogCommentThankYou = Stream
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetFormBlogCommentThankYou", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetFormRow(FieldCaption As String, Innards As String, Optional AlignLeft As Boolean)
    On Error GoTo ErrorTrap
    '
    Dim Stream As String
    Dim AlignmentString As String
    '
    If Not AlignLeft Then
        AlignmentString = " align=right"
    Else
        AlignmentString = " align=left"
        End If
    '
    Stream = Stream & "<TR>"
    Stream = Stream & "<TD style=""width:100px;vertical-align:top"" " & AlignmentString & ">" & FieldCaption & "<img src=""/cclib/images/spacer.gif"" width=100 height=1 alt="" ""></TD>"
    Stream = Stream & "<TD style=""width:100%;vertical-align:top"">" & Innards & "</TD>"
    Stream = Stream & "</TR>"
    '
    GetFormRow = Stream
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetFormRow", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetFormRow2(Innards As String) As String
    On Error GoTo ErrorTrap
    '
    Dim Stream As String
    '
    Stream = Stream & "<TR>"
    Stream = Stream & "<TD colspan=2 width=""100%"">" & Innards & "</TD>"
    Stream = Stream & "</TR>"
    '
    GetFormRow2 = Stream
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetFormRow2", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetField(RequestName As String, Optional Height As String, Optional Width As String, Optional MaxLenghth As String, Optional DefaultValue As String) As String
    On Error GoTo ErrorTrap
    '
    Dim Stream As String
    '
    If Height = "" Then
        Height = 1
        End If
    If Width = "" Then
        Width = 25
        End If
    '
    Stream = Main.GetFormInputText(RequestName, DefaultValue, Height, Width, RequestName)
    Stream = Replace(Stream, "<INPUT ", "<INPUT maxlength=" & MaxLenghth & " ", 1, 99, 1)
    '
    GetField = Stream
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetField", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function Random(Lowerbound As Long, Upperbound As Long)
    On Error GoTo ErrorTrap
    '
        Randomize
        Random = Int(Rnd * Upperbound) + Lowerbound
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetField", Err.Number, Err.Source, Err.Description, True, False)
End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetFormBlogPostList(BlogID As Long, BlogName As String, IsBlogOwner As Boolean, IsEditing As Boolean, EntryID As Long, AllowCategories As Boolean, BlogCategoryID As Long, RSSFeedName As String, RSSFeedFilename As String, ThumbnailImageWidth As Long, BuildVersion As String, ImageWidthMax As Long, blogDescription As String, blogCaption As String, RSSFeedId As Long) As String
    On Error GoTo ErrorTrap
    '
    'Dim EntryCopyOverview As String
    Dim BlogTagList As String
    Dim entryEditLink As String
    Dim PodcastMediaLink As String
    Dim PodcastSize As String
    Dim FeedFooter As String
    Dim ReturnFooter As String
    Dim CategoryFooter As String
    Dim TestCategoryID As Long
    Dim GroupList As String
    Dim BlogCategoryName As String
    Dim Return_CommentCnt As Long
    Dim qs As String
    Dim CommentPtr As Long
    Dim CommentID As Long
    Dim Approved As Boolean
    Dim CommentName As String
    Dim CommentCopy As String
    Dim s As String
    Dim CS As Long
    Dim SQL As String
    Dim CommentPointer As Long
    Dim CommentCount As Long
    Dim CommentSQL As String
    Dim CommentString As String
    Dim CommentBlogID As Integer
    Dim CSP As String
    Dim Divider As String
    Dim AddBlogFlag As Boolean
    Dim SearchFlag As Boolean
    Dim CSCount As Long
    Dim AuthorMemberName As String
    Dim AuthorMemberID As Long
    Dim DateAdded As Date
    Dim RowCopy As String
    'Dim IsEditing As Boolean
    Dim EntryName As String
    Dim Criteria As String
    Dim EntryCopy As String
    Dim allowComments As Boolean
    Dim EntryPtr As Long
    Dim locBlogTitle As String
    Dim IsBlocked As Boolean
    Dim CSCat As Long
    Dim Patch As New PatchClass
    Dim NoneMsg As String
    '
    locBlogTitle = BlogName
    If InStr(1, locBlogTitle, " Blog", vbTextCompare) = 0 Then
        locBlogTitle = locBlogTitle & " blog"
    End If
    '
    If blogCaption <> "" Then
        s = s & vbCrLf & "<h2 class=""aoBlogCaption"">" & blogCaption & "</h2>"
    End If
    If blogDescription <> "" Then
        s = s & vbCrLf & "<div class=""aoBlogDescription"">" & blogDescription & "</div>"
    End If
    s = s & vbCrLf & Main.GetFormStart
    '
    ' List Blog Entries
    '
    EntryPtr = 0
    '
    ' Display the most recent entries
    '
    If AllowCategories And (BlogCategoryID <> 0) Then
        BlogCategoryName = Main.GetRecordName("Blog Categories", BlogCategoryID)
        CS = Main.OpenCSContent(cnBlogEntries, "(BlogID=" & BlogID & ")and(BlogCategoryID=" & BlogCategoryID & ")", "DateAdded Desc")
        s = s & cr & "<div class=""aoBlogCategoryCaption"">Category " & BlogCategoryName & "</div>"
        NoneMsg = "There are no blog posts available in the category " & BlogCategoryName
    Else
        CS = Main.OpenCSContent(cnBlogEntries, "(BlogID=" & BlogID & ")", "DateAdded Desc")
        NoneMsg = "There are no blog posts available"
    End If
    If Not Main.CSOK(CS) Then
        s = s & cr & "<div class=""aoBlogProblem"">" & NoneMsg & "</div>"
    Else
        Do While Main.CSOK(CS) And EntryPtr < PostsToDisplay
            TestCategoryID = Main.GetCSInteger(CS, "BlogCategoryid")
            If TestCategoryID <> 0 Then
                '
                ' Check that this member has access to this category
                '
                IsBlocked = Not Main.IsAdmin()
                If IsBlocked Then
                    '
                    'If IsBlocked Then
                        If Main.ContentServerVersion < "4.1.098" Then
                            CSCat = Main.OpenCSContent(cnBlogCategories, "id=" & TestCategoryID)
                            If Main.IsCSOK(CSCat) Then
                                IsBlocked = Main.GetCSBoolean(CSCat, "UserBlocking")
                            End If
                            Call Main.CloseCS(CSCat)
                            '
                            If IsBlocked Then
                                GroupList = Patch.GetBlockingGroups(Main, TestCategoryID)
                                IsBlocked = Not Patch.IsGroupListMember(Main, GroupList)
                            End If
                        Else
                            CSCat = Main.OpenCSContent(cnBlogCategories, "id=" & TestCategoryID)
                            If Main.IsCSOK(CSCat) Then
                                IsBlocked = Main.GetCSBoolean(CSCat, "UserBlocking")
                                If IsBlocked Then
                                    GroupList = Main.GetCS(CSCat, "BlockingGroups")
                                    IsBlocked = Not Main.IsGroupListMember(GroupList)
                                End If
                            End If
                            Call Main.CloseCS(CSCat)
                        End If
                    'End If
                End If
            End If
            If IsBlocked Then
                '
                '
                '
            Else
                EntryID = Main.GetCSInteger(CS, "ID")
                AuthorMemberID = Main.GetCSInteger(CS, "AuthorMemberID")
                AuthorMemberName = Main.GetRecordName("people", AuthorMemberID)
                DateAdded = Main.GetCSText(CS, "DateAdded")
                EntryName = Main.GetCSText(CS, "Name")
                If IsEditing Then
                    entryEditLink = Main.GetCSRecordEditLink(CS)
                End If
                EntryCopy = Main.GetCSText(CS, "Copy")
                'EntryCopyOverview = Main.GetCSText(CS, "CopyOverview")
                allowComments = Main.GetCSBoolean(CS, "AllowComments")
                PodcastMediaLink = Main.GetCS(CS, "PodcastMediaLink")
                PodcastSize = Main.GetCS(CS, "PodcastSize")
                BlogTagList = Main.GetCS(CS, "BlogTagList")
                s = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, AuthorMemberName, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList)
                's = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, AuthorMemberName, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, EntryCopyOverview)
                'If Main.CSOK(CS) Then
                    s = s & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                'End If
                EntryPtr = EntryPtr + 1
            End If
            Call Main.NextCSRecord(CS)
        Loop
    End If
    '
    ' Build Footers
    '
    If Main.IsAdmin() And AllowCategories Then
        qs = "cid=" & Main.GetContentID("Blog Categories") & "&af=4"
        CategoryFooter = CategoryFooter & cr & "<div class=""aoBlogFooterLink""><a href=""" & Main.SiteProperty_AdminURL & "?" & qs & """>Add a new category</a></div>"
    End If
    ReturnFooter = ""
    If AllowCategories Then
        'If BlogCategoryID <> 0 Then
            '
            ' View all categories
            '
            qs = Main.RefreshQueryString
            qs = ModifyQueryString(qs, RequestNameBlogCategoryIDSet, "0", True)
            CategoryFooter = CategoryFooter & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>See posts in all categories</a></div>"
        'Else
            '
            ' select a category
            '
            qs = Main.RefreshQueryString
            CS = Main.OpenCSContent(cnBlogCategories)
            Do While Main.IsCSOK(CS)
                BlogCategoryID = Main.GetCSInteger(CS, "id")
                IsBlocked = Csv.GetCSBoolean(CS, "UserBlocking")
                If IsBlocked Then
                    IsBlocked = Not Main.IsAdmin()
                End If
                If IsBlocked Then
                    If Main.ContentServerVersion < "4.1.098" Then
                        GroupList = Patch.GetBlockingGroups(Main, BlogCategoryID)
                        IsBlocked = Not Patch.IsGroupListMember(Main, GroupList)
                    Else
                        GroupList = Main.GetCS(CS, "BlockingGroups")
                        IsBlocked = Not Main.IsGroupListMember(GroupList)
                    End If
                End If
                If Not IsBlocked Then
                    qs = ModifyQueryString(qs, RequestNameBlogCategoryIDSet, CStr(BlogCategoryID), True)
                    CategoryFooter = CategoryFooter & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>See posts in the category " & Main.GetCS(CS, "name") & "</a></div>"
                End If
                Call Main.NextCSRecord(CS)
            Loop
            Call Main.CloseCS(CS)
        'End If
    End If
    Call Main.CloseCS(CS)
    '
    ' Footer
    '
    s = s & cr & "<div>&nbsp;</div>"
    If IsBlogOwner Then
        Call Main.TestPoint("Blogs.GetFormBlogPostList, IsBlogOwner=true, appending 'create' message")
        '
        ' Create a new entry if this is the Blog Owner
        '
        qs = Main.RefreshQueryString
        qs = ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor, True)
        s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Create new post</a></div>"
        '
        ' Create a link to edit the blog record
        '
        qs = "cid=" & Main.GetContentID("Blogs") & "&af=4&id=" & BlogID
        s = s & cr & "<div class=""aoBlogFooterLink""><a href=""" & Main.SiteProperty_AdminURL & "?" & qs & """>Edit blog features</a></div>"
        '
        ' Create a link to edit the rss record
        '
        If RSSFeedId = 0 Then
        Else
            qs = "cid=" & Main.GetContentID("RSS Feeds") & "&af=4&id=" & RSSFeedId
            s = s & cr & "<div class=""aoBlogFooterLink""><a href=""" & Main.SiteProperty_AdminURL & "?" & qs & """>Edit rss feed features</a></div>"
        End If
    End If
    s = s & ReturnFooter
    s = s & CategoryFooter
    '
    ' Search
    '
    qs = Main.RefreshQueryString
    qs = ModifyQueryString(qs, RequestNameFormID, FormBlogSearch, True)
    s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
    '
    ' Link to archives
    '
    qs = Main.RefreshQueryString
    qs = ModifyQueryString(qs, RequestNameFormID, FormBlogArchiveDateList, True)
    s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>See archives</a></div>"
    '
    ' Link to RSS Feed
    '
    FeedFooter = ""
    If RSSFeedFilename = "" Then
        '
    Else
        FeedFooter = "<a href=""http://" & Main.ServerDomain & "/RSS/" & RSSFeedFilename & """>"
        FeedFooter = "rss feed " _
            & FeedFooter & RSSFeedName & "</a>" _
            & "&nbsp;" _
            & FeedFooter & "<img src=""/cclib/images/IconXML-25x13.gif"" width=25 height=13 class=""aoBlogRSSFeedImage""></a>" _
            & ""
        s = s & cr & "<div class=""aoBlogFooterLink"">" & FeedFooter & "</div>"
    End If
    '
    s = s & Main.GetFormInputHidden(RequestNameBlogEntryID, CommentBlogID)
    s = s & Main.GetFormInputHidden(RequestNameSourceFormID, FormBlogPostList)
    s = s & Main.GetFormEnd
    '
    GetFormBlogPostList = s
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetFormBlogPostList", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetFormBlogPost(BlogID As Long, BlogName As String, IsBlogOwner As Boolean, EntryID As Long, AllowCategories As Boolean, BlogCategoryID As Long) As String
    On Error GoTo ErrorTrap
    '
    Dim editor As String
    Dim BlogCopy As String
    Dim BlogTagList As String
    Dim Ptr As Long
    Dim imageName As String
    Dim imageDescription As String
    Dim imageFilename As String
    Dim ImageID As Long
    Dim Pos As Long
    Dim CategorySelect As String
    Dim Hint As String
    Dim Button As String
    Dim qs As String
    Dim c As String
    Dim s As String
    Dim CS As Long
    Dim SQL As String
    Dim CommentPointer As Long
    Dim CommentCount As Long
    Dim CommentSQL As String
    Dim CommentString As String
    Dim CommentBlogID As Integer
    Dim Divider As String
    Dim BlogTitle As String
    '
    Hint = "1"
    s = s & Main.GetUploadFormStart
    s = s & "<table width=100% border=0 cellspacing=0 cellpadding=5>"
    '
    'AddBlogFlag = Main.GetStreamBoolean(RequestNameAddBlogFlag)
    '
    If EntryID = 0 Then
        Hint = "2"
        '
        ' New Entry that is being saved
        '
        s = s & GetFormRow2(Main.GetContentCopy2("Blog Create Header for " & BlogName, , "<h2>Create a new blog post</h2>"))
        BlogTitle = Main.GetStreamText(RequestNameBlogTitle)
        BlogCopy = Main.GetStreamText(RequestNameBlogCopy)
        BlogTagList = Main.GetStreamText(RequestNameBlogTagList)
    Else
        Hint = "3"
        '
        ' Edit an entry
        '
        CS = Main.OpenCSContentRecord(cnBlogEntries, EntryID)
        If Main.IsCSOK(CS) Then
            BlogTitle = Main.GetCSText(CS, "Name")
            BlogCopy = Main.GetCSText(CS, "Copy")
            BlogCategoryID = Main.GetCSInteger(CS, "BlogCategoryID")
            BlogTagList = Main.GetCSText(CS, "TagList")
        End If
        Call Main.CloseCS(CS)
        If BlogCopy = "" Then
            BlogCopy = "<!-- cc --><p><br></p><!-- /cc -->"
        End If
    End If
    Hint = "4"
    If Main.SiteProperty_BuildVersion < "4.1.515" Then
        '
        ' block active content
        '
        editor = Main.GetFormInputHTML3(RequestNameBlogCopy, BlogCopy, 500, "100%", False, False)
    Else
        '
        ' contensive enables tools by the permission level
        '
        editor = Main.GetFormInputHTML3(RequestNameBlogCopy, BlogCopy, 500, "100%", False, True)
    End If
    s = s & GetFormRow("<div style=""padding-top:3px"">Title: </div>", Main.GetFormInputText(RequestNameBlogTitle, BlogTitle, 1, 50))
    s = s & GetFormRow("<div style=""padding-top:108px"">Post: </div>", editor)
    s = s & GetFormRow("<div style=""padding-top:3px"">Tag List: </div>", Main.GetFormInputText(RequestNameBlogTagList, BlogTagList, 5, 50))
    If AllowCategories Then
        Hint = "5"
        CategorySelect = Main.GetFormInputSelect(RequestNameBlogCategoryIDSet, BlogCategoryID, "Blog Categories")
        If (InStr(1, CategorySelect, "<option value=""""></option></select>", vbTextCompare) <> 0) Then
            '
            ' Select is empty
            '
            CategorySelect = "<div>This blog has no categories defined</div>"
        End If
        s = s & GetFormRow("Category: ", CategorySelect)
    End If
    '
    ' file upload form taken from Resource Library
    '
    c = ""
    c = c _
        & "<TABLE id=""UploadInsert"" border=""0"" cellpadding=""0"" cellspacing=""1"" width=""100%"">" _
        & "<TR>" _
        & ""
    '
    SQL = "select i.filename,i.description,i.name,i.id from BlogImages i left join BlogImageRules r on r.blogimageid=i.id where i.active<>0 and r.blogentryid=" & EntryID & " order by i.SortOrder"
    CS = Main.OpenCSSQL("default", SQL)
    Ptr = 1
    Do While Main.IsCSOK(CS)
        ImageID = Main.GetCSInteger(CS, "id")
        Call GetBlogImage(ImageID, 200, 0, imageFilename, "", "", "")
        imageDescription = Main.GetCSText(CS, "description")
        imageName = Main.GetCSText(CS, "name")
        '
        '   row delimiter
        '
        If Ptr <> 1 Then
            c = c _
                & "<TR style=""padding-bottom:10px; padding-bottom:10px;"">" _
                & "<TD><HR></TD>" _
                & "</TR>" _
                & ""
        End If
        '
        '   order
        '
        c = c _
            & "<TR style=""padding-bottom:10px;"">" _
            & "<TD style=""text-align:left;vertical-align:top"">Order<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageOrder & "." & Ptr & """ SIZE=""2"" value=""" & Ptr & """></TD>" _
            & "</TR>" _
            & ""
        '
        '   image
        '
        c = c _
            & "<TR style=""padding-bottom:10px;"">" _
            & "<TD style=""text-align:left;vertical-align:top""><input type=""checkbox"" name=""" & rnBlogImageDelete & "." & Ptr & """>del</TD>" _
            & "</TR>" _
            & "<TR style=""padding-bottom:10px;"">" _
            & "<TD align=""left"" class=""ccAdminSmall""><img src=""" & Main.serverFilePath & imageFilename & """></TD>" _
            & "</TR>" _
            & ""
        '
        '   name
        '
        c = c _
            & "<TR>" _
            & "<TD style=""text-align:left;vertical-align:top"">Name<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageName & "." & Ptr & """ SIZE=""25"" value=""" & kmaEncodeHTML(imageName) & """></TD>" _
            & "</TR>" _
            & ""
        '
        '   description
        '
        c = c _
            & "<TR>" _
            & "<TD align=""left"">Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50"">" & kmaEncodeHTML(imageDescription) & "</TEXTAREA><input type=""hidden"" name=""" & rnBlogImageID & "." & Ptr & """ value=""" & ImageID & """></TD>" _
            & "</TR>" _
            & ""
        '
        Call Main.NextCSRecord(CS)
        Ptr = Ptr + 1
    Loop
    Call Main.CloseCS(CS)
    '
    '   row delimiter
    '
    c = c _
        & "<TR style=""padding-bottom:10px; padding-bottom:10px;"">" _
        & "<TD><HR></TD>" _
        & "</TR>" _
        & ""
    '
    '   order
    '
    c = c _
        & "<TR>" _
        & "<TD align=""left"">Order<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageOrder & "." & Ptr & """ SIZE=""2"" value=""" & Ptr & """></TD>" _
        & "</TR>" _
        & ""
    '
    '   image
    '
    c = c _
        & "<TR>" _
        & "<TD align=""left"">Image<br><INPUT TYPE=""file"" name=""LibraryUpload." & Ptr & """></TD>" _
        & "</TR>" _
        & ""
    '
    '   name
    '
    c = c _
        & "<TR>" _
        & "<TD align=""left"">Name<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageName & "." & Ptr & """ SIZE=""25""></TD>" _
        & "</TR>" _
        & ""
    '
    '   description
    '
    c = c _
        & "<TR>" _
        & "<TD align=""left"">Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50""></TEXTAREA></TD>" _
        & "</TR>" _
        & ""
    '
    c = c _
        & "</Table>" _
        & ""
        '
    c = c _
        & "<TABLE border=""0"" cellpadding=""0"" cellspacing=""1"" width=""100%"">" _
        & "<TR><TD Width=""30""><img src=/ResourceLibrary/spacer.gif width=30 height=1></TD><TD align=""left""><a href=""#"" onClick=""blogNewRow();return false;"">+ upload more files</a></TD></TR>" _
        & "</Table>" & Main.GetFormInputHidden("LibraryUploadCount", Ptr, "LibraryUploadCount") _
        & ""
    '
    s = s & GetFormRow("Images: ", c)
    If EntryID <> 0 Then
        Hint = "6"
        s = s & GetFormRow("", Main.GetFormButton(FormButtonPost) & "&nbsp;" & Main.GetFormButton(FormButtonCancel) & "&nbsp;" & Main.GetFormButton(FormButtonDelete))
    Else
        Hint = "7"
        s = s & GetFormRow("", Main.GetFormButton(FormButtonPost) & "&nbsp;" & Main.GetFormButton(FormButtonCancel))
    End If
    Hint = "8"
    qs = Main.RefreshQueryString()
    qs = ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
    qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
    Hint = "9"
    s = s & vbCrLf & GetFormRow2("<div class=""aoBlogFooterLink""><a href=""?" & qs & """>" & BackToRecentPostsMsg & "</a></div>")
    
    '
    s = s & Main.GetFormInputHidden(RequestNameBlogEntryID, EntryID)
    s = s & Main.GetFormInputHidden(RequestNameSourceFormID, FormBlogEntryEditor)
    s = s & "</table>"
    Hint = "95"
    s = s & Main.GetFormEnd
    '
    GetFormBlogPost = s
    Hint = "96"
    Call Main.SetVisitProperty(SNBlogEntryName, CStr(GetRandomInteger()))
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetFormBlogPost, hint=" & Hint, Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function ProcessFormBlogPost(SourceFormID As Long, BlogID As Long, EntryID As Long, ButtonValue As String, BlogCategoryID As Long, RSSFeedId As Long, BlogRootLink As String, ThumbnailImageWidth As Long, BuildVersion As String, ImageWidthMax As Long) As Long
    On Error GoTo ErrorTrap
    '
    Dim UploadCount As Long
    Dim UploadPointer As Long
    Dim Copy As String
    Dim FileExtension As String
    Dim FilenameNoExtension As String
    Dim VirtualFilePath As String
    Dim BlogImageID As Long
    Dim ImageID As Long
    Dim imageOrder As Long
    Dim imageName As String
    Dim imageDescription As String
    Dim AltSizeList As String
    Dim ImageWidth As Long
    Dim ImageHeight As Long
    Dim sf As Object
    Dim qs As String
    Dim EntryName As String
    Dim EntryLink As String
    Dim CS As Long
    Dim SN As String
    Dim SNName As String
    Dim VirtualFilePathPage As String
    Dim Pos As Long
    Dim imageFilename As String
    '
    ProcessFormBlogPost = SourceFormID
    SN = Main.GetVisitProperty(SNBlogEntryName)
    If SN <> "" Then
        Call Main.SetVisitProperty(SNBlogEntryName, "")
        If ButtonValue = FormButtonCancel Then
            '
            ' Cancel
            '
            ProcessFormBlogPost = FormBlogPostList
        ElseIf ButtonValue = FormButtonPost Then
            '
            ' Post
            '
            If EntryID = 0 Then
                CS = Main.InsertCSRecord(cnBlogEntries)
                If Main.CSOK(CS) Then
                    EntryID = Main.GetCS(CS, "ID")
                    Call Main.SetCS(CS, "BlogID", BlogID)
                End If
                Call Main.CloseCS(CS)
            End If
            CS = Main.OpenCSContent(cnBlogEntries, "(blogid=" & BlogID & ")and(ID=" & EntryID & ")")
            If Main.CSOK(CS) Then
                EntryName = Main.GetStreamText(RequestNameBlogTitle)
                Call Main.SetCS(CS, "Name", EntryName)
                Call Main.SetCS(CS, "copy", Main.GetStreamText(RequestNameBlogCopy))
                Call Main.SetCS(CS, "TagList", Main.GetStreamText(RequestNameBlogTagList))
                Call Main.SetCS(CS, "AuthorMemberID", Main.MemberID)
                Call Main.SetCS(CS, "BlogCategoryID", BlogCategoryID)
                ProcessFormBlogPost = FormBlogPostList
            End If
            Call Main.CloseCS(CS)
            Call UpdateBlogFeed(BlogID, RSSFeedId, BlogRootLink)
            ProcessFormBlogPost = FormBlogPostList
            '
            ' Upload files
            '
            UploadCount = Main.GetStreamInteger("LibraryUploadCount")
            If UploadCount > 0 Then
                For UploadPointer = 1 To UploadCount
                    ImageID = Main.GetStreamInteger(rnBlogImageID & "." & UploadPointer)
                    imageFilename = Main.GetStreamText(rnBlogUploadPrefix & "." & UploadPointer)
                    imageOrder = Main.GetStreamInteger(rnBlogImageOrder & "." & UploadPointer)
                    imageName = Main.GetStreamText(rnBlogImageName & "." & UploadPointer)
                    imageDescription = Main.GetStreamText(rnBlogImageDescription & "." & UploadPointer)
                    If ImageID <> 0 Then
                        '
                        ' edit image
                        '
                        If Main.GetStreamBoolean(rnBlogImageDelete & "." & UploadPointer) Then
                            Call Main.DeleteContentRecord(cnBlogImages, ImageID)
                        Else
                            CS = Main.OpenCSContentRecord(cnBlogImages, ImageID)
                            If Main.IsCSOK(CS) Then
                                Call Main.SetCS(CS, "name", imageName)
                                Call Main.SetCS(CS, "description", imageDescription)
                                Call Main.SetCS(CS, "sortorder", String(12 - Len(imageOrder), "0") & imageOrder)
                            End If
                            Call Main.CloseCS(CS)
                        End If
                    ElseIf imageFilename <> "" Then
                        '
                        ' upload image
                        '
                        CS = Main.InsertCSRecord(cnBlogImages)
                        If Main.IsCSOK(CS) Then
                            BlogImageID = Main.GetCSInteger(CS, "id")
                            'If Copy = "" Then
                            '    Copy = ImageFilename
                            'End If
                            Call Main.SetCS(CS, "Name", imageName)
                            'If Copy = "" Then
                            '    Copy = ImageFilename
                            'End If
                            Call Main.SetCS(CS, "Description", imageDescription)
                            FileExtension = ""
                            FilenameNoExtension = ""
                            Pos = InStrRev(imageFilename, ".")
                            If Pos > 0 Then
                                FileExtension = Mid(imageFilename, Pos + 1)
                                FilenameNoExtension = Left(imageFilename, Pos - 1)
                            End If
                            VirtualFilePathPage = Main.GetCSFilename(CS, "Filename", imageFilename, cnBlogImages)
                            VirtualFilePath = Replace(VirtualFilePathPage, imageFilename, "")
                            Call Main.ProcessFormInputFile(rnBlogUploadPrefix & "." & UploadPointer, VirtualFilePath)
                            If BuildVersion > "3.4.190" Then
                                '
                                ' add image resize values
                                '
                                Set sf = CreateObject("sfimageresize.imageresize")
                                sf.Algorithm = 5
                                On Error Resume Next
                                sf.LoadFromFile (Main.PhysicalFilePath & VirtualFilePathPage)
                                If Err.Number = 0 Then
                                    ImageWidth = sf.Width
                                    ImageHeight = sf.Height
                                    Call Main.SetCS(CS, "height", ImageHeight)
                                    Call Main.SetCS(CS, "width", ImageWidth)
                                Else
                                    Err.Clear
                                End If
'                                '
'                                ' Attempt to make thumbnail
'                                '
'                                If ImageWidth > ThumbnailImageWidth Then
'                                    On Error Resume Next
'                                    sf.LoadFromFile (Main.PhysicalFilePath & VirtualFilePathPage)
'                                    If Err.Number <> 0 Then
'                                        On Error GoTo ErrorTrap
'                                        Err.Clear
'                                    Else
'                                        On Error GoTo ErrorTrap
'                                        sf.Width = ThumbnailImageWidth
'                                        Call sf.DoResize
'                                        Call sf.SaveToFile(Main.PhysicalFilePath & VirtualFilePath & FilenameNoExtension & "-" & ThumbnailImageWidth & "x" & sf.Height & "." & FileExtension)
'                                        AltSizeList = AltSizeList & vbCrLf & ThumbnailImageWidth & "x" & sf.Height
'                                    End If
'                                Else
'                                    Err.Clear
'                                End If
'                                '
'                                ' Attempt to make image
'                                '
'                                If ImageWidth > ImageWidthMax Then
'                                    On Error Resume Next
'                                    sf.LoadFromFile (Main.PhysicalFilePath & VirtualFilePathPage)
'                                    If Err.Number <> 0 Then
'                                        On Error GoTo ErrorTrap
'                                        Err.Clear
'                                    Else
'                                        On Error GoTo ErrorTrap
'                                        sf.Width = ImageWidthMax
'                                        Call sf.DoResize
'                                        Call sf.SaveToFile(Main.PhysicalFilePath & VirtualFilePath & FilenameNoExtension & "-" & ImageWidthMax & "x" & sf.Height & "." & FileExtension)
'                                        AltSizeList = AltSizeList & vbCrLf & ImageWidthMax & "x" & sf.Height
'                                    End If
'                                Else
'                                    Err.Clear
'                                End If
                                '
                                Set sf = Nothing
'                                Call Main.SetCS(CS, "AltSizeList", AltSizeList)
                                Call Main.SetCS(CS, "sortorder", String(12 - Len(BlogImageID), "0") & BlogImageID)
                            End If
                        End If
                        Call Main.CloseCS(CS)
                        '
                        ' Create rule that associates this image with this post
                        '
                        CS = Main.InsertCSRecord(cnBlogImageRules)
                        Call Main.SetCS(CS, "BlogEntryID", EntryID)
                        Call Main.SetCS(CS, "BlogImageID", BlogImageID)
                        Call Main.CloseCS(CS)
                    End If
                Next
            End If
        ElseIf ButtonValue = FormButtonDelete Then
            '
            ' Delete
            '
            Call Main.DeleteContentRecords(cnBlogEntries, "(blogid=" & BlogID & ")and(ID=" & EntryID & ")")
            ProcessFormBlogPost = FormBlogPostList
            Call UpdateBlogFeed(BlogID, RSSFeedId, BlogRootLink)
            ProcessFormBlogPost = FormBlogPostList
        End If
    End If
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "ProcessFormBlogPostDetails", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetFormBlogPostDetails(BlogID As Long, EntryID As Long, IsBlogOwner As Boolean, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Long, ThumbnailImageWidth As Long, BuildVersion As String, ImageWidthMax As Long) As String
    On Error GoTo ErrorTrap
    '
    'Dim EntryCopyOverview As String
    Dim BlogTagList As String
    Dim entryEditLink As String
    Dim PodcastMediaLink As String
    Dim PodcastSize As String
    Dim formKey As String
    Dim Return_CommentCnt As Long
    Dim Copy As String
    Dim DateAdded As Date
    Dim AuthorMemberID As Long
    Dim AuthorMemberName As String
    Dim s As String
    Dim CS As Long
    Dim Divider As String
    Dim RandomNumber As Long
    Dim Approved As Boolean
    Dim CommentName As String
    Dim CommentCopy As String
    Dim CommentPtr As Long
    Dim CommentID As Long
    Dim EntryName As String
    Dim EntryCopy As String
    Dim allowComments As Boolean
    Dim EntryPtr As Long
    Dim qs As String
    Dim QSBack As String
    Dim CommentCnt As Long
    '
    s = s & vbCrLf & Main.GetFormStart
    '
    ' setup form key
    '
    formKey = Main.EncodeKeyNumber(Main.VisitID, Now())
    s = s & vbCrLf & Main.GetFormInputHidden("FormKey", formKey)
    QSBack = Main.RefreshQueryString()
    QSBack = ModifyQueryString(QSBack, RequestNameBlogEntryID, "", True)
    QSBack = ModifyQueryString(QSBack, RequestNameFormID, FormBlogPostList)
    s = s & cr & "<div class=""aoBlogHeaderLink""><a href=""?" & QSBack & """>" & BackToRecentPostsMsg & "</a></div>"
    '
    ' Print the Blog Entry
    '
    CommentCnt = 0
    CS = Main.OpenCSContentRecord(cnBlogEntries, EntryID)
    If Not Main.CSOK(CS) Then
        s = s & cr & "<div class=""aoBlogProblem"">Sorry, the blog post you selected is not currently available</div>"
    Else
        EntryID = Main.GetCSInteger(CS, "ID")
        AuthorMemberID = Main.GetCSInteger(CS, "AuthorMemberID")
        AuthorMemberName = Main.GetRecordName("people", AuthorMemberID)
        DateAdded = Main.GetCSText(CS, "DateAdded")
        EntryName = Main.GetCSText(CS, "Name")
        If Main.IsAuthoring("Blogs") Then
            entryEditLink = Main.GetCSRecordEditLink(CS)
        End If
        EntryCopy = Main.GetCSText(CS, "Copy")
        'EntryCopyOverview = Main.GetCSText(CS, "copyOverview")
        allowComments = Main.GetCSBoolean(CS, "allowComments")
        PodcastMediaLink = Main.GetCS(CS, "PodcastMediaLink")
        PodcastSize = Main.GetCS(CS, "PodcastSize")
        BlogTagList = Main.GetCS(CS, "TagList")
        qs = ""
        qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
        qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
        If Main.version > "4.1.160" Then
            Call Main.AddLinkAlias(EntryName, Main.RenderedPageID, qs)
        End If
        Call Main.SetCS(CS, "viewings", 1 + Main.GetCSInteger(CS, "viewings"))
        s = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, AuthorMemberName, DateAdded, True, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList)
        's = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, AuthorMemberName, DateAdded, True, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, EntryCopyOverview)
        EntryPtr = EntryPtr + 1
    End If
    Call Main.CloseCS(CS)
    '
    ' Add viewing log entry
    '
    If Main.version >= "4.1.161" Then
        If (Not Main.VisitExcludeFromAnalytics) Then
            CS = Main.InsertCSContent("Blog Viewing Log")
            If Main.IsCSOK(CS) Then
                Call Main.SetCS(CS, "Name", Main.MemberName & ", post " & CStr(EntryID) & ", " & Now())
                Call Main.SetCS(CS, "BlogEntryID", EntryID)
                Call Main.SetCS(CS, "MemberID", Main.MemberID)
                Call Main.SetCS(CS, "VisitID", Main.VisitID)
            End If
            Call Main.CloseCS(CS)
        End If
    End If
    
    '
    '
    '
    If IsBlogOwner And (Return_CommentCnt > 0) Then
        s = s & cr & "<div class=""aoBlogCommentCopy"">" & Main.GetFormButton(FormButtonApplyCommentChanges) & "</div>"
    End If
    '
'    s = s & cr & "<div class=""aoBlogCommentName"">" & CommentName & "</div>"
'    s = s & cr & "<div class=""aoBlogCommentCopy"">" & CommentCopy & "</div>"
'    Divider = "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
'    s = s & cr & "<div class=""aoBlogCommentHeader"">Comments</div>"
'    s = s & vbCrLf & Divider
    '
    Dim Auth As Long
    Dim AllowPasswordEmail As Boolean
    Dim AllowMemberJoin As Boolean
    '
    ' The new comment block
    '
    's = s & cr & "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
    If allowComments And (Main.VisitCookieSupport) And (Not Main.VisitIsBot) Then
        s = s & cr & "<div class=""aoBlogCommentHeader"">Post a Comment</div>"
        '
        If (Main.IsUserError()) Then
            s = s & "<div class=""aoBlogCommentError"">" & Main.GetUserError() & "</div>"
        End If
        '
        If (Not AllowAnonymous) And (Not Main.IsAuthenticated) Then
            AllowPasswordEmail = kmaEncodeBoolean(Main.GetSiteProperty("AllowPasswordEmail", False))
            AllowMemberJoin = kmaEncodeBoolean(Main.GetSiteProperty("AllowMemberJoin", False))
            Auth = Main.GetStreamInteger("auth")
            If (Auth = 1) And (Not AllowPasswordEmail) Then
                Auth = 3
            ElseIf (Auth = 2) And (Not AllowMemberJoin) Then
                Auth = 3
            End If
            Call Main.AddRefreshQueryString(RequestNameFormID, FormBlogPostDetails)
            Call Main.AddRefreshQueryString(RequestNameBlogEntryID, EntryID)
            Call Main.AddRefreshQueryString("auth", "0")
            qs = Main.RefreshQueryString()
            Select Case Auth
                Case 1
                    '
                    ' password email
                    '
                    Copy = "To retrieve your username and password, submit your email. "
                    qs = ModifyQueryString(qs, "auth", "0")
                    Copy = Copy & " <a href=""?" & qs & """> Login?</a>"
                    If AllowMemberJoin Then
                        qs = ModifyQueryString(qs, "auth", "2")
                        Copy = Copy & " <a href=""?" & qs & """> Join?</a>"
                    End If
                    s = s _
                        & "<div class=""aoBlogLoginBox"">" _
                        & vbCrLf & vbTab & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                        & vbCrLf & vbTab & "<div class=""aoBlogCommentCopy"">" & Main.GetSendPasswordForm() & "</div>" _
                        & cr & "</div>"
                Case 2
                    '
                    ' join
                    '
                    Copy = "To post a comment to this blog, complete this form. "
                    qs = ModifyQueryString(qs, "auth", "0")
                    Copy = Copy & " <a href=""?" & qs & """> Login?</a>"
                    If AllowPasswordEmail Then
                        qs = ModifyQueryString(qs, "auth", "1")
                        Copy = Copy & " <a href=""?" & qs & """> Forget your username or password?</a>"
                    End If
                    s = s _
                        & "<div class=""aoBlogLoginBox"">" _
                        & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                        & cr & "<div class=""aoBlogCommentCopy"">" & Main.GetJoinForm() & "</div>" _
                        & cr & "</div>"
                Case Else
                    '
                    ' login
                    '
                    Copy = "To post a comment to this Blog, please login."
                    If AllowPasswordEmail Then
                        qs = ModifyQueryString(qs, "auth", "1")
                        Copy = Copy & " <a href=""?" & qs & """> Forget your username or password?</a>"
                    End If
                    If AllowMemberJoin Then
                        qs = ModifyQueryString(qs, "auth", "2")
                        Copy = Copy & " <a href=""?" & qs & """> Join?</a>"
                    End If
                    s = s _
                        & "<div class=""aoBlogLoginBox"">" _
                        & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                        & cr & "<div class=""aoBlogCommentCopy"">" & Main.GetLoginForm() & "</div>" _
                        & cr & "</div>"
            End Select
        
        Else
            s = s & cr & "<div>&nbsp;</div>"
            s = s & cr & "<div class=""aoBlogCommentCopy"">Title</div>"
            If RetryCommentPost Then
                s = s & cr & "<div class=""aoBlogCommentCopy"">" & GetField(RequestNameCommentTitle, , 35, , Main.GetStreamText(RequestNameCommentTitle)) & "</div>"
                s = s & cr & "<div>&nbsp;</div>"
                s = s & cr & "<div class=""aoBlogCommentCopy"">Comment</div>"
                s = s & cr & "<div class=""aoBlogCommentCopy"">" & Main.GetFormInputText(RequestNameCommentCopy, Main.GetStreamText(RequestNameCommentCopy), 15, 70) & "</div>"
            Else
                s = s & cr & "<div class=""aoBlogCommentCopy"">" & GetField(RequestNameCommentTitle, , 35) & "</div>"
                s = s & cr & "<div>&nbsp;</div>"
                s = s & cr & "<div class=""aoBlogCommentCopy"">Comment</div>"
                s = s & cr & "<div class=""aoBlogCommentCopy"">" & Main.GetFormInputText(RequestNameCommentCopy, "", 15, 70) & "</div>"
            End If
            s = s & cr & "<div class=""aoBlogCommentCopy"">Verify Text</div>"
            s = s & cr & "<div class=""aoBlogCommentCopy"">" & Main.ExecuteAddon2(reCaptchaDisplayGuid, "") & "</div>"
            s = s & cr & "<div class=""aoBlogCommentCopy"">" & Main.GetFormButton(FormButtonPostComment) & "&nbsp;" & Main.GetFormButton(FormButtonCancel) & "</div>"
        End If
    
    End If
    
    s = s & cr & "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
    '
    ' edit link
    '
    If IsBlogOwner Then
        qs = Main.RefreshQueryString()
        qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
        qs = ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor)
        s = s & cr & "<div class=""aoBlogToolLink""><a href=""?" & qs & """>Edit</a></div>"
    End If
    '
    ' Search
    '
    qs = Main.RefreshQueryString
    qs = ModifyQueryString(qs, RequestNameFormID, FormBlogSearch, True)
    s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
    '
    ' back to recent posts
    '
    QSBack = Main.RefreshQueryString()
    QSBack = ModifyQueryString(QSBack, RequestNameBlogEntryID, "", True)
    QSBack = ModifyQueryString(QSBack, RequestNameFormID, FormBlogPostList)
    s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & QSBack & """>" & BackToRecentPostsMsg & "</a></div>"
    
'    s = s & cr & "<table width=100% border=0 cellspacing=0 cellpadding=5>"
'    s = s & vbCrLf & GetFormRow2(Main.GetContentCopy("Blog Entries"))
'    s = s & vbCrLf & GetFormRow("", "<b>Post a comment<b>")
'    s = s & vbCrLf & GetFormRow("Title: ", GetField(RequestNameCommentTitle, , 35))
'    s = s & vbCrLf & GetFormRow("Comment: ", Main.GetFormInputText(RequestNameCommentCopy, Main.GetStreamText(RequestNameCommentCopy), 15, 70))
'    s = s & vbCrLf & GetFormRow("", Main.GetFormButton(FormButtonPostComment) & "&nbsp;" & Main.GetFormButton(FormButtonCancel))
'    s = s & vbCrLf & GetFormRow2("<a href=""" & Main.ServerPage & WorkingQueryString & RequestNameFormID & "=" & FormBlogPostList & """>Main Blog page</a>")
'    s = s & vbCrLf & Main.GetFormInputHidden(RequestNameSourceFormID, FormBlogPostDetails)
'    s = s & vbCrLf & Main.GetFormInputHidden(RequestNameBlogEntryID, EntryID)
'    s = s & cr & "</table>"
    '
    s = s & vbCrLf & Main.GetFormInputHidden(RequestNameSourceFormID, FormBlogPostDetails)
    s = s & vbCrLf & Main.GetFormInputHidden(RequestNameBlogEntryID, EntryID)
    s = s & vbCrLf & Main.GetFormInputHidden("EntryCnt", EntryPtr)
    s = s & vbCrLf & Main.GetFormEnd
    '
    GetFormBlogPostDetails = s
    '
    Call Main.SetVisitProperty(SNBlogCommentName, CStr(GetRandomInteger()))
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetFormBlogPostDetails", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function ProcessFormBlogPostDetails(SourceFormID As Long, BlogID As Long, IsBlogOwner As Boolean, ButtonValue As String, BlogName As String, BlogOwnerID As Long, AllowAnonymous As Boolean, BlogCategoryID As Long, autoApproveComments As Boolean, authoringGroupId As Long, emailComment As Boolean, OptionString As String) As Long
    On Error GoTo ErrorTrap
    '
    Dim AuthoringMemberId As Long
    Dim authoringGroup As String
    Dim MemberID As Long
    Dim MemberName As String
    Dim EntryLink As String
    'Dim ButtonValue As String
    Dim EmailBody As String
    Dim EmailFromAddress As String
    Dim EntryCnt As Long
    Dim EntryPtr As Long
    Dim CommentCnt As Long
    Dim CommentPtr As Long
    Dim Suffix As String
    Dim CS As Long
    Dim Copy As String
    Dim CSP As Long
    Dim EmailString As String
    Dim CommentInfo As String
    Dim CommentID As Integer
    Dim RandomSerialNumber As Long
    Dim EntryName As String
    Dim CommentTitle As String
    Dim AuthorName As String
    Dim AuthorEmail As String
    Dim CommentCopy As String
    Dim CommentDate As String
    Dim Approved As Boolean
    Dim SN As String
    Dim EntryID As Long
    Dim formKey As String
    Dim optionStr As String
    Dim captchaResponse As String
    '
    ProcessFormBlogPostDetails = SourceFormID
    SN = Main.GetVisitProperty(SNBlogCommentName)
    '
    If SN = "" Then
        '
        ' Process out of order, go to main
        '
        ProcessFormBlogPostDetails = FormBlogPostList
    Else
        If ButtonValue = FormButtonCancel Then
            '
            ' Cancel button, go to main
            '
            ProcessFormBlogPostDetails = FormBlogPostList
        ElseIf ButtonValue = FormButtonPostComment Then
            '
            ' Process recaptcha
            '
            optionStr = "Challenge=" + Main.GetStreamText("recaptcha_challenge_field")
            optionStr = optionStr & "&Response=" + Main.GetStreamText("recaptcha_response_field")
            captchaResponse = Main.ExecuteAddon2(reCaptchaProcessGuid, optionStr)
            If captchaResponse <> "" Then
                Call Main.AddUserError("The Verify Text you entered did not match correctly. Please try again.")
            End If
            '
            ' Process comment post
            '
            RetryCommentPost = True
            formKey = Main.GetStreamText("formkey")
            'formKey = Main.DecodeKeyNumber(formKey)
            Copy = Main.GetStreamText(RequestNameCommentCopy)
            If Copy <> "" Then
                'If (Main.VisitID <> kmaEncodeInteger(Main.DecodeKeyNumber(formKey))) Then
                '    Call Main.AddUserError("<p>This comment has already been accepted.</p>")
                'End If
                CSP = Main.OpenCSContent(cnBlogComments, "formkey=" & KmaEncodeSQLText(formKey))
                If Main.IsCSOK(CSP) Then
                    Call Main.AddUserError("<p>This comment has already been accepted.</p>")
                    RetryCommentPost = False
                End If
                Call Main.CloseCS(CSP)
                If Not Main.IsUserError() Then
                    EntryID = Main.GetStreamInteger(RequestNameBlogEntryID)
                    CSP = Main.InsertCSRecord(cnBlogComments)
                    If AllowAnonymous And (Not Main.IsAuthenticated) Then
                        MemberID = 0
                        MemberName = AnonymousMemberName
                    Else
                        MemberID = Main.MemberID
                        MemberName = Main.MemberName
                    End If
                    If Main.CSOK(CSP) Then
                        Call Main.SetCS(CSP, "BlogID", BlogID)
                        'Call Main.SetCS(CSP, "Name", Main.GetStreamText(RequestNameAuthorName))
                        'Call Main.SetCS(CSP, "AuthorEmail", Main.GetStreamText(RequestNameAuthorEmail))
                        Call Main.SetCS(CSP, "Name", Main.GetStreamText(RequestNameCommentTitle))
'**** should have been copytext, not copy
                        Call Main.SetCS(CSP, "Copytext", Copy)
                        'Call Main.SetCS(CSP, "Copy", Copy)
                        Call Main.SetCS(CSP, "EntryID", EntryID)
                        Call Main.SetCS(CSP, "AuthorMemberID", Main.MemberID)
                        Call Main.SetCS(CSP, "Approved", IsBlogOwner Or autoApproveComments)
                        Call Main.SetCS(CSP, "formkey", formKey)
                        CommentID = Main.GetCSInteger(CSP, "ID")
                        RetryCommentPost = False
                    End If
                    Call Main.CloseCS(CSP)
                    If (emailComment) Then
                    'If (Not IsBlogOwner) And (emailComment) Then
                        '
                        ' Send Comment Notification
                        '
                        EntryName = Main.GetRecordName(cnBlogEntries, EntryID)
                        EntryLink = Main.ServerLink
                        If InStr(1, EntryLink, "?") = 0 Then
                            EntryLink = EntryLink & "?"
                        Else
                            EntryLink = EntryLink & "&"
                        End If
                        EntryLink = EntryLink & "blogentryid=" & EntryID
                        EmailBody = "" _
                            & cr & "The following blog comment was posted " & Now() _
                            & cr & "To approve this comment, go to " & EntryLink _
                            & vbCrLf _
                            & cr & "Blog '" & BlogName & "'" _
                            & cr & "Post '" & EntryName & "'" _
                            & cr & "By " & Main.MemberName _
                            & vbCrLf _
                            & vbCrLf & kmaEncodeHTML(Copy) _
                            & vbCrLf
                        EmailFromAddress = Main.GetSiteProperty("EmailFromAddress", "info@" & Main.ServerDomain)
                        Call Main.SendMemberEmail(BlogOwnerID, EmailFromAddress, "Blog comment notification for [" & BlogName & "]", EmailBody, False, False)
                        If authoringGroupId <> 0 Then
                            authoringGroup = Main.GetRecordName("groups", authoringGroupId)
                            If authoringGroup <> "" Then
                                CS = Main.OpenCSGroupMembers(authoringGroup, "(allowbulkemail<>0)and(email<>'')")
                                Do While Main.IsCSOK(CS)
                                    AuthoringMemberId = Main.GetCSInteger(CS, "id")
                                    Call Main.SendMemberEmail(AuthoringMemberId, EmailFromAddress, "Blog comment on " & BlogName, EmailBody, False, False)
                                    Call Main.NextCSRecord(CS)
                                Loop
                                Call Main.CloseCS(CS)
                            End If
                        End If
                    End If
                End If
            End If
            ProcessFormBlogPostDetails = FormBlogPostDetails
            'ProcessFormBlogPostDetails = FormBlogPostList
        ElseIf ButtonValue = FormButtonApplyCommentChanges Then
            '
            ' Post approval changes if the person is the owner
            '
            If IsBlogOwner Then
                EntryCnt = Main.GetStreamInteger("EntryCnt")
                If EntryCnt > 0 Then
                    For EntryPtr = 0 To EntryCnt - 1
                        CommentCnt = Main.GetStreamInteger("CommentCnt" & EntryPtr)
                        If CommentCnt > 0 Then
                            For CommentPtr = 0 To CommentCnt - 1
                                Suffix = EntryPtr & "." & CommentPtr
                                CommentID = Main.GetStreamInteger("CommentID" & Suffix)
                                If Main.GetStreamBoolean("Delete" & Suffix) Then
                                    '
                                    ' Delete comment
                                    '
                                    Call Main.DeleteContentRecords("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & BlogID & ")")
                                ElseIf Main.GetStreamBoolean("Approve" & Suffix) And Not Main.GetStreamBoolean("Approved" & Suffix) Then
                                    '
                                    ' Approve Comment
                                    '
                                    CS = Main.OpenCSContent("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & BlogID & ")")
                                    If Main.IsCSOK(CS) Then
                                        Call Main.SetCS(CS, "Approved", True)
                                    End If
                                    Call Main.CloseCS(CS)
                                ElseIf Not Main.GetStreamBoolean("Approve" & Suffix) And Main.GetStreamBoolean("Approved" & Suffix) Then
                                    '
                                    ' Unapprove comment
                                    '
                                    CS = Main.OpenCSContent("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & BlogID & ")")
                                    If Main.IsCSOK(CS) Then
                                        'Call Main.SetCS(CS, "Approved", True)
                                        Call Main.SetCS(CS, "Approved", False)
                                    End If
                                    Call Main.CloseCS(CS)
                                End If
                            Next
                        End If
                    Next
                End If
            End If
'            '
'            CommentInfo = "Submitted at " & Now() & " by " & AuthorName
'            EmailString = "<br>"
'            EmailString = EmailString & CommentInfo & "<br>"
'            EmailString = EmailString & "<a href=""http://" & Main.ServerHost & Main.ServerAppRootPath & "admin/index.asp?cid=" & Main.GetContentID(cnBlogEntries) & "&id=" & CommentID & "&af=4"">click here</a>"
'            Call Main.SendSystemEmail(SystemEmailCommentNotification, EmailString)
            '
        End If
        Call Main.SetVisitProperty(SNBlogCommentName, "")
    End If
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "ProcessFormBlogPostDetails", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
'====================================================================================
'
'====================================================================================
'
Private Function GetBlogCommentCell(IsBlogOwner As Boolean, AuthorMemberName As String, DateAdded As Date, Approved As Boolean, CommentName As String, CommentCopy As String, CommentID As Long, EntryPtr As Long, CommentPtr As Long, AuthorMemberID As Long, EntryID As Long, IsSearchListing As Boolean, EntryName As String) As String
    On Error GoTo ErrorTrap
    '
    Dim Copy As String
    Dim s As String
    Dim RowCopy As String
    Dim RequestSuffix As String
    Dim qs As String
    '
    If IsSearchListing Then
        qs = Main.RefreshQueryString
        qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
        qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
        s = s & cr & "<div class=""aoBlogEntryName"">Comment to Blog Post " & EntryName & ", <a href=""?" & qs & """>View this post</a></div>"
        s = s & cr & "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
    End If
    s = s & cr & "<div class=""aoBlogCommentName"">" & kmaEncodeHTML(CommentName) & "</div>"
    Copy = CommentCopy
    Copy = kmaEncodeHTML(Copy)
    Copy = Replace(Copy, vbCrLf, "<BR />")
    s = s & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>"
    RowCopy = ""
    If AuthorMemberName <> "" Then
        RowCopy = RowCopy & "by " & kmaEncodeHTML(AuthorMemberName)
        If DateAdded <> CDate(0) Then
            RowCopy = RowCopy & " | " & DateAdded
        End If
    Else
        If DateAdded <> CDate(0) Then
            RowCopy = RowCopy & DateAdded
        End If
    End If
    '
    If IsBlogOwner Then
        '
        ' Blog owner Approval checkbox
        '
        RequestSuffix = EntryPtr & "." & CommentPtr
        If RowCopy <> "" Then
            RowCopy = RowCopy & " | "
        End If
        RowCopy = RowCopy _
            & Main.GetFormInputHidden("CommentID" & RequestSuffix, CommentID) _
            & Main.GetFormInputCheckBox("Approve" & RequestSuffix, Approved) _
            & Main.GetFormInputHidden("Approved" & RequestSuffix, Approved) _
            & "&nbsp;Approved&nbsp;" _
            & " | " _
            & Main.GetFormInputCheckBox("Delete" & RequestSuffix, False) _
            & "&nbsp;Delete" _
            & ""
    End If
    If RowCopy <> "" Then
        s = s & cr & "<div class=""aoBlogCommentByLine"">Posted " & RowCopy & "</div>"
    End If
    '
    If (Not Approved) And (Not IsBlogOwner) And (AuthorMemberID = Main.MemberID) Then
        s = "<div style=""border:1px solid red;padding-top:10px;padding-bottom:10px;""><span class=""aoBlogCommentName"" style=""color:red;"">Your comment pending approval</span><br />" & s & "</div>"
    End If
    '
    GetBlogCommentCell = s
    'GetBlogCommentCell = cr & "<div class=""aoBlogComment"">" & s & cr & "</div>"
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetBlogCommentCell", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
Private Function GetBlogEntryCell(EntryPtr As Long, IsBlogOwner As Boolean, EntryID As Long, EntryName As String, EntryCopy As String, AuthorMemberName As String, DateAdded As Date, DisplayFullEntry As Boolean, IsSearchListing As Boolean, Return_CommentCnt As Long, allowComments As Boolean, PodcastMediaLink As String, PodcastSize As String, entryEditLink As String, ThumbnailImageWidth As Long, BuildVersion As String, ImageWidthMax As Long, BlogTagList As String) As String
'Private Function GetBlogEntryCell(EntryPtr As Long, IsBlogOwner As Boolean, EntryID As Long, EntryName As String, EntryCopy As String, AuthorMemberName As String, DateAdded As Date, DisplayFullEntry As Boolean, IsSearchListing As Boolean, Return_CommentCnt As Long, allowComments As Boolean, PodcastMediaLink As String, PodcastSize As String, entryEditLink As String, ThumbnailImageWidth As Long, BuildVersion As String, ImageWidthMax As Long, BlogTagList As String, EntryCopyOverview As String) As String
    On Error GoTo ErrorTrap
    '
    Dim Link As String
    Dim Ptr As Long
    Dim Tags() As String
    Dim TagListRow As String
    Dim imageDescription As String
    Dim imageName As String
    Dim cnt As Long
    Dim c As String
    Dim ThumbnailFilename As String
    Dim imageFilename As String
    Dim SQL As String
    Dim imageIDList As String
    Dim ImageID() As String
    Dim OptionString As String
    Dim CSCount As Long
    Dim CommentCount As Long
    Dim CommentLine As String
    Dim ToolLine As String
    Dim qs As String
    Dim CommentCopy As String
    Dim CommentName As String
    Dim Approved As Boolean
    Dim AuthorMemberID As Long
    Dim CommentID As Long
    Dim CommentPtr As Long
    Dim Divider As String
    Dim CS As Long
    Dim s As String
    Dim RowCopy As String
    Dim Criteria As String
    Dim OverviewCopy As String
    Dim EntryLink As String
    Dim CSImages As Long
    Dim RQS As String
    '
    RQS = Main.RefreshQueryString
    qs = RQS
    qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
    qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
    If Main.version > "4.1.160" Then
        EntryLink = getLinkAlias("?" & qs)
    End If
    If EntryLink = "" Then
        qs = Main.RefreshQueryString()
        qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
        qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
        EntryLink = "?" & qs
    End If
    '
    ' Get ImageID List
    '
    imageIDList = ""
    SQL = "select i.id from BlogImages i,BlogImageRules r where r.BlogImageID=i.id and i.active<>0 and r.blogentryid=" & EntryID & " order by i.SortOrder"
    CS = Main.OpenCSSQL("default", SQL)
    Do While Main.IsCSOK(CS)
        imageIDList = imageIDList & "," & Main.GetCSText(CS, "id")
        Call Main.NextCSRecord(CS)
    Loop
    Call Main.CloseCS(CS)
    '
    ' Get Thumbnail filename
    '
    If imageIDList <> "" Then
        imageIDList = Mid(imageIDList, 2)
        ImageID = Split(imageIDList, ",")
        Call GetBlogImage(kmaEncodeInteger(ImageID(0)), ThumbnailImageWidth, 0, ThumbnailFilename, imageFilename, imageDescription, imageName)
    End If
    '
    s = ""
    If DisplayFullEntry Then
        Call Main.AddPageTitle(EntryName)
        s = s & vbCrLf & entryEditLink & "<h2 class=""aoBlogEntryName"">" & EntryName & "</h2>"
        s = s & cr & "<div class=""aoBlogEntryCopy"">"
        If ThumbnailFilename <> "" Then
            s = s & "<img class=""aoBlogEntryThumbnail"" src=""" & Main.serverFilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;"">"
        End If
        s = s & EntryCopy & "</div>"
        qs = Main.RefreshQueryString()
        qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
        qs = ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor)
        c = ""
        If imageIDList <> "" Then
            c = ""
            For cnt = 0 To UBound(ImageID)
                Call GetBlogImage(kmaEncodeInteger(ImageID(cnt)), 0, ImageWidthMax, ThumbnailFilename, imageFilename, imageDescription, imageName)
                If imageFilename <> "" Then
                    c = c _
                        & cr & "<div class=""aoBlogEntryImageContainer"">" _
                        & cr & "<img src=""" & Main.serverFilePath & imageFilename & """>"
                    If imageName <> "" Then
                        c = c & cr & "<h2>" & imageName & "</h2>"
                    End If
                    If imageDescription <> "" Then
                        c = c & cr & "<div>" & imageDescription & "</div>"
                    End If
                    c = c & cr & "</div>"
                End If
            Next
            If c <> "" Then
                s = s _
                    & cr & "<div class=""aoBlogEntryImageSection"">" _
                    & kmaIndent(c) _
                    & cr & "</div>"
            End If
        End If
        If BlogTagList <> "" Then
            BlogTagList = Replace(BlogTagList, ",", vbCrLf)
            Tags = Split(BlogTagList, vbCrLf)
            BlogTagList = ""
Dim SQS As String
            SQS = ModifyQueryString(RQS, RequestNameFormID, FormBlogSearch, True)
            For Ptr = 0 To UBound(Tags)
                'QS = ModifyQueryString(SQS, RequestNameFormID, FormBlogSearch, True)
                qs = ModifyQueryString(SQS, RequestNameQueryTag, Tags(Ptr), True)
                Link = "?" & qs
                BlogTagList = BlogTagList & ", " & "<a href=""" & Link & """>" & Tags(Ptr) & "</a>"
            Next
            BlogTagList = Mid(BlogTagList, 3)
            c = "" _
                & cr & "<div class=""aoBlogTagListHeader"">" _
                & cr & vbTab & "Tags" _
                & cr & "</div>" _
                & cr & "<div class=""aoBlogTagList"">" _
                & cr & vbTab & BlogTagList _
                & cr & "</div>"
            TagListRow = "" _
                & cr & "<div class=""aoBlogTagListSection"">" _
                & kmaIndent(c) _
                & cr & "</div>"
        End If
    Else
        s = s & vbCrLf & entryEditLink & "<h2 class=""aoBlogEntryName""><a href=""" & EntryLink & """>" & EntryName & "</a></h2>"
        s = s & cr & "<div class=""aoBlogEntryCopy"">"
        If ThumbnailFilename <> "" Then
            s = s & "<a href=""" & EntryLink & """><img class=""aoBlogEntryThumbnail"" src=""" & Main.serverFilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;""></a>"
        End If
        s = s & "<p>" & filterCopy(EntryCopy, OverviewLength) & "</p></div>"
        s = s & cr & "<div class=""aoBlogEntryReadMore""><a href=""" & EntryLink & """>Read More</a></div>"
'        If Len(EntryCopyOverview) < 35 Then
'            EntryCopyOverview = EntryCopy
'        End If
'        If Len(EntryCopyOverview) < OverviewLength Then
'            s = s & EntryCopyOverview & "</div>"
'        Else
'            s = s & filterCopy(EntryCopyOverview, OverviewLength) & "</div>"
'            s = s & CR & "<div class=""aoBlogEntryReadMore""><a href=""" & EntryLink & """>Read More</a></div>"
'        End If
    End If
    '
    ' Podcast link
    '
    If PodcastMediaLink <> "" Then
        OptionString = "" _
            & "&Media Link=" & Main.EncodeAddonOptionArgument(PodcastMediaLink) _
            & "&Media Size=" & Main.EncodeAddonOptionArgument(PodcastSize) _
            & "&Hide Player=true" _
            & "&Auto Start=false" _
            & ""
        s = s & Main.ExecuteAddon2("{F6037DEE-023C-4A14-A972-ADAFA5538240}", OptionString)
    End If
    '
    ' Author Row
    '
    RowCopy = ""
    If AuthorMemberName <> "" Then
        RowCopy = RowCopy & "By " & AuthorMemberName
        If DateAdded <> CDate(0) Then
            RowCopy = RowCopy & " | " & DateAdded
        End If
    Else
        If DateAdded <> CDate(0) Then
            RowCopy = RowCopy & DateAdded
        End If
    End If
    If allowComments And (Main.VisitCookieSupport) And (Not Main.VisitIsBot) Then
    'If allowComments Then
        '
        ' Show comment count
        '
        Criteria = "(Approved<>0)and(EntryID=" & EntryID & ")"
        CSCount = Main.OpenCSContent(cnBlogComments, "(Approved<>0)and(EntryID=" & EntryID & ")")
        CommentCount = Main.GetCSRowCount(CSCount)
        Call Main.CloseCS(CSCount)
        If DisplayFullEntry Then
            If CommentCount = 1 Then
                RowCopy = RowCopy & " | 1 Comment"
            ElseIf CommentCount > 1 Then
                RowCopy = RowCopy & " | " & CommentCount & " Comments&nbsp;(" & CommentCount & ")"
            End If
        Else
            qs = Main.RefreshQueryString()
            qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
            qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
            If CommentCount = 0 Then
                RowCopy = RowCopy & " | " & "<a href=""?" & qs & """>Comment</a>"
            Else
                RowCopy = RowCopy & " | " & "<a href=""?" & qs & """>Comments</a>&nbsp;(" & CommentCount & ")"
            End If
        End If
    End If
    If RowCopy <> "" Then
        s = s & cr & "<div class=""aoBlogEntryByLine"">Posted " & RowCopy & "</div>"
    End If
    '
    ' Tag List Row
    '
    If TagListRow <> "" Then
        s = s & TagListRow
    End If
    ''
    'If Main.IsLinkAuthoring(cnBlogEntries) Then
    '    s = s & vbCrLf & Main.GetAdminHintWrapper("Blog Details: <a href=""?" & QS & """>Click here</a>")
    'End If
    '
    If allowComments And (Main.VisitCookieSupport) And (Not Main.VisitIsBot) Then
    'If allowComments Then
        If Not DisplayFullEntry Then
            ''
            '' Show comment count
            ''
            'Criteria = "(Approved<>0)and(EntryID=" & EntryID & ")"
            'CSCount = Main.OpenCSContent(cnBlogComments, "(Approved<>0)and(EntryID=" & EntryID & ")")
            'CommentCount = Main.GetCSRowCount(CSCount)
            'Call Main.CloseCS(CSCount)
            ''
'            QS = Main.RefreshQueryString()
'            QS = ModifyQueryString(QS, RequestNameBlogEntryID, CStr(EntryID))
'            QS = ModifyQueryString(QS, RequestNameFormID, FormBlogPostDetails)
            'If CommentCount = 0 Then
            '    CommentLine = CommentLine & "<a href=""?" & QS & """>Comment</a>"
            'Else
            '    CommentLine = CommentLine & "<a href=""?" & QS & """>Comments</a>&nbsp;(" & CommentCount & ")"
            'End If
            If IsBlogOwner Then
                Criteria = "(EntryID=" & EntryID & ")"
                CSCount = Main.OpenCSContent(cnBlogComments, "((Approved is null)or(Approved=0))and(EntryID=" & EntryID & ")")
                CommentCount = Main.GetCSRowCount(CSCount)
                Call Main.CloseCS(CSCount)
                If ToolLine <> "" Then
                    ToolLine = ToolLine & "&nbsp;|&nbsp;"
                End If
                ToolLine = ToolLine & "Unapproved Comments (" & CommentCount & ")"
                qs = Main.RefreshQueryString()
                qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                qs = ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor)
                If ToolLine <> "" Then
                    ToolLine = ToolLine & "&nbsp;|&nbsp;"
                End If
                ToolLine = ToolLine & "<a href=""?" & qs & """>Edit</a>"
            End If
'            If ToolLine <> "" Then
'                s = s & cr & "<div class=""aoBlogToolLink"">" & ToolLine & "</div>"
'            End If
        
    's = s & cr & "<div><a class=""aoBlogFooterLink"" href=""" & Main.ServerPage & WorkingQueryString & RequestNameBlogEntryID & "=" & Main.GetCSInteger(CS, "ID") & "&" & RequestNameFormID & "=" & FormBlogPostDetails & """>Post a Comment</a></div>"
        
        Else
            '
            ' Show all comments
            '
            If IsBlogOwner Then
                '
                ' Owner - Show all comments
                '
                Criteria = "(EntryID=" & EntryID & ")"
            Else
                '
                ' non-owner - just approved comments plus your own comments
                '
                Criteria = "((Approved<>0)or(AuthorMemberID=" & Main.MemberID & "))and(EntryID=" & EntryID & ")"
            End If
            CS = Main.OpenCSContent(cnBlogComments, Criteria, "DateAdded")
            If Main.CSOK(CS) Then
                Divider = "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                s = s & cr & "<div class=""aoBlogCommentHeader"">Comments</div>"
                s = s & vbCrLf & Divider
                CommentPtr = 0
                Do While Main.CSOK(CS)
                    CommentID = Main.GetCSInteger(CS, "ID")
                    AuthorMemberID = Main.GetCSInteger(CS, "AuthorMemberID")
                    AuthorMemberName = Main.GetRecordName("people", AuthorMemberID)
                    DateAdded = Main.GetCSDate(CS, "DateAdded")
                    Approved = Main.GetCSBoolean(CS, "Approved")
                    CommentName = Main.GetCSText(CS, "Name")
    ' should have been copy text, not copy
                    CommentCopy = Main.GetCSText(CS, "Copytext")
                    If CommentCopy = "" Then
                        CommentCopy = Main.GetCSText(CS, "Copy")
                    End If
                    s = s & GetBlogCommentCell(IsBlogOwner, AuthorMemberName, DateAdded, Approved, CommentName, CommentCopy, CommentID, EntryPtr, CommentPtr, AuthorMemberID, EntryID, False, EntryName)
                    s = s & vbCrLf & Divider
                    CommentPtr = CommentPtr + 1
                    Call Main.NextCSRecord(CS)
                Loop
            End If
            Call Main.CloseCS(CS)
        End If
    
    End If
    '
    If ToolLine <> "" Then
        s = s & cr & "<div class=""aoBlogToolLink"">" & ToolLine & "</div>"
    End If
    s = s & vbCrLf & Main.GetFormInputHidden("CommentCnt" & EntryPtr, CommentPtr)
    '
    Return_CommentCnt = CommentPtr
    GetBlogEntryCell = s
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "GetBlogEntryCell", Err.Number, Err.Source, Err.Description, True, False)
End Function
'
'
'
Private Function AddMonth(StartDate As Date, Months As Long) As Date
    On Error GoTo ErrorTrap
    '
    Dim Y As Long
    Dim M As Long
    '
    M = Month(StartDate) + Months
    Y = Year(StartDate) + Int(M / 12)
    M = M Mod 12
    AddMonth = CDate(M & "/1/" & Y)
    '
    Exit Function
ErrorTrap:
    Call HandleError("UpdatedBlogsClass", "AddMonth", Err.Number, Err.Source, Err.Description, True, False, Main.ServerLink)
End Function

Private Function filterCopy(rawCopy As String, MaxLength As Long) As String
    On Error GoTo ErrorTrap
    
    Dim Copy As String
    Dim objHTML As New kmaHTML.DecodeClass
    
    Copy = objHTML.Decode(rawCopy)
    If Len(Copy) > MaxLength Then
        Copy = Left(Copy, MaxLength)
        Copy = Copy & "..."
    End If
    filterCopy = Copy
    
    Exit Function
ErrorTrap:
    Call HandleError("Blogs", "filterCopy", Err.Number, Err.Source, Err.Description, True, False)
End Function

Private Sub Upgrade()
    On Error GoTo ErrorTrap

    Dim DBType As String
    Dim CurrentVersion As String
    Dim SQL As String
    Dim CSPointer As Long
    Dim csBlogs As Long
    Dim blogCopyName As String
    Dim blogDescription As String
    Dim csEntry As Long
    
    'DBType = Main.GetDataSourceType("Default")

    CurrentVersion = Main.GetSiteProperty("Blog Version", "00000")

    If CurrentVersion < AddOnVersion Then

        Call Main.SetStreamTimeout(StreamUpgradeTimeout)
        If CurrentVersion = "00000" Then
            
            CSPointer = Main.OpenCSContent(cnBlogEntries)
            Do While Main.CSOK(CSPointer)
                Call Main.SetCS(CSPointer, "AllowComments", 1)
                Call Main.SaveCSRecord(CSPointer)
                Call Main.NextCSRecord(CSPointer)
            Loop
            Call Main.CloseCS(CSPointer)
            
        End If
        If CurrentVersion <= "2.0.062" Then
            csBlogs = Main.OpenCSContent(cnBlogs)
            Do While Main.CSOK(csBlogs)
                blogCopyName = Main.GetCSText(csBlogs, "name")
                blogDescription = ""
                If InStr(1, blogCopyName, " Blog", vbTextCompare) = 0 Then
                    blogCopyName = blogCopyName & " blog"
                End If
                blogCopyName = "Blogs Header for " & blogCopyName
                csEntry = Main.OpenCSContent("copy content", "name=" & KmaEncodeSQLText(blogCopyName))
                If Main.IsCSOK(csEntry) Then
                    blogDescription = Main.GetCSText(csEntry, "copy")
                End If
                Call Main.CloseCS(csEntry)
                Call Main.SetCS(csBlogs, "copy", blogDescription)
                
                Call Main.NextCSRecord(csBlogs)
            Loop
            Call Main.CloseCS(csBlogs)
            
        End If
        If CurrentVersion < "2.0.069" Then
            '
            ' create emailComment field for any site with autoApproveComments
            '
            SQL = "update ccBlogs set emailComment=autoApproveComments"
            Call Main.ExecuteSQL("", SQL)
        End If
        '
        Call Main.SetSiteProperty("Blog Version", AddOnVersion)
    End If
    '
    Exit Sub
ErrorTrap:
    Call HandleError("Blogs", "Upgrade", Err.Number, Err.Source, Err.Description, True, False)
End Sub
'
'========================================================================
'   Update the Blog Feed
'       run on every post
'========================================================================
'
Private Sub UpdateBlogFeed(BlogID As Long, RSSFeedId As Long, BlogRootLink As String)
    On Error GoTo ErrorTrap
    '
    Dim RSSTitle As String
    Dim EntryCopy As String
    Dim EntryLink As String
    Dim EntryID As Long
    Dim CSPost As Long
    Dim qs As String
    Dim EntryName As String
    Dim CS As Long
    Dim RuleIDs() As Long
    Dim RuleBlogPostIDs() As Long
    Dim RuleCnt As Long
    Dim RulePtr As Long
    Dim RuleSize As Long
    Dim BlogPostID As Long
    Dim CSRule As Long
    Dim SQL As String
    Dim AdminURL As String
    Dim pageId As Long
    '
    AdminURL = Main.SiteProperty_AdminURL
    'call VerifyFeedReturnArgs(BlogID, )
    If RSSFeedId <> 0 Then
        '
        ' Gather all the current rules
        '
        CS = Csv.OpenCSContent(cnRSSFeedBlogRules, "RSSFeedID=" & RSSFeedId, , , , , , "id,BlogPostID")
        RuleCnt = 0
        Do While Csv.IsCSOK(CS)
            If RuleCnt >= RuleSize Then
                RuleSize = RuleSize + 10
                ReDim Preserve RuleIDs(RuleSize)
                ReDim Preserve RuleBlogPostIDs(RuleSize)
            End If
            RuleIDs(RuleCnt) = Csv.GetCSInteger(CS, "ID")
            RuleBlogPostIDs(RuleCnt) = Csv.GetCSInteger(CS, "BlogPostID")
            RuleCnt = RuleCnt + 1
            Call Csv.NextCSRecord(CS)
        Loop
        Call Csv.CloseCS(CS)
        '
        ' Gather all the posts that should be in the feed
        '
        SQL = "select p.ID" _
            & " from (ccBlogCopy p" _
            & " left join BlogCategories c on c.id=p.blogCategoryID)" _
            & " where (p.blogid=" & BlogID & ")" _
            & " and((c.id is null)or(c.UserBlocking=0)or(c.UserBlocking is null))"
        CS = Csv.OpenCSSQL("default", SQL, 0)
        Do While Csv.IsCSOK(CS)
            BlogPostID = Csv.GetCSInteger(CS, "id")
            For RulePtr = 0 To RuleCnt - 1
                If BlogPostID = RuleBlogPostIDs(RulePtr) Then
                    RuleIDs(RulePtr) = -1
                    Exit For
                End If
            Next
            If RulePtr >= RuleCnt Then
                '
                ' Rule not found, add it
                '
                CSRule = Csv.InsertCSRecord(cnRSSFeedBlogRules, 0)
                If Csv.IsCSOK(CSRule) Then
                    Call Csv.SetCS(CSRule, "RSSFeedID", RSSFeedId)
                    Call Csv.SetCS(CSRule, "BlogPostID", BlogPostID)
                End If
                Call Csv.CloseCS(CSRule)
            End If
            '
            ' Now update the Blog Post RSS fields, RSSLink, RSSTitle, RSSDescription, RSSPublish, RSSExpire
            ' Should do this here because if RSS was installed after Blog, there is no link until a post is edited
            '
            CSPost = Csv.OpenCSContentRecord(cnBlogEntries, BlogPostID)
            If Csv.IsCSOK(CSPost) Then
                EntryName = Csv.GetCSText(CSPost, "name")
                EntryID = Csv.GetCSInteger(CSPost, "id")
                EntryCopy = Csv.GetCSText(CSPost, "copy")
                '
                EntryLink = BlogRootLink
                EntryLink = kmaModifyLinkQuery(EntryLink, RequestNameBlogEntryID, CStr(EntryID))
                EntryLink = kmaModifyLinkQuery(EntryLink, RequestNameFormID, FormBlogPostDetails)
                EntryLink = kmaModifyLinkQuery(EntryLink, RequestNameBlogCategoryID, "")
                EntryLink = getLinkAlias(EntryLink)
                '

                'QS = Main.RefreshQueryString()
                'QS = ModifyQueryString(QS, RequestNameBlogEntryID, CStr(EntryID))
                'QS = ModifyQueryString(QS, RequestNameFormID, FormBlogPostDetails)
                'QS = ModifyQueryString(QS, RequestNameBlogCategoryID, "")
                Call Main.SetCS(CSPost, "RSSTitle", EntryName)
                'EntryLink = "http://" & Main.ServerDomainPrimary & Main.ServerPath & Main.ServerPage & "?" & QS
                If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                    Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                End If
                Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
                RSSTitle = Trim(EntryName)
                If RSSTitle = "" Then
                    RSSTitle = "Blog Post " & EntryID
                End If
                Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
            End If
            Call Csv.CloseCS(CSPost)
            '
            Call Csv.NextCSRecord(CS)
        Loop
        Call Csv.CloseCS(CS)
        '
        ' Now delete all the rules that were not found in the blog
        '
        For RulePtr = 0 To RuleCnt - 1
            If RuleIDs(RulePtr) <> -1 Then
                Call Csv.DeleteContentRecord(cnRSSFeedBlogRules, RuleIDs(RulePtr))
            End If
        Next
    End If
    '
    If Main.ContentServerVersion >= "4.1.098" Then
        Call Csv.ExecuteAddonAsProcess(RSSProcessAddonGuid)
    End If
    '
    Exit Sub
ErrorTrap:
    Call HandleError("Blogs", "UpdateBlogFeed", Err.Number, Err.Source, Err.Description, True, False, Main.ServerLink)
End Sub
'
'========================================================================
'   Verify the RSSFeed and return the ID and the Link
'       run when a new blog is created
'       run on every post
'========================================================================
'
Private Sub VerifyFeedReturnArgs(BlogID As Long, Return_RSSFeedID As Long, Return_RSSFeedName As String, Return_RSSFeedFilename As String, Return_BlogRootLink As String)
    On Error GoTo ErrorTrap
    '
    Dim qs As String
    Dim CSBlog As Long
    Dim BlogName As String
    Dim blogDescription As String
    Dim CSFeed As Long
    '
    CSBlog = Csv.OpenCSContent(cnBlogs, "ID=" & BlogID)
    If Csv.IsCSOK(CSBlog) Then
        BlogName = Csv.GetCSText(CSBlog, "name")
        blogDescription = Trim(Csv.GetCS(CSBlog, "copy"))
        Return_RSSFeedID = Csv.GetCSInteger(CSBlog, "RSSFeedID")
        If Trim(BlogName) = "" Then
            Return_RSSFeedName = "Feed for Blog " & BlogID
        Else
            Return_RSSFeedName = BlogName
        End If
        If Return_RSSFeedID <> 0 Then
            '
            ' Make sure the record exists
            '
            CSFeed = Main.OpenCSContentRecord("RSS Feeds", Return_RSSFeedID)
            If Not Main.IsCSOK(CSFeed) Then
                Return_RSSFeedID = 0
            End If
        End If
        If Return_RSSFeedID = 0 Then
            '
            ' new blog was created, now create new feed
            '   set name and description from the blog
            '
            CSFeed = Main.InsertCSContent(cnRSSFeeds)
            If Main.IsCSOK(CSFeed) Then
                Return_RSSFeedID = Main.GetCSInteger(CSFeed, "ID")
                Call Main.SetCS(CSBlog, "RSSFeedID", Return_RSSFeedID)
                Call Main.SetCS(CSFeed, "Name", Return_RSSFeedName)
                If blogDescription = "" Then
                    blogDescription = Trim(Return_RSSFeedName)
                End If
                Call Main.SetCS(CSFeed, "description", blogDescription)
                Return_RSSFeedFilename = encodeFilename(Return_RSSFeedName) & ".xml"
                Call Main.SetCS(CSFeed, "rssfilename", Return_RSSFeedFilename)
            End If
        End If
        If Main.IsCSOK(CSFeed) Then
            '
            ' Manage the Feed name, title and description
            '   because it is associated to this blog'
            '   only reset the link if it is blank (see desc at top of class)
            '   only manage the RSSFeedFilename if it is blank
            '
            Return_RSSFeedName = Main.GetCSText(CSFeed, "name")
            Return_RSSFeedID = Main.GetCSInteger(CSFeed, "id")
            Return_RSSFeedFilename = Trim(Main.GetCS(CSFeed, "rssfilename"))
            Return_BlogRootLink = Trim(Main.GetCS(CSFeed, "link"))
            If Return_BlogRootLink = "" Then
                '
                ' set blog link to current link without forms/categories
                '   exclude admin
                '   exclude a post
                '
                Return_BlogRootLink = Main.ServerLink
                If (InStr(1, Return_BlogRootLink, "admin", vbTextCompare) = 0) And (Main.ServerForm = "") Then
                    Return_BlogRootLink = ModifyQueryString(Return_BlogRootLink, RequestNameFormID, "", False)
                    Return_BlogRootLink = ModifyQueryString(Return_BlogRootLink, RequestNameSourceFormID, "", False)
                    Return_BlogRootLink = ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryID, "", False)
                    Return_BlogRootLink = ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryIDSet, "", False)
                    Call Main.SetCS(CSFeed, "link", Return_BlogRootLink)
                End If
            End If
        End If
        Call Main.CloseCS(CSFeed)
    End If
    Call Csv.CloseCS(CSBlog)
    '
    Exit Sub
ErrorTrap:
    Call HandleError("Blogs", "VerifyFeedReturnArgs", Err.Number, Err.Source, Err.Description, True, False, Main.ServerLink)
End Sub
'
'
'
Private Function GetBlogImage(BlogImageID As Long, ThumbnailImageWidth As Long, ImageWidthMax As Long, ByRef Return_ThumbnailFilename As String, ByRef Return_ImageFilename As String, ByRef Return_ImageDescription As String, ByRef Return_Imagename As String) As String
    On Error GoTo ErrorTrap
    '
    Dim sf As Object
    Dim FilenameNoExtension As String
    Dim FilenameExtension As String
    Dim Pos As Long
    Dim CS As Long
    Dim Filename As String
    Dim AltSizeList As String
    Dim Sizes() As String
    Dim Size As String
    Dim Ptr As Long
    Dim Dims() As String
    Dim ThumbNailSize As String
    Dim ImageSize As String
    Dim UpdateRecord As Boolean
    Dim DimWidth As Long
    '
    CS = Main.OpenCSContentRecord(cnBlogImages, BlogImageID, , , "name,description,filename,altsizelist")
    If Main.IsCSOK(CS) Then
        Filename = Main.GetCSText(CS, "filename")
        AltSizeList = Main.GetCSText(CS, "AltSizeList")
        Return_ImageDescription = Main.GetCSText(CS, "description")
        Return_Imagename = Main.GetCSText(CS, "name")
        '
        Return_ThumbnailFilename = Filename
        Return_ImageFilename = Filename
        '
        If AltSizeList <> "" Then
            AltSizeList = Replace(AltSizeList, ",", vbCrLf)
            Sizes = Split(AltSizeList, vbCrLf)
            For Ptr = 0 To UBound(Sizes)
                If Sizes(Ptr) <> "" Then
                    Dims = Split(Sizes(Ptr), "x")
                    DimWidth = kmaEncodeInteger(Dims(0))
                    If (ThumbnailImageWidth <> 0) And (DimWidth = ThumbnailImageWidth) Then
                        ThumbNailSize = Sizes(Ptr)
                    End If
                    If (ImageWidthMax <> 0) And (DimWidth = ImageWidthMax) Then
                        ImageSize = Sizes(Ptr)
                    End If
                End If
            Next
        End If
        If True Then
            Pos = InStrRev(Filename, ".")
            If Pos <> 0 Then
                FilenameNoExtension = Mid(Filename, 1, Pos - 1)
                FilenameExtension = Mid(Filename, Pos + 1)
                If ThumbnailImageWidth = 0 Then
                    '
                ElseIf ThumbNailSize <> "" Then
                    Return_ThumbnailFilename = FilenameNoExtension & "-" & ThumbNailSize & "." & FilenameExtension
                Else
                    Set sf = CreateObject("sfimageresize.imageresize")
                    sf.Algorithm = 5
                    On Error Resume Next
                    sf.LoadFromFile (Main.PhysicalFilePath & Filename)
                    If Err.Number <> 0 Then
                        On Error GoTo ErrorTrap
                        Err.Clear
                    Else
                        On Error GoTo ErrorTrap
                        sf.Width = ThumbnailImageWidth
                        Call sf.DoResize
                        ThumbNailSize = ThumbnailImageWidth & "x" & sf.Height
                        Return_ThumbnailFilename = FilenameNoExtension & "-" & ThumbNailSize & "." & FilenameExtension
                        Call sf.SaveToFile(Main.PhysicalFilePath & Return_ThumbnailFilename)
                        AltSizeList = AltSizeList & vbCrLf & ThumbNailSize
                        Call Main.SetCS(CS, "altsizelist", AltSizeList)
                    End If
                End If
                If ImageWidthMax = 0 Then
                    '
                ElseIf ImageSize <> "" Then
                    Return_ImageFilename = FilenameNoExtension & "-" & ImageSize & "." & FilenameExtension
                Else
                    Set sf = CreateObject("sfimageresize.imageresize")
                    sf.Algorithm = 5
                    On Error Resume Next
                    sf.LoadFromFile (Main.PhysicalFilePath & Filename)
                    If Err.Number <> 0 Then
                        On Error GoTo ErrorTrap
                        Err.Clear
                    Else
                        On Error GoTo ErrorTrap
                        sf.Width = ImageWidthMax
                        Call sf.DoResize
                        ImageSize = ImageWidthMax & "x" & sf.Height
                        Return_ImageFilename = FilenameNoExtension & "-" & ImageSize & "." & FilenameExtension
                        Call sf.SaveToFile(Main.PhysicalFilePath & Return_ImageFilename)
                        AltSizeList = AltSizeList & vbCrLf & ImageSize
                        Call Main.SetCS(CS, "altsizelist", AltSizeList)
                    End If
                End If
            End If
        End If
    
    End If
    Call Main.CloseCS(CS)
    '
    Exit Function
ErrorTrap:
    Call HandleError("Blogs", "GetBlogImageFilename", Err.Number, Err.Source, Err.Description, True, False, Main.ServerLink)
End Function
'
'
'
Private Function getLinkAlias(sourceLink As String) As String
    Dim returnLink As String
    Dim Pos As Long
    Dim pageQs() As String
    Dim nameValues() As String
    Dim cnt As Long
    Dim Ptr As Long
    Dim Link As String
    Dim pageId  As Long
    Dim NameValue As String
    Dim qs As String
    '
    returnLink = sourceLink
    If kmaEncodeBoolean(Main.GetSiteProperty("allowLinkAlias", "1")) Then
        Link = sourceLink
        '
        pageQs = Split(LCase(Link), "?")
        If UBound(pageQs) > 0 Then
            nameValues = Split(pageQs(1), "&")
            cnt = UBound(nameValues) + 1
            If UBound(nameValues) > 0 Then
                qs = ""
                For Ptr = 0 To cnt - 1
                    NameValue = nameValues(Ptr)
                    If pageId = 0 Then
                        If Mid(NameValue, 1, 4) = "bid=" Then
                            pageId = kmaEncodeInteger(Mid(NameValue, 5))
                            NameValue = ""
                        End If
                    End If
                    If NameValue <> "" Then
                        qs = qs & "&" & NameValue
                    End If
                Next
                If pageId <> 0 Then
                    If Len(qs) > 1 Then
                        qs = Mid(qs, 2)
                        returnLink = Main.GetLinkAliasByPageID(pageId, qs, sourceLink)
                    End If
                End If
            End If
        End If
    End If
    getLinkAlias = returnLink
End Function
'
'
'
