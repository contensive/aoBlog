Imports System.Linq
Imports System.Text
Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    '
    Public NotInheritable Class EditView
        '
        '====================================================================================
        '
        Public Shared Function getFormBlogEdit(cp As CPBaseClass, app As ApplicationEnvironmentModel, request As View.RequestModel) As String
            Dim result As New StringBuilder()
            Try
                '
                result.Append("<table width=100% border=0 cellspacing=0 cellpadding=5 class=""aoBlogPostTable"">")
                '
                ' -- create properties that can be either from the model, or the request
                Dim blogEntry_id As Integer = 0
                Dim blogEntry_copy As String = ""
                Dim blogEntry_tagList As String = ""
                Dim blogEntry_name As String = ""
                Dim blongEntry_blogCategoryId As Integer = 0
                If (app.blogEntry Is Nothing) Then
                    '
                    ' -- Create a new blog post
                    result.Append("<h2>Create a new blog post</h2>")
                    If (request.ButtonValue = FormButtonPost) Then
                        '
                        ' entry posted, but process could not save, display what was submitted
                        blogEntry_name = request.BlogEntryName
                        blogEntry_copy = request.BlogEntryCopy
                        blogEntry_tagList = request.BlogEntryTagList
                        blongEntry_blogCategoryId = request.BlogEntryCategoryId
                    End If
                Else
                    '
                    ' Edit an entry
                    blogEntry_id = app.blogEntry.id
                    blogEntry_name = app.blogEntry.name
                    blogEntry_tagList = app.blogEntry.tagList
                    blongEntry_blogCategoryId = app.blogEntry.blogCategoryID
                    blogEntry_copy = app.blogEntry.copy
                    If blogEntry_copy = "" Then
                        blogEntry_copy = "<!-- cc --><p><br></p><!-- /cc -->"
                    End If
                End If
                result.Append(genericController.getFormTableRow(cp, "<div style=""padding-top:3px"">Title: </div>", cp.Html.InputText(RequestNameBlogEntryName, blogEntry_name, 255, "form-control")))
                result.Append(genericController.getFormTableRow(cp, "<div style=""padding-top:108px"">Post: </div>", cp.Html.InputWysiwyg(RequestNameBlogEntryCopy, blogEntry_copy, CPHtmlBaseClass.EditorUserScope.ContentManager)))
                result.Append(genericController.getFormTableRow(cp, "<div style=""padding-top:3px"">Tag List: </div>", cp.Html.InputText(RequestNameBlogEntryTagList, blogEntry_tagList, 255, "form-control")))
                If app.blog.AllowCategories Then
                    Dim CategorySelect As String = cp.Html.SelectContent(RequestNameBlogEntryCategoryID, blongEntry_blogCategoryId.ToString(), "Blog Categories")
                    If (InStr(1, CategorySelect, "<option value=""""></option></select>", vbTextCompare) <> 0) Then
                        '
                        ' Select is empty
                        CategorySelect = "<div>This blog has no categories defined</div>"
                    End If
                    result.Append(genericController.getFormTableRow(cp, "Category: ", CategorySelect))
                End If
                '
                ' file upload form taken from Resource Library
                Dim imageForm As String = "" _
                        & "<TABLE id=""UploadInsert"" border=""0"" cellpadding=""0"" cellspacing=""1"" width=""100%"" class=""aoBlogImageTable"">" _
                        & "<tr>" _
                        & ""
                Dim BlogImageModelList As List(Of BlogImageModel) = BlogImageModel.createListFromBlogEntry(cp, blogEntry_id)
                Dim Ptr As Integer = 1
                If BlogImageModelList.Count > 0 Then
                    Dim BlogImage = BlogImageModelList.First
                    'For Each BlogImage In BlogImageModelList
                    Dim imageFilename As String = BlogImage.Filename
                    Dim imageDescription As String = BlogImage.description
                    Dim imageName As String = BlogImage.name
                    If Ptr <> 1 Then
                        imageForm = imageForm _
                                    & "<tr>" _
                                    & "<td><HR></td>" _
                                    & "</tr>" _
                                    & ""
                    End If
                    '
                    ' -- image
                    '
                    imageForm = imageForm _
                                & "<tr>" _
                                & "<td><input type=""checkbox"" name=""" & rnBlogImageDelete & "." & Ptr & """>&nbsp;Delete</td>" _
                                & "</tr>" _
                                & "<tr>" _
                                & "<td align=""left"" class=""ccAdminSmall""><img class=""aoBlogEditImagePreview"" alt=""" & imageName & """ title=""" & imageName & """ src=""" & cp.Http.CdnFilePathPrefix & imageFilename & """></td>" _
                                & "</tr>" _
                                & ""
                    '
                    ' -- name
                    '
                    imageForm = imageForm _
                                & "<tr>" _
                                & "<td>Name<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageName & "." & Ptr & """ SIZE=""25"" value=""" & cp.Utils.EncodeHTML(imageName) & """></td>" _
                                & "</tr>" _
                                & ""
                    Dim ImageID As Integer
                    '
                    ' -- description
                    '
                    imageForm = imageForm _
                                & "<tr>" _
                                & "<td>Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50"">" & cp.Utils.EncodeHTML(imageDescription) & "</TEXTAREA><input type=""hidden"" name=""" & rnBlogImageID & "." & Ptr & """ value=""" & ImageID & """></td>" _
                                & "</tr>" _
                                & ""
                    Ptr = Ptr + 1
                End If
                '
                ' -- Next
                '
                imageForm = imageForm _
                            & "<TR style=""padding-bottom:10px; padding-bottom:10px;"">" _
                            & "<td><HR></td>" _
                            & "</tr>" _
                            & ""
                '
                ' -- image
                '
                If Ptr = 1 Then
                    imageForm = imageForm _
                           & "<tr>" _
                           & "<td align=""left"">Image<br><INPUT TYPE=""file"" name=""LibraryUpload." & Ptr & """></td>" _
                           & "</tr>" _
                           & ""
                    '
                    ' -- image name
                    '
                    imageForm = imageForm _
                                & "<tr>" _
                                & "<td align=""left"">Name<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageName & "." & Ptr & """ SIZE=""25""></td>" _
                                & "</tr>" _
                                & ""
                    '
                    ' -- image description
                    '
                    imageForm = imageForm _
                                & "<tr>" _
                                & "<td align=""left"">Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50""></TEXTAREA></td>" _
                                & "</tr>" _
                                & ""
                End If
                '
                imageForm = imageForm _
                            & "</Table>" _
                            & ""
                '
                imageForm = imageForm _
                            & cp.Html.Hidden("LibraryUploadCount", Ptr.ToString(), "LibraryUploadCount") _
                            & ""
                '
                result.Append(genericController.getFormTableRow(cp, "Images: ", imageForm))
                If blogEntry_id <> 0 Then
                    result.Append(genericController.getFormTableRow(cp, "", cp.Html.Button(rnButton, FormButtonPost) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonDelete)))
                Else
                    result.Append(genericController.getFormTableRow(cp, "", cp.Html.Button(rnButton, FormButtonPost) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel)))
                End If
                Dim qs As String = cp.Doc.RefreshQueryString()
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList.ToString())
                result.Append(vbCrLf & genericController.getFormTableRow2(cp, "<div class=""aoBlogFooterLink""><a href=""" & app.blogPageBaseLink & """>" & BackToRecentPostsMsg & "</a></div>"))
                '
                result.Append(cp.Html.Hidden(RequestNameBlogEntryID, blogEntry_id.ToString()))
                result.Append(cp.Html.Hidden(RequestNameSourceFormID, FormBlogEntryEditor.ToString()))
                result.Append("</table>")
                '
                ' -- *****************************************************
                ' -- De-Vince this
                ' -- *****************************************************
                '
                Call cp.Visit.SetProperty(SNBlogEntryName, CStr(cp.Utils.GetRandomInteger()))
                Return cp.Html.Form(result.ToString())
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
        '
        '====================================================================================
        ''' <summary>
        ''' Process a new blog entry or save the edit from an existing blog entry. If BlogEntry object is nothing, this is a new entry.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="request"></param>
        ''' <returns></returns>
        Public Shared Function ProcessFormBlogEdit(cp As CPBaseClass, app As ApplicationEnvironmentModel, request As View.RequestModel) As Integer
            Try
                Dim blog As BlogModel = app.blog
                Dim blogEntry As BlogPostModel = app.blogEntry
                If cp.Visit.GetText(SNBlogEntryName) <> "" Then
                    Call cp.Visit.GetText(SNBlogEntryName, "")
                    If request.ButtonValue = FormButtonCancel Then
                        '
                        ' Cancel
                        '
                        Return FormBlogPostList
                    ElseIf request.ButtonValue = FormButtonDelete Then
                        '
                        ' Delete
                        DbModel.delete(Of BlogPostModel)(cp, blogEntry.id)
                        Call RSSFeedModel.UpdateBlogFeed(cp)
                        blogEntry = Nothing
                        Return FormBlogPostList
                    ElseIf request.ButtonValue = FormButtonPost Then
                        '
                        ' Post
                        '
                        If (blogEntry Is Nothing) Then
                            blogEntry = DbModel.add(Of BlogPostModel)(cp)
                            '
                            ' -- by default, the RSS feed should be checked for this new blog entry
                            Dim rule As RSSFeedBlogRuleModel = RSSFeedBlogRuleModel.add(cp)
                            rule.BlogPostID = blogEntry.id
                            rule.RSSFeedID = app.rssFeed.id
                            rule.save(cp)
                        End If

                        blogEntry.name = request.BlogEntryName
                        blogEntry.copy = request.BlogEntryCopy
                        blogEntry.tagList = request.BlogEntryTagList
                        blogEntry.blogCategoryID = request.BlogEntryCategoryId
                        blogEntry.blogID = blog.id
                        blogEntry.save(Of BlogPostModel)(cp)
                        '
                        ' Upload files
                        '
                        Dim UploadCount As Integer = cp.Doc.GetInteger("LibraryUploadCount")
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
                                            blogEntry.save(Of BlogPostModel)(cp)
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
                                        Dim VirtualFilePath As String = BlogImage.getUploadPath(Of BlogImageModel)("filename")
                                        Call cp.Html.ProcessInputFile(rnBlogUploadPrefix & "." & UploadPointer, VirtualFilePath)
                                        BlogImage.Filename = VirtualFilePath & imageFilename
                                        BlogImage.save(Of BlogImageModel)(cp)
                                        BlogImage.SortOrder = New String("0"c, 12 - imageOrder.ToString().Length) & imageOrder.ToString()
                                    End If
                                    '
                                    Dim ImageRule As BlogImageRuleModel = BlogImageRuleModel.add(Of BlogImageRuleModel)(cp)
                                    If (ImageRule IsNot Nothing) Then
                                        ImageRule.BlogEntryID = blogEntry.id
                                        ImageRule.BlogImageID = BlogImageID
                                        ImageRule.save(Of BlogImageRuleModel)(cp)
                                    End If
                                End If
                            Next
                        End If
                        '
                        Call RSSFeedModel.UpdateBlogFeed(cp)
                        ProcessFormBlogEdit = FormBlogPostList
                    End If
                End If
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function
    End Class
End Namespace
