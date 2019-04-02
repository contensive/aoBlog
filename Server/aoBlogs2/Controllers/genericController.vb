Option Explicit On
Option Strict On

Imports System.Linq
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Controllers
    Public NotInheritable Class genericController
        Private Sub New()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' if date is invalid, set to minValue
        ''' </summary>
        ''' <param name="srcDate"></param>
        ''' <returns></returns>
        Public Shared Function encodeMinDate(srcDate As DateTime) As DateTime
            Dim returnDate As DateTime = srcDate
            If srcDate < New DateTime(1900, 1, 1) Then
                returnDate = DateTime.MinValue
            End If
            Return returnDate
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' if valid date, return the short date, else return blank string 
        ''' </summary>
        ''' <param name="srcDate"></param>
        ''' <returns></returns>
        Public Shared Function getShortDateString(srcDate As DateTime) As String
            Dim returnString As String = ""
            Dim workingDate As DateTime = encodeMinDate(srcDate)
            If Not isDateEmpty(srcDate) Then
                returnString = workingDate.ToShortDateString()
            End If
            Return returnString
        End Function
        '
        '====================================================================================================
        Public Shared Function isDateEmpty(srcDate As DateTime) As Boolean
            Return (srcDate < New DateTime(1900, 1, 1))
        End Function
        '
        '====================================================================================================
        Public Shared Function getSortOrderFromInteger(id As Integer) As String
            Return id.ToString().PadLeft(7, "0"c)
        End Function
        '
        '====================================================================================================
        Public Shared Function getDateForHtmlInput(source As DateTime) As String
            If isDateEmpty(source) Then
                Return ""
            Else
                Return source.Year.ToString() + "-" + source.Month.ToString().PadLeft(2, "0"c) + "-" + source.Day.ToString().PadLeft(2, "0"c)
            End If
        End Function
        '
        '====================================================================================================
        Public Shared Function convertToDosPath(sourcePath As String) As String
            Return sourcePath.Replace("/", "\")
        End Function
        '
        '====================================================================================================
        Public Shared Function convertToUnixPath(sourcePath As String) As String
            Return sourcePath.Replace("\", "/")
        End Function
        ''' <summary>
        ''' is a member of a group in the group model list
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="groupList"></param>
        ''' <returns></returns>
        Public Shared Function IsGroupListMember(cp As CPBaseClass, groupList As List(Of GroupModel)) As Boolean
            For Each Group In groupList
                If cp.User.IsInGroup(Group.name) Then Return True
            Next
            Return False
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Return a shortened version of the copy
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="rawCopy"></param>
        ''' <param name="MaxLength"></param>
        ''' <returns></returns>
        Friend Shared Function filterCopy(cp As CPBaseClass, rawCopy As String, MaxLength As Integer) As String
            Try
                Dim Copy As String = rawCopy
                If Len(Copy) > MaxLength Then
                    Copy = Left(Copy, MaxLength)
                    Copy = Copy & "..."
                End If
                Return Copy
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Return ""
            End Try
        End Function
        '
        '====================================================================================
        '
        Public Shared Function GetFormTableRow2(cp As CPBaseClass, Innards As String) As String
            '
            Dim Stream As String = ""
            Try

                '
                Stream = Stream & "<tr>"
                Stream = Stream & "<td colspan=2 width=""100%"">" & Innards & "</td>"
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
        Public Shared Function GetFormTableRow(cp As CPBaseClass, FieldCaption As String, Innards As String, Optional AlignLeft As Boolean = True) As String
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
                Stream = Stream & "<td Class=""aoBlogTableRowCellLeft"" " & AlignmentString & ">" & FieldCaption & "</td>"
                Stream = Stream & "<td Class=""aoBlogTableRowCellRight"">" & Innards & "</td>"
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
        Public Shared Function GetField(cp As CPBaseClass, RequestName As String, Height As Integer, Width As Integer, MaxLenghth As Integer, DefaultValue As String) As String
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
        Public Shared Function getBlogEntryCell(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, user As PersonModel, DisplayFullEntry As Boolean, IsSearchListing As Boolean, Return_CommentCnt As Integer, entryEditLink As String, blogListQs As String) As String
            Dim result As String = ""
            Try
                If (blogEntry Is Nothing) Then Throw New ApplicationException("BlogEntryCell called without valid BlogEntry")
                '
                Dim qs As String = cp.Utils.ModifyQueryString("", RequestNameBlogEntryID, CStr(blogEntry.id))
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostDetails.ToString())
                Call cp.Site.addLinkAlias(blogEntry.name, cp.Doc.PageId, qs)
                Dim entryLink As String = cp.Content.GetPageLink(cp.Doc.PageId, qs)
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
                        Select Case blogEntry.articlePrimaryImagePositionId
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
                    result = result & blogEntry.copy & "</div>"
                    qs = blogListQs
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, CStr(blogEntry.id))
                    qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogEntryEditor.ToString())
                    Dim c As String = ""
                    If (blogEntry.imageDisplayTypeId = imageDisplayTypeList) And (blogImageList.Count > 0) Then
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
                    If blogEntry.tagList <> "" Then
                        blogEntry.tagList = Replace(blogEntry.tagList, ",", vbCrLf)
                        Dim Tags() As String = Split(blogEntry.tagList, vbCrLf)
                        blogEntry.tagList = ""
                        Dim SQS As String
                        SQS = cp.Utils.ModifyQueryString(blogListQs, RequestNameFormID, FormBlogSearch.ToString(), True)
                        Dim Ptr As Integer
                        For Ptr = 0 To UBound(Tags)
                            'QS = cp.Utils.ModifyQueryString(SQS, RequestNameFormID, FormBlogSearch, True)
                            qs = cp.Utils.ModifyQueryString(SQS, RequestNameQueryTag, Tags(Ptr), True)
                            Dim Link As String = "?" & qs
                            blogEntry.tagList = blogEntry.tagList & ", " & "<a href=""" & Link & """>" & Tags(Ptr) & "</a>"
                        Next
                        blogEntry.tagList = Mid(blogEntry.tagList, 3)
                        c = "" _
                        & cr & "<div class=""aoBlogTagListHeader"">" _
                        & cr & vbTab & "Tags" _
                        & cr & "</div>" _
                        & cr & "<div class=""aoBlogTagList"">" _
                        & cr & vbTab & blogEntry.tagList _
                        & cr & "</div>"
                        TagListRow = "" _
                        & cr & "<div class=""aoBlogTagListSection"">" _
                        & cp.Html.Indent(c) _
                        & cr & "</div>"
                    End If
                Else
                    result = result & vbCrLf & entryEditLink & "<h2 class=""aoBlogEntryName""><a href=""" & entryLink & """>" & blogEntry.name & "</a></h2>"
                    result = result & cr & "<div class=""aoBlogEntryCopy"">"
                    If (blogImageList.Count > 0) Then
                        Dim ThumbnailFilename As String = ""
                        Dim imageFilename As String = ""
                        Dim imageName As String = ""
                        Dim imageDescription As String = ""
                        GetBlogImage(cp, blog, rssFeed, blogEntry, blogImageList.First, ThumbnailFilename, imageFilename, imageDescription, imageName)
                        If ThumbnailFilename <> "" Then
                            Select Case blogEntry.primaryImagePositionId
                                Case 2
                                    '
                                    ' align right
                                    '
                                    'result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result = result & "<a href=""" & entryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailRight"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:25%;""></a>"
                                Case 3
                                    '
                                    ' align left
                                    '
                                    ' result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result = result & "<a href=""" & entryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnailLeft"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:25%;""></a>"
                                Case 4
                                    '
                                    ' hide
                                    '
                                Case Else
                                    '
                                    ' 1 and none align per stylesheet
                                    '
                                    'result = result & "<a href=""" & EntryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:" & blog.ThumbnailImageWidth & "px;""></a>"
                                    result = result & "<a href=""" & entryLink & """><img alt=""" & imageName & """ title=""" & imageName & """ class=""aoBlogEntryThumbnail"" src=""" & cp.Site.FilePath & ThumbnailFilename & """ style=""width:25%;""></a>"

                            End Select
                        End If
                    End If
                    result = result & "<p>" & genericController.filterCopy(cp, blogEntry.copy, blog.OverviewLength) & "</p></div>"
                    result = result & cr & "<div class=""aoBlogEntryReadMore""><a href=""" & entryLink & """>Read More</a></div>"
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
                    result = result & cp.Utils.ExecuteAddon(addonGuidWebcast)
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
                Dim visit As VisitModel = VisitModel.create(cp, cp.Visit.Id)
                If blogEntry.AllowComments And (cp.Visit.CookieSupport) And (Not visit.Bot()) Then
                    '
                    ' Show comment count
                    Criteria = "(Approved<>0)and(EntryID=" & blogEntry.id & ")"
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
                            RowCopy = RowCopy & " | " & "<a href=""" & entryLink & """>Comment</a>"
                        Else
                            RowCopy = RowCopy & " | " & "<a href=""" & entryLink & """>Comments</a>&nbsp;(" & CommentCount & ")"
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
                        If user.isBlogEditor(cp, blog) Then
                            Criteria = "(EntryID=" & blogEntry.id & ")"
                            BlogCommentModelList = DbModel.createList(Of BlogCommentModel)(cp, "(Approved<>0)and(EntryID=" & blogEntry.id & ")")
                            CommentCount = BlogCommentModelList.Count
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
                        '
                    Else
                        '
                        ' Show all comments
                        '
                        'hint =  hint & ",11"
                        If user.isBlogEditor(cp, blog) Then
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
                            For Each blogComment In DbModel.createList(Of BlogCommentModel)(cp, Criteria)

                                result = result & GetBlogCommentCell(cp, blog, rssFeed, blogEntry, blogComment, user, False)
                                result = result & vbCrLf & Divider
                                CommentPtr = CommentPtr + 1
                            Next

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
                getBlogEntryCell = result
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try

            Return result

        End Function
        '
        '====================================================================================
        '
        Public Shared Function GetBlogCommentCell(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, blogComment As BlogCommentModel, user As PersonModel, IsSearchListing As Boolean) As String
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
                Copy = blogComment.CopyText
                Copy = cp.Utils.EncodeHTML(Copy)
                Copy = Replace(Copy, vbCrLf, "<BR />")
                result = result & cr & "<div class=""aoBlogCommentCopy"">" & Copy & "</div>"
                RowCopy = ""
                If (True) Then
                    Dim author = DbModel.create(Of PersonModel)(cp, blogEntry.AuthorMemberID)
                    If (author IsNot Nothing) AndAlso (author.name <> "") Then
                        RowCopy = RowCopy & "by " & cp.Utils.EncodeHTML(author.name)
                        If blogComment.DateAdded <> Date.MinValue Then
                            RowCopy = RowCopy & " | " & blogComment.DateAdded
                        End If
                    Else
                        If blogComment.DateAdded <> Date.MinValue Then
                            RowCopy = RowCopy & blogComment.DateAdded
                        End If
                    End If
                End If
                '
                If user.isBlogEditor(cp, blog) Then
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
                If (Not blogComment.Approved) And (Not user.isBlogEditor(cp, blog)) And (blogComment.AuthorMemberID = cp.User.Id) Then
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
        '========================================================================
        '
        Public Shared Function GetBlogImage(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, blogImage As BlogImageModel, ByRef Return_ThumbnailFilename As String, ByRef Return_ImageFilename As String, ByRef Return_ImageDescription As String, ByRef Return_Imagename As String) As String
            Dim results As String = ""
            Try
                '
                If (blogImage IsNot Nothing) Then
                    Return_ImageDescription = blogImage.description
                    Return_Imagename = blogImage.name
                    Return_ThumbnailFilename = blogImage.Filename
                    Return_ImageFilename = blogImage.Filename
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return results
        End Function
        '
        '
        '
        '
        Public Shared Function getLinkAlias(cp As CPBaseClass, sourceLink As String) As String
            '
            Dim result As String = ""
            Try
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


    End Class
End Namespace

