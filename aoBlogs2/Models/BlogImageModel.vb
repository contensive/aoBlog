

Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class BlogImageModel        '<------ set set model Name and everywhere that matches this string
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Blog Images"      '<------ set content name
        Public Const contentTableName As String = "BlogImages"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        'instancePropertiesGoHere
        Public Property AltSizeList As String
        Public Property description As String
        Public Property Filename As String
        Public Property height As Integer
        Public Property width As Integer
        '
        ''' <summary>
        ''' Return a list of blog entry images for the blog entry
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="entryId">The id of the blog entry</param>
        ''' <returns></returns>
        Public Shared Function createListFromBlogEntry(cp As CPBaseClass, entryId As Integer) As List(Of BlogImageModel)
            Dim result As New List(Of BlogImageModel)
            Try
                For Each Rule In DbModel.createList(Of BlogImageRuleModel)(cp, "(BlogEntryID=" & entryId & ")")
                    Dim blogimage As BlogImageModel = DbModel.create(Of BlogImageModel)(cp, Rule.BlogImageID)
                    If (blogimage IsNot Nothing) Then
                        result.Add(DbModel.create(Of BlogImageModel)(cp, Rule.BlogImageID))
                    End If
                Next
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function


    End Class
End Namespace
