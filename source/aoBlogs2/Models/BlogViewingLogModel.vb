

Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class BlogViewingLogModel        '<------ set set model Name and everywhere that matches this string
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Blog Viewing Log"      '<------ set content name
        Public Const contentTableName As String = "BlogViewingLog"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        'instancePropertiesGoHere
        Public Property BlogEntryID As Integer
        Public Property MemberID As Integer
        Public Property VisitID As Integer
        '

    End Class
End Namespace
