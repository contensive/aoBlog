
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
        Public Shared Function getBlogImage(cp As CPBaseClass, app As ApplicationEnvironmentModel, blogImage As BlogImageModel, ByRef Return_ThumbnailFilename As String, ByRef Return_ImageFilename As String, ByRef Return_ImageDescription As String, ByRef Return_Imagename As String) As String
            Dim results As String = ""
            Try
                '
                If (blogImage IsNot Nothing) Then
                    Dim imageSizeList As List(Of String) = blogImage.AltSizeList.Split(","c).ToList()
                    Return_ImageDescription = blogImage.description
                    Return_Imagename = blogImage.name
                    Return_ThumbnailFilename = cp.Image.GetBestFitWebP(blogImage.Filename, app.blog.ImageWidthMax, CInt(app.blog.ImageWidthMax * 0.75), imageSizeList)
                    Return_ImageFilename = cp.Image.GetBestFitWebP(blogImage.Filename, app.blog.ThumbnailImageWidth, 0, imageSizeList)
                    If (String.Join(",", imageSizeList) <> blogImage.AltSizeList) Then
                        blogImage.AltSizeList = String.Join(",", imageSizeList)
                        blogImage.save(Of BlogImageModel)(cp)
                    End If
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
