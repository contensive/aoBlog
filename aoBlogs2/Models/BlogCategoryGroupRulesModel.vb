
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class BlogCategoryGroupRulesModel        '<------ set set model Name and everywhere that matches this string
        Inherits baseModel
        Implements ICloneable
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
        '====================================================================================================
        Public Overloads Shared Function add(cp As CPBaseClass) As BlogCategoryGroupRulesModel
            Return add(Of BlogCategoryGroupRulesModel)(cp)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordId As Integer) As BlogCategoryGroupRulesModel
            Return create(Of BlogCategoryGroupRulesModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordGuid As String) As BlogCategoryGroupRulesModel
            Return create(Of BlogCategoryGroupRulesModel)(cp, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cp As CPBaseClass, recordName As String) As BlogCategoryGroupRulesModel
            Return createByName(Of BlogCategoryGroupRulesModel)(cp, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cp As CPBaseClass)
            MyBase.save(Of BlogCategoryGroupRulesModel)(cp)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, recordId As Integer)
            delete(Of BlogCategoryGroupRulesModel)(cp, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, ccGuid As String)
            delete(Of BlogCategoryGroupRulesModel)(cp, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cp As CPBaseClass, sqlCriteria As String, Optional sqlOrderBy As String = "id") As List(Of BlogCategoryGroupRulesModel)
            Return createList(Of BlogCategoryGroupRulesModel)(cp, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of BlogCategoryGroupRulesModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of BlogCategoryGroupRulesModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cp As CPBaseClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of BlogCategoryGroupRulesModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getCount(cp As CPBaseClass, sqlCriteria As String) As Integer
            Return baseModel.getCount(Of BlogCategoryGroupRulesModel)(cp, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Function getUploadPath(fieldName As String) As String
            Return MyBase.getUploadPath(Of BlogCategoryGroupRulesModel)(fieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone(cp As CPBaseClass) As BlogCategoryGroupRulesModel
            Dim result As BlogCategoryGroupRulesModel = DirectCast(Me.Clone(), BlogCategoryGroupRulesModel)
            result.id = cp.Content.AddRecord(contentName)
            result.ccguid = cp.Utils.CreateGuid()
            result.save(cp)
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me.MemberwiseClone()
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Save a list of this model to the database, guid required, using the guid as a key for update/import, and ignoring the id.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="modelList">A dictionary with guid as key, and this model as object</param>
        Public Shared Sub migrationImport(cp As CPBaseClass, modelList As Dictionary(Of String, BlogCategoryGroupRulesModel))
            Dim ContentControlID As Integer = cp.Content.GetID(contentName)
            For Each kvp In modelList
                If (Not String.IsNullOrEmpty(kvp.Value.ccguid)) Then
                    kvp.Value.id = 0
                    Dim dbData As BlogCategoryGroupRulesModel = create(cp, kvp.Value.ccguid)
                    If (dbData IsNot Nothing) Then
                        kvp.Value.id = dbData.id
                    Else
                        kvp.Value.DateAdded = Now
                        kvp.Value.CreatedBy = 0
                    End If
                    kvp.Value.ContentControlID = ContentControlID
                    kvp.Value.ModifiedDate = Now
                    kvp.Value.ModifiedBy = 0
                    kvp.Value.save(cp)
                End If
            Next
        End Sub


    End Class
End Namespace
