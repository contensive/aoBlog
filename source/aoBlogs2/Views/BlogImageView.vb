
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
    Public Class BlogImageView
        '
        '========================================================================
        '
        Public Shared Function getBlogImage(cp As CPBaseClass, blog As BlogModel, rssFeed As RSSFeedModel, blogEntry As BlogEntryModel, blogImage As BlogImageModel, ByRef Return_ThumbnailFilename As String, ByRef Return_ImageFilename As String, ByRef Return_ImageDescription As String, ByRef Return_Imagename As String) As String
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
        '==================================================================================
        '
        '
    End Class
End Namespace
