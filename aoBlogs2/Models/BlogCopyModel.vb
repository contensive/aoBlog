

Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class BlogCopyModel        '<------ set set model Name and everywhere that matches this string
        Inherits baseModel
        Implements ICloneable
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Blog Copy"      '<------ set content name
        Public Const contentTableName As String = "ccBlogCopy"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        'instancePropertiesGoHere
        ' sample instance property -- Public Property DataSourceID As Integer
        '
        '====================================================================================================
        Public Overloads Shared Function add(cp As CPBaseClass) As BlogCopyModel
            Return add(Of BlogCopyModel)(cp)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordId As Integer) As BlogCopyModel
            Return create(Of BlogCopyModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordGuid As String) As BlogCopyModel
            Return create(Of BlogCopyModel)(cp, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cp As CPBaseClass, recordName As String) As BlogCopyModel
            Return createByName(Of BlogCopyModel)(cp, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cp As CPBaseClass)
            MyBase.save(Of BlogCopyModel)(cp)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, recordId As Integer)
            delete(Of BlogCopyModel)(cp, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, ccGuid As String)
            delete(Of BlogCopyModel)(cp, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cp As CPBaseClass, sqlCriteria As String, Optional sqlOrderBy As String = "id") As List(Of BlogCopyModel)
            Return createList(Of BlogCopyModel)(cp, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of BlogCopyModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of BlogCopyModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cp As CPBaseClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of BlogCopyModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getCount(cp As CPBaseClass, sqlCriteria As String) As Integer
            Return baseModel.getCount(Of BlogCopyModel)(cp, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Function getUploadPath(fieldName As String) As String
            Return MyBase.getUploadPath(Of BlogCopyModel)(fieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone(cp As CPBaseClass) As BlogCopyModel
            Dim result As BlogCopyModel = DirectCast(Me.Clone(), BlogCopyModel)
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
        Public Shared Sub migrationImport(cp As CPBaseClass, modelList As Dictionary(Of String, BlogCopyModel))
            Dim ContentControlID As Integer = cp.Content.GetID(contentName)
            For Each kvp In modelList
                If (Not String.IsNullOrEmpty(kvp.Value.ccguid)) Then
                    kvp.Value.id = 0
                    Dim dbData As BlogCopyModel = create(cp, kvp.Value.ccguid)
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
        '
        ''' <summary>
        ''' Return a list of Blog Copy
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="blogId">The id of the Blog Copy</param>
        ''' <returns></returns>
        Public Shared Function createListFromBlogCopy(cp As CPBaseClass, blogId As Integer) As List(Of BlogCopyModel)
            Dim result As New List(Of BlogCopyModel)
            Try
                Dim Sql As String = "select p.ID" _
                        & " from (ccBlogCopy p" _
                        & " left join BlogCategories c on c.id=p.blogCategoryID)" _
                        & " where (p.blogid=" & blogId & ")" _
                        & " and((c.id is null)or(c.UserBlocking=0)or(c.UserBlocking is null))"
                result = createList(cp, "(id in (" & Sql & "))")
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        ''' <summary>
        ''' Return a list of Archive Blog Copy
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="blogId">The id of the Blog Copy</param>
        ''' <returns></returns>
        Public Shared Function createArchiveListFromBlogCopy(cp As CPBaseClass, blogId As Integer) As List(Of BlogCopyModel)
            Dim result As New List(Of BlogCopyModel)
            Try
                Dim SQL = "SELECT distinct Month(DateAdded) as ArchiveMonth, year(dateadded) as ArchiveYear " _
                            & " From ccBlogCopy" _
                            & " Where (ContentControlID = " & cp.Content.GetID(cnBlogEntries) & ") And (Active <> 0)" _
                            & " AND (BlogID=" & blogId & ")" _
                            & " ORDER BY year(dateadded) desc, Month(DateAdded) desc"
                result = createList(cp, "(id in (" & Sql & "))")
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function


    End Class
End Namespace
