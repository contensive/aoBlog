
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class MemberRuleModel
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Member Rules"      '<------ set content name
        Public Const contentTableName As String = "ccMemberRules"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties

        Public Property DateExpires As Date
        Public Property GroupID As Integer
        Public Property MemberID As Integer
        '
    End Class
End Namespace
