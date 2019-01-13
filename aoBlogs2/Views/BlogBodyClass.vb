
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
    Public Class BlogBodyClass
        Inherits AddonBaseClass
        ' todo - remove globals
        Const StreamUpgradeTimeout = 1800
        '
        Private ErrorString As String
        Private RetryCommentPost As Boolean                 ' when true, the comment post page prepopulates with the previous comment post. set by process comment
        Private OverviewLength As Integer
        Private PostsToDisplay As Integer
        Private users() As UserCacheStruct
        Private userCnt As Integer
        '
        Private ReadOnly App As Object
        '
        Public Class UserCacheStruct
            Public Id As Integer
            Public Name As String
            Public authorInfoLink As String
        End Class
        '
        '========================================================================
        ''' <summary>
        ''' Addon interface, if needed (this is the old legacy blog so this will likely not be used)
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <returns></returns>
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                Dim instanceId As String = CP.Doc.GetText("instanceId")
                Dim blog As BlogModel = BlogModel.verifyBlog(CP, Controllers.InstanceIdController.getInstanceId(CP))
                If (blog Is Nothing) Then Return "<!-- Could not find or create blog from instanceId [" & instanceId & "] -->"
                returnHtml = getBlogBody(CP, blog)
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Inner Blog - the list of posts without sidebar
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="blog"></param>
        ''' <returns></returns>
        Public Function getBlogBody(cp As CPBaseClass, blog As BlogModel) As String
            Dim result As String = ""
            Try
                Dim blogListQs As String = cp.Doc.RefreshQueryString()
                blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, "")
                blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameFormID, "")
                blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameBlogCategoryID, "")
                blogListQs = cp.Utils.ModifyQueryString(blogListQs, RequestNameBlogEntryID, "")
                Dim blogListLink As String = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, "", "") & "?" & blogListQs
                Dim isBlogEditor As Boolean = (cp.User.IsAuthenticated And (cp.User.Id = blog.OwnerMemberID)) OrElse cp.User.IsAdmin()
                If Not isBlogEditor Then
                    If blog.AuthoringGroupID <> 0 Then
                        Dim authoringGroup As String = cp.Content.GetRecordName("Groups", blog.AuthoringGroupID)
                        If authoringGroup <> "" Then
                            isBlogEditor = isBlogEditor Or cp.User.IsInGroup(authoringGroup, cp.User.Id)
                        End If
                    End If
                End If
                '
                Dim BlogCategoryID As Integer
                If blog.AllowCategories Then
                    If (cp.Doc.GetText(RequestNameBlogCategoryIDSet) <> "") Then
                        BlogCategoryID = cp.Doc.GetInteger(RequestNameBlogCategoryIDSet)
                    Else
                        BlogCategoryID = cp.Doc.GetInteger(RequestNameBlogCategoryID)
                    End If
                    Call cp.Doc.AddRefreshQueryString(RequestNameBlogCategoryID, BlogCategoryID.ToString())
                End If
                '
                If isBlogEditor And (blog.id = 0) Then
                    '
                    ' Blog record was not, or can not be created
                    '
                    result = result & cp.Html.adminHint("This blog has not been configured. Please edit this page and edit the properties for the blog Add-on")
                Else
                    '
                    ' Get the Feed Args
                    '
                    Dim RSSFeed As RSSFeedModel = DbModel.create(Of RSSFeedModel)(cp, blog.RSSFeedID)
                    If (RSSFeed Is Nothing) Then
                        RSSFeed = DbModel.add(Of RSSFeedModel)(cp)
                        RSSFeed.name = blog.Caption
                        RSSFeed.description = "This is your First RssFeed"
                        RSSFeed.save(Of BlogModel)(cp)
                        blog.RSSFeedID = RSSFeed.id
                        blog.save(Of BlogModel)(cp)
                    End If
                    '
                    ' Process Input
                    Dim ButtonValue As String = cp.Doc.GetText("button")
                    Dim FormID As Integer = cp.Doc.GetInteger(RequestNameFormID)
                    Dim SourceFormID As Integer = cp.Doc.GetInteger(RequestNameSourceFormID)
                    Dim KeywordList As String = cp.Doc.GetText(RequestNameKeywordList)
                    Dim DateSearchText As String = cp.Doc.GetText(RequestNameDateSearch)
                    Dim ArchiveMonth As Integer = cp.Doc.GetInteger(RequestNameArchiveMonth)
                    Dim ArchiveYear As Integer = cp.Doc.GetInteger(RequestNameArchiveYear)
                    Dim EntryID As Integer = cp.Doc.GetInteger(RequestNameBlogEntryID)
                    Dim blogEntry = DbModel.create(Of BlogEntryModel)(cp, EntryID)
                    Dim isBlogAuthor As Boolean = (cp.User.IsAuthenticated) And ((cp.User.Id = blog.CreatedBy) Or (cp.User.IsInGroupList(blog.AuthoringGroupID.ToString())))
                    '
                    If ButtonValue <> "" Then
                        Dim OptionString As String = ""
                        '
                        ' Process the source form into form if there was a button - else keep formid
                        '
                        FormID = ProcessForm(cp, blog, RSSFeed, blogEntry, SourceFormID, ButtonValue, BlogCategoryID, blogListLink, isBlogAuthor)
                    End If
                    '
                    ' -- Get Next Form
                    result = result & GetForm(cp, blog, RSSFeed, blogEntry, FormID, ArchiveMonth, ArchiveYear, KeywordList, ButtonValue, DateSearchText, BlogCategoryID, blogListLink, blogListQs, isBlogAuthor)
                End If
                getBlogBody = result
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================
        '
        Private Function GetForm(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, FormID As Integer, ArchiveMonth As Integer, ArchiveYear As Integer, KeywordList As String, ButtonValue As String, DateSearchText As String, BlogCategoryID As Integer, blogListLink As String, blogListQs As String, isBlogAuthor As Boolean) As String
            Dim Stream As String = ""
            Try
                Select Case FormID
                    Case FormBlogPostDetails
                        Stream = Stream & GetFormBlogPostDetails(cp, blog, rssFeed, blogEntry, isBlogAuthor, BlogCategoryID, blogListLink, blogListQs)
                    Case FormBlogArchiveDateList
                        Stream = Stream & GetFormBlogArchiveDateList(cp, blog, rssFeed, blogEntry, BlogCategoryID, blogListLink, blogListQs, isBlogAuthor)
                    Case FormBlogArchivedBlogs
                        Stream = Stream & GetFormBlogArchivedBlogs(cp, blog, rssFeed, blogEntry, ArchiveMonth, ArchiveYear, BlogCategoryID, blogListLink, blogListQs, isBlogAuthor)
                    Case FormBlogEntryEditor
                        Stream = Stream & GetFormBlogPost(cp, blog, rssFeed, blogEntry, BlogCategoryID, blogListLink)
                    Case FormBlogSearch
                        Stream = Stream & GetFormBlogSearch(cp, blog, rssFeed, blogEntry, KeywordList, ButtonValue, DateSearchText, BlogCategoryID, blogListLink, blogListQs, isBlogAuthor)
                    Case Else
                        If (blogEntry IsNot Nothing) Then
                            '
                            ' Go to details page
                            '
                            FormID = FormBlogPostDetails
                            Stream = Stream & GetFormBlogPostDetails(cp, blog, rssFeed, blogEntry, isBlogAuthor, BlogCategoryID, blogListLink, blogListQs)
                        Else
                            '
                            ' list all the entries
                            '
                            FormID = FormBlogPostList
                            Stream = Stream & GetFormBlogPostList(cp, blog, rssFeed, BlogCategoryID, blogListLink, blogListQs, isBlogAuthor)
                        End If
                End Select
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return Stream
        End Function
        '
        '====================================================================================
        '
        Private Function ProcessForm(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, SourceFormID As Integer, ButtonValue As String, BlogCategoryID As Integer, blogListLink As String, isBlogAuthor As Boolean) As Integer
            '
            Try
                If ButtonValue <> "" Then
                    Select Case SourceFormID
                        Case FormBlogPostList
                            ProcessForm = FormBlogPostList
                        Case FormBlogEntryEditor
                            ProcessForm = ProcessFormBlogPost(cp, blog, rssFeed, SourceFormID, ButtonValue, BlogCategoryID, blogListLink)
                        Case FormBlogPostDetails
                            ProcessForm = ProcessFormBlogPostDetails(cp, blog, rssFeed, SourceFormID, ButtonValue, BlogCategoryID, isBlogAuthor)
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
        Private Function GetFormBlogSearch(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, KeywordList As String, ButtonValue As String, DateSearchText As String, BlogCategoryID As Integer, blogListLink As String, blogListQs As String, isBlogAuthor As Boolean) As String
            '
            Dim result As String = ""
            Try
                '
                Call cp.Doc.AddRefreshQueryString(RequestNameBlogEntryID, "")
                '
                result = result & vbCrLf & cp.Content.GetCopy("Blogs Search Header for " & blog.name, "<h2>" & blog.name & " Blog Search</h2>")
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
                    If DateSearchText <> Date.MinValue.ToString Then
                        DateSearch = CDate(DateSearchText)
                        If DateSearch < CDate("1/1/2000") Then
                            DateSearch = Date.MinValue
                            Call cp.Site.ErrorReport("The date is not valid")
                        End If
                    End If
                    '
                    Dim Subcaption As String = ""
                    Dim subCriteria As String
                    Dim Criteria As String = "(blog.id=" & blog.id & ")"
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
                        Dim SearchMonth As Integer = Month(DateSearch)
                        Dim SearchYear As Integer = Year(DateSearch)
                        Subcaption = Subcaption & " in " & SearchMonth & "/" & SearchYear
                        If Criteria <> "" Then
                            Criteria = Criteria & "AND"
                        End If
                        Criteria = Criteria & "(DateAdded>=" & cp.Db.EncodeSQLDate(DateSearch) & ")and(DateAdded<" & cp.Db.EncodeSQLDate(DateSearch.AddMonths(1)) & ")"
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
                    Dim BlogCopyList As New List(Of BlogCopyModel)
                    If Criteria <> "" Then
                        BlogCopyList = DbModel.createList(Of BlogCopyModel)(cp, Criteria)
                        cp.Utils.AppendLog("search list criteria=" & Criteria)
                    End If
                    '
                    ' Display the results
                    '
                    result = result & cr & "<div class=""aoBlogEntryCopy"">" & Subcaption & "</div>"
                    '
                    If (BlogCopyList.Count = 0) Then
                        cp.Utils.AppendLog("search list count" & BlogCopyList.Count)
                        result = result & "</br>" & "<div class=""aoBlogProblem"">There were no matches to your search</div>"
                    Else
                        cp.Utils.AppendLog("search list count" & BlogCopyList.Count)
                        result = result & "</br>" & "<div class=""aoBlogEntryDivider"">&nbsp;</div>"
                        For Each blogSearchEntry In DbModel.createList(Of BlogEntryModel)(cp, Criteria)
                            Dim AuthorMemberID As Integer
                            Dim ResultPtr As Integer
                            '
                            ' Entry
                            '
                            AuthorMemberID = blogSearchEntry.AuthorMemberID
                            If AuthorMemberID = 0 Then
                                AuthorMemberID = blogSearchEntry.CreatedBy
                            End If
                            Dim Return_CommentCnt As Integer
                            result = result & GetBlogEntryCell(cp, blog, rssFeed, blogSearchEntry, False, True, Return_CommentCnt, "", blogEntry.imageDisplayTypeId, blogEntry.primaryImagePositionId, blogEntry.primaryImagePositionId, blogListQs, isBlogAuthor)
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
        Private Function GetFormBlogArchiveDateList(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, BlogCategoryID As Integer, blogListLink As String, blogListQs As String, isBlogAuthor As Boolean) As String
            '
            Dim result As String = ""
            Try
                Dim OpenSQL As String = ""
                Dim contentControlId As String = cp.Content.GetID(cnBlogEntries).ToString
                '
                cp.Utils.AppendLog("entering GetFormBlogArchiveDateList")
                result = vbCrLf & cp.Content.GetCopy("Blogs Archives Header for " & blog.name, "<h2>" & blog.name & " Blog Archive</h2>")
                ' 
                'Dim BlogCopyModelList As List(Of BlogCopyModel) = BlogCopyModel.createList(cp, "(blogid=" & blog.id & ")", "year(dateadded) desc, Month(DateAdded) desc")
                Dim archiveDateList As List(Of BlogCopyModel.ArchiveDateModel) = BlogCopyModel.createArchiveListFromBlogCopy(cp, blog.id)
                cp.Utils.AppendLog("open archive date list")
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
                        cp.Utils.AppendLog("display one archive count 1")
                        '
                        ArchiveMonth = archiveDateList.First.Month
                        ArchiveYear = archiveDateList.First.Year
                        result = result & GetFormBlogArchivedBlogs(cp, blog, rssFeed, blogEntry, ArchiveMonth, ArchiveYear, BlogCategoryID, blogListLink, blogListQs, isBlogAuthor)
                        cp.Utils.AppendLog("after display one archive count 1")
                    Else
                        '
                        cp.Utils.AppendLog("enter display list of archive")
                        ' Display List of archive
                        '
                        Dim qs As String = cp.Utils.ModifyQueryString(blogListQs, RequestNameSourceFormID, FormBlogArchiveDateList.ToString())
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchivedBlogs.ToString())
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
                        cp.Utils.AppendLog("exit display list of archive")
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
        Private Function GetFormBlogArchivedBlogs(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, ArchiveMonth As Integer, ArchiveYear As Integer, BlogCategoryID As Integer, blogListLink As String, blogListQs As String, isBlogAuthor As Boolean) As String
            '
            Dim result As String = ""
            Try
                Dim PageNumber As Integer
                '
                ' If it is the current month, start at entry 6
                '
                PageNumber = 1
                '
                ' List Blog Entries
                '
                Dim BlogEntryModelList As List(Of BlogEntryModel) = DbModel.createList(Of BlogEntryModel)(cp, "(Month(DateAdded) = " & ArchiveMonth & ")And(year(DateAdded)=" & ArchiveYear & ")And(BlogID=" & blog.id & ")", "DateAdded Desc")
                If (BlogEntryModelList.Count = 0) Then
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
                        If cp.User.IsAuthoring("Blogs") Then
                            entryEditLink = cp.Content.GetEditLink(EntryName, EntryID.ToString(), True, EntryName, True)
                        End If
                        Dim EntryCopy As String = BlogEntry.Copy ' cp.Doc.GetText("Copy")
                        'EntryCopyOverview = cp.Doc.GetText(CS, "copyOverview")
                        Dim allowComments As Boolean = BlogEntry.AllowComments ' cp.Site.GetBoolean("allowComments")
                        Dim BlogTagList As String = BlogEntry.TagList ' cp.Doc.GetText("TagList")
                        Dim imageDisplayTypeId As Integer = BlogEntry.imageDisplayTypeId ' cp.Doc.GetInteger("imageDisplayTypeId")
                        Dim primaryImagePositionId As Integer = BlogEntry.primaryImagePositionId ' cp.Doc.GetInteger("primaryImagePositionId")
                        Dim articlePrimaryImagePositionId As Integer = BlogEntry.articlePrimaryImagePositionId ' cp.Doc.GetInteger("articlePrimaryImagePositionId")
                        Dim Return_CommentCnt As Integer
                        result = result & GetBlogEntryCell(cp, blog, rssFeed, BlogEntry, False, False, Return_CommentCnt, BlogTagList, imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, isBlogAuthor)
                    Next
                    'Call cp.CSNew.GoNext()
                    result = result & cr & "<div Class=""aoBlogEntryDivider"">&nbsp;</div>"
                    EntryPtr = EntryPtr + 1

                End If
                '              
                '
                Dim qs As String = blogListQs
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch.ToString())
                result = result & cr & "<div>&nbsp;</div>"
                result = result & cr & "<div Class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                qs = cp.Doc.RefreshQueryString()
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList.ToString())
                result = result & cr & "<div Class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                Call cp.CSNew.Close()
                'Call Main.CloseCS(CS)
                '
                's = s & Main.GetFormInputHidden(RequestNameBlogEntryID, Commentblog.id)
                result = result & cp.Html.Hidden(RequestNameSourceFormID, FormBlogArchivedBlogs.ToString())
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
        Friend Function GetFormTableRow(cp As CPBaseClass, FieldCaption As String, Innards As String, Optional AlignLeft As Boolean = True) As String
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
        Private Function GetField(cp As CPBaseClass, RequestName As String, Height As Integer, Width As Integer, MaxLenghth As Integer, DefaultValue As String) As String
            Dim result As String = ""
            Try
                If Height = 0 Then
                    Height = 1
                End If
                If Width = 0 Then
                    Width = 25
                End If
                '           
                result = cp.Html.InputText(RequestName, DefaultValue, Height.ToString(), Width.ToString())
                result = Replace(result, "<INPUT ", "<INPUT maxlength=""" & MaxLenghth & """ ", 1, 99, CompareMethod.Text)
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
        Private Function GetFormBlogPostList(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, BlogCategoryID As Integer, blogListLink As String, blogListQs As String, isBlogAuthor As Boolean) As String
            '
            Dim result As String = ""

            Try
                Dim locBlogTitle As String
                '
                locBlogTitle = blog.name

                If InStr(1, locBlogTitle, " Blog", vbTextCompare) = 0 Then
                    locBlogTitle = locBlogTitle & " blog"
                End If
                '

                If blog.Caption <> "" Then
                    result = result & vbCrLf & "<h2 Class=""aoBlogCaption"">" & blog.Caption & "</h2>"
                End If
                If blog.Copy <> "" Then
                    result = result & vbCrLf & "<div Class=""aoBlogDescription"">" & blog.Copy & "</div>"
                End If
                ' s = s & vbCrLf & Main.GetFormStart
                '
                ' List Blog Entries
                '
                Dim EntryPtr As Integer = 0
                '
                ' Display the most recent entries
                '
                cp.Utils.AppendLog(" GetFormBlogPostList 300")
                Dim BlogEntryModelList As List(Of BlogEntryModel)
                Dim NoneMsg As String
                If blog.AllowCategories And (BlogCategoryID <> 0) Then

                    Dim BlogCategoryName As String = cp.Doc.GetText("Blog Categories", BlogCategoryID.ToString())
                    BlogEntryModelList = DbModel.createList(Of BlogEntryModel)(cp, "(BlogID=" & blog.id & ")And(BlogCategoryID=" & BlogCategoryID & ")", "DateAdded Desc")

                    'CS = Main.OpenCSContent(cnBlogEntries, "(BlogID=" & blog.id & ")And(BlogCategoryID=" & BlogCategoryID & ")", "DateAdded Desc")
                    result = result & cr & "<div Class=""aoBlogCategoryCaption"">Category " & BlogCategoryName & "</div>"
                    NoneMsg = "There are no blog posts available In the category " & BlogCategoryName

                Else
                    BlogEntryModelList = DbModel.createList(Of BlogEntryModel)(cp, "(BlogID=" & blog.id & ")", "DateAdded Desc")
                    'CS = Main.OpenCSContent(cnBlogEntries, "(BlogID=" & blog.id & ")", "DateAdded Desc")
                    NoneMsg = "There are no blog posts available"
                End If

                Dim TestCategoryID As Integer
                Dim GroupList As List(Of GroupModel)
                Dim IsBlocked As Boolean

                If Not (BlogEntryModelList.Count <> 0) Then
                    'If Not Main.csok(CS) Then
                    result = result & cr & "<div Class=""aoBlogProblem"">" & NoneMsg & "</div>"
                Else
                    'Do While BlogEntryModelList.Count > 0 And EntryPtr < PostsToDisplay
                    Dim blogEntry As BlogEntryModel = BlogEntryModelList.First
                    For Each blogEntry In BlogEntryModelList
                        If EntryPtr < PostsToDisplay Then
                            TestCategoryID = blogEntry.blogCategoryID 'cp.Doc.GetInteger(CS, "BlogCategoryid")
                            If TestCategoryID <> 0 Then
                                '
                                ' Check that this member has access to this category
                                '
                                IsBlocked = Not cp.User.IsAdmin()
                                If IsBlocked Then
                                    Dim BlogCategorieModelList As List(Of BlogCategorieModel) = DbModel.createList(Of BlogCategorieModel)(cp, "id=" & TestCategoryID)
                                    ' CSCat = Main.OpenCSContent(cnBlogCategories, "id=" & TestCategoryID)
                                    Dim blogCat As BlogCategorieModel = BlogCategorieModelList.First
                                    'If cp.CSNew.OK() Then
                                    IsBlocked = blogCat.UserBlocking 'cp.Site.GetBoolean("UserBlocking")
                                    If IsBlocked Then
                                        GroupList = GroupModel.GetBlockingGroups(cp, blogCat.id) 'cp.Doc.GetText("BlockingGroups")
                                        IsBlocked = Not genericController.IsGroupListMember(cp, GroupList)
                                    End If
                                End If
                            End If
                            If IsBlocked Then
                                '
                                '
                                '
                            Else
                                Dim AuthorMemberID As Integer = blogEntry.AuthorMemberID ' cp.Doc.GetInteger("AuthorMemberID")
                                If AuthorMemberID = 0 Then
                                    AuthorMemberID = blogEntry.CreatedBy 'cp.Doc.GetInteger("createdBy")
                                End If
                                Dim DateAdded As Date = blogEntry.DateAdded    'cp.Doc.GetText("DateAdded")
                                Dim EntryName As String = blogEntry.name  'cp.Doc.GetText("Name")
                                Dim entryEditLink As String

                                If cp.User.IsAuthoring("Blogs") Then
                                    entryEditLink = cp.Content.GetEditLink("Blog Entries", blogEntry.id.ToString(), True, EntryName, True)
                                End If
                                Dim EntryCopy As String = blogEntry.Copy   'cp.Doc.GetText("Copy")
                                'EntryCopyOverview = cp.Doc.GetText(CS, "CopyOverview")
                                Dim BlogTagList As String = blogEntry.TagList 'cp.Doc.GetText("BlogTagList")
                                Dim imageDisplayTypeId As Integer = blogEntry.imageDisplayTypeId 'cp.Doc.GetInteger("imageDisplayTypeId")
                                Dim primaryImagePositionId As Integer = blogEntry.primaryImagePositionId 'cp.Doc.GetInteger("primaryImagePositionId")
                                Dim articlePrimaryImagePositionId As Integer = blogEntry.articlePrimaryImagePositionId 'cp.Doc.GetInteger("articlePrimaryImagePositionId")
                                Dim Return_CommentCnt As Integer
                                result = result & GetBlogEntryCell(cp, blog, rssFeed, Nothing, False, True, Return_CommentCnt, "", imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, isBlogAuthor)
                                result = result & cr & "<div Class=""aoBlogEntryDivider"">&nbsp;</div>"

                            End If
                            If EntryPtr = PostsToDisplay Then Exit For
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

                If cp.User.IsAdmin() And blog.AllowCategories Then
                    qs = "cid=" & cp.Content.GetID("Blog Categories") & "&af=4"
                    CategoryFooter = CategoryFooter & cr & "<div Class=""aoBlogFooterLink""><a href=""" & cp.Site.GetProperty("ADMINURL") & "?" & qs & """>Add a New category</a></div>"
                End If
                Dim ReturnFooter As String = ""
                If blog.AllowCategories Then

                    'If BlogCategoryID <> 0 Then
                    '
                    ' View all categories
                    '
                    qs = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogCategoryIDSet, "0", True)
                    CategoryFooter = CategoryFooter & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>See posts in all categories</a></div>"
                    'Else
                    '
                    ' select a category
                    '
                    qs = cp.Doc.RefreshQueryString
                    Dim BlogCategorieModelList As List(Of BlogCategorieModel) = DbModel.createList(Of BlogCategorieModel)(cp, "id=" & TestCategoryID)
                    If (BlogCategorieModelList.Count > 0) Then
                        Dim blogCat As BlogCategorieModel = BlogCategorieModelList.First
                        For Each blogCat In BlogCategorieModelList
                            BlogCategoryID = blogCat.id
                            IsBlocked = blogCat.UserBlocking
                            If IsBlocked Then
                                IsBlocked = Not cp.User.IsAdmin()
                            End If

                            If IsBlocked Then
                                GroupList = GroupModel.GetBlockingGroups(cp, BlogCategoryID)
                                IsBlocked = Not genericController.IsGroupListMember(cp, GroupList)
                            End If
                            If Not IsBlocked Then
                                Dim categoryLink As String = cp.Utils.ModifyQueryString(qs, RequestNameBlogCategoryIDSet, CStr(BlogCategoryID), True)
                                'categoryLink = kmaModifyLinkQuery(blogListLink, RequestNameBlogCategoryIDSet, CStr(BlogCategoryID), True)
                                'Dim categoryLink As String = cp.Content.GetEditLink(qs, CStr(BlogCategoryID), True, "", True)
                                CategoryFooter = CategoryFooter & cr & "<div class=""aoBlogFooterLink""><a href=""?" & categoryLink & """> See posts in the category " & blogCat.name & "</a></div>"
                            End If
                            ' Call Main.NextCSRecord(CS)
                        Next
                    End If
                End If
                'Call Main.CloseCS(CS)
                '
                ' Footer
                '
                result = result & cr & "<div>&nbsp;</div>"

                If isBlogAuthor Then
                    Call cp.Site.TestPoint("Blogs.GetFormBlogPostList, isBlogAuthor = True, appending 'create' message")
                    '
                    ' Create a new entry if this is the Blog Owner
                    '
                    qs = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor.ToString(), True)
                    result = result & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Create new post</a></div>"
                    '
                    ' Create a link to edit the blog record
                    '
                    qs = "cid=" & cp.Content.GetID("Blogs") & "&af=4&id=" & blog.id
                    result = result & cr & "<div class=""aoBlogFooterLink""><a href=""" & cp.Site.GetProperty("adminUrl") & "?" & qs & """>Edit blog features</a></div>"
                    '
                    ' Create a link to edit the rss record
                    '
                    If rssFeed.id = 0 Then

                    Else
                        qs = "cid=" & cp.Content.GetID("RSS Feeds") & "&af=4&id=" & rssFeed.id
                        result = result & cr & "<div class=""aoBlogFooterLink""><a href=""" & cp.Site.GetProperty("adminUrl") & "?" & qs & """>Edit rss feed features</a></div>"
                    End If
                End If
                result = result & ReturnFooter
                result = result & CategoryFooter
                '
                ' Search
                '
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch.ToString(), True)
                result = result & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                '
                ' Link to archives
                '
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogArchiveDateList.ToString(), True)
                result = result & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>See archives</a></div>"
                '
                ' Link to RSS Feed
                '
                Dim FeedFooter As String = ""
                If rssFeed.rssFilename <> "" Then
                    FeedFooter = "<a href=""http://" & cp.Site.Domain & "/RSS/" & rssFeed.rssFilename & """>"
                    FeedFooter = "rss feed " _
                        & FeedFooter & rssFeed.name & "</a>" _
                        & "&nbsp;" _
                        & FeedFooter & "<img src=""/cclib/images/IconXML-25x13.gif"" width=25 height=13 class=""aoBlogRSSFeedImage""></a>" _
                        & ""
                    result = result & cr & "<div class=""aoBlogFooterLink"">" & FeedFooter & "</div>"
                End If
                'result = result & cp.Html.Hidden(RequestNameBlogEntryID, blogentry.id)
                result = result & cp.Html.Hidden(RequestNameSourceFormID, FormBlogPostList.ToString())
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================
        '
        Private Function GetFormBlogPost(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, BlogCategoryID As Integer, blogListLink As String) As String
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
                If blogEntry.id = 0 Then
                    'hint =  "2"
                    '
                    ' New Entry that is being saved
                    '
                    result = result & GetFormTableRow2(cp, cp.Content.GetCopy("Blog Create Header for " & blog.name, "<h2>Create a new blog post</h2>"))
                    BlogTitle = cp.Doc.GetText(RequestNameBlogTitle)
                    BlogCopy = cp.Doc.GetText(RequestNameBlogCopy)
                    BlogTagList = cp.Doc.GetText(RequestNameBlogTagList)
                Else
                    '
                    ' Edit an entry
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
                'hint =  "4"
                Dim editor As String = cp.Html.InputText(RequestNameBlogCopy, BlogCopy, "50", "100%", False)
                result = result & GetFormTableRow(cp, "<div style=""padding-top:3px"">Title: </div>", cp.Html.InputText(RequestNameBlogTitle, BlogTitle, "1", "70"))
                result = result & GetFormTableRow(cp, "<div style=""padding-top:108px"">Post: </div>", editor)
                result = result & GetFormTableRow(cp, "<div style=""padding-top:3px"">Tag List: </div>", cp.Html.InputText(RequestNameBlogTagList, BlogTagList, "5", "70"))
                If blog.AllowCategories Then
                    'hint =  "5"
                    Dim CategorySelect As String = cp.Html.SelectContent(RequestNameBlogCategoryIDSet, BlogCategoryID.ToString(), "Blog Categories")
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
                Dim BlogImageModelList As List(Of BlogImageModel) = BlogImageModel.createListFromBlogEntry(cp, blogEntry.id)
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
                        & "</Table>" & cp.Html.Hidden("LibraryUploadCount", Ptr.ToString(), "LibraryUploadCount") _
                        & ""
                '
                result = result & GetFormTableRow(cp, "Images: ", c)
                If blogEntry.id <> 0 Then
                    'hint =  "6"
                    result = result & GetFormTableRow(cp, "", cp.Html.Button(rnButton, FormButtonPost) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonDelete))
                Else
                    'hint =  "7"
                    result = result & GetFormTableRow(cp, "", cp.Html.Button(rnButton, FormButtonPost) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel))
                End If
                'hint =  "8"
                Dim qs As String = cp.Doc.RefreshQueryString()
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList.ToString())
                'hint =  "9"
                result = result & vbCrLf & GetFormTableRow2(cp, "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>")

                '
                result = result & cp.Html.Hidden(RequestNameBlogEntryID, blogEntry.id.ToString())
                result = result & cp.Html.Hidden(RequestNameSourceFormID, FormBlogEntryEditor.ToString())
                result = result & "</table>"
                'hint =  "95"
                result = cp.Html.Form(result)
                '
                ' GetFormBlogPost = result
                'hint =  "96"

                Call cp.Visit.SetProperty(SNBlogEntryName, CStr(cp.Utils.GetRandomInteger()))
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            '
            Return result
        End Function
        '
        '====================================================================================
        '
        Private Function ProcessFormBlogPost(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, SourceFormID As Integer, ButtonValue As String, BlogCategoryID As Integer, blogListLink As String) As Integer
            '
            Try
                Dim Copy As String = ""                '
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
                        Dim BlogEntry = DbModel.add(Of BlogEntryModel)(cp)
                        BlogEntry.BlogID = blog.id
                        BlogEntry.save(Of BlogModel)(cp)
                        If (BlogEntry IsNot Nothing) Then
                            Dim EntryName As String = cp.Doc.GetText(RequestNameBlogTitle)
                            BlogEntry.name = EntryName
                            BlogEntry.Copy = cp.Doc.GetText(RequestNameBlogCopy)
                            BlogEntry.TagList = cp.Doc.GetText(RequestNameBlogTagList)
                            BlogEntry.blogCategoryID = BlogCategoryID
                            ProcessFormBlogPost = FormBlogPostList
                            BlogEntry.BlogID = blog.id
                            BlogEntry.save(Of BlogEntryModel)(cp)
                        End If

                        Call UpdateBlogFeed(cp)
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
                                    Dim BlogImage As BlogImageModel = DbModel.add(Of BlogImageModel)(cp)
                                    If cp.Doc.GetBoolean(rnBlogImageDelete & "." & UploadPointer) Then
                                        Call cp.Content.Delete(cnBlogImages, ImageID.ToString())
                                    Else

                                        If (BlogImage IsNot Nothing) Then
                                            BlogImage.name = imageName
                                            BlogImage.description = imageDescription
                                            BlogImage.SortOrder = New String(CChar("0"), 12 - imageOrder.ToString().Length) & imageOrder.ToString() ' String.Empty.PadLeft((12 - Len(imageOrder.ToString())), "0") & imageOrder
                                            BlogEntry.save(Of BlogEntryModel)(cp)
                                        End If
                                    End If
                                ElseIf imageFilename <> "" Then
                                    '
                                    ' upload image
                                    '
                                    'CS = Main.InsertCSRecord(cnBlogImages)
                                    'If Main.IsCSOK(CS) Then
                                    Dim BlogImage As BlogImageModel = DbModel.add(Of BlogImageModel)(cp)
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
                                        Dim VirtualFilePath As String = BlogImage.getUploadPath(Of BlogImageModel)("filename")
                                        Call cp.Html.ProcessInputFile(rnBlogUploadPrefix & "." & UploadPointer, VirtualFilePath)
                                        BlogImage.Filename = VirtualFilePath & imageFilename
                                        BlogImage.save(Of BlogImageModel)(cp)

                                        'If BuildVersion > "3.4.190" Then
                                        '
                                        ' add image resize values
                                        '
                                        'Dim sf As Object = CreateObject("sfimageresize.imageresize")
                                        'sf.Algorithm = 5

                                        'sf.LoadFromFile(cp.Site.PhysicalFilePath & VirtualFilePath & imageFilename)
                                        'If Err.Number = 0 Then
                                        '    Dim ImageWidth As Integer = sf.Width
                                        '    Dim ImageHeight As Integer = sf.Height
                                        '    Call cp.CSNew.SetField("height", ImageHeight)
                                        '    Call cp.CSNew.SetField("width", ImageWidth)
                                        'Else
                                        '    Err.Clear()
                                        'End If
                                        ''
                                        'sf = Nothing
                                        '                                Call Main.SetCS(CS, "AltSizeList", AltSizeList)
                                        BlogImage.SortOrder = New String("0"c, 12 - imageOrder.ToString().Length) & imageOrder.ToString()
                                        'Call Main.SetCS(CS, "sortorder", String(12 - Len(BlogImageID), "0") & BlogImageID)
                                        'End If
                                    End If
                                    '
                                    Dim ImageRule As BlogImageRuleModel = BlogImageRuleModel.add(Of BlogImageRuleModel)(cp)
                                    If (ImageRule IsNot Nothing) Then
                                        ImageRule.BlogEntryID = BlogEntry.id
                                        ImageRule.BlogImageID = BlogImageID
                                        ImageRule.save(Of BlogImageRuleModel)(cp)
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
                        Dim blogEntryId = cp.Doc.GetInteger(RequestNameBlogEntryID)
                        DbModel.delete(Of BlogEntryModel)(cp, blogEntryId)
                        ProcessFormBlogPost = FormBlogPostList
                        Call UpdateBlogFeed(cp)
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
        Private Function GetFormBlogPostDetails(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, isBlogAuthor As Boolean, BlogCategoryID As Integer, blogListLink As String, blogListQs As String) As String
            '
            Dim result As String = ""
            Try
                'Dim EntryCopyOverview As String
                Dim BlogTagList As String
                Dim entryEditLink As String
                Dim formKey As String
                Dim Return_CommentCnt As Integer
                Dim Copy As String
                Dim DateAdded As Date
                Dim AuthorMemberID As Integer
                Dim EntryName As String
                Dim EntryCopy As String
                Dim allowComments As Boolean
                Dim EntryPtr As Integer
                Dim qs As String
                Dim CommentCnt As Integer
                Dim imageDisplayTypeId As Integer
                Dim primaryImagePositionId As Integer
                Dim articlePrimaryImagePositionId As Integer
                '
                Call cp.Site.TestPoint("blog -- getFormBlogPostDetails, enter ")
                Call cp.Site.TestPoint("blog -- getFormBlogPostDetails, blogEntry.recaptcha=[" & blog.recaptcha & "]")
                '
                ' setup form key
                '
                formKey = "{" & Guid.NewGuid().ToString() & "}" ' cp.Utils.enc  Main.EncodeKeyNumber(Main.VisitID, Now())
                result = vbCrLf & cp.Html.Hidden("FormKey", formKey)
                result = result & cr & "<div class=""aoBlogHeaderLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                '
                ' Print the Blog Entry
                CommentCnt = 0
                If (blogEntry IsNot Nothing) Then

                    'Dim BlogEntry As BlogEntryModel = blogEntryList.First
                    If Not (blogEntry IsNot Nothing) Then
                        result = result & cr & "<div class=""aoBlogProblem"">Sorry, the blog post you selected is not currently available</div>"
                    Else
                        AuthorMemberID = blogEntry.AuthorMemberID
                        If AuthorMemberID = 0 Then
                            AuthorMemberID = blogEntry.CreatedBy
                        End If
                        DateAdded = blogEntry.DateAdded
                        EntryName = blogEntry.name
                        If cp.User.IsAuthoring("Blogs") Then
                            entryEditLink = cp.Content.GetEditLink(EntryName, blogEntry.id.ToString(), True, EntryName, True)
                        End If
                        EntryCopy = blogEntry.Copy

                        allowComments = blogEntry.AllowComments
                        BlogTagList = blogEntry.TagList
                        qs = ""
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
                        blogEntry.Viewings = (1 + cp.Doc.GetInteger("viewings"))
                        blogEntry.imageDisplayTypeId = cp.Doc.GetInteger("imageDisplayTypeId")
                        blogEntry.primaryImagePositionId = cp.Doc.GetInteger("primaryImagePositionId")
                        ' blogEntry.articlePrimaryImagePositionId = cp.Doc.GetInteger("articlePrimaryImagePositionId")
                        result = result & GetBlogEntryCell(cp, blog, rssFeed, blogEntry, False, True, Return_CommentCnt, "", imageDisplayTypeId, primaryImagePositionId, articlePrimaryImagePositionId, blogListQs, isBlogAuthor)
                        EntryPtr = EntryPtr + 1
                        '
                        blog.id = cp.Doc.GetInteger("BlogID")
                    End If

                    '
                End If
                '
                Dim criteria As String = ""
                Dim VisitModel As New VisitModel
                Dim excludeFromAnalytics As Boolean = VisitModel.ExcludeFromAnalytics
                If (Not excludeFromAnalytics) Then
                    Dim BlogViewingLog As BlogViewingLogModel = DbModel.add(Of BlogViewingLogModel)(cp)
                    If (BlogViewingLog IsNot Nothing) Then
                        BlogViewingLog.name = cp.User.Name & ", post " & CStr(blogEntry.id) & ", " & Now()
                        BlogViewingLog.BlogEntryID = blogEntry.id
                        BlogViewingLog.MemberID = cp.User.Id
                        BlogViewingLog.VisitID = cp.Visit.Id
                        BlogViewingLog.save(Of BlogModel)(cp)
                    End If
                End If
                '
                If isBlogAuthor And (Return_CommentCnt > 0) Then
                    result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.Button(FormButtonApplyCommentChanges) & "</div>"
                End If
                '
                Dim Auth As Integer
                Dim AllowPasswordEmail As Boolean
                Dim AllowMemberJoin As Boolean
                '
                If allowComments And (cp.Visit.CookieSupport) And (Not VisitModel.Bot()) Then
                    result = result & cr & "<div class=""aoBlogCommentHeader"">Post a Comment</div>"
                    '
                    If Not (cp.UserError.OK()) Then
                        result = result & "<div class=""aoBlogCommentError"">" & (cp.UserError.OK()) & "</div>"
                    End If
                    '

                    If (Not blog.AllowAnonymous) And (Not cp.User.IsAuthenticated) Then
                        AllowPasswordEmail = cp.Utils.EncodeBoolean(cp.Site.GetProperty("AllowPasswordEmail", "0"))
                        AllowMemberJoin = cp.Utils.EncodeBoolean(cp.Site.GetProperty("AllowMemberJoin", "0"))
                        Auth = cp.Doc.GetInteger("auth")
                        If (Auth = 1) And (Not AllowPasswordEmail) Then
                            Auth = 3
                        ElseIf (Auth = 2) And (Not AllowMemberJoin) Then
                            Auth = 3
                        End If
                        Call cp.Doc.AddRefreshQueryString(RequestNameFormID, FormBlogPostDetails.ToString())
                        Call cp.Doc.AddRefreshQueryString(RequestNameBlogEntryID, blogEntry.id.ToString())
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
                                'loginForm = "Get login Form removed"
                                'loginForm = Replace(loginForm, "LoginUsernameInput", "LoginUsernameInput-BlockFocus")
                                result = result _
                                        & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>" _
                                         & cr & "</div>"
                                '& cr & "<div class=""aoBlogLoginBox"">" _
                                '& cr & "<div class=""aoBlogCommentCopy"">" & loginForm & "</div>" _
                                '& cr & "</div>"
                        End Select

                    Else
                        result = result & cr & "<div>&nbsp;</div>"
                        result = result & cr & "<div class=""aoBlogCommentCopy"">Title</div>"
                        If RetryCommentPost Then
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & GetField(cp, RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(RequestNameCommentTitle.ToString)) & "</div>"
                            result = result & cr & "<div>&nbsp;</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">Comment</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.InputText(RequestNameCommentCopy, cp.Doc.GetText(RequestNameCommentCopy), "15", "70",) & "</div>"
                        Else
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & GetField(cp, RequestNameCommentTitle, 1, 35, 35, cp.Doc.GetText(RequestNameCommentTitle.ToString)) & "</div>"
                            result = result & cr & "<div>&nbsp;</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">Comment</div>"
                            result = result & cr & "<div class=""aoBlogCommentCopy"">" & cp.Html.InputText(RequestNameCommentCopy, "", "15", "70") & "</div>"
                        End If
                        's = s & cr & "<div class=""aoBlogCommentCopy"">Verify Text</div>"
                        '
                        If blog.recaptcha Then
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
                If isBlogAuthor Then
                    qs = cp.Doc.RefreshQueryString()
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor.ToString())
                    result = result & cr & "<div class=""aoBlogToolLink""><a href=""?" & qs & """>Edit</a></div>"
                End If
                '
                ' Search
                '
                qs = cp.Doc.RefreshQueryString
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogSearch.ToString(), True)
                result = result & cr & "<div class=""aoBlogFooterLink""><a href=""?" & qs & """>Search</a></div>"
                '
                ' back to recent posts
                '
                'QSBack = cp.Doc.RefreshQueryString()
                'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameBlogEntryID, "", True)
                'QSBack = cp.Utils.ModifyQueryString(QSBack, RequestNameFormID, FormBlogPostList)
                result = result & cr & "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"
                '
                result = result & vbCrLf & cp.Html.Hidden(RequestNameSourceFormID, FormBlogPostDetails.ToString())
                result = result & vbCrLf & cp.Html.Hidden(RequestNameBlogEntryID, blogEntry.id.ToString())
                result = result & vbCrLf & cp.Html.Hidden("EntryCnt", EntryPtr.ToString())
                'result = result & "</div>"
                GetFormBlogPostDetails = result
                result = cp.Html.Form(GetFormBlogPostDetails)
                '
                '
                Try
                    Call cp.Visit.SetProperty(SNBlogCommentName, CStr(cp.Utils.GetRandomInteger()))
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
        Private Function ProcessFormBlogPostDetails(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, SourceFormID As Integer, ButtonValue As String, BlogCategoryID As Integer, isBlogAuthor As Boolean) As Integer
            '
            Try
                Dim MemberID As Integer
                Dim MemberName As String
                Dim EntryLink As String
                Dim EmailBody As String
                Dim EmailFromAddress As String
                Dim EntryCnt As Integer
                Dim EntryPtr As Integer
                Dim CommentCnt As Integer
                Dim CommentPtr As Integer
                Dim Suffix As String
                Dim Copy As String
                Dim CommentID As Integer
                Dim SN As String
                'Dim EntryID As Integer
                Dim formKey As String
                Dim optionStr As String
                Dim captchaResponse As String
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
                        If blog.recaptcha Then
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
                                Call cp.Utils.AppendLog("testpoint1")
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
                            Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(formkey=" & cp.Db.EncodeSQLText(formKey) & ")", "ID")
                            If (BlogCommentModelList.Count <> 0) Then
                                Call cp.UserError.Add("<p>This comment has already been accepted.</p>")
                                RetryCommentPost = False
                                Call cp.Utils.AppendLog("testpoint2")
                            Else
                                Call cp.Site.TestPoint("blog -- adding comment, no user error")
                                Dim EntryID = cp.Doc.GetInteger(RequestNameBlogEntryID)
                                Dim BlogEntry As BlogEntryModel = DbModel.create(Of BlogEntryModel)(cp, EntryID)
                                Dim BlogComment As BlogCommentModel = DbModel.add(Of BlogCommentModel)(cp)
                                'CSP = Main.InsertCSRecord(cnBlogComments)
                                If blog.AllowAnonymous And (Not cp.User.IsAuthenticated) Then
                                    MemberID = 0
                                    MemberName = AnonymousMemberName
                                Else
                                    MemberID = cp.User.Id
                                    MemberName = cp.User.Name
                                End If
                                cp.Utils.AppendLog("test1")
                                BlogComment.BlogID = blog.id
                                BlogComment.Active = True
                                BlogComment.name = cp.Doc.GetText(RequestNameCommentTitle)
                                BlogComment.CopyText = Copy
                                BlogComment.EntryID = EntryID
                                BlogComment.Approved = isBlogAuthor Or blog.autoApproveComments
                                BlogComment.FormKey = formKey
                                BlogComment.save(Of BlogCommentModel)(cp)
                                CommentID = BlogComment.id
                                RetryCommentPost = False
                                '
                                cp.Utils.AppendLog("test2")
                                If (blog.emailComment) Then
                                    '
                                    ' Send Comment Notification
                                    EntryLink = BlogEntry.RSSLink
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
                                                    & cr & "Blog '" & blog.name & "'" _
                                                    & cr & "Post '" & BlogEntry.name & "'" _
                                                    & cr & "By " & cp.User.Name _
                                                    & vbCrLf _
                                                    & vbCrLf & cp.Utils.EncodeHTML(Copy) _
                                                    & vbCrLf
                                    EmailFromAddress = cp.Site.GetProperty("EmailFromAddress", "info@" & cp.Site.Domain)
                                    'Call Main.SendMemberEmail(BlogOwnerID, EmailFromAddress, "Blog comment notification for [" & blog.name & "]", EmailBody, False, False)
                                    Call cp.Email.sendUser(BlogEntry.AuthorMemberID.ToString(), EmailFromAddress, "Blog comment notification for [" & blog.name & "]", EmailBody, False, False)
                                    If blog.AuthoringGroupID <> 0 Then
                                        Dim MemberRuleList As List(Of MemberRuleModel) = DbModel.createList(Of MemberRuleModel)(cp, "GroupId=" & blog.AuthoringGroupID)
                                        For Each MemberRule In MemberRuleList
                                            Call cp.Email.sendUser(MemberRule.MemberID.ToString(), EmailFromAddress, "Blog comment on " & blog.name, EmailBody, False, False)
                                        Next
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
                        If isBlogAuthor Then
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
                                                Call cp.Content.Delete("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blog.id & ")")
                                            ElseIf cp.Doc.GetBoolean("Approve" & Suffix) And Not cp.Doc.GetBoolean("Approved" & Suffix) Then
                                                '
                                                ' Approve Comment
                                                '
                                                Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(name=" & cp.Utils.EncodeQueryString(blog.name) & ")", "ID")
                                                If (BlogCommentModelList.Count > 0) Then
                                                    ' CS = Main.OpenCSContent("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blog.id & ")")
                                                    Dim BlogComment As BlogCommentModel = DbModel.add(Of BlogCommentModel)(cp)
                                                    If cp.CSNew.OK() Then
                                                        BlogComment.Approved = True
                                                        ' Call Main.SetCS(CS, "Approved", True)
                                                    End If
                                                    'Call Main.CloseCS(CS)
                                                ElseIf Not cp.Doc.GetBoolean("Approve" & Suffix) And cp.Doc.GetBoolean("Approved" & Suffix) Then
                                                    '
                                                    ' Unapprove comment
                                                    '
                                                    'CS = Main.OpenCSContent("Blog Comments", "(id=" & CommentID & ")and(BlogID=" & blog.id & ")")
                                                    Dim BlogComment As BlogCommentModel = DbModel.add(Of BlogCommentModel)(cp)
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
        Private Function GetBlogCommentCell(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, blogComment As BlogCommentModel, IsSearchListing As Boolean, isBlogAuthor As Boolean) As String
            Dim result As String = ""
            Try
                Dim Copy As String
                Dim RowCopy As String
                Dim qs As String
                '
                If IsSearchListing Then
                    qs = cp.Doc.RefreshQueryString
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList.ToString())
                    result = result & cr & "<div class=""aoBlogEntryName"">Comment to Blog Post " & blogEntry.name & ", <a href=""?" & qs & """>View this post</a></div>"
                    result = result & cr & "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                End If
                result = result & cr & "<div class=""aoBlogCommentName"">" & cp.Utils.EncodeHTML(blogComment.name) & "</div>"
                Copy = blogComment.Copy
                Copy = cp.Utils.EncodeHTML(Copy)
                Copy = Replace(Copy, vbCrLf, "<BR />")
                result = result & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>"
                RowCopy = ""
                Dim author = DbModel.create(Of PersonModel)(cp, blogEntry.AuthorMemberID)
                If author.name <> "" Then
                    RowCopy = RowCopy & "by " & cp.Utils.EncodeHTML(author.name)
                    If blogComment.DateAdded <> Date.MinValue Then
                        RowCopy = RowCopy & " | " & blogComment.DateAdded
                    End If
                Else
                    If blogComment.DateAdded <> Date.MinValue Then
                        RowCopy = RowCopy & blogComment.DateAdded
                    End If
                End If
                '
                If isBlogAuthor Then
                    '
                    ' Blog owner Approval checkbox
                    '
                    If RowCopy <> "" Then
                        RowCopy = RowCopy & " | "
                    End If
                    RowCopy = RowCopy _
                        & cp.Html.Hidden("CommentID", blogComment.id.ToString()) _
                        & cp.Html.CheckBox("Approve", blogComment.Approved) _
                        & cp.Html.Hidden("Approved", blogComment.Approved.ToString()) _
                        & "&nbsp;Approved&nbsp;" _
                        & " | " _
                        & cp.Html.CheckBox("Delete", False) _
                        & "&nbsp;Delete" _
                        & ""
                End If
                If RowCopy <> "" Then
                    result = result & cr & "<div class=""aoBlogCommentByLine"">Posted " & RowCopy & "</div>"
                End If
                '
                If (Not blogComment.Approved) And (Not isBlogAuthor) And (blogComment.AuthorMemberID = cp.User.Id) Then
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
        '====================================================================================
        '
        Private Function GetBlogEntryCell(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, DisplayFullEntry As Boolean, IsSearchListing As Boolean, Return_CommentCnt As Integer, entryEditLink As String, imageDisplayTypeId As Integer, primaryImagePositionId As Integer, articlePrimaryImagePositionId As Integer, blogListQs As String, isBlogAuthor As Boolean) As String
            Dim result As String = ""
            Try
                Dim visit As VisitModel = VisitModel.create(cp, cp.Visit.Id)
                Dim qs As String = blogListQs
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
                Dim EntryLink As String = getLinkAlias(cp, "?" & qs)
                If EntryLink = "" Then
                    qs = blogListQs
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
                    EntryLink = "?" & qs
                End If
                '
                Dim blogCopy As BlogCopyModel = DbModel.create(Of BlogCopyModel)(cp, blogEntry.id)
                articlePrimaryImagePositionId = blogCopy.articlePrimaryImagePositionId
                result = ""
                Dim TagListRow As String = ""
                Dim blogImageList = BlogImageModel.createListFromBlogEntry(cp, blogEntry.id)
                If DisplayFullEntry Then
                    result = result & vbCrLf & entryEditLink & "<h2 class=""aoBlogEntryName"">" & blogEntry.name & "</h2>"
                    result = result & cr & "<div class=""aoBlogEntryCopy"">"
                    If (blogImageList.Count > 0) Then
                        Dim ThumbnailFilename As String = ""
                        Dim imageFilename As String = ""
                        Dim imageName As String = ""
                        Dim imageDescription As String = ""
                        GetBlogImage(cp, blog, rssFeed, blogEntry, blogImageList.First, ThumbnailFilename, imageFilename, imageDescription, imageName)
                        Select Case articlePrimaryImagePositionId
                            Case 2
                                '
                                ' align right
                                result = result & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:40%;"">"
                            Case 3
                                '
                                ' align left
                                result = result & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:40%;"">"
                            Case 4
                                '
                                ' hide
                            Case Else
                                '
                                ' 1 and none align per stylesheet
                                result = result & "<img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:40%;"">"
                        End Select
                    End If
                    result = result & blogEntry.Copy & "</div>"
                    qs = blogListQs
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor.ToString())
                    Dim c As String = ""
                    If (imageDisplayTypeId = imageDisplayTypeList) And (blogImageList.Count > 0) Then
                        c = ""
                        '
                        ' Get ImageID List
                        For Each blogImage In blogImageList
                            Dim ThumbnailFilename As String = ""
                            Dim imageFilename As String = ""
                            Dim imageName As String = ""
                            Dim imageDescription As String = ""
                            GetBlogImage(cp, blog, rssFeed, blogEntry, blogImageList.First, ThumbnailFilename, imageFilename, imageDescription, imageName)
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
                    If blogEntry.TagList <> "" Then
                        blogEntry.TagList = Replace(blogEntry.TagList, ",", vbCrLf)
                        Dim Tags() As String = Split(blogEntry.TagList, vbCrLf)
                        blogEntry.TagList = ""
                        Dim SQS As String
                        SQS = cp.Utils.ModifyQueryString(blogListQs, RequestNameFormID, FormBlogSearch.ToString(), True)
                        Dim Ptr As Integer
                        For Ptr = 0 To UBound(Tags)
                            'QS = cp.Utils.ModifyQueryString(SQS, RequestNameFormID, FormBlogSearch, True)
                            qs = cp.Utils.ModifyQueryString(SQS, RequestNameQueryTag, Tags(Ptr), True)
                            Dim Link As String = "?" & qs
                            blogEntry.TagList = blogEntry.TagList & ", " & "<a href=""" & Link & """>" & Tags(Ptr) & "</a>"
                        Next
                        blogEntry.TagList = Mid(blogEntry.TagList, 3)
                        c = "" _
                        & cr & "<div class=""aoBlogTagListHeader"">" _
                        & cr & vbTab & "Tags" _
                        & cr & "</div>" _
                        & cr & "<div class=""aoBlogTagList"">" _
                        & cr & vbTab & blogEntry.TagList _
                        & cr & "</div>"
                        TagListRow = "" _
                        & cr & "<div class=""aoBlogTagListSection"">" _
                        & cp.Html.Indent(c) _
                        & cr & "</div>"
                    End If
                Else
                    result = result & vbCrLf & entryEditLink & "<h2 class=""aoBlogEntryName""><a href=""" & EntryLink & """>" & blogEntry.name & "</a></h2>"
                    result = result & cr & "<div class=""aoBlogEntryCopy"">"
                    If (blogImageList.Count > 0) Then
                        Dim ThumbnailFilename As String = ""
                        Dim imageFilename As String = ""
                        Dim imageName As String = ""
                        Dim imageDescription As String = ""
                        GetBlogImage(cp, blog, rssFeed, blogEntry, blogImageList.First, ThumbnailFilename, imageFilename, imageDescription, imageName)
                        If ThumbnailFilename <> "" Then
                            Select Case primaryImagePositionId
                                Case 2
                                    '
                                    ' align right
                                    '
                                    'result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:25%;""></a>"
                                Case 3
                                    '
                                    ' align left
                                    '
                                    ' result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:25%;""></a>"
                                Case 4
                                    '
                                    ' hide
                                    '
                                Case Else
                                    '
                                    ' 1 and none align per stylesheet
                                    '
                                    'result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:25%;""></a>"

                            End Select
                        End If
                    End If
                    result = result & "<p>" & genericController.filterCopy(cp, blogEntry.Copy, OverviewLength) & "</p></div>"
                    result = result & cr & "<div class=""aoBlogEntryReadMore""><a href=""" & EntryLink & """>Read More</a></div>"
                End If
                '
                ' Podcast link
                '
                If blogEntry.PodcastMediaLink <> "" Then
                    cp.Doc.SetProperty("Media Link", blogEntry.PodcastMediaLink)
                    cp.Doc.SetProperty("Media Link", blogEntry.PodcastSize.ToString())
                    cp.Doc.SetProperty("Hide Player", "True")
                    cp.Doc.SetProperty("Auto Start", "False")
                    '
                    result = result & cp.Utils.ExecuteAddon("{F6037DEE-023C-4A14-A972-ADAFA5538240}")
                End If
                '
                ' Author Row
                '
                Dim RowCopy As String = ""
                Dim author = DbModel.create(Of PersonModel)(cp, blogEntry.CreatedBy)
                If (author IsNot Nothing) Then
                    RowCopy = RowCopy & "By " & author.name
                    If blogEntry.DateAdded <> Date.MinValue Then
                        RowCopy = RowCopy & " | " & blogEntry.DateAdded
                    End If
                Else
                    If blogEntry.DateAdded <> Date.MinValue Then
                        RowCopy = RowCopy & blogEntry.DateAdded
                    End If
                End If
                Dim CommentCount As Integer
                Dim Criteria As String
                If blogEntry.AllowComments And (cp.Visit.CookieSupport) And (Not visit.Bot()) Then
                    'If allowComments Then
                    '
                    ' Show comment count
                    '
                    Criteria = "(Approved<>0)and(EntryID=" & blogEntry.id & ")"
                    ' CSCount = Main.OpenCSContent(cnBlogComments, "(Approved<>0)and(EntryID=" & blogEntry.id & ")")
                    Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(Approved<>0)and(EntryID=" & blogEntry.id & ")")
                    CommentCount = BlogCommentModelList.Count
                    If DisplayFullEntry Then
                        If CommentCount = 1 Then
                            RowCopy = RowCopy & " | 1 Comment"
                        ElseIf CommentCount > 1 Then
                            RowCopy = RowCopy & " | " & CommentCount & " Comments&nbsp;(" & CommentCount & ")"
                        End If
                    Else
                        qs = blogListQs
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
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
                'hint =  hint & ",10"
                Dim ToolLine As String = ""
                Dim CommentPtr As Integer
                If blogEntry.AllowComments And (cp.Visit.CookieSupport) And (Not visit.Bot) Then


                    'If blogEntry.AllowComments Then
                    If Not DisplayFullEntry Then
                        ''
                        '' Show comment count
                        ''
                        Criteria = "(Approved<>0)and(EntryID=" & blogEntry.id & ")"
                        Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, "(Approved<>0)and(EntryID=" & blogEntry.id & ")")
                        CommentCount = BlogCommentModelList.Count
                        'CSCount = Main.OpenCSContent(cnBlogComments, "(Approved<>0)and(EntryID=" & blogEntry.id & ")")
                        'CommentCount = Main.GetCSRowCount(CSCount)
                        'Call Main.CloseCS(CSCount)
                        '
                        qs = blogListQs
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
                        Dim CommentLine As String = ""
                        If CommentCount = 0 Then
                            CommentLine = CommentLine & "<a href=""?" & qs & """>Comment</a>"
                        Else
                            CommentLine = CommentLine & "<a href=""?" & qs & """>Comments</a>&nbsp;(" & CommentCount & ")"
                        End If
                        If isBlogAuthor Then
                            Criteria = "(EntryID=" & blogEntry.id & ")"
                            'CSCount = Main.OpenCSContent(cnBlogComments, "((Approved is null)or(Approved=0))and(EntryID=" & blogEntry.id & ")")
                            BlogCommentModelList = DbModel.createList(Of BlogCommentModel)(cp, "(Approved<>0)and(EntryID=" & blogEntry.id & ")")
                            CommentCount = BlogCommentModelList.Count
                            'Call Main.CloseCS(CSCount)
                            If ToolLine <> "" Then
                                ToolLine = ToolLine & "&nbsp;|&nbsp;"
                            End If
                            ToolLine = ToolLine & "Unapproved Comments (" & CommentCount & ")"
                            qs = blogListQs
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor.ToString())
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
                        'hint =  hint & ",11"
                        If isBlogAuthor Then
                            '
                            ' Owner - Show all comments
                            '
                            Criteria = "(EntryID=" & blogEntry.id & ")"
                        Else
                            '
                            ' non-owner - just approved comments plus your own comments
                            '
                            Criteria = "((Approved<>0)or(AuthorMemberID=" & cp.User.Id & "))and(EntryID=" & blogEntry.id & ")"
                        End If
                        Dim BlogCommentModelList As List(Of BlogCommentModel) = DbModel.createList(Of BlogCommentModel)(cp, Criteria, "DateAdded")
                        If (BlogCommentModelList.Count > 0) Then
                            Dim Divider As String = "<div class=""aoBlogCommentDivider"">&nbsp;</div>"
                            result = result & cr & "<div class=""aoBlogCommentHeader"">Comments</div>"
                            result = result & vbCrLf & Divider
                            CommentPtr = 0
                            Dim blogComment As BlogCommentModel = DbModel.add(Of BlogCommentModel)(cp)
                            If (blogComment IsNot Nothing) Then
                                For Each blogComment In DbModel.createList(Of BlogCommentModel)(cp, Criteria)

                                    result = result & GetBlogCommentCell(cp, blog, rssFeed, blogEntry, blogComment, False, isBlogAuthor)
                                    result = result & vbCrLf & Divider
                                    CommentPtr = CommentPtr + 1
                                    'Call Main.NextCSRecord(CS)
                                Next
                            End If

                        End If

                    End If
                End If
                '
                'hint =  hint & ",12"
                If ToolLine <> "" Then
                    result = result & cr & "<div class=""aoBlogToolLink"">" & ToolLine & "</div>"
                End If
                result = result & vbCrLf & cp.Html.Hidden("CommentCnt" & blogEntry.id, CommentPtr.ToString())
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
        '========================================================================
        '
        Private Sub UpdateBlogFeed(cp As CPBaseClass)
            '
            Try
                Call cp.Utils.ExecuteAddon(RSSProcessAddonGuid)
                'Dim RSSTitle As String
                'Dim EntryCopy As String
                'Dim EntryLink As String
                'Dim qs As String
                'Dim RuleIDs() As Integer
                'Dim RuleBlogPostIDs() As Integer
                'Dim RuleCnt As Integer
                'Dim RulePtr As Integer
                'Dim RuleSize As Integer
                'Dim BlogPostID As Integer
                'Dim AdminURL As String
                ''
                'AdminURL = cp.Site.GetProperty("adminUrl")
                'If rssFeed.id <> 0 Then
                '    '
                '    ' Gather all the current rules
                '    '
                '    Dim RSSFeedBlogRuleList As List(Of RSSFeedBlogRuleModel) = RSSFeedBlogRuleModel.createList(cp, "RSSFeedID=" & rssFeed.id, "id,BlogPostID")
                '    RuleCnt = 0
                '    For Each RSSFeedBlogRule In RSSFeedBlogRuleList
                '        If RuleCnt >= RuleSize Then
                '            RuleSize = RuleSize + 10
                '            ReDim Preserve RuleIDs(RuleSize)
                '            ReDim Preserve RuleBlogPostIDs(RuleSize)
                '        End If
                '        RuleIDs(RuleCnt) = RSSFeedBlogRule.id 'Csv.GetCSInteger(CS, "ID")
                '        RuleBlogPostIDs(RuleCnt) = RSSFeedBlogRule.BlogPostID 'Csv.GetCSInteger(CS, "BlogPostID")
                '        RuleCnt = RuleCnt + 1
                '    Next

                '    Dim BlogCopyModelList As List(Of BlogCopyModel) = BlogCopyModel.createListFromBlogCopy(cp, blog.id)
                '    For Each BlogCopy In BlogCopyModelList
                '        BlogPostID = BlogCopy.id 'Csv.GetCSInteger(CS, "id")
                '        For RulePtr = 0 To RuleCnt - 1
                '            If BlogPostID = RuleBlogPostIDs(RulePtr) Then
                '                RuleIDs(RulePtr) = -1
                '                Exit For
                '            End If
                '        Next
                '        If RulePtr >= RuleCnt Then
                '            '
                '            ' Rule not found, add it
                '            '
                '            Dim RSSFeedBlogRule As RSSFeedBlogRuleModel = RSSFeedBlogRuleModel.add(cp)
                '            If (RSSFeedBlogRule IsNot Nothing) Then
                '                RSSFeedBlogRule.RSSFeedID = rssFeed.id
                '                RSSFeedBlogRule.BlogPostID = BlogPostID
                '                RSSFeedBlogRule.save(Of BlogModel)(cp)
                '            End If
                '            'CSRule = Csv.InsertCSRecord(cnRSSFeedBlogRules, 0)
                '            'If Csv.IsCSOK(CSRule) Then

                '            'Call Csv.SetCS(CSRule, "RSSFeedID", RSSFeed.id)
                '            'Call Csv.SetCS(CSRule, "BlogPostID", BlogPostID)
                '            'End If
                '            'Call Csv.CloseCS(CSRule)
                '            Dim BlogEntry As BlogEntryModel = DbModel.add(Of BlogEntryModel)(cp)
                '            If (BlogEntry IsNot Nothing) Then
                '                RSSTitle = Trim(BlogEntry.name)
                '                If RSSTitle = "" Then
                '                    RSSTitle = "Blog Post " & EntryID
                '                End If
                '                BlogEntry.RSSTitle = RSSTitle
                '                'Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
                '                '
                '                qs = ""
                '                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                '                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                '                EntryLink = cp.Content.GetLinkAliasByPageID(cp.Doc.PageId, qs, blogListLink & "?" & qs)
                '                If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                '                    'Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                '                    BlogEntry.RSSLink = EntryLink
                '                End If
                '                BlogEntry.RSSDescription = genericController.filterCopy(cp, EntryCopy, 150)
                '                ' Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
                '            End If
                '            ' CSPost = Csv.OpenCSContentRecord(cnBlogEntries, BlogPostID)
                '            'If Csv.IsCSOK(CSPost) Then
                '            '            blogEntry.Name = Csv.GetCSText(CSPost, "name")
                '            '            EntryID = Csv.GetCSInteger(CSPost, "id")
                '            '            EntryCopy = Csv.GetCSText(CSPost, "copy")
                '            '            '
                '            '            RSSTitle = Trim(blogEntry.Name)
                '            '            If RSSTitle = "" Then
                '            '                RSSTitle = "Blog Post " & EntryID
                '            '            End If
                '            '            Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
                '            '            '
                '            '            qs = ""
                '            '            qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                '            '            qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                '            '            EntryLink = Main.GetLinkAliasByPageID(cp.Doc.PageId, qs, blogListLink & "?" & qs)
                '            '            If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                '            '                Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                '            '            End If
                '            '            Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
                '        End If

                '        ' Call Csv.CloseCS(CSPost)
                '        '
                '    Next
                '    For RulePtr = 0 To RuleCnt - 1
                '        If RuleIDs(RulePtr) <> -1 Then
                '            'Call Csv.DeleteContentRecord(cnRSSFeedBlogRules, RuleIDs(RulePtr))
                '            Call cp.Content.Delete(cnRSSFeedBlogRules, RuleIDs(RulePtr))
                '        End If
                '    Next
                'End If
                ''
                'Dim BuildVersion As String = cp.Site.GetProperty("BuildVersion")
                'If BuildVersion >= "4.1.098" Then
                '    Call cp.Utils.ExecuteAddon(RSSProcessAddonGuid)
                'End If
                ''Do While Csv.IsCSOK(CS)
                ''    BlogPostID = Csv.GetCSInteger(CS, "id")
                ''    For RulePtr = 0 To RuleCnt - 1
                ''        If BlogPostID = RuleBlogPostIDs(RulePtr) Then
                ''            RuleIDs(RulePtr) = -1
                ''            Exit For
                ''        End If
                ''    Next
                ''If RulePtr >= RuleCnt Then
                ''        '
                ''        ' Rule not found, add it
                ''        '
                ''        CSRule = Csv.InsertCSRecord(cnRSSFeedBlogRules, 0)
                ''        If Csv.IsCSOK(CSRule) Then
                ''            Call Csv.SetCS(CSRule, "RSSFeedID", RSSFeed.id)
                ''            Call Csv.SetCS(CSRule, "BlogPostID", BlogPostID)
                ''        End If
                ''        Call Csv.CloseCS(CSRule)
                ''    End If
                ''
                '' Now update the Blog Post RSS fields, RSSLink, RSSTitle, RSSDescription, RSSPublish, RSSExpire
                '' Should do this here because if RSS was installed after Blog, there is no link until a post is edited
                ''
                ''CSPost = Csv.OpenCSContentRecord(cnBlogEntries, BlogPostID)
                ''    If Csv.IsCSOK(CSPost) Then
                ''        blogEntry.Name = Csv.GetCSText(CSPost, "name")
                ''        EntryID = Csv.GetCSInteger(CSPost, "id")
                ''        EntryCopy = Csv.GetCSText(CSPost, "copy")
                ''        '
                ''        RSSTitle = Trim(blogEntry.Name)
                ''        If RSSTitle = "" Then
                ''            RSSTitle = "Blog Post " & EntryID
                ''        End If
                ''        Call Main.SetCS(CSPost, "RSSTitle", RSSTitle)
                ''        '
                ''        qs = ""
                ''        qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(BlogPostID))
                ''        qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails)
                ''        EntryLink = Main.GetLinkAliasByPageID(cp.Doc.PageId, qs, blogListLink & "?" & qs)
                ''        If InStr(1, EntryLink, AdminURL, vbTextCompare) = 0 Then
                ''            Call Main.SetCS(CSPost, "RSSLink", EntryLink)
                ''        End If
                ''        Call Main.SetCS(CSPost, "RSSDescription", filterCopy(EntryCopy, 150))
                ''    End If
                ''    Call Csv.CloseCS(CSPost)
                ''    '
                ''    Call Csv.NextCSRecord(CS)
                ''Loop
                ''Call Csv.CloseCS(CS)
                ''
                '' Now delete all the rules that were not found in the blog
                ''
                ''For RulePtr = 0 To RuleCnt - 1
                ''        If RuleIDs(RulePtr) <> -1 Then
                ''            Call Csv.DeleteContentRecord(cnRSSFeedBlogRules, RuleIDs(RulePtr))
                ''        End If
                ''    Next
                ''End If
                ''
                ''If Main.ContentServerVersion >= "4.1.098" Then
                ''    Call cp.Utils.ExecuteAddon(RSSProcessAddonGuid)
                ''End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Sub

        '
        '
        Private Function GetBlogImage(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, blogImage As BlogImageModel, ByRef Return_ThumbnailFilename As String, ByRef Return_ImageFilename As String, ByRef Return_ImageDescription As String, ByRef Return_Imagename As String) As String
            Dim results As String = ""
            Try
                '
                If (blogImage IsNot Nothing) Then
                    Return_ImageDescription = blogImage.description
                    Return_Imagename = blogImage.name
                    Return_ThumbnailFilename = blogImage.Filename
                    Return_ImageFilename = blogImage.Filename
                    '
                    ' todo replace resize
                    '
                    'Dim Filename As String = blogImage.Filename
                    'Dim AltSizeList As String = blogImage.AltSizeList
                    'Dim ThumbNailSize As String = ""
                    'Dim ImageSize As String = ""
                    ''
                    '' -- search for a version of this image resized for this blog's thumbnail
                    'If AltSizeList <> "" Then
                    '    AltSizeList = Replace(AltSizeList, ",", vbCrLf)
                    '    Dim Sizes() As String = Split(AltSizeList, vbCrLf)
                    '    Dim Ptr As Integer
                    '    For Ptr = 0 To UBound(Sizes)
                    '        If Sizes(Ptr) <> "" Then
                    '            Dim Dims() As String = Split(Sizes(Ptr), "x")
                    '            Dim DimWidth As Integer = cp.Utils.EncodeInteger(Dims(0))
                    '            If (blog.ThumbnailImageWidth <> 0) And (DimWidth = blog.ThumbnailImageWidth) Then
                    '                ThumbNailSize = Sizes(Ptr)
                    '            End If
                    '            If (blog.ImageWidthMax <> 0) And (DimWidth = blog.ImageWidthMax) Then
                    '                ImageSize = Sizes(Ptr)
                    '            End If
                    '        End If
                    '    Next
                    'End If
                    'If True Then
                    '    Dim Pos As Integer = InStrRev(Filename, ".")
                    '    If Pos <> 0 Then
                    '        Dim FilenameNoExtension As String = Mid(Filename, 1, Pos - 1)
                    '        Dim FilenameExtension As String = Mid(Filename, Pos + 1)
                    '        Dim sf As Object
                    '        If blog.ThumbnailImageWidth = 0 Then
                    '            '
                    '        ElseIf ThumbNailSize <> "" Then
                    '            Return_ThumbnailFilename = FilenameNoExtension & "-" & ThumbNailSize & "." & FilenameExtension
                    '            cp.Utils.AppendLog("Return_ThumbnailFilename=" & Return_ThumbnailFilename)
                    '        Else
                    '            sf = CreateObject("sfimageresize.imageresize")
                    '            sf.Algorithm = 5
                    '            Try
                    '                Try
                    '                    sf.LoadFromFile(cp.CdnFiles.cp.Site.PhysicalFilePath & Filename)
                    '                Catch ex As Exception

                    '                End Try
                    '                ' On Error GoTo ErrorTrap
                    '                sf.Width = blog.ThumbnailImageWidth
                    '                Try
                    '                    Call sf.DoResize
                    '                Catch ex As Exception

                    '                End Try
                    '                ThumbNailSize = blog.ThumbnailImageWidth & "x" & sf.Height
                    '                Return_ThumbnailFilename = FilenameNoExtension & "-" & ThumbNailSize & "." & FilenameExtension
                    '                Try
                    '                    sf.SaveToFile(cp.Site.PhysicalFilePath & Return_ThumbnailFilename)
                    '                Catch ex As Exception

                    '                End Try
                    '                AltSizeList = AltSizeList & vbCrLf & ThumbNailSize
                    '                blogImage.AltSizeList = AltSizeList
                    '                'Call Main.SetCS(CS, "altsizelist", AltSizeList)
                    '                cp.Utils.AppendLog("Return_ThumbnailFilename=" & Return_ThumbnailFilename)
                    '            Catch ex As Exception
                    '                '
                    '            End Try
                    '        End If
                    '        If blog.ImageWidthMax = 0 Then
                    '            '
                    '        ElseIf ImageSize <> "" Then
                    '            Return_ImageFilename = FilenameNoExtension & "-" & ImageSize & "." & FilenameExtension
                    '        Else
                    '            sf = CreateObject("sfimageresize.imageresize")
                    '            sf.Algorithm = 5

                    '            sf.LoadFromFile(cp.Site.PhysicalFilePath & Filename)
                    '            If Err.Number <> 0 Then
                    '                ' On Error GoTo ErrorTrap
                    '                Err.Clear()
                    '            Else
                    '                ' On Error GoTo ErrorTrap
                    '                sf.Width = blog.ImageWidthMax
                    '                Call sf.DoResize
                    '                ImageSize = blog.ImageWidthMax & "x" & sf.Height
                    '                Return_ImageFilename = FilenameNoExtension & "-" & ImageSize & "." & FilenameExtension
                    '                Call sf.SaveToFile(cp.Site.FilePath & Return_ImageFilename)
                    '                AltSizeList = AltSizeList & vbCrLf & ImageSize
                    '                blogImage.AltSizeList = AltSizeList
                    '                'Call Main.SetCS(CS, "altsizelist", AltSizeList)
                    '            End If
                    '        End If
                    '    End If
                    'End If
                End If
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
        Private Function getUserPtr(cp As CPBaseClass, userid As Integer, allowAuthorInfoLink As Boolean) As Integer
            '
            Try
                Dim userPtr As Integer
                '
                userPtr = -1
                If userCnt > 0 Then
                    For userPtr = 0 To userCnt - 1
                        If users(userPtr).Id = userid Then
                        End If
                    Next
                End If
                If userPtr >= userCnt Then
                    Dim PeopleModelList As List(Of PersonModel) = PersonModel.createList(cp, "id=" & userid)
                    Dim user As PersonModel = PeopleModelList.First
                    'CS = Main.OpenCSContent("people", "id=" & userid)
                    'If Main.IsCSOK(CS) Then
                    userPtr = userCnt
                    userCnt = userCnt + 1
                    ReDim Preserve users(userCnt)
                    users(userPtr).Id = userid
                    users(userPtr).Name = user.id.ToString()
                    users(userPtr).authorInfoLink = ""
                    If allowAuthorInfoLink Then
                        users(userPtr).authorInfoLink = cp.Doc.GetText("authorInfoLink")
                    End If
                End If
                Return userPtr
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function

    End Class
End Namespace
