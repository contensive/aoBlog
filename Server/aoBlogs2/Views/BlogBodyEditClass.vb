
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
    Public Class BlogBodyEditClass
        '
        '====================================================================================
        '
        Public Shared Function GetFormBlogEdit(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, request As View.RequestModel, user As PersonModel, blogListLink As String) As String
            Dim result As New StringBuilder()
            Try
                Dim blogEntry_id As Integer = 0
                Dim blogEntry_copy As String = ""
                Dim blogEntry_tagList As String = ""
                Dim blogEntry_name As String = ""
                Dim blongEntry_blogCategoryId As Integer = 0
                '
                result.Append("<table width=100% border=0 cellspacing=0 cellpadding=5 class=""aoBlogPostTable"">")
                If (blogEntry Is Nothing) Then
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
                    blogEntry_id = blogEntry.id
                    blogEntry_name = blogEntry.name
                    blogEntry_tagList = blogEntry.tagList
                    blongEntry_blogCategoryId = blogEntry.blogCategoryID
                    blogEntry_copy = blogEntry.copy
                    If blogEntry_copy = "" Then
                        blogEntry_copy = "<!-- cc --><p><br></p><!-- /cc -->"
                    End If
                End If
                result.Append(genericController.GetFormTableRow(cp, "<div style=""padding-top:3px"">Title: </div>", cp.Html.InputText(RequestNameBlogEntryName, blogEntry_name, "1", "70")))
                result.Append(genericController.GetFormTableRow(cp, "<div style=""padding-top:108px"">Post: </div>", cp.Html.InputWysiwyg(RequestNameBlogEntryCopy, blogEntry_copy)))
                result.Append(genericController.GetFormTableRow(cp, "<div style=""padding-top:3px"">Tag List: </div>", cp.Html.InputText(RequestNameBlogEntryTagList, blogEntry_tagList, "5", "70")))
                If blog.AllowCategories Then
                    Dim CategorySelect As String = cp.Html.SelectContent(RequestNameBlogEntryCategoryID, blongEntry_blogCategoryId.ToString(), "Blog Categories")
                    If (InStr(1, CategorySelect, "<option value=""""></option></select>", vbTextCompare) <> 0) Then
                        '
                        ' Select is empty
                        CategorySelect = "<div>This blog has no categories defined</div>"
                    End If
                    result.Append(genericController.GetFormTableRow(cp, "Category: ", CategorySelect))
                End If
                '
                ' file upload form taken from Resource Library
                Dim imageForm As String = "" _
                    & "<TABLE id=""UploadInsert"" border=""0"" cellpadding=""0"" cellspacing=""1"" width=""100%"" class=""aoBlogImageTable"">" _
                    & "<tr>" _
                    & ""
                Dim BlogImageModelList As List(Of BlogImageModel) = BlogImageModel.createListFromBlogEntry(cp, blogEntry_id)
                Dim Ptr As Integer = 1
                For Each BlogImage In BlogImageModelList
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
                    '   image
                    '
                    imageForm = imageForm _
                        & "<tr>" _
                        & "<td><input type=""checkbox"" name=""" & rnBlogImageDelete & "." & Ptr & """>&nbsp;Delete</td>" _
                        & "</tr>" _
                        & "<tr>" _
                        & "<td align=""left"" class=""ccAdminSmall""><img class=""aoBlogEditImagePreview"" alt=""" & imageName & """ title=""" & imageName & """ src=""" & cp.Site.FilePath & imageFilename & """></td>" _
                        & "</tr>" _
                        & ""
                    '
                    '   order
                    '
                    imageForm = imageForm _
                        & "<tr>" _
                        & "<td>Order<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageOrder & "." & Ptr & """ SIZE=""2"" value=""" & Ptr & """></td>" _
                        & "</tr>" _
                        & ""
                    '
                    '   name
                    '
                    imageForm = imageForm _
                        & "<tr>" _
                        & "<td>Name<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageName & "." & Ptr & """ SIZE=""25"" value=""" & cp.Utils.EncodeHTML(imageName) & """></td>" _
                        & "</tr>" _
                        & ""
                    Dim ImageID As Integer
                    '
                    '   description
                    '
                    imageForm = imageForm _
                        & "<tr>" _
                        & "<td>Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50"">" & cp.Utils.EncodeHTML(imageDescription) & "</TEXTAREA><input type=""hidden"" name=""" & rnBlogImageID & "." & Ptr & """ value=""" & ImageID & """></td>" _
                        & "</tr>" _
                        & ""
                    Ptr = Ptr + 1
                Next
                '
                '   row delimiter
                '
                imageForm = imageForm _
                        & "<TR style=""padding-bottom:10px; padding-bottom:10px;"">" _
                        & "<td><HR></td>" _
                        & "</tr>" _
                        & ""
                '
                '   order
                '
                imageForm = imageForm _
                        & "<tr>" _
                        & "<td align=""left"">Order<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageOrder & "." & Ptr & """ SIZE=""2"" value=""" & Ptr & """></td>" _
                        & "</tr>" _
                        & ""
                '
                '   image
                '
                imageForm = imageForm _
                        & "<tr>" _
                        & "<td align=""left"">Image<br><INPUT TYPE=""file"" name=""LibraryUpload." & Ptr & """></td>" _
                        & "</tr>" _
                        & ""
                '
                '   name
                '
                imageForm = imageForm _
                        & "<tr>" _
                        & "<td align=""left"">Name<br><INPUT TYPE=""Text"" NAME=""" & rnBlogImageName & "." & Ptr & """ SIZE=""25""></td>" _
                        & "</tr>" _
                        & ""
                '
                '   description
                '
                imageForm = imageForm _
                        & "<tr>" _
                        & "<td align=""left"">Description<br><TEXTAREA NAME=""" & rnBlogImageDescription & "." & Ptr & """ ROWS=""5"" COLS=""50""></TEXTAREA></td>" _
                        & "</tr>" _
                        & ""
                '
                imageForm = imageForm _
                        & "</Table>" _
                        & ""
                '
                imageForm = imageForm _
                        & "<TABLE border=""0"" cellpadding=""0"" cellspacing=""1"" width=""100%""  class=""aoBlogNewRowTable"">" _
                        & "<tr><td Width=""30""><img src=/ResourceLibrary/spacer.gif width=30 height=1></td><td align=""left""><a href=""#"" onClick=""blogNewRow();return false;"">+ upload more files</a></td></tr>" _
                        & "</Table>" & cp.Html.Hidden("LibraryUploadCount", Ptr.ToString(), "LibraryUploadCount") _
                        & ""
                '
                result.Append(genericController.GetFormTableRow(cp, "Images: ", imageForm))
                If blogEntry_id <> 0 Then
                    'hint =  "6"
                    result.Append(genericController.GetFormTableRow(cp, "", cp.Html.Button(rnButton, FormButtonPost) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonDelete)))
                Else
                    'hint =  "7"
                    result.Append(genericController.GetFormTableRow(cp, "", cp.Html.Button(rnButton, FormButtonPost) & "&nbsp;" & cp.Html.Button(rnButton, FormButtonCancel)))
                End If
                'hint =  "8"
                Dim qs As String = cp.Doc.RefreshQueryString()
                qs = cp.Utils.ModifyQueryString(qs, RequestNameBlogEntryID, "", True)
                qs = cp.Utils.ModifyQueryString(qs, RequestNameFormID, FormBlogPostList.ToString())
                'hint =  "9"
                result.Append(vbCrLf & genericController.GetFormTableRow2(cp, "<div class=""aoBlogFooterLink""><a href=""" & blogListLink & """>" & BackToRecentPostsMsg & "</a></div>"))

                '
                result.Append(cp.Html.Hidden(RequestNameBlogEntryID, blogEntry_id.ToString()))
                result.Append(cp.Html.Hidden(RequestNameSourceFormID, FormBlogEntryEditor.ToString()))
                result.Append("</table>")
                Call cp.Visit.SetProperty(SNBlogEntryName, CStr(cp.Utils.GetRandomInteger()))
                Return cp.Html.Form(result.ToString())
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Return String.Empty
            End Try
        End Function
        '
        '====================================================================================
        '
        Public Shared Function ProcessFormBlogEdit(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, ByRef blogEntry As BlogEntryModel, request As View.RequestModel, blogListLink As String) As Integer
            Try
                If cp.Visit.GetProperty(SNBlogEntryName) <> "" Then
                    Call cp.Visit.SetProperty(SNBlogEntryName, "")
                    If request.ButtonValue = FormButtonCancel Then
                        '
                        ' Cancel
                        '
                        Return FormBlogPostList
                    ElseIf request.ButtonValue = FormButtonDelete Then
                        '
                        ' Delete
                        DbModel.delete(Of BlogEntryModel)(cp, blogEntry.id)
                        Call RSSFeedModel.UpdateBlogFeed(cp)
                        blogEntry = Nothing
                        Return FormBlogPostList
                    ElseIf request.ButtonValue = FormButtonPost Then
                        '
                        ' Post
                        '
                        'blogEntry = If(request.blogEntryId > 0, DbModel.create(Of BlogEntryModel)(cp, request.blogEntryId), Nothing)
                        If (blogEntry Is Nothing) Then blogEntry = DbModel.add(Of BlogEntryModel)(cp)
                        blogEntry.name = request.BlogEntryName
                        blogEntry.copy = request.BlogEntryCopy
                        blogEntry.tagList = request.BlogEntryTagList
                        blogEntry.blogCategoryID = request.BlogEntryCategoryId
                        blogEntry.blogID = blog.id
                        blogEntry.save(Of BlogEntryModel)(cp)
                        '
                        Call RSSFeedModel.UpdateBlogFeed(cp)
                        ProcessFormBlogEdit = FormBlogPostList
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
                                            blogEntry.save(Of BlogEntryModel)(cp)
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
                    End If
                End If
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
        End Function
    End Class
End Namespace