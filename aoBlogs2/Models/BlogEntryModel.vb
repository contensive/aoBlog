
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class BlogEntryModel        '<------ set set model Name and everywhere that matches this string
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Blog Entries"      '<------ set content name
        Public Const contentTableName As String = "ccBlogCopy"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        Public Property AllowComments As Boolean
        Public Property Approved As Boolean
        Public Property articlePrimaryImagePositionId As Integer
        Public Property AuthorMemberID As Integer
        Public Property blogCategoryID As Integer
        Public Property BlogID As Integer
        Public Property Copy As String
        Public Property CopyText As String
        Public Property EntryID As Integer
        Public Property imageDisplayTypeId As Integer
        Public Property PodcastMediaLink As String
        Public Property PodcastSize As Integer
        Public Property primaryImagePositionId As Integer
        Public Property RSSDateExpire As Date
        Public Property RSSDatePublish As Date
        Public Property RSSDescription As String
        Public Property RSSLink As String
        Public Property RSSTitle As String
        Public Property TagList As String
        Public Property Viewings As Integer

        Public Property metaTitle As String
        Public Property metaDescription As String
        Public Property metaKeywordList As String



    End Class
End Namespace
