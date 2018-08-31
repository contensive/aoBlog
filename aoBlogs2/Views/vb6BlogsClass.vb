
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
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
        Private ReadOnly App As Object

        Public Function GetContent(cp As CPBaseClass) As String
            '
            Dim result As String = ""
            Dim instanceId As String = cp.Doc.GetText("instanceId")
            Try
                Dim blogListQs As String
                '

                ' get blogListLink - link to the main list page
                '
                blogListQs = cp.Doc.RefreshQueryString()
                blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, "")
                blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameFormID, "")
                blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameBlogCategoryID, "")
                blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameBlogEntryID, "")
                'Dim BlogRootLink As String
                Dim blogListLink As String = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", "") & "?" & blogListQs
                Dim BlogName As String = ""
                '

                result = vbCrLf & "<!-- Blog " & BlogName & " -->" & vbCrLf
                '
                'If Not (Main Is Nothing) Then
                BlogName = cp.Doc.GetText("BlogName")
                If BlogName = "" Then
                    BlogName = "Default"
                End If

                Dim blog As blogModel = blogModel.create(cp, instanceId)
                Dim blogCaption As String
                Dim blogDescription As String
                Dim ignoreLegacyInstanceOptions As Boolean
                Dim authoringGroupId As Integer
                Dim blogId As Integer
                Dim BlogOwnerID As Integer
                Dim AllowAnonymous As Boolean
                Dim AllowCategories As Boolean
                Dim RSSFeedId As Integer
                Dim ThumbnailImageWidth As Integer
                Dim ImageWidthMax As Integer
                Dim autoApproveComments As Boolean
                Dim emailComment As Boolean
                Dim allowRecaptcha As Boolean

                If Not (blog Is Nothing) Then
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

                Dim RSSFeedFilename As String = ""
                Dim IsBlogOwner As Boolean
                Dim EntryID As Integer
                Dim RSSFeedName As String = ""
                If blogId = 0 Then
                    '
                    ' Create New Blog
                    '

                    Dim IsContentManager As Boolean = cp.User.IsContentManager("Page Content")
                    If IsContentManager Then
                        '
                        ' BIG assumption - First hit by a content manager for this page is the author
                        '
                        blog = Models.blogModel.add(cp)
                        blogId = blog.id
                        BlogOwnerID = cp.User.Id
                        IsBlogOwner = True
                        blogCaption = BlogName
                        '
                        Blog.name = BlogName
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
                        Blog.ImageWidthMax = ImageWidthMax
                        Blog.ccguid = instanceId
                        Blog.save(cp)
                        '
                    End If

                    '
                    ' Create the Feed for the new blog
                    '
                    Call VerifyFeedReturnArgs(cp, blogId, blogListLink, RSSFeedId, RSSFeedName, RSSFeedFilename)
                    '
                    Dim Title As String = "Welcome to the " & BlogName
                    If InStr(1, BlogName, " Blog", vbTextCompare) = 0 Then
                        Title = Title & " blog"
                    End If
                    Title = Title & "!"
                    '
                    'CS = Main.InsertCSRecord(cnBlogEntries)

                    Dim blogEntry As Models.BlogEntryModel = Models.BlogEntryModel.add(cp)
                    Dim BlogPostID As Integer
                    If (blogEntry IsNot Nothing) Then

                        blogEntry.id = BlogPostID
                        blogEntry.BlogID = blogId.ToString
                        blogEntry.name = Title

                        '
                        Dim RSSTitle As String = Trim(Title)
                        If RSSTitle = "" Then
                            RSSTitle = "Blog Post " & EntryID
                        End If
                        blogEntry.RSSTitle = RSSTitle

                        '
                        Dim NewPostCopy As String = cp.File.ReadVirtual("blogs/DefaultPostCopy.txt")
                        If NewPostCopy = "" Then
                            NewPostCopy = DefaultPostCopy
                        End If
                        blogEntry.Copy = NewPostCopy

                        '

                        Dim qs As String = ""
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                        Call cp.Site.addLinkAlias(Title, cp.Doc.PageId, qs)
                        'Dim LinkAlias As Models.LinkAliasesModel = Models.LinkAliasesModel.add(cp)
                        Dim LinkAlias As List(Of Models.LinkAliasesModel) = Models.LinkAliasesModel.createList(cp, "(pageid=" & cp.Doc.PageId & ")and(QueryStringSuffix=" & cp.Db.EncodeSQLText(qs) & ")")
                        If (LinkAlias.Count > 0) Then
                            Dim EntryLink As String = LinkAlias.First.name
                        End If
                        blogEntry.RSSDescription = filterCopy(cp, NewPostCopy, 150)
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
                        RSSFeedBlogRules.save(cp)

                    End If

                End If

                'End If

                IsBlogOwner = (cp.User.IsAuthenticated And (cp.User.Id = BlogOwnerID))
                If Not IsBlogOwner Then
                    IsBlogOwner = IsBlogOwner Or cp.User.IsAdmin()
                    If Not IsBlogOwner Then
                        If authoringGroupId <> 0 Then
                            Dim authoringGroup As String = cp.Content.GetRecordName("Groups", authoringGroupId)
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
                    AllowAnonymous = cp.Doc.GetBoolean("Allow Anonymous Comments")
                    autoApproveComments = cp.Doc.GetBoolean("Auto-Approve New Comments")
                    AllowCategories = cp.Doc.GetBoolean("Allow Categories")
                    PostsToDisplay = cp.Doc.GetInteger("Number of entries to display")
                    OverviewLength = cp.Doc.GetInteger("Overview Character Length")
                    ThumbnailImageWidth = cp.Doc.GetInteger("Thumbnail Image Width")
                    If ThumbnailImageWidth = 0 Then
                        ThumbnailImageWidth = 150
                    End If
                    ImageWidthMax = cp.Doc.GetInteger("Image Width Max")
                    If ImageWidthMax = 0 Then
                        ImageWidthMax = 400
                    End If
                    If OverviewLength = 0 Then
                        OverviewLength = 150
                    End If
                    If PostsToDisplay = 0 Then
                        PostsToDisplay = 5
                    End If

                    Dim legacyblog As Models.blogModel = Models.blogModel.create(cp, blogId)
                    If Not (legacyblog Is Nothing) Then
                        legacyblog = Models.blogModel.add(cp)
                        legacyblog.ignoreLegacyInstanceOptions = True
                        legacyblog.AllowAnonymous = AllowAnonymous
                        legacyblog.autoApproveComments = autoApproveComments
                        legacyblog.AllowCategories = AllowCategories
                        legacyblog.PostsToDisplay = PostsToDisplay
                        legacyblog.OverviewLength = OverviewLength
                        legacyblog.ThumbnailImageWidth = ThumbnailImageWidth
                        legacyblog.ImageWidthMax = ImageWidthMax
                        legacyblog.save(cp)

                    End If
                End If
                Dim BlogCategoryID As Integer

                If AllowCategories Then
                    If (cp.Doc.GetText(RequestNameBlogCategoryIDSet) <> "") Then
                        BlogCategoryID = cp.Doc.GetInteger(RequestNameBlogCategoryIDSet)
                    Else
                        BlogCategoryID = cp.Doc.GetInteger(RequestNameBlogCategoryID)
                    End If

                    Call cp.Doc.AddRefreshQueryString(RequestNameBlogCategoryID, BlogCategoryID)

                End If

                '
                If IsBlogOwner And (blogId = 0) Then
                    '
                    ' Blog record was not, or can not be created
                    '
                    result = result & cp.Html.adminHint("This blog has not been configured. Please edit this page and edit the properties for the blog Add-on")
                Else
                    '
                    ' Get the Feed Args
                    '

                    Dim RSSFeedModelList As List(Of Models.RSSFeedModel) = Models.RSSFeedModel.createList(cp, "id=" & RSSFeedId)
                    If (RSSFeedModelList.Count > 0) Then
                        Dim RSSFeed As Models.RSSFeedModel = RSSFeedModelList.First
                        RSSFeedName = RSSFeed.name
                        RSSFeedFilename = RSSFeed.RSSFilename
                    Else
                        Dim RSSFeed As Models.RSSFeedModel = Models.RSSFeedModel.add(cp)
                        If Not RSSFeedName <> "" Then
                            RSSFeedName = "Default"
                        End If
                        RSSFeed.name = RSSFeedName
                        RSSFeed.Description = "This is your First RssFeed"
                        RSSFeed.save(cp)
                    End If

                    '

                    If RSSFeedFilename = "" Then
                        'If BlogRootLink = "" Or RSSFeedFilename = "" Then
                        '
                        ' feed has not been initialized yet, call it now
                        '
                        Call VerifyFeedReturnArgs(cp, blogId, blogListLink, RSSFeedId, RSSFeedName, RSSFeedFilename)
                    End If

                    '
                    ' Process Input
                    '
                    Dim ButtonValue As String = cp.Doc.GetText("button")
                    Dim FormID As Integer = cp.Doc.GetInteger(RequestNameFormID)
                    Dim SourceFormID As Integer = cp.Doc.GetInteger(RequestNameSourceFormID)
                    Dim KeywordList As String = cp.Doc.GetText(RequestNameKeywordList)
                    Dim DateSearchText As String = cp.Doc.GetText(RequestNameDateSearch)
                    'BlogTitle = cp.Doc.getText(RequestNameBlogTitle)
                    'BlogCopy = cp.Doc.getText(RequestNameBlogCopy)
                    Dim ArchiveMonth As Integer = cp.Doc.GetInteger(RequestNameArchiveMonth)
                    Dim ArchiveYear As Integer = cp.Doc.GetInteger(RequestNameArchiveYear)
                    EntryID = cp.Doc.GetInteger(RequestNameBlogEntryID)
                    Dim BuildVersion As String
                    '
                    '
                    If ButtonValue <> "" Then
                        Dim OptionString As String = ""
                        '
                        ' Process the source form into form if there was a button - else keep formid
                        '
                        FormID = ProcessForm(cp, SourceFormID, blogId, IsBlogOwner, EntryID, ButtonValue, BlogName, BlogOwnerID, AllowAnonymous, AllowCategories, BlogCategoryID, RSSFeedId, blogListLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, autoApproveComments, authoringGroupId, emailComment, OptionString, allowRecaptcha)
                    End If
                    '
                    ' Get Next Form
                    '

                    result = result & GetForm(cp, FormID, blogId, BlogName, IsBlogOwner, ArchiveMonth, ArchiveYear, EntryID, KeywordList, ButtonValue, DateSearchText, AllowAnonymous, AllowCategories, BlogCategoryID, RSSFeedName, RSSFeedFilename, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogDescription, blogCaption, RSSFeedId, blogListLink, blogListQs, allowRecaptcha)

                End If


                '
                GetContent = result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetForm(cp As CPBaseClass, FormID As Integer, blogId As Integer, BlogName As String, IsBlogOwner As Boolean, ArchiveMonth As Integer, ArchiveYear As Integer, EntryID As Integer, KeywordList As String, ButtonValue As String, DateSearchText As String, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, RSSFeedName As String, RSSFeedFilename As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogDescription As String, blogCaption As String, RSSFeedId As Integer, blogListLink As String, blogListQs As String, allowCaptcha As Boolean) As String
            '
            Dim Stream As String = ""
            Try

                Dim IsEditing As Boolean
                '
                'Call Main.AddRefreshQueryString(RequestNameSourceFormID, FormID)

                IsEditing = cp.User.IsAuthoring("Blogs")

                ' Dim Csv As Object = Nothing

                Dim PeopleModelList As List(Of Models.PeopleModel) = Models.PeopleModel.createList(cp, "id=" & cp.User.Id)
                Dim user As Models.PeopleModel = PeopleModelList.First

                allowAuthorInfoLink = True
                '

                Select Case FormID
                    Case FormBlogPostDetails
                        Stream = Stream & GetFormBlogPostDetails(cp, blogId, EntryID, IsBlogOwner, AllowAnonymous, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs, allowCaptcha)
                    Case FormBlogArchiveDateList
                        Stream = Stream & GetFormBlogArchiveDateList(cp, blogId, BlogName, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                    Case FormBlogArchivedBlogs
                        Stream = Stream & GetFormBlogArchivedBlogs(cp, blogId, BlogName, ArchiveMonth, ArchiveYear, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                    Case FormBlogEntryEditor
                        Stream = Stream & GetFormBlogPost(cp, blogId, BlogName, IsBlogOwner, EntryID, AllowCategories, BlogCategoryID, blogListLink)
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
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return Stream
        End Function
        '
        '====================================================================================
        '   Processes the SourceFormID
        '       returns the FormID for the next one to display
        '====================================================================================
        '
        Private Function ProcessForm(cp As CPBaseClass, SourceFormID As Integer, blogId As Integer, IsBlogOwner As Boolean, EntryID As Integer, ButtonValue As String, BlogName As String, BlogOwnerID As Integer, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, RSSFeedId As Integer, blogListLink As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, autoApproveComments As Boolean, authoringGroupId As Integer, emailComment As Boolean, OptionString As String, allowRecaptcha As Boolean) As Integer
            '
            Try
                If ButtonValue <> "" Then
                    Select Case SourceFormID
                        Case FormBlogPostList
                            ProcessForm = FormBlogPostList
                        Case FormBlogEntryEditor
                            ProcessForm = ProcessFormBlogPost(cp, SourceFormID, blogId, EntryID, ButtonValue, BlogCategoryID, RSSFeedId, blogListLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax)
                        Case FormBlogPostDetails
                            ProcessForm = ProcessFormBlogPostDetails(cp, SourceFormID, blogId, IsBlogOwner, ButtonValue, BlogName, BlogOwnerID, AllowAnonymous, BlogCategoryID, autoApproveComments, authoringGroupId, emailComment, OptionString, allowRecaptcha)
                        Case FormBlogArchiveDateList
                            ProcessForm = FormBlogArchiveDateList
                        Case FormBlogSearch
                            ProcessForm = FormBlogSearch
                        Case FormBlogArchivedBlogs
                            ProcessForm = FormBlogArchivedBlogs
                    End Select
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogSearch(cp As CPBaseClass, blogId As Integer, BlogName As String, IsBlogOwner As Boolean, KeywordList As String, ButtonValue As String, DateSearchText As String, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String) As String
            '
            Dim result As String = ""
            Try
                '
                Call cp.Doc.AddRefreshQueryString(RequestNameBlogEntryID, "")
                '
                result = result & vbCrLf & cp.Content.GetCopy("Blogs Search Header for " & BlogName, "<h2>" & BlogName & " Blog Search</h2>")
                '
                ' Search results
                '
                If DateSearchText = "" Then
                    DateSearchText = Date.MinValue.ToString
                End If
                Dim QueryTag As String = cp.Doc.GetText(RequestNameQueryTag)
                Dim Button As String = cp.Doc.GetText("button")
                If (Button = FormButtonSearch) Or (QueryTag <> "") Then
                    Dim DateSearch As Date
                    '
                    ' Attempt to figure out the date provided
                    '
                    If DateSearchText <> Date.MinValue Then
                        DateSearch = DateSearchText
                        If DateSearch < CDate("1/1/2000") Then
                            DateSearch = Date.MinValue
                            Call cp.Site.ErrorReport("The date is not valid")
                        End If
                    End If
                    '
                    Dim Subcaption As String = ""
                    Dim subCriteria As String
                    Dim Criteria As String = "(blogId=" & blogId & ")"
                    subCriteria = ""
                    If (KeywordList <> "") Then
                        KeywordList = "," & KeywordList & ","
                        Dim KeyWordsArray() As String = Split(KeywordList, ",", , vbTextCompare)
                        Dim KeyWordsArrayCounter As Integer = UBound(KeyWordsArray)
                        Dim CounterKeyWords As Integer
                        For CounterKeyWords = 0 To KeyWordsArrayCounter
                            If KeyWordsArray(CounterKeyWords) <> "" Then
                                If subCriteria <> "" Then
                                    subCriteria = subCriteria & "or"
                                    Subcaption = Subcaption & " or "
                                End If
                                Dim EnteredKeyWords As String = KeyWordsArray(CounterKeyWords)
                                Subcaption = Subcaption & "'<i>" & cp.Db.EncodeSQLText(EnteredKeyWords) & "</i>'"
                                EnteredKeyWords = cp.Db.EncodeSQLText(EnteredKeyWords)
                                EnteredKeyWords = "'%" & Mid(EnteredKeyWords, 2, Len(EnteredKeyWords) - 2) & "%'"
                                subCriteria = subCriteria & " (Copy like " & EnteredKeyWords & ") or (name like " & EnteredKeyWords & ")"
                            End If
                        Next
                        If Subcaption <> "" Then
                            Subcaption = " containing " & Subcaption
                        End If
                        If subCriteria <> "" Then
                            Criteria = Criteria & "and(" & subCriteria & ")"
                        End If
                    End If
                    If (DateSearch <> Date.MinValue) Then
                        Dim SearchMonth As String = Month(DateSearch)
                        Dim SearchYear As String = Year(DateSearch)
                        Subcaption = Subcaption & " in " & SearchMonth & "/" & SearchYear
                        If Criteria <> "" Then
                            Criteria = Criteria & "AND"
                        End If
                        Criteria = Criteria & "(DateAdded>=" & cp.Db.EncodeSQLDate(DateSearch) & ")and(DateAdded<" & cp.Db.EncodeSQLDate(AddMonth(cp, DateSearch, 1)) & ")"
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


                    If Not cp.UserError.OK Then
                        Subcaption = Subcaption & cp.Html.ul(cp.UserError.GetList)
                    End If
                    '
                    Dim BlogCopyList As New List(Of Models.BlogCopyModel)
                    If Criteria <> "" Then
                        BlogCopyList = Models.BlogCopyModel.createList(cp, Criteria)
                        cp.Utils.AppendLogFile("search list criteria=" & Criteria)
                    End If
                    '
                    ' Display the results
                    '
                    result = result & cr & "<div class=""aoBlogEntryCopy"">" & Subcaption & "</div>"
                    '
                    If (BlogCopyList.Count = 0) Then
                        cp.Utils.AppendLogFile("search list count" & BlogCopyList.Count)
                        result = result & "</br>" & "<div class=""aoBlogProblem"">There were no matches to your search</div>"
                    Else
                        cp.Utils.AppendLogFile("search list count" & BlogCopyList.Count)
                        result = result & "</br>" & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                        For Each blogCopy In Models.BlogCopyModel.createList(cp, Criteria)
                            Dim ParentID As Integer = cp.Doc.GetInteger("EntryID")
                            Dim AuthorMemberID As Integer
                            Dim ResultPtr As Integer
                            Dim EntryID As Integer
                            Dim EntryName As String
                            Dim DateAdded As Date
                            If ParentID = 0 Then
                                '
                                ' Entry
                                '
                                AuthorMemberID = blogCopy.AuthorMemberID
                                If AuthorMemberID = 0 Then
                                    AuthorMemberID = blogCopy.CreatedBy
                                End If
                                EntryID = blogCopy.id
                                EntryName = blogCopy.name
                                Dim EntryCopy As String = blogCopy.Copy
                                'EntryCopyOverview = cp.Doc.GetText(CSPointer, "copyOverview")
                                DateAdded = blogCopy.DateAdded
                                Dim allowComments As Boolean = blogCopy.AllowComments
                                Dim PodcastMediaLink As String = blogCopy.PodcastMediaLink
                                Dim PodcastSize As String = blogCopy.PodcastSize
                                Dim BlogTagList As String = blogCopy.TagList
                                Dim imageDisplayTypeId As Integer = blogCopy.imageDisplayTypeId
                                Dim primaryImagePositionId As Integer = blogCopy.primaryImagePositionId
                                Dim articlePrimaryImagePositionId As Integer = blogCopy.articlePrimaryImagePositionId
                                Dim Return_CommentCnt As Integer
                                result = result & GetBlogEntryCell(cp, ResultPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, True, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, "", ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                            Else
                                '
                                ' Comment
                                '
                                AuthorMemberID = cp.Doc.GetInteger("createdBy")
                                Dim CommentID As Integer = cp.Doc.GetInteger("ID")
                                Dim CommentName As String = cp.Doc.GetText("name")
                                Dim CommentCopy As String = cp.Doc.GetText("copyText")
                                DateAdded = cp.Doc.GetDate("DateAdded")
                                Dim Approved As Boolean = cp.Doc.GetBoolean("Approved")
                                EntryID = cp.Doc.GetInteger("EntryID")
                                EntryName = cp.Doc.GetText("Blog Copy", EntryID.ToString)
                                result = result & GetBlogCommentCell(cp, IsBlogOwner, DateAdded, Approved, CommentName, CommentCopy, CommentID, ResultPtr, 0, AuthorMemberID, EntryID, True, EntryName)
                            End If
                            '
                            result = result & cr & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                            ResultPtr = ResultPtr + 1

                        Next
                    End If

                    result = "" _
                                    & cr & "<div class=""aoBlogSearchResultsCon"">" _
                                    & cp.Html.Indent(result) _
                                    & cr & "</div>"
                End If
                '
                Dim searchForm As String = "" _
                                & cr & "<table width=100% border=0 cellspacing=0 cellpadding=5 class=""aoBlogSearchTable"">" _
                                & GetFormTableRow(cp, "Date:", GetField(cp, RequestNameDateSearch, 1, 10, 10, cp.Doc.GetText(RequestNameDateSearch).ToString) & " " & "&nbsp;(mm/yyyy)") _
                                & GetFormTableRow(cp, "Keyword(s):", GetField(cp, RequestNameKeywordList, 1, 10, 30, cp.Doc.GetText(RequestNameKeywordList))) _
                                & GetFormTableRow(cp, "", cp.Html.Button(rnButton, FormButtonSearch)) _
                                & GetFormTableRow2(cp, "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>") _
                                & cr & "</table>" _
                                & ""

                searchForm = "" _
                                & cr & "<div  class=""aoBlogSearchFormCon"">" _
                                & searchForm _
                                & cr & "</div>" _
                                & ""
                result = result & searchForm

                result = result & cr & "<input type=""hidden"" name=""" & RequestNameSourceFormID & """ value=""" & FormBlogSearch.ToString & """>"
                result = result & cr & "<input type=""hidden"" name=""" & RequestNameFormID & """ value=""" & FormBlogSearch.ToString & """>"
                result = cp.Html.Form(result)
                '

                GetFormBlogSearch = result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function


        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogArchiveDateList(cp As CPBaseClass, blogId As Integer, BlogName As String, IsEditing As Boolean, IsBlogOwner As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String) As String
            '
            Dim result As String = ""
            Try
                Dim OpenSQL As String = ""
                Dim contentControlId As String = cp.Content.GetID(cnBlogEntries).ToString
                '
                cp.Utils.AppendLogFile("entering GetFormBlogArchiveDateList")
                result = vbCrLf & cp.Content.GetCopy("Blogs Archives Header for " & BlogName, "<h2>" & BlogName & " Blog Archive</h2>")
                ' 
                'Dim BlogCopyModelList As List(Of BlogCopyModel) = BlogCopyModel.createList(cp, "(BlogID=" & blogId & ")", "year(dateadded) desc, Month(DateAdded) desc")
                Dim archiveDateList As List(Of Models.BlogCopyModel.ArchiveDateModel) = Models.BlogCopyModel.createArchiveListFromBlogCopy(cp, blogId)
                cp.Utils.AppendLogFile("open archive date list")
                If (archiveDateList.Count = 0) Then
                    '
                    ' No archives, give them an error
                    '
                    result = result & cr & "<div class=""aoBlogProblem"">There are no current blog entries</div>"
                    result = result & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                Else
                    'Dim RowCnt As Integer = BlogCopyModelList.Count
                    Dim ArchiveMonth As Integer
                    Dim ArchiveYear As Integer
                    If archiveDateList.Count = 1 Then
                        '
                        ' one archive - just display it
                        cp.Utils.AppendLogFile("display one archive count 1")
                        '
                        ArchiveMonth = archiveDateList.First.Month
                        ArchiveYear = archiveDateList.First.Year
                        result = result & GetFormBlogArchivedBlogs(cp, blogId, BlogName, ArchiveMonth, ArchiveYear, IsEditing, IsBlogOwner, AllowCategories, BlogCategoryID, ThumbnailImageWidth, BuildVersion, ImageWidthMax, blogListLink, blogListQs)
                        cp.Utils.AppendLogFile("after display one archive count 1")
                    Else
                        '
                        cp.Utils.AppendLogFile("enter display list of archive")
                        ' Display List of archive
                        '
                        Dim qs As String = cp.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, FormBlogArchiveDateList)
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchivedBlogs)
                        For Each archiveDate In archiveDateList
                            ArchiveMonth = archiveDate.Month
                            ArchiveYear = archiveDate.Year
                            Dim NameOfMonth As String = MonthName(ArchiveMonth)
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveMonth, CStr(ArchiveMonth))
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameArchiveYear, CStr(ArchiveYear))
                            '
                            result = result & vbCrLf & vbTab & vbTab & "<div class=""aoBlogArchiveLink""><a href=""?" & qs & """>" & NameOfMonth & " " & ArchiveYear & "</a></div>"
                            's = s & vbCrLf & vbTab & vbTab & "<div class=""aoBlogArchiveLink""><a href=""?" & qs & RequestNameFormID & "=" & FormBlogArchivedBlogs & "&" & RequestNameArchiveMonth & "=" & ArchiveMonth & "&" & RequestNameArchiveYear & "=" & ArchiveYear & "&" & RequestNameSourceFormID & "=" & FormBlogArchiveDateList & """>" & NameOfMonth & " " & ArchiveYear & "</a></div>"

                        Next
                        cp.Utils.AppendLogFile("exit display list of archive")
                        result = result & vbCrLf & vbTab & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                    End If
                End If
                '

            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogArchivedBlogs(cp As CPBaseClass, blogId As Integer, BlogName As String, ArchiveMonth As Integer, ArchiveYear As Integer, IsEditing As Boolean, IsBlogOwner As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String) As String
            '
            Dim result As String = ""
            Try
                Dim PageNumber As Integer
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
                Dim BlogEntryModelList As List(Of Models.BlogEntryModel) = Models.BlogEntryModel.createList(cp, "(Month(DateAdded) = " & ArchiveMonth & ")And(year(DateAdded)=" & ArchiveYear & ")And(BlogID=" & blogId & ")", "DateAdded Desc")
                Dim BlogEntry As Models.BlogEntryModel = Models.BlogEntryModel.add(cp)
                If Not (BlogEntry IsNot Nothing) Then 'If cp.CSNew.OK() Then
                    result = "<div Class=""aoBlogProblem"">There are no blog archives For " & ArchiveMonth & "/" & ArchiveYear & "</div>"
                Else
                    Dim EntryPtr As Integer = 0
                    Dim entryEditLink As String = ""
                    'Do While cp.CSNew.OK()
                    For Each BlogEntry In BlogEntryModelList
                        Dim EntryID As Integer = BlogEntry.id ' cp.Doc.GetInteger("ID")
                        Dim AuthorMemberID As Integer = BlogEntry.AuthorMemberID ' cp.Doc.GetInteger("AuthorMemberID")
                        If AuthorMemberID = 0 Then
                            AuthorMemberID = BlogEntry.CreatedBy ' cp.Doc.GetInteger("createdBy")
                        End If
                        Dim DateAdded As Date = BlogEntry.DateAdded ' cp.Doc.GetText("DateAdded")
                        Dim EntryName As String = BlogEntry.name 'cp.Doc.GetText("Name")
                        If IsEditing Then
                            entryEditLink = cp.Content.GetEditLink(EntryName, EntryID, 1, EntryName, 1) 'Main.GetCSRecordEditLink(CS)
                        End If
                        Dim EntryCopy As String = BlogEntry.Copy ' cp.Doc.GetText("Copy")
                        'EntryCopyOverview = cp.Doc.GetText(CS, "copyOverview")
                        Dim allowComments As Boolean = BlogEntry.AllowComments ' cp.Site.GetBoolean("allowComments")
                        Dim PodcastMediaLink As String = BlogEntry.PodcastMediaLink ' cp.Doc.GetText("PodcastMediaLink")
                        Dim PodcastSize As String = BlogEntry.PodcastSize ' cp.Doc.GetText("PodcastSize")
                        Dim BlogTagList As String = BlogEntry.TagList ' cp.Doc.GetText("TagList")
                        Dim imageDisplayTypeId As Integer = BlogEntry.imageDisplayTypeId ' cp.Doc.GetInteger("imageDisplayTypeId")
                        Dim primaryImagePositionId As Integer = BlogEntry.primaryImagePositionId ' cp.Doc.GetInteger("primaryImagePositionId")
                        Dim articlePrimaryImagePositionId As Integer = BlogEntry.articlePrimaryImagePositionId ' cp.Doc.GetInteger("articlePrimaryImagePositionId")
                        Dim Return_CommentCnt As Integer
                        result = result & GetBlogEntryCell(cp, EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                    Next
                    'Call cp.CSNew.GoNext()
                    result = result & cr & "<div Class=""aoBlogEntryDivider"">&nbsp;</div>"
                    EntryPtr = EntryPtr + 1

                End If
                '              
                '
                Dim qs As String = blogListQs
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch)
                result = result & cr & "<div>&nbsp;</div>"
                result = result & cr & "<div Class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                qs = cp.Doc.RefreshQueryString()
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
                result = result & cr & "<div Class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                Call cp.CSNew.Close()
                'Call Main.CloseCS(CS)
                '
                's = s & Main.GetFormInputHidden(RequestNameBlogEntryID, CommentBlogID)
                result = result & cp.Html.Hidden(RequestNameSourceFormID, FormBlogArchivedBlogs)
                result = cp.Html.Form(result)
                '
                GetFormBlogArchivedBlogs = result
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Friend Function GetFormTableRow(cp As CPBaseClass, FieldCaption As String, Innards As String, Optional AlignLeft As Boolean = True) As Object
            '
            Dim Stream As String = ""
            Try
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
                ' GetFormTableRow = Stream
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return Stream
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormTableRow2(cp As CPBaseClass, Innards As String) As String
            '
            Dim Stream As String = ""
            Try

                '
                Stream = Stream & "<tr>"
                Stream = Stream & "<td colspan=2 width=""100%"">" & Innards & "</TD>"
                Stream = Stream & "</tr>"
                '
                ' GetFormTableRow2 = Stream
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return Stream
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetField(cp As CPBaseClass, RequestName As String, Height As String, Width As String, MaxLenghth As String, DefaultValue As String) As String
            '
            Dim result As String = ""
            Try

                '
                If Height = "" Then
                    Height = 1
                End If
                If Width = "" Then
                    Width = 25
                End If
                '           
                result = cp.Html.InputText(RequestName, DefaultValue, Height, Width)
                result = Replace(result, "<INPUT ", "<INPUT maxlength=""" & MaxLenghth & """ ", 1, 99, CompareMethod.Text)
                '
                GetField = result
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
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
            Dim result As String = ""

            Try
                Dim locBlogTitle As String
                '
                locBlogTitle = BlogName

                If InStr(1, locBlogTitle, " Blog", vbTextCompare) = 0 Then
                    locBlogTitle = locBlogTitle & " blog"
                End If
                '

                If blogCaption <> "" Then
                    result = result & vbCrLf & "<h2 Class=""aoBlogCaption"">" & blogCaption & "</h2>"
                End If
                If blogDescription <> "" Then
                    result = result & vbCrLf & "<div Class=""aoBlogDescription"">" & blogDescription & "</div>"
                End If
                ' s = s & vbCrLf & Main.GetFormStart
                '
                ' List Blog Entries
                '
                Dim EntryPtr As Integer = 0
                '
                ' Display the most recent entries
                '
                cp.Utils.AppendLogFile(" GetFormBlogPostList 300")
                Dim BlogEntryModelList As List(Of Models.BlogEntryModel)
                Dim NoneMsg As String
                If AllowCategories And (BlogCategoryID <> 0) Then

                    Dim BlogCategoryName As String = cp.Doc.GetText("Blog Categories", BlogCategoryID)
                    BlogEntryModelList = Models.BlogEntryModel.createList(cp, "(BlogID=" & blogId & ")And(BlogCategoryID=" & BlogCategoryID & ")", "DateAdded Desc")

                    'CS = Main.OpenCSContent(cnBlogEntries, "(BlogID=" & blogId & ")And(BlogCategoryID=" & BlogCategoryID & ")", "DateAdded Desc")
                    result = result & cr & "<div Class=""aoBlogCategoryCaption"">Category " & BlogCategoryName & "</div>"
                    NoneMsg = "There are no blog posts available In the category " & BlogCategoryName

                Else
                    BlogEntryModelList = Models.BlogEntryModel.createList(cp, "(BlogID=" & blogId & ")", "DateAdded Desc")
                    'CS = Main.OpenCSContent(cnBlogEntries, "(BlogID=" & blogId & ")", "DateAdded Desc")
                    NoneMsg = "There are no blog posts available"
                End If

                Dim TestCategoryID As Integer
                Dim GroupList As List(Of GroupModel)
                Dim IsBlocked As Boolean

                If Not (BlogEntryModelList.Count <> 0) Then
                    'If Not Main.csok(CS) Then
                    result = result & cr & "<div Class=""aoBlogProblem"">" & NoneMsg & "</div>"
                Else
                    'Do While Main.csok(CS) And EntryPtr < PostsToDisplay

                    Dim blogEntry As Models.BlogEntryModel = BlogEntryModelList.First
                    For Each blogEntry In BlogEntryModelList
                        TestCategoryID = blogEntry.blogCategoryID 'cp.Doc.GetInteger(CS, "BlogCategoryid")
                        If TestCategoryID <> 0 Then
                            '
                            ' Check that this member has access to this category
                            '
                            IsBlocked = Not cp.User.IsAdmin()
                            If IsBlocked Then
                                '
                                'If IsBlocked Then
                                BuildVersion = cp.Site.GetProperty("BUILDVERSION")
                                If BuildVersion < "4.1.098" Then
                                    Dim BlogCategorieModelList As List(Of Models.BlogCategorieModel) = Models.BlogCategorieModel.createList(cp, "id=" & TestCategoryID)
                                    ' CSCat = Main.OpenCSContent(cnBlogCategories, "id=" & TestCategoryID)
                                    Dim blogCat As Models.BlogCategorieModel = BlogCategorieModelList.First
                                    'If cp.CSNew.OK() Then
                                    IsBlocked = blogCat.UserBlocking 'cp.Site.GetBoolean("UserBlocking")
                                    'End If
                                    'Call Main.CloseCS(CSCat)
                                    '
                                    If IsBlocked Then
                                        GroupList = GroupModel.GetBlockingGroups(cp, TestCategoryID)
                                        IsBlocked = Not genericController.IsGroupListMember(cp, GroupList)
                                    End If
                                Else

                                    Dim BlogCategorieModelList As List(Of Models.BlogCategorieModel) = Models.BlogCategorieModel.createList(cp, "id=" & TestCategoryID)
                                    ' CSCat = Main.OpenCSContent(cnBlogCategories, "id=" & TestCategoryID)
                                    Dim blogCat As Models.BlogCategorieModel = BlogCategorieModelList.First
                                    'If cp.CSNew.OK() Then
                                    IsBlocked = blogCat.UserBlocking 'cp.Site.GetBoolean("UserBlocking")
                                    If IsBlocked Then
                                        GroupList = GroupModel.GetBlockingGroups(cp, blogCat.id) 'cp.Doc.GetText("BlockingGroups")
                                        IsBlocked = Not genericController.IsGroupListMember(cp, GroupList)
                                    End If
                                    'End If
                                    ' Call Main.CloseCS(CSCat)
                                End If
                                'End If
                            End If
                        End If
                        If IsBlocked Then
                            '
                            '
                            '
                        Else

                            EntryID = blogEntry.id 'cp.Doc.GetInteger("ID")
                            Dim AuthorMemberID As Integer = blogEntry.AuthorMemberID ' cp.Doc.GetInteger("AuthorMemberID")
                            If AuthorMemberID = 0 Then
                                AuthorMemberID = blogEntry.CreatedBy 'cp.Doc.GetInteger("createdBy")
                            End If
                            Dim DateAdded As Date = blogEntry.DateAdded    'cp.Doc.GetText("DateAdded")
                            Dim EntryName As String = blogEntry.name  'cp.Doc.GetText("Name")
                            Dim entryEditLink As String

                            If IsEditing Then
                                entryEditLink = cp.Content.GetEditLink(EntryName, EntryID, True, EntryName, True)
                                ' entryEditLink = Main.GetCSRecordEditLink(CS)
                            End If
                            Dim EntryCopy As String = blogEntry.Copy   'cp.Doc.GetText("Copy")
                            Dim PodcastMediaLink As String = ""
                            'EntryCopyOverview = cp.Doc.GetText(CS, "CopyOverview")
                            Dim allowComments As Boolean = 'cp.Site.GetBoolean("AllowComments")
                                    PodcastMediaLink = blogEntry.PodcastMediaLink 'cp.Doc.GetText("PodcastMediaLink")
                            Dim PodcastSize As String = blogEntry.PodcastSize 'cp.Doc.GetText("PodcastSize")
                            'PodcastSize = Main.GetCS(CS, "PodcastSize")
                            Dim BlogTagList As String = blogEntry.TagList 'cp.Doc.GetText("BlogTagList")
                            Dim imageDisplayTypeId As Integer = blogEntry.imageDisplayTypeId 'cp.Doc.GetInteger("imageDisplayTypeId")
                            Dim primaryImagePositionId As Integer = blogEntry.primaryImagePositionId 'cp.Doc.GetInteger("primaryImagePositionId")
                            Dim articlePrimaryImagePositionId As Integer = blogEntry.articlePrimaryImagePositionId 'cp.Doc.GetInteger("articlePrimaryImagePositionId")
                            Dim Return_CommentCnt As Integer

                            result = result & GetBlogEntryCell(cp, EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, False, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                            result = result & cr & "<div Class=""aoBlogEntryDivider"">&nbsp;</div>"
                            EntryPtr = EntryPtr + 1
                        End If

                    Next

                    'Call cp.CSNew.GoNext()
                    'Loop
                End If
                Dim CategoryFooter As String = ""
                Dim qs As String
                '
                ' Build Footers
                '

                If cp.User.IsAdmin() And AllowCategories Then
                    qs = "cid=" & cp.Content.GetID("Blog Categories") & "&af=4"
                    CategoryFooter = CategoryFooter & cr & "<div Class=""aoBlogFooterLink""><a href=""" & cp.Site.GetProperty("ADMINURL") & "?" & qs & """>Add a New category</a></div>"
                End If
                Dim ReturnFooter As String = ""
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
                    Dim BlogCategorieModelList As List(Of Models.BlogCategorieModel) = Models.BlogCategorieModel.createList(cp, "id=" & TestCategoryID)
                    Dim blogCat As Models.BlogCategorieModel = BlogCategorieModelList.First

                    For Each blogCat In BlogCategorieModelList
                        BlogCategoryID = blogCat.id
                        IsBlocked = blogCat.UserBlocking
                        If IsBlocked Then
                            IsBlocked = Not cp.User.IsAdmin()
                        End If

                        If IsBlocked Then
                            If BuildVersion < "4.1.098" Then
                                GroupList = GroupModel.GetBlockingGroups(cp, BlogCategoryID)
                                IsBlocked = Not genericController.IsGroupListMember(cp, GroupList)
                            Else
                                GroupList = GroupModel.GetBlockingGroups(cp, BlogCategoryID)
                                IsBlocked = Not genericController.IsGroupListMember(cp, GroupList)
                            End If
                        End If
                        If Not IsBlocked Then
                            'qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogCategoryIDSet, CStr(BlogCategoryID), True)
                            Dim categoryLink As String = cp.Content.GetEditLink(qs, CStr(BlogCategoryID), True, "", True)
                            CategoryFooter = CategoryFooter & cr & "<div Class=""aoBlogFooterLink""><a href=""" & categoryLink & """>See posts In the category " & cp.Doc.GetText("name") & "</a></div>"
                        End If
                        ' Call Main.NextCSRecord(CS)
                    Next
                End If
                'Call Main.CloseCS(CS)
                '
                ' Footer
                '
                result = result & cr & "<div>&nbsp;</div>"

                If IsBlogOwner Then
                    Call cp.Site.TestPoint("Blogs.GetFormBlogPostList, IsBlogOwner = True, appending 'create' message")
                    '
                    ' Create a new entry if this is the Blog Owner
                    '
                    qs = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor, True)
                    result = result & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Create new post</a></div>"
                    '
                    ' Create a link to edit the blog record
                    '
                    qs = "cid=" & cp.Content.GetID("Blogs") & "&af=4&id=" & blogId
                    result = result & cr & "<div class=""aoBlogFooterLink""><a href=""" & cp.Site.GetProperty("adminUrl") & "?" & qs & """>Edit blog features</a></div>"
                    '
                    ' Create a link to edit the rss record
                    '
                    If RSSFeedId = 0 Then

                    Else
                        qs = "cid=" & cp.Content.GetID("RSS Feeds") & "&af=4&id=" & RSSFeedId
                        result = result & cr & "<div class=""aoBlogFooterLink""><a href=""" & cp.Site.GetProperty("adminUrl") & "?" & qs & """>Edit rss feed features</a></div>"
                    End If
                End If
                result = result & ReturnFooter
                result = result & CategoryFooter
                '
                ' Search
                '
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch, True)
                result = result & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                '
                ' Link to archives
                '
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchiveDateList, True)
                result = result & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>See archives</a></div>"
                '
                ' Link to RSS Feed
                '
                Dim FeedFooter As String = ""
                If RSSFeedFilename = "" Then
                    '
                Else
                    FeedFooter = "<a href=""http://" & cp.Site.Domain & "/RSS/" & RSSFeedFilename & """>"
                    FeedFooter = "rss feed " _
                    & FeedFooter & RSSFeedName & "</a>" _
                    & "&nbsp;" _
                    & FeedFooter & "<img src=""/cclib/images/IconXML-25x13.gif"" width=25 height=13 class=""aoBlogRSSFeedImage""></a>" _
                    & ""
                    result = result & cr & "<div class=""aoBlogFooterLink"">" & FeedFooter & "</div>"
                End If
                Dim CommentBlogID As Integer
                '

                result = result & cp.Html.Hidden(RequestNameBlogEntryID, CommentBlogID)
                result = result & cp.Html.Hidden(RequestNameSourceFormID, FormBlogPostList)
                'result = result & cp.Html.Form(result)
                '
                GetFormBlogPostList = result
                '

            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogPost(cp As CPBaseClass, blogId As Integer, BlogName As String, IsBlogOwner As Boolean, EntryID As Integer, AllowCategories As Boolean, BlogCategoryID As Integer, blogListLink As String) As String
            '
            Dim result As String = ""
            Try
                Dim hint As String = "1"
                result = "<table width=100% border=0 cellspacing=0 cellpadding=5 class=""aoBlogPostTable"">"
                Dim BlogCopy As String = ""
                Dim BlogTagList As String = ""
                Dim BlogTitle As String = ""
                '
                'AddBlogFlag = cp.doc.getBoolean(RequestNameAddBlogFlag)
                '
                If EntryID = 0 Then
                    hint = "2"
                    '
                    ' New Entry that is being saved
                    '
                    result = result & GetFormTableRow2(cp, cp.Content.GetCopy("Blog Create Header for " & BlogName, "<h2>Create a new blog post</h2>"))
                    BlogTitle = cp.Doc.GetText(RequestNameBlogTitle)
                    BlogCopy = cp.Doc.GetText(RequestNameBlogCopy)
                    BlogTagList = cp.Doc.GetText(RequestNameBlogTagList)
                Else
                    hint = "3"
                    '
                    ' Edit an entry
                    '
                    'CS = Main.OpenCSContentRecord(cnBlogEntries, EntryID)
                    Dim blogEntry As Models.BlogEntryModel = Models.BlogEntryModel.create(cp, EntryID)
                    If blogEntry.EntryID <> 0 Then
                        BlogTitle = blogEntry.name
                        BlogCopy = blogEntry.Copy
                        BlogCategoryID = blogEntry.blogCategoryID
                        BlogTagList = blogEntry.TagList
                        If BlogCopy = "" Then
                            BlogCopy = "<!-- cc --><p><br></p><!-- /cc -->"
                        End If
                    End If


                End If
                hint = "4"
                Dim editor As String = cp.Html.InputText(RequestNameBlogCopy, BlogCopy, 50, "100%", False)
                result = result & GetFormTableRow(cp, "<div style=""padding-top:3px"">Title: </div>", cp.Html.InputText(RequestNameBlogTitle, BlogTitle, 1, 70))
                result = result & GetFormTableRow(cp, "<div style=""padding-top:108px"">Post: </div>", editor)
                result = result & GetFormTableRow(cp, "<div style=""padding-top:3px"">Tag List: </div>", cp.Html.InputText(RequestNameBlogTagList, BlogTagList, 5, 70))
                If AllowCategories Then
                    hint = "5"
                    Dim CategorySelect As String = cp.Html.SelectContent(RequestNameBlogCategoryIDSet, BlogCategoryID, "Blog Categories")
                    If (InStr(1, CategorySelect, "<option value=""""></option></select>", vbTextCompare) <> 0) Then
                        '
                        ' Select is empty
                        '
                        CategorySelect = "<div>This blog has no categories defined</div>"
                    End If
                    result = result & GetFormTableRow(cp, "Category: ", CategorySelect)
                End If
                '
                ' file upload form taken from Resource Library
                '
                Dim c As String = ""
                c = c _
                            & "<TABLE id=""UploadInsert"" border=""0"" cellpadding=""0"" cellspacing=""1"" width=""100%"" class=""aoBlogImageTable"">" _
                            & "<tr>" _
                            & ""
                '

                'SQL = "select i.filename,i.description,i.name,i.id from BlogImages i left join BlogImageRules r on r.blogimageid=i.id where i.active<>0 and r.blogentryid=" & EntryID & " order by i.SortOrder"
                'CS = cp.CSNew.OpenSQL(SQL)
                Dim BlogImageModelList As List(Of Models.BlogImageModel) = Models.BlogImageModel.createListFromBlogEntry(cp, EntryID)
                Dim Ptr As Integer
                For Each BlogImage In BlogImageModelList
                    Dim imageFilename As String = BlogImage.Filename
                    Dim imageDescription As String = BlogImage.description
                    Dim imageName As String = BlogImage.name
                    Ptr = 1

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
                        & "<td align=""left"" class=""ccAdminSmall""><img alt=""" & imageName & """ title=""" & imageName & """ src=""" & cp.Site.PhysicalFilePath & imageFilename & """></TD>" _
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
                    Dim ImageID As Integer
                    '
                    '   description
                    '
                    c = c _
                        & "<tr>" _
                        & "<td>Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50"">" & cp.Utils.EncodeHTML(imageDescription) & "</TEXTAREA><input type=""hidden"" name=""" & rnBlogImageID & "." & Ptr & """ value=""" & ImageID & """></TD>" _
                        & "</tr>" _
                        & ""
                    '
                Next
                Ptr = Ptr + 1

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
                        & "</Table>" & cp.Html.Hidden("LibraryUploadCount", Ptr, "LibraryUploadCount") _
                        & ""
                '
                result = result & GetFormTableRow(cp, "Images: ", c)
                If EntryID <> 0 Then
                    hint = "6"
                    result = result & GetFormTableRow(cp, "", cp.Html.Button(rnButton, FormButtonPost) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonDelete))
                Else
                    hint = "7"
                    result = result & GetFormTableRow(cp, "", cp.Html.Button(rnButton, FormButtonPost) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel))
                End If
                hint = "8"
                Dim qs As String = cp.Doc.RefreshQueryString()
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
                hint = "9"
                result = result & vbCrLf & GetFormTableRow2(cp, "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>")

                '
                result = result & cp.Html.Hidden(RequestNameBlogEntryID, EntryID)
                result = result & cp.Html.Hidden(RequestNameSourceFormID, FormBlogEntryEditor)
                result = result & "</table>"
                hint = "95"
                result = cp.Html.Form(result)
                '
                ' GetFormBlogPost = result
                hint = "96"

                Call cp.Visit.SetProperty(SNBlogEntryName, CStr(GetRandomInteger(cp)))
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            '
            Return result
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function ProcessFormBlogPost(cp As CPBaseClass, SourceFormID As Integer, blogId As Integer, EntryID As Integer, ButtonValue As String, BlogCategoryID As Integer, RSSFeedId As Integer, blogListLink As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer) As Integer
            '
            Try
                Dim Copy As String = ""

                '
                ProcessFormBlogPost = SourceFormID
                Dim SN As String = cp.Visit.GetProperty(SNBlogEntryName)
                If SN <> "" Then
                    Call cp.Visit.SetProperty(SNBlogEntryName, "")
                    Dim UploadCount As Integer
                    If ButtonValue = FormButtonCancel Then
                        '
                        ' Cancel
                        '
                        ProcessFormBlogPost = FormBlogPostList
                    ElseIf ButtonValue = FormButtonPost Then
                        '
                        ' Post
                        '
                        Dim BlogEntry As Models.BlogEntryModel
                        If EntryID = 0 Then
                            BlogEntry = Models.BlogEntryModel.add(cp)
                            ' CS = Main.InsertCSRecord(cnBlogEntries)
                            EntryID = BlogEntry.id
                            BlogEntry.BlogID = blogId
                            'BlogEntry.save(cp)
                        End If
                        'CS = Main.OpenCSContent(cnBlogEntries, "(blogid=" & blogId & ")and(ID=" & EntryID & ")")
                        BlogEntry = Models.BlogEntryModel.create(cp, EntryID)
                        If (BlogEntry IsNot Nothing) Then
                            Dim EntryName As String = cp.Doc.GetText(RequestNameBlogTitle)
                            BlogEntry.name = EntryName
                            BlogEntry.Copy = cp.Doc.GetText(RequestNameBlogCopy)
                            BlogEntry.TagList = cp.Doc.GetText(RequestNameBlogTagList)
                            BlogEntry.blogCategoryID = BlogCategoryID
                            ProcessFormBlogPost = FormBlogPostList
                            BlogEntry.BlogID = blogId
                            BlogEntry.save(cp)
                        End If

                        Call UpdateBlogFeed(cp, blogId, RSSFeedId, EntryID, blogListLink)
                        ProcessFormBlogPost = FormBlogPostList
                        '
                        ' Upload files
                        '
                        UploadCount = cp.Doc.GetInteger("LibraryUploadCount")

                        If UploadCount > 0 Then
                            Dim UploadPointer As Integer
                            For UploadPointer = 1 To UploadCount
                                Dim ImageID As Integer = cp.Doc.GetInteger(rnBlogImageID & "." & UploadPointer)
                                Dim imageFilename As String = cp.Doc.GetText(rnBlogUploadPrefix & "." & UploadPointer)
                                Dim imageOrder As Integer = cp.Doc.GetInteger(rnBlogImageOrder & "." & UploadPointer)
                                Dim imageName As String = cp.Doc.GetText(rnBlogImageName & "." & UploadPointer)
                                Dim imageDescription As String = cp.Doc.GetText(rnBlogImageDescription & "." & UploadPointer)
                                Dim BlogImageID As Integer = 0
                                If ImageID <> 0 Then
                                    '
                                    ' edit image
                                    '
                                    Dim BlogImage As Models.BlogImageModel = Models.BlogImageModel.add(cp)
                                    If cp.Doc.GetBoolean(rnBlogImageDelete & "." & UploadPointer) Then
                                        Call cp.Content.Delete(cnBlogImages, ImageID)
                                    Else

                                        If (BlogImage IsNot Nothing) Then
                                            BlogImage.name = imageName
                                            BlogImage.description = imageDescription
                                            BlogImage.SortOrder = New String("0", 12 - imageOrder.ToString().Length) & imageOrder.ToString() ' String.Empty.PadLeft((12 - Len(imageOrder.ToString())), "0") & imageOrder
                                            BlogEntry.save(cp)
                                        End If
                                    End If
                                ElseIf imageFilename <> "" Then
                                    '
                                    ' upload image
                                    '
                                    'CS = Main.InsertCSRecord(cnBlogImages)
                                    'If Main.IsCSOK(CS) Then
                                    Dim BlogImage As Models.BlogImageModel = Models.BlogImageModel.add(cp)
                                    If (BlogImage IsNot Nothing) Then
                                        BlogImageID = BlogImage.id
                                        BlogImage.name = imageName
                                        BlogImage.description = imageDescription
                                        Dim FileExtension As String = ""
                                        Dim FilenameNoExtension As String = ""
                                        Dim Pos As Integer = InStrRev(imageFilename, ".")
                                        If Pos > 0 Then
                                            FileExtension = Mid(imageFilename, Pos + 1)
                                            FilenameNoExtension = Left(imageFilename, Pos - 1)
                                        End If

                                        'BlogImage.getUploadPath("filename")
                                        'Dim VirtualFilePathPage As String = cp.Doc.GetText(cnBlogImages, imageFilename)
                                        Dim VirtualFilePath As String = BlogImage.getUploadPath("filename")
                                        Call cp.Html.ProcessInputFile(rnBlogUploadPrefix & "." & UploadPointer, VirtualFilePath)
                                        BlogImage.Filename = VirtualFilePath & imageFilename
                                        BlogImage.save(cp)

                                        'If BuildVersion > "3.4.190" Then
                                        '
                                        ' add image resize values
                                        '
                                        Dim sf As Object = CreateObject("sfimageresize.imageresize")
                                        sf.Algorithm = 5

                                        sf.LoadFromFile(cp.Site.PhysicalFilePath & VirtualFilePath & imageFilename)
                                        If Err.Number = 0 Then
                                            Dim ImageWidth As Integer = sf.Width
                                            Dim ImageHeight As Integer = sf.Height
                                            Call cp.CSNew.SetField("height", ImageHeight)
                                            Call cp.CSNew.SetField("width", ImageWidth)
                                        Else
                                            Err.Clear()
                                        End If
                                        '
                                        sf = Nothing
                                        '                                Call Main.SetCS(CS, "AltSizeList", AltSizeList)
                                        BlogImage.SortOrder = New String("0", 12 - imageOrder.ToString().Length) & imageOrder.ToString()
                                        'Call Main.SetCS(CS, "sortorder", String(12 - Len(BlogImageID), "0") & BlogImageID)
                                        'End If
                                    End If
                                    '
                                    Dim ImageRule As Models.BlogImageRuleModel = Models.BlogImageRuleModel.add(cp)
                                    If (ImageRule IsNot Nothing) Then
                                        ImageRule.BlogEntryID = EntryID
                                        ImageRule.BlogImageID = BlogImageID
                                        ImageRule.save(cp)
                                    End If
                                    'CS = Main.InsertCSRecord(cnBlogImageRules)
                                    '    Call Main.SetCS(CS, "BlogEntryID", EntryID)
                                    '    Call Main.SetCS(CS, "BlogImageID", BlogImageID)
                                    '    Call Main.CloseCS(CS)
                                End If
                            Next
                        End If
                    ElseIf ButtonValue = FormButtonDelete Then
                        '
                        ' Delete
                        '
                        Models.BlogEntryModel.delete(cp, EntryID)
                        ProcessFormBlogPost = FormBlogPostList
                        Call UpdateBlogFeed(cp, blogId, RSSFeedId, EntryID, blogListLink)
                        ProcessFormBlogPost = FormBlogPostList
                    End If
                End If
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetFormBlogPostDetails(cp As CPBaseClass, blogId As Integer, EntryID As Integer, IsBlogOwner As Boolean, AllowAnonymous As Boolean, AllowCategories As Boolean, BlogCategoryID As Integer, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, blogListLink As String, blogListQs As String, allowCaptcha As Boolean) As String
            '
            Dim result As String
            Try
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
                BuildVersion = cp.Site.GetProperty("BuildVersion")
                '
                'Dim allowCaptcha As Boolean
                'Dim blogId As Integer

                Call cp.Site.TestPoint("blog -- getFormBlogPostDetails, enter ")
                Call cp.Site.TestPoint("blog -- getFormBlogPostDetails, allowCaptcha=[" & allowCaptcha & "]")
                ' s = s & vbCrLf & Main.GetFormStart
                '
                ' setup form key
                '
                formKey = "{" & Guid.NewGuid().ToString() & "}" ' cp.Utils.enc  Main.EncodeKeyNumber(Main.VisitID, Now())
                result = vbCrLf & cp.Html.Hidden("FormKey", formKey)
                'QSBack = cp.Doc.RefreshQueryString()
                'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameBlogEntryID, "", True)
                'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameFormID, FormBlogPostList)
                result = result & cr & "<div class=""aoBlogHeaderLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                '
                ' Print the Blog Entry
                '
                CommentCnt = 0

                'CS = Main.OpenCSContentRecord(cnBlogEntries, EntryID)
                'If Not Main.csok(CS) Then
                Dim blogEntry As Models.BlogEntryModel = Models.BlogEntryModel.create(cp, EntryID)
                If (blogEntry IsNot Nothing) Then

                    'Dim BlogEntry As Models.BlogEntryModel = blogEntryList.First
                    If Not (BlogEntry IsNot Nothing) Then
                        result = result & cr & "<div class=""aoBlogProblem"">Sorry, the blog post you selected is not currently available</div>"
                    Else
                        EntryID = BlogEntry.id
                        AuthorMemberID = BlogEntry.AuthorMemberID
                        If AuthorMemberID = 0 Then
                            AuthorMemberID = BlogEntry.CreatedBy 'cp.Doc.GetInteger(CS, "createdBy")
                        End If
                        DateAdded = BlogEntry.DateAdded 'cp.Doc.GetText(CS, "DateAdded")
                        EntryName = BlogEntry.name 'cp.Doc.GetText(CS, "Name")
                        If cp.User.IsAuthoring("Blogs") Then
                            entryEditLink = cp.Content.GetEditLink(EntryName, EntryID, True, EntryName, True) 'Main.GetCSRecordEditLink(CS)
                        End If
                        EntryCopy = BlogEntry.Copy 'cp.Doc.GetText(CS, "Copy")
                        'EntryCopyOverview = cp.Doc.GetText(CS, "copyOverview")
                        allowComments = blogEntry.AllowComments
                        PodcastMediaLink = BlogEntry.PodcastMediaLink 'Main.GetCS(CS, "PodcastMediaLink")
                        PodcastSize = BlogEntry.PodcastSize 'Main.GetCS(CS, "PodcastSize")
                        BlogTagList = BlogEntry.TagList ' Main.GetCS(CS, "TagList")
                        qs = ""
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                        ' Call cp.Site.addLinkAlias(BlogEntry.name, cp.Doc.PageId, qs)

                        'Call Main.SetCS(CS, "viewings", 1 + cp.Doc.GetInteger(CS, "viewings"))
                        BlogEntry.Viewings = (1 + cp.Doc.GetInteger("viewings"))
                        BlogEntry.imageDisplayTypeId = cp.Doc.GetInteger("imageDisplayTypeId")
                        BlogEntry.primaryImagePositionId = cp.Doc.GetInteger("primaryImagePositionId")
                        BlogEntry.articlePrimaryImagePositionId = cp.Doc.GetInteger("articlePrimaryImagePositionId")
                        'BlogEntry.save(cp)
                        result = result & GetBlogEntryCell(cp, EntryPtr, IsBlogOwner, EntryID, EntryName, EntryCopy, DateAdded, True, False, Return_CommentCnt, allowComments, PodcastMediaLink, PodcastSize, entryEditLink, ThumbnailImageWidth, BuildVersion, ImageWidthMax, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, AuthorMemberID)
                        EntryPtr = EntryPtr + 1
                        '
                        blogId = cp.Doc.GetInteger("BlogID")
                    End If

                    '
                End If
                'Call Main.CloseCS(CS)


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
                Dim criteria As String = ""
                Dim VisitModel As New Models.VisitModel
                Dim excludeFromAnalytics As Boolean = VisitModel.ExcludeFromAnalytics

                If BuildVersion >= "4.1.161" Then
                    If (Not excludeFromAnalytics) Then
                        'BlogViewingLogModel
                        Dim BlogViewingLog As Models.BlogViewingLogModel = Models.BlogViewingLogModel.add(cp)
                        'CS = Main.InsertCSContent("Blog Viewing Log")
                        If (BlogViewingLog IsNot Nothing) Then
                            BlogViewingLog.name = cp.User.Name & ", post " & CStr(EntryID) & ", " & Now()
                            BlogViewingLog.BlogEntryID = EntryID
                            BlogViewingLog.MemberID = cp.User.Id
                            BlogViewingLog.VisitID = cp.Visit.Id
                            BlogViewingLog.save(cp)
                        End If
                        'If Main.IsCSOK(CS) Then
                        '        Call Main.SetCS(CS, "Name", Main.MemberName & ", post " & CStr(EntryID) & ", " & Now())
                        '        Call Main.SetCS(CS, "BlogEntryID", EntryID)
                        '        Call Main.SetCS(CS, "MemberID", cp.User.Id)
                        '        Call Main.SetCS(CS, "VisitID", Main.VisitID)
                        '    End If
                        '    Call Main.CloseCS(CS)
                    End If
                End If

                '
                '
                '
                If IsBlogOwner And (Return_CommentCnt > 0) Then
                    result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.Button(FormButtonApplyCommentChanges) & "</div>"
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
                If allowComments And (cp.Visit.CookieSupport) And (Not VisitModel.Bot()) Then
                    result = result & cr & "<div class=""aoBlogCommentHeader"">Post a Comment</div>"
                    '
                    If Not (cp.UserError.OK()) Then
                        result = result & "<div class=""aoBlogCommentError"">" & (cp.UserError.OK()) & "</div>"
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
                                result = result _
                                        & "<div class=""aoBlogLoginBox"">" _
                                        & vbCrLf & vbTab & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                                        & vbCrLf & vbTab & "<div class=""aoBlogCommentCopy"">" & "send password form removed" & "</div>" _
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
                                result = result _
                                        & "<div class=""aoBlogLoginBox"">" _
                                        & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                                        & cr & "<div class=""aoBlogCommentCopy"">" & "Send join form removed" & "</div>" _
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
                                loginForm = "Get login Form removed"
                                loginForm = Replace(loginForm, "LoginUsernameInput", "LoginUsernameInput-BlockFocus")
                                result = result _
                                        & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                                        & cr & "<div class=""aoBlogLoginBox"">" _
                                        & cr & "<div class=""aoBlogCommentCopy"">" & loginForm & "</div>" _
                                        & cr & "</div>"
                        End Select

                    Else
                        result = result & cr & "<div>&nbsp;</div>"
                        result = result & cr & "<div class=""aoBlogCommentCopy"">Title</div>"
                        If RetryCommentPost Then
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & GetField(cp, RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(RequestNameCommentTitle.ToString)) & "</div>"
                            result = result & cr & "<div>&nbsp;</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">Comment</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.InputText(RequestNameCommentCopy, cp.Doc.GetText(RequestNameCommentCopy), 15, 70,) & "</div>"
                        Else
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & GetField(cp, RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(RequestNameCommentTitle.ToString)) & "</div>"
                            result = result & cr & "<div>&nbsp;</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">Comment</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.InputText(RequestNameCommentCopy, "", 15, 70) & "</div>"
                        End If
                        's = s & cr & "<div class=""aoBlogCommentCopy"">Verify Text</div>"
                        '
                        If allowCaptcha Then
                            result = result & cr & "<div class=""aoBlogCommentCopy"">Verify Text</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Utils.ExecuteAddon(reCaptchaDisplayGuid) & "</div>"
                            Call cp.Site.TestPoint("output - reCaptchaDisplayGuid")
                        End If
                        '
                        result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.Button(rnButton, FormButtonPostComment) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel) & "</div>"
                    End If

                End If

                result = result & cr & "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                '
                ' edit link
                '
                If IsBlogOwner Then
                    qs = cp.Doc.RefreshQueryString()
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor)
                    result = result & cr & "<div class=""aoBlogToolLink""><a href=""?" & qs & """>Edit</a></div>"
                End If
                '
                ' Search
                '
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch, True)
                result = result & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                '
                ' back to recent posts
                '
                'QSBack = cp.Doc.RefreshQueryString()
                'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameBlogEntryID, "", True)
                'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameFormID, FormBlogPostList)
                result = result & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                '
                result = result & vbCrLf & cp.Html.Hidden(RequestNameSourceFormID, FormBlogPostDetails)
                result = result & vbCrLf & cp.Html.Hidden(RequestNameBlogEntryID, EntryID)
                result = result & vbCrLf & cp.Html.Hidden("EntryCnt", EntryPtr)
                'result = result & "</div>"
                GetFormBlogPostDetails = result
                result = cp.Html.Form(GetFormBlogPostDetails)
                '
                '
                Try
                    Call cp.Visit.SetProperty(SNBlogCommentName, CStr(GetRandomInteger(cp)))
                Catch ex As Exception

                End Try

                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function ProcessFormBlogPostDetails(cp As CPBaseClass, SourceFormID As Integer, blogId As Integer, IsBlogOwner As Boolean, ButtonValue As String, BlogName As String, BlogOwnerID As Integer, AllowAnonymous As Boolean, BlogCategoryID As Integer, autoApproveComments As Boolean, authoringGroupId As Integer, emailComment As Boolean, OptionString As String, allowCaptcha As Boolean) As Integer
            '
            Try
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
                Dim blog As Models.blogModel = Models.blogModel.create(cp, blogId)
                'Dim allowCaptcha As Boolean
                '
                ProcessFormBlogPostDetails = SourceFormID
                SN = cp.Visit.GetProperty(SNBlogCommentName)
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
                            optionStr = "Challenge=" + cp.Doc.GetText("recaptcha_challenge_field")
                            optionStr = optionStr & "&Response=" + cp.Doc.GetText("recaptcha_response_field")
                            Dim WrapperId As Integer = Nothing
                            captchaResponse = cp.Utils.ExecuteAddon(reCaptchaProcessGuid)
                            Call cp.Site.TestPoint("output - reCaptchaProcessGuid, result=" & captchaResponse)
                            If captchaResponse <> "" Then
                                Call cp.UserError.Add("The verify text you entered did not match correctly. Please try again.")
                                Call cp.Utils.AppendLogFile("testpoint1")
                            End If
                        End If
                        '
                        ' Process comment post
                        '
                        RetryCommentPost = True
                        formKey = cp.Doc.GetText("formkey")
                        'formKey = Main.DecodeKeyNumber(formKey)
                        Copy = cp.Doc.GetText(RequestNameCommentCopy)
                        If Copy <> "" Then
                            Call cp.Site.TestPoint("blog -- adding comment [" & Copy & "]")
                            'If (Main.VisitID <> kmaEncodeInteger(Main.DecodeKeyNumber(formKey))) Then
                            '    Call Main.AddUserError("<p>This comment has already been accepted.</p>")
                            'End If
                            Dim BlogCommentModelList As List(Of Models.BlogCommentModel) = Models.BlogCommentModel.createList(cp, "(formkey=" & cp.Db.EncodeSQLText(formKey) & ")", "ID")
                            If (BlogCommentModelList.Count <> 0) Then
                                Call cp.UserError.Add("<p>This comment has already been accepted.</p>")
                                RetryCommentPost = False
                                Call cp.Utils.AppendLogFile("testpoint2")
                            Else
                                Call cp.Site.TestPoint("blog -- adding comment, no user error")
                                EntryID = cp.Doc.GetInteger(RequestNameBlogEntryID)
                                Dim BlogComment As Models.BlogCommentModel = Models.BlogCommentModel.add(cp)
                                'CSP = Main.InsertCSRecord(cnBlogComments)
                                If AllowAnonymous And (Not cp.User.IsAuthenticated) Then
                                    MemberID = 0
                                    MemberName = AnonymousMemberName
                                Else
                                    MemberID = cp.User.Id
                                    MemberName = cp.User.Name
                                End If
                                cp.Utils.AppendLog("subscript test", "test1")
                                BlogComment.BlogID = blogId
                                BlogComment.Active = 1
                                BlogComment.name = cp.Doc.GetText(RequestNameCommentTitle)
                                BlogComment.CopyText = Copy
                                BlogComment.EntryID = EntryID
                                BlogComment.Approved = IsBlogOwner Or autoApproveComments
                                BlogComment.FormKey = formKey
                                BlogComment.save(cp)
                                CommentID = BlogComment.id
                                RetryCommentPost = False
                                '
                                cp.Utils.AppendLog("subscript test", "test2")
                                If (emailComment) Then
                                    'If (Not IsBlogOwner) And (emailComment) Then
                                    '
                                    ' Send Comment Notification
                                    '
                                    Dim blogEntryList As List(Of Models.BlogEntryModel) = Models.BlogEntryModel.createList(cp, "id=" & EntryID)
                                    If (blogEntryList.Count <> 0) Then
                                        Dim BlogEntry As Models.BlogEntryModel = blogEntryList.First
                                        EntryName = BlogEntry.name
                                        EntryLink = BlogEntry.RSSLink 'Main.ServerLink
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
                                                    & cr & "By " & cp.User.Name _
                                                    & vbCrLf _
                                                    & vbCrLf & cp.Utils.EncodeHTML(Copy) _
                                                    & vbCrLf
                                        EmailFromAddress = cp.Site.GetProperty("EmailFromAddress", "info@" & cp.Site.Domain)
                                        'Call Main.SendMemberEmail(BlogOwnerID, EmailFromAddress, "Blog comment notification for [" & BlogName & "]", EmailBody, False, False)
                                        Call cp.Email.sendUser(BlogOwnerID, EmailFromAddress, "Blog comment notification for [" & BlogName & "]", EmailBody, False, False)
                                        authoringGroupId = blog.AuthoringGroupID
                                        If authoringGroupId <> 0 Then
                                            Dim MemberRuleList As List(Of Models.MemberRuleModel) = Models.MemberRuleModel.createList(cp, "GroupId=" & authoringGroupId)
                                            For Each MemberRule In MemberRuleList
                                                Call cp.Email.sendUser(MemberRule.MemberID, EmailFromAddress, "Blog comment on " & BlogName, EmailBody, False, False)
                                            Next
                                            'authoringGroup = cp.Group.GetName(authoringGroupId)
                                            'If authoringGroup <> "" Then
                                            '        Dim blogModelList As List(Of Models.blogModel) = Models.blogModel.createList(cp, "(allowbulkemail<>0)and(email<>'')")
                                            '        For Each blogModel In blogModelList
                                            '            AuthoringMemberId = cp.Doc.GetInteger(CS, "id")
                                            '            Call cp.Email.sendUser(AuthoringMemberId, EmailFromAddress, "Blog comment on " & BlogName, EmailBody, False, False)
                                            '        Next
                                            'CS = Main.OpenCSGroupMembers(authoringGroup, "(allowbulkemail<>0)and(email<>'')")
                                            'Do While Main.IsCSOK(CS)
                                            '    AuthoringMemberId = cp.Doc.GetInteger(CS, "id")
                                            '    Call cp.Email.sendUser(AuthoringMemberId, EmailFromAddress, "Blog comment on " & BlogName, EmailBody, False, False)
                                            '    Call Main.NextCSRecord(CS)
                                            'Loop
                                            'Call Main.CloseCS(CS)
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
                            EntryCnt = cp.Doc.GetInteger("EntryCnt")
                            If EntryCnt > 0 Then
                                For EntryPtr = 0 To EntryCnt - 1
                                    CommentCnt = cp.Doc.GetInteger("CommentCnt" & EntryPtr)
                                    If CommentCnt > 0 Then
                                        For CommentPtr = 0 To CommentCnt - 1
                                            Suffix = EntryPtr & "." & CommentPtr
                                            CommentID = cp.Doc.GetInteger("CommentID" & Suffix)
                                            If cp.Doc.GetBoolean("Delete" & Suffix) Then
                                                '
                                                ' Delete comment
                                                '
                                                Call cp.Content.Delete("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blogId & ")")
                                            ElseIf cp.Doc.GetBoolean("Approve" & Suffix) And Not cp.Doc.GetBoolean("Approved" & Suffix) Then
                                                '
                                                ' Approve Comment
                                                '
                                                Dim BlogCommentModelList As List(Of Models.BlogCommentModel) = Models.BlogCommentModel.createList(cp, "(name=" & cp.Utils.EncodeQueryString(BlogName) & ")", "ID")
                                                If (BlogCommentModelList.Count > 0) Then
                                                    ' CS = Main.OpenCSContent("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blogId & ")")
                                                    Dim BlogComment As Models.BlogCommentModel = Models.BlogCommentModel.add(cp)
                                                    If cp.CSNew.OK() Then
                                                        BlogComment.Approved = True
                                                        ' Call Main.SetCS(CS, "Approved", True)
                                                    End If
                                                    'Call Main.CloseCS(CS)
                                                ElseIf Not cp.Doc.GetBoolean("Approve" & Suffix) And cp.Doc.GetBoolean("Approved" & Suffix) Then
                                                    '
                                                    ' Unapprove comment
                                                    '
                                                    'CS = Main.OpenCSContent("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blogId & ")")
                                                    Dim BlogComment As Models.BlogCommentModel = Models.BlogCommentModel.add(cp)
                                                    If (BlogComment IsNot Nothing) Then
                                                        'Call Main.SetCS(CS, "Approved", True)
                                                        BlogComment.Approved = False
                                                        'Call Main.SetCS(CS, "Approved", False)
                                                    End If
                                                    'Call Main.CloseCS(CS)
                                                End If
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
                    Call cp.Visit.SetProperty(SNBlogCommentName, "")
                End If
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function
        '
        '====================================================================================
        '
        '====================================================================================
        '
        Private Function GetBlogCommentCell(cp As CPBaseClass, IsBlogOwner As Boolean, DateAdded As Date, Approved As Boolean, CommentName As String, CommentCopy As String, CommentID As Integer, EntryPtr As Integer, CommentPtr As Integer, AuthorMemberID As Integer, EntryID As Integer, IsSearchListing As Boolean, EntryName As String) As String
            '
            Dim result As String = ""
            Try
                Dim Copy As String

                Dim RowCopy As String
                Dim RequestSuffix As String
                Dim qs As String
                Dim userPtr As Integer
                Dim authorMemberName As String
                '
                userPtr = getUserPtr(cp, AuthorMemberID)
                authorMemberName = ""
                If userPtr > 0 Then
                    authorMemberName = users(userPtr).Name
                End If
                If IsSearchListing Then
                    qs = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList)
                    result = result & cr & "<div class=""aoBlogEntryName"">Comment to Blog Post " & EntryName & ", <a href=""?" & qs & """>View this post</a></div>"
                    result = result & cr & "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                End If
                result = result & cr & "<div class=""aoBlogCommentName"">" & cp.Utils.EncodeHTML(CommentName) & "</div>"
                Copy = CommentCopy
                Copy = cp.Utils.EncodeHTML(Copy)
                Copy = Replace(Copy, vbCrLf, "<BR />")
                result = result & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>"
                RowCopy = ""
                If authorMemberName <> "" Then
                    RowCopy = RowCopy & "by " & cp.Utils.EncodeHTML(authorMemberName)
                    If DateAdded <> Date.MinValue Then
                        RowCopy = RowCopy & " | " & DateAdded
                    End If
                Else
                    If DateAdded <> Date.MinValue Then
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
                        & cp.Html.Hidden("CommentID" & RequestSuffix, CommentID) _
                        & cp.Html.CheckBox("Approve" & RequestSuffix, Approved) _
                        & cp.Html.Hidden("Approved" & RequestSuffix, Approved) _
                        & "&nbsp;Approved&nbsp;" _
                        & " | " _
                        & cp.Html.CheckBox("Delete" & RequestSuffix, False) _
                        & "&nbsp;Delete" _
                        & ""
                End If
                If RowCopy <> "" Then
                    result = result & cr & "<div class=""aoBlogCommentByLine"">Posted " & RowCopy & "</div>"
                End If
                '
                If (Not Approved) And (Not IsBlogOwner) And (AuthorMemberID = cp.User.Id) Then
                    result = "<div style=""border:1px solid red;padding-top:10px;padding-bottom:10px;""><span class=""aoBlogCommentName"" style=""color:red;"">Your comment pending approval</span><br />" & result & "</div>"
                End If
                '
                GetBlogCommentCell = result
                'GetBlogCommentCell = cr & "<div class=""aoBlogComment"">" & s & cr & "</div>"
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        Private Function GetBlogEntryCell(cp As CPBaseClass, EntryPtr As Integer, IsBlogOwner As Boolean, EntryID As Integer, EntryName As String, EntryCopy As String, DateAdded As Date, DisplayFullEntry As Boolean, IsSearchListing As Boolean, Return_CommentCnt As Integer, allowComments As Boolean, PodcastMediaLink As String, PodcastSize As String, entryEditLink As String, ThumbnailImageWidth As Integer, BuildVersion As String, ImageWidthMax As Integer, BlogTagList As String, imageDisplayTypeId As Integer, primaryImagePositionId As Integer, articlePrimaryImagePositionId As Integer, blogListQs As String, AuthorMemberID As Integer) As String
            '
            Dim result As String
            Try
                Dim hint As String
                Dim visit As Models.VisitModel = Models.VisitModel.create(cp, cp.Visit.Id)

                '
                hint = "enter"
                Dim authorMemberName As String = ""
                Dim userPtr As Integer = getUserPtr(cp, AuthorMemberID)
                If userPtr > 0 Then
                    authorMemberName = users(userPtr).Name
                End If
                Dim qs As String = blogListQs
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                Dim EntryLink As String = ""
                If BuildVersion > "4.1.160" Then
                    EntryLink = getLinkAlias(cp, "?" & qs)
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
                Dim imageIDList As String = ""
                'SQL = "select i.id from BlogImages i,BlogImageRules r where r.BlogImageID=i.id and i.active<>0 and r.blogentryid=" & EntryID & " order by i.SortOrder"
                'CS = Main.OpenCSSQL("default", SQL)
                Dim imageDescription As String = ""
                Dim imageName As String = ""
                Dim ThumbnailFilename As String = ""
                Dim imageFilename As String = ""
                Dim BlogImageModelList As List(Of Models.BlogImageModel) = Models.BlogImageModel.createListFromBlogEntry(cp, EntryID)
                For Each BlogImage In BlogImageModelList
                    imageIDList = imageIDList & "," & BlogImage.id
                    imageDescription = BlogImage.description
                    imageName = BlogImage.name
                    ThumbnailFilename = BlogImage.Filename
                    imageFilename = BlogImage.Filename

                Next

                'Do While Main.IsCSOK(CS)
                '    imageIDList = imageIDList & "," & cp.Doc.GetText(CS, "id")
                '    Call Main.NextCSRecord(CS)
                'Loop
                'Call Main.CloseCS(CS)
                '
                ' Get Thumbnail filename
                '
                hint = hint & ",2"

                Dim ImageID() As String
                If imageIDList <> "" Then
                    imageIDList = Mid(imageIDList, 2)
                    ImageID = Split(imageIDList, ",")
                    Call GetBlogImage(cp, cp.Utils.EncodeInteger(ImageID(0)), ThumbnailImageWidth, 0, ThumbnailFilename, imageFilename, imageDescription, imageName)
                End If
                '
                result = ""
                hint = hint & ",3"
                Dim TagListRow As String
                If DisplayFullEntry Then
                    ' added page title with meta content in dotnet blog wrapper
                    'Call Main.AddPageTitle(EntryName)
                    result = result & vbCrLf & entryEditLink & "<h2 class=""aoBlogEntryName"">" & EntryName & "</h2>"
                    result = result & cr & "<div class=""aoBlogEntryCopy"">"
                    If ThumbnailFilename <> "" Then
                        Select Case articlePrimaryImagePositionId
                            Case 2
                                '
                                ' align right
                                '
                                result = result & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;"">"
                            Case 3
                                '
                                ' align left
                                '
                                result = result & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;"">"
                            Case 4
                                '
                                ' hide
                                '
                            Case Else
                                '
                                ' 1 and none align per stylesheet
                                '
                                result = result & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;"">"
                        End Select
                    End If
                    hint = hint & ",4"
                    result = result & EntryCopy & "</div>"
                    qs = blogListQs
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor)
                    Dim c As String = ""
                    If (imageDisplayTypeId = imageDisplayTypeList) And (imageIDList <> "") Then
                        c = ""
                        Dim cnt As Integer
                        For cnt = 0 To UBound(ImageID)
                            Call GetBlogImage(cp, cp.Utils.EncodeInteger(ImageID(cnt)), 0, ImageWidthMax, ThumbnailFilename, imageFilename, imageDescription, imageName)
                            If imageFilename <> "" Then
                                c = c _
                                & cr & "<div class=""aoBlogEntryImageContainer"">" _
                                & cr & "<img alt=""" & imageName & """ title=""" & imageName & """  src=""" & cp.Site.FilePath & imageFilename & """>"
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
                            result = result _
                            & cr & "<div class=""aoBlogEntryImageSection"">" _
                            & cp.Html.Indent(c) _
                            & cr & "</div>"
                        End If
                    End If
                    hint = hint & ",5"
                    If BlogTagList <> "" Then
                        BlogTagList = Replace(BlogTagList, ",", vbCrLf)
                        Dim Tags() As String = Split(BlogTagList, vbCrLf)
                        BlogTagList = ""
                        Dim SQS As String
                        SQS = cp.Utils.ModifyQueryString(blogListQs, RequestNameFormID, FormBlogSearch, True)
                        Dim Ptr As Integer
                        For Ptr = 0 To UBound(Tags)
                            'QS = cp.Utils.ModifyQueryString(SQS, RequestNameFormID, FormBlogSearch, True)
                            qs = cp.Utils.ModifyQueryString(SQS, RequestNameQueryTag, Tags(Ptr), True)
                            Dim Link As String = "?" & qs
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
                    result = result & vbCrLf & entryEditLink & "<h2 class=""aoBlogEntryName""><a href=""" & EntryLink & """>" & EntryName & "</a></h2>"
                    result = result & cr & "<div class=""aoBlogEntryCopy"">"
                    If ThumbnailFilename <> "" Then
                        Select Case primaryImagePositionId
                            Case 2
                                '
                                ' align right
                                '
                                result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;""></a>"
                            Case 3
                                '
                                ' align left
                                '
                                result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;""></a>"
                            Case 4
                                '
                                ' hide
                                '
                            Case Else
                                '
                                ' 1 and none align per stylesheet
                                '
                                result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & ThumbnailImageWidth & "px;""></a>"
                        End Select
                    End If
                    result = result & "<p>" & filterCopy(cp, EntryCopy, OverviewLength) & "</p></div>"
                    result = result & cr & "<div class=""aoBlogEntryReadMore""><a href=""" & EntryLink & """>Read More</a></div>"
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
                    Dim OptionString As String = "" _
                    '& "&Media Link=" & cp.MyAddon.ArgumentList(PodcastMediaLink) _
                    '& "&Media Size=" & cp.MyAddon.ArgumentList(PodcastSize) _
                    '& "&Hide Player=true" _
                    '& "&Auto Start=false" _
                    '& ""
                    cp.Doc.SetProperty("Media Link", PodcastMediaLink)
                    cp.Doc.SetProperty("Media Link", PodcastSize)
                    cp.Doc.SetProperty("Hide Player", "True")
                    cp.Doc.SetProperty("Auto Start", "False")
                    '
                    result = result & cp.Utils.ExecuteAddon("{F6037DEE-023C-4A14-A972-ADAFA5538240}")
                End If
                '
                ' Author Row
                '
                hint = hint & ",8"
                Dim RowCopy As String = ""
                If authorMemberName <> "" Then
                    RowCopy = RowCopy & "By " & authorMemberName
                    If DateAdded <> Date.MinValue Then
                        RowCopy = RowCopy & " | " & DateAdded
                    End If
                Else
                    If DateAdded <> Date.MinValue Then
                        RowCopy = RowCopy & DateAdded
                    End If
                End If
                hint = hint & ",9"
                Dim CommentCount As Integer
                Dim Criteria As String
                If allowComments And (cp.Visit.CookieSupport) And (Not visit.Bot()) Then
                    'If allowComments Then
                    '
                    ' Show comment count
                    '
                    Criteria = "(Approved<>0)and(EntryID=" & EntryID & ")"
                    ' CSCount = Main.OpenCSContent(cnBlogComments, "(Approved<>0)and(EntryID=" & EntryID & ")")
                    Dim BlogCommentModelList As List(Of Models.BlogCommentModel) = Models.BlogCommentModel.createList(cp, "(Approved<>0)and(EntryID=" & EntryID & ")")
                    CommentCount = BlogCommentModelList.Count
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
                    result = result & cr & "<div class=""aoBlogEntryByLine"">Posted " & RowCopy & "</div>"
                End If
                '
                ' Tag List Row
                '
                If TagListRow <> "" Then
                    result = result & TagListRow
                End If
                ''
                'If Main.IsLinkAuthoring(cnBlogEntries) Then
                '    s = s & vbCrLf & Main.GetAdminHintWrapper("Blog Details: <a href=""?" & QS & """>Click here</a>")
                'End If
                '
                hint = hint & ",10"
                Dim ToolLine As String = ""
                Dim CommentPtr As Integer
                If allowComments And (cp.Visit.CookieSupport) And (Not visit.Bot) Then


                    'If allowComments Then
                    If Not DisplayFullEntry Then
                        ''
                        '' Show comment count
                        ''
                        Criteria = "(Approved<>0)and(EntryID=" & EntryID & ")"
                        Dim BlogCommentModelList As List(Of Models.BlogCommentModel) = Models.BlogCommentModel.createList(cp, "(Approved<>0)and(EntryID=" & EntryID & ")")
                        CommentCount = BlogCommentModelList.Count
                        'CSCount = Main.OpenCSContent(cnBlogComments, "(Approved<>0)and(EntryID=" & EntryID & ")")
                        'CommentCount = Main.GetCSRowCount(CSCount)
                        'Call Main.CloseCS(CSCount)
                        '
                        qs = blogListQs
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(EntryID))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                        Dim CommentLine As String = ""
                        If CommentCount = 0 Then
                            CommentLine = CommentLine & "<a href=""?" & qs & """>Comment</a>"
                        Else
                            CommentLine = CommentLine & "<a href=""?" & qs & """>Comments</a>&nbsp;(" & CommentCount & ")"
                        End If
                        If IsBlogOwner Then
                            Criteria = "(EntryID=" & EntryID & ")"
                            'CSCount = Main.OpenCSContent(cnBlogComments, "((Approved is null)or(Approved=0))and(EntryID=" & EntryID & ")")
                            BlogCommentModelList = Models.BlogCommentModel.createList(cp, "(Approved<>0)and(EntryID=" & EntryID & ")")
                            CommentCount = BlogCommentModelList.Count
                            'Call Main.CloseCS(CSCount)
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
                        Dim BlogCommentModelList As List(Of Models.BlogCommentModel) = Models.BlogCommentModel.createList(cp, Criteria, "DateAdded")
                        If (BlogCommentModelList.Count > 0) Then
                            Dim Divider As String = "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                            result = result & cr & "<div class=""aoBlogCommentHeader"">Comments</div>"
                            result = result & vbCrLf & Divider
                            CommentPtr = 0
                            Dim blogComment As Models.BlogCommentModel = Models.BlogCommentModel.add(cp)
                            If (blogComment IsNot Nothing) Then
                                For Each blogComment In Models.BlogCommentModel.createList(cp, Criteria)

                                    result = result & GetBlogCommentCell(cp, IsBlogOwner, blogComment.DateAdded, blogComment.Approved, blogComment.name, blogComment.CopyText, blogComment.id, EntryPtr, CommentPtr, AuthorMemberID, EntryID, False, EntryName)
                                    result = result & vbCrLf & Divider
                                    CommentPtr = CommentPtr + 1
                                    'Call Main.NextCSRecord(CS)
                                Next
                            End If

                        End If

                    End If
                End If
                '
                hint = hint & ",12"
                If ToolLine <> "" Then
                    result = result & cr & "<div class=""aoBlogToolLink"">" & ToolLine & "</div>"
                End If
                result = result & vbCrLf & cp.Html.Hidden("CommentCnt" & EntryPtr, CommentPtr)
                '
                Return_CommentCnt = CommentPtr
                GetBlogEntryCell = result
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try

            Return result

        End Function
        '
        '
        '
        Private Function AddMonth(cp As CPBaseClass, StartDate As Date, Months As Integer) As Date
            '
            Try
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
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function

        Private Function filterCopy(cp As CPBaseClass, rawCopy As String, MaxLength As Integer) As String
            Try
                Dim Copy As String
                'Dim objHTML As New kmaHTML.DecodeClass

                Copy = rawCopy
                If Len(Copy) > MaxLength Then
                    Copy = Left(Copy, MaxLength)
                    Copy = Copy & "..."
                End If
                filterCopy = Copy
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function
        '
        '========================================================================
        '   Update the Blog Feed
        '       run on every post
        '========================================================================
        '
        Private Sub UpdateBlogFeed(cp As CPBaseClass, blogId As Integer, RSSFeedId As Integer, EntryID As Integer, blogListLink As String)
            '
            Try
                Dim RSSTitle As String
                Dim EntryCopy As String
                Dim EntryLink As String
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
                AdminURL = cp.Site.GetProperty("adminUrl")
                'call VerifyFeedReturnArgs(BlogID, )
                If RSSFeedId <> 0 Then
                    '
                    ' Gather all the current rules
                    '
                    Dim RSSFeedBlogRuleList As List(Of Models.RSSFeedBlogRuleModel) = Models.RSSFeedBlogRuleModel.createList(cp, "RSSFeedID=" & RSSFeedId, "id,BlogPostID")
                    RuleCnt = 0
                    For Each RSSFeedBlogRule In RSSFeedBlogRuleList
                        If RuleCnt >= RuleSize Then
                            RuleSize = RuleSize + 10
                            ReDim Preserve RuleIDs(RuleSize)
                            ReDim Preserve RuleBlogPostIDs(RuleSize)
                        End If
                        RuleIDs(RuleCnt) = RSSFeedBlogRule.id 'Csv.GetCSInteger(CS, "ID")
                        RuleBlogPostIDs(RuleCnt) = RSSFeedBlogRule.BlogPostID 'Csv.GetCSInteger(CS, "BlogPostID")
                        RuleCnt = RuleCnt + 1
                    Next

                    Dim BlogCopyModelList As List(Of Models.BlogCopyModel) = Models.BlogCopyModel.createListFromBlogCopy(cp, blogId)
                    For Each BlogCopy In BlogCopyModelList
                        BlogPostID = BlogCopy.id 'Csv.GetCSInteger(CS, "id")
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
                            Dim RSSFeedBlogRule As Models.RSSFeedBlogRuleModel = Models.RSSFeedBlogRuleModel.add(cp)
                            If (RSSFeedBlogRule IsNot Nothing) Then
                                RSSFeedBlogRule.RSSFeedID = RSSFeedId
                                RSSFeedBlogRule.BlogPostID = BlogPostID
                                RSSFeedBlogRule.save(cp)
                            End If
                            'CSRule = Csv.InsertCSRecord(cnRSSFeedBlogRules, 0)
                            'If Csv.IsCSOK(CSRule) Then

                            'Call Csv.SetCS(CSRule, "RSSFeedID", RSSFeedId)
                            'Call Csv.SetCS(CSRule, "BlogPostID", BlogPostID)
                            'End If
                            'Call Csv.CloseCS(CSRule)
                            Dim BlogEntry As Models.BlogEntryModel = Models.BlogEntryModel.add(cp)
                            If (BlogEntry IsNot Nothing) Then
                                EntryName = BlogEntry.name 'Csv.GetCSText(CSPost, "name")
                                EntryID = BlogEntry.id 'Csv.GetCSInteger(CSPost, "id")
                                EntryCopy = BlogEntry.Copy   'Csv.GetCSText(CSPost, "copy")
                                RSSTitle = Trim(EntryName)
                                If RSSTitle = "" Then
                                    RSSTitle = "Blog Post " & EntryID
                                End If
                                BlogEntry.RSSTitle = RSSTitle
                                'Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
                                '
                                qs = ""
                                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                                EntryLink = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, qs, blogListLink & "?" & qs)
                                If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                                    'Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                                    BlogEntry.RSSLink = EntryLink
                                End If
                                BlogEntry.RSSDescription = filterCopy(cp, EntryCopy, 150)
                                ' Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
                            End If
                            ' CSPost = Csv.OpenCSContentRecord(cnBlogEntries, BlogPostID)
                            'If Csv.IsCSOK(CSPost) Then
                            '            EntryName = Csv.GetCSText(CSPost, "name")
                            '            EntryID = Csv.GetCSInteger(CSPost, "id")
                            '            EntryCopy = Csv.GetCSText(CSPost, "copy")
                            '            '
                            '            RSSTitle = Trim(EntryName)
                            '            If RSSTitle = "" Then
                            '                RSSTitle = "Blog Post " & EntryID
                            '            End If
                            '            Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
                            '            '
                            '            qs = ""
                            '            qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                            '            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                            '            EntryLink = Main.GetLinkAliasByPageID(cp.Doc.PageId, qs, blogListLink & "?" & qs)
                            '            If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                            '                Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                            '            End If
                            '            Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
                        End If

                        ' Call Csv.CloseCS(CSPost)
                        '
                    Next
                    For RulePtr = 0 To RuleCnt - 1
                        If RuleIDs(RulePtr) <> -1 Then
                            'Call Csv.DeleteContentRecord(cnRSSFeedBlogRules, RuleIDs(RulePtr))
                            Call cp.Content.Delete(cnRSSFeedBlogRules, RuleIDs(RulePtr))
                        End If
                    Next
                End If
                '
                Dim BuildVersion As String = cp.Site.GetProperty("BuildVersion")
                If BuildVersion >= "4.1.098" Then
                    Call cp.Utils.ExecuteAddon(RSSProcessAddonGuid)
                End If
                'Do While Csv.IsCSOK(CS)
                '    BlogPostID = Csv.GetCSInteger(CS, "id")
                '    For RulePtr = 0 To RuleCnt - 1
                '        If BlogPostID = RuleBlogPostIDs(RulePtr) Then
                '            RuleIDs(RulePtr) = -1
                '            Exit For
                '        End If
                '    Next
                'If RulePtr >= RuleCnt Then
                '        '
                '        ' Rule not found, add it
                '        '
                '        CSRule = Csv.InsertCSRecord(cnRSSFeedBlogRules, 0)
                '        If Csv.IsCSOK(CSRule) Then
                '            Call Csv.SetCS(CSRule, "RSSFeedID", RSSFeedId)
                '            Call Csv.SetCS(CSRule, "BlogPostID", BlogPostID)
                '        End If
                '        Call Csv.CloseCS(CSRule)
                '    End If
                '
                ' Now update the Blog Post RSS fields, RSSLink, RSSTitle, RSSDescription, RSSPublish, RSSExpire
                ' Should do this here because if RSS was installed after Blog, there is no link until a post is edited
                '
                'CSPost = Csv.OpenCSContentRecord(cnBlogEntries, BlogPostID)
                '    If Csv.IsCSOK(CSPost) Then
                '        EntryName = Csv.GetCSText(CSPost, "name")
                '        EntryID = Csv.GetCSInteger(CSPost, "id")
                '        EntryCopy = Csv.GetCSText(CSPost, "copy")
                '        '
                '        RSSTitle = Trim(EntryName)
                '        If RSSTitle = "" Then
                '            RSSTitle = "Blog Post " & EntryID
                '        End If
                '        Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
                '        '
                '        qs = ""
                '        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                '        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                '        EntryLink = Main.GetLinkAliasByPageID(cp.Doc.PageId, qs, blogListLink & "?" & qs)
                '        If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                '            Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                '        End If
                '        Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
                '    End If
                '    Call Csv.CloseCS(CSPost)
                '    '
                '    Call Csv.NextCSRecord(CS)
                'Loop
                'Call Csv.CloseCS(CS)
                '
                ' Now delete all the rules that were not found in the blog
                '
                'For RulePtr = 0 To RuleCnt - 1
                '        If RuleIDs(RulePtr) <> -1 Then
                '            Call Csv.DeleteContentRecord(cnRSSFeedBlogRules, RuleIDs(RulePtr))
                '        End If
                '    Next
                'End If
                ''
                'If Main.ContentServerVersion >= "4.1.098" Then
                '    Call cp.Utils.ExecuteAddon(RSSProcessAddonGuid)
                'End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Sub
        '
        '========================================================================
        '   Verify the RSSFeed and return the ID and the Link
        '       run when a new blog is created
        '       run on every post
        '========================================================================
        '
        Private Sub VerifyFeedReturnArgs(cp As CPBaseClass, blogId As Integer, blogListLink As String, ByRef Return_RSSFeedID As Integer, ByRef Return_RSSFeedName As String, ByRef Return_RSSFeedFilename As String)
            '
            Try
                Dim qs As String
                Dim CSBlog As Integer
                Dim BlogName As String = cp.Utils.EncodeQueryString("BlogName")
                Dim blogDescription As String
                Dim CSFeed As Integer
                Dim rssLink As String
                '
                Dim blog As Models.blogModel = Models.blogModel.create(cp, BlogName)
                ' Dim blogList As List(Of Models.blogModel) = Models.blogModel.createList(cp, "(name=" & cp.Utils.EncodeQueryString(BlogName) & ")", "ID")
                If Not (blog Is Nothing) Then
                    ' Dim blog As Models.blogModel = blogList.First
                    BlogName = blog.name 'Csv.GetCSText(CSBlog, "name")
                    blogDescription = Trim(blog.Copy)
                    Return_RSSFeedID = blog.RSSFeedID 'Csv.GetCSInteger(CSBlog, "RSSFeedID")
                    If Trim(BlogName) = "" Then
                        Return_RSSFeedName = "Feed for Blog " & blog.id
                    Else
                        Return_RSSFeedName = BlogName
                    End If
                    blog.save(cp)
                    Dim RssFeed As Models.RSSFeedModel = Models.RSSFeedModel.create(cp, Return_RSSFeedID)
                    If Return_RSSFeedID <> 0 Then
                        '
                        ' Make sure the record exists
                        '
                        If Not cnRSSFeeds Then
                            Return_RSSFeedID = 0
                            'CSFeed = Main.OpenCSContentRecord("RSS Feeds", Return_RSSFeedID)
                            'If Not Main.IsCSOK(CSFeed) Then
                            '    Return_RSSFeedID = 0
                            'End If
                        End If
                        '
                        RssFeed = Models.RSSFeedModel.add(cp)
                        Return_RSSFeedID = cp.Doc.GetInteger(CSFeed, "ID")
                        RssFeed.id = Return_RSSFeedID
                        RssFeed.name = Return_RSSFeedName
                        If blogDescription = "" Then
                            blogDescription = Trim(Return_RSSFeedName)
                        End If
                        RssFeed.Description = blogDescription

                        Return_RSSFeedFilename = cp.Db.EncodeSQLText(Return_RSSFeedName) & ".xml"
                        RssFeed.RSSFilename = Return_RSSFeedFilename
                        RssFeed.save(cp)
                    End If
                    RssFeed = Models.RSSFeedModel.create(cp, Return_RSSFeedID)
                    RssFeed = Models.RSSFeedModel.add(cp)
                    Return_RSSFeedName = RssFeed.name
                    Return_RSSFeedID = RssFeed.id 'cp.Doc.GetInteger("id")
                    Return_RSSFeedFilename = Trim(RssFeed.RSSFilename)
                    If Trim(RssFeed.Link) = "" Then
                        rssLink = cp.Request.Protocol & cp.Request.Host & blogListLink
                        RssFeed.Link = rssLink

                    End If
                    'If Main.IsCSOK(CSFeed) Then
                    '    '
                    '    ' Manage the Feed name, title and description
                    '    '   because it is associated to this blog'
                    '    '   only reset the link if it is blank (see desc at top of class)
                    '    '   only manage the RSSFeedFilename if it is blank
                    '    '
                    '    Return_RSSFeedName = rssfeed.  'cp.Doc.GetText(CSFeed, "name")
                    '    Return_RSSFeedID = cp.Doc.GetInteger(CSFeed, "id")
                    '    Return_RSSFeedFilename = Trim(Main.GetCS(CSFeed, "rssfilename"))
                    '    If Trim(Main.GetCS(CSFeed, "link")) = "" Then
                    '        rssLink = Main.serverProtocol & Main.ServerHost & blogListLink
                    '        Call Main.SetCS(CSFeed, "link", rssLink)
                    '    End If
                    '    'Return_BlogRootLink = Trim(Main.GetCS(CSFeed, "link"))
                    '    'If Return_BlogRootLink = "" Then
                    '    '    '
                    '    '    ' set blog link to current link without forms/categories
                    '    '    '   exclude admin
                    '    '    '   exclude a post
                    '    '    '
                    '    '    Return_BlogRootLink = Main.ServerLink
                    '    '    If (InStr(1, Return_BlogRootLink, "admin", vbTextCompare) = 0) And (Main.ServerForm = "") Then
                    '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameFormID, "", False)
                    '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameSourceFormID, "", False)
                    '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryID, "", False)
                    '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryIDSet, "", False)
                    '    '        Call Main.SetCS(CSFeed, "link", Return_BlogRootLink)
                    '    '    End If
                    '    'End If
                    'End If
                    ' Call Main.CloseCS(CSFeed)
                End If
                ' Call Csv.CloseCS(CSBlog)
                'CSBlog = Csv.OpenCSContent(cnBlogs, "ID=" & blogId)
                'If Csv.IsCSOK(CSBlog) Then
                '    BlogName = Csv.GetCSText(CSBlog, "name")
                '    blogDescription = Trim(Csv.GetCS(CSBlog, "copy"))
                '    Return_RSSFeedID = Csv.GetCSInteger(CSBlog, "RSSFeedID")
                '    If Trim(BlogName) = "" Then
                '        Return_RSSFeedName = "Feed for Blog " & blogId
                '    Else
                '        Return_RSSFeedName = BlogName
                '    End If
                '    If Return_RSSFeedID <> 0 Then
                '        '
                '        ' Make sure the record exists
                '        '
                '        CSFeed = Main.OpenCSContentRecord("RSS Feeds", Return_RSSFeedID)
                '        If Not Main.IsCSOK(CSFeed) Then
                '            Return_RSSFeedID = 0
                '        End If
                '    End If
                '    If Return_RSSFeedID = 0 Then
                '
                ' new blog was created, now create new feed
                '   set name and description from the blog
                '
                '        CSFeed = Main.InsertCSContent(cnRSSFeeds)
                '    If Main.IsCSOK(CSFeed) Then
                '        Return_RSSFeedID = cp.Doc.GetInteger(CSFeed, "ID")
                '        Call Main.SetCS(CSBlog, "RSSFeedID", Return_RSSFeedID)
                '        Call Main.SetCS(CSFeed, "Name", Return_RSSFeedName)
                '        If blogDescription = "" Then
                '            blogDescription = Trim(Return_RSSFeedName)
                '        End If
                '        Call Main.SetCS(CSFeed, "description", blogDescription)
                '        Return_RSSFeedFilename = encodeFilename(Return_RSSFeedName) & ".xml"
                '        Call Main.SetCS(CSFeed, "rssfilename", Return_RSSFeedFilename)
                '    End If
                'End If
                'If Main.IsCSOK(CSFeed) Then
                '    '
                '    ' Manage the Feed name, title and description
                '    '   because it is associated to this blog'
                '    '   only reset the link if it is blank (see desc at top of class)
                '    '   only manage the RSSFeedFilename if it is blank
                '    '
                '    Return_RSSFeedName = cp.Doc.GetText(CSFeed, "name")
                '    Return_RSSFeedID = cp.Doc.GetInteger(CSFeed, "id")
                '    Return_RSSFeedFilename = Trim(Main.GetCS(CSFeed, "rssfilename"))
                '    If Trim(Main.GetCS(CSFeed, "link")) = "" Then
                '        rssLink = Main.serverProtocol & Main.ServerHost & blogListLink
                '        Call Main.SetCS(CSFeed, "link", rssLink)
                '    End If
                '    'Return_BlogRootLink = Trim(Main.GetCS(CSFeed, "link"))
                '    'If Return_BlogRootLink = "" Then
                '    '    '
                '    '    ' set blog link to current link without forms/categories
                '    '    '   exclude admin
                '    '    '   exclude a post
                '    '    '
                '    '    Return_BlogRootLink = Main.ServerLink
                '    '    If (InStr(1, Return_BlogRootLink, "admin", vbTextCompare) = 0) And (Main.ServerForm = "") Then
                '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameFormID, "", False)
                '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameSourceFormID, "", False)
                '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryID, "", False)
                '    '        Return_BlogRootLink = cp.Utils.ModifyQueryString(Return_BlogRootLink, RequestNameBlogCategoryIDSet, "", False)
                '    '        Call Main.SetCS(CSFeed, "link", Return_BlogRootLink)
                '    '    End If
                '    'End If
                'End If
                'Call Main.CloseCS(CSFeed)
                'End If
                'Call Csv.CloseCS(CSBlog)
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try

        End Sub
        '
        '
        '
        Private Function GetBlogImage(cp As CPBaseClass, BlogImageID As Integer, ThumbnailImageWidth As Integer, ImageWidthMax As Integer, ByRef Return_ThumbnailFilename As String, ByRef Return_ImageFilename As String, ByRef Return_ImageDescription As String, ByRef Return_Imagename As String) As String
            '
            Dim results As String = ""
            Try
                '
                'CS = Main.OpenCSContentRecord(cnBlogImages, BlogImageID, , , "name,description,filename,altsizelist")
                'If Main.IsCSOK(CS) Then
                cp.Utils.AppendLogFile("entering GetBlogImage")
                Dim BlogImage As Models.BlogImageModel = Models.BlogImageModel.create(cp, BlogImageID)
                If (BlogImage IsNot Nothing) Then
                    ' Dim blogImage As Models.BlogImageModel = blogImage.First

                    Dim Filename As String = BlogImage.Filename
                    Dim AltSizeList As String = BlogImage.AltSizeList
                    Return_ImageDescription = BlogImage.description
                    Return_Imagename = BlogImage.name
                    '
                    Return_ThumbnailFilename = Filename
                    Return_ImageFilename = Filename
                    cp.Utils.AppendLogFile("filename=" & Filename)

                    Dim ThumbNailSize As String
                    Dim ImageSize As String
                    'Filename = BlogImageList.
                    '    AltSizeList = cp.Doc.GetText(CS, "AltSizeList")
                    '    Return_ImageDescription = cp.Doc.GetText(CS, "description")
                    '    Return_Imagename = cp.Doc.GetText(CS, "name")
                    '    '
                    '    Return_ThumbnailFilename = Filename
                    '    Return_ImageFilename = Filename
                    '
                    If AltSizeList <> "" Then
                        AltSizeList = Replace(AltSizeList, ",", vbCrLf)
                        Dim Sizes() As String = Split(AltSizeList, vbCrLf)
                        Dim Ptr As Integer
                        For Ptr = 0 To UBound(Sizes)
                            If Sizes(Ptr) <> "" Then
                                Dim Dims() As String = Split(Sizes(Ptr), "x")
                                Dim DimWidth As Integer = cp.Utils.EncodeInteger(Dims(0))
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
                        Dim Pos As Integer = InStrRev(Filename, ".")
                        If Pos <> 0 Then
                            Dim FilenameNoExtension As String = Mid(Filename, 1, Pos - 1)
                            Dim FilenameExtension As String = Mid(Filename, Pos + 1)
                            Dim sf As Object
                            If ThumbnailImageWidth = 0 Then
                                '
                            ElseIf ThumbNailSize <> "" Then
                                Return_ThumbnailFilename = FilenameNoExtension & "-" & ThumbNailSize & "." & FilenameExtension
                                cp.Utils.AppendLogFile("Return_ThumbnailFilename=" & Return_ThumbnailFilename)
                            Else
                                sf = CreateObject("sfimageresize.imageresize")
                                sf.Algorithm = 5
                                Try
                                    sf.LoadFromFile(cp.Site.PhysicalFilePath & Filename)
                                    ' On Error GoTo ErrorTrap
                                    sf.Width = ThumbnailImageWidth
                                    Call sf.DoResize
                                    ThumbNailSize = ThumbnailImageWidth & "x" & sf.Height
                                    Return_ThumbnailFilename = FilenameNoExtension & "-" & ThumbNailSize & "." & FilenameExtension
                                    Call sf.SaveToFile(cp.Site.PhysicalFilePath & Return_ThumbnailFilename)
                                    AltSizeList = AltSizeList & vbCrLf & ThumbNailSize
                                    BlogImage.AltSizeList = AltSizeList
                                    'Call Main.SetCS(CS, "altsizelist", AltSizeList)
                                    cp.Utils.AppendLogFile("Return_ThumbnailFilename=" & Return_ThumbnailFilename)
                                Catch ex As Exception
                                    '
                                End Try
                            End If
                            If ImageWidthMax = 0 Then
                                '
                            ElseIf ImageSize <> "" Then
                                Return_ImageFilename = FilenameNoExtension & "-" & ImageSize & "." & FilenameExtension
                            Else
                                sf = CreateObject("sfimageresize.imageresize")
                                sf.Algorithm = 5

                                sf.LoadFromFile(cp.Site.PhysicalFilePath & Filename)
                                If Err.Number <> 0 Then
                                    ' On Error GoTo ErrorTrap
                                    Err.Clear()
                                Else
                                    ' On Error GoTo ErrorTrap
                                    sf.Width = ImageWidthMax
                                    Call sf.DoResize
                                    ImageSize = ImageWidthMax & "x" & sf.Height
                                    Return_ImageFilename = FilenameNoExtension & "-" & ImageSize & "." & FilenameExtension
                                    Call sf.SaveToFile(cp.Site.FilePath & Return_ImageFilename)
                                    AltSizeList = AltSizeList & vbCrLf & ImageSize
                                    BlogImage.AltSizeList = AltSizeList
                                    'Call Main.SetCS(CS, "altsizelist", AltSizeList)
                                End If
                            End If
                        End If
                    End If
                End If
                cp.Utils.AppendLogFile("exiting GetBlogImage")

                'End If
                'Call Main.CloseCS(CS)
                '
                Exit Function
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return results
        End Function
        '
        '
        '
        Private Function getLinkAlias(cp As CPBaseClass, sourceLink As String) As String
            '
            Dim result As String = ""
            Try
                '
                'Call Main.SaveVirtualFile("blog.log", "getLinkAlias(" & sourceLink & ")")
                '
                result = sourceLink
                If cp.Utils.EncodeBoolean(cp.Site.GetProperty("allowLinkAlias", "1")) Then
                    Dim Link As String = sourceLink
                    '
                    Dim pageQs() As String = Split(LCase(Link), "?")
                    If UBound(pageQs) > 0 Then
                        Dim nameValues() As String = Split(pageQs(1), "&")
                        Dim cnt As Integer = UBound(nameValues) + 1
                        If UBound(nameValues) < 0 Then
                        Else
                            Dim qs As String = ""
                            Dim Ptr As Integer
                            Dim pageId As Integer
                            For Ptr = 0 To cnt - 1
                                Dim NameValue As String = nameValues(Ptr)
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
                                result = cp.Content.GetLinkAliasByPageID(pageId, qs, sourceLink)
                            End If
                        End If
                    End If
                End If
                getLinkAlias = result
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Private Function getUserPtr(cp As CPBaseClass, userid As Integer) As Integer
            '
            Try
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
                    Dim PeopleModelList As List(Of Models.PeopleModel) = Models.PeopleModel.createList(cp, "id=" & userid)
                    Dim user As Models.PeopleModel = PeopleModelList.First
                    'CS = Main.OpenCSContent("people", "id=" & userid)
                    'If Main.IsCSOK(CS) Then
                    userPtr = userCnt
                    userCnt = userCnt + 1
                    ReDim Preserve users(userCnt)
                    users(userPtr).Id = userid
                    users(userPtr).Name = user.id
                    users(userPtr).authorInfoLink = ""
                    If allowAuthorInfoLink Then
                        users(userPtr).authorInfoLink = cp.Doc.GetText("authorInfoLink")
                    End If
                End If
                getUserPtr = userPtr
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function
        '
        Public Function GetRandomInteger(cp As CPBaseClass) As Long
            '
            Try
                Dim RandomBase As Long
                Dim RandomLimit As Long
                '
                RandomBase = App.ThreadID
                RandomBase = RandomBase And ((2 ^ 30) - 1)
                RandomLimit = (2 ^ 31) - RandomBase - 1
                Randomize()
                GetRandomInteger = RandomBase + (Rnd() * RandomLimit)
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function
    End Class
End Namespace
