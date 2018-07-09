
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Views
    '
    Public Class vb6BlogsClass
        Inherits AddonBaseClass
        '
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                '
                returnHtml = GetContent(CP)
                '
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
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
        '
        'Private WorkingQueryString As String
        Private ErrorString As String
        Private RetryCommentPost As Boolean                 ' when true, the comment post page prepopulates with the previous comment post. set by process comment

        Private OverviewLength As Integer
        Private PostsToDisplay As Integer

        Const StreamUpgradeTimeout = 1800

        '
        Public Class userCacheStruct
            Public Id As Integer
            Public Name As String
            Public authorInfoLink As String
        End Class
        '
        Private users() As userCacheStruct
        Private userCnt As Integer
        '
        Private allowAuthorInfoLink As Boolean

        Public Function GetContent(cp As CPBaseClass) As String
            '
            Dim blogCaption As String
            Dim blogDescription As String
            Dim ignoreLegacyInstanceOptions As Boolean
            Dim authoringGroup As String
            Dim authoringGroupId As Integer
            Dim NewPostCopy As String
            Dim BlogPostID As Integer
            Dim CSRule As Integer
            Dim csLink As Integer
            Dim EntryLink As String
            Dim RSSTitle As String
            Dim RSSFeedFilename As String
            Dim CSFeed As Integer
            Dim DateSearchText As String
            Dim KeywordList As String
            Dim ButtonValue As String
            Dim IsContentManager As Boolean
            Dim Stream As String
            Dim BlogName As String
            Dim blogId As Integer
            Dim BlogOwnerID As Integer
            Dim IsBlogOwner As Boolean
            Dim CS As Integer
            Dim FormID As Integer
            Dim SourceFormID As Integer
            Dim DbVersion As String
            Dim AppVersion As String
            Dim s As String
            Dim ArchiveMonth As Integer
            Dim ArchiveYear As Integer
            Dim EntryID As Integer
            Dim AllowAnonymous As Boolean
            Dim Title As String
            Dim AllowCategories As Boolean
            Dim BlogCategoryID As Integer
            Dim RSSFeedId As Integer
            Dim RSSFeedName As String
            Dim ThumbnailImageWidth As Integer
            Dim BuildVersion As String
            Dim ImageWidthMax As Integer
            Dim autoApproveComments As Boolean
            Dim emailComment As Boolean
            Dim qs As String
            'Dim BlogRootLink As String
            Dim blogListLink As String
            Dim blogListQs As String
            Dim allowRecaptcha As Boolean
            '
            ' get blogListLink - link to the main list page
            '
            blogListQs = cp.Doc.RefreshQueryString()
            blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, "")
            blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameFormID, "")
            blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameBlogCategoryID, "")
            blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameBlogEntryID, "")
            blogListLink = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", "") & "?" & blogListQs
            '
            s = s & vbCrLf & "<!-- Blog " & BlogName & " -->" & vbCrLf
            '
            'If Not (Main Is Nothing) Then
            BlogName = cp.Doc.GetText("BlogName")
            If BlogName = "" Then
                    BlogName = "Default"
                End If
            'BuildVersion = Main.SiteProperty_BuildVersion
            '
            ' add warning if the blog is being viewed in the admin site
            '
            '    If (InStr(1, Main.ServerLink, Main.SiteProperty_AdminURL, vbTextCompare) <> 0) Then
            '    s = s & cp.Utils.("Some blog features such as the rss feed can not be initialized when viewed from the admin site.")
            'End If
            '
            ' Get the blog record
            '
            Dim csopen As CPCSBaseClass = cp.CSNew()
            CS = csopen.Open(cnBlogs, "(name=" & cp.Utils.EncodeQueryString(BlogName) & ")", "ID")
            'If Main.IsCSOK(CS) Then
            blogId = cp.Site.GetInteger(CS, "ID")
            RSSFeedId = cp.Site.GetInteger(CS, "RSSFeedID")
            BlogOwnerID = cp.Site.GetInteger(CS, "OwnerMemberID")
            authoringGroupId = cp.Site.GetInteger(CS, "AuthoringGroupID")
            ignoreLegacyInstanceOptions = cp.Site.GetBoolean(CS, "ignoreLegacyInstanceOptions")
            AllowAnonymous = cp.Site.GetBoolean(CS, "AllowAnonymous")
            autoApproveComments = cp.Site.GetBoolean(CS, "autoApproveComments")
            emailComment = cp.Site.GetBoolean(CS, "emailComment")
            AllowCategories = cp.Site.GetBoolean(CS, "AllowCategories")
            PostsToDisplay = cp.Site.GetInteger(CS, "PostsToDisplay")
            OverviewLength = cp.Site.GetInteger(CS, "OverviewLength")
            ThumbnailImageWidth = cp.Site.GetInteger(CS, "ThumbnailImageWidth")
            ImageWidthMax = cp.Site.GetInteger(CS, "ImageWidthMax")
            blogDescription = cp.Site.GetText(CS, "copy")
            blogCaption = cp.Site.GetText(CS, "caption")
            allowRecaptcha = cp.Site.GetBoolean(CS, "recaptcha")
            'End If
             csopen.Close()
            If blogId = 0 Then
                '
                ' Create New Blog
                '
                IsContentManager = cp.Doc.GetText("Page Content") 'Main.IsContentManager("Page Content")
                If IsContentManager Then
                        '
                        ' BIG assumption - First hit by a content manager for this page is the author
                        '
                        CS = Main.InsertCSContent(cnBlogs)
                        If Main.IsCSOK(CS) Then
                            blogId = Main.GetCSInteger(CS, "ID")
                            BlogOwnerID = Main.MemberID
                            IsBlogOwner = True
                            blogCaption = BlogName
                            '
                            Call Main.SetCS(CS, "name", BlogName)
                            Call Main.SetCS(CS, "caption", blogCaption)
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
                        Call VerifyFeedReturnArgs(blogId, blogListLink, RSSFeedId, RSSFeedName, RSSFeedFilename)
                        '
                        Title = "Welcome to the " & BlogName
                        If InStr(1, BlogName, " Blog", vbTextCompare) = 0 Then
                            Title = Title & " blog"
                        End If
                        Title = Title & "!"
                        '
                        CS = Main.InsertCSRecord(cnBlogEntries)
                        If Main.csok(CS) Then
                            BlogPostID = Main.GetCS(CS, "ID")
                            Call Main.SetCS(CS, "BlogID", blogId)
                            Call Main.SetCS(CS, "Name", Title)
                            ' if authormemberid=0, then use creator
                            'Call Main.SetCS(cs, "AuthorMemberID", Main.MemberID)
                            '
                            RSSTitle = Trim(Title)
                            If RSSTitle = "" Then
                                RSSTitle = "Blog Post " & EntryID
                            End If
                            Call Main.SetCS(CS, "RSSTitle", RSSTitle)
                            '
                            NewPostCopy = Main.ReadVirtualFile("blogs/DefaultPostCopy.txt")
                            If NewPostCopy = "" Then
                                NewPostCopy = DefaultPostCopy
                            End If
                            Call Main.SetCS(CS, "copy", NewPostCopy)
                            '
                            qs = ""
                            qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                            qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                            Call Main.AddLinkAlias(Title, Main.renderedPageId, qs)
                            csLink = Main.OpenCSContent("link Aliases", "(pageid=" & Main.renderedPageId & ")and(QueryStringSuffix=" & KmaEncodeSQLText(qs) & ")")
                            If Main.csok(csLink) Then
                                EntryLink = Main.GetCSText(csLink, "name")
                            End If
                            Call Main.CloseCS(csLink)
                            If InStr(1, EntryLink, Main.SiteProperty_AdminURL, vbTextCompare) = 0 Then
                                Call Main.SetCS(CS, "RSSLink", EntryLink)
                            End If
                            '
                            Call Main.SetCS(CS, "RSSDescription", filterCopy(NewPostCopy, 150))
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
                    CS = Main.OpenCSContent(cnBlogs, "(id=" & KmaEncodeSQLNumber(blogId) & ")", "ID")
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
                Call Main.testpoint("Blogs.GetContent, Main.IsAuthenticated=" & Main.IsAuthenticated)
                Call Main.testpoint("Blogs.GetContent, Main.MemberID=" & Main.MemberID)
                Call Main.testpoint("Blogs.GetContent, BlogOwnerID=" & BlogOwnerID)
                Call Main.testpoint("Blogs.GetContent, IsBlogOwner=" & IsBlogOwner)
                If IsBlogOwner And (blogId = 0) Then
                    '
                    ' Blog record was not, or can not be created
                    '
                    s = s & Main.GetAdminHintWrapper("This blog has not been configured. Please edit this page and edit the properties for the blog Add-on")
                Else
                    '
                    ' Get the Feed Args
                    '
                    CSFeed = Main.OpenCSContent(cnRSSFeeds, "id=" & RSSFeedId)
                    If Main.IsCSOK(CSFeed) Then
                        RSSFeedName = Main.GetCS(CSFeed, "Name")
                        RSSFeedFilename = Main.GetCS(CSFeed, "RSSFilename")
                        'BlogRootLink = Main.GetCS(CSFeed, "Link")
                    End If
                    Call Main.CloseCS(CSFeed)
                    '
                    If RSSFeedFilename = "" Then
                        'If BlogRootLink = "" Or RSSFeedFilename = "" Then
                        '
                        ' feed has not been initialized yet, call it now
                        '
                        Call VerifyFeedReturnArgs(blogId, blogListLink, RSSFeedId, RSSFeedName, RSSFeedFilename)
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
                    'WorkingQueryString = Main.RefreshQueryString
                    'If WorkingQueryString = "" Then
                    '    WorkingQueryString = "?"
                    'Else
                    '    WorkingQueryString = "?" & WorkingQueryString & "&"
                    'End If
                    '
                    If ButtonValue <> "" Then
                        '
                        ' Process the source form into form if there was a button - else keep formid
                        '
                        FormID = ProcessForm(SourceFormID, blogId, IsBlogOwner, EntryID, ButtonValue, BlogName, BlogOwnerID, AllowAnonymous, AllowCategories, BlogCategoryID, RSSFeedId, blogListLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, autoApproveComments, authoringGroupId, emailComment, OptionString, allowRecaptcha)
                    End If
                    '
                    ' Get Next Form
                    '
                    s = s & GetForm(FormID, blogId, BlogName, IsBlogOwner, ArchiveMonth, ArchiveYear, EntryID, KeywordList, ButtonValue, DateSearchText, AllowAnonymous, AllowCategories, BlogCategoryID, RSSFeedName, RSSFeedFilename, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogDescription, blogCaption, RSSFeedId, blogListLink, blogListQs, allowRecaptcha)
                End If

            '
            GetContent = s
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetForm(FormID As Integer, blogId As Integer, BlogName As String, IsBlogOwner As Boolean, ArchiveMonth As Integer, ArchiveYear As Integer, EntryID As Integer, KeywordList As String, ButtonValue As String, DateSearchText As String, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, RSSFeedName As String, RSSFeedFilename As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogDescription As String, blogCaption As String, RSSFeedId As Integer, blogListLink As String, blogListQs As String, allowCaptcha As Boolean) As String
            '
            Dim Stream As String
            Dim IsEditing As Boolean
            '
            'Call Main.AddRefreshQueryString(RequestNameSourceFormID, FormID)
            IsEditing = Main.IsAuthoring("Blogs")
            allowAuthorInfoLink = Csv.IsContentFieldSupported("people", "autoinfoLink")
            '
            Select Case FormID
                Case FormBlogPostDetails
                    Stream = Stream & GetFormBlogPostDetails(blogId, EntryID, IsBlogOwner, AllowAnonymous, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs, allowCaptcha)
                Case FormBlogArchiveDateList
                    Stream = Stream & GetFormBlogArchiveDateList(blogId, BlogName, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                Case FormBlogArchivedBlogs
                    Stream = Stream & GetFormBlogArchivedBlogs(blogId, BlogName, ArchiveMonth, ArchiveYear, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                Case FormBlogEntryEditor
                    Stream = Stream & GetFormBlogPost(blogId, BlogName, IsBlogOwner, EntryID, AllowCategories, BlogCategoryID, blogListLink)
                Case FormBlogSearch
                    Stream = Stream & GetFormBlogSearch(blogId, BlogName, IsBlogOwner, KeywordList, ButtonValue, DateSearchText, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                Case Else
                    If EntryID <> 0 Then
                        '
                        ' Go to details page
                        '
                        FormID = FormBlogPostDetails
                        Stream = Stream & GetFormBlogPostDetails(blogId, EntryID, IsBlogOwner, AllowAnonymous, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs, allowCaptcha)
                    Else
                        '
                        ' list all the entries
                        '
                        FormID = FormBlogPostList
                        Stream = Stream & GetFormBlogPostList(blogId, BlogName, IsBlogOwner, IsEditing, EntryID, AllowCategories, BlogCategoryID, RSSFeedName, RSSFeedFilename, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogDescription, blogCaption, RSSFeedId, blogListLink, blogListQs)
                    End If
            End Select
            '
            GetForm = Stream
        End Function
        '
        '====================================================================================
        '   Processes the SourceFormID
        '       returns the FormID for the next one to display
        '====================================================================================
        '
        Private Function ProcessForm(SourceFormID As Integer, blogId As Integer, IsBlogOwner As Boolean, EntryID As Integer, ButtonValue As String, BlogName As String, BlogOwnerID As Integer, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, RSSFeedId As Integer, blogListLink As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, autoApproveComments As Boolean, authoringGroupId As Integer, emailComment As Boolean, OptionString As String, allowRecaptcha As Boolean) As Integer
            '
            If ButtonValue <> "" Then
                Select Case SourceFormID
                    Case FormBlogPostList
                        ProcessForm = FormBlogPostList
                    Case FormBlogEntryEditor
                        ProcessForm = ProcessFormBlogPost(SourceFormID, blogId, EntryID, ButtonValue, BlogCategoryID, RSSFeedId, blogListLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax)
                    Case FormBlogPostDetails
                        ProcessForm = ProcessFormBlogPostDetails(SourceFormID, blogId, IsBlogOwner, ButtonValue, BlogName, BlogOwnerID, AllowAnonymous, BlogCategoryID, autoApproveComments, authoringGroupId, emailComment, OptionString, allowRecaptcha)
                    Case FormBlogArchiveDateList
                        ProcessForm = FormBlogArchiveDateList
                    Case FormBlogSearch
                        ProcessForm = FormBlogSearch
                    Case FormBlogArchivedBlogs
                        ProcessForm = FormBlogArchivedBlogs
                End Select
            End If
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogSearch(blogId As Integer, BlogName As String, IsBlogOwner As Boolean, KeywordList As String, ButtonValue As String, DateSearchText As String, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String) As String
            '
            'Dim EntryCopyOverview As String
            Dim BlogTagList As String
            Dim PodcastMediaLink As String
            Dim PodcastSize As String
            Dim Return_CommentCnt As Integer
            Dim DateSearch As Date
            Dim Subcaption As String
            Dim AuthorMemberID As Integer
            Dim ParentID As Integer
            Dim ResultPtr As Integer
            Dim EntryID As Integer
            Dim EntryName As String
            Dim EntryCopy As String
            Dim CommentID As Integer
            Dim CommentName As String
            Dim CommentCopy As String
            Dim DateAdded As Date
            Dim Approved As Boolean
            Dim s As String
            Dim CS As Integer
            Dim SQL As String
            Dim CommentPointer As Integer
            Dim CommentCount As Integer
            Dim CommentSQL As String
            Dim CommentString As String
            Dim CommentBlogID As Integer
            Dim CSP As String
            Dim Divider As String
            Dim qs As String
            '
            Dim CSPointer As Integer
            Dim Criteria As String
            Dim SearchMonth As String
            Dim SearchYear As String
            Dim DateString As String
            Dim KeyWordsArray() As String
            Dim KeyWordsCounter As Integer
            Dim KeyWordsArrayCounter As Integer
            Dim EnteredKeyWords As String
            Dim CounterKeyWords As Integer
            Dim OrClause As String
            Dim Button As String
            Dim OrCnt As Integer
            Dim allowComments As Boolean
            Dim QueryTag As String
            Dim imageDisplayTypeId As Integer
            Dim searchForm As String
            Dim primaryImagePositionId As Integer
            Dim articlePrimaryImagePositionId As Integer
            '
            'Call Main.AddRefreshQueryString(RequestNameSourceFormID, FormBlogSearch)
            'Call Main.AddRefreshQueryString(RequestNameFormID, FormBlogSearch)
            Call Main.AddRefreshQueryString(RequestNameBlogEntryID, "")
            '
            s = s & vbCrLf & Main.GetContentCopy2("Blogs Search Header for " & BlogName, , "<h2>" & BlogName & " Blog Search</h2>")
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
                Dim subCriteria As String
                Criteria = "(blogId=" & blogId & ")"
                subCriteria = ""
                If (KeywordList <> "") Then
                    KeywordList = "," & KeywordList & ","
                    KeyWordsArray = Split(KeywordList, ",", , vbTextCompare)
                    KeyWordsArrayCounter = UBound(KeyWordsArray)
                    For CounterKeyWords = 0 To KeyWordsArrayCounter
                        If KeyWordsArray(CounterKeyWords) <> "" Then
                            If subCriteria <> "" Then
                                subCriteria = subCriteria & "or"
                                Subcaption = Subcaption & " or "
                            End If
                            EnteredKeyWords = KeyWordsArray(CounterKeyWords)
                            Subcaption = Subcaption & "'<i>" & kmaEncodeHTML(EnteredKeyWords) & "</i>'"
                            EnteredKeyWords = KmaEncodeSQLText(EnteredKeyWords)
                            EnteredKeyWords = "'%" & Mid(EnteredKeyWords, 2, Len(EnteredKeyWords) - 2) & "%'"
                            subCriteria = subCriteria & " (Copy like " & EnteredKeyWords & ")"
                        End If
                    Next
                    If Subcaption <> "" Then
                        Subcaption = " containing " & Subcaption
                    End If
                    If subCriteria <> "" Then
                        Criteria = Criteria & "and(" & subCriteria & ")"
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
                '
                If Not Main.csok(CSPointer) Then
                    s = s & cr & "<div class=""aoBlogProblem"">There were no matches to your search</div>"
                Else
                    s = s & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                    Do While Main.csok(CSPointer)
                        ParentID = Main.GetCSInteger(CSPointer, "EntryID")
                        If ParentID = 0 Then
                            '
                            ' Entry
                            '
                            AuthorMemberID = Main.GetCSInteger(CSPointer, "AuthorMemberID")
                            If AuthorMemberID = 0 Then
                                AuthorMemberID = Main.GetCSInteger(CSPointer, "createdBy")
                            End If
                            EntryID = Main.GetCSInteger(CSPointer, "ID")
                            EntryName = Main.GetCSText(CSPointer, "name")
                            EntryCopy = Main.GetCSText(CSPointer, "copy")
                            'EntryCopyOverview = Main.GetCSText(CSPointer, "copyOverview")
                            DateAdded = Main.GetCSDate(CSPointer, "DateAdded")
                            allowComments = Main.GetCSBoolean(CSPointer, "allowComments")
                            PodcastMediaLink = Main.GetCS(CSPointer, "PodcastMediaLink")
                            PodcastSize = Main.GetCS(CSPointer, "PodcastSize")
                            BlogTagList = Main.GetCS(CSPointer, "TagList")
                            imageDisplayTypeId = Main.GetCSInteger(CSPointer, "imageDisplayTypeId")
                            primaryImagePositionId = Main.GetCSInteger(CSPointer, "primaryImagePositionId")
                            articlePrimaryImagePositionId = Main.GetCSInteger(CSPointer, "articlePrimaryImagePositionId")
                            s = s & GetBlogEntryCell(ResultPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, True, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, "", ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                        Else
                            '
                            ' Comment
                            '
                            AuthorMemberID = Main.GetCSInteger(CSPointer, "createdBy")
                            CommentID = Main.GetCSInteger(CSPointer, "ID")
                            CommentName = Main.GetCSText(CSPointer, "name")
                            CommentCopy = Main.GetCSText(CSPointer, "copyText")
                            DateAdded = Main.GetCSDate(CSPointer, "DateAdded")
                            Approved = Main.GetCSBoolean(CSPointer, "Approved")
                            EntryID = Main.GetCSInteger(CSPointer, "EntryID")
                            EntryName = Main.GetRecordName("Blog Copy", EntryID)
                            s = s & GetBlogCommentCell(IsBlogOwner, DateAdded, Approved, CommentName, CommentCopy, CommentID, ResultPtr, 0, AuthorMemberID, EntryID, True, EntryName)
                        End If
                        '
                        s = s & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                        ResultPtr = ResultPtr + 1
                        Call Main.NextCSRecord(CSPointer)
                    Loop
                End If
                Call Main.CloseCS(CSPointer)
                s = "" _
        & cr & "<div class=""aoBlogSearchResultsCon"">" _
        & kmaIndent(s) _
        & cr & "</div>"
            End If
            '
            searchForm = "" _
    & cr & "<table width=100% border=0 cellspacing=0 cellpadding=5 class=""aoBlogSearchTable"">" _
    & kmaIndent(GetFormTableRow("Date:", GetField(RequestNameDateSearch, , 10, , Main.GetStreamText(RequestNameDateSearch)) & " " & "&nbsp;(mm/yyyy)")) _
    & kmaIndent(GetFormTableRow("Keyword(s):", GetField(RequestNameKeywordList, , , 30, Main.GetStreamText(RequestNameKeywordList)))) _
    & kmaIndent(GetFormTableRow("", Main.GetFormButton(FormButtonSearch))) _
    & kmaIndent(GetFormTableRow2("<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>")) _
    & cr & "</table>" _
    & ""
            searchForm = "" _
    & cr & "<div  class=""aoBlogSearchFormCon"">" _
    & kmaIndent(searchForm) _
    & cr & "</div>" _
    & ""
            s = s & searchForm
            s = "" _
    & cr & Main.GetFormStart _
    & kmaIndent(s) _
    & cr & "<input type=""hidden"" name=""" & RequestNameSourceFormID & """ value=""" & FormBlogSearch & """>" _
    & cr & "<input type=""hidden"" name=""" & RequestNameFormID & """ value=""" & FormBlogSearch & """>" _
    & cr & Main.GetFormEnd
            '
            GetFormBlogSearch = s
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogArchiveDateList(blogId As Integer, BlogName As String, IsEditing As Boolean, IsBlogOwner As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String) As String
            '
            Dim s As String
            Dim CS As Integer
            Dim CSPointer As Integer
            Dim ArchiveList As String ' ** link for different month
            Dim TitleBlog As String
            Dim DateBlog As String
            Dim CopyBlog As String
            Dim MonthCounter As Integer
            Dim ThisMonth As Integer
            Dim ThisYear As Integer
            Dim ArchiveMonth As Integer
            Dim ArchiveYear As Integer
            Dim Counter As Integer
            Dim NameOfMonth As String
            Dim DateAddedSQL As String
            Dim FieldList As String
            Dim qs As String
            Dim RowCnt As Integer
            Dim SQL As String
            '
            s = s & vbCrLf & Main.GetContentCopy2("Blogs Archives Header for " & BlogName, , "<h2>" & BlogName & " Blog Archive</h2>")
            '
            SQL = "SELECT distinct Month(DateAdded) as ArchiveMonth, year(dateadded) as ArchiveYear " _
    & " From ccBlogCopy" _
    & " Where (ContentControlID = " & Main.GetContentID(cnBlogEntries) & ") And (Active <> 0)" _
    & " AND (BlogID=" & blogId & ")" _
    & " ORDER BY year(dateadded) desc, Month(DateAdded) desc"
            CS = Main.OpenCSSQL("Default", SQL)
            If Not Main.IsCSOK(CS) Then
                '
                ' No archives, give them an error
                '
                s = s & cr & "<div class=""aoBlogProblem"">There are no current blog entries</div>"
                s = s & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"

            Else
                RowCnt = Main.GetCSRowCount(CS)
                If RowCnt = 0 Then
                    '
                    ' weird - just give them the same error
                    '
                    s = s & cr & "<div class=""aoBlogProblem"">There are no current blog posts</div>"
                    s = s & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                ElseIf RowCnt = 1 Then
                    '
                    ' one archive - just display it
                    '
                    ArchiveMonth = Main.GetCSInteger(CS, "ArchiveMonth")
                    ArchiveYear = Main.GetCSInteger(CS, "ArchiveYear")
                    s = s & GetFormBlogArchivedBlogs(blogId, BlogName, ArchiveMonth, ArchiveYear, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                Else
                    '
                    ' Display List of archive
                    '
                    qs = ModifyQueryString(blogListQs, RequestNameSourceFormID, FormBlogArchiveDateList)
                    qs = ModifyQueryString(qs, RequestNameFormID, FormBlogArchivedBlogs)
                    Do While Main.IsCSOK(CS)
                        ArchiveMonth = Main.GetCSInteger(CS, "ArchiveMonth")
                        ArchiveYear = Main.GetCSInteger(CS, "ArchiveYear")
                        NameOfMonth = MonthName(ArchiveMonth)
                        qs = ModifyQueryString(qs, RequestNameArchiveMonth, CStr(ArchiveMonth))
                        qs = ModifyQueryString(qs, RequestNameArchiveYear, CStr(ArchiveYear))
                        '
                        s = s & vbCrLf & vbTab & vbTab & "<div class=""aoBlogArchiveLink""><a href=""?" & qs & """>" & NameOfMonth & " " & ArchiveYear & "</a></div>"
                        's = s & vbCrLf & vbTab & vbTab & "<div class=""aoBlogArchiveLink""><a href=""?" & qs & RequestNameFormID & "=" & FormBlogArchivedBlogs & "&" & RequestNameArchiveMonth & "=" & ArchiveMonth & "&" & RequestNameArchiveYear & "=" & ArchiveYear & "&" & RequestNameSourceFormID & "=" & FormBlogArchiveDateList & """>" & NameOfMonth & " " & ArchiveYear & "</a></div>"
                        Main.NextCSRecord(CS)
                    Loop
                    s = s & vbCrLf & vbTab & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                End If
            End If
            '
            GetFormBlogArchiveDateList = s
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogArchivedBlogs(blogId As Integer, BlogName As String, ArchiveMonth As Integer, ArchiveYear As Integer, IsEditing As Boolean, IsBlogOwner As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String) As String
            '
            'Dim EntryCopyOverview As String
            Dim PodcastMediaLink As String
            Dim PodcastSize As String
            Dim Return_CommentCnt As Integer
            Dim qs As String
            Dim EntryCopy As String
            Dim EntryName As String
            Dim entryEditLink As String
            Dim DateAdded As Date
            Dim AuthorMemberID As Integer
            Dim EntryID As Integer
            Dim BlogTagList As String
            Dim EntryPtr As Integer
            Dim s As String
            Dim ArchivesSQL As Integer
            Dim ArchivesPointer As Integer
            Dim SelectedDate As String
            Dim BlahDate As Integer
            Dim CurrentBLogPointer As Integer
            Dim CurrentBlogID As Integer
            Dim PageNumber As Integer
            Dim CS As Integer
            Dim allowComments As Boolean
            Dim imageDisplayTypeId As Integer
            Dim primaryImagePositionId As Integer
            Dim articlePrimaryImagePositionId As Integer
            '
            ' If it is the current month, start at entry 6
            '
            PageNumber = 1
            'If Month(Now) = ArchiveMonth And Year(Now) = ArchiveYear Then
            '    PageNumber = 2
            'End If
            IsEditing = Main.IsAuthoring("Blogs")
            s = s & vbCrLf & Main.GetFormStart
            '
            ' List Blog Entries
            '
            CS = Main.OpenCSContent(cnBlogEntries, "(Month(DateAdded)=" & ArchiveMonth & ")and(year(DateAdded)=" & ArchiveYear & ")and(BlogID=" & blogId & ")", "DateAdded Desc", , , , , PostsToDisplay, PageNumber)
            If Not Main.csok(CS) Then
                s = s & cr & "<div class=""aoBlogProblem"">There are no blog archives for " & ArchiveMonth & "/" & ArchiveYear & "</div>"
            Else
                EntryPtr = 0
                entryEditLink = ""
                Do While Main.csok(CS)
                    EntryID = Main.GetCSInteger(CS, "ID")
                    AuthorMemberID = Main.GetCSInteger(CS, "AuthorMemberID")
                    If AuthorMemberID = 0 Then
                        AuthorMemberID = Main.GetCSInteger(CS, "createdBy")
                    End If
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
                    imageDisplayTypeId = Main.GetCSInteger(CS, "imageDisplayTypeId")
                    primaryImagePositionId = Main.GetCSInteger(CS, "primaryImagePositionId")
                    articlePrimaryImagePositionId = Main.GetCSInteger(CS, "articlePrimaryImagePositionId")
                    s = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                    Call Main.NextCSRecord(CS)
                    s = s & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                    EntryPtr = EntryPtr + 1
                Loop
            End If
            qs = blogListQs
            qs = ModifyQueryString(qs, RequestNameFormID, FormBlogSearch)
            s = s & cr & "<div>&nbsp;</div>"
            s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
            qs = Main.RefreshQueryString()
            qs = ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
            qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
            s = s & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
            Call Main.CloseCS(CS)
            '
            's = s & Main.GetFormInputHidden(RequestNameBlogEntryID, CommentBlogID)
            s = s & Main.GetFormInputHidden(RequestNameSourceFormID, FormBlogArchivedBlogs)
            s = s & Main.GetFormEnd
            '
            GetFormBlogArchivedBlogs = s
            '
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormTableRow(FieldCaption As String, Innards As String, Optional AlignLeft As Boolean)
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
            Stream = Stream & "<tr>"
            Stream = Stream & "<td class=""aoBlogTableRowCellLeft"" " & AlignmentString & ">" & FieldCaption & "<img src=""/cclib/images/spacer.gif"" width=100 height=1 alt="" ""></TD>"
            Stream = Stream & "<td class=""aoBlogTableRowCellRight"">" & Innards & "</TD>"
            Stream = Stream & "</tr>"
            '
            GetFormTableRow = Stream
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormTableRow2(Innards As String) As String
            '
            Dim Stream As String
            '
            Stream = Stream & "<tr>"
            Stream = Stream & "<td colspan=2 width=""100%"">" & Innards & "</TD>"
            Stream = Stream & "</tr>"
            '
            GetFormTableRow2 = Stream
            '
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetField(RequestName As String, Optional Height As String, Optional Width As String, Optional MaxLenghth As String, Optional DefaultValue As String) As String
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
            Stream = Replace(Stream, "<INPUT ", "<INPUT maxlength=""" & MaxLenghth & """ ", 1, 99, 1)
            '
            GetField = Stream
            '
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function Random(Lowerbound As Integer, Upperbound As Integer)
            '
            Randomize()
            Random = Int(Rnd() * Upperbound) + Lowerbound
            '
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogPostList(blogId As Integer, BlogName As String, IsBlogOwner As Boolean, IsEditing As Boolean, EntryID As Integer, AllowCategories As Boolean, BlogCategoryID As Integer, RSSFeedName As String, RSSFeedFilename As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogDescription As String, blogCaption As String, RSSFeedId As Integer, blogListLink As String, blogListQs As String) As String
            '
            'Dim EntryCopyOverview As String
            Dim BlogTagList As String
            Dim entryEditLink As String
            Dim PodcastMediaLink As String
            Dim PodcastSize As String
            Dim FeedFooter As String
            Dim ReturnFooter As String
            Dim CategoryFooter As String
            Dim TestCategoryID As Integer
            Dim GroupList As String
            Dim BlogCategoryName As String
            Dim Return_CommentCnt As Integer
            Dim qs As String
            Dim CommentPtr As Integer
            Dim CommentID As Integer
            Dim Approved As Boolean
            Dim CommentName As String
            Dim CommentCopy As String
            Dim s As String
            Dim CS As Integer
            Dim SQL As String
            Dim CommentPointer As Integer
            Dim CommentCount As Integer
            Dim CommentSQL As String
            Dim CommentString As String
            Dim CommentBlogID As Integer
            Dim CSP As String
            Dim Divider As String
            Dim AddBlogFlag As Boolean
            Dim SearchFlag As Boolean
            Dim CSCount As Integer
            Dim AuthorMemberID As Integer
            Dim DateAdded As Date
            Dim RowCopy As String
            'Dim IsEditing As Boolean
            Dim EntryName As String
            Dim Criteria As String
            Dim EntryCopy As String
            Dim allowComments As Boolean
            Dim EntryPtr As Integer
            Dim locBlogTitle As String
            Dim IsBlocked As Boolean
            Dim CSCat As Integer
            Dim Patch As New PatchClass
            Dim NoneMsg As String
            Dim imageDisplayTypeId As Integer
            Dim primaryImagePositionId As Integer
            Dim articlePrimaryImagePositionId As Integer
            Dim categoryLink As String
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
                CS = Main.OpenCSContent(cnBlogEntries, "(BlogID=" & blogId & ")and(BlogCategoryID=" & BlogCategoryID & ")", "DateAdded Desc")
                s = s & cr & "<div class=""aoBlogCategoryCaption"">Category " & BlogCategoryName & "</div>"
                NoneMsg = "There are no blog posts available in the category " & BlogCategoryName
            Else
                CS = Main.OpenCSContent(cnBlogEntries, "(BlogID=" & blogId & ")", "DateAdded Desc")
                NoneMsg = "There are no blog posts available"
            End If
            If Not Main.csok(CS) Then
                s = s & cr & "<div class=""aoBlogProblem"">" & NoneMsg & "</div>"
            Else
                Do While Main.csok(CS) And EntryPtr < PostsToDisplay
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
                        If AuthorMemberID = 0 Then
                            AuthorMemberID = Main.GetCSInteger(CS, "createdBy")
                        End If
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
                        imageDisplayTypeId = Main.GetCSInteger(CS, "imageDisplayTypeId")
                        primaryImagePositionId = Main.GetCSInteger(CS, "primaryImagePositionId")
                        articlePrimaryImagePositionId = Main.GetCSInteger(CS, "articlePrimaryImagePositionId")
                        s = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                        s = s & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
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
                CategoryFooter = CategoryFooter & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>See posts in all categories</a></div>"
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
                        'qs = ModifyQueryString(qs, RequestNameBlogCategoryIDSet, CStr(BlogCategoryID), True)
                        categoryLink = kmaModifyLinkQuery(blogListLink, RequestNameBlogCategoryIDSet, CStr(BlogCategoryID), True)
                        CategoryFooter = CategoryFooter & cr & "<div class=""aoBlogFooterLink""><a href=""" & categoryLink & """>See posts in the category " & Main.GetCS(CS, "name") & "</a></div>"
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
                Call Main.testpoint("Blogs.GetFormBlogPostList, IsBlogOwner=true, appending 'create' message")
                '
                ' Create a new entry if this is the Blog Owner
                '
                qs = Main.RefreshQueryString
                qs = ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor, True)
                s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Create new post</a></div>"
                '
                ' Create a link to edit the blog record
                '
                qs = "cid=" & Main.GetContentID("Blogs") & "&af=4&id=" & blogId
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
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogPost(blogId As Integer, BlogName As String, IsBlogOwner As Boolean, EntryID As Integer, AllowCategories As Boolean, BlogCategoryID As Integer, blogListLink As String) As String
            '
            Dim editor As String
            Dim BlogCopy As String
            Dim BlogTagList As String
            Dim Ptr As Integer
            Dim imageName As String
            Dim imageDescription As String
            Dim imageFilename As String
            Dim ImageID As Integer
            Dim Pos As Integer
            Dim CategorySelect As String
            Dim hint As String
            Dim Button As String
            Dim qs As String
            Dim c As String
            Dim s As String
            Dim CS As Integer
            Dim SQL As String
            Dim CommentPointer As Integer
            Dim CommentCount As Integer
            Dim CommentSQL As String
            Dim CommentString As String
            Dim CommentBlogID As Integer
            Dim Divider As String
            Dim BlogTitle As String
            '
            hint = "1"
            s = s & Main.GetUploadFormStart
            s = s & "<table width=100% border=0 cellspacing=0 cellpadding=5 class=""aoBlogPostTable"">"
            '
            'AddBlogFlag = Main.GetStreamBoolean(RequestNameAddBlogFlag)
            '
            If EntryID = 0 Then
                hint = "2"
                '
                ' New Entry that is being saved
                '
                s = s & GetFormTableRow2(Main.GetContentCopy2("Blog Create Header for " & BlogName, , "<h2>Create a new blog post</h2>"))
                BlogTitle = Main.GetStreamText(RequestNameBlogTitle)
                BlogCopy = Main.GetStreamText(RequestNameBlogCopy)
                BlogTagList = Main.GetStreamText(RequestNameBlogTagList)
            Else
                hint = "3"
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
            hint = "4"
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
            s = s & GetFormTableRow("<div style=""padding-top:3px"">Title: </div>", Main.GetFormInputText(RequestNameBlogTitle, BlogTitle, 1, 50))
            s = s & GetFormTableRow("<div style=""padding-top:108px"">Post: </div>", editor)
            s = s & GetFormTableRow("<div style=""padding-top:3px"">Tag List: </div>", Main.GetFormInputText(RequestNameBlogTagList, BlogTagList, 5, 50))
            If AllowCategories Then
                hint = "5"
                CategorySelect = Main.GetFormInputSelect(RequestNameBlogCategoryIDSet, BlogCategoryID, "Blog Categories")
                If (InStr(1, CategorySelect, "<option value=""""></option></select>", vbTextCompare) <> 0) Then
                    '
                    ' Select is empty
                    '
                    CategorySelect = "<div>This blog has no categories defined</div>"
                End If
                s = s & GetFormTableRow("Category: ", CategorySelect)
            End If
            '
            ' file upload form taken from Resource Library
            '
            c = ""
            c = c _
    & "<TABLE id=""UploadInsert"" border=""0"" cellpadding=""0"" cellspacing=""1"" width=""100%"" class=""aoBlogImageTable"">" _
    & "<tr>" _
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
            & "<tr>" _
            & "<td><HR></TD>" _
            & "</tr>" _
            & ""
                End If
                '
                '   image
                '
                c = c _
        & "<tr>" _
        & "<td><input type=""checkbox"" name=""" & rnBlogImageDelete & "." & Ptr & """>&nbsp;Delete</TD>" _
        & "</tr>" _
        & "<tr>" _
        & "<td align=""left"" class=""ccAdminSmall""><img alt=""" & imageName & """ title=""" & imageName & """ src=""" & Main.serverFilePath & imageFilename & """></TD>" _
        & "</tr>" _
        & ""
                '
                '   order
                '
                c = c _
        & "<tr>" _
        & "<td>Order<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageOrder & "." & Ptr & """ SIZE=""2"" value=""" & Ptr & """></TD>" _
        & "</tr>" _
        & ""
                '
                '   name
                '
                c = c _
        & "<tr>" _
        & "<td>Name<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageName & "." & Ptr & """ SIZE=""25"" value=""" & kmaEncodeHTML(imageName) & """></TD>" _
        & "</tr>" _
        & ""
                '
                '   description
                '
                c = c _
        & "<tr>" _
        & "<td>Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50"">" & kmaEncodeHTML(imageDescription) & "</TEXTAREA><input type=""hidden"" name=""" & rnBlogImageID & "." & Ptr & """ value=""" & ImageID & """></TD>" _
        & "</tr>" _
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
    & "<td><HR></TD>" _
    & "</tr>" _
    & ""
            '
            '   order
            '
            c = c _
    & "<tr>" _
    & "<td align=""left"">Order<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageOrder & "." & Ptr & """ SIZE=""2"" value=""" & Ptr & """></TD>" _
    & "</tr>" _
    & ""
            '
            '   image
            '
            c = c _
    & "<tr>" _
    & "<td align=""left"">Image<br><INPUT TYPE=""file"" name=""LibraryUpload." & Ptr & """></TD>" _
    & "</tr>" _
    & ""
            '
            '   name
            '
            c = c _
    & "<tr>" _
    & "<td align=""left"">Name<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageName & "." & Ptr & """ SIZE=""25""></TD>" _
    & "</tr>" _
    & ""
            '
            '   description
            '
            c = c _
    & "<tr>" _
    & "<td align=""left"">Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50""></TEXTAREA></TD>" _
    & "</tr>" _
    & ""
            '
            c = c _
    & "</Table>" _
    & ""
            '
            c = c _
    & "<TABLE border=""0"" cellpadding=""0"" cellspacing=""1"" width=""100%""  class=""aoBlogNewRowTable"">" _
    & "<tr><td Width=""30""><img src=/ResourceLibrary/spacer.gif width=30 height=1></TD><td align=""left""><a href=""#"" onClick=""blogNewRow();return false;"">+ upload more files</a></TD></tr>" _
    & "</Table>" & Main.GetFormInputHidden("LibraryUploadCount", Ptr, "LibraryUploadCount") _
    & ""
            '
            s = s & GetFormTableRow("Images: ", c)
            If EntryID <> 0 Then
                hint = "6"
                s = s & GetFormTableRow("", Main.GetFormButton(FormButtonPost) & "&nbsp;" & Main.GetFormButton(FormButtonCancel) & "&nbsp;" & Main.GetFormButton(FormButtonDelete))
            Else
                hint = "7"
                s = s & GetFormTableRow("", Main.GetFormButton(FormButtonPost) & "&nbsp;" & Main.GetFormButton(FormButtonCancel))
            End If
            hint = "8"
            qs = Main.RefreshQueryString()
            qs = ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
            qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
            hint = "9"
            s = s & vbCrLf & GetFormTableRow2("<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>")

            '
            s = s & Main.GetFormInputHidden(RequestNameBlogEntryID, EntryID)
            s = s & Main.GetFormInputHidden(RequestNameSourceFormID, FormBlogEntryEditor)
            s = s & "</table>"
            hint = "95"
            s = s & Main.GetFormEnd
            '
            GetFormBlogPost = s
            hint = "96"
            Call Main.SetVisitProperty(SNBlogEntryName, CStr(GetRandomInteger()))
            '
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function ProcessFormBlogPost(SourceFormID As Integer, blogId As Integer, EntryID As Integer, ButtonValue As String, BlogCategoryID As Integer, RSSFeedId As Integer, blogListLink As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer) As Integer
            '
            Dim UploadCount As Integer
            Dim UploadPointer As Integer
            Dim Copy As String
            Dim FileExtension As String
            Dim FilenameNoExtension As String
            Dim VirtualFilePath As String
            Dim BlogImageID As Integer
            Dim ImageID As Integer
            Dim imageOrder As Integer
            Dim imageName As String
            Dim imageDescription As String
            Dim AltSizeList As String
            Dim ImageWidth As Integer
            Dim ImageHeight As Integer
            Dim sf As Object
            Dim qs As String
            Dim EntryName As String
            Dim EntryLink As String
            Dim CS As Integer
            Dim SN As String
            Dim SNName As String
            Dim VirtualFilePathPage As String
            Dim Pos As Integer
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
                        If Main.csok(CS) Then
                            EntryID = Main.GetCS(CS, "ID")
                            Call Main.SetCS(CS, "BlogID", blogId)
                        End If
                        Call Main.CloseCS(CS)
                    End If
                    CS = Main.OpenCSContent(cnBlogEntries, "(blogid=" & blogId & ")and(ID=" & EntryID & ")")
                    If Main.csok(CS) Then
                        EntryName = Main.GetStreamText(RequestNameBlogTitle)
                        Call Main.SetCS(CS, "Name", EntryName)
                        Call Main.SetCS(CS, "copy", Main.GetStreamText(RequestNameBlogCopy))
                        Call Main.SetCS(CS, "TagList", Main.GetStreamText(RequestNameBlogTagList))
                        ' if authormemberid=0, use createdby
                        'Call Main.SetCS(cs, "AuthorMemberID", Main.MemberID)
                        Call Main.SetCS(CS, "BlogCategoryID", BlogCategoryID)
                        ProcessFormBlogPost = FormBlogPostList
                    End If
                    Call Main.CloseCS(CS)
                    Call UpdateBlogFeed(blogId, RSSFeedId, blogListLink)
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

                                        sf.LoadFromFile(Main.PhysicalFilePath & VirtualFilePathPage)
                                        If Err.Number = 0 Then
                                            ImageWidth = sf.Width
                                            ImageHeight = sf.Height
                                            Call Main.SetCS(CS, "height", ImageHeight)
                                            Call Main.SetCS(CS, "width", ImageWidth)
                                        Else
                                            Err.Clear()
                                        End If
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
                    Call Main.DeleteContentRecords(cnBlogEntries, "(blogid=" & blogId & ")and(ID=" & EntryID & ")")
                    ProcessFormBlogPost = FormBlogPostList
                    Call UpdateBlogFeed(blogId, RSSFeedId, blogListLink)
                    ProcessFormBlogPost = FormBlogPostList
                End If
            End If
            '
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogPostDetails(blogId As Integer, EntryID As Integer, IsBlogOwner As Boolean, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String, allowCaptcha As Boolean) As String
            '
            'Dim EntryCopyOverview As String
            Dim BlogTagList As String
            Dim entryEditLink As String
            Dim PodcastMediaLink As String
            Dim PodcastSize As String
            Dim formKey As String
            Dim Return_CommentCnt As Integer
            Dim Copy As String
            Dim DateAdded As Date
            Dim AuthorMemberID As Integer
            Dim s As String
            Dim CS As Integer
            Dim Divider As String
            Dim RandomNumber As Integer
            Dim Approved As Boolean
            Dim CommentName As String
            Dim CommentCopy As String
            Dim CommentPtr As Integer
            Dim CommentID As Integer
            Dim EntryName As String
            Dim EntryCopy As String
            Dim allowComments As Boolean
            Dim EntryPtr As Integer
            Dim qs As String
            Dim QSBack As String
            Dim CommentCnt As Integer
            Dim imageDisplayTypeId As Integer
            Dim loginForm As String
            Dim primaryImagePositionId As Integer
            Dim articlePrimaryImagePositionId As Integer
            '
            'Dim allowCaptcha As Boolean
            'Dim blogId As Integer

            Call Main.testpoint("blog -- getFormBlogPostDetails, enter ")
            Call Main.testpoint("blog -- getFormBlogPostDetails, allowCaptcha=[" & allowCaptcha & "]")
            s = s & vbCrLf & Main.GetFormStart
            '
            ' setup form key
            '
            formKey = Main.EncodeKeyNumber(Main.VisitID, Now())
            s = s & vbCrLf & Main.GetFormInputHidden("FormKey", formKey)
            'QSBack = Main.RefreshQueryString()
            'QSBack = ModifyQueryString(QSBack, RequestNameBlogEntryID, "", True)
            'QSBack = ModifyQueryString(QSBack, RequestNameFormID, FormBlogPostList)
            s = s & cr & "<div class=""aoBlogHeaderLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
            '
            ' Print the Blog Entry
            '
            CommentCnt = 0
            CS = Main.OpenCSContentRecord(cnBlogEntries, EntryID)
            If Not Main.csok(CS) Then
                s = s & cr & "<div class=""aoBlogProblem"">Sorry, the blog post you selected is not currently available</div>"
            Else
                EntryID = Main.GetCSInteger(CS, "ID")
                AuthorMemberID = Main.GetCSInteger(CS, "AuthorMemberID")
                If AuthorMemberID = 0 Then
                    AuthorMemberID = Main.GetCSInteger(CS, "createdBy")
                End If
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
                Call Main.AddLinkAlias(EntryName, Main.renderedPageId, qs)
                Call Main.SetCS(CS, "viewings", 1 + Main.GetCSInteger(CS, "viewings"))
                imageDisplayTypeId = Main.GetCSInteger(CS, "imageDisplayTypeId")
                primaryImagePositionId = Main.GetCSInteger(CS, "primaryImagePositionId")
                articlePrimaryImagePositionId = Main.GetCSInteger(CS, "articlePrimaryImagePositionId")
                s = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, True, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                EntryPtr = EntryPtr + 1
                '
                blogId = Main.GetCSInteger(CS, "BlogID")
                '
            End If
            Call Main.CloseCS(CS)


            ' contensive
            ' cnBlogs
            'CS = Main.OpenCSContentRecord(cnBlogs, blogId)
            '
            'allowCaptcha = Main.GetCSBoolean(CS, "recaptcha")
            '
            'Call Main.CloseCS(CS)

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
            Dim Auth As Integer
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
                            'If AllowPasswordEmail Then
                            '    qs = ModifyQueryString(qs, "auth", "1")
                            '    Copy = Copy & " <a href=""?" & qs & """> Forget your username or password?</a>"
                            'End If
                            If AllowMemberJoin Then
                                qs = ModifyQueryString(qs, "auth", "2")
                                Copy = Copy & "<div class=""aoBlogRegisterLink""><a href=""?" & qs & """>Need to Register?</a></div>"
                            End If
                            loginForm = Main.GetLoginForm()
                            loginForm = Replace(loginForm, "LoginUsernameInput", "LoginUsernameInput-BlockFocus")
                            s = s _
                    & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                    & cr & "<div class=""aoBlogLoginBox"">" _
                    & cr & "<div class=""aoBlogCommentCopy"">" & loginForm & "</div>" _
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
                    's = s & cr & "<div class=""aoBlogCommentCopy"">Verify Text</div>"
                    '
                    If allowCaptcha Then
                        s = s & cr & "<div class=""aoBlogCommentCopy"">Verify Text</div>"
                        s = s & cr & "<div class=""aoBlogCommentCopy"">" & Main.ExecuteAddon2(reCaptchaDisplayGuid, "") & "</div>"
                        Call Main.testpoint("output - reCaptchaDisplayGuid")
                    End If
                    '
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
            'QSBack = Main.RefreshQueryString()
            'QSBack = ModifyQueryString(QSBack, RequestNameBlogEntryID, "", True)
            'QSBack = ModifyQueryString(QSBack, RequestNameFormID, FormBlogPostList)
            s = s & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
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
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function ProcessFormBlogPostDetails(SourceFormID As Integer, blogId As Integer, IsBlogOwner As Boolean, ButtonValue As String, BlogName As String, BlogOwnerID As Integer, AllowAnonymous As Boolean, BlogCategoryID As Integer, autoApproveComments As Boolean, authoringGroupId As Integer, emailComment As Boolean, OptionString As String, allowCaptcha As Boolean) As Integer
            '
            Dim AuthoringMemberId As Integer
            Dim authoringGroup As String
            Dim MemberID As Integer
            Dim MemberName As String
            Dim EntryLink As String
            'Dim ButtonValue As String
            Dim EmailBody As String
            Dim EmailFromAddress As String
            Dim EntryCnt As Integer
            Dim EntryPtr As Integer
            Dim CommentCnt As Integer
            Dim CommentPtr As Integer
            Dim Suffix As String
            Dim CS As Integer
            Dim Copy As String
            Dim CSP As Integer
            Dim EmailString As String
            Dim CommentInfo As String
            Dim CommentID As Integer
            Dim RandomSerialNumber As Integer
            Dim EntryName As String
            Dim CommentTitle As String
            Dim AuthorName As String
            Dim AuthorEmail As String
            Dim CommentCopy As String
            Dim CommentDate As String
            Dim Approved As Boolean
            Dim SN As String
            Dim EntryID As Integer
            Dim formKey As String
            Dim optionStr As String
            Dim captchaResponse As String
            'Dim allowCaptcha As Boolean
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
                    If allowCaptcha Then
                        '
                        ' Process recaptcha
                        '
                        optionStr = "Challenge=" + Main.GetStreamText("recaptcha_challenge_field")
                        optionStr = optionStr & "&Response=" + Main.GetStreamText("recaptcha_response_field")
                        captchaResponse = Main.ExecuteAddon2(reCaptchaProcessGuid, optionStr)
                        Call Main.testpoint("output - reCaptchaProcessGuid, result=" & captchaResponse)
                        If captchaResponse <> "" Then
                            Call Main.AddUserError("The verify text you entered did not match correctly. Please try again.")
                            Call AppendLog("Trace", "testpoint1")
                        End If
                    End If
                    '
                    ' Process comment post
                    '
                    RetryCommentPost = True
                    formKey = Main.GetStreamText("formkey")
                    'formKey = Main.DecodeKeyNumber(formKey)
                    Copy = Main.GetStreamText(RequestNameCommentCopy)
                    If Copy <> "" Then
                        Call Main.testpoint("blog -- adding comment [" & Copy & "]")
                        'If (Main.VisitID <> kmaEncodeInteger(Main.DecodeKeyNumber(formKey))) Then
                        '    Call Main.AddUserError("<p>This comment has already been accepted.</p>")
                        'End If
                        CSP = Main.OpenCSContent(cnBlogComments, "formkey=" & KmaEncodeSQLText(formKey))
                        If Main.IsCSOK(CSP) Then
                            Call Main.AddUserError("<p>This comment has already been accepted.</p>")
                            RetryCommentPost = False
                            Call AppendLog("Trace", "testpoint2")
                        End If
                        Call Main.CloseCS(CSP)
                        If Main.IsUserError() Then
                            Call Main.testpoint("blog -- user error")
                            Call AppendLog("Trace", "testpoint3")
                        Else
                            Call Main.testpoint("blog -- adding comment, no user error")
                            EntryID = Main.GetStreamInteger(RequestNameBlogEntryID)
                            CSP = Main.InsertCSRecord(cnBlogComments)
                            If AllowAnonymous And (Not Main.IsAuthenticated) Then
                                MemberID = 0
                                MemberName = AnonymousMemberName
                            Else
                                MemberID = Main.MemberID
                                MemberName = Main.MemberName
                            End If
                            If Main.csok(CSP) Then
                                Call Main.SetCS(CSP, "BlogID", blogId)
                                Call Main.SetCS(CSP, "active", 1)
                                'Call Main.SetCS(CSP, "Name", Main.GetStreamText(RequestNameAuthorName))
                                'Call Main.SetCS(CSP, "AuthorEmail", Main.GetStreamText(RequestNameAuthorEmail))
                                Call Main.SetCS(CSP, "Name", Main.GetStreamText(RequestNameCommentTitle))
                                '**** should have been copytext, not copy
                                Call Main.SetCS(CSP, "Copytext", Copy)
                                'Call Main.SetCS(CSP, "Copy", Copy)
                                Call Main.SetCS(CSP, "EntryID", EntryID)
                                'Call Main.SetCS(CSP, "AuthorMemberID", Main.MemberID)
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
                                            Call Main.DeleteContentRecords("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blogId & ")")
                                        ElseIf Main.GetStreamBoolean("Approve" & Suffix) And Not Main.GetStreamBoolean("Approved" & Suffix) Then
                                            '
                                            ' Approve Comment
                                            '
                                            CS = Main.OpenCSContent("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blogId & ")")
                                            If Main.IsCSOK(CS) Then
                                                Call Main.SetCS(CS, "Approved", True)
                                            End If
                                            Call Main.CloseCS(CS)
                                        ElseIf Not Main.GetStreamBoolean("Approve" & Suffix) And Main.GetStreamBoolean("Approved" & Suffix) Then
                                            '
                                            ' Unapprove comment
                                            '
                                            CS = Main.OpenCSContent("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blogId & ")")
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
                    ' CommentInfo = "Submitted at " & Now() & " by " & AuthorName
                    '  EmailString = "<br>"
                    ' EmailString = EmailString & CommentInfo & "<br>"
                    'EmailString = EmailString & "<a href=""http://" & Main.ServerHost & Main.ServerAppRootPath & "admin/index.asp?cid=" & Main.GetContentID(cnBlogEntries) & "&id=" & CommentID & "&af=4"">click here</a>"
                    ' Call Main.SendSystemEmail(SystemEmailCommentNotification, EmailString)
                    'test good
                End If
                Call Main.SetVisitProperty(SNBlogCommentName, "")
            End If
            '
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetBlogCommentCell(IsBlogOwner As Boolean, DateAdded As Date, Approved As Boolean, CommentName As String, CommentCopy As String, CommentID As Integer, EntryPtr As Integer, CommentPtr As Integer, AuthorMemberID As Integer, EntryID As Integer, IsSearchListing As Boolean, EntryName As String) As String
            '
            Dim Copy As String
            Dim s As String
            Dim RowCopy As String
            Dim RequestSuffix As String
            Dim qs As String
            Dim userPtr As Integer
            Dim authorMemberName As String
            '
            userPtr = getUserPtr(AuthorMemberID)
            authorMemberName = ""
            If userPtr > 0 Then
                authorMemberName = users(userPtr).Name
            End If
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
            If authorMemberName <> "" Then
                RowCopy = RowCopy & "by " & kmaEncodeHTML(authorMemberName)
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
        End Function
        '
        Private Function GetBlogEntryCell(EntryPtr As Integer, IsBlogOwner As Boolean, EntryID As Integer, EntryName As String, EntryCopy As String, DateAdded As Date, DisplayFullEntry As Boolean, IsSearchListing As Boolean, Return_CommentCnt As Integer, allowComments As Boolean, PodcastMediaLink As String, PodcastSize As String, entryEditLink As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, BlogTagList As String, imageDisplayTypeId As Integer, primaryImagePositionId As Integer, articlePrimaryImagePositionId As Integer, blogListQs As String, AuthorMemberID As Integer) As String
            '
            Dim hint As String
            Dim Link As String
            Dim Ptr As Integer
            Dim Tags() As String
            Dim TagListRow As String
            Dim imageDescription As String
            Dim imageName As String
            Dim cnt As Integer
            Dim c As String
            Dim ThumbnailFilename As String
            Dim imageFilename As String
            Dim SQL As String
            Dim imageIDList As String
            Dim ImageID() As String
            Dim OptionString As String
            Dim CSCount As Integer
            Dim CommentCount As Integer
            Dim CommentLine As String
            Dim ToolLine As String
            Dim qs As String
            Dim CommentCopy As String
            Dim CommentName As String
            Dim Approved As Boolean
            Dim CommentID As Integer
            Dim CommentPtr As Integer
            Dim Divider As String
            Dim CS As Integer
            Dim s As String
            Dim RowCopy As String
            Dim Criteria As String
            Dim OverviewCopy As String
            Dim EntryLink As String
            Dim CSImages As Integer
            Dim userPtr As Integer
            Dim authorMemberName As String
            '
            hint = "enter"
            authorMemberName = ""
            userPtr = getUserPtr(AuthorMemberID)
            If userPtr > 0 Then
                authorMemberName = users(userPtr).Name
            End If
            qs = blogListQs
            qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
            qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
            If Main.version > "4.1.160" Then
                EntryLink = getLinkAlias("?" & qs)
            End If
            If EntryLink = "" Then
                qs = blogListQs
                qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                EntryLink = "?" & qs
            End If
            '
            ' Get ImageID List
            '
            hint = hint & ",1"
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
            hint = hint & ",2"
            If imageIDList <> "" Then
                imageIDList = Mid(imageIDList, 2)
                ImageID = Split(imageIDList, ",")
                Call GetBlogImage(kmaEncodeInteger(ImageID(0)), ThumbnailImageWidth, 0, ThumbnailFilename, imageFilename, imageDescription, imageName)
            End If
            '
            s = ""
            hint = hint & ",3"
            If DisplayFullEntry Then
                ' added page title with meta content in dotnet blog wrapper
                'Call Main.AddPageTitle(EntryName)
                s = s & vbCrLf & entryEditLink & "<h2 class=""aoBlogEntryName"">" & EntryName & "</h2>"
                s = s & cr & "<div class=""aoBlogEntryCopy"">"
                If ThumbnailFilename <> "" Then
                    Select Case articlePrimaryImagePositionId
                        Case 2
                            '
                            ' align right
                            '
                            s = s & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & Main.serverFilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;"">"
                        Case 3
                            '
                            ' align left
                            '
                            s = s & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & Main.serverFilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;"">"
                        Case 4
                            '
                            ' hide
                            '
                        Case Else
                            '
                            ' 1 and none align per stylesheet
                            '
                            s = s & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & Main.serverFilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;"">"
                    End Select
                End If
                hint = hint & ",4"
                s = s & EntryCopy & "</div>"
                qs = blogListQs
                qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                qs = ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor)
                c = ""
                If (imageDisplayTypeId = imageDisplayTypeList) And (imageIDList <> "") Then
                    c = ""
                    For cnt = 0 To UBound(ImageID)
                        Call GetBlogImage(kmaEncodeInteger(ImageID(cnt)), 0, ImageWidthMax, ThumbnailFilename, imageFilename, imageDescription, imageName)
                        If imageFilename <> "" Then
                            c = c _
                    & cr & "<div class=""aoBlogEntryImageContainer"">" _
                    & cr & "<img alt=""" & imageName & """ title=""" & imageName & """  src=""" & Main.serverFilePath & imageFilename & """>"
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
                hint = hint & ",5"
                If BlogTagList <> "" Then
                    BlogTagList = Replace(BlogTagList, ",", vbCrLf)
                    Tags = Split(BlogTagList, vbCrLf)
                    BlogTagList = ""
                    Dim SQS As String
                    SQS = ModifyQueryString(blogListQs, RequestNameFormID, FormBlogSearch, True)
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
                hint = hint & ",6"
                s = s & vbCrLf & entryEditLink & "<h2 class=""aoBlogEntryName""><a href=""" & EntryLink & """>" & EntryName & "</a></h2>"
                s = s & cr & "<div class=""aoBlogEntryCopy"">"
                If ThumbnailFilename <> "" Then
                    Select Case primaryImagePositionId
                        Case 2
                            '
                            ' align right
                            '
                            s = s & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & Main.serverFilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;""></a>"
                        Case 3
                            '
                            ' align left
                            '
                            s = s & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & Main.serverFilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;""></a>"
                        Case 4
                            '
                            ' hide
                            '
                        Case Else
                            '
                            ' 1 and none align per stylesheet
                            '
                            s = s & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & Main.serverFilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;""></a>"
                    End Select
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
            hint = hint & ",7"
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
            hint = hint & ",8"
            RowCopy = ""
            If authorMemberName <> "" Then
                RowCopy = RowCopy & "By " & authorMemberName
                If DateAdded <> CDate(0) Then
                    RowCopy = RowCopy & " | " & DateAdded
                End If
            Else
                If DateAdded <> CDate(0) Then
                    RowCopy = RowCopy & DateAdded
                End If
            End If
            hint = hint & ",9"
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
                    qs = blogListQs
                    qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                    qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                    If CommentCount = 0 Then
                        RowCopy = RowCopy & " | " & "<a href=""" & EntryLink & """>Comment</a>"
                    Else
                        RowCopy = RowCopy & " | " & "<a href=""" & EntryLink & """>Comments</a>&nbsp;(" & CommentCount & ")"
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
            hint = hint & ",10"
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
                    '            QS = blogListQs
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
                        qs = blogListQs
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
                    hint = hint & ",11"
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
                    If Main.csok(CS) Then
                        Divider = "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                        s = s & cr & "<div class=""aoBlogCommentHeader"">Comments</div>"
                        s = s & vbCrLf & Divider
                        CommentPtr = 0
                        Do While Main.csok(CS)
                            CommentID = Main.GetCSInteger(CS, "ID")
                            AuthorMemberID = Main.GetCSInteger(CS, "createdby")
                            DateAdded = Main.GetCSDate(CS, "DateAdded")
                            Approved = Main.GetCSBoolean(CS, "Approved")
                            CommentName = Main.GetCSText(CS, "Name")
                            CommentCopy = Main.GetCSText(CS, "Copytext")
                            If CommentCopy = "" Then
                                CommentCopy = Main.GetCSText(CS, "Copy")
                            End If
                            s = s & GetBlogCommentCell(IsBlogOwner, DateAdded, Approved, CommentName, CommentCopy, CommentID, EntryPtr, CommentPtr, AuthorMemberID, EntryID, False, EntryName)
                            s = s & vbCrLf & Divider
                            CommentPtr = CommentPtr + 1
                            Call Main.NextCSRecord(CS)
                        Loop
                    End If
                    Call Main.CloseCS(CS)
                End If

            End If
            '
            hint = hint & ",12"
            If ToolLine <> "" Then
                s = s & cr & "<div class=""aoBlogToolLink"">" & ToolLine & "</div>"
            End If
            s = s & vbCrLf & Main.GetFormInputHidden("CommentCnt" & EntryPtr, CommentPtr)
            '
            Return_CommentCnt = CommentPtr
            GetBlogEntryCell = s
            '
        End Function
        '
        '
        '
        Private Function AddMonth(StartDate As Date, Months As Integer) As Date
            '
            Dim Y As Integer
            Dim M As Integer
            '
            M = Month(StartDate) + Months
            Y = Year(StartDate)
            If M > 12 Then
                Y = Y + Int(M / 12)
                M = ((M - 1) Mod 12) + 1
            End If
            AddMonth = CDate(M & "/1/" & Y)
            '
        End Function

        Private Function filterCopy(rawCopy As String, MaxLength As Integer) As String

            Dim Copy As String
            Dim objHTML As New kmaHTML.DecodeClass

            Copy = objHTML.Decode(rawCopy)
            If Len(Copy) > MaxLength Then
                Copy = Left(Copy, MaxLength)
                Copy = Copy & "..."
            End If
            filterCopy = Copy

        End Function
        '
        '========================================================================
        '   Update the Blog Feed
        '       run on every post
        '========================================================================
        '
        Private Sub UpdateBlogFeed(blogId As Integer, RSSFeedId As Integer, blogListLink As String)
            '
            Dim RSSTitle As String
            Dim EntryCopy As String
            Dim EntryLink As String
            Dim EntryID As Integer
            Dim CSPost As Integer
            Dim qs As String
            Dim EntryName As String
            Dim CS As Integer
            Dim RuleIDs() As Integer
            Dim RuleBlogPostIDs() As Integer
            Dim RuleCnt As Integer
            Dim RulePtr As Integer
            Dim RuleSize As Integer
            Dim BlogPostID As Integer
            Dim CSRule As Integer
            Dim SQL As String
            Dim AdminURL As String
            Dim pageId As Integer
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
        & " where (p.blogid=" & blogId & ")" _
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
                        RSSTitle = Trim(EntryName)
                        If RSSTitle = "" Then
                            RSSTitle = "Blog Post " & EntryID
                        End If
                        Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
                        '
                        qs = ""
                        qs = ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                        qs = ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                        EntryLink = Main.GetLinkAliasByPageID(Main.renderedPageId, qs, blogListLink & "?" & qs)
                        If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                            Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                        End If
                        Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
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
        End Sub
        '
        '========================================================================
        '   Verify the RSSFeed and return the ID and the Link
        '       run when a new blog is created
        '       run on every post
        '========================================================================
        '
        Private Sub VerifyFeedReturnArgs(blogId As Integer, blogListLink As String, Return_RSSFeedID As Integer, Return_RSSFeedName As String, Return_RSSFeedFilename As String)
            '
            Dim qs As String
            Dim CSBlog As Integer
            Dim BlogName As String
            Dim blogDescription As String
            Dim CSFeed As Integer
            Dim rssLink As String
            '
            CSBlog = Csv.OpenCSContent(cnBlogs, "ID=" & blogId)
            If Csv.IsCSOK(CSBlog) Then
                BlogName = Csv.GetCSText(CSBlog, "name")
                blogDescription = Trim(Csv.GetCS(CSBlog, "copy"))
                Return_RSSFeedID = Csv.GetCSInteger(CSBlog, "RSSFeedID")
                If Trim(BlogName) = "" Then
                    Return_RSSFeedName = "Feed for Blog " & blogId
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
                    If Trim(Main.GetCS(CSFeed, "link")) = "" Then
                        rssLink = Main.serverProtocol & Main.ServerHost & blogListLink
                        Call Main.SetCS(CSFeed, "link", rssLink)
                    End If
                    'Return_BlogRootLink = Trim(Main.GetCS(CSFeed, "link"))
                    'If Return_BlogRootLink = "" Then
                    '    '
                    '    ' set blog link to current link without forms/categories
                    '    '   exclude admin
                    '    '   exclude a post
                    '    '
                    '    Return_BlogRootLink = Main.ServerLink
                    '    If (InStr(1, Return_BlogRootLink, "admin", vbTextCompare) = 0) And (Main.ServerForm = "") Then
                    '        Return_BlogRootLink = ModifyQueryString(Return_BlogRootLink, RequestNameFormID, "", False)
                    '        Return_BlogRootLink = ModifyQueryString(Return_BlogRootLink, RequestNameSourceFormID, "", False)
                    '        Return_BlogRootLink = ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryID, "", False)
                    '        Return_BlogRootLink = ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryIDSet, "", False)
                    '        Call Main.SetCS(CSFeed, "link", Return_BlogRootLink)
                    '    End If
                    'End If
                End If
                Call Main.CloseCS(CSFeed)
            End If
            Call Csv.CloseCS(CSBlog)
            '
        End Sub
        '
        '
        '
        Private Function GetBlogImage(BlogImageID As Integer, ThumbnailImageWidth As Integer, ImageWidthMax As Integer, ByRef Return_ThumbnailFilename As String, ByRef Return_ImageFilename As String, ByRef Return_ImageDescription As String, ByRef Return_Imagename As String) As String
            '
            Dim sf As Object
            Dim FilenameNoExtension As String
            Dim FilenameExtension As String
            Dim Pos As Integer
            Dim CS As Integer
            Dim Filename As String
            Dim AltSizeList As String
            Dim Sizes() As String
            Dim Size As String
            Dim Ptr As Integer
            Dim Dims() As String
            Dim ThumbNailSize As String
            Dim ImageSize As String
            Dim UpdateRecord As Boolean
            Dim DimWidth As Integer
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

                            sf.LoadFromFile(Main.PhysicalFilePath & Filename)
                            If Err.Number <> 0 Then
                                On Error GoTo ErrorTrap
                                Err.Clear()
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

                            sf.LoadFromFile(Main.PhysicalFilePath & Filename)
                            If Err.Number <> 0 Then
                                On Error GoTo ErrorTrap
                                Err.Clear()
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
            Call handleClassError("Blogs", "GetBlogImageFilename", Err.Number, Err.Source, Err.Description, True, False, Main.ServerLink)
        End Function
        '
        '
        '
        Private Function getLinkAlias(cp As CPBaseClass, sourceLink As String) As String
            '
            Dim returnLink As String
            Dim Pos As Integer
            Dim pageQs() As String
            Dim nameValues() As String
            Dim cnt As Integer
            Dim Ptr As Integer
            Dim Link As String
            Dim pageId As Integer
            Dim NameValue As String
            Dim qs As String
            '
            'Call Main.SaveVirtualFile("blog.log", "getLinkAlias(" & sourceLink & ")")
            '
            returnLink = sourceLink
            If cp.utils.EncodeBoolean(cp.Site.GetProperty("allowLinkAlias", "1")) Then
                Link = sourceLink
                '
                pageQs = Split(LCase(Link), "?")
                If UBound(pageQs) > 0 Then
                    nameValues = Split(pageQs(1), "&")
                    cnt = UBound(nameValues) + 1
                    If UBound(nameValues) < 0 Then
                    Else
                        qs = ""
                        For Ptr = 0 To cnt - 1
                            NameValue = nameValues(Ptr)
                            If pageId = 0 Then
                                If Mid(NameValue, 1, 4) = "bid=" Then
                                    pageId = cp.Utils.EncodeInteger(Mid(NameValue, 5))
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
                            End If
                            returnLink = cp.Content.GetLinkAliasByPageID(pageId, qs, sourceLink)
                        End If
                    End If
                End If
            End If
            getLinkAlias = returnLink
            '
        End Function
        '
        '
        '
        Private Function getUserPtr(userid As Integer) As Integer
            '
            Dim userPtr As Integer
            Dim CS As Integer
            '
            userPtr = -1
            If userCnt > 0 Then
                For userPtr = 0 To userCnt - 1
                    If users(userPtr).Id = userid Then
                    End If
                Next
            End If
            If userPtr >= userCnt Then
                CS = Main.OpenCSContent("people", "id=" & userid)
                If Main.IsCSOK(CS) Then
                    userPtr = userCnt
                    userCnt = userCnt + 1
                    ReDim Preserve users(userCnt)
                    users(userPtr).Id = userid
                    users(userPtr).Name = Main.GetCSText(CS, "name")
                    users(userPtr).authorInfoLink = ""
                    If allowAuthorInfoLink Then
                        users(userPtr).authorInfoLink = Main.GetCSText(CS, "authorInfoLink")
                    End If
                End If
                Call Main.CloseCS(CS)
            End If
            getUserPtr = userPtr
            '
        End Function
    End Class
End Namespace
