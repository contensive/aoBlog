﻿Option Explicit On
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
        Public Shared Sub setMetadata(cp As CPBaseClass, blogEntry As BlogPostModel)
            '
            cp.Utils.AppendLog("Blog.setMetadata, blogEntry.id [" & blogEntry.id & "], set Open Graph Title = blogEntry.name [" & blogEntry.name & "]")
            '
            Dim blogImageList As List(Of BlogImageModel) = BlogImageModel.createListFromBlogEntry(cp, blogEntry.id)
            Dim blogEntryBrief As String = blogEntry.RSSDescription
            If blogEntryBrief = "" Then
                blogEntryBrief = cp.Utils.ConvertHTML2Text(blogEntry.copy)
                If blogEntryBrief.Length > 300 Then
                    Dim ptr As Integer = blogEntryBrief.IndexOf(" ", 290)
                    If ptr < 0 Then ptr = 300
                    blogEntryBrief = blogEntryBrief.Substring(0, ptr - 1) & "..."
                End If
            End If
            '
            ' -- set article meta data
            Call cp.Doc.AddTitle(If(Not String.IsNullOrEmpty(blogEntry.metaTitle), blogEntry.metaTitle, blogEntry.name))
            Call cp.Doc.AddMetaDescription(If(Not String.IsNullOrEmpty(blogEntry.metaDescription), blogEntry.metaDescription, blogEntry.name))
            Call cp.Doc.AddMetaKeywordList((blogEntry.metaKeywordList & "," & blogEntry.tagList).Replace(vbCrLf, ",").Replace(vbCr, ",").Replace(vbLf, ",").Replace(",,", ","))
            '
            ' -- set open graph properties modified by the Blog
            Call cp.Doc.SetProperty("Open Graph URL", cp.Content.GetPageLink(cp.Doc.PageId, "BlogEntryID=" & blogEntry.id & "&FormID=300"))
            Call cp.Doc.SetProperty("Open Graph Title", blogEntry.name)
            Call cp.Doc.SetProperty("Open Graph Description", blogEntryBrief)
            If (blogImageList.Count > 0) Then
                If cp.Request.Secure Then
                    Call cp.Doc.SetProperty("Open Graph Image", "https://" & cp.Site.Domain & cp.Http.CdnFilePathPrefix & blogImageList.First().Filename)
                Else
                    Call cp.Doc.SetProperty("Open Graph Image", "http://" & cp.Site.Domain & cp.Http.CdnFilePathPrefix & blogImageList.First().Filename)
                End If
            End If
        End Sub
    End Class
End Namespace

