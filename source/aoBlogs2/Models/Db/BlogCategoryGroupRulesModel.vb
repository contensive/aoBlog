
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class BlogCategoryGroupRulesModel        '<------ set set model Name and everywhere that matches this string
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Blog Category Group Rules"      '<------ set content name
        Public Const contentTableName As String = "ccBlogCategoryGroupRules"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        'instancePropertiesGoHere
        Public Property BlogCategoryID As Integer
        Public Property GroupID As Integer
        '
    End Class
End Namespace
