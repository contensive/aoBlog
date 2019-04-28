Option Explicit On
Option Strict On

Imports System.Linq
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Controllers
    Public NotInheritable Class MetadataController
        Private Sub New()
        End Sub
        '
        '====================================================================================================
        Public Shared Sub setMetadata(cp As CPBaseClass, blogEntry As BlogEntryModel)
            '
            cp.Utils.AppendLog("Blog.setMetadata, blogEntry.id [" & blogEntry.id & "], set Open Graph Title = blogEntry.name [" & blogEntry.name & "]")
            '
            '
            Dim blogImageList As List(Of BlogImageModel) = BlogImageModel.createListFromBlogEntry(cp, blogEntry.id)
            Dim blogEntryBrief As String = blogEntry.RSSDescription
            If blogEntryBrief = "" Then
                blogEntryBrief = cp.Utils.DecodeHTML(blogEntry.copy)
                If blogEntryBrief.Length > 300 Then
                    Dim ptr As Integer = blogEntryBrief.IndexOf(" ", 290)
                    If ptr < 0 Then ptr = 300
                    blogEntryBrief = blogEntryBrief.Substring(1, ptr - 1) & "..."
                End If
            End If
            '
            ' -- set article meta data
            Call cp.Doc.AddTitle(blogEntry.metaTitle)
            Call cp.Doc.AddMetaDescription(blogEntry.metaDescription)
            Call cp.Doc.AddMetaKeywordList((blogEntry.metaKeywordList & "," & blogEntry.tagList).Replace(vbCrLf, ",").Replace(vbCr, ",").Replace(vbLf, ",").Replace(",,", ","))
            '
            ' -- set open graph properties modified by the Blog
            Call cp.Doc.SetProperty("Open Graph URL", cp.Content.GetPageLink(cp.Doc.PageId, "BlogEntryID=" & blogEntry.id & "&FormID=300"))
            Call cp.Doc.SetProperty("Open Graph Title", blogEntry.name)
            Call cp.Doc.SetProperty("Open Graph Description", blogEntryBrief)
            If (blogImageList.Count > 0) Then
                Call cp.Doc.SetProperty("Open Graph Image", "http://" & cp.Site.Domain & cp.Site.FilePath & blogImageList.First().Filename)
            End If
        End Sub
    End Class
End Namespace

