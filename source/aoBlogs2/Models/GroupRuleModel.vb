
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class GroupRuleModel
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Group Rules"      '<------ set content name
        Public Const contentTableName As String = "xxxxxtableNameGoesHerexxxxx"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "ccGroupRules"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties

        Public Property AllowAdd As Boolean
        Public Property AllowDelete As Boolean
        Public Property ContentID As Integer
        Public Property GroupID As Integer

    End Class
End Namespace
