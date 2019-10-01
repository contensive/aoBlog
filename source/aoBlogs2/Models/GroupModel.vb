
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class GroupModel
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Groups"      '<------ set content name
        Public Const contentTableName As String = "ccGroups"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties

        Public Property AllowBulkEmail As Boolean
        Public Property Caption As String
        Public Property CopyFilename As String
        Public Property PublicJoin As Boolean
        '
        Public Shared Function GetBlockingGroups(cp As CPBaseClass, BlogCategoryID As Integer) As List(Of GroupModel)
            '
            Dim result As New List(Of GroupModel)
            '           
            For Each BlogCategoryGroupRule In DbModel.createList(Of BlogCategoryGroupRulesModel)(cp, "BlogCategoryID=" & BlogCategoryID)
                result.Add(DbModel.create(Of GroupModel)(cp, BlogCategoryGroupRule.GroupID))
            Next
            Return result
        End Function

    End Class
End Namespace
