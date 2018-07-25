
Imports System
Imports System.Collections.Generic
Imports System.Linq
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
            Dim BlogName As String = ""
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
            Dim cs1 As CPCSBaseClass = cp.CSNew()
            'CS = cs1.Open(cnBlogs, "(name=" & cp.Utils.EncodeQueryString(BlogName) & ")", "ID")
            Dim blogList As List(Of Models.blogModel) = Models.blogModel.createList(cp, "(name=" & cp.Utils.EncodeQueryString(BlogName) & ")", "ID")
            If (blogList.Count > 0) Then
                Dim blog As Models.blogModel = blogList.First
                blogId = blog.id
                RSSFeedId = blog.RSSFeedID
                BlogOwnerID = blog.OwnerMemberID
                authoringGroupId = blog.AuthoringGroupID
                ignoreLegacyInstanceOptions = blog.ignoreLegacyInstanceOptions
                AllowAnonymous = blog.AllowAnonymous
                autoApproveComments = blog.autoApproveComments
                emailComment = blog.emailComment
                AllowCategories = blog.AllowCategories
                PostsToDisplay = blog.PostsToDisplay
                OverviewLength = blog.OverviewLength
                ThumbnailImageWidth = blog.ThumbnailImageWidth
                ImageWidthMax = blog.ImageWidthMax
                blogDescription = blog.Copy
                blogCaption = blog.Caption
                allowRecaptcha = blog.recaptcha

            End If

            If blogId = 0 Then
                '
                ' Create New Blog
                '

                IsContentManager = cp.User.IsContentManager("Page Content")
                If IsContentManager Then
                    '
                    ' BIG assumption - First hit by a content manager for this page is the author
                    '
                    Dim blog As Models.blogModel = blogList.First

                    blogId = blog.id
                    BlogOwnerID = cp.User.Id
                    IsBlogOwner = True
                    blogCaption = BlogName
                    '
                    blog = Models.blogModel.add(cp)
                    blog.name = BlogName
                    blog.Caption = blogCaption
                    blog.OwnerMemberID = BlogOwnerID
                    blog.RSSFeedID = RSSFeedId
                    blog.AuthoringGroupID = authoringGroupId
                    blog.ignoreLegacyInstanceOptions = ignoreLegacyInstanceOptions
                    blog.AllowAnonymous = AllowAnonymous
                    blog.autoApproveComments = autoApproveComments
                    blog.AllowCategories = AllowCategories
                    blog.PostsToDisplay = PostsToDisplay
                    blog.OverviewLength = OverviewLength
                    blog.ThumbnailImageWidth = ThumbnailImageWidth
                    blog.ImageWidthMax = ImageWidthMax
                    blog.save(cp)


                    '
                End If

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
                'CS = Main.InsertCSRecord(cnBlogEntries)
                Dim blogEntry As Models.BlogEntryModel = Models.BlogEntryModel.add(cp)
                If (blogEntry IsNot Nothing) Then

                    blogEntry.id = BlogPostID
                    blogEntry.BlogID = blogId
                    blogEntry.name = Title

                    '
                    RSSTitle = Trim(Title)
                    If RSSTitle = "" Then
                        RSSTitle = "Blog Post " & EntryID
                    End If
                    blogEntry.RSSTitle = RSSTitle

                    '
                    NewPostCopy = cp.File.ReadVirtual("blogs/DefaultPostCopy.txt")
                    If NewPostCopy = "" Then
                        NewPostCopy = DefaultPostCopy
                    End If
                    blogEntry.Copy = NewPostCopy

                    '

                    qs = ""
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                    Call cp.Site.addLinkAlias(Title, cp.Doc.PageId, qs)
                    'Dim LinkAlias As Models.LinkAliasesModel = Models.LinkAliasesModel.add(cp)
                    Dim LinkAlias As List(Of Models.LinkAliasesModel) = Models.LinkAliasesModel.createList(cp, "(pageid=" & cp.Doc.PageId & ")and(QueryStringSuffix=" & cp.Db.EncodeSQLText(qs) & ")")
                    If (LinkAlias.Count > 0) Then
                        EntryLink = LinkAlias.First.name
                    End If
                    blogEntry.RSSDescription = filterCopy(NewPostCopy, 150)
                    blogEntry.save(cp)
                End If
                '
                ' Add this new default post to the new feed
                '
                Dim RSSFeedBlogRules As Models.RSSFeedBlogRuleModel = Models.RSSFeedBlogRuleModel.add(cp)
                If (RSSFeedBlogRules IsNot Nothing) Then
                    RSSFeedBlogRules.RSSFeedID = RSSFeedId
                    RSSFeedBlogRules.BlogPostID = BlogPostID
                    RSSFeedBlogRules.name = "RSS Feed [" & RSSFeedName & "], Blog Post [" & Title & "]"

                End If
                'CSRule = Csv.InsertCSRecord(cnRSSFeedBlogRules, 0)
                '    If Csv.IsCSOK(CSRule) Then
                '        Call Csv.SetCS(CSRule, "name", "RSS Feed [" & RSSFeedName & "], Blog Post [" & Title & "]")
                '        Call Csv.SetCS(CSRule, "RSSFeedID", RSSFeedId)
                '        Call Csv.SetCS(CSRule, "BlogPostID", BlogPostID)
                '    End If
                '    Call Csv.CloseCS(CSRule)
                '    '
                '    If Main.ContentServerVersion >= "4.1.098" Then
                '        Call Csv.ExecuteAddonAsProcess(RSSProcessAddonGuid)
                '    End If

            End If
            'End If
            IsBlogOwner = (cp.User.IsAuthenticated And (cp.User.Id = BlogOwnerID))
            If Not IsBlogOwner Then
                IsBlogOwner = IsBlogOwner Or cp.User.IsAdmin()
                If Not IsBlogOwner Then
                    If authoringGroupId <> 0 Then
                        authoringGroup = cp.Content.GetRecordName("Groups", authoringGroupId)
                        If authoringGroup <> "" Then
                            IsBlogOwner = IsBlogOwner Or cp.User.IsInGroup(authoringGroup, cp.User.Id)
                        End If
                    End If
                End If
            End If
            '
            ' handle legacy instance option migration to the blog record
            '
            If Not ignoreLegacyInstanceOptions Then
                AllowAnonymous = cp.Utils.EncodeBoolean(cp.Doc.GetText("Allow Anonymous Comments"))
                autoApproveComments = cp.Utils.EncodeBoolean(cp.Doc.GetText("Auto-Approve New Comments"))
                AllowCategories = cp.Utils.EncodeBoolean(cp.Doc.GetText("Allow Categories"))
                PostsToDisplay = cp.Utils.EncodeInteger(cp.Doc.GetText("Number of entries to display"))
                OverviewLength = cp.Utils.EncodeInteger(cp.Doc.GetText("Overview Character Length"))
                ThumbnailImageWidth = cp.Utils.EncodeInteger(cp.Doc.GetText("Thumbnail Image Width"))
                If ThumbnailImageWidth = 0 Then
                    ThumbnailImageWidth = 150
                End If
                ImageWidthMax = cp.Utils.EncodeInteger(cp.Doc.GetText("Image Width Max"))
                If ImageWidthMax = 0 Then
                    ImageWidthMax = 400
                End If
                If OverviewLength = 0 Then
                    OverviewLength = 150
                End If
                If PostsToDisplay = 0 Then
                    PostsToDisplay = 5
                End If
                Dim legacyblogList As List(Of Models.blogModel) = Models.blogModel.createList(cp, "(id=" & cp.Utils.EncodeNumber(blogId) & ")", "ID")

                If (legacyblogList.Count > 0) Then
                    Dim legacyblog As Models.blogModel = blogList.First
                    legacyblog = Models.blogModel.add(cp)
                    legacyblog.ignoreLegacyInstanceOptions = True
                    legacyblog.AllowAnonymous = AllowAnonymous
                    legacyblog.autoApproveComments = autoApproveComments
                    legacyblog.AllowCategories = AllowCategories
                    legacyblog.PostsToDisplay = PostsToDisplay
                    legacyblog.OverviewLength = OverviewLength
                    legacyblog.ThumbnailImageWidth = ThumbnailImageWidth
                    legacyblog.ImageWidthMax = ImageWidthMax
          
                End If

                'CS = Main.OpenCSContent(cnBlogs, "(id=" & cp.Utils.EncodeNumber(blogId) & ")", "ID")
                'If Main.IsCSOK(CS) Then
                '    Call Main.SetCS(CS, "ignoreLegacyInstanceOptions", True)
                '    Call Main.SetCS(CS, "AllowAnonymous", AllowAnonymous)
                '    Call Main.SetCS(CS, "autoApproveComments", autoApproveComments)
                '    Call Main.SetCS(CS, "AllowCategories", AllowCategories)
                '    Call Main.SetCS(CS, "PostsToDisplay", PostsToDisplay)
                '    Call Main.SetCS(CS, "OverviewLength", OverviewLength)
                '    Call Main.SetCS(CS, "ThumbnailImageWidth", ThumbnailImageWidth)
                '    Call Main.SetCS(CS, "ImageWidthMax", ImageWidthMax)
                'End If
                'Call Main.CloseCS(CS)
            End If
            If AllowCategories Then
                If (cp.Doc.GetText(RequestNameBlogCategoryIDSet) <> "") Then
                    BlogCategoryID = cp.Doc.GetInteger(RequestNameBlogCategoryIDSet)
                Else
                    BlogCategoryID = cp.Doc.GetInteger(RequestNameBlogCategoryID)
                End If
                'If Main.InStream(RequestNameBlogCategoryIDSet) Then

                '    BlogCategoryID = cp.Doc.GetInteger(RequestNameBlogCategoryIDSet)
                'Else
                '    BlogCategoryID = cp.Doc.GetInteger(RequestNameBlogCategoryID)
                'End If
                Call cp.Doc.AddRefreshQueryString(RequestNameBlogCategoryID, BlogCategoryID)
                ' Call Main.AddRefreshQueryString(RequestNameBlogCategoryID, BlogCategoryID)
            End If
            '
            'Call Main.testpoint("Blogs.GetContent, Main.IsAuthenticated=" & Main.IsAuthenticated)
            'Call Main.testpoint("Blogs.GetContent, Main.MemberID=" & cp.User.Id)
            'Call Main.testpoint("Blogs.GetContent, BlogOwnerID=" & BlogOwnerID)
            'Call Main.testpoint("Blogs.GetContent, IsBlogOwner=" & IsBlogOwner)
            If IsBlogOwner And (blogId = 0) Then
                '
                ' Blog record was not, or can not be created
                '
                s = s & cp.Html.adminHint("This blog has not been configured. Please edit this page and edit the properties for the blog Add-on")
            Else
                '
                ' Get the Feed Args
                '
                Dim RSSFeedModelList As List(Of Models.RSSFeedModel) = Models.RSSFeedModel.createList(cp, "id=" & RSSFeedId)
                Dim RSSFeed As Models.RSSFeedModel = Models.RSSFeedModel.add(cp)
                If cp.CSNew.OK() Then
                    RSSFeedName = RSSFeed.name
                    RSSFeedFilename = RSSFeed.RSSFilename
                    'BlogRootLink = Main.GetCS(CSFeed, "Link")
                End If

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
                ButtonValue = cp.Doc.GetText("button")
                FormID = cp.Doc.GetInteger(RequestNameFormID)
                SourceFormID = cp.Doc.GetInteger(RequestNameSourceFormID)
                KeywordList = cp.Doc.GetText(RequestNameKeywordList)
                DateSearchText = cp.Doc.GetText(RequestNameDateSearch)
                'BlogTitle = cp.Doc.getText(RequestNameBlogTitle)
                'BlogCopy = cp.Doc.getText(RequestNameBlogCopy)
                ArchiveMonth = cp.Doc.GetInteger(RequestNameArchiveMonth)
                ArchiveYear = cp.Doc.GetInteger(RequestNameArchiveYear)
                EntryID = cp.Doc.GetInteger(RequestNameBlogEntryID)
                '
                'WorkingQueryString = cp.Doc.RefreshQueryString
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
                    FormID = ProcessForm(cp, SourceFormID, blogId, IsBlogOwner, EntryID, ButtonValue, BlogName, BlogOwnerID, AllowAnonymous, AllowCategories, BlogCategoryID, RSSFeedId, blogListLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, autoApproveComments, authoringGroupId, emailComment, OptionString, allowRecaptcha)
                End If
                '
                ' Get Next Form
                '
                s = s & GetForm(cp, FormID, blogId, BlogName, IsBlogOwner, ArchiveMonth, ArchiveYear, EntryID, KeywordList, ButtonValue, DateSearchText, AllowAnonymous, AllowCategories, BlogCategoryID, RSSFeedName, RSSFeedFilename, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogDescription, blogCaption, RSSFeedId, blogListLink, blogListQs, allowRecaptcha)
            End If

            '
            GetContent = s
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetForm(cp As CPBaseClass, FormID As Integer, blogId As Integer, BlogName As String, IsBlogOwner As Boolean, ArchiveMonth As Integer, ArchiveYear As Integer, EntryID As Integer, KeywordList As String, ButtonValue As String, DateSearchText As String, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, RSSFeedName As String, RSSFeedFilename As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogDescription As String, blogCaption As String, RSSFeedId As Integer, blogListLink As String, blogListQs As String, allowCaptcha As Boolean) As String
            '
            Dim Stream As String
            Dim IsEditing As Boolean
            '
            'Call Main.AddRefreshQueryString(RequestNameSourceFormID, FormID)
            IsEditing = cp.User.IsAuthoring("Blogs")
            Dim Csv As Object = Nothing
            allowAuthorInfoLink = Csv.IsContentFieldSupported("people", "autoinfoLink")
            '
            Select Case FormID
                Case FormBlogPostDetails
                    Stream = Stream & GetFormBlogPostDetails(cp, blogId, EntryID, IsBlogOwner, AllowAnonymous, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs, allowCaptcha)
                Case FormBlogArchiveDateList
                    Stream = Stream & GetFormBlogArchiveDateList(cp, blogId, BlogName, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                Case FormBlogArchivedBlogs
                    Stream = Stream & GetFormBlogArchivedBlogs(cp, blogId, BlogName, ArchiveMonth, ArchiveYear, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                Case FormBlogEntryEditor
                    Stream = Stream & GetFormBlogPost(blogId, BlogName, IsBlogOwner, EntryID, AllowCategories, BlogCategoryID, blogListLink)
                Case FormBlogSearch
                    Stream = Stream & GetFormBlogSearch(cp, blogId, BlogName, IsBlogOwner, KeywordList, ButtonValue, DateSearchText, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                Case Else
                    If EntryID <> 0 Then
                        '
                        ' Go to details page
                        '
                        FormID = FormBlogPostDetails
                        Stream = Stream & GetFormBlogPostDetails(cp, blogId, EntryID, IsBlogOwner, AllowAnonymous, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs, allowCaptcha)
                    Else
                        '
                        ' list all the entries
                        '
                        FormID = FormBlogPostList
                        Stream = Stream & GetFormBlogPostList(cp, blogId, BlogName, IsBlogOwner, IsEditing, EntryID, AllowCategories, BlogCategoryID, RSSFeedName, RSSFeedFilename, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogDescription, blogCaption, RSSFeedId, blogListLink, blogListQs)
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
        Private Function ProcessForm(cp As CPBaseClass, SourceFormID As Integer, blogId As Integer, IsBlogOwner As Boolean, EntryID As Integer, ButtonValue As String, BlogName As String, BlogOwnerID As Integer, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, RSSFeedId As Integer, blogListLink As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, autoApproveComments As Boolean, authoringGroupId As Integer, emailComment As Boolean, OptionString As String, allowRecaptcha As Boolean) As Integer
            '
            If ButtonValue <> "" Then
                Select Case SourceFormID
                    Case FormBlogPostList
                        ProcessForm = FormBlogPostList
                    Case FormBlogEntryEditor
                        ProcessForm = ProcessFormBlogPost(cp, SourceFormID, blogId, EntryID, ButtonValue, BlogCategoryID, RSSFeedId, blogListLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax)
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
        Private Function GetFormBlogSearch(cp As CPBaseClass, blogId As Integer, BlogName As String, IsBlogOwner As Boolean, KeywordList As String, ButtonValue As String, DateSearchText As String, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String) As String
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
            'Dim CS As Integer
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
            'Dim CSPointer As Integer
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
            Dim GetUserError As String = ""

            '
            'Call Main.AddRefreshQueryString(RequestNameSourceFormID, FormBlogSearch)
            'Call Main.AddRefreshQueryString(RequestNameFormID, FormBlogSearch)
            Call cp.Doc.AddRefreshQueryString(RequestNameBlogEntryID, "")
            '
            s = s & vbCrLf & cp.Content.GetCopy("Blogs Search Header for " & BlogName, "<h2>" & BlogName & " Blog Search</h2>")
            '
            ' Search results
            '
            QueryTag = cp.Doc.GetText(RequestNameQueryTag)
            Button = cp.Doc.GetText("button")
            If (Button = FormButtonSearch) Or (QueryTag <> "") Then
                '
                ' Attempt to figure out the date provided
                '
                If cp.Db.EncodeSQLDate(DateSearchText) <> CDate("0") Then
                    DateSearch = cp.Db.EncodeSQLDate(DateSearchText)
                    If DateSearch < CDate("1/1/2000") Then
                        DateSearch = CDate("0")
                        Call cp.Site.ErrorReport("The date is not valid")
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
                            Subcaption = Subcaption & "'<i>" & cp.Db.EncodeSQLText(EnteredKeyWords) & "</i>'"
                            EnteredKeyWords = cp.Db.EncodeSQLText(EnteredKeyWords)
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
                If (DateSearch <> CDate("0")) Then
                    SearchMonth = Month(DateSearch)
                    SearchYear = Year(DateSearch)
                    Subcaption = Subcaption & " in " & SearchMonth & "/" & SearchYear
                    If Criteria <> "" Then
                        Criteria = Criteria & "AND"
                    End If
                    Criteria = Criteria & "(DateAdded>=" & cp.Db.EncodeSQLDate(DateSearch) & ")and(DateAdded<" & cp.Db.EncodeSQLDate(AddMonth(DateSearch, 1)) & ")"
                End If
                If (QueryTag <> "") Then
                    Subcaption = Subcaption & " tagged with '<i>" & cp.Utils.EncodeHTML(QueryTag) & "</i>'"
                    If Criteria <> "" Then
                        Criteria = Criteria & "AND"
                    End If
                    QueryTag = cp.Db.EncodeSQLText(QueryTag)
                    QueryTag = "'%" & Mid(QueryTag, 2, Len(QueryTag) - 2) & "%'"
                    Criteria = Criteria & " (taglist like " & QueryTag & ")"
                End If
                If Subcaption <> "" Then
                    Subcaption = "Search for entries and comments " & Subcaption
                End If

                If GetUserError Then
                    Subcaption = Subcaption & (cp.UserError.ToString)
                End If
                '
                Dim BlogCopyList As New List(Of Models.BlogCopyModel)
                If Criteria <> "" Then
                    BlogCopyList = Models.BlogCopyModel.createList(cp, Criteria)
                End If
                '
                ' Display the results
                '
                s = s & cr & "<div class=""aoBlogEntryCopy"">" & Subcaption & "</div>"
                '
                If (blogcopylist.count = 0) Then
                    s = s & "</br>" & "<div class=""aoBlogProblem"">There were no matches to your search</div>"
                Else
                    s = s & "</br>" & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                    For Each BlogComment In Models.BlogCopyModel.createList(cp, Criteria)
                        ParentID = cp.Doc.GetInteger("EntryID")
                        If ParentID = 0 Then
                            '
                            ' Entry
                            '
                            AuthorMemberID = cp.Doc.GetInteger("AuthorMemberID")
                            If AuthorMemberID = 0 Then
                                AuthorMemberID = cp.Doc.GetInteger("createdBy")
                            End If
                            EntryID = cp.Doc.GetInteger("ID")
                            EntryName = cp.Doc.GetText("name")
                            EntryCopy = cp.Doc.GetText("copy")
                            'EntryCopyOverview = cp.Doc.GetText(CSPointer, "copyOverview")
                            DateAdded = cp.Doc.GetDate("DateAdded")
                            allowComments = cp.Doc.GetBoolean("allowComments")
                            PodcastMediaLink = cp.Doc.GetText("PodcastMediaLink")
                            PodcastSize = cp.Doc.GetText("PodcastSize")
                            BlogTagList = cp.Doc.GetText("TagList")
                            imageDisplayTypeId = cp.Doc.GetInteger("imageDisplayTypeId")
                            primaryImagePositionId = cp.Doc.GetInteger("primaryImagePositionId")
                            articlePrimaryImagePositionId = cp.Doc.GetInteger("articlePrimaryImagePositionId")
                            s = s & GetBlogEntryCell(cp, ResultPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, True, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, "", ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                        Else
                            '
                            ' Comment
                            '
                            AuthorMemberID = cp.Doc.GetInteger("createdBy")
                            CommentID = cp.Doc.GetInteger("ID")
                            CommentName = cp.Doc.GetText("name")
                            CommentCopy = cp.Doc.GetText("copyText")
                            DateAdded = cp.Doc.GetDate("DateAdded")
                            Approved = cp.Doc.GetBoolean("Approved")
                            EntryID = cp.Doc.GetInteger("EntryID")
                            EntryName = cp.Doc.GetInteger("Blog Copy", EntryID)
                            s = s & GetBlogCommentCell(cp, IsBlogOwner, DateAdded, Approved, CommentName, CommentCopy, CommentID, ResultPtr, 0, AuthorMemberID, EntryID, True, EntryName)
                        End If
                        '
                        s = s & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                        ResultPtr = ResultPtr + 1

                    Next
                End If
                'Do While Main.csok(CSPointer)
                '    ParentID = Main.GetCSInteger(CSPointer, "EntryID")
                '    If ParentID = 0 Then
                '        '
                '        ' Entry
                '        '
                '        AuthorMemberID = Main.GetCSInteger(CSPointer, "AuthorMemberID")
                '        If AuthorMemberID = 0 Then
                '            AuthorMemberID = Main.GetCSInteger(CSPointer, "createdBy")
                '        End If
                '        EntryID = Main.GetCSInteger(CSPointer, "ID")
                '        EntryName = cp.Doc.GetText(CSPointer, "name")
                '        EntryCopy = cp.Doc.GetText(CSPointer, "copy")
                '        'EntryCopyOverview = cp.Doc.GetText(CSPointer, "copyOverview")
                '        DateAdded = Main.GetCSDate(CSPointer, "DateAdded")
                '        allowComments = cp.Site.GetBoolean(CSPointer, "allowComments")
                '        PodcastMediaLink = Main.GetCS(CSPointer, "PodcastMediaLink")
                '        PodcastSize = Main.GetCS(CSPointer, "PodcastSize")
                '        BlogTagList = Main.GetCS(CSPointer, "TagList")
                '        imageDisplayTypeId = Main.GetCSInteger(CSPointer, "imageDisplayTypeId")
                '        primaryImagePositionId = Main.GetCSInteger(CSPointer, "primaryImagePositionId")
                '        articlePrimaryImagePositionId = Main.GetCSInteger(CSPointer, "articlePrimaryImagePositionId")
                '        s = s & GetBlogEntryCell(ResultPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, True, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, "", ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                '    Else
                '        '
                '        ' Comment
                '        '
                '        AuthorMemberID = Main.GetCSInteger(CSPointer, "createdBy")
                '        CommentID = Main.GetCSInteger(CSPointer, "ID")
                '        CommentName = cp.Doc.GetText(CSPointer, "name")
                '        CommentCopy = cp.Doc.GetText(CSPointer, "copyText")
                '        DateAdded = Main.GetCSDate(CSPointer, "DateAdded")
                '        Approved = cp.Site.GetBoolean(CSPointer, "Approved")
                '        EntryID = Main.GetCSInteger(CSPointer, "EntryID")
                '        EntryName = Main.GetRecordName("Blog Copy", EntryID)
                '        s = s & GetBlogCommentCell(IsBlogOwner, DateAdded, Approved, CommentName, CommentCopy, CommentID, ResultPtr, 0, AuthorMemberID, EntryID, True, EntryName)
                '    End If
                '    '
                '    s = s & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                '    ResultPtr = ResultPtr + 1
                '    Call Main.NextCSRecord(CSPointer)
                'Loop
                'End If
                'Call Main.CloseCS(CSPointer)
                s = "" _
                        & cr & "<div class=""aoBlogSearchResultsCon"">" _
                        & cp.Html.Indent(s) _
                        & cr & "</div>"
                End If
            '
            searchForm = "" _
                    & cr & "<table width=100% border=0 cellspacing=0 cellpadding=5 class=""aoBlogSearchTable"">" _
                    & cp.Html.Indent(GetFormTableRow("Date:", cp.GetType.GetField(RequestNameDateSearch, cp.Doc.GetText(RequestNameDateSearch)).ToString & " " & "&nbsp;(mm/yyyy)"), 10).ToString _
                    & cp.Html.Indent(GetFormTableRow("Keyword(s):", cp.GetType.GetField(RequestNameKeywordList, cp.Doc.GetText(RequestNameKeywordList)).ToString) _
                    & cp.Html.Indent(GetFormTableRow("", cp.Html.Button(FormButtonSearch))) _
                    & cp.Html.Indent(GetFormTableRow2("<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>")) _
                    & cr & "</table>" _
                    & "")

            searchForm = "" _
                    & cr & "<div  class=""aoBlogSearchFormCon"">" _
                    & cp.Html.Indent(searchForm) _
                    & cr & "</div>" _
                    & ""
            s = s & searchForm

            s = "" _
                 & s = s & cp.Html.Indent(s) _
                 & s = s & cr & "<input type=""hidden"" name=""" & RequestNameSourceFormID & """ value=""" & FormBlogSearch & """>" _
                 & s = s & cr & "<input type=""hidden"" name=""" & RequestNameFormID & """ value=""" & FormBlogSearch & """>" _
                 & s = s & cr & cp.Html.Form(s)
            '

            GetFormBlogSearch = s
        End Function

        Private Function GetFormTableRow(v1 As String, v2 As String) As String
            Throw New NotImplementedException()
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogArchiveDateList(cp As CPBaseClass, blogId As Integer, BlogName As String, IsEditing As Boolean, IsBlogOwner As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String) As String
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
            Dim OpenSQL As String = ""
            '
            s = s & vbCrLf & cp.Content.GetCopy("Blogs Archives Header for " & BlogName, "<h2>" & BlogName & " Blog Archive</h2>")
            '            
            SQL = "SELECT distinct Month(DateAdded) as ArchiveMonth, year(dateadded) as ArchiveYear " _
                    & " From ccBlogCopy" _
                    & " Where (ContentControlID = " & cp.Content.GetID(cnBlogEntries) & ") And (Active <> 0)" _
                    & " AND (BlogID=" & blogId & ")" _
                    & " ORDER BY year(dateadded) desc, Month(DateAdded) desc"
            OpenSQL = cp.Db.ExecuteSQL(SQL, "Default")
            If Not OpenSQL Then
                '
                ' No archives, give them an error
                '
                s = s & cr & "<div class=""aoBlogProblem"">There are no current blog entries</div>"
                s = s & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"

            Else

                RowCnt = cp.CSNew.GetRowCount()
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
                    ArchiveMonth = cp.Doc.GetInteger("ArchiveMonth")
                    ArchiveYear = cp.Doc.GetInteger("ArchiveYear")
                    s = s & GetFormBlogArchivedBlogs(cp, blogId, BlogName, ArchiveMonth, ArchiveYear, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                Else
                    '
                    ' Display List of archive
                    '
                    qs = cp.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, FormBlogArchiveDateList)
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchivedBlogs)

                    Do While cp.CSNew.OK()
                        ArchiveMonth = cp.Doc.GetInteger("ArchiveMonth")
                        ArchiveYear = cp.Doc.GetInteger("ArchiveYear")
                        NameOfMonth = MonthName(ArchiveMonth)
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveMonth, CStr(ArchiveMonth))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveYear, CStr(ArchiveYear))
                        '
                        s = s & vbCrLf & vbTab & vbTab & "<div class=""aoBlogArchiveLink""><a href=""?" & qs & """>" & NameOfMonth & " " & ArchiveYear & "</a></div>"
                        's = s & vbCrLf & vbTab & vbTab & "<div class=""aoBlogArchiveLink""><a href=""?" & qs & RequestNameFormID & "=" & FormBlogArchivedBlogs & "&" & RequestNameArchiveMonth & "=" & ArchiveMonth & "&" & RequestNameArchiveYear & "=" & ArchiveYear & "&" & RequestNameSourceFormID & "=" & FormBlogArchiveDateList & """>" & NameOfMonth & " " & ArchiveYear & "</a></div>"

                        cp.CSNew.GoNext()
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
        Private Function GetFormBlogArchivedBlogs(cp As CPBaseClass, blogId As Integer, BlogName As String, ArchiveMonth As Integer, ArchiveYear As Integer, IsEditing As Boolean, IsBlogOwner As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String) As String
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
            IsEditing = cp.User.IsAuthoring("Blogs")
            's = s & vbCrLf & Main.GetFormStart
            '
            ' List Blog Entries
            '
            Dim BlogEntryModelList As List(Of Models.BlogEntryModel) = Models.BlogEntryModel.createList(cp, "(Month(DateAdded) = " & ArchiveMonth & ")And(year(DateAdded)=" & ArchiveYear & ")And(BlogID=" & blogId & ")", "DateAdded Desc",  , , , PostsToDisplay, PageNumber)
            Dim BlogEntry As Models.BlogEntryModel = Models.BlogEntryModel.add(cp)
            If cp.CSNew.OK() Then
                s = s & cr & "<div Class=""aoBlogProblem"">There are no blog archives For " & ArchiveMonth & "/" & ArchiveYear & "</div>"
            Else
                EntryPtr = 0
                entryEditLink = ""
                Do While cp.CSNew.OK()
                    EntryID = cp.Doc.GetInteger("ID")
                    AuthorMemberID = cp.Doc.GetInteger("AuthorMemberID")
                    If AuthorMemberID = 0 Then
                        AuthorMemberID = cp.Doc.GetInteger("createdBy")
                    End If
                    DateAdded = cp.Doc.GetText("DateAdded")
                    EntryName = cp.Doc.GetText("Name")
                    If IsEditing Then
                        entryEditLink = cp.Content.GetEditLink(EntryName, EntryID, 1, EntryName, 1) 'Main.GetCSRecordEditLink(CS)
                    End If
                    EntryCopy = cp.Doc.GetText("Copy")
                    'EntryCopyOverview = cp.Doc.GetText(CS, "copyOverview")
                    allowComments = cp.Site.GetBoolean("allowComments")
                    PodcastMediaLink = cp.Doc.GetText("PodcastMediaLink")
                    PodcastSize = cp.Doc.GetText("PodcastSize")
                    BlogTagList = cp.Doc.GetText("TagList")
                    imageDisplayTypeId = cp.Doc.GetInteger("imageDisplayTypeId")
                    primaryImagePositionId = cp.Doc.GetInteger("primaryImagePositionId")
                    articlePrimaryImagePositionId = cp.Doc.GetInteger("articlePrimaryImagePositionId")
                    s = s & GetBlogEntryCell(cp, EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                    Call cp.CSNew.GoNext()
                    s = s & cr & "<div Class=""aoBlogEntryDivider"">&nbsp;</div>"
                    EntryPtr = EntryPtr + 1
                Loop
            End If


            'CS = Main.OpenCSContent(cnBlogEntries, "(Month(DateAdded) = " & ArchiveMonth & ")And(year(DateAdded)=" & ArchiveYear & ")And(BlogID=" & blogId & ")", "DateAdded Desc", , , , , PostsToDisplay, PageNumber)
            'If Not Main.csok(CS) Then
            '    s = s & cr & "<div Class=""aoBlogProblem"">There are no blog archives For " & ArchiveMonth & "/" & ArchiveYear & "</div>"
            'Else
            '    EntryPtr = 0
            '    entryEditLink = ""
            '    Do While Main.csok(CS)
            '        EntryID = cp.Doc.GetInteger(CS, "ID")
            '        AuthorMemberID = cp.Doc.GetInteger(CS, "AuthorMemberID")
            '        If AuthorMemberID = 0 Then
            '            AuthorMemberID = cp.Doc.GetInteger(CS, "createdBy")
            '        End If
            '        DateAdded = cp.Doc.GetText(CS, "DateAdded")
            '        EntryName = cp.Doc.GetText(CS, "Name")
            '        If IsEditing Then
            '            entryEditLink = Main.GetCSRecordEditLink(CS)
            '        End If
            '        EntryCopy = cp.Doc.GetText(CS, "Copy")
            '        'EntryCopyOverview = cp.Doc.GetText(CS, "copyOverview")
            '        allowComments = cp.Site.GetBoolean(CS, "allowComments")
            '        PodcastMediaLink = Main.GetCS(CS, "PodcastMediaLink")
            '        PodcastSize = Main.GetCS(CS, "PodcastSize")
            '        BlogTagList = Main.GetCS(CS, "TagList")
            '        imageDisplayTypeId = cp.Doc.GetInteger(CS, "imageDisplayTypeId")
            '        primaryImagePositionId = cp.Doc.GetInteger(CS, "primaryImagePositionId")
            '        articlePrimaryImagePositionId = cp.Doc.GetInteger(CS, "articlePrimaryImagePositionId")
            '        s = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
            '        Call Main.NextCSRecord(CS)
            '        s = s & cr & "<div Class=""aoBlogEntryDivider"">&nbsp;</div>"
            '        EntryPtr = EntryPtr + 1
            '    Loop
            'End If
            qs = blogListQs
            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch)
            s = s & cr & "<div>&nbsp;</div>"
            s = s & cr & "<div Class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
            qs = cp.Doc.RefreshQueryString()
            qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
            s = s & cr & "<div Class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
            Call cp.CSNew.Close()
            'Call Main.CloseCS(CS)
            '
            's = s & Main.GetFormInputHidden(RequestNameBlogEntryID, CommentBlogID)
            s = s & cp.Html.Hidden(RequestNameSourceFormID, FormBlogArchivedBlogs)
            s = s & cp.Html.Form(s)
            '
            GetFormBlogArchivedBlogs = s
            '
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Friend Function GetFormTableRow(FieldCaption As String, Innards As String, AlignLeft As Boolean) As Object
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
            Stream = Stream & "<td Class=""aoBlogTableRowCellLeft"" " & AlignmentString & ">" & FieldCaption & "<img src=""/cclib/images/spacer.gif"" width=100 height=1 alt="" ""></TD>"
            Stream = Stream & "<td Class=""aoBlogTableRowCellRight"">" & Innards & "</TD>"
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
        Private Function GetField(cp As CPBaseClass, RequestName As String, Height As String, Width As String, MaxLenghth As String, DefaultValue As String) As String
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
            Stream = cp.Html.InputText(RequestName, DefaultValue, Height, Width, RequestName)
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
        Private Function GetFormBlogPostList(cp As CPBaseClass, blogId As Integer, BlogName As String, IsBlogOwner As Boolean, IsEditing As Boolean, EntryID As Integer, AllowCategories As Boolean, BlogCategoryID As Integer, RSSFeedName As String, RSSFeedFilename As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogDescription As String, blogCaption As String, RSSFeedId As Integer, blogListLink As String, blogListQs As String) As String
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
                s = s & vbCrLf & "<h2 Class=""aoBlogCaption"">" & blogCaption & "</h2>"
            End If
            If blogDescription <> "" Then
                s = s & vbCrLf & "<div Class=""aoBlogDescription"">" & blogDescription & "</div>"
            End If
            ' s = s & vbCrLf & Main.GetFormStart
            '
            ' List Blog Entries
            '
            EntryPtr = 0
            '
            ' Display the most recent entries
            '
            If AllowCategories And (BlogCategoryID <> 0) Then
                BlogCategoryName = Main.GetRecordName("Blog Categories", BlogCategoryID)
                CS = Main.OpenCSContent(cnBlogEntries, "(BlogID=" & blogId & ")And(BlogCategoryID=" & BlogCategoryID & ")", "DateAdded Desc")
                s = s & cr & "<div Class=""aoBlogCategoryCaption"">Category " & BlogCategoryName & "</div>"
                NoneMsg = "There are no blog posts available In the category " & BlogCategoryName
            Else
                CS = Main.OpenCSContent(cnBlogEntries, "(BlogID=" & blogId & ")", "DateAdded Desc")
                NoneMsg = "There are no blog posts available"
            End If
            If Not Main.csok(CS) Then
                s = s & cr & "<div Class=""aoBlogProblem"">" & NoneMsg & "</div>"
            Else
                Do While Main.csok(CS) And EntryPtr < PostsToDisplay
                    TestCategoryID = cp.Doc.GetInteger(CS, "BlogCategoryid")
                    If TestCategoryID <> 0 Then
                        '
                        ' Check that this member has access to this category
                        '
                        IsBlocked = Not cp.User.IsAdmin()
                        If IsBlocked Then
                            '
                            'If IsBlocked Then
                            If Main.ContentServerVersion < "4.1.098" Then
                                CSCat = Main.OpenCSContent(cnBlogCategories, "id=" & TestCategoryID)
                                If Main.IsCSOK(CSCat) Then
                                    IsBlocked = cp.Site.GetBoolean(CSCat, "UserBlocking")
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
                                    IsBlocked = cp.Site.GetBoolean(CSCat, "UserBlocking")
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
                        EntryID = cp.Doc.GetInteger(CS, "ID")
                        AuthorMemberID = cp.Doc.GetInteger(CS, "AuthorMemberID")
                        If AuthorMemberID = 0 Then
                            AuthorMemberID = cp.Doc.GetInteger(CS, "createdBy")
                        End If
                        DateAdded = cp.Doc.GetText(CS, "DateAdded")
                        EntryName = cp.Doc.GetText(CS, "Name")
                        If IsEditing Then
                            entryEditLink = Main.GetCSRecordEditLink(CS)
                        End If
                        EntryCopy = cp.Doc.GetText(CS, "Copy")
                        'EntryCopyOverview = cp.Doc.GetText(CS, "CopyOverview")
                        allowComments = cp.Site.GetBoolean(CS, "AllowComments")
                        PodcastMediaLink = Main.GetCS(CS, "PodcastMediaLink")
                        PodcastSize = Main.GetCS(CS, "PodcastSize")
                        BlogTagList = Main.GetCS(CS, "BlogTagList")
                        imageDisplayTypeId = cp.Doc.GetInteger(CS, "imageDisplayTypeId")
                        primaryImagePositionId = cp.Doc.GetInteger(CS, "primaryImagePositionId")
                        articlePrimaryImagePositionId = cp.Doc.GetInteger(CS, "articlePrimaryImagePositionId")
                        s = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                        s = s & cr & "<div Class=""aoBlogEntryDivider"">&nbsp;</div>"
                        EntryPtr = EntryPtr + 1
                    End If
                    Call Main.NextCSRecord(CS)
                Loop
            End If
            '
            ' Build Footers
            '
            If cp.User.IsAdmin() And AllowCategories Then
                qs = "cid=" & cp.Content.GetID("Blog Categories") & "&af=4"
                CategoryFooter = CategoryFooter & cr & "<div Class=""aoBlogFooterLink""><a href=""" & Main.SiteProperty_AdminURL & "?" & qs & """>Add a New category</a></div>"
            End If
            ReturnFooter = ""
            If AllowCategories Then
                'If BlogCategoryID <> 0 Then
                '
                ' View all categories
                '
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogCategoryIDSet, "0", True)
                CategoryFooter = CategoryFooter & cr & "<div Class=""aoBlogFooterLink""><a href=""" & blogListLink & """>See posts In all categories</a></div>"
                'Else
                '
                ' select a category
                '
                qs = cp.Doc.RefreshQueryString
                CS = Main.OpenCSContent(cnBlogCategories)
                Do While Main.IsCSOK(CS)
                    BlogCategoryID = cp.Doc.GetInteger(CS, "id")
                    IsBlocked = Csv.GetCSBoolean(CS, "UserBlocking")
                    If IsBlocked Then
                        IsBlocked = Not cp.User.IsAdmin()
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
                        'qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogCategoryIDSet, CStr(BlogCategoryID), True)
                        categoryLink = kmaModifyLinkQuery(blogListLink, RequestNameBlogCategoryIDSet, CStr(BlogCategoryID), True)
                        CategoryFooter = CategoryFooter & cr & "<div Class=""aoBlogFooterLink""><a href=""" & categoryLink & """>See posts In the category " & Main.GetCS(CS, "name") & "</a></div>"
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
                Call Main.testpoint("Blogs.GetFormBlogPostList, IsBlogOwner = True, appending 'create' message")
            '
            ' Create a new entry if this is the Blog Owner
            '
            qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor, True)
                s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Create new post</a></div>"
                '
                ' Create a link to edit the blog record
                '
                qs = "cid=" & cp.Content.GetID("Blogs") & "&af=4&id=" & blogId
                s = s & cr & "<div class=""aoBlogFooterLink""><a href=""" & Main.SiteProperty_AdminURL & "?" & qs & """>Edit blog features</a></div>"
                '
                ' Create a link to edit the rss record
                '
                If RSSFeedId = 0 Then
                Else
                    qs = "cid=" & cp.Content.GetID("RSS Feeds") & "&af=4&id=" & RSSFeedId
                    s = s & cr & "<div class=""aoBlogFooterLink""><a href=""" & Main.SiteProperty_AdminURL & "?" & qs & """>Edit rss feed features</a></div>"
                End If
            End If
            s = s & ReturnFooter
            s = s & CategoryFooter
            '
            ' Search
            '
            qs = cp.Doc.RefreshQueryString
            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch, True)
            s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
            '
            ' Link to archives
            '
            qs = cp.Doc.RefreshQueryString
            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchiveDateList, True)
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
                BlogTitle = cp.Doc.getText(RequestNameBlogTitle)
                BlogCopy = cp.Doc.getText(RequestNameBlogCopy)
                BlogTagList = cp.Doc.getText(RequestNameBlogTagList)
            Else
                hint = "3"
                '
                ' Edit an entry
                '
                CS = Main.OpenCSContentRecord(cnBlogEntries, EntryID)
                If Main.IsCSOK(CS) Then
                    BlogTitle = cp.Doc.GetText(CS, "Name")
                    BlogCopy = cp.Doc.GetText(CS, "Copy")
                    BlogCategoryID = cp.doc.getinteger(CS, "BlogCategoryID")
                    BlogTagList = cp.Doc.GetText(CS, "TagList")
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
            s = s & GetFormTableRow("<div style=""padding-top:3px"">Title: </div>", cp.Html.InputText(RequestNameBlogTitle, BlogTitle, 1, 50))
            s = s & GetFormTableRow("<div style=""padding-top:108px"">Post: </div>", editor)
            s = s & GetFormTableRow("<div style=""padding-top:3px"">Tag List: </div>", cp.Html.InputText(RequestNameBlogTagList, BlogTagList, 5, 50))
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
                ImageID = cp.doc.getinteger(CS, "id")
                Call GetBlogImage(ImageID, 200, 0, imageFilename, "", "", "")
                imageDescription = cp.Doc.GetText(CS, "description")
                imageName = cp.Doc.GetText(CS, "name")
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
        & "<td>Name<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageName & "." & Ptr & """ SIZE=""25"" value=""" & cp.Utils.EncodeHTML(imageName) & """></TD>" _
        & "</tr>" _
        & ""
                '
                '   description
                '
                c = c _
        & "<tr>" _
        & "<td>Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50"">" & cp.Utils.EncodeHTML(imageDescription) & "</TEXTAREA><input type=""hidden"" name=""" & rnBlogImageID & "." & Ptr & """ value=""" & ImageID & """></TD>" _
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
            qs = cp.Doc.RefreshQueryString()
            qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
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
        Private Function ProcessFormBlogPost(cp As CPBaseClass, SourceFormID As Integer, blogId As Integer, EntryID As Integer, ButtonValue As String, BlogCategoryID As Integer, RSSFeedId As Integer, blogListLink As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer) As Integer
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
                        EntryName = cp.Doc.GetText(RequestNameBlogTitle)
                        Call Main.SetCS(CS, "Name", EntryName)
                        Call Main.SetCS(CS, "copy", cp.Doc.GetText(RequestNameBlogCopy))
                        Call Main.SetCS(CS, "TagList", cp.Doc.GetText(RequestNameBlogTagList))
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
                    UploadCount = cp.Doc.GetInteger("LibraryUploadCount")
                    If UploadCount > 0 Then
                        For UploadPointer = 1 To UploadCount
                            ImageID = cp.Doc.GetInteger(rnBlogImageID & "." & UploadPointer)
                            imageFilename = cp.Doc.GetText(rnBlogUploadPrefix & "." & UploadPointer)
                            imageOrder = cp.Doc.GetInteger(rnBlogImageOrder & "." & UploadPointer)
                            imageName = cp.Doc.GetText(rnBlogImageName & "." & UploadPointer)
                            imageDescription = cp.Doc.GetText(rnBlogImageDescription & "." & UploadPointer)
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
                                    BlogImageID = cp.Doc.GetInteger(CS, "id")
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
                                        sf = CreateObject("sfimageresize.imageresize")
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
                                        sf = Nothing
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
        Private Function GetFormBlogPostDetails(cp As CPBaseClass, blogId As Integer, EntryID As Integer, IsBlogOwner As Boolean, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String, allowCaptcha As Boolean) As String
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
            'QSBack = cp.Doc.RefreshQueryString()
            'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameBlogEntryID, "", True)
            'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameFormID, FormBlogPostList)
            s = s & cr & "<div class=""aoBlogHeaderLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
            '
            ' Print the Blog Entry
            '
            CommentCnt = 0
            CS = Main.OpenCSContentRecord(cnBlogEntries, EntryID)
            If Not Main.csok(CS) Then
                s = s & cr & "<div class=""aoBlogProblem"">Sorry, the blog post you selected is not currently available</div>"
            Else
                EntryID = cp.Doc.GetInteger(CS, "ID")
                AuthorMemberID = cp.Doc.GetInteger(CS, "AuthorMemberID")
                If AuthorMemberID = 0 Then
                    AuthorMemberID = cp.Doc.GetInteger(CS, "createdBy")
                End If
                DateAdded = cp.Doc.GetText(CS, "DateAdded")
                EntryName = cp.Doc.GetText(CS, "Name")
                If cp.User.IsAuthoring("Blogs") Then
                    entryEditLink = Main.GetCSRecordEditLink(CS)
                End If
                EntryCopy = cp.Doc.GetText(CS, "Copy")
                'EntryCopyOverview = cp.Doc.GetText(CS, "copyOverview")
                allowComments = cp.Site.GetBoolean(CS, "allowComments")
                PodcastMediaLink = Main.GetCS(CS, "PodcastMediaLink")
                PodcastSize = Main.GetCS(CS, "PodcastSize")
                BlogTagList = Main.GetCS(CS, "TagList")
                qs = ""
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                Call Main.AddLinkAlias(EntryName, cp.Doc.PageId, qs)
                Call Main.SetCS(CS, "viewings", 1 + cp.Doc.GetInteger(CS, "viewings"))
                imageDisplayTypeId = cp.Doc.GetInteger(CS, "imageDisplayTypeId")
                primaryImagePositionId = cp.Doc.GetInteger(CS, "primaryImagePositionId")
                articlePrimaryImagePositionId = cp.Doc.GetInteger(CS, "articlePrimaryImagePositionId")
                s = s & GetBlogEntryCell(EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, True, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                EntryPtr = EntryPtr + 1
                '
                blogId = cp.Doc.GetInteger(CS, "BlogID")
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
                        Call Main.SetCS(CS, "MemberID", cp.User.Id)
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
                    s = s & "<div class=""aoBlogCommentError"">" & (cp.UserError.ToString) & "</div>"
                End If
                '

                If (Not AllowAnonymous) And (Not cp.User.IsAuthenticated) Then
                    AllowPasswordEmail = cp.Utils.EncodeBoolean(cp.Site.GetProperty("AllowPasswordEmail", False))
                    AllowMemberJoin = cp.Utils.EncodeBoolean(cp.Site.GetProperty("AllowMemberJoin", False))
                    Auth = cp.Doc.GetInteger("auth")
                    If (Auth = 1) And (Not AllowPasswordEmail) Then
                        Auth = 3
                    ElseIf (Auth = 2) And (Not AllowMemberJoin) Then
                        Auth = 3
                    End If
                    Call cp.Doc.AddRefreshQueryString(RequestNameFormID, FormBlogPostDetails)
                    Call cp.Doc.AddRefreshQueryString(RequestNameBlogEntryID, EntryID)
                    Call cp.Doc.AddRefreshQueryString("auth", "0")
                    qs = cp.Doc.RefreshQueryString()
                    Select Case Auth
                        Case 1
                            '
                            ' password email
                            '
                            Copy = "To retrieve your username and password, submit your email. "
                            qs = cp.Utils.ModifyQueryString(qs, "auth", "0")
                            Copy = Copy & " <a href=""?" & qs & """> Login?</a>"
                            If AllowMemberJoin Then
                                qs = cp.Utils.ModifyQueryString(qs, "auth", "2")
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
                            qs = cp.Utils.ModifyQueryString(qs, "auth", "0")
                            Copy = Copy & " <a href=""?" & qs & """> Login?</a>"
                            If AllowPasswordEmail Then
                                qs = cp.Utils.ModifyQueryString(qs, "auth", "1")
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
                            '    qs = cp.Utils.ModifyQueryString(qs, "auth", "1")
                            '    Copy = Copy & " <a href=""?" & qs & """> Forget your username or password?</a>"
                            'End If
                            If AllowMemberJoin Then
                                qs = cp.Utils.ModifyQueryString(qs, "auth", "2")
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
                        s = s & cr & "<div class=""aoBlogCommentCopy"">" & GetField(RequestNameCommentTitle, , 35, , cp.Doc.GetText(RequestNameCommentTitle)) & "</div>"
                        s = s & cr & "<div>&nbsp;</div>"
                        s = s & cr & "<div class=""aoBlogCommentCopy"">Comment</div>"
                        s = s & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.InputText(RequestNameCommentCopy, cp.Doc.GetText(RequestNameCommentCopy), 15, 70) & "</div>"
                    Else
                        s = s & cr & "<div class=""aoBlogCommentCopy"">" & GetField(RequestNameCommentTitle, , 35) & "</div>"
                        s = s & cr & "<div>&nbsp;</div>"
                        s = s & cr & "<div class=""aoBlogCommentCopy"">Comment</div>"
                        s = s & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.InputText(RequestNameCommentCopy, "", 15, 70) & "</div>"
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
                qs = cp.Doc.RefreshQueryString()
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor)
                s = s & cr & "<div class=""aoBlogToolLink""><a href=""?" & qs & """>Edit</a></div>"
            End If
            '
            ' Search
            '
            qs = cp.Doc.RefreshQueryString
            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch, True)
            s = s & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
            '
            ' back to recent posts
            '
            'QSBack = cp.Doc.RefreshQueryString()
            'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameBlogEntryID, "", True)
            'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameFormID, FormBlogPostList)
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
                        optionStr = "Challenge=" + cp.Doc.getText("recaptcha_challenge_field")
                        optionStr = optionStr & "&Response=" + cp.Doc.getText("recaptcha_response_field")
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
                    formKey = cp.Doc.getText("formkey")
                    'formKey = Main.DecodeKeyNumber(formKey)
                    Copy = cp.Doc.getText(RequestNameCommentCopy)
                    If Copy <> "" Then
                        Call Main.testpoint("blog -- adding comment [" & Copy & "]")
                        'If (Main.VisitID <> kmaEncodeInteger(Main.DecodeKeyNumber(formKey))) Then
                        '    Call Main.AddUserError("<p>This comment has already been accepted.</p>")
                        'End If
                        CSP = Main.OpenCSContent(cnBlogComments, "formkey=" & cp.db.encodeSqlText(formKey))
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
                            EntryID = cp.doc.getInteger(RequestNameBlogEntryID)
                            CSP = Main.InsertCSRecord(cnBlogComments)
                            If AllowAnonymous And (Not cp.User.IsAuthenticated) Then
                                MemberID = 0
                                MemberName = AnonymousMemberName
                            Else
                                MemberID = cp.User.Id
                                MemberName = Main.MemberName
                            End If
                            If Main.csok(CSP) Then
                                Call Main.SetCS(CSP, "BlogID", blogId)
                                Call Main.SetCS(CSP, "active", 1)
                                'Call Main.SetCS(CSP, "Name", cp.Doc.getText(RequestNameAuthorName))
                                'Call Main.SetCS(CSP, "AuthorEmail", cp.Doc.getText(RequestNameAuthorEmail))
                                Call Main.SetCS(CSP, "Name", cp.Doc.getText(RequestNameCommentTitle))
                                '**** should have been copytext, not copy
                                Call Main.SetCS(CSP, "Copytext", Copy)
                                'Call Main.SetCS(CSP, "Copy", Copy)
                                Call Main.SetCS(CSP, "EntryID", EntryID)
                                'Call Main.SetCS(CSP, "AuthorMemberID", Main.MemberID)
                                Call Main.SetCS(CSP, "Approved", IsBlogOwner Or autoApproveComments)
                                Call Main.SetCS(CSP, "formkey", formKey)
                                CommentID = cp.doc.getinteger(CSP, "ID")
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
                        & vbCrLf & cp.Utils.EncodeHTML(Copy) _
                        & vbCrLf
                                EmailFromAddress = cp.Site.GetProperty("EmailFromAddress", "info@" & Main.ServerDomain)
                                Call Main.SendMemberEmail(BlogOwnerID, EmailFromAddress, "Blog comment notification for [" & BlogName & "]", EmailBody, False, False)
                                If authoringGroupId <> 0 Then
                                    authoringGroup = Main.GetRecordName("groups", authoringGroupId)
                                    If authoringGroup <> "" Then
                                        CS = Main.OpenCSGroupMembers(authoringGroup, "(allowbulkemail<>0)and(email<>'')")
                                        Do While Main.IsCSOK(CS)
                                            AuthoringMemberId = cp.doc.getinteger(CS, "id")
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
                        EntryCnt = cp.doc.getInteger("EntryCnt")
                        If EntryCnt > 0 Then
                            For EntryPtr = 0 To EntryCnt - 1
                                CommentCnt = cp.doc.getInteger("CommentCnt" & EntryPtr)
                                If CommentCnt > 0 Then
                                    For CommentPtr = 0 To CommentCnt - 1
                                        Suffix = EntryPtr & "." & CommentPtr
                                        CommentID = cp.doc.getInteger("CommentID" & Suffix)
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
        Private Function GetBlogCommentCell(cp As CPBaseClass, IsBlogOwner As Boolean, DateAdded As Date, Approved As Boolean, CommentName As String, CommentCopy As String, CommentID As Integer, EntryPtr As Integer, CommentPtr As Integer, AuthorMemberID As Integer, EntryID As Integer, IsSearchListing As Boolean, EntryName As String) As String
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
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
                s = s & cr & "<div class=""aoBlogEntryName"">Comment to Blog Post " & EntryName & ", <a href=""?" & qs & """>View this post</a></div>"
                s = s & cr & "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
            End If
            s = s & cr & "<div class=""aoBlogCommentName"">" & cp.Utils.EncodeHTML(CommentName) & "</div>"
            Copy = CommentCopy
            Copy = cp.Utils.EncodeHTML(Copy)
            Copy = Replace(Copy, vbCrLf, "<BR />")
            s = s & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>"
            RowCopy = ""
            If authorMemberName <> "" Then
                RowCopy = RowCopy & "by " & cp.Utils.EncodeHTML(authorMemberName)
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
            If (Not Approved) And (Not IsBlogOwner) And (AuthorMemberID = cp.User.Id) Then
                s = "<div style=""border:1px solid red;padding-top:10px;padding-bottom:10px;""><span class=""aoBlogCommentName"" style=""color:red;"">Your comment pending approval</span><br />" & s & "</div>"
            End If
            '
            GetBlogCommentCell = s
            'GetBlogCommentCell = cr & "<div class=""aoBlogComment"">" & s & cr & "</div>"
            '
        End Function
        '
        Private Function GetBlogEntryCell(cp As CPBaseClass, EntryPtr As Integer, IsBlogOwner As Boolean, EntryID As Integer, EntryName As String, EntryCopy As String, DateAdded As Date, DisplayFullEntry As Boolean, IsSearchListing As Boolean, Return_CommentCnt As Integer, allowComments As Boolean, PodcastMediaLink As String, PodcastSize As String, entryEditLink As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, BlogTagList As String, imageDisplayTypeId As Integer, primaryImagePositionId As Integer, articlePrimaryImagePositionId As Integer, blogListQs As String, AuthorMemberID As Integer) As String
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
            qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
            If Main.version > "4.1.160" Then
                EntryLink = getLinkAlias("?" & qs)
            End If
            If EntryLink = "" Then
                qs = blogListQs
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
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
                imageIDList = imageIDList & "," & cp.Doc.GetText(CS, "id")
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
                Call GetBlogImage(cp.Utils.EncodeInteger(ImageID(0)), ThumbnailImageWidth, 0, ThumbnailFilename, imageFilename, imageDescription, imageName)
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
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor)
                c = ""
                If (imageDisplayTypeId = imageDisplayTypeList) And (imageIDList <> "") Then
                    c = ""
                    For cnt = 0 To UBound(ImageID)
                        Call GetBlogImage(cp.Utils.EncodeInteger(ImageID(cnt)), 0, ImageWidthMax, ThumbnailFilename, imageFilename, imageDescription, imageName)
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
                & cp.Html.Indent(c) _
                & cr & "</div>"
                    End If
                End If
                hint = hint & ",5"
                If BlogTagList <> "" Then
                    BlogTagList = Replace(BlogTagList, ",", vbCrLf)
                    Tags = Split(BlogTagList, vbCrLf)
                    BlogTagList = ""
                    Dim SQS As String
                    SQS = cp.Utils.ModifyQueryString(blogListQs, RequestNameFormID, FormBlogSearch, True)
                    For Ptr = 0 To UBound(Tags)
                        'QS = cp.Utils.ModifyQueryString(SQS, RequestNameFormID, FormBlogSearch, True)
                        qs = cp.Utils.ModifyQueryString(SQS, RequestNameQueryTag, Tags(Ptr), True)
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
            & cp.Html.Indent(c) _
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
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
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
                    '            QS = cp.Utils.ModifyQueryString(QS, RequestNameBlogEntryID, CStr(EntryID))
                    '            QS = cp.Utils.ModifyQueryString(QS, RequestNameFormID, FormBlogPostDetails)
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
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor)
                        If ToolLine <> "" Then
                            ToolLine = ToolLine & "&nbsp;|&nbsp;"
                        End If
                        ToolLine = ToolLine & "<a href=""?" & qs & """>Edit</a>"
                    End If
                    '            If ToolLine <> "" Then
                    '                s = s & cr & "<div class=""aoBlogToolLink"">" & ToolLine & "</div>"
                    '            End If

                    's = s & cr & "<div><a class=""aoBlogFooterLink"" href=""" & Main.ServerPage & WorkingQueryString & RequestNameBlogEntryID & "=" & cp.doc.getinteger(CS, "ID") & "&" & RequestNameFormID & "=" & FormBlogPostDetails & """>Post a Comment</a></div>"

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
                        Criteria = "((Approved<>0)or(AuthorMemberID=" & cp.User.Id & "))and(EntryID=" & EntryID & ")"
                    End If
                    CS = Main.OpenCSContent(cnBlogComments, Criteria, "DateAdded")
                    If Main.csok(CS) Then
                        Divider = "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                        s = s & cr & "<div class=""aoBlogCommentHeader"">Comments</div>"
                        s = s & vbCrLf & Divider
                        CommentPtr = 0
                        Do While Main.csok(CS)
                            CommentID = cp.Doc.GetInteger(CS, "ID")
                            AuthorMemberID = cp.Doc.GetInteger(CS, "createdby")
                            DateAdded = cp.Doc.GetDate(CS, "DateAdded")
                            Approved = cp.Site.GetBoolean(CS, "Approved")
                            CommentName = cp.Doc.GetText(CS, "Name")
                            CommentCopy = cp.Doc.GetText(CS, "Copytext")
                            If CommentCopy = "" Then
                                CommentCopy = cp.Doc.GetText(CS, "Copy")
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
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                        EntryLink = Main.GetLinkAliasByPageID(cp.doc.pageId, qs, blogListLink & "?" & qs)
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
                        Return_RSSFeedID = cp.doc.getinteger(CSFeed, "ID")
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
                    Return_RSSFeedName = cp.Doc.GetText(CSFeed, "name")
                    Return_RSSFeedID = cp.doc.getinteger(CSFeed, "id")
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
                    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameFormID, "", False)
                    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameSourceFormID, "", False)
                    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryID, "", False)
                    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryIDSet, "", False)
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
                Filename = cp.Doc.GetText(CS, "filename")
                AltSizeList = cp.Doc.GetText(CS, "AltSizeList")
                Return_ImageDescription = cp.Doc.GetText(CS, "description")
                Return_Imagename = cp.Doc.GetText(CS, "name")
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
                            DimWidth = cp.Utils.encodeInteger(Dims(0))
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
                            sf = CreateObject("sfimageresize.imageresize")
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
                            sf = CreateObject("sfimageresize.imageresize")
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
            If cp.Utils.EncodeBoolean(cp.Site.GetProperty("allowLinkAlias", "1")) Then
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
                    users(userPtr).Name = cp.Doc.GetText(CS, "name")
                    users(userPtr).authorInfoLink = ""
                    If allowAuthorInfoLink Then
                        users(userPtr).authorInfoLink = cp.Doc.GetText(CS, "authorInfoLink")
                    End If
                End If
                Call Main.CloseCS(CS)
            End If
            getUserPtr = userPtr
            '
        End Function
    End Class
End Namespace
